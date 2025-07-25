/*! Jcrop.js v2.0.4 - build: 20151117
 *  @copyright 2008-2015 Tapmodo Interactive LLC
 *  @license Free software under MIT License
 *  @website http://jcrop.org/
 **/
(function($){
  'use strict';

  // Jcrop constructor
  var Jcrop = function(element,opt){
    var _ua = navigator.userAgent.toLowerCase();

    this.opt = $.extend({},Jcrop.defaults,opt || {});

    this.container = $(element);

    this.opt.is_msie = /msie/.test(_ua);
    this.opt.is_ie_lt9 = /msie [1-8]\./.test(_ua);

    this.container.addClass(this.opt.css_container);

    this.ui = {};
    this.state = null;
    this.ui.multi = [];
    this.ui.selection = null;
    this.filter = {};

    this.init();
    this.setOptions(opt);
    this.applySizeConstraints();
    this.container.trigger('cropinit',this);
      
    // IE<9 doesn't work if mouse events are attached to window
    if (this.opt.is_ie_lt9)
      this.opt.dragEventTarget = document.body;
  };


  // Jcrop static functions
  $.extend(Jcrop,{
    component: { },
    filter: { },
    stage: { },
    registerComponent: function(name,component){
      Jcrop.component[name] = component;
    },
    registerFilter: function(name,filter){
      Jcrop.filter[name] = filter;
    },
    registerStageType: function(name,stage){
      Jcrop.stage[name] = stage;
    },
    // attach: function(element,opt){{{
    attach: function(element,opt){
      var obj = new $.Jcrop(element,opt);
      return obj;
    },
    // }}}
    // imgCopy: function(imgel){{{
    imgCopy: function(imgel){
      var img = new Image;
      img.src = imgel.src;
      return img;
    },
    // }}}
    // imageClone: function(imgel){{{
    imageClone: function(imgel){
      return $.Jcrop.supportsCanvas?
        Jcrop.canvasClone(imgel):
        Jcrop.imgCopy(imgel);
    },
    // }}}
    // canvasClone: function(imgel){{{
    canvasClone: function(imgel){
      var canvas = document.createElement('canvas'),
          ctx = canvas.getContext('2d');

      $(canvas).width(imgel.width).height(imgel.height),
      canvas.width = imgel.naturalWidth;
      canvas.height = imgel.naturalHeight;
      ctx.drawImage(imgel,0,0,imgel.naturalWidth,imgel.naturalHeight);
      return canvas;
    },
    // }}}
    // propagate: function(plist,config,obj){{{
    propagate: function(plist,config,obj){
      for(var i=0,l=plist.length;i<l;i++)
        if (config.hasOwnProperty(plist[i]))
          obj[plist[i]] = config[plist[i]];
    },
    // }}}
    // getLargestBox: function(ratio,w,h){{{
    getLargestBox: function(ratio,w,h){
      if ((w/h) > ratio)
        return [ h * ratio, h ];
          else return [ w, w / ratio ];
    },
    // }}}
    // stageConstructor: function(el,options,callback){{{
    stageConstructor: function(el,options,callback){

      // Get a priority-ordered list of available stages
      var stages = [];
      $.each(Jcrop.stage,function(i,e){
        stages.push(e);
      });
      stages.sort(function(a,b){ return a.priority - b.priority; });

      // Find the first one that supports this element
      for(var i=0,l=stages.length;i<l;i++){
        if (stages[i].isSupported(el,options)){
          stages[i].create(el,options,function(obj,opt){
            if (typeof callback == 'function') callback(obj,opt);
          });
          break;
        }
      }
    },
    // }}}
    // supportsColorFade: function(){{{
    supportsColorFade: function(){
      return $.fx.step.hasOwnProperty('backgroundColor');
    },
    // }}}
    // wrapFromXywh: function(xywh){{{
    wrapFromXywh: function(xywh){
      var b = { x: xywh[0], y: xywh[1], w: xywh[2], h: xywh[3] };
      b.x2 = b.x + b.w;
      b.y2 = b.y + b.h;
      return b;
    }
    // }}}
  });

var AbstractStage = function(){
};

$.extend(AbstractStage,{
  isSupported: function(el,o){
    // @todo: should actually check if it's an HTML element
    return true;
  },
  // A higher priority means less desirable
  // AbstractStage is the last one we want to use
  priority: 100,
  create: function(el,options,callback){
    var obj = new AbstractStage;
    obj.element = el;
    callback.call(this,obj,options);
  },
  prototype: {
    attach: function(core){
      this.init(core);
      core.ui.stage = this;
    },
    triggerEvent: function(ev){
      $(this.element).trigger(ev);
      return this;
    },
    getElement: function(){
      return this.element;
    }
  }
});
Jcrop.registerStageType('Block',AbstractStage);


var ImageStage = function(){
};

ImageStage.prototype = new AbstractStage();

$.extend(ImageStage,{
  isSupported: function(el,o){
    if (el.tagName == 'IMG') return true;
  },
  priority: 90,
  create: function(el,options,callback){
    $.Jcrop.component.ImageLoader.attach(el,function(w,h){
      var obj = new ImageStage;
      obj.element = $(el).wrap('<div />').parent();

      obj.element.width(w).height(h);
      obj.imgsrc = el;

      if (typeof callback == 'function')
        callback.call(this,obj,options);
    });
  }
});
Jcrop.registerStageType('Image',ImageStage);


var CanvasStage = function(){
  this.angle = 0;
  this.scale = 1;
  this.scaleMin = 0.2;
  this.scaleMax = 1.25;
  this.offset = [0,0];
};

CanvasStage.prototype = new ImageStage();

$.extend(CanvasStage,{
  isSupported: function(el,o){
    if ($.Jcrop.supportsCanvas && (el.tagName == 'IMG')) return true;
  },
  priority: 60,
  create: function(el,options,callback){
    var $el = $(el);
    var opt = $.extend({},options);
    $.Jcrop.component.ImageLoader.attach(el,function(w,h){
      var obj = new CanvasStage;
      $el.hide();
      obj.createCanvas(el,w,h);
      $el.before(obj.element);
      obj.imgsrc = el;
      opt.imgsrc = el;

      if (typeof callback == 'function'){
        callback(obj,opt);
        obj.redraw();
      }
    });
  }
});

$.extend(CanvasStage.prototype,{
  init: function(core){
    this.core = core;
  },
  // setOffset: function(x,y) {{{
  setOffset: function(x,y) {
    this.offset = [x,y];
    return this;
  },
  // }}}
  // setAngle: function(v) {{{
  setAngle: function(v) {
    this.angle = v;
    return this;
  },
  // }}}
  // setScale: function(v) {{{
  setScale: function(v) {
    this.scale = this.boundScale(v);
    return this;
  },
  // }}}
  boundScale: function(v){
    if (v<this.scaleMin) v = this.scaleMin;
    else if (v>this.scaleMax) v = this.scaleMax;
    return v;
  },
  createCanvas: function(img,w,h){
    this.width = w;
    this.height = h;
    this.canvas = document.createElement('canvas');
    this.canvas.width = w;
    this.canvas.height = h;
    this.$canvas = $(this.canvas).width('100%').height('100%');
    this.context = this.canvas.getContext('2d');
    this.fillstyle = "rgb(0,0,0)";
    this.element = this.$canvas.wrap('<div />').parent().width(w).height(h);
  },
  triggerEvent: function(ev){
    this.$canvas.trigger(ev);
    return this;
  },
  // clear: function() {{{
  clear: function() {
    this.context.fillStyle = this.fillstyle;
    this.context.fillRect(0, 0, this.canvas.width, this.canvas.height);
    return this;
  },
  // }}}
  // redraw: function() {{{
  redraw: function() {
    // Save the current context
    this.context.save();
    this.clear();

    // Translate to the center point of our image
    this.context.translate(parseInt(this.width * 0.5), parseInt(this.height * 0.5));
    // Perform the rotation and scaling
    this.context.translate(this.offset[0]/this.core.opt.xscale,this.offset[1]/this.core.opt.yscale);
    this.context.rotate(this.angle * (Math.PI/180));
    this.context.scale(this.scale,this.scale);
    // Translate back to the top left of our image
    this.context.translate(-parseInt(this.width * 0.5), -parseInt(this.height * 0.5));
    // Finally we draw the image
    this.context.drawImage(this.imgsrc,0,0,this.width,this.height);

    // And restore the updated context
    this.context.restore();
    this.$canvas.trigger('cropredraw');
    return this;
  },
  // }}}
  // setFillStyle: function(v) {{{
  setFillStyle: function(v) {
    this.fillstyle = v;
    return this;
  }
  // }}}
});

Jcrop.registerStageType('Canvas',CanvasStage);


  /**
   *  BackoffFilter
   *  move out-of-bounds selection into allowed position at same size
   */
  var BackoffFilter = function(){
    this.minw = 40;
    this.minh = 40;
    this.maxw = 0;
    this.maxh = 0;
    this.core = null;
  };
  $.extend(BackoffFilter.prototype,{
    tag: 'backoff',
    priority: 22,
    filter: function(b){
      var r = this.bound;

      if (b.x < r.minx) { b.x = r.minx; b.x2 = b.w + b.x; }
      if (b.y < r.miny) { b.y = r.miny; b.y2 = b.h + b.y; }
      if (b.x2 > r.maxx) { b.x2 = r.maxx; b.x = b.x2 - b.w; }
      if (b.y2 > r.maxy) { b.y2 = r.maxy; b.y = b.y2 - b.h; }

      return b;
    },
    refresh: function(sel){
      this.elw = sel.core.container.width();
      this.elh = sel.core.container.height();
      this.bound = {
        minx: 0 + sel.edge.w,
        miny: 0 + sel.edge.n,
        maxx: this.elw + sel.edge.e,
        maxy: this.elh + sel.edge.s
      };
    }
  });
  Jcrop.registerFilter('backoff',BackoffFilter);

  /**
   *  ConstrainFilter
   *  a filter to constrain crop selection to bounding element
   */
  var ConstrainFilter = function(){
    this.core = null;
  };
  $.extend(ConstrainFilter.prototype,{
    tag: 'constrain',
    priority: 5,
    filter: function(b,ord){
      if (ord == 'move') {
        if (b.x < this.minx) { b.x = this.minx; b.x2 = b.w + b.x; }
        if (b.y < this.miny) { b.y = this.miny; b.y2 = b.h + b.y; }
        if (b.x2 > this.maxx) { b.x2 = this.maxx; b.x = b.x2 - b.w; }
        if (b.y2 > this.maxy) { b.y2 = this.maxy; b.y = b.y2 - b.h; }
      } else {
        if (b.x < this.minx) { b.x = this.minx; }
        if (b.y < this.miny) { b.y = this.miny; }
        if (b.x2 > this.maxx) { b.x2 = this.maxx; }
        if (b.y2 > this.maxy) { b.y2 = this.maxy; }
      }
      b.w = b.x2 - b.x;
      b.h = b.y2 - b.y;
      return b;
    },
    refresh: function(sel){
      this.elw = sel.core.container.width();
      this.elh = sel.core.container.height();
      this.minx = 0 + sel.edge.w;
      this.miny = 0 + sel.edge.n;
      this.maxx = this.elw + sel.edge.e;
      this.maxy = this.elh + sel.edge.s;
    }
  });
  Jcrop.registerFilter('constrain',ConstrainFilter);

  /**
   *  ExtentFilter
   *  a filter to implement minimum or maximum size
   */
  var ExtentFilter = function(){
    this.core = null;
  };
  $.extend(ExtentFilter.prototype,{
    tag: 'extent',
    priority: 12,
    offsetFromCorner: function(corner,box,b){
      var w = box[0], h = box[1];
      switch(corner){
        case 'bl': return [ b.x2 - w, b.y, w, h ];
        case 'tl': return [ b.x2 - w , b.y2 - h, w, h ];
        case 'br': return [ b.x, b.y, w, h ];
        case 'tr': return [ b.x, b.y2 - h, w, h ];
      }
    },
    getQuadrant: function(s){
      var relx = s.opposite[0]-s.offsetx
      var rely = s.opposite[1]-s.offsety;

      if ((relx < 0) && (rely < 0)) return 'br';
        else if ((relx >= 0) && (rely >= 0)) return 'tl';
        else if ((relx < 0) && (rely >= 0)) return 'tr';
        return 'bl';
    },
    filter: function(b,ord,sel){

      if (ord == 'move') return b;

      var w = b.w, h = b.h, st = sel.state, r = this.limits;
      var quad = st? this.getQuadrant(st): 'br';

      if (r.minw && (w < r.minw)) w = r.minw;
      if (r.minh && (h < r.minh)) h = r.minh;
      if (r.maxw && (w > r.maxw)) w = r.maxw;
      if (r.maxh && (h > r.maxh)) h = r.maxh;

      if ((w == b.w) && (h == b.h)) return b;

      return Jcrop.wrapFromXywh(this.offsetFromCorner(quad,[w,h],b));
    },
    refresh: function(sel){
      this.elw = sel.core.container.width();
      this.elh = sel.core.container.height();

      this.limits = {
        minw: sel.minSize[0],
        minh: sel.minSize[1],
        maxw: sel.maxSize[0],
        maxh: sel.maxSize[1]
      };
    }
  });
  Jcrop.registerFilter('extent',ExtentFilter);


  /**
   *  GridFilter
   *  a rudimentary grid effect
   */
  var GridFilter = function(){
    this.stepx = 1;
    this.stepy = 1;
    this.core = null;
  };
  $.extend(GridFilter.prototype,{
    tag: 'grid',
    priority: 19,
    filter: function(b){
      
      var n = {
        x: Math.round(b.x / this.stepx) * this.stepx,
        y: Math.round(b.y / this.stepy) * this.stepy,
        x2: Math.round(b.x2 / this.stepx) * this.stepx,
        y2: Math.round(b.y2 / this.stepy) * this.stepy
      };
      
      n.w = n.x2 - n.x;
      n.h = n.y2 - n.y;

      return n;
    }
  });
  Jcrop.registerFilter('grid',GridFilter);


  /**
   *  RatioFilter
   *  implements aspectRatio locking
   */
  var RatioFilter = function(){
    this.ratio = 0;
    this.core = null;
  };
  $.extend(RatioFilter.prototype,{
    tag: 'ratio',
    priority: 15,
    offsetFromCorner: function(corner,box,b){
      var w = box[0], h = box[1];
      switch(corner){
        case 'bl': return [ b.x2 - w, b.y, w, h ];
        case 'tl': return [ b.x2 - w , b.y2 - h, w, h ];
        case 'br': return [ b.x, b.y, w, h ];
        case 'tr': return [ b.x, b.y2 - h, w, h ];
      }
    },
    getBoundRatio: function(b,quad){
      var box = Jcrop.getLargestBox(this.ratio,b.w,b.h);
      return Jcrop.wrapFromXywh(this.offsetFromCorner(quad,box,b));
    },
    getQuadrant: function(s){
      var relx = s.opposite[0]-s.offsetx
      var rely = s.opposite[1]-s.offsety;

      if ((relx < 0) && (rely < 0)) return 'br';
        else if ((relx >= 0) && (rely >= 0)) return 'tl';
        else if ((relx < 0) && (rely >= 0)) return 'tr';
        return 'bl';
    },
    filter: function(b,ord,sel){

      if (!this.ratio) return b;

      var rt = b.w / b.h;
      var st = sel.state;

      var quad = st? this.getQuadrant(st): 'br';
      ord = ord || 'se';

      if (ord == 'move') return b;

      switch(ord) {
        case 'n':
          b.x2 = this.elw;
          b.w = b.x2 - b.x;
          quad = 'tr';
          break;
        case 's':
          b.x2 = this.elw;
          b.w = b.x2 - b.x;
          quad = 'br';
          break;
        case 'e':
          b.y2 = this.elh;
          b.h = b.y2 - b.y;
          quad = 'br';
          break;
        case 'w':
          b.y2 = this.elh;
          b.h = b.y2 - b.y;
          quad = 'bl';
          break;
      }

      return this.getBoundRatio(b,quad);
    },
    refresh: function(sel){
      this.ratio = sel.aspectRatio;
      this.elw = sel.core.container.width();
      this.elh = sel.core.container.height();
    }
  });
  Jcrop.registerFilter('ratio',RatioFilter);


  /**
   *  RoundFilter
   *  rounds coordinate values to integers
   */
  var RoundFilter = function(){
    this.core = null;
  };
  $.extend(RoundFilter.prototype,{
    tag: 'round',
    priority: 90,
    filter: function(b){
      
      var n = {
        x: Math.round(b.x),
        y: Math.round(b.y),
        x2: Math.round(b.x2),
        y2: Math.round(b.y2)
      };
      
      n.w = n.x2 - n.x;
      n.h = n.y2 - n.y;

      return n;
    }
  });
  Jcrop.registerFilter('round',RoundFilter);


  /**
   *  ShadeFilter
   *  A filter that implements div-based shading on any element
   *
   *  The shading you see is actually four semi-opaque divs
   *  positioned inside the container, around the selection
   */
  var ShadeFilter = function(opacity,color){
    this.color = color || 'black';
    this.opacity = opacity || 0.5;
    this.core = null;
    this.shades = {};
  };
  $.extend(ShadeFilter.prototype,{
    tag: 'shader',
    fade: true,
    fadeEasing: 'swing',
    fadeSpeed: 320,
    priority: 95,
    init: function(){
      var t = this;

      if (!t.attached) {
        t.visible = false;

        t.container = $('<div />').addClass(t.core.opt.css_shades)
          .prependTo(this.core.container).hide();

        t.elh = this.core.container.height();
        t.elw = this.core.container.width();

        t.shades = {
          top: t.createShade(),
          right: t.createShade(),
          left: t.createShade(),
          bottom: t.createShade()
        };

        t.attached = true;
      }
    },
    destroy: function(){
      this.container.remove();
    },
    setColor: function(color,instant){
      var t = this;

      if (color == t.color) return t;

      this.color = color;
      var colorfade = Jcrop.supportsColorFade();
      $.each(t.shades,function(u,i){
        if (!t.fade || instant || !colorfade) i.css('backgroundColor',color);
          else i.animate({backgroundColor:color},{queue:false,duration:t.fadeSpeed,easing:t.fadeEasing});
      });
      return t;
    },
    setOpacity: function(opacity,instant){
      var t = this;

      if (opacity == t.opacity) return t;

      t.opacity = opacity;
      $.each(t.shades,function(u,i){
        if (!t.fade || instant) i.css({opacity:opacity});
          else i.animate({opacity:opacity},{queue:false,duration:t.fadeSpeed,easing:t.fadeEasing});
      });
      return t;
    },
    createShade: function(){
      return $('<div />').css({
        position: 'absolute',
        backgroundColor: this.color,
        opacity: this.opacity
      }).appendTo(this.container);
    },
    refresh: function(sel){
      var m = this.core, s = this.shades;

      this.setColor(sel.bgColor?sel.bgColor:this.core.opt.bgColor);
      this.setOpacity(sel.bgOpacity?sel.bgOpacity:this.core.opt.bgOpacity);
        
      this.elh = m.container.height();
      this.elw = m.container.width();
      s.right.css('height',this.elh+'px');
      s.left.css('height',this.elh+'px');
    },
    filter: function(b,ord,sel){

      if (!sel.active) return b;

      var t = this,
        s = t.shades;
      
      s.top.css({
        left: Math.round(b.x)+'px',
        width: Math.round(b.w)+'px',
        height: Math.round(b.y)+'px'
      });
      s.bottom.css({
        top: Math.round(b.y2)+'px',
        left: Math.round(b.x)+'px',
        width: Math.round(b.w)+'px',
        height: (t.elh-Math.round(b.y2))+'px'
      });
      s.right.css({
        left: Math.round(b.x2)+'px',
        width: (t.elw-Math.round(b.x2))+'px'
      });
      s.left.css({
        width: Math.round(b.x)+'px'
      });

      if (!t.visible) {
        t.container.show();
        t.visible = true;
      }

      return b;
    }
  });
  Jcrop.registerFilter('shader',ShadeFilter);
  

  /**
   *  CanvasAnimator
   *  manages smooth cropping animation
   *
   *  This object is called internally to manage animation.
   *  An in-memory div is animated and a progress callback
   *  is used to update the selection coordinates of the
   *  visible selection in realtime.
   */
  var CanvasAnimator = function(stage){
    this.stage = stage;
    this.core = stage.core;
    this.cloneStagePosition();
  };

  CanvasAnimator.prototype = {

    cloneStagePosition: function(){
      var s = this.stage;
      this.angle = s.angle;
      this.scale = s.scale;
      this.offset = s.offset;
    },

    getElement: function(){
      var s = this.stage;

      return $('<div />')
        .css({
          position: 'absolute',
          top: s.offset[0]+'px',
          left: s.offset[1]+'px',
          width: s.angle+'px',
          height: s.scale+'px'
        });
    },

    animate: function(cb){
      var t = this;

      this.scale = this.stage.boundScale(this.scale);
      t.stage.triggerEvent('croprotstart');

      t.getElement().animate({
        top: t.offset[0]+'px',
        left: t.offset[1]+'px',
        width: t.angle+'px',
        height: t.scale+'px'
      },{
        easing: t.core.opt.animEasing,
        duration: t.core.opt.animDuration,
        complete: function(){
          t.stage.triggerEvent('croprotend');
          (typeof cb == 'function') && cb.call(this);
        },
        progress: function(anim){
          var props = {}, i, tw = anim.tweens;

          for(i=0;i<tw.length;i++){
            props[tw[i].prop] = tw[i].now; }

          t.stage.setAngle(props.width)
            .setScale(props.height)
            .setOffset(props.top,props.left)
            .redraw();
        }
      });
    }

  };
  Jcrop.stage.Canvas.prototype.getAnimator = function(){
    return new CanvasAnimator(this);
  };
  Jcrop.registerComponent('CanvasAnimator',CanvasAnimator);


  /**
   *  CropAnimator
   *  manages smooth cropping animation
   *
   *  This object is called internally to manage animation.
   *  An in-memory div is animated and a progress callback
   *  is used to update the selection coordinates of the
   *  visible selection in realtime.
   */
  // var CropAnimator = function(selection){{{
  var CropAnimator = function(selection){
    this.selection = selection;
    this.core = selection.core;
  };
  // }}}

  CropAnimator.prototype = {

    getElement: function(){
      var b = this.selection.get();

      return $('<div />')
        .css({
          position: 'absolute',
          top: b.y+'px',
          left: b.x+'px',
          width: b.w+'px',
          height: b.h+'px'
        });
    },

    animate: function(x,y,w,h,cb){
      var t = this;

      t.selection.allowResize(false);

      t.getElement().animate({
        top: y+'px',
        left: x+'px',
        width: w+'px',
        height: h+'px'
      },{
        easing: t.core.opt.animEasing,
        duration: t.core.opt.animDuration,
        complete: function(){
          t.selection.allowResize(true);
          cb && cb.call(this);
        },
        progress: function(anim){
          var props = {}, i, tw = anim.tweens;

          for(i=0;i<tw.length;i++){
            props[tw[i].prop] = tw[i].now; }

          var b = {
            x: parseInt(props.left),
            y: parseInt(props.top),
            w: parseInt(props.width),
            h: parseInt(props.height)
          };

          b.x2 = b.x + b.w;
          b.y2 = b.y + b.h;

          t.selection.updateRaw(b,'se');
        }
      });
    }

  };
  Jcrop.registerComponent('Animator',CropAnimator);


  /**
   *  DragState
   *  an object that handles dragging events
   *
   *  This object is used by the built-in selection object to
   *  track a dragging operation on a selection
   */
  // var DragState = function(e,selection,ord){{{
  var DragState = function(e,selection,ord){
    var t = this;

    t.x = e.pageX;
    t.y = e.pageY;

    t.selection = selection;
    t.eventTarget = selection.core.opt.dragEventTarget;
    t.orig = selection.get();

    selection.callFilterFunction('refresh');

    var p = selection.core.container.position();
    t.elx = p.left;
    t.ely = p.top;

    t.offsetx = 0;
    t.offsety = 0;
    t.ord = ord;
    t.opposite = t.getOppositeCornerOffset();

    t.initEvents(e);

  };
  // }}}

  DragState.prototype = {
    // getOppositeCornerOffset: function(){{{
    // Calculate relative offset of locked corner
    getOppositeCornerOffset: function(){

      var o = this.orig;
      var relx = this.x - this.elx - o.x;
      var rely = this.y - this.ely - o.y;

      switch(this.ord){
        case 'nw':
        case 'w':
          return [ o.w - relx, o.h - rely ];
          return [ o.x + o.w, o.y + o.h ];

        case 'sw':
          return [ o.w - relx, -rely ];
          return [ o.x + o.w, o.y ];

        case 'se':
        case 's':
        case 'e':
          return [ -relx, -rely ];
          return [ o.x, o.y ];

        case 'ne':
        case 'n':
          return [ -relx, o.h - rely ];
          return [ o.w, o.y + o.h ];
      }

      return [ null, null ];
    },
    // }}}
    // initEvents: function(e){{{
    initEvents: function(e){
      $(this.eventTarget)
        .on('mousemove.jcrop',this.createDragHandler())
        .on('mouseup.jcrop',this.createStopHandler());
    },
    // }}}
    // dragEvent: function(e){{{
    dragEvent: function(e){
      this.offsetx = e.pageX - this.x;
      this.offsety = e.pageY - this.y;
      this.selection.updateRaw(this.getBox(),this.ord);
    },
    // }}}
    // endDragEvent: function(e){{{
    endDragEvent: function(e){
      var sel = this.selection;
      sel.core.container.removeClass('jcrop-dragging');
      sel.element.trigger('cropend',[sel,sel.core.unscale(sel.get())]);
      sel.focus();
    },
    // }}}
    // createStopHandler: function(){{{
    createStopHandler: function(){
      var t = this;
      return function(e){
        $(t.eventTarget).off('.jcrop');
        t.endDragEvent(e);
        return false;
      };
    },
    // }}}
    // createDragHandler: function(){{{
    createDragHandler: function(){
      var t = this;
      return function(e){
        t.dragEvent(e);
        return false;
      };
    },
    // }}}
    //update: function(x,y){{{
    update: function(x,y){
      var t = this;
      t.offsetx = x - t.x;
      t.offsety = y - t.y;
    },
    //}}}
    //resultWrap: function(d){{{
    resultWrap: function(d){
      var b = {
          x: Math.min(d[0],d[2]),
          y: Math.min(d[1],d[3]),
          x2: Math.max(d[0],d[2]),
          y2: Math.max(d[1],d[3])
        };

      b.w = b.x2 - b.x;
      b.h = b.y2 - b.y;

      return b;
    },
    //}}}
    //getBox: function(){{{
    getBox: function(){
      var t = this;
      var o = t.orig;
      var _c = { x2: o.x + o.w, y2: o.y + o.h };
      switch(t.ord){
        case 'n': return t.resultWrap([ o.x, t.offsety + o.y, _c.x2, _c.y2 ]);
        case 's': return t.resultWrap([ o.x, o.y, _c.x2, t.offsety + _c.y2 ]);
        case 'e': return t.resultWrap([ o.x, o.y, t.offsetx + _c.x2, _c.y2 ]);
        case 'w': return t.resultWrap([ o.x + t.offsetx, o.y, _c.x2, _c.y2 ]);
        case 'sw': return t.resultWrap([ t.offsetx + o.x, o.y, _c.x2, t.offsety + _c.y2 ]);
        case 'se': return t.resultWrap([ o.x, o.y, t.offsetx + _c.x2, t.offsety + _c.y2 ]);
        case 'ne': return t.resultWrap([ o.x, t.offsety + o.y, t.offsetx + _c.x2, _c.y2 ]);
        case 'nw': return t.resultWrap([ t.offsetx + o.x, t.offsety + o.y, _c.x2, _c.y2 ]);
        case 'move':
          _c.nx = o.x + t.offsetx;
          _c.ny = o.y + t.offsety;
          return t.resultWrap([ _c.nx, _c.ny, _c.nx + o.w, _c.ny + o.h ]);
      }
    }
    //}}}
  };
  Jcrop.registerComponent('DragState',DragState);


  /**
   *  EventManager
   *  provides internal event support
   */
  var EventManager = function(core){
    this.core = core;
  };
  EventManager.prototype = {
      on: function(n,cb){ $(this).on(n,cb); },
      off: function(n){ $(this).off(n); },
      trigger: function(n){ $(this).trigger(n); }
  };
  Jcrop.registerComponent('EventManager',EventManager);


  /**
   * Image Loader
   * Reliably pre-loads images
   */
  // var ImageLoader = function(src,element,cb){{{
  var ImageLoader = function(src,element,cb){
    this.src = src;
    if (!element) element = new Image;
    this.element = element;
    this.callback = cb;
    this.load();
  };
  // }}}

  $.extend(ImageLoader,{
    // attach: function(el,cb){{{
    attach: function(el,cb){
      return new ImageLoader(el.src,el,cb);
    },
    // }}}
    // prototype: {{{
    prototype: {
      getDimensions: function(){
        var el = this.element;

        if (el.naturalWidth)
          return [ el.naturalWidth, el. naturalHeight ];

        if (el.width)
          return [ el.width, el.height ];

        return null;
      },
      fireCallback: function(){
        this.element.onload = null;
        if (typeof this.callback == 'function')
          this.callback.apply(this,this.getDimensions());
      },
      isLoaded: function(){
        return this.element.complete;
      },
      load: function(){
        var t = this;
        var el = t.element;

        el.src = t.src;

        if (t.isLoaded()) t.fireCallback();
          else t.element.onload = function(e){
            t.fireCallback();
          };
      }
    }
    // }}}
  });
  Jcrop.registerComponent('ImageLoader',ImageLoader);


  /**
   * JcropTouch
   * Detects and enables mobile touch support
   */
  // var JcropTouch = function(core){{{
  var JcropTouch = function(core){
    this.core = core;
    this.init();
  };
  // }}}

  $.extend(JcropTouch,{
    // support: function(){{{
    support: function(){
      if(('ontouchstart' in window) || window.DocumentTouch && document instanceof DocumentTouch)
        return true;
    },
    // }}}
    prototype: {
      // init: function(){{{
      init: function(){
        var t = this,
          p = $.Jcrop.component.DragState.prototype;

        // A bit of an ugly hack to make sure we modify prototype
        // only once, store a key on the prototype
        if (!p.touch) {
          t.initEvents();
          t.shimDragState();
          t.shimStageDrag();
          p.touch = true;
        }
      },
      // }}}
      // shimDragState: function(){{{
      shimDragState: function(){
        var t = this;
        $.Jcrop.component.DragState.prototype.initEvents = function(e){
          
          // Attach subsequent drag event handlers based on initial
          // event type - avoids collecting "pseudo-mouse" events
          // generated by some mobile browsers in some circumstances
          if (e.type.substr(0,5) == 'touch') {

            $(this.eventTarget)
              .on('touchmove.jcrop.jcrop-touch',t.dragWrap(this.createDragHandler()))
              .on('touchend.jcrop.jcrop-touch',this.createStopHandler());

          }
          
          // For other events, use the mouse handlers that
          // the default DragState.initEvents() method sets...
          else {

            $(this.eventTarget)
              .on('mousemove.jcrop',this.createDragHandler())
              .on('mouseup.jcrop',this.createStopHandler());

          }

        };
      },
      // }}}
      // shimStageDrag: function(){{{
      shimStageDrag: function(){
        this.core.container
          .addClass('jcrop-touch')
          .on('touchstart.jcrop.jcrop-stage',this.dragWrap(this.core.ui.manager.startDragHandler()));
      },
      // }}}
      // dragWrap: function(cb){{{
      dragWrap: function(cb){
        return function(e){
          e.preventDefault();
          e.stopPropagation();
          if (e.type.substr(0,5) == 'touch') {
            e.pageX = e.originalEvent.changedTouches[0].pageX;
            e.pageY = e.originalEvent.changedTouches[0].pageY;
            return cb(e);
          }
          return false;
        };
      },
      // }}}
      // initEvents: function(){{{
      initEvents: function(){
        var t = this, c = t.core;

        c.container.on(
          'touchstart.jcrop.jcrop-touch',
          '.'+c.opt.css_drag,
          t.dragWrap(c.startDrag())
        );
      }
      // }}}
    }
  });
  Jcrop.registerComponent('Touch',JcropTouch);


  /**
   *  KeyWatcher
   *  provides keyboard support
   */
  // var KeyWatcher = function(core){{{
  var KeyWatcher = function(core){
    this.core = core;
    this.init();
  };
  // }}}
  $.extend(KeyWatcher,{
    // defaults: {{{
    defaults: {
      eventName: 'keydown.jcrop',
      passthru: [ 9 ],
      debug: false
    },
    // }}}
    prototype: {
      // init: function(){{{
      init: function(){
        $.extend(this,KeyWatcher.defaults);
        this.enable();
      },
      // }}}
      // disable: function(){{{
      disable: function(){
        this.core.container.off(this.eventName);
      },
      // }}}
      // enable: function(){{{
      enable: function(){
        var t = this, m = t.core;
        m.container.on(t.eventName,function(e){
          var nudge = e.shiftKey? 16: 2;

          if ($.inArray(e.keyCode,t.passthru) >= 0)
            return true;

          switch(e.keyCode){
            case 37: m.nudge(-nudge,0); break;
            case 38: m.nudge(0,-nudge); break;
            case 39: m.nudge(nudge,0); break;
            case 40: m.nudge(0,nudge); break;

            case 46:
            case 8:
              m.requestDelete();
              return false;
              break;

            default:
              if (t.debug) console.log('keycode: ' + e.keyCode);
              break;
          }

          if (!e.metaKey && !e.ctrlKey)
            e.preventDefault();
        });
      }
      // }}}
    }
  });
  Jcrop.registerComponent('Keyboard',KeyWatcher);


  /**
   * Selection
   * Built-in selection object
   */
  var Selection = function(){};

  $.extend(Selection,{
    // defaults: {{{
    defaults: {
      minSize: [ 8, 8 ],
      maxSize: [ 0, 0 ],
      aspectRatio: 0,
      edge: { n: 0, s: 0, e: 0, w: 0 },
      bgColor: null,
      bgOpacity: null,
      last: null,

      state: null,
      active: true,
      linked: true,
      canDelete: true,
      canDrag: true,
      canResize: true,
      canSelect: true
    },
    // }}}
    prototype: {
      // init: function(core){{{
      init: function(core){
        this.core = core;
        this.startup();
        this.linked = this.core.opt.linked;
        this.attach();
        this.setOptions(this.core.opt);
        core.container.trigger('cropcreate',[this]);
      },
      // }}}
      // attach: function(){{{
      attach: function(){
        // For extending init() sequence
      },
      // }}}
      // startup: function(){{{
      startup: function(){
        var t = this, o = t.core.opt;
        $.extend(t,Selection.defaults);
        t.filter = t.core.getDefaultFilters();

        t.element = $('<div />').addClass(o.css_selection).data({ selection: t });
        t.frame = $('<button />').addClass(o.css_button).data('ord','move').attr('type','button');
        t.element.append(t.frame).appendTo(t.core.container);

        // IE background/draggable hack
        if (t.core.opt.is_msie) t.frame.css({
          opacity: 0,
          backgroundColor: 'white'
        });

        t.insertElements();

        // Bind focus and blur events for this selection
        t.frame.on('focus.jcrop',function(e){
          t.core.setSelection(t);
          t.element.trigger('cropfocus',t);
          t.element.addClass('jcrop-focus');
        }).on('blur.jcrop',function(e){
          t.element.removeClass('jcrop-focus');
          t.element.trigger('cropblur',t);
        });
      },
      // }}}
      // propagate: [{{{
      propagate: [
        'canDelete', 'canDrag', 'canResize', 'canSelect',
        'minSize', 'maxSize', 'aspectRatio', 'edge'
      ],
      // }}}
      // setOptions: function(opt){{{
      setOptions: function(opt){
        Jcrop.propagate(this.propagate,opt,this);
        this.refresh();
        return this;
      },
      // }}}
      // refresh: function(){{{
      refresh: function(){
        this.allowResize();
        this.allowDrag();
        this.allowSelect();
        this.callFilterFunction('refresh');
        this.updateRaw(this.get(),'se');
      },
      // }}}
      // callFilterFunction: function(f,args){{{
      callFilterFunction: function(f,args){
        for(var i=0;i<this.filter.length;i++)
          if (this.filter[i][f]) this.filter[i][f](this);
        return this;
      },
      // }}}
      //addFilter: function(filter){{{
      addFilter: function(filter){
        filter.core = this.core;
        if (!this.hasFilter(filter)) {
          this.filter.push(filter);
          this.sortFilters();
          if (filter.init) filter.init();
          this.refresh();
        }
      },
      //}}}
      // hasFilter: function(filter){{{
      hasFilter: function(filter){
        var i, f = this.filter, n = [];
        for(i=0;i<f.length;i++) if (f[i] === filter) return true;
      },
      // }}}
      // sortFilters: function(){{{
      sortFilters: function(){
        this.filter.sort(
          function(x,y){ return x.priority - y.priority; }
        );
      },
      // }}}
      //clearFilters: function(){{{
      clearFilters: function(){
        var i, f = this.filter;

        for(var i=0;i<f.length;i++)
          if (f[i].destroy) f[i].destroy();

        this.filter = [];
      },
      //}}}
      // removeFiltersByTag: function(tag){{{
      removeFilter: function(tag){
        var i, f = this.filter, n = [];

        for(var i=0;i<f.length;i++)
          if ((f[i].tag && (f[i].tag == tag)) || (tag === f[i])){
            if (f[i].destroy) f[i].destroy();
          }
          else n.push(f[i]);

        this.filter = n;
      },
      // }}}
      // runFilters: function(b,ord){{{
      runFilters: function(b,ord){
        for(var i=0;i<this.filter.length;i++)
          b = this.filter[i].filter(b,ord,this);
        return b;
      },
      // }}}
      //endDrag: function(){{{
      endDrag: function(){
        if (this.state) {
          $(document.body).off('.jcrop');
          this.focus();
          this.state = null;
        }
      },
      //}}}
      // startDrag: function(e,ord){{{
      startDrag: function(e,ord){
        var t = this;
        var m = t.core;

        ord = ord || $(e.target).data('ord');

        this.focus();

        if ((ord == 'move') && t.element.hasClass(t.core.opt.css_nodrag))
          return false;

        this.state = new Jcrop.component.DragState(e,this,ord);
        return false;
      },
      // }}}
      // allowSelect: function(v){{{
      allowSelect: function(v){
        if (v === undefined) v = this.canSelect;

        if (v && this.canSelect) this.frame.attr('disabled',false);
          else this.frame.attr('disabled','disabled');

        return this;
      },
      // }}}
      // allowDrag: function(v){{{
      allowDrag: function(v){
        var t = this, o = t.core.opt;
        if (v == undefined) v = t.canDrag;

        if (v && t.canDrag) t.element.removeClass(o.css_nodrag);
          else t.element.addClass(o.css_nodrag);

        return this;
      },
      // }}}
      // allowResize: function(v){{{
      allowResize: function(v){
        var t = this, o = t.core.opt;
        if (v == undefined) v = t.canResize;

        if (v && t.canResize) t.element.removeClass(o.css_noresize);
          else t.element.addClass(o.css_noresize);

        return this;
      },
      // }}}
      // remove: function(){{{
      remove: function(){
        this.element.trigger('cropremove',this);
        this.element.remove();
      },
      // }}}
      // toBack: function(){{{
      toBack: function(){
        this.active = false;
        this.element.removeClass('jcrop-current jcrop-focus');
      },
      // }}}
      // toFront: function(){{{
      toFront: function(){
        this.active = true;
        this.element.addClass('jcrop-current');
        this.callFilterFunction('refresh');
        this.refresh();
      },
      // }}}
      // redraw: function(b){{{
      redraw: function(b){
        this.moveTo(b.x,b.y);
        this.resize(b.w,b.h);
        this.last = b;
        return this;
      },
      // }}}
      // update: function(b,ord){{{
      update: function(b,ord){
        return this.updateRaw(this.core.scale(b),ord);
      },
      // }}}
      // update: function(b,ord){{{
      updateRaw: function(b,ord){
        b = this.runFilters(b,ord);
        this.redraw(b);
        this.element.trigger('cropmove',[this,this.core.unscale(b)]);
        return this;
      },
      // }}}
      // animateTo: function(box,cb){{{
      animateTo: function(box,cb){
        var ca = new Jcrop.component.Animator(this),
            b = this.core.scale(Jcrop.wrapFromXywh(box));

        ca.animate(b.x,b.y,b.w,b.h,cb);
      },
      // }}}
      // center: function(instant){{{
      center: function(instant){
        var b = this.get(), m = this.core;
        var elw = m.container.width(), elh = m.container.height();
        var box = [ (elw-b.w)/2, (elh-b.h)/2, b.w, b.h ];
        return this[instant?'setSelect':'animateTo'](box);
      },
      // }}}
      //createElement: function(type,ord){{{
      createElement: function(type,ord){
        return $('<div />').addClass(type+' ord-'+ord).data('ord',ord);
      },
      //}}}
      //moveTo: function(x,y){{{
      moveTo: function(x,y){
        this.element.css({top: y+'px', left: x+'px'});
      },
      //}}}
      // blur: function(){{{
      blur: function(){
        this.element.blur();
        return this;
      },
      // }}}
      // focus: function(){{{
      focus: function(){
        this.core.setSelection(this);
        this.frame.focus();
        return this;
      },
      // }}}
      //resize: function(w,h){{{
      resize: function(w,h){
        this.element.css({width: w+'px', height: h+'px'});
      },
      //}}}
      //get: function(){{{
      get: function(){
        var b = this.element,
          o = b.position(),
          w = b.width(),
          h = b.height(),
          rv = { x: o.left, y: o.top };

        rv.x2 = rv.x + w;
        rv.y2 = rv.y + h;
        rv.w = w;
        rv.h = h;

        return rv;
      },
      //}}}
      //insertElements: function(){{{
      insertElements: function(){
        var t = this, i,
          m = t.core,
          fr = t.element,
          o = t.core.opt,
          b = o.borders,
          h = o.handles,
          d = o.dragbars;

        for(i=0; i<d.length; i++)
          fr.append(t.createElement(o.css_dragbars,d[i]));

        for(i=0; i<h.length; i++)
          fr.append(t.createElement(o.css_handles,h[i]));

        for(i=0; i<b.length; i++)
          fr.append(t.createElement(o.css_borders,b[i]));
      }
      //}}}
    }
  });
  Jcrop.registerComponent('Selection',Selection);


  /**
   * StageDrag
   * Facilitates dragging
   */
  // var StageDrag = function(manager,opt){{{
  var StageDrag = function(manager,opt){
    $.extend(this,StageDrag.defaults,opt || {});
    this.manager = manager;
    this.core = manager.core;
  };
  // }}}
  // StageDrag.defaults = {{{
  StageDrag.defaults = {
    offset: [ -8, -8 ],
    active: true,
    minsize: [ 20, 20 ]
  };
  // }}}

  $.extend(StageDrag.prototype,{
    // start: function(e){{{
    start: function(e){
      var c = this.core;

      // Do nothing if allowSelect is off
      if (!c.opt.allowSelect) return;

      // Also do nothing if we can't draw any more selections
      if (c.opt.multi && c.opt.multiMax && (c.ui.multi.length >= c.opt.multiMax)) return false;

      // calculate a few variables for this drag operation
      var o = $(e.currentTarget).offset();
      var origx = e.pageX - o.left + this.offset[0];
      var origy = e.pageY - o.top + this.offset[1];
      var m = c.ui.multi;

      // Determine newly dragged crop behavior if multi disabled
      if (!c.opt.multi) {
        // For multiCleaanup true, remove all existing selections
        if (c.opt.multiCleanup){
          for(var i=0;i<m.length;i++) m[i].remove();
          c.ui.multi = [];
        }
        // If not, only remove the currently active selection
        else {
          c.removeSelection(c.ui.selection);
        }
      }

      c.container.addClass('jcrop-dragging');

      // Create the new selection
      var sel = c.newSelection()
        // and position it
        .updateRaw(Jcrop.wrapFromXywh([origx,origy,1,1]));

      sel.element.trigger('cropstart',[sel,this.core.unscale(sel.get())]);
      
      return sel.startDrag(e,'se');
    },
    // }}}
    // end: function(x,y){{{
    end: function(x,y){
      this.drag(x,y);
      var b = this.sel.get();

      this.core.container.removeClass('jcrop-dragging');

      if ((b.w < this.minsize[0]) || (b.h < this.minsize[1]))
        this.core.requestDelete();

        else this.sel.focus();
    }
    // }}}
  });
  Jcrop.registerComponent('StageDrag',StageDrag);


  /**
   * StageManager
   * Provides basic stage-specific functionality
   */
  // var StageManager = function(core){{{
  var StageManager = function(core){
    this.core = core;
    this.ui = core.ui;
    this.init();
  };
  // }}}

  $.extend(StageManager.prototype,{
    // init: function(){{{
    init: function(){
      this.setupEvents();
      this.dragger = new StageDrag(this);
    },
    // }}}
    // tellConfigUpdate: function(options){{{
    tellConfigUpdate: function(options){
      for(var i=0,m=this.ui.multi,l=m.length;i<l;i++)
        if (m[i].setOptions && (m[i].linked || (this.core.opt.linkCurrent && m[i] == this.ui.selection)))
          m[i].setOptions(options);
    },
    // }}}
    // startDragHandler: function(){{{
    startDragHandler: function(){
      var t = this;
      return function(e){
        if (!e.button || t.core.opt.is_ie_lt9) return t.dragger.start(e);
      };
    },
    // }}}
    // removeEvents: function(){{{
    removeEvents: function(){
      this.core.event.off('.jcrop-stage');
      this.core.container.off('.jcrop-stage');
    },
    // }}}
    // shimLegacyHandlers: function(options){{{
    // This method uses the legacyHandlers configuration object to
    // gracefully wrap old-style Jcrop events with new ones
    shimLegacyHandlers: function(options){
      var _x = {}, core = this.core, tmp;

      $.each(core.opt.legacyHandlers,function(k,i){
        if (k in options) {
          tmp = options[k];
          core.container.off('.jcrop-'+k)
            .on(i+'.jcrop.jcrop-'+k,function(e,s,c){
              tmp.call(core,c);
            });
          delete options[k];
        }
      });
    },
    // }}}
    // setupEvents: function(){{{
    setupEvents: function(){
      var t = this, c = t.core;

      c.event.on('configupdate.jcrop-stage',function(e){
        t.shimLegacyHandlers(c.opt);
        t.tellConfigUpdate(c.opt)
        c.container.trigger('cropconfig',[c,c.opt]);
      });

      this.core.container
        .on('mousedown.jcrop.jcrop-stage',this.startDragHandler());
    }
    // }}}
  });
  Jcrop.registerComponent('StageManager',StageManager);


  var Thumbnailer = function(){
  };

  $.extend(Thumbnailer,{
    defaults: {
      // Set to a specific Selection object
      // If this value is set, the preview will only track that Selection
      selection: null,

      fading: true,
      fadeDelay: 1000,
      fadeDuration: 1000,
      autoHide: false,
      width: 80,
      height: 80,
      _hiding: null
    },

    prototype: {
      recopyCanvas: function(){
        var s = this.core.ui.stage, cxt = s.context;
        this.context.putImageData(cxt.getImageData(0,0,s.canvas.width,s.canvas.height),0,0);
      },
      init: function(core,options){
        var t = this;
        this.core = core;
        $.extend(this,Thumbnailer.defaults,options);
        t.initEvents();
        t.refresh();
        t.insertElements();
        if (t.selection) {
          t.renderSelection(t.selection);
          t.selectionTarget = t.selection.element[0];
        } else if (t.core.ui.selection) {
          t.renderSelection(t.core.ui.selection);
        }

        if (t.core.ui.stage.canvas) {
          t.context = t.preview[0].getContext('2d');
          t.core.container.on('cropredraw',function(e){
            t.recopyCanvas();
            t.refresh();
          });
        }
      },
      updateImage: function(imgel){
        this.preview.remove();
        this.preview = $($.Jcrop.imageClone(imgel));
        this.element.append(this.preview);
        this.refresh();
        return this;
      },
      insertElements: function(){
        this.preview = $($.Jcrop.imageClone(this.core.ui.stage.imgsrc));

        this.element = $('<div />').addClass('jcrop-thumb')
          .width(this.width).height(this.height)
          .append(this.preview)
          .appendTo(this.core.container);
      },
      resize: function(w,h){
        this.width = w;
        this.height = h;
        this.element.width(w).height(h);
        this.renderCoords(this.last);
      },
      refresh: function(){
        this.cw = (this.core.opt.xscale * this.core.container.width());
        this.ch = (this.core.opt.yscale * this.core.container.height());
        if (this.last) {
          this.renderCoords(this.last);
        }
      },
      renderCoords: function(c){
        var rx = this.width / c.w;
        var ry = this.height / c.h;

        this.preview.css({
          width: Math.round(rx * this.cw) + 'px',
          height: Math.round(ry * this.ch) + 'px',
          marginLeft: '-' + Math.round(rx * c.x) + 'px',
          marginTop: '-' + Math.round(ry * c.y) + 'px'
        });

        this.last = c;
        return this;
      },
      renderSelection: function(s){
        return this.renderCoords(s.core.unscale(s.get()));
      },
      selectionStart: function(s){
        this.renderSelection(s);
      },
      show: function(){
        if (this._hiding) clearTimeout(this._hiding);

        if (!this.fading) this.element.stop().css({ opacity: 1 });
        else this.element.stop().animate({ opacity: 1 },{ duration: 80, queue: false });
      },
      hide: function(){
        var t = this;
        if (!t.fading) t.element.hide();
        else t._hiding = setTimeout(function(){
          t._hiding = null;
          t.element.stop().animate({ opacity: 0 },{ duration: t.fadeDuration, queue: false });
        },t.fadeDelay);
      },
      initEvents: function(){
        var t = this;
        t.core.container.on('croprotstart croprotend cropimage cropstart cropmove cropend',function(e,s,c){
          if (t.selectionTarget && (t.selectionTarget !== e.target)) return false;

          switch(e.type){

            case 'cropimage':
              t.updateImage(c);
              break;

            case 'cropstart':
              t.selectionStart(s);
            case 'croprotstart':
              t.show();
              break;

            case 'cropend':
              t.renderCoords(c);
            case 'croprotend':
              if (t.autoHide) t.hide();
              break;

            case 'cropmove':
              t.renderCoords(c);
              break;
          }
        });
      }
    }
  });
  Jcrop.registerComponent('Thumbnailer',Thumbnailer);


  /**
   * DialDrag component
   * This is a little hacky, it was adapted from some previous/old code
   * Plan to update this API in the future
   */
  var DialDrag = function() { };

  DialDrag.prototype = {

    init: function(core,actuator,callback){
      var that = this;

      if (!actuator) actuator = core.container;
      this.$btn = $(actuator);
      this.$targ = $(actuator);
      this.core = core;

      this.$btn
        .addClass('dialdrag')
        .on('mousedown.dialdrag',this.mousedown())
        .data('dialdrag',this);

      if (!$.isFunction(callback)) callback = function(){ };
      this.callback = callback;
      this.ondone = callback;
    },

    remove: function(){
      this.$btn
        .removeClass('dialdrag')
        .off('.dialdrag')
        .data('dialdrag',null);
      return this;
    },

    setTarget: function(obj){
      this.$targ = $(obj);
      return this;
    },

    getOffset: function(){
      var targ = this.$targ, pos = targ.offset();
      return [
        pos.left + (targ.width()/2),
        pos.top + (targ.height()/2)
      ];
    },

    relMouse: function(e){
      var x = e.pageX - this.offset[0],
          y = e.pageY - this.offset[1],
          ang = Math.atan2(y,x) * (180 / Math.PI),
          vec = Math.sqrt(Math.pow(x,2)+Math.pow(y,2));
      return [ x, y, ang, vec ];
    },

    mousedown: function(){
      var that = this;

      function mouseUp(e){
        $(window).off('.dialdrag');
        that.ondone.call(that,that.relMouse(e));
        that.core.container.trigger('croprotend');
      }

      function mouseMove(e){
        that.callback.call(that,that.relMouse(e));
      }

      return function(e) {
        that.offset = that.getOffset();
        var rel = that.relMouse(e);
        that.angleOffset = -that.core.ui.stage.angle+rel[2];
        that.distOffset = rel[3];
        that.dragOffset = [rel[0],rel[1]];
        that.core.container.trigger('croprotstart');

        $(window)
          .on('mousemove.dialdrag',mouseMove)
          .on('mouseup.dialdrag',mouseUp);

        that.callback.call(that,that.relMouse(e));

        return false;
      };
    }
    
  };
  Jcrop.registerComponent('DialDrag',DialDrag);


    /////////////////////////////////
    // DEFAULT SETTINGS

    Jcrop.defaults = {

      // Selection Behavior
      edge: { n: 0, s: 0, e: 0, w: 0 },
      setSelect: null,
      linked: true,
      linkCurrent: true,
      canDelete: true,
      canSelect: true,
      canDrag: true,
      canResize: true,

      // Component constructors
      eventManagerComponent:  Jcrop.component.EventManager,
      keyboardComponent:      Jcrop.component.Keyboard,
      dragstateComponent:     Jcrop.component.DragState,
      stagemanagerComponent:  Jcrop.component.StageManager,
      animatorComponent:      Jcrop.component.Animator,
      selectionComponent:     Jcrop.component.Selection,

      // This is a function that is called, which returns a stage object
      stageConstructor:       Jcrop.stageConstructor,

      // Stage Behavior
      allowSelect: true,
      multi: false,
      multiMax: false,
      multiCleanup: true,
      animation: true,
      animEasing: 'swing',
      animDuration: 400,
      fading: true,
      fadeDuration: 300,
      fadeEasing: 'swing',
      bgColor: 'black',
      bgOpacity: .5,

      // Startup options
      applyFilters: [ 'constrain', 'extent', 'backoff', 'ratio', 'shader', 'round' ],
      borders:  [ 'e', 'w', 's', 'n' ],
      handles:  [ 'n', 's', 'e', 'w', 'sw', 'ne', 'nw', 'se' ],
      dragbars: [ 'n', 'e', 'w', 's' ],

      dragEventTarget: window,

      xscale: 1,
      yscale: 1,

      boxWidth: null,
      boxHeight: null,

      // CSS Classes
      // @todo: These need to be moved to top-level object keys
      // for better customization. Currently if you try to extend one
      // via an options object to Jcrop, it will wipe out all
      // the others you don't specify. Be careful for now!
      css_nodrag: 'jcrop-nodrag',
      css_drag: 'jcrop-drag',
      css_container: 'jcrop-active',
      css_shades: 'jcrop-shades',
      css_selection: 'jcrop-selection',
      css_borders: 'jcrop-border',
      css_handles: 'jcrop-handle jcrop-drag',
      css_button: 'jcrop-box jcrop-drag',
      css_noresize: 'jcrop-noresize',
      css_dragbars: 'jcrop-dragbar jcrop-drag',

      legacyHandlers: {
        onChange: 'cropmove',
        onSelect: 'cropend'
      }

    };


  // Jcrop API methods
  $.extend(Jcrop.prototype,{
    //init: function(){{{
    init: function(){
      this.event = new this.opt.eventManagerComponent(this);
      this.ui.keyboard = new this.opt.keyboardComponent(this);
      this.ui.manager = new this.opt.stagemanagerComponent(this);
      this.applyFilters();

      if ($.Jcrop.supportsTouch)
        new $.Jcrop.component.Touch(this);

      this.initEvents();
    },
    //}}}
    // applySizeConstraints: function(){{{
    applySizeConstraints: function(){
      var o = this.opt,
          img = this.opt.imgsrc;

      if (img){

        var iw = img.naturalWidth || img.width,
            ih = img.naturalHeight || img.height,
            bw = o.boxWidth || iw,
            bh = o.boxHeight || ih;

        if (img && ((iw > bw) || (ih > bh))){
          var bx = Jcrop.getLargestBox(iw/ih,bw,bh);
          $(img).width(bx[0]).height(bx[1]);
          this.resizeContainer(bx[0],bx[1]);
          this.opt.xscale = iw / bx[0];
          this.opt.yscale = ih / bx[1];
        }
          
      }

      if (this.opt.trueSize){
        var dw = this.opt.trueSize[0];
        var dh = this.opt.trueSize[1];
        var cs = this.getContainerSize();
        this.opt.xscale = dw / cs[0];
        this.opt.yscale = dh / cs[1];
      }
    },
    // }}}
    initComponent: function(name){
      if (Jcrop.component[name]) {
        var args = Array.prototype.slice.call(arguments);
        var obj = new Jcrop.component[name];
        args.shift();
        args.unshift(this);
        obj.init.apply(obj,args);
        return obj;
      }
    },
    // setOptions: function(opt){{{
    setOptions: function(opt,proptype){

      if (!$.isPlainObject(opt)) opt = {};

      $.extend(this.opt,opt);

      // Handle a setSelect value
      if (this.opt.setSelect) {

        // If there is no current selection
        // passing setSelect will create one
        if (!this.ui.multi.length)
          this.newSelection();

        // Use these values to update the current selection
        this.setSelect(this.opt.setSelect);

        // Set to null so it doesn't get called again
        this.opt.setSelect = null;
      }

      this.event.trigger('configupdate');
      return this;
    },
    // }}}
    //destroy: function(){{{
    destroy: function(){
      if (this.opt.imgsrc) {
        this.container.before(this.opt.imgsrc);
        this.container.remove();
        $(this.opt.imgsrc).removeData('Jcrop').show();
      } else {
        // @todo: more elegant destroy() process for non-image containers
        this.container.remove();
      }
    },
    // }}}
    // applyFilters: function(){{{
    applyFilters: function(){
      var obj;
      for(var i=0,f=this.opt.applyFilters,l=f.length; i<l; i++){
        if ($.Jcrop.filter[f[i]])
          obj = new $.Jcrop.filter[f[i]];
          obj.core = this;
          if (obj.init) obj.init();
          this.filter[f[i]] = obj;
      }
    },
    // }}}
    // getDefaultFilters: function(){{{
    getDefaultFilters: function(){
      var rv = [];

      for(var i=0,f=this.opt.applyFilters,l=f.length; i<l; i++)
        if(this.filter.hasOwnProperty(f[i]))
          rv.push(this.filter[f[i]]);

      rv.sort(function(x,y){ return x.priority - y.priority; });
      return rv;
    },
    // }}}
    // setSelection: function(sel){{{
    setSelection: function(sel){
      var m = this.ui.multi;
      var n = [];
      for(var i=0;i<m.length;i++) {
        if (m[i] !== sel) n.push(m[i]);
        m[i].toBack();
      }
      n.unshift(sel);
      this.ui.multi = n;
      this.ui.selection = sel;
      sel.toFront();
      return sel;
    },
    // }}}
    // getSelection: function(raw){{{
    getSelection: function(raw){
      var b = this.ui.selection.get();
      return b;
    },
    // }}}
    // newSelection: function(){{{
    newSelection: function(sel){
      if (!sel)
        sel = new this.opt.selectionComponent();

      sel.init(this);
      this.setSelection(sel);

      return sel;
    },
    // }}}
    // hasSelection: function(sel){{{
    hasSelection: function(sel){
      for(var i=0;i<this.ui.multi;i++)
        if (sel === this.ui.multi[i]) return true;
    },
    // }}}
    // removeSelection: function(sel){{{
    removeSelection: function(sel){
      var i, n = [], m = this.ui.multi;
      for(var i=0;i<m.length;i++){
        if (sel !== m[i])
          n.push(m[i]);
        else m[i].remove();
      }
      return this.ui.multi = n;
    },
    // }}}
    //addFilter: function(filter){{{
    addFilter: function(filter){
      for(var i=0,m=this.ui.multi,l=m.length; i<l; i++)
        m[i].addFilter(filter);

      return this;
    },
    //}}}
    // removeFiltersByTag: function(tag){{{
    removeFilter: function(filter){
      for(var i=0,m=this.ui.multi,l=m.length; i<l; i++)
        m[i].removeFilter(filter);

      return this;
    },
    // }}}
    // blur: function(){{{
    blur: function(){
      this.ui.selection.blur();
      return this;
    },
    // }}}
    // focus: function(){{{
    focus: function(){
      this.ui.selection.focus();
      return this;
    },
    // }}}
    //initEvents: function(){{{
    initEvents: function(){
      var t = this;
      t.container.on('selectstart',function(e){ return false; })
        .on('mousedown','.'+t.opt.css_drag,t.startDrag());
    },
    //}}}
    // maxSelect: function(){{{
    maxSelect: function(){
      this.setSelect([0,0,this.elw,this.elh]);
    },
    // }}}
    // nudge: function(x,y){{{
    nudge: function(x,y){
      var s = this.ui.selection, b = s.get();

      b.x += x;
      b.x2 += x;
      b.y += y;
      b.y2 += y;

      if (b.x < 0) { b.x2 = b.w; b.x = 0; }
        else if (b.x2 > this.elw) { b.x2 = this.elw; b.x = b.x2 - b.w; }

      if (b.y < 0) { b.y2 = b.h; b.y = 0; }
        else if (b.y2 > this.elh) { b.y2 = this.elh; b.y = b.y2 - b.h; }
      
      s.element.trigger('cropstart',[s,this.unscale(b)]);
      s.updateRaw(b,'move');
      s.element.trigger('cropend',[s,this.unscale(b)]);
    },
    // }}}
    // refresh: function(){{{
    refresh: function(){
      for(var i=0,s=this.ui.multi,l=s.length;i<l;i++)
        s[i].refresh();
    },
    // }}}
    // blurAll: function(){{{
    blurAll: function(){
      var m = this.ui.multi;
      for(var i=0;i<m.length;i++) {
        if (m[i] !== sel) n.push(m[i]);
        m[i].toBack();
      }
    },
    // }}}
    // scale: function(b){{{
    scale: function(b){
      var xs = this.opt.xscale,
          ys = this.opt.yscale;

      return {
        x: b.x / xs,
        y: b.y / ys,
        x2: b.x2 / xs,
        y2: b.y2 / ys,
        w: b.w / xs,
        h: b.h / ys
      };
    },
    // }}}
    // unscale: function(b){{{
    unscale: function(b){
      var xs = this.opt.xscale,
          ys = this.opt.yscale;

      return {
        x: b.x * xs,
        y: b.y * ys,
        x2: b.x2 * xs,
        y2: b.y2 * ys,
        w: b.w * xs,
        h: b.h * ys
      };
    },
    // }}}
    // requestDelete: function(){{{
    requestDelete: function(){
      if ((this.ui.multi.length > 1) && (this.ui.selection.canDelete))
        return this.deleteSelection();
    },
    // }}}
    // deleteSelection: function(){{{
    deleteSelection: function(){
      if (this.ui.selection) {
        this.removeSelection(this.ui.selection);
        if (this.ui.multi.length) this.ui.multi[0].focus();
        this.ui.selection.refresh();
      }
    },
    // }}}
    // animateTo: function(box){{{
    animateTo: function(box){
      if (this.ui.selection)
        this.ui.selection.animateTo(box);
      return this;
    },
    // }}}
    // setselect: function(box){{{
    setSelect: function(box){
      if (this.ui.selection)
        this.ui.selection.update(Jcrop.wrapFromXywh(box));
      return this;
    },
    // }}}
    //startDrag: function(){{{
    startDrag: function(){
      var t = this;
      return function(e){
        var $targ = $(e.target);
        var selection = $targ.closest('.'+t.opt.css_selection).data('selection');
        var ord = $targ.data('ord');
        t.container.trigger('cropstart',[selection,t.unscale(selection.get())]);
        selection.startDrag(e,ord);
        return false;
      };
    },
    //}}}
    // getContainerSize: function(){{{
    getContainerSize: function(){
      return [ this.container.width(), this.container.height() ];
    },
    // }}}
    // resizeContainer: function(w,h){{{
    resizeContainer: function(w,h){
      this.container.width(w).height(h);
      this.refresh();
    },
    // }}}
    // setImage: function(src,cb){{{
    setImage: function(src,cb){
      var t = this, targ = t.opt.imgsrc;

      if (!targ) return false;

      new $.Jcrop.component.ImageLoader(src,null,function(w,h){
        t.resizeContainer(w,h);

        targ.src = src;
        $(targ).width(w).height(h);
        t.applySizeConstraints();
        t.refresh();
        t.container.trigger('cropimage',[t,targ]);

        if (typeof cb == 'function')
          cb.call(t,w,h);
      });
    },
    // }}}
    // update: function(b){{{
    update: function(b){
      if (this.ui.selection)
        this.ui.selection.update(b);
    }
    // }}}
  });

  // Jcrop jQuery plugin function
  $.fn.Jcrop = function(options,callback){
    options = options || {};

    var first = this.eq(0).data('Jcrop');
    var args = Array.prototype.slice.call(arguments);

    // Return API if requested
    if (options == 'api') { return first; }

    // Allow calling API methods (with arguments)
    else if (first && (typeof options == 'string')) {

      // Call method if it exists
      if (first[options]) {
        args.shift();
        first[options].apply(first,args);
        return first;
      }

      // Unknown input/method does not exist
      return false;
    }

    // Otherwise, loop over selected elements
    this.each(function(){
      var t = this, $t = $(this);
      var exists = $t.data('Jcrop');
      var obj;

      // If Jcrop already exists on this element only setOptions()
      if (exists)
        exists.setOptions(options);

      else {

        if (!options.stageConstructor)
          options.stageConstructor = $.Jcrop.stageConstructor;

        options.stageConstructor(this,options,function(stage,options){
          var selection = options.setSelect;
          if (selection) delete(options.setSelect);

          var obj = $.Jcrop.attach(stage.element,options);

          if (typeof stage.attach == 'function')
            stage.attach(obj);

          $t.data('Jcrop',obj);

          if (selection) {
            obj.newSelection();
            obj.setSelect(selection);
          }

          if (typeof callback == 'function')
            callback.call(obj);
        });
      }

      return this;
    });
  };

 /*!
 * modernizr v3.6.0
 * Build https://modernizr.com/download?-canvas-canvastext-csstransforms-datauri-inlinesvg-svg-svgclippaths-domprefixes-hasevent-prefixes-setclasses-testallprops-testprop-teststyles-dontmin
 *
 * Copyright (c)
 *  Faruk Ates
 *  Paul Irish
 *  Alex Sexton
 *  Ryan Seddon
 *  Patrick Kettner
 *  Stu Cox
 *  Richard Herrera

 * MIT License
 */

/*
 * Modernizr tests which native CSS3 and HTML5 features are available in the
 * current UA and makes the results available to you in two ways: as properties on
 * a global `Modernizr` object, and as classes on the `<html>` element. This
 * information allows you to progressively enhance your pages with a granular level
 * of control over the experience.
*/

;(function(window, document, undefined){
  var classes = [];
  

  var tests = [];
  

  /**
   *
   * ModernizrProto is the constructor for Modernizr
   *
   * @class
   * @access public
   */

  var ModernizrProto = {
    // The current version, dummy
    _version: '3.6.0',

    // Any settings that don't work as separate modules
    // can go in here as configuration.
    _config: {
      'classPrefix': '',
      'enableClasses': true,
      'enableJSClass': true,
      'usePrefixes': true
    },

    // Queue of tests
    _q: [],

    // Stub these for people who are listening
    on: function(test, cb) {
      // I don't really think people should do this, but we can
      // safe guard it a bit.
      // -- NOTE:: this gets WAY overridden in src/addTest for actual async tests.
      // This is in case people listen to synchronous tests. I would leave it out,
      // but the code to *disallow* sync tests in the real version of this
      // function is actually larger than this.
      var self = this;
      setTimeout(function() {
        cb(self[test]);
      }, 0);
    },

    addTest: function(name, fn, options) {
      tests.push({name: name, fn: fn, options: options});
    },

    addAsyncTest: function(fn) {
      tests.push({name: null, fn: fn});
    }
  };

  

  // Fake some of Object.create so we can force non test results to be non "own" properties.
  var Modernizr = function() {};
  Modernizr.prototype = ModernizrProto;

  // Leak modernizr globally when you `require` it rather than force it here.
  // Overwrite name so constructor name is nicer :D
  Modernizr = new Modernizr();

  
/*!
{
  "name": "SVG",
  "property": "svg",
  "caniuse": "svg",
  "tags": ["svg"],
  "authors": ["Erik Dahlstrom"],
  "polyfills": [
    "svgweb",
    "raphael",
    "amplesdk",
    "canvg",
    "svg-boilerplate",
    "sie",
    "dojogfx",
    "fabricjs"
  ]
}
!*/
/* DOC
Detects support for SVG in `<embed>` or `<object>` elements.
*/

  Modernizr.addTest('svg', !!document.createElementNS && !!document.createElementNS('http://www.w3.org/2000/svg', 'svg').createSVGRect);


  /**
   * List of property values to set for css tests. See ticket #21
   * http://git.io/vUGl4
   *
   * @memberof Modernizr
   * @name Modernizr._prefixes
   * @optionName Modernizr._prefixes
   * @optionProp prefixes
   * @access public
   * @example
   *
   * Modernizr._prefixes is the internal list of prefixes that we test against
   * inside of things like [prefixed](#modernizr-prefixed) and [prefixedCSS](#-code-modernizr-prefixedcss). It is simply
   * an array of kebab-case vendor prefixes you can use within your code.
   *
   * Some common use cases include
   *
   * Generating all possible prefixed version of a CSS property
   * ```js
   * var rule = Modernizr._prefixes.join('transform: rotate(20deg); ');
   *
   * rule === 'transform: rotate(20deg); webkit-transform: rotate(20deg); moz-transform: rotate(20deg); o-transform: rotate(20deg); ms-transform: rotate(20deg);'
   * ```
   *
   * Generating all possible prefixed version of a CSS value
   * ```js
   * rule = 'display:' +  Modernizr._prefixes.join('flex; display:') + 'flex';
   *
   * rule === 'display:flex; display:-webkit-flex; display:-moz-flex; display:-o-flex; display:-ms-flex; display:flex'
   * ```
   */

  // we use ['',''] rather than an empty array in order to allow a pattern of .`join()`ing prefixes to test
  // values in feature detects to continue to work
  var prefixes = (ModernizrProto._config.usePrefixes ? ' -webkit- -moz- -o- -ms- '.split(' ') : ['','']);

  // expose these for the plugin API. Look in the source for how to join() them against your input
  ModernizrProto._prefixes = prefixes;

  

  /**
   * is returns a boolean if the typeof an obj is exactly type.
   *
   * @access private
   * @function is
   * @param {*} obj - A thing we want to check the type of
   * @param {string} type - A string to compare the typeof against
   * @returns {boolean}
   */

  function is(obj, type) {
    return typeof obj === type;
  }
  ;

  /**
   * Run through all tests and detect their support in the current UA.
   *
   * @access private
   */

  function testRunner() {
    var featureNames;
    var feature;
    var aliasIdx;
    var result;
    var nameIdx;
    var featureName;
    var featureNameSplit;

    for (var featureIdx in tests) {
      if (tests.hasOwnProperty(featureIdx)) {
        featureNames = [];
        feature = tests[featureIdx];
        // run the test, throw the return value into the Modernizr,
        // then based on that boolean, define an appropriate className
        // and push it into an array of classes we'll join later.
        //
        // If there is no name, it's an 'async' test that is run,
        // but not directly added to the object. That should
        // be done with a post-run addTest call.
        if (feature.name) {
          featureNames.push(feature.name.toLowerCase());

          if (feature.options && feature.options.aliases && feature.options.aliases.length) {
            // Add all the aliases into the names list
            for (aliasIdx = 0; aliasIdx < feature.options.aliases.length; aliasIdx++) {
              featureNames.push(feature.options.aliases[aliasIdx].toLowerCase());
            }
          }
        }

        // Run the test, or use the raw value if it's not a function
        result = is(feature.fn, 'function') ? feature.fn() : feature.fn;


        // Set each of the names on the Modernizr object
        for (nameIdx = 0; nameIdx < featureNames.length; nameIdx++) {
          featureName = featureNames[nameIdx];
          // Support dot properties as sub tests. We don't do checking to make sure
          // that the implied parent tests have been added. You must call them in
          // order (either in the test, or make the parent test a dependency).
          //
          // Cap it to TWO to make the logic simple and because who needs that kind of subtesting
          // hashtag famous last words
          featureNameSplit = featureName.split('.');

          if (featureNameSplit.length === 1) {
            Modernizr[featureNameSplit[0]] = result;
          } else {
            // cast to a Boolean, if not one already
            if (Modernizr[featureNameSplit[0]] && !(Modernizr[featureNameSplit[0]] instanceof Boolean)) {
              Modernizr[featureNameSplit[0]] = new Boolean(Modernizr[featureNameSplit[0]]);
            }

            Modernizr[featureNameSplit[0]][featureNameSplit[1]] = result;
          }

          classes.push((result ? '' : 'no-') + featureNameSplit.join('-'));
        }
      }
    }
  }
  ;

  /**
   * docElement is a convenience wrapper to grab the root element of the document
   *
   * @access private
   * @returns {HTMLElement|SVGElement} The root element of the document
   */

  var docElement = document.documentElement;
  

  /**
   * A convenience helper to check if the document we are running in is an SVG document
   *
   * @access private
   * @returns {boolean}
   */

  var isSVG = docElement.nodeName.toLowerCase() === 'svg';
  

  /**
   * setClasses takes an array of class names and adds them to the root element
   *
   * @access private
   * @function setClasses
   * @param {string[]} classes - Array of class names
   */

  // Pass in an and array of class names, e.g.:
  //  ['no-webp', 'borderradius', ...]
  function setClasses(classes) {
    var className = docElement.className;
    var classPrefix = Modernizr._config.classPrefix || '';

    if (isSVG) {
      className = className.baseVal;
    }

    // Change `no-js` to `js` (independently of the `enableClasses` option)
    // Handle classPrefix on this too
    if (Modernizr._config.enableJSClass) {
      var reJS = new RegExp('(^|\\s)' + classPrefix + 'no-js(\\s|$)');
      className = className.replace(reJS, '$1' + classPrefix + 'js$2');
    }

    if (Modernizr._config.enableClasses) {
      // Add the new classes
      className += ' ' + classPrefix + classes.join(' ' + classPrefix);
      if (isSVG) {
        docElement.className.baseVal = className;
      } else {
        docElement.className = className;
      }
    }

  }

  ;

  /**
   * If the browsers follow the spec, then they would expose vendor-specific styles as:
   *   elem.style.WebkitBorderRadius
   * instead of something like the following (which is technically incorrect):
   *   elem.style.webkitBorderRadius

   * WebKit ghosts their properties in lowercase but Opera & Moz do not.
   * Microsoft uses a lowercase `ms` instead of the correct `Ms` in IE8+
   *   erik.eae.net/archives/2008/03/10/21.48.10/

   * More here: github.com/Modernizr/Modernizr/issues/issue/21
   *
   * @access private
   * @returns {string} The string representing the vendor-specific style properties
   */

  var omPrefixes = 'Moz O ms Webkit';
  

  /**
   * List of JavaScript DOM values used for tests
   *
   * @memberof Modernizr
   * @name Modernizr._domPrefixes
   * @optionName Modernizr._domPrefixes
   * @optionProp domPrefixes
   * @access public
   * @example
   *
   * Modernizr._domPrefixes is exactly the same as [_prefixes](#modernizr-_prefixes), but rather
   * than kebab-case properties, all properties are their Capitalized variant
   *
   * ```js
   * Modernizr._domPrefixes === [ "Moz", "O", "ms", "Webkit" ];
   * ```
   */

  var domPrefixes = (ModernizrProto._config.usePrefixes ? omPrefixes.toLowerCase().split(' ') : []);
  ModernizrProto._domPrefixes = domPrefixes;
  

  /**
   * createElement is a convenience wrapper around document.createElement. Since we
   * use createElement all over the place, this allows for (slightly) smaller code
   * as well as abstracting away issues with creating elements in contexts other than
   * HTML documents (e.g. SVG documents).
   *
   * @access private
   * @function createElement
   * @returns {HTMLElement|SVGElement} An HTML or SVG element
   */

  function createElement() {
    if (typeof document.createElement !== 'function') {
      // This is the case in IE7, where the type of createElement is "object".
      // For this reason, we cannot call apply() as Object is not a Function.
      return document.createElement(arguments[0]);
    } else if (isSVG) {
      return document.createElementNS.call(document, 'http://www.w3.org/2000/svg', arguments[0]);
    } else {
      return document.createElement.apply(document, arguments);
    }
  }

  ;

  /**
   * Modernizr.hasEvent() detects support for a given event
   *
   * @memberof Modernizr
   * @name Modernizr.hasEvent
   * @optionName Modernizr.hasEvent()
   * @optionProp hasEvent
   * @access public
   * @function hasEvent
   * @param  {string|*} eventName - the name of an event to test for (e.g. "resize")
   * @param  {Element|string} [element=HTMLDivElement] - is the element|document|window|tagName to test on
   * @returns {boolean}
   * @example
   *  `Modernizr.hasEvent` lets you determine if the browser supports a supplied event.
   *  By default, it does this detection on a div element
   *
   * ```js
   *  hasEvent('blur') // true;
   * ```
   *
   * However, you are able to give an object as a second argument to hasEvent to
   * detect an event on something other than a div.
   *
   * ```js
   *  hasEvent('devicelight', window) // true;
   * ```
   *
   */

  var hasEvent = (function() {

    // Detect whether event support can be detected via `in`. Test on a DOM element
    // using the "blur" event b/c it should always exist. bit.ly/event-detection
    var needsFallback = !('onblur' in document.documentElement);

    function inner(eventName, element) {

      var isSupported;
      if (!eventName) { return false; }
      if (!element || typeof element === 'string') {
        element = createElement(element || 'div');
      }

      // Testing via the `in` operator is sufficient for modern browsers and IE.
      // When using `setAttribute`, IE skips "unload", WebKit skips "unload" and
      // "resize", whereas `in` "catches" those.
      eventName = 'on' + eventName;
      isSupported = eventName in element;

      // Fallback technique for old Firefox - bit.ly/event-detection
      if (!isSupported && needsFallback) {
        if (!element.setAttribute) {
          // Switch to generic element if it lacks `setAttribute`.
          // It could be the `document`, `window`, or something else.
          element = createElement('div');
        }

        element.setAttribute(eventName, '');
        isSupported = typeof element[eventName] === 'function';

        if (element[eventName] !== undefined) {
          // If property was created, "remove it" by setting value to `undefined`.
          element[eventName] = undefined;
        }
        element.removeAttribute(eventName);
      }

      return isSupported;
    }
    return inner;
  })();


  ModernizrProto.hasEvent = hasEvent;
  
/*!
{
  "name": "Canvas",
  "property": "canvas",
  "caniuse": "canvas",
  "tags": ["canvas", "graphics"],
  "polyfills": ["flashcanvas", "excanvas", "slcanvas", "fxcanvas"]
}
!*/
/* DOC
Detects support for the `<canvas>` element for 2D drawing.
*/

  // On the S60 and BB Storm, getContext exists, but always returns undefined
  // so we actually have to call getContext() to verify
  // github.com/Modernizr/Modernizr/issues/issue/97/
  Modernizr.addTest('canvas', function() {
    var elem = createElement('canvas');
    return !!(elem.getContext && elem.getContext('2d'));
  });

/*!
{
  "name": "Canvas text",
  "property": "canvastext",
  "caniuse": "canvas-text",
  "tags": ["canvas", "graphics"],
  "polyfills": ["canvastext"]
}
!*/
/* DOC
Detects support for the text APIs for `<canvas>` elements.
*/

  Modernizr.addTest('canvastext',  function() {
    if (Modernizr.canvas  === false) {
      return false;
    }
    return typeof createElement('canvas').getContext('2d').fillText == 'function';
  });

/*!
{
  "name": "Inline SVG",
  "property": "inlinesvg",
  "caniuse": "svg-html5",
  "tags": ["svg"],
  "notes": [{
    "name": "Test page",
    "href": "https://paulirish.com/demo/inline-svg"
  }, {
    "name": "Test page and results",
    "href": "https://codepen.io/eltonmesquita/full/GgXbvo/"
  }],
  "polyfills": ["inline-svg-polyfill"],
  "knownBugs": ["False negative on some Chromia browsers."]
}
!*/
/* DOC
Detects support for inline SVG in HTML (not within XHTML).
*/

  Modernizr.addTest('inlinesvg', function() {
    var div = createElement('div');
    div.innerHTML = '<svg/>';
    return (typeof SVGRect != 'undefined' && div.firstChild && div.firstChild.namespaceURI) == 'http://www.w3.org/2000/svg';
  });


  /**
   * Object.prototype.toString can be used with every object and allows you to
   * get its class easily. Abstracting it off of an object prevents situations
   * where the toString property has been overridden
   *
   * @access private
   * @function toStringFn
   * @returns {function} An abstracted toString function
   */

  var toStringFn = ({}).toString;
  
/*!
{
  "name": "SVG clip paths",
  "property": "svgclippaths",
  "tags": ["svg"],
  "notes": [{
    "name": "Demo",
    "href": "http://srufaculty.sru.edu/david.dailey/svg/newstuff/clipPath4.svg"
  }]
}
!*/
/* DOC
Detects support for clip paths in SVG (only, not on HTML content).

See [this discussion](https://github.com/Modernizr/Modernizr/issues/213) regarding applying SVG clip paths to HTML content.
*/

  Modernizr.addTest('svgclippaths', function() {
    return !!document.createElementNS &&
      /SVGClipPath/.test(toStringFn.call(document.createElementNS('http://www.w3.org/2000/svg', 'clipPath')));
  });


  var cssomPrefixes = (ModernizrProto._config.usePrefixes ? omPrefixes.split(' ') : []);
  ModernizrProto._cssomPrefixes = cssomPrefixes;
  


  /**
   * contains checks to see if a string contains another string
   *
   * @access private
   * @function contains
   * @param {string} str - The string we want to check for substrings
   * @param {string} substr - The substring we want to search the first string for
   * @returns {boolean}
   */

  function contains(str, substr) {
    return !!~('' + str).indexOf(substr);
  }

  ;

  /**
   * getBody returns the body of a document, or an element that can stand in for
   * the body if a real body does not exist
   *
   * @access private
   * @function getBody
   * @returns {HTMLElement|SVGElement} Returns the real body of a document, or an
   * artificially created element that stands in for the body
   */

  function getBody() {
    // After page load injecting a fake body doesn't work so check if body exists
    var body = document.body;

    if (!body) {
      // Can't use the real body create a fake one.
      body = createElement(isSVG ? 'svg' : 'body');
      body.fake = true;
    }

    return body;
  }

  ;

  /**
   * injectElementWithStyles injects an element with style element and some CSS rules
   *
   * @access private
   * @function injectElementWithStyles
   * @param {string} rule - String representing a css rule
   * @param {function} callback - A function that is used to test the injected element
   * @param {number} [nodes] - An integer representing the number of additional nodes you want injected
   * @param {string[]} [testnames] - An array of strings that are used as ids for the additional nodes
   * @returns {boolean}
   */

  function injectElementWithStyles(rule, callback, nodes, testnames) {
    var mod = 'modernizr';
    var style;
    var ret;
    var node;
    var docOverflow;
    var div = createElement('div');
    var body = getBody();

    if (parseInt(nodes, 10)) {
      // In order not to give false positives we create a node for each test
      // This also allows the method to scale for unspecified uses
      while (nodes--) {
        node = createElement('div');
        node.id = testnames ? testnames[nodes] : mod + (nodes + 1);
        div.appendChild(node);
      }
    }

    style = createElement('style');
    style.type = 'text/css';
    style.id = 's' + mod;

    // IE6 will false positive on some tests due to the style element inside the test div somehow interfering offsetHeight, so insert it into body or fakebody.
    // Opera will act all quirky when injecting elements in documentElement when page is served as xml, needs fakebody too. #270
    (!body.fake ? div : body).appendChild(style);
    body.appendChild(div);

    if (style.styleSheet) {
      style.styleSheet.cssText = rule;
    } else {
      style.appendChild(document.createTextNode(rule));
    }
    div.id = mod;

    if (body.fake) {
      //avoid crashing IE8, if background image is used
      body.style.background = '';
      //Safari 5.13/5.1.4 OSX stops loading if ::-webkit-scrollbar is used and scrollbars are visible
      body.style.overflow = 'hidden';
      docOverflow = docElement.style.overflow;
      docElement.style.overflow = 'hidden';
      docElement.appendChild(body);
    }

    ret = callback(div, rule);
    // If this is done after page load we don't want to remove the body so check if body exists
    if (body.fake) {
      body.parentNode.removeChild(body);
      docElement.style.overflow = docOverflow;
      // Trigger layout so kinetic scrolling isn't disabled in iOS6+
      // eslint-disable-next-line
      docElement.offsetHeight;
    } else {
      div.parentNode.removeChild(div);
    }

    return !!ret;

  }

  ;

  /**
   * testStyles injects an element with style element and some CSS rules
   *
   * @memberof Modernizr
   * @name Modernizr.testStyles
   * @optionName Modernizr.testStyles()
   * @optionProp testStyles
   * @access public
   * @function testStyles
   * @param {string} rule - String representing a css rule
   * @param {function} callback - A function that is used to test the injected element
   * @param {number} [nodes] - An integer representing the number of additional nodes you want injected
   * @param {string[]} [testnames] - An array of strings that are used as ids for the additional nodes
   * @returns {boolean}
   * @example
   *
   * `Modernizr.testStyles` takes a CSS rule and injects it onto the current page
   * along with (possibly multiple) DOM elements. This lets you check for features
   * that can not be detected by simply checking the [IDL](https://developer.mozilla.org/en-US/docs/Mozilla/Developer_guide/Interface_development_guide/IDL_interface_rules).
   *
   * ```js
   * Modernizr.testStyles('#modernizr { width: 9px; color: papayawhip; }', function(elem, rule) {
   *   // elem is the first DOM node in the page (by default #modernizr)
   *   // rule is the first argument you supplied - the CSS rule in string form
   *
   *   addTest('widthworks', elem.style.width === '9px')
   * });
   * ```
   *
   * If your test requires multiple nodes, you can include a third argument
   * indicating how many additional div elements to include on the page. The
   * additional nodes are injected as children of the `elem` that is returned as
   * the first argument to the callback.
   *
   * ```js
   * Modernizr.testStyles('#modernizr {width: 1px}; #modernizr2 {width: 2px}', function(elem) {
   *   document.getElementById('modernizr').style.width === '1px'; // true
   *   document.getElementById('modernizr2').style.width === '2px'; // true
   *   elem.firstChild === document.getElementById('modernizr2'); // true
   * }, 1);
   * ```
   *
   * By default, all of the additional elements have an ID of `modernizr[n]`, where
   * `n` is its index (e.g. the first additional, second overall is `#modernizr2`,
   * the second additional is `#modernizr3`, etc.).
   * If you want to have more meaningful IDs for your function, you can provide
   * them as the fourth argument, as an array of strings
   *
   * ```js
   * Modernizr.testStyles('#foo {width: 10px}; #bar {height: 20px}', function(elem) {
   *   elem.firstChild === document.getElementById('foo'); // true
   *   elem.lastChild === document.getElementById('bar'); // true
   * }, 2, ['foo', 'bar']);
   * ```
   *
   */

  var testStyles = ModernizrProto.testStyles = injectElementWithStyles;
  

  /**
   * cssToDOM takes a kebab-case string and converts it to camelCase
   * e.g. box-sizing -> boxSizing
   *
   * @access private
   * @function cssToDOM
   * @param {string} name - String name of kebab-case prop we want to convert
   * @returns {string} The camelCase version of the supplied name
   */

  function cssToDOM(name) {
    return name.replace(/([a-z])-([a-z])/g, function(str, m1, m2) {
      return m1 + m2.toUpperCase();
    }).replace(/^-/, '');
  }
  ;

  /**
   * hasOwnProp is a shim for hasOwnProperty that is needed for Safari 2.0 support
   *
   * @author kangax
   * @access private
   * @function hasOwnProp
   * @param {object} object - The object to check for a property
   * @param {string} property - The property to check for
   * @returns {boolean}
   */

  // hasOwnProperty shim by kangax needed for Safari 2.0 support
  var hasOwnProp;

  (function() {
    var _hasOwnProperty = ({}).hasOwnProperty;
    /* istanbul ignore else */
    /* we have no way of testing IE 5.5 or safari 2,
     * so just assume the else gets hit */
    if (!is(_hasOwnProperty, 'undefined') && !is(_hasOwnProperty.call, 'undefined')) {
      hasOwnProp = function(object, property) {
        return _hasOwnProperty.call(object, property);
      };
    }
    else {
      hasOwnProp = function(object, property) { /* yes, this can give false positives/negatives, but most of the time we don't care about those */
        return ((property in object) && is(object.constructor.prototype[property], 'undefined'));
      };
    }
  })();

  


   // _l tracks listeners for async tests, as well as tests that execute after the initial run
  ModernizrProto._l = {};

  /**
   * Modernizr.on is a way to listen for the completion of async tests. Being
   * asynchronous, they may not finish before your scripts run. As a result you
   * will get a possibly false negative `undefined` value.
   *
   * @memberof Modernizr
   * @name Modernizr.on
   * @access public
   * @function on
   * @param {string} feature - String name of the feature detect
   * @param {function} cb - Callback function returning a Boolean - true if feature is supported, false if not
   * @example
   *
   * ```js
   * Modernizr.on('flash', function( result ) {
   *   if (result) {
   *    // the browser has flash
   *   } else {
   *     // the browser does not have flash
   *   }
   * });
   * ```
   */

  ModernizrProto.on = function(feature, cb) {
    // Create the list of listeners if it doesn't exist
    if (!this._l[feature]) {
      this._l[feature] = [];
    }

    // Push this test on to the listener list
    this._l[feature].push(cb);

    // If it's already been resolved, trigger it on next tick
    if (Modernizr.hasOwnProperty(feature)) {
      // Next Tick
      setTimeout(function() {
        Modernizr._trigger(feature, Modernizr[feature]);
      }, 0);
    }
  };

  /**
   * _trigger is the private function used to signal test completion and run any
   * callbacks registered through [Modernizr.on](#modernizr-on)
   *
   * @memberof Modernizr
   * @name Modernizr._trigger
   * @access private
   * @function _trigger
   * @param {string} feature - string name of the feature detect
   * @param {function|boolean} [res] - A feature detection function, or the boolean =
   * result of a feature detection function
   */

  ModernizrProto._trigger = function(feature, res) {
    if (!this._l[feature]) {
      return;
    }

    var cbs = this._l[feature];

    // Force async
    setTimeout(function() {
      var i, cb;
      for (i = 0; i < cbs.length; i++) {
        cb = cbs[i];
        cb(res);
      }
    }, 0);

    // Don't trigger these again
    delete this._l[feature];
  };

  /**
   * addTest allows you to define your own feature detects that are not currently
   * included in Modernizr (under the covers it's the exact same code Modernizr
   * uses for its own [feature detections](https://github.com/Modernizr/Modernizr/tree/master/feature-detects)). Just like the offical detects, the result
   * will be added onto the Modernizr object, as well as an appropriate className set on
   * the html element when configured to do so
   *
   * @memberof Modernizr
   * @name Modernizr.addTest
   * @optionName Modernizr.addTest()
   * @optionProp addTest
   * @access public
   * @function addTest
   * @param {string|object} feature - The string name of the feature detect, or an
   * object of feature detect names and test
   * @param {function|boolean} test - Function returning true if feature is supported,
   * false if not. Otherwise a boolean representing the results of a feature detection
   * @example
   *
   * The most common way of creating your own feature detects is by calling
   * `Modernizr.addTest` with a string (preferably just lowercase, without any
   * punctuation), and a function you want executed that will return a boolean result
   *
   * ```js
   * Modernizr.addTest('itsTuesday', function() {
   *  var d = new Date();
   *  return d.getDay() === 2;
   * });
   * ```
   *
   * When the above is run, it will set Modernizr.itstuesday to `true` when it is tuesday,
   * and to `false` every other day of the week. One thing to notice is that the names of
   * feature detect functions are always lowercased when added to the Modernizr object. That
   * means that `Modernizr.itsTuesday` will not exist, but `Modernizr.itstuesday` will.
   *
   *
   *  Since we only look at the returned value from any feature detection function,
   *  you do not need to actually use a function. For simple detections, just passing
   *  in a statement that will return a boolean value works just fine.
   *
   * ```js
   * Modernizr.addTest('hasJquery', 'jQuery' in window);
   * ```
   *
   * Just like before, when the above runs `Modernizr.hasjquery` will be true if
   * jQuery has been included on the page. Not using a function saves a small amount
   * of overhead for the browser, as well as making your code much more readable.
   *
   * Finally, you also have the ability to pass in an object of feature names and
   * their tests. This is handy if you want to add multiple detections in one go.
   * The keys should always be a string, and the value can be either a boolean or
   * function that returns a boolean.
   *
   * ```js
   * var detects = {
   *  'hasjquery': 'jQuery' in window,
   *  'itstuesday': function() {
   *    var d = new Date();
   *    return d.getDay() === 2;
   *  }
   * }
   *
   * Modernizr.addTest(detects);
   * ```
   *
   * There is really no difference between the first methods and this one, it is
   * just a convenience to let you write more readable code.
   */

  function addTest(feature, test) {

    if (typeof feature == 'object') {
      for (var key in feature) {
        if (hasOwnProp(feature, key)) {
          addTest(key, feature[ key ]);
        }
      }
    } else {

      feature = feature.toLowerCase();
      var featureNameSplit = feature.split('.');
      var last = Modernizr[featureNameSplit[0]];

      // Again, we don't check for parent test existence. Get that right, though.
      if (featureNameSplit.length == 2) {
        last = last[featureNameSplit[1]];
      }

      if (typeof last != 'undefined') {
        // we're going to quit if you're trying to overwrite an existing test
        // if we were to allow it, we'd do this:
        //   var re = new RegExp("\\b(no-)?" + feature + "\\b");
        //   docElement.className = docElement.className.replace( re, '' );
        // but, no rly, stuff 'em.
        return Modernizr;
      }

      test = typeof test == 'function' ? test() : test;

      // Set the value (this is the magic, right here).
      if (featureNameSplit.length == 1) {
        Modernizr[featureNameSplit[0]] = test;
      } else {
        // cast to a Boolean, if not one already
        if (Modernizr[featureNameSplit[0]] && !(Modernizr[featureNameSplit[0]] instanceof Boolean)) {
          Modernizr[featureNameSplit[0]] = new Boolean(Modernizr[featureNameSplit[0]]);
        }

        Modernizr[featureNameSplit[0]][featureNameSplit[1]] = test;
      }

      // Set a single class (either `feature` or `no-feature`)
      setClasses([(!!test && test != false ? '' : 'no-') + featureNameSplit.join('-')]);

      // Trigger the event
      Modernizr._trigger(feature, test);
    }

    return Modernizr; // allow chaining.
  }

  // After all the tests are run, add self to the Modernizr prototype
  Modernizr._q.push(function() {
    ModernizrProto.addTest = addTest;
  });

  

/*!
{
  "name": "Data URI",
  "property": "datauri",
  "caniuse": "datauri",
  "tags": ["url"],
  "builderAliases": ["url_data_uri"],
  "async": true,
  "notes": [{
    "name": "Wikipedia article",
    "href": "https://en.wikipedia.org/wiki/Data_URI_scheme"
  }],
  "warnings": ["Support in Internet Explorer 8 is limited to images and linked resources like CSS files, not HTML files"]
}
!*/
/* DOC
Detects support for data URIs. Provides a subproperty to report support for data URIs over 32kb in size:

```javascript
Modernizr.datauri           // true
Modernizr.datauri.over32kb  // false in IE8
```
*/

  // https://github.com/Modernizr/Modernizr/issues/14
  Modernizr.addAsyncTest(function() {
      //datauri is not allowed by CSP, no need to check and throw an error
      setTimeout(function() {
        addTest('datauri', false);
      }, 10);
  });


  /**
   * fnBind is a super small [bind](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Function/bind) polyfill.
   *
   * @access private
   * @function fnBind
   * @param {function} fn - a function you want to change `this` reference to
   * @param {object} that - the `this` you want to call the function with
   * @returns {function} The wrapped version of the supplied function
   */

  function fnBind(fn, that) {
    return function() {
      return fn.apply(that, arguments);
    };
  }

  ;

  /**
   * testDOMProps is a generic DOM property test; if a browser supports
   *   a certain property, it won't return undefined for it.
   *
   * @access private
   * @function testDOMProps
   * @param {array.<string>} props - An array of properties to test for
   * @param {object} obj - An object or Element you want to use to test the parameters again
   * @param {boolean|object} elem - An Element to bind the property lookup again. Use `false` to prevent the check
   * @returns {false|*} returns false if the prop is unsupported, otherwise the value that is supported
   */
  function testDOMProps(props, obj, elem) {
    var item;

    for (var i in props) {
      if (props[i] in obj) {

        // return the property name as a string
        if (elem === false) {
          return props[i];
        }

        item = obj[props[i]];

        // let's bind a function
        if (is(item, 'function')) {
          // bind to obj unless overriden
          return fnBind(item, elem || obj);
        }

        // return the unbound function or obj or value
        return item;
      }
    }
    return false;
  }

  ;

  /**
   * Create our "modernizr" element that we do most feature tests on.
   *
   * @access private
   */

  var modElem = {
    elem: createElement('modernizr')
  };

  // Clean up this element
  Modernizr._q.push(function() {
    delete modElem.elem;
  });

  

  var mStyle = {
    style: modElem.elem.style
  };

  // kill ref for gc, must happen before mod.elem is removed, so we unshift on to
  // the front of the queue.
  Modernizr._q.unshift(function() {
    delete mStyle.style;
  });

  


  /**
   * wrapper around getComputedStyle, to fix issues with Firefox returning null when
   * called inside of a hidden iframe
   *
   * @access private
   * @function computedStyle
   * @param {HTMLElement|SVGElement} - The element we want to find the computed styles of
   * @param {string|null} [pseudoSelector]- An optional pseudo element selector (e.g. :before), of null if none
   * @returns {CSSStyleDeclaration}
   */

  function computedStyle(elem, pseudo, prop) {
    var result;

    if ('getComputedStyle' in window) {
      result = getComputedStyle.call(window, elem, pseudo);
      var console = window.console;

      if (result !== null) {
        if (prop) {
          result = result.getPropertyValue(prop);
        }
      } else {
        if (console) {
          var method = console.error ? 'error' : 'log';
          console[method].call(console, 'getComputedStyle returning null, its possible modernizr test results are inaccurate');
        }
      }
    } else {
      result = !pseudo && elem.currentStyle && elem.currentStyle[prop];
    }

    return result;
  }

  ;

  /**
   * domToCSS takes a camelCase string and converts it to kebab-case
   * e.g. boxSizing -> box-sizing
   *
   * @access private
   * @function domToCSS
   * @param {string} name - String name of camelCase prop we want to convert
   * @returns {string} The kebab-case version of the supplied name
   */

  function domToCSS(name) {
    return name.replace(/([A-Z])/g, function(str, m1) {
      return '-' + m1.toLowerCase();
    }).replace(/^ms-/, '-ms-');
  }
  ;

  /**
   * nativeTestProps allows for us to use native feature detection functionality if available.
   * some prefixed form, or false, in the case of an unsupported rule
   *
   * @access private
   * @function nativeTestProps
   * @param {array} props - An array of property names
   * @param {string} value - A string representing the value we want to check via @supports
   * @returns {boolean|undefined} A boolean when @supports exists, undefined otherwise
   */

  // Accepts a list of property names and a single value
  // Returns `undefined` if native detection not available
  function nativeTestProps(props, value) {
    var i = props.length;
    // Start with the JS API: http://www.w3.org/TR/css3-conditional/#the-css-interface
    if ('CSS' in window && 'supports' in window.CSS) {
      // Try every prefixed variant of the property
      while (i--) {
        if (window.CSS.supports(domToCSS(props[i]), value)) {
          return true;
        }
      }
      return false;
    }
    // Otherwise fall back to at-rule (for Opera 12.x)
    else if ('CSSSupportsRule' in window) {
      // Build a condition string for every prefixed variant
      var conditionText = [];
      while (i--) {
        conditionText.push('(' + domToCSS(props[i]) + ':' + value + ')');
      }
      conditionText = conditionText.join(' or ');
      return injectElementWithStyles('@supports (' + conditionText + ') { #modernizr { position: absolute; } }', function(node) {
        return computedStyle(node, null, 'position') == 'absolute';
      });
    }
    return undefined;
  }
  ;

  // testProps is a generic CSS / DOM property test.

  // In testing support for a given CSS property, it's legit to test:
  //    `elem.style[styleName] !== undefined`
  // If the property is supported it will return an empty string,
  // if unsupported it will return undefined.

  // We'll take advantage of this quick test and skip setting a style
  // on our modernizr element, but instead just testing undefined vs
  // empty string.

  // Property names can be provided in either camelCase or kebab-case.

  function testProps(props, prefixed, value, skipValueTest) {
    skipValueTest = is(skipValueTest, 'undefined') ? false : skipValueTest;

    // Try native detect first
    if (!is(value, 'undefined')) {
      var result = nativeTestProps(props, value);
      if (!is(result, 'undefined')) {
        return result;
      }
    }

    // Otherwise do it properly
    var afterInit, i, propsLength, prop, before;

    // If we don't have a style element, that means we're running async or after
    // the core tests, so we'll need to create our own elements to use

    // inside of an SVG element, in certain browsers, the `style` element is only
    // defined for valid tags. Therefore, if `modernizr` does not have one, we
    // fall back to a less used element and hope for the best.
    // for strict XHTML browsers the hardly used samp element is used
    var elems = ['modernizr', 'tspan', 'samp'];
    while (!mStyle.style && elems.length) {
      afterInit = true;
      mStyle.modElem = createElement(elems.shift());
      mStyle.style = mStyle.modElem.style;
    }

    // Delete the objects if we created them.
    function cleanElems() {
      if (afterInit) {
        delete mStyle.style;
        delete mStyle.modElem;
      }
    }

    propsLength = props.length;
    for (i = 0; i < propsLength; i++) {
      prop = props[i];
      before = mStyle.style[prop];

      if (contains(prop, '-')) {
        prop = cssToDOM(prop);
      }

      if (mStyle.style[prop] !== undefined) {

        // If value to test has been passed in, do a set-and-check test.
        // 0 (integer) is a valid property value, so check that `value` isn't
        // undefined, rather than just checking it's truthy.
        if (!skipValueTest && !is(value, 'undefined')) {

          // Needs a try catch block because of old IE. This is slow, but will
          // be avoided in most cases because `skipValueTest` will be used.
          try {
            mStyle.style[prop] = value;
          } catch (e) {}

          // If the property value has changed, we assume the value used is
          // supported. If `value` is empty string, it'll fail here (because
          // it hasn't changed), which matches how browsers have implemented
          // CSS.supports()
          if (mStyle.style[prop] != before) {
            cleanElems();
            return prefixed == 'pfx' ? prop : true;
          }
        }
        // Otherwise just return true, or the property name if this is a
        // `prefixed()` call
        else {
          cleanElems();
          return prefixed == 'pfx' ? prop : true;
        }
      }
    }
    cleanElems();
    return false;
  }

  ;

  /**
   * testProp() investigates whether a given style property is recognized
   * Property names can be provided in either camelCase or kebab-case.
   *
   * @memberof Modernizr
   * @name Modernizr.testProp
   * @access public
   * @optionName Modernizr.testProp()
   * @optionProp testProp
   * @function testProp
   * @param {string} prop - Name of the CSS property to check
   * @param {string} [value] - Name of the CSS value to check
   * @param {boolean} [useValue] - Whether or not to check the value if @supports isn't supported
   * @returns {boolean}
   * @example
   *
   * Just like [testAllProps](#modernizr-testallprops), only it does not check any vendor prefixed
   * version of the string.
   *
   * Note that the property name must be provided in camelCase (e.g. boxSizing not box-sizing)
   *
   * ```js
   * Modernizr.testProp('pointerEvents')  // true
   * ```
   *
   * You can also provide a value as an optional second argument to check if a
   * specific value is supported
   *
   * ```js
   * Modernizr.testProp('pointerEvents', 'none') // true
   * Modernizr.testProp('pointerEvents', 'penguin') // false
   * ```
   */

  var testProp = ModernizrProto.testProp = function(prop, value, useValue) {
    return testProps([prop], undefined, value, useValue);
  };
  

  /**
   * testPropsAll tests a list of DOM properties we want to check against.
   * We specify literally ALL possible (known and/or likely) properties on
   * the element including the non-vendor prefixed one, for forward-
   * compatibility.
   *
   * @access private
   * @function testPropsAll
   * @param {string} prop - A string of the property to test for
   * @param {string|object} [prefixed] - An object to check the prefixed properties on. Use a string to skip
   * @param {HTMLElement|SVGElement} [elem] - An element used to test the property and value against
   * @param {string} [value] - A string of a css value
   * @param {boolean} [skipValueTest] - An boolean representing if you want to test if value sticks when set
   * @returns {false|string} returns the string version of the property, or false if it is unsupported
   */
  function testPropsAll(prop, prefixed, elem, value, skipValueTest) {

    var ucProp = prop.charAt(0).toUpperCase() + prop.slice(1),
      props = (prop + ' ' + cssomPrefixes.join(ucProp + ' ') + ucProp).split(' ');

    // did they call .prefixed('boxSizing') or are we just testing a prop?
    if (is(prefixed, 'string') || is(prefixed, 'undefined')) {
      return testProps(props, prefixed, value, skipValueTest);

      // otherwise, they called .prefixed('requestAnimationFrame', window[, elem])
    } else {
      props = (prop + ' ' + (domPrefixes).join(ucProp + ' ') + ucProp).split(' ');
      return testDOMProps(props, prefixed, elem);
    }
  }

  // Modernizr.testAllProps() investigates whether a given style property,
  // or any of its vendor-prefixed variants, is recognized
  //
  // Note that the property names must be provided in the camelCase variant.
  // Modernizr.testAllProps('boxSizing')
  ModernizrProto.testAllProps = testPropsAll;

  

  /**
   * testAllProps determines whether a given CSS property is supported in the browser
   *
   * @memberof Modernizr
   * @name Modernizr.testAllProps
   * @optionName Modernizr.testAllProps()
   * @optionProp testAllProps
   * @access public
   * @function testAllProps
   * @param {string} prop - String naming the property to test (either camelCase or kebab-case)
   * @param {string} [value] - String of the value to test
   * @param {boolean} [skipValueTest=false] - Whether to skip testing that the value is supported when using non-native detection
   * @example
   *
   * testAllProps determines whether a given CSS property, in some prefixed form,
   * is supported by the browser.
   *
   * ```js
   * testAllProps('boxSizing')  // true
   * ```
   *
   * It can optionally be given a CSS value in string form to test if a property
   * value is valid
   *
   * ```js
   * testAllProps('display', 'block') // true
   * testAllProps('display', 'penguin') // false
   * ```
   *
   * A boolean can be passed as a third parameter to skip the value check when
   * native detection (@supports) isn't available.
   *
   * ```js
   * testAllProps('shapeOutside', 'content-box', true);
   * ```
   */

  function testAllProps(prop, value, skipValueTest) {
    return testPropsAll(prop, undefined, undefined, value, skipValueTest);
  }
  ModernizrProto.testAllProps = testAllProps;
  
/*!
{
  "name": "CSS Transforms",
  "property": "csstransforms",
  "caniuse": "transforms2d",
  "tags": ["css"]
}
!*/

  Modernizr.addTest('csstransforms', function() {
    // Android < 3.0 is buggy, so we sniff and blacklist
    // http://git.io/hHzL7w
    return navigator.userAgent.indexOf('Android 2.') === -1 &&
           testAllProps('transform', 'scale(1)', true);
  });


  // Run each test
  testRunner();

  // Remove the "no-js" class if it exists
  setClasses(classes);

  delete ModernizrProto.addTest;
  delete ModernizrProto.addAsyncTest;

  // Run the things that are supposed to run after the tests
  for (var i = 0; i < Modernizr._q.length; i++) {
    Modernizr._q[i]();
  }

  // Leak Modernizr namespace
  window.Modernizr = Modernizr;


;

})(window, document);

  // Attach to jQuery object
  $.Jcrop = Jcrop;

  $.Jcrop.supportsCanvas = Modernizr.canvas;
  $.Jcrop.supportsCanvasText = Modernizr.canvastext;
  $.Jcrop.supportsDragAndDrop = Modernizr.draganddrop;
  $.Jcrop.supportsDataURI = Modernizr.datauri;
  $.Jcrop.supportsSVG = Modernizr.svg;
  $.Jcrop.supportsInlineSVG = Modernizr.inlinesvg;
  $.Jcrop.supportsSVGClipPaths = Modernizr.svgclippaths;
  $.Jcrop.supportsCSSTransforms = Modernizr.csstransforms;
  $.Jcrop.supportsTouch = Modernizr.touch;

})(jQuery);
