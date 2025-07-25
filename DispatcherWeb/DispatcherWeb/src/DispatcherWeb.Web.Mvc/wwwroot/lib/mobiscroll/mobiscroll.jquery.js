/* eslint-disable */
(function (global, factory) {
  typeof exports === 'object' && typeof module !== 'undefined' ? factory(exports, require('jquery')) :
  typeof define === 'function' && define.amd ? define(['exports', 'jquery'], factory) :
  (global = typeof globalThis !== 'undefined' ? globalThis : global || self, factory(global.mobiscroll = {}, global.jQuery));
}(this, (function (exports, jQuery) { 'use strict';

  function _interopDefaultLegacy (e) { return e && typeof e === 'object' && 'default' in e ? e : { 'default': e }; }

  var jQuery__default = /*#__PURE__*/_interopDefaultLegacy(jQuery);

  /* eslint-disable */

  /******************************************************************************
  Copyright (c) Microsoft Corporation.

  Permission to use, copy, modify, and/or distribute this software for any
  purpose with or without fee is hereby granted.

  THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
  REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY
  AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
  INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM
  LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR
  OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR
  PERFORMANCE OF THIS SOFTWARE.
  ***************************************************************************** */
  /* global Reflect, Promise */

  var extendStatics = function(d, b) {
      extendStatics = Object.setPrototypeOf ||
          ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
          function (d, b) { for (var p in b) if (Object.prototype.hasOwnProperty.call(b, p)) d[p] = b[p]; };
      return extendStatics(d, b);
  };

  function __extends(d, b) {
      if (typeof b !== "function" && b !== null)
          throw new TypeError("Class extends value " + String(b) + " is not a constructor or null");
      extendStatics(d, b);
      function __() { this.constructor = d; }
      d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
  }

  var __assign = function() {
      __assign = Object.assign || function __assign(t) {
          for (var s, i = 1, n = arguments.length; i < n; i++) {
              s = arguments[i];
              for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p)) t[p] = s[p];
          }
          return t;
      };
      return __assign.apply(this, arguments);
  };

  function __rest(s, e) {
      var t = {};
      for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p) && e.indexOf(p) < 0)
          t[p] = s[p];
      if (s != null && typeof Object.getOwnPropertySymbols === "function")
          for (var i = 0, p = Object.getOwnPropertySymbols(s); i < p.length; i++) {
              if (e.indexOf(p[i]) < 0 && Object.prototype.propertyIsEnumerable.call(s, p[i]))
                  t[p[i]] = s[p[i]];
          }
      return t;
  }

  var Observable = /*#__PURE__*/ (function () {
      function Observable() {
          this.nr = 0;
          this.keys = 1;
          // handler function map
          this.subscribers = {};
      }
      /**
       * Subscribes a function that will be called when the observable changes. It will receive the new value as parameter.
       * NOTE: Don't forget to unsubscribe to prevent memory leaks!
       * @param handler A function that will be called when a new value is provided by the observable
       */
      Observable.prototype.subscribe = function (handler) {
          var key = this.keys++;
          this.subscribers[key] = handler;
          this.nr++;
          return key;
      };
      /**
       * Unsubscribes a handler from the observable
       * @param handler The handler of the function returned by the subscribe method or the function itself
       */
      Observable.prototype.unsubscribe = function (key) {
          this.nr--;
          delete this.subscribers[key];
      };
      /**
       * Notifies the subscribers of the observable of the next value.
       * @param value The next value of the observable
       */
      Observable.prototype.next = function (value) {
          var subscribers = this.subscribers;
          for (var _i = 0, _a = Object.keys(subscribers); _i < _a.length; _i++) {
              var key = _a[_i];
              if (subscribers[key]) {
                  subscribers[key](value);
              }
          }
      };
      return Observable;
  }());

  var os;
  var vers;
  var version = [];
  var touchUi = false;
  var isBrowser = typeof window !== 'undefined';
  var isDarkQuery = isBrowser && window.matchMedia && window.matchMedia('(prefers-color-scheme:dark)');
  var userAgent = isBrowser ? navigator.userAgent : '';
  var platform = isBrowser ? navigator.platform : '';
  var maxTouchPoints = isBrowser ? navigator.maxTouchPoints : 0;
  var device = userAgent && userAgent.match(/Android|iPhone|iPad|iPod|Windows Phone|Windows|MSIE/i);
  var isSafari = userAgent && /Safari/.test(userAgent);
  if (/Android/i.test(device)) {
      os = 'android';
      vers = userAgent.match(/Android\s+([\d.]+)/i);
      touchUi = true;
      if (vers) {
          version = vers[0].replace('Android ', '').split('.');
      }
  }
  else if (/iPhone|iPad|iPod/i.test(device) || /iPhone|iPad|iPod/i.test(platform) || (platform === 'MacIntel' && maxTouchPoints > 1)) {
      // On iPad with iOS 13 desktop site request is automatically enabled in Safari,
      // so 'iPad' is no longer present in the user agent string.
      // In this case we check `navigator.platform` and `navigator.maxTouchPoints`.
      // maxTouchPoints is needed to exclude desktop Mac OS X.
      os = 'ios';
      vers = userAgent.match(/OS\s+([\d_]+)/i);
      touchUi = true;
      if (vers) {
          version = vers[0].replace(/_/g, '.').replace('OS ', '').split('.');
      }
  }
  else if (/Windows Phone/i.test(device)) {
      os = 'wp';
      touchUi = true;
  }
  else if (/Windows|MSIE/i.test(device)) {
      os = 'windows';
  }
  var majorVersion = +version[0];
  var minorVersion = +version[1];

  /** @hidden */
  var options = {};
  /** @hidden */
  var util = {};
  /** @hidden */
  var themes = {};
  /** @hidden */
  var autoDetect = {};
  /** @hidden */
  var globalChanges = new Observable();
  /** @hidden */
  function getAutoTheme() {
      var autoTheme = '';
      var theme = '';
      var firstTheme = '';
      if (os === 'android') {
          theme = 'material';
      }
      else if (os === 'wp' || os === 'windows') {
          theme = 'windows';
      }
      else {
          theme = 'ios';
      }
      for (var key in themes) {
          // Stop at the first custom theme with the OS base theme
          if (themes[key].baseTheme === theme && themes[key].auto !== false && key !== theme + '-dark') {
              autoTheme = key;
              break;
          }
          else if (key === theme) {
              autoTheme = key;
          }
          else if (!firstTheme) {
              firstTheme = key;
          }
      }
      return autoTheme || firstTheme;
  }
  function setOptions(local) {
      for (var _i = 0, _a = Object.keys(local); _i < _a.length; _i++) {
          var k = _a[_i];
          options[k] = local[k];
      }
      globalChanges.next(options);
  }
  /**
   * Creates a custom theme definition object. It inherits the defaults from the specified base theme.
   * @param name Name of the custom theme.
   * @param baseTheme Name of the base theme (ios, material or windows).
   * @param auto Allow to set it as auto theme, if the component has theme: 'auto' set. True, if not set.
   */
  function createCustomTheme(name, baseTheme, auto) {
      var base = themes[baseTheme];
      themes[name] = __assign({}, base, { auto: auto,
          baseTheme: baseTheme });
      autoDetect.theme = getAutoTheme();
  }
  var platform$1 = {
      majorVersion: majorVersion,
      minorVersion: minorVersion,
      name: os,
  };

  // tslint:disable max-line-length
  var arrowDropDown = '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24"><path d="M7 10l5 5 5-5z"/><path d="M0 0h24v24H0z" fill="none"/></svg>';

  // tslint:disable max-line-length
  var arrowDropUp = '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24"><path d="M7 14l5-5 5 5z"/><path d="M0 0h24v24H0z" fill="none"/></svg>';

  // tslint:disable max-line-length
  var chevronLeft = '<svg xmlns="http://www.w3.org/2000/svg" width="36" height="36" viewBox="0 0 36 36"><path d="M23.12 11.12L21 9l-9 9 9 9 2.12-2.12L16.24 18z"/></svg>';

  // tslint:disable max-line-length
  var chevronRight = '<svg xmlns="http://www.w3.org/2000/svg" width="36" height="36" viewBox="0 0 36 36"><path d="M15 9l-2.12 2.12L19.76 18l-6.88 6.88L15 27l9-9z"/></svg>';

  // tslint:disable max-line-length
  var clear = '<svg xmlns="http://www.w3.org/2000/svg" height="24" viewBox="0 0 24 24" width="24"><path d="M0 0h24v24H0z" fill="none"/><path d="M12 2C6.47 2 2 6.47 2 12s4.47 10 10 10 10-4.47 10-10S17.53 2 12 2zm5 13.59L15.59 17 12 13.41 8.41 17 7 15.59 10.59 12 7 8.41 8.41 7 12 10.59 15.59 7 17 8.41 13.41 12 17 15.59z"/></svg>';

  // tslint:disable max-line-length
  var keyboardArrowDown = '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24"><path d="M7.41 8.59L12 13.17l4.59-4.58L18 10l-6 6-6-6 1.41-1.41z"/><path fill="none" d="M0 0h24v24H0V0z"/></svg>';

  // tslint:disable max-line-length
  var keyboardArrowUp = '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24"><path d="M7.41 15.41L12 10.83l4.59 4.58L18 14l-6-6-6 6z"/><path d="M0 0h24v24H0z" fill="none"/></svg>';

  var textFieldOpt = {
      clearIcon: clear,
      dropdownIcon: arrowDropDown,
      inputStyle: 'box',
      labelStyle: 'floating',
      notch: true,
      ripple: true,
  };
  var themeName = 'material';
  themes[themeName] = {
      Button: {
          ripple: true,
      },
      Calendar: {
          downIcon: arrowDropDown,
          nextIconH: chevronRight,
          nextIconV: keyboardArrowDown,
          prevIconH: chevronLeft,
          prevIconV: keyboardArrowUp,
          upIcon: arrowDropUp,
      },
      Datepicker: {
          clearIcon: clear,
          display: 'center',
      },
      Dropdown: textFieldOpt,
      Eventcalendar: {
          chevronIconDown: keyboardArrowDown,
          colorEventList: true,
          downIcon: arrowDropDown,
          nextIconH: chevronRight,
          nextIconV: keyboardArrowDown,
          prevIconH: chevronLeft,
          prevIconV: keyboardArrowUp,
          upIcon: arrowDropUp,
      },
      Input: textFieldOpt,
      ListItem: {
          ripple: true,
      },
      Scroller: {
          display: 'center',
          rows: 3,
      },
      Select: {
          clearIcon: clear,
          rows: 3,
      },
      Textarea: textFieldOpt,
  };
  createCustomTheme('material-dark', themeName);

  autoDetect.theme = getAutoTheme();

  var UNDEFINED = undefined;
  var ARRAY3 = getArray(3);
  var ARRAY4 = getArray(4);
  var ARRAY7 = getArray(7);
  getArray(24);
  /**
   * Constrains the value to be between min and max.
   * @hidden
   * @param val   Tha value to constrain.
   * @param min   Min value.
   * @param max   Max value.
   * @return      The constrained value.
   */
  function constrain(val, min, max) {
      return Math.max(min, Math.min(val, max));
  }
  /** @hidden */
  function isArray(obj) {
      return Array.isArray(obj);
  }
  /** @hidden */
  function isNumeric(a) {
      return a - parseFloat(a) >= 0;
  }
  /** @hidden */
  function isNumber(a) {
      return typeof a === 'number';
  }
  /** @hidden */
  function isString(s) {
      return typeof s === 'string';
  }
  /** @hidden */
  function isEmpty(v) {
      return v === UNDEFINED || v === null || v === '';
  }
  /** @hidden */
  function isUndefined(v) {
      return typeof v === 'undefined';
  }
  /** @hidden */
  function isObject(v) {
      return typeof v === 'object';
  }
  /**
   * Returns an array with the specified length.
   * @hidden
   * @param nr Length of the array to create.
   * @return Array with the specified length.
   */
  function getArray(nr) {
      return Array.apply(0, Array(Math.max(0, nr)));
  }
  /** @hidden */
  function addPixel(value) {
      return value !== UNDEFINED ? value + (isNumeric(value) ? 'px' : '') : '';
  }
  /** @hidden */
  function noop() {
      return;
  }
  /** @hidden */
  function pad(num, size) {
      if (size === void 0) { size = 2; }
      var str = num + '';
      while (str.length < size) {
          str = '0' + str;
      }
      return str;
  }
  /** @hidden */
  function round(nr) {
      return Math.round(nr);
  }
  /** @hidden */
  function step(value, st) {
      // return Math.min(max, floor(value / st) * st + min);
      return floor(value / st) * st;
  }
  /** @hidden */
  function floor(nr) {
      return Math.floor(nr);
  }
  /** @hidden */
  function throttle(fn, threshhold) {
      if (threshhold === void 0) { threshhold = 100; }
      var last;
      var timer;
      return function () {
          var args = [];
          for (var _i = 0; _i < arguments.length; _i++) {
              args[_i] = arguments[_i];
          }
          var now = +new Date();
          if (last && now < last + threshhold) {
              clearTimeout(timer);
              timer = setTimeout(function () {
                  last = now;
                  fn.apply(void 0, args);
              }, threshhold);
          }
          else {
              last = now;
              fn.apply(void 0, args);
          }
      };
  }
  /** @hidden */
  function debounce(fn, threshhold) {
      if (threshhold === void 0) { threshhold = 100; }
      var timer;
      return function () {
          var args = [];
          for (var _i = 0; _i < arguments.length; _i++) {
              args[_i] = arguments[_i];
          }
          clearTimeout(timer);
          timer = setTimeout(function () {
              fn.apply(void 0, args);
          }, threshhold);
      };
  }
  /**
   * Like setTimeout, but only for Angular, otherwise calls the function instantly.
   * @param inst The component instance.
   * @param cb The callback function.
   */
  function ngSetTimeout(inst, cb) {
      if (inst._cdr) {
          // It's an Angular component
          setTimeout(cb);
      }
      else {
          cb();
      }
  }
  /**
   * Returns the value of the first element in the array that satisfies the testing function.
   * If no values satisfy the testing function, undefined is returned.
   * @hidden
   * @param arr The array to search.
   * @param fn Function to execute on each value in the array.
   */
  function find(arr, fn) {
      return findItemOrIndex(arr, fn);
  }
  /**
   * Returns the index of the first element in the array that satisfies the testing function.
   * If no values satisfy the testing function, -1 is returned.
   * @hidden
   * @param arr The array to search.
   * @param fn Function to execute on each value in the array.
   */
  function findIndex(arr, fn) {
      return findItemOrIndex(arr, fn, true);
  }
  function findItemOrIndex(arr, fn, index) {
      var len = arr.length;
      for (var i = 0; i < len; i++) {
          var item = arr[i];
          if (fn(item, i)) {
              return index ? i : item;
          }
      }
      return index ? -1 : UNDEFINED;
  }
  /**
   * Converts map to array.
   */
  function toArray(m) {
      var arr = [];
      if (m) {
          for (var _i = 0, _a = Object.keys(m); _i < _a.length; _i++) {
              var key = _a[_i];
              arr.push(m[key]);
          }
      }
      return arr;
  }

  // tslint:disable no-non-null-assertion
  /**
   * Generic DOM functions.
   */
  var doc = isBrowser ? document : UNDEFINED;
  var win = isBrowser ? window : UNDEFINED;
  var prefixes = ['Webkit', 'Moz'];
  var elem = doc && doc.createElement('div').style;
  var canvas = doc && doc.createElement('canvas');
  var ctx = canvas && canvas.getContext && canvas.getContext('2d', { willReadFrequently: true });
  var css = win && win.CSS;
  var cssSupports = css && css.supports;
  var textColors = {};
  var raf = (win && win.requestAnimationFrame) ||
      (function (func) {
          return setTimeout(func, 20);
      });
  var rafc = (win && win.cancelAnimationFrame) ||
      (function (id) {
          clearTimeout(id);
      });
  var hasAnimation = elem && elem.animationName !== UNDEFINED;
  // UIWebView on iOS still has the ghost click,
  // WkWebView does not have a ghost click, but it's hard to tell if it's UIWebView or WkWebView
  // In addition in iOS 12.2 if we enable tap handling, it brakes the form inputs
  // (keyboard appears, but the cursor is not in the input).
  var isWebView = os === 'ios' && !isSafari;
  var isWkWebView = isWebView && win && win.webkit && win.webkit.messageHandlers;
  var hasGhostClick = (elem && elem.touchAction === UNDEFINED) || (isWebView && !isWkWebView);
  var jsPrefix = getPrefix();
  var cssPrefix = jsPrefix ? '-' + jsPrefix.toLowerCase() + '-' : '';
  cssSupports && cssSupports('(transform-style: preserve-3d)');
  var hasSticky = cssSupports && (cssSupports('position', 'sticky') || cssSupports('position', '-webkit-sticky'));
  /** @hidden */
  function getPrefix() {
      if (!elem || elem.transform !== UNDEFINED) {
          return '';
      }
      for (var _i = 0, prefixes_1 = prefixes; _i < prefixes_1.length; _i++) {
          var prefix = prefixes_1[_i];
          if (elem[prefix + 'Transform'] !== UNDEFINED) {
              return prefix;
          }
      }
      return '';
  }
  /**
   * @hidden
   * @param el
   * @param event
   * @param handler
   */
  function listen(el, event, handler, opt) {
      if (el) {
          el.addEventListener(event, handler, opt);
      }
  }
  /**
   * @hidden
   * @param el
   * @param event
   * @param handler
   */
  function unlisten(el, event, handler, opt) {
      if (el) {
          el.removeEventListener(event, handler, opt);
      }
  }
  /**
   * @hidden
   * @param el
   */
  function getDocument(el) {
      if (!isBrowser) {
          return UNDEFINED;
      }
      return el && el.ownerDocument ? el.ownerDocument : doc;
  }
  function getDimension(el, property) {
      return parseFloat(getComputedStyle(el)[property] || '0');
  }
  function getScrollLeft(el) {
      return el.scrollLeft !== UNDEFINED ? el.scrollLeft : el.pageXOffset;
  }
  function getScrollTop(el) {
      return el.scrollTop !== UNDEFINED ? el.scrollTop : el.pageYOffset;
  }
  function setScrollLeft(el, val) {
      if (el.scrollTo) {
          el.scrollTo(val, el.scrollY);
      }
      else {
          el.scrollLeft = val;
      }
  }
  function setScrollTop(el, val) {
      if (el.scrollTo) {
          el.scrollTo(el.scrollX, val);
      }
      else {
          el.scrollTop = val;
      }
  }
  /**
   * @hidden
   * @param el
   */
  function getWindow(el) {
      if (!isBrowser) {
          return UNDEFINED;
      }
      return el && el.ownerDocument && el.ownerDocument.defaultView ? el.ownerDocument.defaultView : win;
  }
  /**
   * @hidden
   * @param el
   * @param vertical
   */
  function getPosition(el, vertical) {
      var style = getComputedStyle(el);
      var transform = jsPrefix ? style[jsPrefix + 'Transform'] : style.transform;
      var matrix = transform.split(')')[0].split(', ');
      var px = vertical ? matrix[13] || matrix[5] : matrix[12] || matrix[4];
      return +px || 0;
  }
  /**
   * Calculates the text color to be used with a given color (black or white)
   * @hidden
   * @param color
   */
  function getTextColor(color) {
      if (!ctx || !color) {
          return '#000';
      }
      // Cache calculated text colors, because it is slow
      if (textColors[color]) {
          return textColors[color];
      }
      // Use canvas element, since it does not require DOM append
      ctx.fillStyle = color;
      ctx.fillRect(0, 0, 1, 1);
      var img = ctx.getImageData(0, 0, 1, 1);
      var rgb = img ? img.data : [0, 0, 0];
      var delta = +rgb[0] * 0.299 + +rgb[1] * 0.587 + +rgb[2] * 0.114;
      var textColor = delta < 130 ? '#fff' : '#000';
      textColors[color] = textColor;
      return textColor;
  }
  /** @hidden */
  function scrollStep(elm, startTime, fromX, fromY, toX, toY, callback) {
      var elapsed = Math.min(1, (+new Date() - startTime) / 468);
      var eased = 0.5 * (1 - Math.cos(Math.PI * elapsed));
      var currentX;
      var currentY;
      if (toX !== UNDEFINED) {
          currentX = round(fromX + (toX - fromX) * eased);
          elm.scrollLeft = currentX;
      }
      if (toY !== UNDEFINED) {
          currentY = round(fromY + (toY - fromY) * eased);
          elm.scrollTop = currentY;
      }
      if (currentX !== toX || currentY !== toY) {
          raf(function () {
              scrollStep(elm, startTime, fromX, fromY, toX, toY, callback);
          });
      }
      else if (callback) {
          callback();
      }
  }
  /**
   * Scrolls a container to the given position
   * @hidden
   * @param elm Element to scroll
   * @param toX Position to scroll horizontally to
   * @param toY Position to scroll vertically to
   * @param animate If true, scroll will be animated
   * @param rtl Rtl
   * @param callback Callback when scroll position is reached
   */
  function smoothScroll(elm, toX, toY, animate, rtl, callback) {
      if (toX !== UNDEFINED) {
          toX = Math.max(0, round(toX));
      }
      if (toY !== UNDEFINED) {
          toY = Math.max(0, round(toY));
      }
      if (rtl && toX !== UNDEFINED) {
          toX = -toX;
      }
      if (animate) {
          scrollStep(elm, +new Date(), elm.scrollLeft, elm.scrollTop, toX, toY, callback);
      }
      else {
          if (toX !== UNDEFINED) {
              elm.scrollLeft = toX;
          }
          if (toY !== UNDEFINED) {
              elm.scrollTop = toY;
          }
          if (callback) {
              callback();
          }
      }
  }
  /**
   * Convert html text to plain text
   * @hidden
   * @param htmlString
   */
  function htmlToText(htmlString) {
      if (doc && htmlString) {
          var tempElm = doc.createElement('div');
          tempElm.innerHTML = htmlString;
          return tempElm.textContent.trim();
      }
      return htmlString || '';
  }
  /**
   * Gets the offset of a HTML element relative to the window
   * @param el The HTML element
   */
  function getOffset(el) {
      var bRect = el.getBoundingClientRect();
      var ret = {
          left: bRect.left,
          top: bRect.top,
      };
      var window = getWindow(el);
      if (window !== UNDEFINED) {
          ret.top += getScrollTop(window);
          ret.left += getScrollLeft(window);
      }
      return ret;
  }
  /**
   * Checks if an HTML element matches the given selector
   * @param elm
   * @param selector
   */
  function matches(elm, selector) {
      // IE11 only supports msMatchesSelector
      var matchesSelector = elm && (elm.matches || elm.msMatchesSelector);
      return matchesSelector && matchesSelector.call(elm, selector);
  }
  /**
   * Returns the closest parent element matching the selector
   * @param elm The starting element
   * @param selector The selector string
   * @param context Only look within the context element
   */
  function closest(elm, selector, context) {
      while (elm && !matches(elm, selector)) {
          if (elm === context || elm.nodeType === elm.DOCUMENT_NODE) {
              return null;
          }
          elm = elm.parentNode;
      }
      return elm;
  }
  function forEach(items, func) {
      for (var i = 0; i < items.length; i++) {
          func(items[i], i);
      }
  }

  var EMPTY_OBJ = {};
  var EMPTY_ARR = [];
  var IS_NON_DIMENSIONAL = /acit|ex(?:s|g|n|p|$)|rph|grid|ows|mnc|ntw|ine[ch]|zoo|^ord|itera/i;

  /**
   * Assign properties from `props` to `obj`
   * @template O, P The obj and props types
   * @param {O} obj The object to copy properties to
   * @param {P} props The object to copy properties from
   * @returns {O & P}
   */
  function assign(obj, props) {
    // @ts-ignore We change the type of `obj` to be `O & P`
    for (var i in props) {
      obj[i] = props[i];
    }

    return (
      /** @type {O & P} */
      obj
    );
  }
  /**
   * Remove a child node from its parent if attached. This is a workaround for
   * IE11 which doesn't support `Element.prototype.remove()`. Using this function
   * is smaller than including a dedicated polyfill.
   * @param {Node} node The node to remove
   */

  function removeNode(node) {
    var parentNode = node.parentNode;
    if (parentNode) parentNode.removeChild(node);
  }

  /**
   * Find the closest error boundary to a thrown error and call it
   * @param {object} error The thrown value
   * @param {import('../internal').VNode} vnode The vnode that threw
   * the error that was caught (except for unmounting when this parameter
   * is the highest parent that was being unmounted)
   */
  function _catchError(error, vnode) {
    /** @type {import('../internal').Component} */
    var component, ctor, handled;

    for (; vnode = vnode._parent;) {
      if ((component = vnode._component) && !component._processingException) {
        try {
          ctor = component.constructor;

          if (ctor && ctor.getDerivedStateFromError != null) {
            component.setState(ctor.getDerivedStateFromError(error));
            handled = component._dirty;
          }

          if (component.componentDidCatch != null) {
            component.componentDidCatch(error);
            handled = component._dirty;
          } // This is an error boundary. Mark it as having bailed out, and whether it was mid-hydration.


          if (handled) {
            return component._pendingError = component;
          }
        } catch (e) {
          error = e;
        }
      }
    }

    throw error;
  }

  /**
   * The `option` object can potentially contain callback functions
   * that are called during various stages of our renderer. This is the
   * foundation on which all our addons like `preact/debug`, `preact/compat`,
   * and `preact/hooks` are based on. See the `Options` type in `internal.d.ts`
   * for a full list of available option hooks (most editors/IDEs allow you to
   * ctrl+click or cmd+click on mac the type definition below).
   * @type {import('./internal').Options}
   */

  var options$1 = {
    _catchError: _catchError,
    _vnodeId: 0
  };

  /**
   * Create an virtual node (used for JSX)
   * @param {import('./internal').VNode["type"]} type The node name or Component
   * constructor for this virtual node
   * @param {object | null | undefined} [props] The properties of the virtual node
   * @param {Array<import('.').ComponentChildren>} [children] The children of the virtual node
   * @returns {import('./internal').VNode}
   */

  function createElement(type, props, children) {
    var normalizedProps = {},
        key,
        ref,
        i;

    for (i in props) {
      if (i == 'key') key = props[i];else if (i == 'ref') ref = props[i];else normalizedProps[i] = props[i];
    }

    if (arguments.length > 3) {
      children = [children]; // https://github.com/preactjs/preact/issues/1916

      for (i = 3; i < arguments.length; i++) {
        children.push(arguments[i]);
      }
    }

    if (children != null) {
      normalizedProps.children = children;
    } // If a Component VNode, check for and apply defaultProps
    // Note: type may be undefined in development, must never error here.


    if (typeof type == 'function' && type.defaultProps != null) {
      for (i in type.defaultProps) {
        if (normalizedProps[i] === undefined) {
          normalizedProps[i] = type.defaultProps[i];
        }
      }
    }

    return createVNode(type, normalizedProps, key, ref, null);
  }
  /**
   * Create a VNode (used internally by Preact)
   * @param {import('./internal').VNode["type"]} type The node name or Component
   * Constructor for this virtual node
   * @param {object | string | number | null} props The properties of this virtual node.
   * If this virtual node represents a text node, this is the text of the node (string or number).
   * @param {string | number | null} key The key for this virtual node, used when
   * diffing it against its children
   * @param {import('./internal').VNode["ref"]} ref The ref property that will
   * receive a reference to its created child
   * @returns {import('./internal').VNode}
   */

  function createVNode(type, props, key, ref, original) {
    // V8 seems to be better at detecting type shapes if the object is allocated from the same call site
    // Do not inline into createElement and coerceToVNode!
    var vnode = {
      type: type,
      props: props,
      key: key,
      ref: ref,
      _children: null,
      _parent: null,
      _depth: 0,
      _dom: null,
      // _nextDom must be initialized to undefined b/c it will eventually
      // be set to dom.nextSibling which can return `null` and it is important
      // to be able to distinguish between an uninitialized _nextDom and
      // a _nextDom that has been set to `null`
      _nextDom: undefined,
      _component: null,
      _hydrating: null,
      constructor: undefined,
      _original: original == null ? ++options$1._vnodeId : original
    };
    if (options$1.vnode != null) options$1.vnode(vnode);
    return vnode;
  }
  function Fragment(props) {
    return props.children;
  }

  /**
   * Base Component class. Provides `setState()` and `forceUpdate()`, which
   * trigger rendering
   * @param {object} props The initial component props
   * @param {object} context The initial context from parent components'
   * getChildContext
   */

  function Component(props, context) {
    this.props = props;
    this.context = context;
  }
  /**
   * Update component state and schedule a re-render.
   * @this {import('./internal').Component}
   * @param {object | ((s: object, p: object) => object)} update A hash of state
   * properties to update with new values or a function that given the current
   * state and props returns a new partial state
   * @param {() => void} [callback] A function to be called once component state is
   * updated
   */

  Component.prototype.setState = function (update, callback) {
    // only clone state when copying to nextState the first time.
    var s;

    if (this._nextState != null && this._nextState !== this.state) {
      s = this._nextState;
    } else {
      s = this._nextState = assign({}, this.state);
    }

    if (typeof update == 'function') {
      // Some libraries like `immer` mark the current state as readonly,
      // preventing us from mutating it, so we need to clone it. See #2716
      update = update(assign({}, s), this.props);
    }

    if (update) {
      assign(s, update);
    } // Skip update if updater function returned null


    if (update == null) return;

    if (this._vnode) {
      if (callback) this._renderCallbacks.push(callback);
      enqueueRender(this);
    }
  };
  /**
   * Immediately perform a synchronous re-render of the component
   * @this {import('./internal').Component}
   * @param {() => void} [callback] A function to be called after component is
   * re-rendered
   */


  Component.prototype.forceUpdate = function (callback) {
    if (this._vnode) {
      // Set render mode so that we can differentiate where the render request
      // is coming from. We need this because forceUpdate should never call
      // shouldComponentUpdate
      this._force = true;
      if (callback) this._renderCallbacks.push(callback);
      enqueueRender(this);
    }
  };
  /**
   * Accepts `props` and `state`, and returns a new Virtual DOM tree to build.
   * Virtual DOM is generally constructed via [JSX](http://jasonformat.com/wtf-is-jsx).
   * @param {object} props Props (eg: JSX attributes) received from parent
   * element/component
   * @param {object} state The component's current state
   * @param {object} context Context object, as returned by the nearest
   * ancestor's `getChildContext()`
   * @returns {import('./index').ComponentChildren | void}
   */


  Component.prototype.render = Fragment;
  /**
   * @param {import('./internal').VNode} vnode
   * @param {number | null} [childIndex]
   */

  function getDomSibling(vnode, childIndex) {
    if (childIndex == null) {
      // Use childIndex==null as a signal to resume the search from the vnode's sibling
      return vnode._parent ? getDomSibling(vnode._parent, vnode._parent._children.indexOf(vnode) + 1) : null;
    }

    var sibling;

    for (; childIndex < vnode._children.length; childIndex++) {
      sibling = vnode._children[childIndex];

      if (sibling != null && sibling._dom != null) {
        // Since updateParentDomPointers keeps _dom pointer correct,
        // we can rely on _dom to tell us if this subtree contains a
        // rendered DOM node, and what the first rendered DOM node is
        return sibling._dom;
      }
    } // If we get here, we have not found a DOM node in this vnode's children.
    // We must resume from this vnode's sibling (in it's parent _children array)
    // Only climb up and search the parent if we aren't searching through a DOM
    // VNode (meaning we reached the DOM parent of the original vnode that began
    // the search)


    return typeof vnode.type == 'function' ? getDomSibling(vnode) : null;
  }
  /**
   * Trigger in-place re-rendering of a component.
   * @param {import('./internal').Component} component The component to rerender
   */

  function renderComponent(component) {
    var vnode = component._vnode,
        oldDom = vnode._dom,
        parentDom = component._parentDom;

    if (parentDom) {
      var commitQueue = [];
      var oldVNode = assign({}, vnode);
      oldVNode._original = vnode._original + 1;
      diff(parentDom, vnode, oldVNode, component._globalContext, parentDom.ownerSVGElement !== undefined, vnode._hydrating != null ? [oldDom] : null, commitQueue, oldDom == null ? getDomSibling(vnode) : oldDom, vnode._hydrating);
      commitRoot(commitQueue, vnode);

      if (vnode._dom != oldDom) {
        updateParentDomPointers(vnode);
      }
    }
  }
  /**
   * @param {import('./internal').VNode} vnode
   */


  function updateParentDomPointers(vnode) {
    if ((vnode = vnode._parent) != null && vnode._component != null) {
      vnode._dom = vnode._component.base = null;

      for (var i = 0; i < vnode._children.length; i++) {
        var child = vnode._children[i];

        if (child != null && child._dom != null) {
          vnode._dom = vnode._component.base = child._dom;
          break;
        }
      }

      return updateParentDomPointers(vnode);
    }
  }
  /**
   * The render queue
   * @type {Array<import('./internal').Component>}
   */


  var rerenderQueue = [];
  /**
   * Asynchronously schedule a callback
   * @type {(cb: () => void) => void}
   */

  /* istanbul ignore next */
  // Note the following line isn't tree-shaken by rollup cuz of rollup/rollup#2566

  var defer = typeof Promise == 'function' ? Promise.prototype.then.bind(Promise.resolve()) : setTimeout;
  /*
   * The value of `Component.debounce` must asynchronously invoke the passed in callback. It is
   * important that contributors to Preact can consistently reason about what calls to `setState`, etc.
   * do, and when their effects will be applied. See the links below for some further reading on designing
   * asynchronous APIs.
   * * [Designing APIs for Asynchrony](https://blog.izs.me/2013/08/designing-apis-for-asynchrony)
   * * [Callbacks synchronous and asynchronous](https://blog.ometer.com/2011/07/24/callbacks-synchronous-and-asynchronous/)
   */

  var prevDebounce;
  /**
   * Enqueue a rerender of a component
   * @param {import('./internal').Component} c The component to rerender
   */

  function enqueueRender(c) {
    if (!c._dirty && (c._dirty = true) && rerenderQueue.push(c) && !process._rerenderCount++ || prevDebounce !== options$1.debounceRendering) {
      prevDebounce = options$1.debounceRendering;
      (prevDebounce || defer)(process);
    }
  }
  /** Flush the render queue by rerendering all queued components */

  function process() {
    var queue;

    while (process._rerenderCount = rerenderQueue.length) {
      queue = rerenderQueue.sort(function (a, b) {
        return a._vnode._depth - b._vnode._depth;
      });
      rerenderQueue = []; // Don't update `renderCount` yet. Keep its value non-zero to prevent unnecessary
      // process() calls from getting scheduled while `queue` is still being consumed.

      queue.some(function (c) {
        if (c._dirty) renderComponent(c);
      });
    }
  }

  process._rerenderCount = 0;

  /**
   * Diff the children of a virtual node
   * @param {import('../internal').PreactElement} parentDom The DOM element whose
   * children are being diffed
   * @param {import('../internal').ComponentChildren[]} renderResult
   * @param {import('../internal').VNode} newParentVNode The new virtual
   * node whose children should be diff'ed against oldParentVNode
   * @param {import('../internal').VNode} oldParentVNode The old virtual
   * node whose children should be diff'ed against newParentVNode
   * @param {object} globalContext The current context object - modified by getChildContext
   * @param {boolean} isSvg Whether or not this DOM node is an SVG node
   * @param {Array<import('../internal').PreactElement>} excessDomChildren
   * @param {Array<import('../internal').Component>} commitQueue List of components
   * which have callbacks to invoke in commitRoot
   * @param {import('../internal').PreactElement} oldDom The current attached DOM
   * element any new dom elements should be placed around. Likely `null` on first
   * render (except when hydrating). Can be a sibling DOM element when diffing
   * Fragments that have siblings. In most cases, it starts out as `oldChildren[0]._dom`.
   * @param {boolean} isHydrating Whether or not we are in hydration
   */

  function diffChildren(parentDom, renderResult, newParentVNode, oldParentVNode, globalContext, isSvg, excessDomChildren, commitQueue, oldDom, isHydrating) {
    var i, j, oldVNode, childVNode, newDom, firstChildDom, refs; // This is a compression of oldParentVNode!=null && oldParentVNode != EMPTY_OBJ && oldParentVNode._children || EMPTY_ARR
    // as EMPTY_OBJ._children should be `undefined`.

    var oldChildren = oldParentVNode && oldParentVNode._children || EMPTY_ARR;
    var oldChildrenLength = oldChildren.length;
    newParentVNode._children = [];

    for (i = 0; i < renderResult.length; i++) {
      childVNode = renderResult[i];

      if (childVNode == null || typeof childVNode == 'boolean') {
        childVNode = newParentVNode._children[i] = null;
      } // If this newVNode is being reused (e.g. <div>{reuse}{reuse}</div>) in the same diff,
      // or we are rendering a component (e.g. setState) copy the oldVNodes so it can have
      // it's own DOM & etc. pointers
      else if (typeof childVNode == 'string' || typeof childVNode == 'number' || // eslint-disable-next-line valid-typeof
      typeof childVNode == 'bigint') {
        childVNode = newParentVNode._children[i] = createVNode(null, childVNode, null, null, childVNode);
      } else if (Array.isArray(childVNode)) {
        childVNode = newParentVNode._children[i] = createVNode(Fragment, {
          children: childVNode
        }, null, null, null);
      } else if (childVNode._depth > 0) {
        // VNode is already in use, clone it. This can happen in the following
        // scenario:
        //   const reuse = <div />
        //   <div>{reuse}<span />{reuse}</div>
        childVNode = newParentVNode._children[i] = createVNode(childVNode.type, childVNode.props, childVNode.key, null, childVNode._original);
      } else {
        childVNode = newParentVNode._children[i] = childVNode;
      } // Terser removes the `continue` here and wraps the loop body
      // in a `if (childVNode) { ... } condition


      if (childVNode == null) {
        continue;
      }

      childVNode._parent = newParentVNode;
      childVNode._depth = newParentVNode._depth + 1; // Check if we find a corresponding element in oldChildren.
      // If found, delete the array item by setting to `undefined`.
      // We use `undefined`, as `null` is reserved for empty placeholders
      // (holes).

      oldVNode = oldChildren[i];

      if (oldVNode === null || oldVNode && childVNode.key == oldVNode.key && childVNode.type === oldVNode.type) {
        oldChildren[i] = undefined;
      } else {
        // Either oldVNode === undefined or oldChildrenLength > 0,
        // so after this loop oldVNode == null or oldVNode is a valid value.
        for (j = 0; j < oldChildrenLength; j++) {
          oldVNode = oldChildren[j]; // If childVNode is unkeyed, we only match similarly unkeyed nodes, otherwise we match by key.
          // We always match by type (in either case).

          if (oldVNode && childVNode.key == oldVNode.key && childVNode.type === oldVNode.type) {
            oldChildren[j] = undefined;
            break;
          }

          oldVNode = null;
        }
      }

      oldVNode = oldVNode || EMPTY_OBJ; // Morph the old element into the new one, but don't append it to the dom yet

      diff(parentDom, childVNode, oldVNode, globalContext, isSvg, excessDomChildren, commitQueue, oldDom, isHydrating);
      newDom = childVNode._dom;

      if ((j = childVNode.ref) && oldVNode.ref != j) {
        if (!refs) refs = [];
        if (oldVNode.ref) refs.push(oldVNode.ref, null, childVNode);
        refs.push(j, childVNode._component || newDom, childVNode);
      }

      if (newDom != null) {
        if (firstChildDom == null) {
          firstChildDom = newDom;
        }

        if (typeof childVNode.type == 'function' && childVNode._children != null && // Can be null if childVNode suspended
        childVNode._children === oldVNode._children) {
          childVNode._nextDom = oldDom = reorderChildren(childVNode, oldDom, parentDom);
        } else {
          oldDom = placeChild(parentDom, childVNode, oldVNode, oldChildren, newDom, oldDom);
        } // Browsers will infer an option's `value` from `textContent` when
        // no value is present. This essentially bypasses our code to set it
        // later in `diff()`. It works fine in all browsers except for IE11
        // where it breaks setting `select.value`. There it will be always set
        // to an empty string. Re-applying an options value will fix that, so
        // there are probably some internal data structures that aren't
        // updated properly.
        //
        // To fix it we make sure to reset the inferred value, so that our own
        // value check in `diff()` won't be skipped.


        if (!isHydrating && newParentVNode.type === 'option') {
          // @ts-ignore We have validated that the type of parentDOM is 'option'
          // in the above check
          parentDom.value = '';
        } else if (typeof newParentVNode.type == 'function') {
          // Because the newParentVNode is Fragment-like, we need to set it's
          // _nextDom property to the nextSibling of its last child DOM node.
          //
          // `oldDom` contains the correct value here because if the last child
          // is a Fragment-like, then oldDom has already been set to that child's _nextDom.
          // If the last child is a DOM VNode, then oldDom will be set to that DOM
          // node's nextSibling.
          newParentVNode._nextDom = oldDom;
        }
      } else if (oldDom && oldVNode._dom == oldDom && oldDom.parentNode != parentDom) {
        // The above condition is to handle null placeholders. See test in placeholder.test.js:
        // `efficiently replace null placeholders in parent rerenders`
        oldDom = getDomSibling(oldVNode);
      }
    }

    newParentVNode._dom = firstChildDom; // Remove remaining oldChildren if there are any.

    for (i = oldChildrenLength; i--;) {
      if (oldChildren[i] != null) {
        if (typeof newParentVNode.type == 'function' && oldChildren[i]._dom != null && oldChildren[i]._dom == newParentVNode._nextDom) {
          // If the newParentVNode.__nextDom points to a dom node that is about to
          // be unmounted, then get the next sibling of that vnode and set
          // _nextDom to it
          newParentVNode._nextDom = getDomSibling(oldParentVNode, i + 1);
        }

        unmount(oldChildren[i], oldChildren[i]);
      }
    } // Set refs only after unmount


    if (refs) {
      for (i = 0; i < refs.length; i++) {
        applyRef(refs[i], refs[++i], refs[++i]);
      }
    }
  }

  function reorderChildren(childVNode, oldDom, parentDom) {
    for (var tmp = 0; tmp < childVNode._children.length; tmp++) {
      var vnode = childVNode._children[tmp];

      if (vnode) {
        // We typically enter this code path on sCU bailout, where we copy
        // oldVNode._children to newVNode._children. If that is the case, we need
        // to update the old children's _parent pointer to point to the newVNode
        // (childVNode here).
        vnode._parent = childVNode;

        if (typeof vnode.type == 'function') {
          oldDom = reorderChildren(vnode, oldDom, parentDom);
        } else {
          oldDom = placeChild(parentDom, vnode, vnode, childVNode._children, vnode._dom, oldDom);
        }
      }
    }

    return oldDom;
  }

  function placeChild(parentDom, childVNode, oldVNode, oldChildren, newDom, oldDom) {
    var nextDom;

    if (childVNode._nextDom !== undefined) {
      // Only Fragments or components that return Fragment like VNodes will
      // have a non-undefined _nextDom. Continue the diff from the sibling
      // of last DOM child of this child VNode
      nextDom = childVNode._nextDom; // Eagerly cleanup _nextDom. We don't need to persist the value because
      // it is only used by `diffChildren` to determine where to resume the diff after
      // diffing Components and Fragments. Once we store it the nextDOM local var, we
      // can clean up the property

      childVNode._nextDom = undefined;
    } else if (oldVNode == null || newDom != oldDom || newDom.parentNode == null) {
      outer: if (oldDom == null || oldDom.parentNode !== parentDom) {
        parentDom.appendChild(newDom);
        nextDom = null;
      } else {
        // `j<oldChildrenLength; j+=2` is an alternative to `j++<oldChildrenLength/2`
        for (var sibDom = oldDom, j = 0; (sibDom = sibDom.nextSibling) && j < oldChildren.length; j += 2) {
          if (sibDom == newDom) {
            break outer;
          }
        }

        parentDom.insertBefore(newDom, oldDom);
        nextDom = oldDom;
      }
    } // If we have pre-calculated the nextDOM node, use it. Else calculate it now
    // Strictly check for `undefined` here cuz `null` is a valid value of `nextDom`.
    // See more detail in create-element.js:createVNode


    if (nextDom !== undefined) {
      oldDom = nextDom;
    } else {
      oldDom = newDom.nextSibling;
    }

    return oldDom;
  }

  /**
   * Diff the old and new properties of a VNode and apply changes to the DOM node
   * @param {import('../internal').PreactElement} dom The DOM node to apply
   * changes to
   * @param {object} newProps The new props
   * @param {object} oldProps The old props
   * @param {boolean} isSvg Whether or not this node is an SVG node
   * @param {boolean} hydrate Whether or not we are in hydration mode
   */

  function diffProps(dom, newProps, oldProps, isSvg, hydrate) {
    var i;

    for (i in oldProps) {
      if (i !== 'children' && i !== 'key' && !(i in newProps)) {
        setProperty(dom, i, null, oldProps[i], isSvg);
      }
    }

    for (i in newProps) {
      if ((!hydrate || typeof newProps[i] == 'function') && i !== 'children' && i !== 'key' && i !== 'value' && i !== 'checked' && oldProps[i] !== newProps[i]) {
        setProperty(dom, i, newProps[i], oldProps[i], isSvg);
      }
    }
  }

  function setStyle(style, key, value) {
    if (key[0] === '-') {
      style.setProperty(key, value);
    } else if (value == null) {
      style[key] = '';
    } else if (typeof value != 'number' || IS_NON_DIMENSIONAL.test(key)) {
      style[key] = value;
    } else {
      style[key] = value + 'px';
    }
  }
  /**
   * Set a property value on a DOM node
   * @param {import('../internal').PreactElement} dom The DOM node to modify
   * @param {string} name The name of the property to set
   * @param {*} value The value to set the property to
   * @param {*} oldValue The old value the property had
   * @param {boolean} isSvg Whether or not this DOM node is an SVG node or not
   */


  function setProperty(dom, name, value, oldValue, isSvg) {
    var useCapture;

    o: if (name === 'style') {
      if (typeof value == 'string') {
        dom.style.cssText = value;
      } else {
        if (typeof oldValue == 'string') {
          dom.style.cssText = oldValue = '';
        }

        if (oldValue) {
          for (name in oldValue) {
            if (!(value && name in value)) {
              setStyle(dom.style, name, '');
            }
          }
        }

        if (value) {
          for (name in value) {
            if (!oldValue || value[name] !== oldValue[name]) {
              setStyle(dom.style, name, value[name]);
            }
          }
        }
      }
    } // Benchmark for comparison: https://esbench.com/bench/574c954bdb965b9a00965ac6
    else if (name[0] === 'o' && name[1] === 'n') {
      useCapture = name !== (name = name.replace(/Capture$/, '')); // Infer correct casing for DOM built-in events:

      if (name.toLowerCase() in dom) name = name.toLowerCase().slice(2);else name = name.slice(2);
      if (!dom._listeners) dom._listeners = {};
      dom._listeners[name + useCapture] = value;

      if (value) {
        if (!oldValue) {
          var handler = useCapture ? eventProxyCapture : eventProxy;
          dom.addEventListener(name, handler, useCapture);
        }
      } else {
        var _handler = useCapture ? eventProxyCapture : eventProxy;

        dom.removeEventListener(name, _handler, useCapture);
      }
    } else if (name !== 'dangerouslySetInnerHTML') {
      if (isSvg) {
        // Normalize incorrect prop usage for SVG:
        // - xlink:href / xlinkHref --> href (xlink:href was removed from SVG and isn't needed)
        // - className --> class
        name = name.replace(/xlink[H:h]/, 'h').replace(/sName$/, 's');
      } else if (name !== 'href' && name !== 'list' && name !== 'form' && // Default value in browsers is `-1` and an empty string is
      // cast to `0` instead
      name !== 'tabIndex' && name !== 'download' && name in dom) {
        try {
          dom[name] = value == null ? '' : value; // labelled break is 1b smaller here than a return statement (sorry)

          break o;
        } catch (e) {}
      } // ARIA-attributes have a different notion of boolean values.
      // The value `false` is different from the attribute not
      // existing on the DOM, so we can't remove it. For non-boolean
      // ARIA-attributes we could treat false as a removal, but the
      // amount of exceptions would cost us too many bytes. On top of
      // that other VDOM frameworks also always stringify `false`.


      if (typeof value === 'function') ; else if (value != null && (value !== false || name[0] === 'a' && name[1] === 'r')) {
        dom.setAttribute(name, value);
      } else {
        dom.removeAttribute(name);
      }
    }
  }
  /**
   * Proxy an event to hooked event handlers
   * @param {Event} e The event object from the browser
   * @private
   */

  function eventProxy(e) {
    this._listeners[e.type + false](options$1.event ? options$1.event(e) : e);
  }

  function eventProxyCapture(e) {
    this._listeners[e.type + true](options$1.event ? options$1.event(e) : e);
  }

  /**
   * Diff two virtual nodes and apply proper changes to the DOM
   * @param {import('../internal').PreactElement} parentDom The parent of the DOM element
   * @param {import('../internal').VNode} newVNode The new virtual node
   * @param {import('../internal').VNode} oldVNode The old virtual node
   * @param {object} globalContext The current context object. Modified by getChildContext
   * @param {boolean} isSvg Whether or not this element is an SVG node
   * @param {Array<import('../internal').PreactElement>} excessDomChildren
   * @param {Array<import('../internal').Component>} commitQueue List of components
   * which have callbacks to invoke in commitRoot
   * @param {import('../internal').PreactElement} oldDom The current attached DOM
   * element any new dom elements should be placed around. Likely `null` on first
   * render (except when hydrating). Can be a sibling DOM element when diffing
   * Fragments that have siblings. In most cases, it starts out as `oldChildren[0]._dom`.
   * @param {boolean} [isHydrating] Whether or not we are in hydration
   */

  function diff(parentDom, newVNode, oldVNode, globalContext, isSvg, excessDomChildren, commitQueue, oldDom, isHydrating) {
    var tmp,
        newType = newVNode.type; // When passing through createElement it assigns the object
    // constructor as undefined. This to prevent JSON-injection.

    if (newVNode.constructor !== undefined) return null; // If the previous diff bailed out, resume creating/hydrating.

    if (oldVNode._hydrating != null) {
      isHydrating = oldVNode._hydrating;
      oldDom = newVNode._dom = oldVNode._dom; // if we resume, we want the tree to be "unlocked"

      newVNode._hydrating = null;
      excessDomChildren = [oldDom];
    }

    if (tmp = options$1._diff) tmp(newVNode);

    try {
      outer: if (typeof newType == 'function') {
        var c, isNew, oldProps, oldState, snapshot, clearProcessingException;
        var newProps = newVNode.props; // Necessary for createContext api. Setting this property will pass
        // the context value as `this.context` just for this component.

        tmp = newType.contextType;
        var provider = tmp && globalContext[tmp._id];
        var componentContext = tmp ? provider ? provider.props.value : tmp._defaultValue : globalContext; // Get component and set it to `c`

        if (oldVNode._component) {
          c = newVNode._component = oldVNode._component;
          clearProcessingException = c._processingException = c._pendingError;
        } else {
          // Instantiate the new component
          if ('prototype' in newType && newType.prototype.render) {
            // @ts-ignore The check above verifies that newType is suppose to be constructed
            newVNode._component = c = new newType(newProps, componentContext); // eslint-disable-line new-cap
          } else {
            // @ts-ignore Trust me, Component implements the interface we want
            newVNode._component = c = new Component(newProps, componentContext);
            c.constructor = newType;
            c.render = doRender;
          }

          if (provider) provider.sub(c);
          c.props = newProps;
          if (!c.state) c.state = {};
          c.context = componentContext;
          c._globalContext = globalContext;
          isNew = c._dirty = true;
          c._renderCallbacks = [];
        } // Invoke getDerivedStateFromProps


        if (c._nextState == null) {
          c._nextState = c.state;
        }

        if (newType.getDerivedStateFromProps != null) {
          if (c._nextState == c.state) {
            c._nextState = assign({}, c._nextState);
          }

          assign(c._nextState, newType.getDerivedStateFromProps(newProps, c._nextState));
        }

        oldProps = c.props;
        oldState = c.state; // Invoke pre-render lifecycle methods

        if (isNew) {
          if (newType.getDerivedStateFromProps == null && c.componentWillMount != null) {
            c.componentWillMount();
          }

          if (c.componentDidMount != null) {
            c._renderCallbacks.push(c.componentDidMount);
          }
        } else {
          if (newType.getDerivedStateFromProps == null && newProps !== oldProps && c.componentWillReceiveProps != null) {
            c.componentWillReceiveProps(newProps, componentContext);
          }

          if (!c._force && c.shouldComponentUpdate != null && c.shouldComponentUpdate(newProps, c._nextState, componentContext) === false || newVNode._original === oldVNode._original) {
            c.props = newProps;
            c.state = c._nextState; // More info about this here: https://gist.github.com/JoviDeCroock/bec5f2ce93544d2e6070ef8e0036e4e8

            if (newVNode._original !== oldVNode._original) c._dirty = false;
            c._vnode = newVNode;
            newVNode._dom = oldVNode._dom;
            newVNode._children = oldVNode._children;

            newVNode._children.forEach(function (vnode) {
              if (vnode) vnode._parent = newVNode;
            });

            if (c._renderCallbacks.length) {
              commitQueue.push(c);
            }

            break outer;
          }

          if (c.componentWillUpdate != null) {
            c.componentWillUpdate(newProps, c._nextState, componentContext);
          }

          if (c.componentDidUpdate != null) {
            c._renderCallbacks.push(function () {
              c.componentDidUpdate(oldProps, oldState, snapshot);
            });
          }
        }

        c.context = componentContext;
        c.props = newProps;
        c.state = c._nextState;
        if (tmp = options$1._render) tmp(newVNode);
        c._dirty = false;
        c._vnode = newVNode;
        c._parentDom = parentDom;
        tmp = c.render(c.props, c.state, c.context); // Handle setState called in render, see #2553

        c.state = c._nextState;

        if (c.getChildContext != null) {
          globalContext = assign(assign({}, globalContext), c.getChildContext());
        }

        if (!isNew && c.getSnapshotBeforeUpdate != null) {
          snapshot = c.getSnapshotBeforeUpdate(oldProps, oldState);
        }

        var isTopLevelFragment = tmp != null && tmp.type === Fragment && tmp.key == null;
        var renderResult = isTopLevelFragment ? tmp.props.children : tmp;
        diffChildren(parentDom, Array.isArray(renderResult) ? renderResult : [renderResult], newVNode, oldVNode, globalContext, isSvg, excessDomChildren, commitQueue, oldDom, isHydrating);
        c.base = newVNode._dom; // We successfully rendered this VNode, unset any stored hydration/bailout state:

        newVNode._hydrating = null;

        if (c._renderCallbacks.length) {
          commitQueue.push(c);
        }

        if (clearProcessingException) {
          c._pendingError = c._processingException = null;
        }

        c._force = false;
      } else if (excessDomChildren == null && newVNode._original === oldVNode._original) {
        newVNode._children = oldVNode._children;
        newVNode._dom = oldVNode._dom;
      } else {
        newVNode._dom = diffElementNodes(oldVNode._dom, newVNode, oldVNode, globalContext, isSvg, excessDomChildren, commitQueue, isHydrating);
      }

      if (tmp = options$1.diffed) tmp(newVNode);
    } catch (e) {
      newVNode._original = null; // if hydrating or creating initial tree, bailout preserves DOM:

      if (isHydrating || excessDomChildren != null) {
        newVNode._dom = oldDom;
        newVNode._hydrating = !!isHydrating;
        excessDomChildren[excessDomChildren.indexOf(oldDom)] = null; // ^ could possibly be simplified to:
        // excessDomChildren.length = 0;
      }

      options$1._catchError(e, newVNode, oldVNode);
    }
  }
  /**
   * @param {Array<import('../internal').Component>} commitQueue List of components
   * which have callbacks to invoke in commitRoot
   * @param {import('../internal').VNode} root
   */

  function commitRoot(commitQueue, root) {
    if (options$1._commit) options$1._commit(root, commitQueue);
    commitQueue.some(function (c) {
      try {
        // @ts-ignore Reuse the commitQueue variable here so the type changes
        commitQueue = c._renderCallbacks;
        c._renderCallbacks = [];
        commitQueue.some(function (cb) {
          // @ts-ignore See above ts-ignore on commitQueue
          cb.call(c);
        });
      } catch (e) {
        options$1._catchError(e, c._vnode);
      }
    });
  }
  /**
   * Diff two virtual nodes representing DOM element
   * @param {import('../internal').PreactElement} dom The DOM element representing
   * the virtual nodes being diffed
   * @param {import('../internal').VNode} newVNode The new virtual node
   * @param {import('../internal').VNode} oldVNode The old virtual node
   * @param {object} globalContext The current context object
   * @param {boolean} isSvg Whether or not this DOM node is an SVG node
   * @param {*} excessDomChildren
   * @param {Array<import('../internal').Component>} commitQueue List of components
   * which have callbacks to invoke in commitRoot
   * @param {boolean} isHydrating Whether or not we are in hydration
   * @returns {import('../internal').PreactElement}
   */

  function diffElementNodes(dom, newVNode, oldVNode, globalContext, isSvg, excessDomChildren, commitQueue, isHydrating) {
    var oldProps = oldVNode.props;
    var newProps = newVNode.props;
    var nodeType = newVNode.type;
    var i = 0; // Tracks entering and exiting SVG namespace when descending through the tree.

    if (nodeType === 'svg') isSvg = true;

    if (excessDomChildren != null) {
      for (; i < excessDomChildren.length; i++) {
        var child = excessDomChildren[i]; // if newVNode matches an element in excessDomChildren or the `dom`
        // argument matches an element in excessDomChildren, remove it from
        // excessDomChildren so it isn't later removed in diffChildren

        if (child && (child === dom || (nodeType ? child.localName == nodeType : child.nodeType == 3))) {
          dom = child;
          excessDomChildren[i] = null;
          break;
        }
      }
    }

    if (dom == null) {
      if (nodeType === null) {
        // @ts-ignore createTextNode returns Text, we expect PreactElement
        return document.createTextNode(newProps);
      }

      if (isSvg) {
        dom = document.createElementNS('http://www.w3.org/2000/svg', // @ts-ignore We know `newVNode.type` is a string
        nodeType);
      } else {
        dom = document.createElement( // @ts-ignore We know `newVNode.type` is a string
        nodeType, newProps.is && newProps);
      } // we created a new parent, so none of the previously attached children can be reused:


      excessDomChildren = null; // we are creating a new node, so we can assume this is a new subtree (in case we are hydrating), this deopts the hydrate

      isHydrating = false;
    }

    if (nodeType === null) {
      // During hydration, we still have to split merged text from SSR'd HTML.
      if (oldProps !== newProps && (!isHydrating || dom.data !== newProps)) {
        dom.data = newProps;
      }
    } else {
      // If excessDomChildren was not null, repopulate it with the current element's children:
      excessDomChildren = excessDomChildren && EMPTY_ARR.slice.call(dom.childNodes);
      oldProps = oldVNode.props || EMPTY_OBJ;
      var oldHtml = oldProps.dangerouslySetInnerHTML;
      var newHtml = newProps.dangerouslySetInnerHTML; // During hydration, props are not diffed at all (including dangerouslySetInnerHTML)
      // @TODO we should warn in debug mode when props don't match here.

      if (!isHydrating) {
        // But, if we are in a situation where we are using existing DOM (e.g. replaceNode)
        // we should read the existing DOM attributes to diff them
        if (excessDomChildren != null) {
          oldProps = {}; // NOTE: this is commented, because we need to keep the existing DOM attributes
          // See: https://github.com/preactjs/preact/issues/2449
          // for (let i = 0; i < dom.attributes.length; i++) {
          //	 oldProps[dom.attributes[i].name] = dom.attributes[i].value;
          // }
        }

        if (newHtml || oldHtml) {
          // Avoid re-applying the same '__html' if it did not changed between re-render
          if (!newHtml || (!oldHtml || newHtml.__html != oldHtml.__html) && newHtml.__html !== dom.innerHTML) {
            dom.innerHTML = newHtml && newHtml.__html || '';
          }
        }
      }

      diffProps(dom, newProps, oldProps, isSvg, isHydrating); // If the new vnode didn't have dangerouslySetInnerHTML, diff its children

      if (newHtml) {
        newVNode._children = [];
      } else {
        i = newVNode.props.children;
        diffChildren(dom, Array.isArray(i) ? i : [i], newVNode, oldVNode, globalContext, isSvg && nodeType !== 'foreignObject', excessDomChildren, commitQueue, dom.firstChild, isHydrating); // Remove children that are not part of any vnode.

        if (excessDomChildren != null) {
          for (i = excessDomChildren.length; i--;) {
            if (excessDomChildren[i] != null) removeNode(excessDomChildren[i]);
          }
        }
      } // (as above, don't diff props during hydration)


      if (!isHydrating) {
        if ('value' in newProps && (i = newProps.value) !== undefined && ( // #2756 For the <progress>-element the initial value is 0,
        // despite the attribute not being present. When the attribute
        // is missing the progress bar is treated as indeterminate.
        // To fix that we'll always update it when it is 0 for progress elements
        i !== dom.value || nodeType === 'progress' && !i)) {
          setProperty(dom, 'value', i, oldProps.value, false);
        }

        if ('checked' in newProps && (i = newProps.checked) !== undefined && i !== dom.checked) {
          setProperty(dom, 'checked', i, oldProps.checked, false);
        }
      }
    }

    return dom;
  }
  /**
   * Invoke or update a ref, depending on whether it is a function or object ref.
   * @param {object|function} ref
   * @param {any} value
   * @param {import('../internal').VNode} vnode
   */


  function applyRef(ref, value, vnode) {
    try {
      if (typeof ref == 'function') ref(value);else ref.current = value;
    } catch (e) {
      options$1._catchError(e, vnode);
    }
  }
  /**
   * Unmount a virtual node from the tree and apply DOM changes
   * @param {import('../internal').VNode} vnode The virtual node to unmount
   * @param {import('../internal').VNode} parentVNode The parent of the VNode that
   * initiated the unmount
   * @param {boolean} [skipRemove] Flag that indicates that a parent node of the
   * current element is already detached from the DOM.
   */

  function unmount(vnode, parentVNode, skipRemove) {
    var r;
    if (options$1.unmount) options$1.unmount(vnode);

    if (r = vnode.ref) {
      if (!r.current || r.current === vnode._dom) applyRef(r, null, parentVNode);
    }

    var dom;

    if (!skipRemove && typeof vnode.type != 'function') {
      skipRemove = (dom = vnode._dom) != null;
    } // Must be set to `undefined` to properly clean up `_nextDom`
    // for which `null` is a valid value. See comment in `create-element.js`


    vnode._dom = vnode._nextDom = undefined;

    if ((r = vnode._component) != null) {
      if (r.componentWillUnmount) {
        try {
          r.componentWillUnmount();
        } catch (e) {
          options$1._catchError(e, parentVNode);
        }
      }

      r.base = r._parentDom = null;
    }

    if (r = vnode._children) {
      for (var i = 0; i < r.length; i++) {
        if (r[i]) unmount(r[i], parentVNode, skipRemove);
      }
    }

    if (dom != null) removeNode(dom);
  }
  /** The `.render()` method for a PFC backing instance. */

  function doRender(props, state, context) {
    return this.constructor(props, context);
  }

  /**
   * Render a Preact virtual node into a DOM element
   * @param {import('./internal').ComponentChild} vnode The virtual node to render
   * @param {import('./internal').PreactElement} parentDom The DOM element to
   * render into
   * @param {import('./internal').PreactElement | object} [replaceNode] Optional: Attempt to re-use an
   * existing DOM tree rooted at `replaceNode`
   */

  function render(vnode, parentDom, replaceNode) {
    if (options$1._root) options$1._root(vnode, parentDom); // We abuse the `replaceNode` parameter in `hydrate()` to signal if we are in
    // hydration mode or not by passing the `hydrate` function instead of a DOM
    // element..

    var isHydrating = typeof replaceNode === 'function'; // To be able to support calling `render()` multiple times on the same
    // DOM node, we need to obtain a reference to the previous tree. We do
    // this by assigning a new `_children` property to DOM nodes which points
    // to the last rendered tree. By default this property is not present, which
    // means that we are mounting a new tree for the first time.

    var oldVNode = isHydrating ? null : replaceNode && replaceNode._children || parentDom._children;
    vnode = (!isHydrating && replaceNode || parentDom)._children = createElement(Fragment, null, [vnode]); // List of effects that need to be called after diffing.

    var commitQueue = [];
    diff(parentDom, // Determine the new vnode tree and store it on the DOM element on
    // our custom `_children` property.
    vnode, oldVNode || EMPTY_OBJ, EMPTY_OBJ, parentDom.ownerSVGElement !== undefined, !isHydrating && replaceNode ? [replaceNode] : oldVNode ? null : parentDom.firstChild ? EMPTY_ARR.slice.call(parentDom.childNodes) : null, commitQueue, !isHydrating && replaceNode ? replaceNode : oldVNode ? oldVNode._dom : parentDom.firstChild, isHydrating); // Flush all queued effects

    commitRoot(commitQueue, vnode);
  }

  var i = 0;
  function createContext(defaultValue, contextId) {
    contextId = '__cC' + i++;
    var context = {
      _id: contextId,
      _defaultValue: defaultValue,

      /** @type {import('./internal').FunctionComponent} */
      Consumer: function Consumer(props, contextValue) {
        // return props.children(
        // 	context[contextId] ? context[contextId].props.value : defaultValue
        // );
        return props.children(contextValue);
      },

      /** @type {import('./internal').FunctionComponent} */
      Provider: function Provider(props) {
        if (!this.getChildContext) {
          var subs = [];
          var ctx = {};
          ctx[contextId] = this;

          this.getChildContext = function () {
            return ctx;
          };

          this.shouldComponentUpdate = function (_props) {
            if (this.props.value !== _props.value) {
              // I think the forced value propagation here was only needed when `options.debounceRendering` was being bypassed:
              // https://github.com/preactjs/preact/commit/4d339fb803bea09e9f198abf38ca1bf8ea4b7771#diff-54682ce380935a717e41b8bfc54737f6R358
              // In those cases though, even with the value corrected, we're double-rendering all nodes.
              // It might be better to just tell folks not to use force-sync mode.
              // Currently, using `useContext()` in a class component will overwrite its `this.context` value.
              // subs.some(c => {
              // 	c.context = _props.value;
              // 	enqueueRender(c);
              // });
              // subs.some(c => {
              // 	c.context[contextId] = _props.value;
              // 	enqueueRender(c);
              // });
              subs.some(enqueueRender);
            }
          };

          this.sub = function (c) {
            subs.push(c);
            var old = c.componentWillUnmount;

            c.componentWillUnmount = function () {
              subs.splice(subs.indexOf(c), 1);
              if (old) old.call(c);
            };
          };
        }

        return props.children;
      }
    }; // Devtools needs access to the context object when it
    // encounters a Provider. This is necessary to support
    // setting `displayName` on the context object instead
    // of on the component itself. See:
    // https://reactjs.org/docs/context.html#contextdisplayname

    return context.Provider._contextRef = context.Consumer.contextType = context;
  }

  var PureComponent = /*#__PURE__*/ (function (_super) {
      __extends(PureComponent, _super);
      function PureComponent() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      PureComponent.prototype.render = function () {
          return;
      };
      PureComponent.prototype.shouldComponentUpdate = function (props, state) {
          return shallowDiffers(props, this.props) || shallowDiffers(state, this.state);
      };
      return PureComponent;
  }(Component));
  function shallowDiffers(a, b) {
      for (var key in a) {
          if (a[key] !== b[key]) {
              return true;
          }
      }
      for (var key in b) {
          if (!(key in a)) {
              return true;
          }
      }
      return false;
  }

  var ON_ANIMATION_END = 'onAnimationEnd';
  var ON_CONTEXT_MENU = 'onContextMenu';
  var ON_DOUBLE_CLICK = 'onDoubleClick';
  var ON_KEY_DOWN = 'onKeyDown';
  var ON_MOUSE_LEAVE = 'onMouseLeave';
  var ON_MOUSE_MOVE = 'onMouseMove';
  options$1.vnode = function (vnode) {
      var props = vnode.props;
      var normalizedProps = {};
      // Only check props on Element nodes
      if (isString(vnode.type)) {
          // tslint:disable-next-line: forin
          for (var i in props) {
              var value = props[i];
              // Alter preact behavior to modify onAnimationEnd to onanimationend,
              // to make it work on older Edge versions.
              if (/^onAni/.test(i)) {
                  i = i.toLowerCase();
              }
              else if (/ondoubleclick/i.test(i)) {
                  i = 'ondblclick';
              }
              normalizedProps[i] = value;
          }
          vnode.props = normalizedProps;
      }
  };
  var components = {};
  var guid = 0;
  function initComponents(target, selector, Component, renderOptions, opt) {
      if (matches(target, selector)) {
          if (!target.__mbscFormInst) {
              createComponent(Component, target, opt, renderOptions, true);
          }
      }
      else {
          var elements = target.querySelectorAll(selector);
          forEach(elements, function (elm) {
              if (!elm.__mbscFormInst) {
                  createComponent(Component, elm, opt, renderOptions, true);
              }
          });
      }
  }
  /**
   * Creates and renders a Preact component for/inside the specified element.
   * @param Component The component which needs to be created.
   * @param elm The element for which the component is needed.
   * @param initOpt Init options for the component.
   * @param renderOptions Render options for the component.
   */
  function createComponent(Component, elm, initOpt, renderOptions, formControl) {
      var _a;
      var inst;
      var children = [];
      var allChildren = [];
      var slotElms = {};
      var renderOpt = renderOptions || {};
      var replaceNode = renderOpt.renderToParent ? elm.parentNode : elm;
      var renderTo = replaceNode.parentNode;
      var childrenNode = renderOpt.useOwnChildren ? elm : replaceNode;
      var elmClass = elm.getAttribute('class');
      var value = elm.value;
      var opt = __assign({ className: replaceNode.getAttribute('class') }, elm.dataset, initOpt, { ref: function (c) {
              inst = c;
          } });
      if (renderOpt.readProps) {
          renderOpt.readProps.forEach(function (prop) {
              var v = elm[prop];
              if (v !== UNDEFINED) {
                  opt[prop] = v;
              }
          });
      }
      if (renderOpt.readAttrs) {
          renderOpt.readAttrs.forEach(function (prop) {
              var v = elm.getAttribute(prop);
              if (v !== null) {
                  opt[prop] = v;
              }
          });
      }
      var slots = renderOpt.slots;
      if (slots) {
          for (var _i = 0, _b = Object.keys(slots); _i < _b.length; _i++) {
              var key = _b[_i];
              var slot = slots[key];
              var slotElm = replaceNode.querySelector('[mbsc-' + slot + ']');
              if (slotElm) {
                  slotElms[key] = slotElm;
                  slotElm.parentNode.removeChild(slotElm);
                  // Create a virtual node placeholder element
                  opt[key] = createElement('span', { className: 'mbsc-slot-' + slot });
              }
          }
      }
      if (renderOpt.hasChildren) {
          // Remove existing children
          forEach(childrenNode.childNodes, function (child) {
              if (child !== elm && child.nodeType !== 8 && (child.nodeType !== 3 || (child.nodeType === 3 && /\S/.test(child.nodeValue)))) {
                  children.push(child);
              }
              allChildren.push(child);
          });
          forEach(children, function (child) {
              childrenNode.removeChild(child);
          });
          if (children.length) {
              opt.hasChildren = true;
              // opt.children = createElement('span', { className: 'mbsc-slot-children' });
          }
      }
      // Generate an id for the element, if there's none
      if (!elm.id) {
          elm.id = 'mbsc-control-' + guid++;
      }
      if (renderOpt.before) {
          renderOpt.before(elm, opt, children);
      }
      // Render the element
      render(createElement(Component, opt), renderTo, replaceNode);
      if (elmClass && renderOpt.renderToParent) {
          (_a = elm.classList).add.apply(_a, elmClass
              .replace(/^\s+|\s+$/g, '')
              .replace(/\s+|^\s|\s$/g, ' ')
              .split(' '));
      }
      if (renderOpt.hasChildren) {
          var selector = '.' + renderOpt.parentClass;
          var placeholder_1 = matches(replaceNode, selector) ? replaceNode : replaceNode.querySelector(selector);
          // const placeholder = replaceNode.querySelector('.mbsc-slot-children');
          // Add back existing children
          if (placeholder_1) {
              forEach(children, function (child) {
                  placeholder_1.appendChild(child);
              });
          }
      }
      if (renderOpt.hasValue) {
          elm.value = value;
      }
      if (slots) {
          var _loop_1 = function (key) {
              var slot = slots[key];
              var slotElm = slotElms[key];
              var placeholders = replaceNode.querySelectorAll('.mbsc-slot-' + slot);
              forEach(placeholders, function (placeholder, i) {
                  var child = i > 0 ? slotElm.cloneNode(true) : slotElm;
                  placeholder.appendChild(child);
              });
          };
          for (var _c = 0, _d = Object.keys(slotElms); _c < _d.length; _c++) {
              var key = _d[_c];
              _loop_1(key);
          }
      }
      // Create a destroy function
      inst.destroy = function () {
          var parent = replaceNode.parentNode;
          var placeholder = doc.createComment('');
          parent.insertBefore(placeholder, replaceNode);
          render(null, replaceNode);
          delete elm.__mbscInst;
          delete elm.__mbscFormInst;
          delete replaceNode._listeners;
          replaceNode.innerHTML = '';
          // Restore css class
          replaceNode.setAttribute('class', opt.className);
          // Put back the original element
          parent.replaceChild(replaceNode, placeholder);
          // Restore children and slots
          if (renderOpt.hasChildren) {
              // Add back existing children
              forEach(allChildren, function (child) {
                  childrenNode.appendChild(child);
              });
          }
          // Restore css class on the element
          if (renderOpt.renderToParent) {
              elm.setAttribute('class', elmClass || '');
          }
      };
      // Store the instance on the element
      if (formControl) {
          if (!elm.__mbscInst) {
              elm.__mbscInst = inst;
          }
          elm.__mbscFormInst = inst;
      }
      else {
          elm.__mbscInst = inst;
      }
      return inst;
  }
  function getInst(elm, formControl) {
      return formControl ? elm.__mbscFormInst : elm.__mbscInst;
  }
  function registerComponent(Component) {
      components[Component._name] = Component;
  }
  /**
   * Will auto-init the registered components inside the provided element.
   * @param elm The element in which the components should be enhanced.
   */
  function enhance(elm, opt) {
      if (elm) {
          for (var _i = 0, _a = Object.keys(components); _i < _a.length; _i++) {
              var name_1 = _a[_i];
              var Component = components[name_1];
              var selector = Component._selector;
              var renderOpt = Component._renderOpt;
              initComponents(elm, selector, Component, renderOpt, opt);
          }
      }
  }

  var extend = jQuery__default['default'].extend;
  var components$1 = {};
  function registerComponent$1(Component) {
      // Register for auto-init
      if (Component._selector) {
          registerComponent(Component);
      }
      // Register as a jquery plugin
      components$1[Component._fname] = function (options) {
          if (Component) {
              this.each(function () {
                  createComponent(Component, this, options, Component._renderOpt);
              });
          }
          return this;
      };
  }
  jQuery__default['default'].fn.mobiscroll = function (options) {
      var args = [];
      for (var _i = 1; _i < arguments.length; _i++) {
          args[_i - 1] = arguments[_i];
      }
      extend(this, components$1);
      if (isString(options)) {
          var ret_1 = this;
          this.each(function () {
              var returnValue;
              var inst = this.__mbscInst;
              if (inst && inst[options]) {
                  returnValue = inst[options].apply(inst, args);
                  if (returnValue !== UNDEFINED) {
                      ret_1 = returnValue;
                      return false;
                  }
              }
          });
          return ret_1;
      }
      return this;
  };
  if (isBrowser) {
      jQuery__default['default'](function () {
          enhance(doc);
      });
      jQuery__default['default'](doc).on('mbsc-enhance', function (ev) {
          enhance(ev.target);
      });
  }

  // Arabic
  function intPart(floatNum) {
      if (floatNum < -0.0000001) {
          return Math.ceil(floatNum - 0.0000001);
      }
      return Math.floor(floatNum + 0.0000001);
  }
  function hijriToGregorian(hY, hM, hD) {
      var l;
      var j;
      var n;
      var i;
      var k;
      var gregDate = new Array(3);
      var jd = intPart((11 * hY + 3) / 30) + 354 * hY + 30 * hM - intPart((hM - 1) / 2) + hD + 1948440 - 385;
      if (jd > 2299160) {
          l = jd + 68569;
          n = intPart((4 * l) / 146097);
          l = l - intPart((146097 * n + 3) / 4);
          i = intPart((4000 * (l + 1)) / 1461001);
          l = l - intPart((1461 * i) / 4) + 31;
          j = intPart((80 * l) / 2447);
          hD = l - intPart((2447 * j) / 80);
          l = intPart(j / 11);
          hM = j + 2 - 12 * l;
          hY = 100 * (n - 49) + i + l;
      }
      else {
          j = jd + 1402;
          k = intPart((j - 1) / 1461);
          l = j - 1461 * k;
          n = intPart((l - 1) / 365) - intPart(l / 1461);
          i = l - 365 * n + 30;
          j = intPart((80 * i) / 2447);
          hD = i - intPart((2447 * j) / 80);
          i = intPart(j / 11);
          hM = j + 2 - 12 * i;
          hY = 4 * k + n + i - 4716;
      }
      gregDate[2] = hD;
      gregDate[1] = hM;
      gregDate[0] = hY;
      return gregDate;
  }
  function gregorianToHijri(gY, gM, gD) {
      var jd;
      var l;
      var hijriDate = [0, 0, 0];
      if (gY > 1582 || (gY === 1582 && gM > 10) || (gY === 1582 && gM === 10 && gD > 14)) {
          jd =
              intPart((1461 * (gY + 4800 + intPart((gM - 14) / 12))) / 4) +
                  intPart((367 * (gM - 2 - 12 * intPart((gM - 14) / 12))) / 12) -
                  intPart((3 * intPart((gY + 4900 + intPart((gM - 14) / 12)) / 100)) / 4) +
                  gD -
                  32075;
      }
      else {
          jd = 367 * gY - intPart((7 * (gY + 5001 + intPart((gM - 9) / 7))) / 4) + intPart((275 * gM) / 9) + gD + 1729777;
      }
      l = jd - 1948440 + 10632;
      var n = intPart((l - 1) / 10631);
      l = l - 10631 * n + 354;
      var j = intPart((10985 - l) / 5316) * intPart((50 * l) / 17719) + intPart(l / 5670) * intPart((43 * l) / 15238);
      l = l - intPart((30 - j) / 15) * intPart((17719 * j) / 50) - intPart(j / 16) * intPart((15238 * j) / 43) + 29;
      gM = intPart((24 * l) / 709);
      gD = l - intPart((709 * gM) / 24);
      gY = 30 * n + j - 30;
      hijriDate[2] = gD;
      hijriDate[1] = gM;
      hijriDate[0] = gY;
      return hijriDate;
  }
  /** @hidden */
  var hijriCalendar = {
      getYear: function (date) {
          return gregorianToHijri(date.getFullYear(), date.getMonth() + 1, date.getDate())[0];
      },
      getMonth: function (date) {
          return --gregorianToHijri(date.getFullYear(), date.getMonth() + 1, date.getDate())[1];
      },
      getDay: function (date) {
          return gregorianToHijri(date.getFullYear(), date.getMonth() + 1, date.getDate())[2];
      },
      getDate: function (y, m, d, h, i, s, u) {
          if (m < 0) {
              y += Math.floor(m / 12);
              m = m % 12 ? 12 + (m % 12) : 0;
          }
          if (m > 11) {
              y += Math.floor(m / 12);
              m = m % 12;
          }
          var gregorianDate = hijriToGregorian(y, +m + 1, d);
          return new Date(gregorianDate[0], gregorianDate[1] - 1, gregorianDate[2], h || 0, i || 0, s || 0, u || 0);
      },
      getMaxDayOfMonth: function (hY, hM) {
          if (hM < 0) {
              hY += Math.floor(hM / 12);
              hM = hM % 12 ? 12 + (hM % 12) : 0;
          }
          if (hM > 11) {
              hY += Math.floor(hM / 12);
              hM = hM % 12;
          }
          var daysPerMonth = [30, 29, 30, 29, 30, 29, 30, 29, 30, 29, 30, 29];
          var leapYear = (hY * 11 + 14) % 30 < 11;
          return daysPerMonth[hM] + (hM === 11 && leapYear ? 1 : 0);
      },
  };

  // فارسی
  var gDaysInMonth = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
  var jDaysInMonth = [31, 31, 31, 31, 31, 31, 30, 30, 30, 30, 30, 29];
  function jalaliToGregorian(jY, jM, jD) {
      var i;
      var jy = jY - 979;
      var jm = jM - 1;
      var jd = jD - 1;
      var jDayNo = 365 * jy + floor(jy / 33) * 8 + floor(((jy % 33) + 3) / 4);
      for (i = 0; i < jm; ++i) {
          jDayNo += jDaysInMonth[i];
      }
      jDayNo += jd;
      var gDayNo = jDayNo + 79;
      var gy = 1600 + 400 * floor(gDayNo / 146097);
      gDayNo = gDayNo % 146097;
      var leap = true;
      if (gDayNo >= 36525) {
          gDayNo--;
          gy += 100 * floor(gDayNo / 36524);
          gDayNo = gDayNo % 36524;
          if (gDayNo >= 365) {
              gDayNo++;
          }
          else {
              leap = false;
          }
      }
      gy += 4 * floor(gDayNo / 1461);
      gDayNo %= 1461;
      if (gDayNo >= 366) {
          leap = false;
          gDayNo--;
          gy += floor(gDayNo / 365);
          gDayNo = gDayNo % 365;
      }
      for (i = 0; gDayNo >= gDaysInMonth[i] + (i === 1 && leap ? 1 : 0); i++) {
          gDayNo -= gDaysInMonth[i] + (i === 1 && leap ? 1 : 0);
      }
      var gm = i + 1;
      var gd = gDayNo + 1;
      return [gy, gm, gd];
  }
  function checkDate(jY, jM, jD) {
      return !(jY < 0 ||
          jY > 32767 ||
          jM < 1 ||
          jM > 12 ||
          jD < 1 ||
          jD > jDaysInMonth[jM - 1] + (jM === 12 && ((jY - 979) % 33) % 4 === 0 ? 1 : 0));
  }
  function gregorianToJalali(gY, gM, gD) {
      var i;
      var gy = gY - 1600;
      var gm = gM - 1;
      var gd = gD - 1;
      var gDayNo = 365 * gy + floor((gy + 3) / 4) - floor((gy + 99) / 100) + floor((gy + 399) / 400);
      for (i = 0; i < gm; ++i) {
          gDayNo += gDaysInMonth[i];
      }
      if (gm > 1 && ((gy % 4 === 0 && gy % 100 !== 0) || gy % 400 === 0)) {
          ++gDayNo;
      }
      gDayNo += gd;
      var jDayNo = gDayNo - 79;
      var jNp = floor(jDayNo / 12053);
      jDayNo %= 12053;
      var jy = 979 + 33 * jNp + 4 * floor(jDayNo / 1461);
      jDayNo %= 1461;
      if (jDayNo >= 366) {
          jy += floor((jDayNo - 1) / 365);
          jDayNo = (jDayNo - 1) % 365;
      }
      for (i = 0; i < 11 && jDayNo >= jDaysInMonth[i]; ++i) {
          jDayNo -= jDaysInMonth[i];
      }
      var jm = i + 1;
      var jd = jDayNo + 1;
      return [jy, jm, jd];
  }
  /** @hidden */
  var jalaliCalendar = {
      getYear: function (date) {
          return gregorianToJalali(date.getFullYear(), date.getMonth() + 1, date.getDate())[0];
      },
      getMonth: function (date) {
          return --gregorianToJalali(date.getFullYear(), date.getMonth() + 1, date.getDate())[1];
      },
      getDay: function (date) {
          return gregorianToJalali(date.getFullYear(), date.getMonth() + 1, date.getDate())[2];
      },
      getDate: function (y, m, d, h, i, s, u) {
          if (m < 0) {
              y += floor(m / 12);
              m = m % 12 ? 12 + (m % 12) : 0;
          }
          if (m > 11) {
              y += floor(m / 12);
              m = m % 12;
          }
          var gregorianDate = jalaliToGregorian(y, +m + 1, d);
          return new Date(gregorianDate[0], gregorianDate[1] - 1, gregorianDate[2], h || 0, i || 0, s || 0, u || 0);
      },
      getMaxDayOfMonth: function (y, m) {
          var maxdays = 31;
          if (m < 0) {
              y += floor(m / 12);
              m = m % 12 ? 12 + (m % 12) : 0;
          }
          if (m > 11) {
              y += floor(m / 12);
              m = m % 12;
          }
          while (!checkDate(y, m + 1, maxdays) && maxdays > 29) {
              maxdays--;
          }
          return maxdays;
      },
  };

  // ar import localeAr from '../i18n/ar';
  // bg import localeBg from '../i18n/bg';
  // ca import localeCa from '../i18n/ca';
  // cs import localeCs from '../i18n/cs';
  // da import localeDa from '../i18n/da';
  // de import localeDe from '../i18n/de';
  // el import localeEl from '../i18n/el';
  // engb import localeEnGB from '../i18n/en-GB';
  // es import localeEs from '../i18n/es';
  // fa import localeFa from '../i18n/fa';
  // fi import localeFi from '../i18n/fi';
  // fr import localeFr from '../i18n/fr';
  // he import localeHe from '../i18n/he';
  // hi import localeHi from '../i18n/hi';
  // hr import localeHr from '../i18n/hr';
  // hu import localeHu from '../i18n/hu';
  // it import localeIt from '../i18n/it';
  // ja import localeJa from '../i18n/ja';
  // ko import localeKo from '../i18n/ko';
  // lt import localeLt from '../i18n/lt';
  // nl import localeNl from '../i18n/nl';
  // no import localeNo from '../i18n/no';
  // pl import localePl from '../i18n/pl';
  // ptbr import localePtBR from '../i18n/pt-BR';
  // ptpt import localePtPT from '../i18n/pt-PT';
  // ro import localeRo from '../i18n/ro';
  // ru import localeRu from '../i18n/ru';
  // ruua import localeRuUA from '../i18n/ru-UA';
  // sk import localeSk from '../i18n/sk';
  // sr import localeSr from '../i18n/sr';
  // sv import localeSv from '../i18n/sv';
  // th import localeTh from '../i18n/th';
  // tr import localeTr from '../i18n/tr';
  // ua import localeUa from '../i18n/ua';
  // vi import localeVi from '../i18n/vi';
  // zh import localeZh from '../i18n/zh';




  var localeEn = {};



  var locale = {

  // ar   ar: localeAr,
  // bg   bg: localeBg,
  // ca   ca: localeCa,
  // cs   cs: localeCs,
  // da   da: localeDa,
  // de   de: localeDe,
  // el   el: localeEl,
    en: localeEn,
  // engb   'en-GB': localeEnGB,
  // es   es: localeEs,
  // fa   fa: localeFa,
  // fi   fi: localeFi,
  // fr   fr: localeFr,
  // he   he: localeHe,
  // hi   hi: localeHi,
  // hr   hr: localeHr,
  // hu   hu: localeHu,
  // it   it: localeIt,
  // ja   ja: localeJa,
  // ko   ko: localeKo,
  // lt   lt: localeLt,
  // nl   nl: localeNl,
  // no   no: localeNo,
  // pl   pl: localePl,
  // ptbr   'pt-BR': localePtBR,
  // ptpt   'pt-PT': localePtPT,
  // ro   ro: localeRo,
  // ru   ru: localeRu,
  // ruua   'ru-UA': localeRuUA,
  // sk   sk: localeSk,
  // sr   sr: localeSr,
  // sv   sv: localeSv,
  // th   th: localeTh,
  // tr   tr: localeTr,
  // ua   ua: localeUa,
  // vi   vi: localeVi,
  // zh   zh: localeZh,
  };

  var print = {
      init: function (inst) {
          inst.print = function (config) {
              if (win) {
                  inst._print = true;
                  inst.forceUpdate();
                  var printWin_1 = win.open('', 'Print', 'menubar=no,location=no,resizable=no,scrollbars=no,status=no,width=1024,height=1024');
                  var printDoc_1 = printWin_1.document;
                  // Add Base tag to the print document
                  var configOrDefault = config || {};
                  var baseUrl = configOrDefault.baseUrl || win.location.origin;
                  var base = printDoc_1.createElement('base');
                  base.setAttribute('href', baseUrl);
                  printDoc_1.head.appendChild(base);
                  var el_1 = inst._el;
                  var doc = getDocument(el_1);
                  // Set title
                  printDoc_1.title = doc.title;
                  // Copy all styles
                  var styles = doc.querySelectorAll('style,link');
                  var sMap_1 = new Map();
                  styles.forEach(function (style) {
                      if (style.nodeName.toLowerCase() !== 'link' || style.getAttribute('rel') === 'stylesheet') {
                          var styleClone_1 = cloneNode(printDoc_1, style);
                          sMap_1.set(styleClone_1, false);
                          styleClone_1.onload = function () { return sMap_1.delete(styleClone_1); };
                          printDoc_1.head.appendChild(styleClone_1);
                      }
                  });
                  // fix for third party styles that make the body disappear
                  printDoc_1.body.style.setProperty('display', 'block', 'important');
                  // Copy the element to print
                  var loading_1 = printDoc_1.createElement('div'); // printDoc.importNode(el, true);
                  loading_1.innerHTML = 'Loading...'; // in IE11 the cloning of the eventcalendar takes a while
                  printDoc_1.body.appendChild(loading_1);
                  var i_1 = 0;
                  var int_1 = setInterval(function () {
                      if (!sMap_1.size || i_1 > 300) {
                          clearInterval(int_1);
                          // clone the element
                          var elClone = cloneNode(printDoc_1, el_1);
                          printDoc_1.body.appendChild(elClone);
                          printDoc_1.body.removeChild(loading_1);
                          // ** Eventcalendar specific code **
                          // Restore horizontal scroll positions (timeline)
                          var tlScrollDiv = el_1.querySelector('.mbsc-timeline-grid-scroll');
                          if (tlScrollDiv) {
                              var tlScrollDivClone = elClone.querySelector('.mbsc-timeline-grid-scroll');
                              tlScrollDivClone.scrollLeft = tlScrollDiv.scrollLeft;
                          }
                          // Restore horizontal scroll positions (scheduler)
                          var scScrollDiv = el_1.querySelector('.mbsc-schedule-grid-scroll');
                          if (scScrollDiv) {
                              var scScrollDivClone = elClone.querySelector('.mbsc-schedule-grid-scroll');
                              scScrollDivClone.scrollLeft = scScrollDiv.scrollLeft;
                          }
                          // Open print dialog then close the window
                          printDoc_1.close();
                          setTimeout(function () {
                              // the browser needs time for rendering
                              printWin_1.focus();
                              printWin_1.print();
                              printWin_1.close();
                              inst._print = false;
                              inst.forceUpdate();
                          }, 100);
                      }
                      i_1++;
                  }, 10);
              }
          };
      },
  };
  /**
   * Creates a clone of a Node. Clones elements, text and attributes.
   * We need to use this, instead of the built in clone function of elements for IE11.
   * Reason: In IE11, elements created/cloned from a different document, won't be added to a new document
   * @param doc The document the Node needs to be added to
   * @param node The Original Node that needs to be cloned
   * @returns The cloned Node for a document
   */
  function cloneNode(doc, node) {
      var clone = null;
      if (node.nodeType === 1) {
          // Element
          clone = doc.createElement(node.nodeName);
          var attrs = getAllAttributes(node);
          attrs.forEach(function (attributeName) {
              var attrValue = node.getAttribute(attributeName);
              if (attrValue) {
                  clone.setAttribute(attributeName, attrValue);
              }
          });
      }
      else if (node.nodeType === 3) {
          // text
          clone = doc.createTextNode(node.nodeValue);
      }
      if (clone != null) {
          node.childNodes.forEach(function (child) {
              var childClone = cloneNode(doc, child);
              if (childClone) {
                  clone.appendChild(childClone);
              }
          });
      }
      return clone;
  }
  /** Returns an array of all attributes of a HTMLElement */
  function getAllAttributes(el) {
      var arr = [];
      var atts = el.attributes;
      // tslint:disable-next-line
      for (var i = 0; i < atts.length; i++) {
          arr.push(atts[i].name);
      }
      return arr;
  }

  var Base = /*#__PURE__*/ (function (_super) {
      __extends(Base, _super);
      function Base() {
          var _this = _super !== null && _super.apply(this, arguments) || this;
          _this._newProps = {};
          // tslint:disable-next-line: variable-name
          _this._setEl = function (el) {
              _this._el = el ? el._el || el : null;
          };
          return _this;
      }
      Object.defineProperty(Base.prototype, "value", {
          get: function () {
              return this.__value;
          },
          set: function (v) {
              this.__value = v;
          },
          enumerable: true,
          configurable: true
      });
      // tslint:enable variable-name
      Base.prototype.componentDidMount = function () {
          this.__init(); // For base class
          this._init();
          this._mounted();
          // this._hook('onMarkupReady', { target: this.base });
          this._updated();
          this._enhance();
      };
      Base.prototype.componentDidUpdate = function () {
          this._updated();
          this._enhance();
      };
      Base.prototype.componentWillUnmount = function () {
          this._destroy();
          this.__destroy(); // For base class
      };
      Base.prototype.render = function () {
          this._willUpdate();
          return this._template(this.s, this.state);
      };
      Base.prototype.getInst = function () {
          return this;
      };
      Base.prototype.setOptions = function (opt) {
          // this._newProps = {
          //   ...this._newProps as any,
          //   ...opt as any,
          // };
          // tslint:disable-next-line: forin
          for (var prop in opt) {
              this.props[prop] = opt[prop];
          }
          this.forceUpdate();
      };
      Base.prototype._safeHtml = function (html) {
          return { __html: html };
      };
      Base.prototype._init = function () { };
      Base.prototype.__init = function () { };
      Base.prototype._emit = function (name, args) { };
      Base.prototype._template = function (s, state) { };
      Base.prototype._mounted = function () { };
      Base.prototype._updated = function () { };
      Base.prototype._destroy = function () { };
      Base.prototype.__destroy = function () { };
      Base.prototype._willUpdate = function () { };
      Base.prototype._enhance = function () {
          var shouldEnhance = this._shouldEnhance;
          if (shouldEnhance) {
              enhance(shouldEnhance === true ? this._el : shouldEnhance);
              this._shouldEnhance = false;
          }
      };
      return Base;
  }(PureComponent));

  var guid$1 = 0;
  var BREAKPOINTS = {
      large: 992,
      medium: 768,
      small: 576,
      xlarge: 1200,
      xsmall: 0,
  };
  var isDark;
  if (isDarkQuery) {
      isDark = isDarkQuery.matches;
      // addListener is deprecated, however addEventListener does not have the necessary browser support
      // tslint:disable-next-line:deprecation
      isDarkQuery.addListener(function (ev) {
          isDark = ev.matches;
          globalChanges.next();
      });
  }
  /** @hidden */
  var BaseComponent = /*#__PURE__*/ (function (_super) {
      __extends(BaseComponent, _super);
      function BaseComponent() {
          var _this = _super !== null && _super.apply(this, arguments) || this;
          /** @hidden */
          _this.s = {};
          /** @hidden */
          _this.state = {};
          /**
           * Used to identify if it's a mobiscroll component
           * @hidden
           */
          _this._mbsc = true;
          /** @hidden */
          _this._v = {
              version: '5.26.2',
          };
          _this._uid = ++guid$1;
          return _this;
      }
      Object.defineProperty(BaseComponent.prototype, "nativeElement", {
          /** @hidden */
          get: function () {
              return this._el;
          },
          enumerable: true,
          configurable: true
      });
      /* TRIALFUNC */
      /** @hidden */
      // tslint:enable variable-name
      BaseComponent.prototype.destroy = function () { };
      /** @hidden */
      BaseComponent.prototype._hook = function (name, args) {
          var s = this.s;
          args.inst = this;
          args.type = name;
          if (s[name]) {
              return s[name](args, this);
          }
          this._emit(name, args);
      };
      BaseComponent.prototype.__init = function () {
          var _this = this;
          var self = this.constructor;
          // Subscribe only for top level components. Subcomponents get their settings from the top.
          // Checking the top level by the existence of static defaults property
          if (self.defaults) {
              this._optChange = globalChanges.subscribe(function () {
                  _this.forceUpdate();
              });
              // this.s.modules is not ready yet bc ngOnInit is called before ngDoCheck (when the first _merge is)
              var modules = this.props.modules;
              if (modules) {
                  for (var _i = 0, modules_1 = modules; _i < modules_1.length; _i++) {
                      var module = modules_1[_i];
                      if (module.init) {
                          module.init(this);
                      }
                  }
              }
          }
          this._hook('onInit', {});
      };
      BaseComponent.prototype.__destroy = function () {
          if (this._optChange !== UNDEFINED) {
              globalChanges.unsubscribe(this._optChange);
          }
          this._hook('onDestroy', {});
      };
      BaseComponent.prototype._render = function (s, state) {
          return;
      };
      BaseComponent.prototype._willUpdate = function () {
          this._merge();
          this._render(this.s, this.state);
      };
      BaseComponent.prototype._resp = function (s) {
          var resp = s.responsive;
          var ret;
          var br = -1;
          var width = this.state.width;
          // Default to 375 (a standard mobile view), if width is not yet calculated
          if (width === UNDEFINED) {
              width = 375;
          }
          if (resp && width) {
              for (var _i = 0, _a = Object.keys(resp); _i < _a.length; _i++) {
                  var key = _a[_i];
                  var value = resp[key];
                  var breakpoint = value.breakpoint || BREAKPOINTS[key];
                  if (width >= breakpoint && breakpoint > br) {
                      ret = value;
                      br = breakpoint;
                  }
              }
          }
          return ret;
      };
      BaseComponent.prototype._merge = function () {
          var self = this.constructor;
          var defaults = self.defaults;
          var context = this._opt || {};
          var props = {};
          var s;
          var themeDef;
          this._prevS = this.s || {};
          // TODO: don't merge if setState call
          if (defaults) {
              // Filter undefined values
              for (var prop in this.props) {
                  if (this.props[prop] !== UNDEFINED) {
                      props[prop] = this.props[prop];
                  }
              }
              // if (this._newProps) {
              //   for (const prop in this._newProps) {
              //     if (this._newProps[prop] !== UNDEFINED) {
              //       props[prop] = this._newProps[prop];
              //     }
              //   }
              // }
              // Load locale options
              var locale = props.locale || context.locale || options.locale || {};
              var calendarSystem = props.calendarSystem || locale.calendarSystem || context.calendarSystem || options.calendarSystem;
              // Load theme options
              var themeName = props.theme || context.theme || options.theme;
              var themeVariant = props.themeVariant || context.themeVariant || options.themeVariant;
              if (themeName === 'auto' || !themeName) {
                  themeName = autoDetect.theme || '';
              }
              // Set dark theme if:
              // - themeVariant is explicitly set to dark OR
              // - themeVariant is auto or not set, and system theme is dark
              // Also check if the theme exists in the themes object
              if ((themeVariant === 'dark' || (isDark && (themeVariant === 'auto' || !themeVariant))) && themes[themeName + '-dark']) {
                  themeName += '-dark';
              }
              // Write back the auto-detected theme
              props.theme = themeName;
              themeDef = themes[themeName];
              var theme = themeDef && themeDef[self._name];
              // Merge everything together
              s = __assign({}, defaults, theme, locale, options, context, calendarSystem, props);
              // Merge responsive options
              var resp = this._resp(s);
              this._respProps = resp;
              if (resp) {
                  s = __assign({}, s, resp);
              }
          }
          else {
              s = __assign({}, this.props);
              themeDef = themes[s.theme];
          }
          var baseTheme = themeDef && themeDef.baseTheme;
          s.baseTheme = baseTheme;
          this.s = s;
          this._className = s.cssClass || s.class || s.className || '';
          this._rtl = ' mbsc-' + (s.rtl ? 'rtl' : 'ltr');
          this._theme = ' mbsc-' + s.theme + (baseTheme ? ' mbsc-' + baseTheme : '');
          this._touchUi = s.touchUi === 'auto' || s.touchUi === UNDEFINED ? touchUi : s.touchUi;
          this._hb = os === 'ios' && (s.theme === 'ios' || baseTheme === 'ios') ? ' mbsc-hb' : '';
      };
      // tslint:disable variable-name
      /** @hidden */
      BaseComponent.defaults = UNDEFINED;
      BaseComponent._name = '';
      return BaseComponent;
  }(Base));

  // tslint:disable no-non-null-assertion
  // TODO: Add types and descriptions
  var REF_DATE = new Date(1970, 0, 1);
  var ONE_MIN = 60000;
  var ONE_HOUR = 60 * ONE_MIN;
  var ONE_DAY = 24 * ONE_HOUR;
  /**
   * Returns if a date object is a pseudo-date object
   * Pseudo-date objects are our implementation of a Date interface
   */
  function isMBSCDate(d) {
      return !!d._mbsc;
  }
  /**
   * Returns an ISO8601 date string in data timezone, if it's a date with timezone, otherwise the original date.
   * @param d The date to check.
   * @param s Options object containing timezone options.
   * @param tz Explicit timezone, if specified
   */
  function convertTimezone(d, s, tz) {
      var timezone = tz || s.dataTimezone || s.displayTimezone;
      var timezonePlugin = s.timezonePlugin;
      if (timezone && timezonePlugin && isMBSCDate(d)) {
          var clone = d.clone();
          clone.setTimezone(timezone);
          return clone.toISOString();
      }
      return d;
  }
  /** @hidden */
  var dateTimeLocale = {
      amText: 'am',
      dateFormat: 'MM/DD/YYYY',
      dateFormatLong: 'D DDD MMM YYYY',
      dayNames: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
      dayNamesMin: ['S', 'M', 'T', 'W', 'T', 'F', 'S'],
      dayNamesShort: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
      daySuffix: '',
      firstDay: 0,
      fromText: 'Start',
      getDate: adjustedDate,
      monthNames: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
      monthNamesShort: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
      monthSuffix: '',
      pmText: 'pm',
      separator: ' ',
      shortYearCutoff: '+10',
      timeFormat: 'h:mm A',
      toText: 'End',
      todayText: 'Today',
      weekText: 'Week {count}',
      yearSuffix: '',
      getMonth: function (d) {
          return d.getMonth();
      },
      getDay: function (d) {
          return d.getDate();
      },
      getYear: function (d) {
          return d.getFullYear();
      },
      getMaxDayOfMonth: function (y, m) {
          return 32 - new Date(y, m, 32, 12).getDate();
      },
      getWeekNumber: function (dt) {
          // Copy date so don't modify original
          var d = new Date(+dt);
          d.setHours(0, 0, 0);
          // Set to nearest Thursday: current date + 4 - current day number
          // Make Sunday's day number 7
          d.setDate(d.getDate() + 4 - (d.getDay() || 7));
          // Get first day of year
          var yearStart = new Date(d.getFullYear(), 0, 1);
          // Calculate full weeks to nearest Thursday
          return Math.ceil(((d - yearStart) / 86400000 + 1) / 7);
      },
  };
  // tslint:disable-next-line max-line-length
  var ISO_8601_FULL = /^(\d{4}|[+-]\d{6})(?:-?(\d{2})(?:-?(\d{2}))?)?(?:[T\s](\d{2}):?(\d{2})(?::?(\d{2})(?:\.(\d{3}))?)?((Z)|([+-])(\d{2})(?::?(\d{2}))?)?)?$/;
  var ISO_8601_TIME = /^((\d{2}):(\d{2})(?::(\d{2})(?:\.(\d{3}))?)?(?:(Z)|([+-])(\d{2})(?::?(\d{2}))?)?)?$/;
  /** @hidden */
  function setISOParts(parsed, offset, parts) {
      var part;
      var v;
      var p = { y: 1, m: 2, d: 3, h: 4, i: 5, s: 6, u: 7, tz: 8 };
      if (parts) {
          for (var _i = 0, _a = Object.keys(p); _i < _a.length; _i++) {
              part = _a[_i];
              v = parsed[p[part] - offset];
              if (v) {
                  parts[part] = part === 'tz' ? v : 1;
              }
          }
      }
  }
  /**
   * Returns the milliseconds of a date since midnight.
   * @hidden
   * @param d The date.
   */
  function getDayMilliseconds(d) {
      // We need to use a date where we don't have DST change
      var dd = new Date(1970, 0, 1, d.getHours(), d.getMinutes(), d.getSeconds(), d.getMilliseconds());
      return +dd - +REF_DATE;
  }
  /**
   * Checks if two date ranges are overlapping each other
   * @hidden
   * @param start1 start date of the first range
   * @param end1 end date of the first range
   * @param start2 start date of the second range
   * @param end2 end date of the second range
   * @param adjust if true, 0 length range will be modified to 1ms
   * @returns true if there is overlap false otherwise
   */
  function checkDateRangeOverlap(start1, end1, start2, end2, adjust) {
      var aStart = +start1;
      var bStart = +start2;
      var aEnd = adjust && aStart === +end1 ? +end1 + 1 : +end1;
      var bEnd = adjust && bStart === +end2 ? +end2 + 1 : +end2;
      return aStart < bEnd && aEnd > bStart;
  }
  /**
   * Returns the starting point of a day in display timezone
   * @param s
   * @param d
   * @returns
   */
  function getDayStart(s, d) {
      var newDate = createDate(s, d);
      newDate.setHours(0, 0, 0, 0);
      return newDate;
  }
  /**
   * Returns the last point of a day in display timezone
   * @param s
   * @param d
   * @returns
   */
  function getDayEnd(s, d) {
      var newDate = createDate(s, d);
      newDate.setHours(23, 59, 59, 999);
      return newDate;
  }
  /** @hidden */
  function getEndDate(s, allDay, start, end, isList) {
      var exclusive = allDay || isList ? s.exclusiveEndDates : true;
      var tzOpt = allDay ? UNDEFINED : s;
      return exclusive && start && end && start < end ? createDate(tzOpt, +end - 1) : end;
  }
  /** @hidden */
  function getDateStr(d) {
      return d.getFullYear() + '-' + pad(d.getMonth() + 1) + '-' + pad(d.getDate());
  }
  /** @hidden */
  function getDateOnly(d, nativeDate) {
      if (isMBSCDate(d) && !nativeDate) {
          return d.createDate(d.getFullYear(), d.getMonth(), d.getDate());
      }
      else {
          return adjustedDate(d.getFullYear(), d.getMonth(), d.getDate());
      }
  }
  /** @hidden */
  function getUTCDateOnly(d) {
      return Date.UTC(d.getFullYear(), d.getMonth(), d.getDate());
  }
  /**
   * Returns the difference in days for two dates
   * @hidden
   * @param d1 First date
   * @param d2 Second date
   * @returns Difference in days
   */
  function getDayDiff(d1, d2) {
      return round((getUTCDateOnly(d2) - getUTCDateOnly(d1)) / ONE_DAY);
  }
  /**
   * Returns the number of days between two dates if there are missing days between them
   * @hidden
   */
  function getGridDayDiff(from, to, startDay, endDay) {
      var dayIndex = -1;
      for (var d = getDateOnly(from); d <= getDateOnly(to); d.setDate(d.getDate() + 1)) {
          if (isInWeek(d.getDay(), startDay, endDay)) {
              dayIndex++;
          }
      }
      return dayIndex;
  }
  /**
   * Returns the date of the first day of the week for a given date
   * @hidden
   */
  function getFirstDayOfWeek(d, s, w) {
      var y = d.getFullYear(); // s.getYear(d);
      var m = d.getMonth(); // s.getMonth(d);
      var weekDay = d.getDay();
      var firstWeekDay = w === UNDEFINED ? s.firstDay : w;
      var offset = firstWeekDay - weekDay > 0 ? 7 : 0;
      return new Date(y, m, firstWeekDay - offset - weekDay + d.getDate());
  }
  /**
   * Checks if two dates are on the same date
   * @hidden
   * @param d1 First date
   * @param d2 Second date
   * @returns True or false
   */
  function isSameDay(d1, d2) {
      return d1.getFullYear() === d2.getFullYear() && d1.getMonth() === d2.getMonth() && d1.getDate() === d2.getDate();
  }
  /**
   * Check if 2 dates are in the same month (depends on the calendar system).
   * @param d1 First date.
   * @param d2 Second date.
   * @param s Settings containing the calendar system functions.
   */
  function isSameMonth(d1, d2, s) {
      return s.getYear(d1) === s.getYear(d2) && s.getMonth(d1) === s.getMonth(d2);
  }
  /** @hidden */
  function adjustedDate(y, m, d, h, i, s, u) {
      var date = new Date(y, m, d, h || 0, i || 0, s || 0, u || 0);
      if (date.getHours() === 23 && (h || 0) === 0) {
          date.setHours(date.getHours() + 2);
      }
      return date;
  }
  function isDate(d) {
      return d.getTime;
  }
  function isTime(d) {
      return isString(d) && ISO_8601_TIME.test(d);
  }
  /**
   * When a timezone plugin is specified, return a date with the same parts as the passed date (year, month, day, hour)
   * only with a timezone specified to display timezone
   * Otherwise it returns the same thing in the local timezone
   * @param s Settings object
   * @param d Date object
   * @returns
   */
  function addTimezone(s, d) {
      return createDate(s, d.getFullYear(), d.getMonth(), d.getDate(), d.getHours(), d.getMinutes(), d.getSeconds(), d.getMilliseconds());
  }
  /**
   * Returns a local date with the same year/month day/hours/minutes... as the original date in the parameter
   * It does not convert to any timezone, it just takes the date/hour/minute and creates a new local date from that
   * @param d Date with or without timezone data or null/undefined
   * @returns A new local Date object or undefined/null when nothing is pass as param
   */
  function removeTimezone(d) {
      if (!d) {
          return d;
      }
      else {
          return new Date(d.getFullYear(), d.getMonth(), d.getDate(), d.getHours(), d.getMinutes(), d.getSeconds(), d.getMilliseconds());
      }
  }
  function createDate(s, yearOrStamp, month, date, h, min, sec, ms) {
      if (yearOrStamp === null) {
          return null;
      }
      if (yearOrStamp && (isNumber(yearOrStamp) || isString(yearOrStamp)) && isUndefined(month)) {
          return makeDate(yearOrStamp, s);
      }
      if (s && s.timezonePlugin) {
          return s.timezonePlugin.createDate(s, yearOrStamp, month, date, h, min, sec, ms);
      }
      if (isObject(yearOrStamp)) {
          return new Date(yearOrStamp);
      }
      if (isUndefined(yearOrStamp)) {
          return new Date();
      }
      return new Date(yearOrStamp, month || 0, date || 1, h || 0, min || 0, sec || 0, ms || 0);
  }
  /** @hidden */
  // this should return a Date type or null, but it's fucking hard to make this work, so I give up
  // re: nice comment, but tslint gave an error about the line length, so I moved it above the function (@dioslaska).
  function makeDate(d, s, format, parts, skipTimezone) {
      var parse;
      if (isString(d)) {
          d = d.trim();
      }
      if (!d) {
          return null;
      }
      var plugin = s && s.timezonePlugin;
      if (plugin && !skipTimezone) {
          var parsedDate = isMBSCDate(d) ? d : plugin.parse(d, s);
          parsedDate.setTimezone(s.displayTimezone);
          return parsedDate;
      }
      // If already date object
      if (isDate(d)) {
          return d;
      }
      // Moment object
      if (d._isAMomentObject) {
          return d.toDate();
      }
      // timestamp
      if (isNumber(d)) {
          return new Date(d);
      }
      parse = ISO_8601_TIME.exec(d);
      var def = makeDate((s && s.defaultValue) || new Date());
      var defYear = def.getFullYear();
      var defMonth = def.getMonth();
      var defDay = def.getDate();
      // If ISO 8601 time string
      if (parse) {
          setISOParts(parse, 2, parts);
          return new Date(defYear, defMonth, defDay, parse[2] ? +parse[2] : 0, parse[3] ? +parse[3] : 0, parse[4] ? +parse[4] : 0, parse[5] ? +parse[5] : 0);
      }
      parse = ISO_8601_FULL.exec(d);
      // If ISO 8601 date string
      if (parse) {
          setISOParts(parse, 0, parts);
          return new Date(parse[1] ? +parse[1] : defYear, parse[2] ? parse[2] - 1 : defMonth, parse[3] ? +parse[3] : defDay, parse[4] ? +parse[4] : 0, parse[5] ? +parse[5] : 0, parse[6] ? +parse[6] : 0, parse[7] ? +parse[7] : 0);
      }
      // Parse date based on format
      return parseDate(format, d, s);
  }
  /**
   * Format a date into a string value with a specified format.
   * @param {string} format - Output format.
   * @param {Date} date - Date to format.
   * @param {IDatetimeProps} options - Locale options.
   * @returns {string} The formatted date string.
   */
  function formatDatePublic(format, date, options$1) {
      var s = __assign({}, dateTimeLocale, options.locale, options$1);
      return formatDate(format, date, s);
  }
  /**
   * Format a date into a string value with a specified format.
   * This is for inner usage, and it's faster than the one above, because it skips the option merge.
   * @param {string} format - Output format.
   * @param {Date} date - Date to format.
   * @param {IDatetimeProps} options - Locale options.
   * @returns {string} The formatted date string.
   */
  function formatDate(format, date, s) {
      // if (!date) {
      //   return null;
      // }
      var i;
      var year;
      var output = '';
      var literal = false;
      var c;
      // Counts how many times a symbol is repeated (0 if not repeated, 1 if its doubled, etc...)
      var peekAhead = function (symbol) {
          var nr = 0;
          var j = i;
          while (j + 1 < format.length && format.charAt(j + 1) === symbol) {
              nr++;
              j++;
          }
          return nr;
      };
      // Check whether a format character is doubled
      var lookAhead = function (symbol) {
          var nr = peekAhead(symbol);
          i += nr;
          return nr;
      };
      // Format a number, with leading zero if necessary
      var formatNumber = function (symbol, val, len) {
          var ret = '' + val;
          if (lookAhead(symbol)) {
              while (ret.length < len) {
                  ret = '0' + ret;
              }
          }
          return ret;
      };
      // Format a name, short or long as requested
      var formatName = function (symbol, val, short, long) {
          return lookAhead(symbol) === 3 ? long[val] : short[val];
      };
      for (i = 0; i < format.length; i++) {
          if (literal) {
              if (format.charAt(i) === "'" && !lookAhead("'")) {
                  literal = false;
              }
              else {
                  output += format.charAt(i);
              }
          }
          else {
              switch (format.charAt(i)) {
                  case 'D':
                      c = peekAhead('D');
                      if (c > 1) {
                          output += formatName('D', date.getDay(), s.dayNamesShort, s.dayNames);
                      }
                      else {
                          output += formatNumber('D', s.getDay(date), 2);
                      }
                      break;
                  case 'M':
                      c = peekAhead('M');
                      if (c > 1) {
                          output += formatName('M', s.getMonth(date), s.monthNamesShort, s.monthNames);
                      }
                      else {
                          output += formatNumber('M', s.getMonth(date) + 1, 2);
                      }
                      break;
                  case 'Y':
                      year = s.getYear(date);
                      output += lookAhead('Y') === 3 ? year : (year % 100 < 10 ? '0' : '') + (year % 100);
                      break;
                  case 'h': {
                      var h = date.getHours();
                      output += formatNumber('h', h > 12 ? h - 12 : h === 0 ? 12 : h, 2);
                      break;
                  }
                  case 'H':
                      output += formatNumber('H', date.getHours(), 2);
                      break;
                  case 'm':
                      output += formatNumber('m', date.getMinutes(), 2);
                      break;
                  case 's':
                      output += formatNumber('s', date.getSeconds(), 2);
                      break;
                  case 'a':
                      output += date.getHours() > 11 ? s.pmText : s.amText;
                      break;
                  case 'A':
                      output += date.getHours() > 11 ? s.pmText.toUpperCase() : s.amText.toUpperCase();
                      break;
                  case "'":
                      if (lookAhead("'")) {
                          output += "'";
                      }
                      else {
                          literal = true;
                      }
                      break;
                  default:
                      output += format.charAt(i);
              }
          }
      }
      return output;
  }
  /**
   * Extract a date from a string value with a specified format.
   * @param {string} format Input format.
   * @param {string} value String to parse.
   * @param {IDatetimeProps} options Locale options
   * @return {Date} Returns the extracted date or defaults to now if no format or no value is given
   */
  function parseDate(format, value, options) {
      var s = __assign({}, dateTimeLocale, options);
      var def = makeDate(s.defaultValue || new Date());
      if (!value) {
          return def;
      }
      if (!format) {
          format = s.dateFormat + s.separator + s.timeFormat;
      }
      var shortYearCutoff = s.shortYearCutoff;
      var year = s.getYear(def);
      var month = s.getMonth(def) + 1;
      // let doy = -1,
      var day = s.getDay(def);
      var hours = def.getHours();
      var minutes = def.getMinutes();
      var seconds = 0; // def.getSeconds()
      var ampm = -1;
      var literal = false;
      var iValue = 0;
      var iFormat;
      /**
       * Counts how many times a symbol is repeated (0 if not repeated, 1 if its doubled, etc...)
       * without moving the index forward
       */
      var peekAhead = function (symbol) {
          var nr = 0;
          var j = iFormat;
          while (j + 1 < format.length && format.charAt(j + 1) === symbol) {
              nr++;
              j++;
          }
          return nr;
      };
      /**
       * Check whether a format character is doubled
       * Check how many times a format character is repeated. Also move the index forward.
       */
      var lookAhead = function (match) {
          var matches = peekAhead(match);
          iFormat += matches;
          return matches;
      };
      /**
       * Extract a number from the string value
       * @param {string} match The current symbol in the format string
       * @returns {number} The extracted number
       */
      var getNumber = function (match) {
          var count = lookAhead(match);
          // const size = count === 3 ? 4 : 2; // size is either 4 digit (year) or a maximum 2 digit number
          var size = count >= 2 ? 4 : 2;
          var digits = new RegExp('^\\d{1,' + size + '}');
          var num = value.substr(iValue).match(digits);
          if (!num) {
              return 0;
          }
          iValue += num[0].length;
          return parseInt(num[0], 10);
      };
      /**
       * Extracts a name from the string value and converts to an index
       * @param {string} match The symbol we are looking at in the format string
       * @param {Array<string>} shortNames Short names array
       * @param {Array<string>} longNames Long names array
       * @returns {number} Returns the index + 1 of the name in the names array if found, 0 otherwise
       */
      var getName = function (match, shortNames, longNames) {
          var count = lookAhead(match);
          var names = count === 3 ? longNames : shortNames;
          for (var i = 0; i < names.length; i++) {
              if (value.substr(iValue, names[i].length).toLowerCase() === names[i].toLowerCase()) {
                  iValue += names[i].length;
                  return i + 1;
              }
          }
          return 0;
      };
      var checkLiteral = function () {
          iValue++;
      };
      for (iFormat = 0; iFormat < format.length; iFormat++) {
          if (literal) {
              if (format.charAt(iFormat) === "'" && !lookAhead("'")) {
                  literal = false;
              }
              else {
                  checkLiteral();
              }
          }
          else {
              switch (format.charAt(iFormat)) {
                  case 'Y':
                      year = getNumber('Y');
                      break;
                  case 'M': {
                      var p = peekAhead('M');
                      if (p < 2) {
                          month = getNumber('M');
                      }
                      else {
                          month = getName('M', s.monthNamesShort, s.monthNames);
                      }
                      break;
                  }
                  case 'D': {
                      var p = peekAhead('D');
                      if (p < 2) {
                          day = getNumber('D');
                      }
                      else {
                          getName('D', s.dayNamesShort, s.dayNames);
                      }
                      break;
                  }
                  case 'H':
                      hours = getNumber('H');
                      break;
                  case 'h':
                      hours = getNumber('h');
                      break;
                  case 'm':
                      minutes = getNumber('m');
                      break;
                  case 's':
                      seconds = getNumber('s');
                      break;
                  case 'a':
                      ampm = getName('a', [s.amText, s.pmText], [s.amText, s.pmText]) - 1;
                      break;
                  case 'A':
                      ampm = getName('A', [s.amText, s.pmText], [s.amText, s.pmText]) - 1;
                      break;
                  case "'":
                      if (lookAhead("'")) {
                          checkLiteral();
                      }
                      else {
                          literal = true;
                      }
                      break;
                  default:
                      checkLiteral();
              }
          }
      }
      if (year < 100) {
          var cutoffYear = void 0;
          // Cut off year setting supports string and number. When string, it is considered relative to the current year,
          // otherwise it is the year number in the current century
          if (!isString(shortYearCutoff)) {
              cutoffYear = +shortYearCutoff;
          }
          else {
              cutoffYear = (new Date().getFullYear() % 100) + parseInt(shortYearCutoff, 10);
          }
          var addedCentury = void 0;
          if (year <= cutoffYear) {
              addedCentury = 0;
          }
          else {
              addedCentury = -100;
          }
          year += new Date().getFullYear() - (new Date().getFullYear() % 100) + addedCentury;
      }
      hours = ampm === -1 ? hours : ampm && hours < 12 ? hours + 12 : !ampm && hours === 12 ? 0 : hours;
      var date = s.getDate(year, month - 1, day, hours, minutes, seconds);
      if (s.getYear(date) !== year || s.getMonth(date) + 1 !== month || s.getDay(date) !== day) {
          return def; // Invalid date
      }
      return date;
  }
  /**
   * Clones a date object (native or custom mbsc date).
   * @param date The date to clone.
   */
  function cloneDate(date) {
      return isMBSCDate(date) ? date.clone() : new Date(date);
  }
  /**
   * Adds the specified number of days to a date. Returns a new date object.
   * @param date The date.
   * @param days Days to add.
   */
  function addDays(date, days) {
      var copy = cloneDate(date);
      copy.setDate(copy.getDate() + days);
      return copy;
  }
  /**
   * Adds the specified number of days to a date. Returns a new date object.
   * @param date The date.
   * @param months Days to add.
   * @param s
   */
  function addMonths(date, months, s) {
      var year = s.getYear(date);
      var month = s.getMonth(date) + months;
      var maxDays = s.getMaxDayOfMonth(year, month);
      return addTimezone(s, s.getDate(year, month, Math.min(s.getDay(date), maxDays), date.getHours(), date.getMinutes(), date.getSeconds(), date.getMilliseconds()));
  }
  /**
   * Check if a day is inside the visible week days.
   * @param day Weekday to check.
   * @param startDay Start day of the week.
   * @param endDay End day of the week.
   */
  function isInWeek(day, startDay, endDay) {
      return startDay > endDay ? day <= endDay || day >= startDay : day >= startDay && day <= endDay;
  }
  /**
   * Rounds a date to the specified minute step.
   * @param date The date to round.
   * @param step Step specified as minutes.
   */
  function roundTime(date, step) {
      var ms = ONE_MIN * step;
      var copy = cloneDate(date).setHours(0, 0, 0, 0);
      var rounded = copy + Math.round((+date - +copy) / ms) * ms;
      return isMBSCDate(date) ? date.createDate(rounded) : new Date(rounded);
  }
  // Symbol dummy for IE11
  if (isBrowser && typeof Symbol === 'undefined') {
      window.Symbol = {
          toPrimitive: 'toPrimitive',
      };
  }
  util.datetime = {
      formatDate: formatDatePublic,
      parseDate: parseDate,
  };

  // tslint:disable no-non-null-assertion
  // tslint:disable no-inferrable-types
  var WEEK_DAYNAMES = { 0: 'SU', 1: 'MO', 2: 'TU', 3: 'WE', 4: 'TH', 5: 'FR', 6: 'SA' };
  var WEEK_DAYS = { SU: 0, MO: 1, TU: 2, WE: 3, TH: 4, FR: 5, SA: 6 };
  var RULE_KEY_MAP = {
      byday: 'weekDays',
      bymonth: 'month',
      bymonthday: 'day',
      bysetpos: 'pos',
      dtstart: 'from',
      freq: 'repeat',
      wkst: 'weekStart',
  };
  /** @hidden */
  function addMultiDayEvent(obj, item, s, overwrite) {
      var start = makeDate(item.start, item.allDay ? UNDEFINED : s);
      var end = makeDate(item.end, item.allDay ? UNDEFINED : s);
      var duration = end - start;
      if (overwrite) {
          item.start = start;
          item.end = end;
      }
      start = getDateOnly(start);
      end = s.exclusiveEndDates ? end : getDateOnly(addDays(end, 1));
      // If event has no duration, it should still be added to the start day
      while (start < end || !duration) {
          addToList(obj, start, item);
          start = addDays(start, 1);
          duration = 1;
      }
  }
  /** @hidden */
  function addToList(obj, d, data) {
      var key = getDateStr(d);
      if (!obj[key]) {
          obj[key] = [];
          // Stored the date object on the array for performance reasons, so we don't have to parse it again later
          // TODO: do this with proper types
          obj[key].date = getDateOnly(d, true);
      }
      obj[key].push(data);
  }
  /** @hidden */
  function getExceptionDateMap(dtStart, start, end, s, exception, exceptionRule) {
      var map = {};
      if (exception) {
          var exceptionDates = getExceptionList(exception);
          for (var _i = 0, exceptionDates_1 = exceptionDates; _i < exceptionDates_1.length; _i++) {
              var e = exceptionDates_1[_i];
              map[getDateStr(makeDate(e))] = true;
          }
      }
      if (exceptionRule) {
          // Get exception date list from the rule
          var exceptionDateList = getOccurrences(exceptionRule, dtStart, dtStart, start, end, s);
          for (var _a = 0, exceptionDateList_1 = exceptionDateList; _a < exceptionDateList_1.length; _a++) {
              var ex = exceptionDateList_1[_a];
              map[getDateStr(ex.d)] = true;
          }
      }
      return map;
  }
  /** @hidden */
  function getDateFromItem(item) {
      // If the item is a string, Date, or moment object, it's directly the date (e.g. in case of invalid setting),
      // otherwise check the d or start attributes
      return isString(item) || item.getTime || item.toDate ? item : item.start || item.date;
  }
  /** @hidden */
  function getTzOpt(s, event, dataAsDisplay) {
      var origStart = event.original ? event.original.start : event.start;
      var allDay = event.allDay || !origStart;
      var tz = s.timezonePlugin;
      var dataTimezone = event.timezone || s.dataTimezone || s.displayTimezone;
      return tz && !allDay
          ? {
              dataTimezone: dataTimezone,
              displayTimezone: dataAsDisplay ? dataTimezone : s.displayTimezone,
              timezonePlugin: tz,
          }
          : UNDEFINED;
  }
  /** @hidden */
  function parseRule(ruleStr) {
      var rule = {};
      var pairs = ruleStr.split(';');
      for (var _i = 0, pairs_1 = pairs; _i < pairs_1.length; _i++) {
          var pair = pairs_1[_i];
          var values = pair.split('=');
          var key = values[0].trim().toLowerCase();
          var value = values[1].trim();
          rule[RULE_KEY_MAP[key] || key] = value;
      }
      return rule;
  }
  /** @hidden */
  function getRule(rule) {
      return isString(rule) ? parseRule(rule) : __assign({}, rule);
  }
  /**
   * Updates a recurring rule, based on a new start date and old start date.
   * @param recurringRule
   * @param newStart
   * @param oldStart
   */
  function updateRecurringRule(recurringRule, newStart, oldStart) {
      var updatedRule = getRule(recurringRule);
      var newStartDate = makeDate(newStart);
      var oldStartDate = makeDate(oldStart);
      var dayDelta = getDayDiff(oldStartDate, newStartDate);
      var repeat = (updatedRule.repeat || '').toLowerCase();
      var updateArray = function (values, oldValue, newValue) {
          var newValues = values.filter(function (value) { return value !== oldValue; });
          if (newValues.indexOf(newValue) === -1) {
              newValues.push(newValue);
          }
          return newValues;
      };
      var updateNumber = function (values, oldValue, newValue) {
          var oldValues = isArray(values) ? values : ((values || '') + '').split(',').map(function (nr) { return +nr; });
          var newValues = updateArray(oldValues, oldValue, newValue);
          return newValues.length > 1 ? newValues : newValues[0];
      };
      var updateWeekDays = function () {
          if (updatedRule.weekDays) {
              var oldWeekDays = updatedRule.weekDays.split(',');
              // if (oldValues.length > 1) {
              var oldWeekDay = WEEK_DAYNAMES[oldStartDate.getDay()];
              var newWeekDay = WEEK_DAYNAMES[newStartDate.getDay()];
              var newWeekDays = updateArray(oldWeekDays, oldWeekDay, newWeekDay);
              // const newValues = oldValues.filter((day: string) => day !== oldValue);
              // if (newValues.indexOf(newValue) === -1) {
              //   newValues.push(newValue);
              // }
              // } else {
              //   // Shift all values in the week days array with dayDelta
              //   newValues = oldValues.map((day: string) => {
              //     const dayIndex = WEEK_DAYS[day.trim()];
              //     const delta = dayDelta % 7;
              //     const newDayIndex = dayIndex + delta + (dayIndex + delta < 0 ? 7 : 0);
              //     return WEEK_DAYNAMES[newDayIndex % 7];
              //   });
              // }
              updatedRule.weekDays = newWeekDays.join();
          }
      };
      if (repeat === 'weekly') {
          updateWeekDays();
      }
      else if (repeat === 'monthly') {
          if (updatedRule.pos === UNDEFINED) {
              updatedRule.day = updateNumber(updatedRule.day, oldStartDate.getDate(), newStartDate.getDate());
          }
          else {
              updateWeekDays();
          }
      }
      else if (repeat === 'yearly') {
          if (updatedRule.pos === UNDEFINED) {
              updatedRule.month = updateNumber(updatedRule.month, oldStartDate.getMonth() + 1, newStartDate.getMonth() + 1);
              updatedRule.day = updateNumber(updatedRule.day, oldStartDate.getDate(), newStartDate.getDate());
          }
          else {
              updateWeekDays();
          }
      }
      if (updatedRule.from) {
          updatedRule.from = addDays(makeDate(updatedRule.from), dayDelta);
      }
      if (updatedRule.until) {
          updatedRule.until = addDays(makeDate(updatedRule.until), dayDelta);
      }
      return updatedRule;
  }
  /**
   * Updates a recurring event, returns the updated and the new event.
   * @param originalRecurringEvent The original event to update.
   * @param oldEventOccurrence The original event occurrence in case of d&d (what is dragged).
   * @param newEvent The created even in case of d&d (where is dragged).
   * @param updatedEvent The updated event from popup.
   * @param updateMode The update type.
   */
  function updateRecurringEvent(originalRecurringEvent, oldEventOccurrence, newEvent, updatedEvent, updateMode, timezone, timezonePlugin) {
      var retUpdatedEvent = __assign({}, originalRecurringEvent);
      var retNewEvent = null;
      var newStart = newEvent && newEvent.start;
      var newEnd = newEvent && newEvent.end;
      var newRule;
      var oldStart = oldEventOccurrence && oldEventOccurrence.start;
      var originalRule = getRule(originalRecurringEvent.recurring);
      switch (updateMode) {
          case 'following':
              if (updatedEvent) {
                  // edit from popup
                  if (updatedEvent.recurring) {
                      newRule = getRule(updatedEvent.recurring);
                  }
                  retNewEvent = __assign({}, updatedEvent);
                  delete retNewEvent.id;
              }
              else if (newStart && oldStart) {
                  // drag & drop
                  newRule = updateRecurringRule(originalRule, newStart, oldStart);
                  retNewEvent = __assign({}, newEvent);
              }
              // set the hours to 00:00
              originalRule.until = getDateStr(makeDate(oldStart));
              if (originalRule.count) {
                  var oldNr = (oldEventOccurrence && oldEventOccurrence.nr) || 0;
                  if (newRule) {
                      newRule.count = originalRule.count - oldNr;
                  }
                  originalRule.count = oldNr;
              }
              if (newStart && newRule) {
                  newRule.from = newStart;
              }
              if (retNewEvent && newRule) {
                  retNewEvent.recurring = newRule;
              }
              retUpdatedEvent.recurring = originalRule;
              break;
          case 'all':
              if (updatedEvent) {
                  // edit from popup
                  newStart = updatedEvent.start;
                  newEnd = updatedEvent.end;
                  retUpdatedEvent = __assign({}, updatedEvent);
              }
              else if (newEvent && newStart && newEnd && oldStart) {
                  // drag & drop
                  retUpdatedEvent.allDay = newEvent.allDay;
                  retUpdatedEvent.recurring = updateRecurringRule(originalRule, newStart, oldStart);
              }
              if (newStart && newEnd) {
                  var tzOpt = timezone && timezonePlugin ? { displayTimezone: timezone, timezonePlugin: timezonePlugin } : UNDEFINED;
                  var tzOpt1 = retUpdatedEvent.allDay ? UNDEFINED : tzOpt;
                  var tzOpt2 = originalRecurringEvent.allDay ? UNDEFINED : tzOpt;
                  var start = makeDate(newStart, tzOpt1);
                  var end = makeDate(newEnd, tzOpt1);
                  var origStart = originalRecurringEvent.start;
                  var origEnd = originalRecurringEvent.end;
                  var allDayChange = originalRecurringEvent.allDay && !retUpdatedEvent.allDay;
                  var origStartDate = origStart && makeDate(origStart, tzOpt2);
                  var oldStartDate = oldStart && makeDate(oldStart, tzOpt2);
                  var duration = end - start;
                  var delta = oldStartDate ? start - oldStartDate : 0;
                  var updatedStart = origStartDate && oldStartDate ? createDate(tzOpt1, +origStartDate + delta) : start;
                  var updatedEnd = createDate(tzOpt1, +updatedStart + duration);
                  if (isTime(origStart) || (!origStart && allDayChange)) {
                      // Set the time only
                      retUpdatedEvent.start = formatDatePublic('HH:mm', start);
                  }
                  else if (origStart) {
                      retUpdatedEvent.start = tzOpt1 ? updatedStart.toISOString() : updatedStart;
                  }
                  if (isTime(origEnd) || (!origEnd && allDayChange)) {
                      // Set the time only
                      retUpdatedEvent.end = formatDatePublic('HH:mm', end);
                  }
                  else if (origEnd) {
                      retUpdatedEvent.end = tzOpt1 ? updatedEnd.toISOString() : updatedEnd;
                  }
              }
              break;
          default: {
              var originalException = originalRecurringEvent.recurringException;
              var exception = isArray(originalException) ? originalException.slice() : originalException ? [originalException] : [];
              if (oldStart) {
                  exception.push(oldStart);
              }
              retUpdatedEvent.recurringException = exception;
              // from popup or drag & drop
              retNewEvent = updatedEvent || newEvent;
              break;
          }
      }
      return { updatedEvent: retUpdatedEvent, newEvent: retNewEvent };
  }
  /** @hidden */
  function getExceptionList(exception) {
      if (exception) {
          if (isArray(exception)) {
              return exception;
          }
          if (isString(exception)) {
              return exception.split(',');
          }
          return [exception];
      }
      return [];
  }
  /** @hidden */
  function getOccurrences(rule, dtStart, origStart, start, end, s, exception, exceptionRule, returnOccurrence) {
      if (isString(rule)) {
          rule = parseRule(rule);
      }
      var getYear = s.getYear;
      var getMonth = s.getMonth;
      var getDay = s.getDay;
      var getDate = s.getDate;
      var getMaxDays = s.getMaxDayOfMonth;
      var freq = (rule.repeat || '').toLowerCase();
      var interval = rule.interval || 1;
      var count = rule.count;
      // the staring point of the current rule
      var from = rule.from ? makeDate(rule.from) : dtStart || (interval !== 1 || count !== UNDEFINED ? new Date() : start);
      var fromDate = getDateOnly(from);
      var fromYear = getYear(from);
      var fromMonth = getMonth(from);
      var fromDay = getDay(from);
      var origHours = origStart ? origStart.getHours() : 0;
      var origMinutes = origStart ? origStart.getMinutes() : 0;
      var origSeconds = origStart ? origStart.getSeconds() : 0;
      var until = rule.until ? makeDate(rule.until) : Infinity;
      var occurredBefore = from < start;
      var rangeStart = occurredBefore ? start : getDateOnly(from);
      var firstOnly = returnOccurrence === 'first';
      var lastOnly = returnOccurrence === 'last';
      var rangeEnd = firstOnly || lastOnly || !end ? until : until < end ? until : end;
      var countOrInfinity = count === UNDEFINED ? Infinity : count;
      var weekDays = (rule.weekDays || WEEK_DAYNAMES[from.getDay()]).split(',');
      var weekStart = WEEK_DAYS[(rule.weekStart || 'MO').trim().toUpperCase()];
      var days = isArray(rule.day) ? rule.day : ((rule.day || fromDay) + '').split(',');
      var months = isArray(rule.month) ? rule.month : ((rule.month || fromMonth + 1) + '').split(',');
      var occurrences = [];
      var hasPos = rule.pos !== UNDEFINED;
      var pos = hasPos ? +rule.pos : 1;
      var weekDayValues = [];
      var exceptionDateMap = end ? getExceptionDateMap(dtStart, start, end, s, exception, exceptionRule) : {};
      var first;
      var iterator;
      var repeat = true;
      var i = 0;
      var nr = 0;
      var closest = null;
      var latest = start;
      for (var _i = 0, weekDays_1 = weekDays; _i < weekDays_1.length; _i++) {
          var weekDay = weekDays_1[_i];
          weekDayValues.push(WEEK_DAYS[weekDay.trim().toUpperCase()]);
      }
      var handleOccurrence = function () {
          // If end is not specified, get the exception dates for the current day
          if (!end) {
              exceptionDateMap = getExceptionDateMap(iterator, iterator, addDays(iterator, 1), s, exception, exceptionRule);
          }
          // Check that it's not an exception date and it's after the start of the range
          if (!exceptionDateMap[getDateStr(iterator)] && iterator >= rangeStart) {
              if (firstOnly) {
                  // if it is closer to the start than the current one, stop looking further
                  closest = !closest || iterator < closest ? iterator : closest;
                  repeat = false;
              }
              else if (lastOnly) {
                  var diff = getDayDiff(latest, iterator);
                  latest = iterator > latest && diff <= 1 ? iterator : latest;
                  repeat = diff <= 1;
              }
              else {
                  occurrences.push({ d: iterator, i: nr });
              }
          }
          nr++;
      };
      var handlePos = function (monthFirstDay, monthLastDay) {
          var matches = [];
          for (var _i = 0, weekDayValues_1 = weekDayValues; _i < weekDayValues_1.length; _i++) {
              var weekDay = weekDayValues_1[_i];
              var startWeekDay = getFirstDayOfWeek(monthFirstDay, { firstDay: weekDay });
              for (var d = startWeekDay; d < monthLastDay; d.setDate(d.getDate() + 7)) {
                  if (d.getMonth() === monthFirstDay.getMonth()) {
                      matches.push(+d);
                  }
              }
          }
          matches.sort();
          var match = matches[pos < 0 ? matches.length + pos : pos - 1];
          iterator = match ? new Date(match) : monthLastDay;
          iterator = getDate(getYear(iterator), getMonth(iterator), getDay(iterator), origHours, origMinutes, origSeconds);
          if (iterator <= rangeEnd) {
              if (match) {
                  handleOccurrence();
              }
          }
          else {
              repeat = false;
          }
      };
      switch (freq) {
          case 'daily':
              nr = count && occurredBefore ? floor(getDayDiff(from, start) / interval) : 0;
              while (repeat) {
                  iterator = getDate(fromYear, fromMonth, fromDay + nr * interval, origHours, origMinutes, origSeconds);
                  if (iterator <= rangeEnd && nr < countOrInfinity) {
                      handleOccurrence();
                  }
                  else {
                      repeat = false;
                  }
              }
              break;
          case 'weekly': {
              // const nrByDay: { [key: number]: number } = {};
              var sortedDays = weekDayValues;
              var fromFirstWeekDay = getFirstDayOfWeek(from, { firstDay: weekStart });
              var fromWeekDay_1 = fromFirstWeekDay.getDay();
              // const startFirstWeekDay = getFirstDayOfWeek(start, { firstDay: weekStart });
              // Sort week day numbers to start with from day
              sortedDays.sort(function (a, b) {
                  a = a - fromWeekDay_1;
                  a = a < 0 ? a + 7 : a;
                  b = b - fromWeekDay_1;
                  b = b < 0 ? b + 7 : b;
                  return a - b;
              });
              // TODO: the calculation below is not always correct, and leads to skipping occurrences in the actual range
              // Calculate how many times the event occurred before the start date of the range
              // if (occurredBefore && count === UNDEFINED) {
              //   const daysNr = floor(getDayDiff(fromFirstWeekDay, startFirstWeekDay));
              //   for (const weekDay of sortedDays) {
              //     let temp = floor(daysNr / (7 * interval));
              //     if (weekDay < from.getDay()) {
              //       temp--;
              //     }
              //     if (weekDay < start.getDay()) {
              //       temp++;
              //     }
              //     nrByDay[weekDay] = temp;
              //     nr += temp;
              //   }
              // }
              while (repeat) {
                  for (var _a = 0, sortedDays_1 = sortedDays; _a < sortedDays_1.length; _a++) {
                      var weekDay = sortedDays_1[_a];
                      first = addDays(fromFirstWeekDay, weekDay < weekStart ? weekDay - weekStart + 7 : weekDay - weekStart);
                      // iterator = getDate(getYear(first), getMonth(first), getDay(first) + ((nrByDay[weekDay] || 0) + i) * 7 * interval);
                      iterator = getDate(getYear(first), getMonth(first), getDay(first) + i * 7 * interval, origHours, origMinutes, origSeconds);
                      if (iterator <= rangeEnd && nr < countOrInfinity) {
                          if (iterator >= fromDate) {
                              handleOccurrence();
                          }
                      }
                      else {
                          repeat = false;
                      }
                  }
                  i++;
              }
              break;
          }
          case 'monthly':
              // TODO: calculate occurrences before start instead of iterating through all
              while (repeat) {
                  var maxDays = getMaxDays(fromYear, fromMonth + i * interval);
                  if (hasPos) {
                      var monthFirstDay = getDate(fromYear, fromMonth + i * interval, 1);
                      var monthLastDay = getDate(fromYear, fromMonth + i * interval + 1, 1);
                      handlePos(monthFirstDay, monthLastDay);
                  }
                  else {
                      for (var _b = 0, days_1 = days; _b < days_1.length; _b++) {
                          var d = days_1[_b];
                          var day = +d;
                          iterator = getDate(fromYear, fromMonth + i * interval, day < 0 ? maxDays + day + 1 : day, origHours, origMinutes, origSeconds);
                          if (iterator <= rangeEnd && nr < countOrInfinity) {
                              if (maxDays >= d && iterator >= fromDate) {
                                  handleOccurrence();
                              }
                          }
                          else {
                              repeat = false;
                          }
                      }
                  }
                  i++;
              }
              break;
          case 'yearly':
              // TODO: calculate occurrences before start instead of iterating through all
              while (repeat) {
                  for (var _c = 0, months_1 = months; _c < months_1.length; _c++) {
                      var m = months_1[_c];
                      var month = +m;
                      var maxDays = getMaxDays(fromYear + i * interval, month - 1);
                      if (hasPos) {
                          var monthFirstDay = getDate(fromYear + i * interval, month - 1, 1);
                          var monthLastDay = getDate(fromYear + i * interval, month, 1);
                          handlePos(monthFirstDay, monthLastDay);
                      }
                      else {
                          for (var _d = 0, days_2 = days; _d < days_2.length; _d++) {
                              var d = days_2[_d];
                              var day = +d;
                              iterator = getDate(fromYear + i * interval, month - 1, day < 0 ? maxDays + day + 1 : day, origHours, origMinutes, origSeconds);
                              if (iterator <= rangeEnd && nr < countOrInfinity) {
                                  if (maxDays >= d && iterator >= fromDate) {
                                      handleOccurrence();
                                  }
                              }
                              else {
                                  repeat = false;
                              }
                          }
                      }
                  }
                  i++;
              }
              break;
      }
      return firstOnly ? closest : lastOnly ? latest : occurrences;
  }
  /** @hidden */
  function getEventMap(list, start, end, s, overwrite) {
      var obj = {};
      if (!list) {
          return UNDEFINED;
      }
      for (var _i = 0, list_3 = list; _i < list_3.length; _i++) {
          var item = list_3[_i];
          var tzOpt = getTzOpt(s, item, true);
          var tzOptDisplay = getTzOpt(s, item);
          var d = getDateFromItem(item);
          var dt = makeDate(d, tzOptDisplay);
          if (item.recurring) {
              // Use a timezone-less start for getting the occurrences, since getOccurrences does not use timezones
              var dtStart = ISO_8601_TIME.test(d) ? null : makeDate(d);
              var origStart = createDate(tzOpt, dt);
              var oEnd = item.end ? makeDate(item.end, tzOpt) : origStart;
              var origEnd = item.end === '00:00' ? addDays(oEnd, 1) : oEnd;
              var duration = +origEnd - +origStart;
              // We need to extend the range with 1-1 days, because
              // start/end is in local timezone, but data is in data timezone.
              // We cannot convert start/end to data timezone, because the time part is not relevant here.
              var from = addDays(start, -1);
              var until = addDays(end, 1);
              var dates = getOccurrences(item.recurring, dtStart, origStart, from, until, s, item.recurringException, item.recurringExceptionRule);
              for (var _a = 0, dates_1 = dates; _a < dates_1.length; _a++) {
                  var occurrence = dates_1[_a];
                  var date = occurrence.d;
                  // For each occurrence create a clone of the event
                  var clone = __assign({}, item);
                  // Modify the start/end dates for the occurrence
                  if (item.start) {
                      clone.start = createDate(tzOpt, date.getFullYear(), date.getMonth(), date.getDate(), origStart.getHours(), origStart.getMinutes(), origStart.getSeconds());
                  }
                  else {
                      clone.allDay = true;
                      clone.start = createDate(UNDEFINED, date.getFullYear(), date.getMonth(), date.getDate());
                  }
                  if (item.end) {
                      if (item.allDay) {
                          // In case of all-day events keep the length in days, end set the original time for the end day
                          var endDay = addDays(date, getDayDiff(origStart, origEnd));
                          clone.end = new Date(endDay.getFullYear(), endDay.getMonth(), endDay.getDate(), origEnd.getHours(), origEnd.getMinutes(), origEnd.getSeconds());
                      }
                      else {
                          // In case of non all-day events keep the event duration
                          clone.end = createDate(tzOpt, +clone.start + duration);
                      }
                  }
                  // Save the occurrence number
                  clone.nr = occurrence.i;
                  // Set uid
                  clone.occurrenceId = clone.id + '_' + getDateStr(clone.start);
                  // Save reference to the original event
                  clone.original = item;
                  if (clone.start && clone.end) {
                      addMultiDayEvent(obj, clone, s, overwrite);
                  }
                  else {
                      addToList(obj, date, clone);
                  }
              }
          }
          else if (item.start && item.end) {
              addMultiDayEvent(obj, item, s, overwrite);
          }
          else if (dt) {
              // Exact date
              addToList(obj, dt, item);
          }
      }
      return obj;
  }

  // tslint:disable no-non-null-assertion
  var labelGuid = 1;
  var MONTH_VIEW = 'month';
  var YEAR_VIEW = 'year';
  var MULTI_YEAR_VIEW = 'multi-year';
  var PAGE_WIDTH = 296;
  var calendarViewDefaults = __assign({}, dateTimeLocale, { dateText: 'Date', eventText: 'event', eventsText: 'events', moreEventsText: '{count} more', nextPageText: 'Next page', prevPageText: 'Previous page', showEventTooltip: true, showToday: true, timeText: 'Time' });
  /**
   * @hidden
   * Returns the first date of the given page.
   * The pages are defined by the eventRange and eventRangeSize props.
   */
  function getFirstPageDay(pageIndex, s) {
      var refDate = s.refDate ? makeDate(s.refDate) : REF_DATE;
      var pageType = s.showCalendar ? s.calendarType : s.eventRange;
      var pageSize = (s.showCalendar ? (pageType === 'year' ? 1 : pageType === 'week' ? s.weeks : s.size) : s.eventRangeSize) || 1;
      var getDate = s.getDate;
      var ref = pageType === 'week' ? getFirstDayOfWeek(refDate, s) : refDate;
      var year = s.getYear(ref);
      var month = s.getMonth(ref);
      var day = s.getDay(ref);
      switch (pageType) {
          case 'year':
              return getDate(year + pageIndex * pageSize, 0, 1);
          case 'week':
              return getDate(year, month, day + 7 * pageSize * pageIndex);
          case 'day':
              return getDate(year, month, day + pageSize * pageIndex);
          default:
              return getDate(year, month + pageIndex * pageSize, 1);
      }
  }
  /** @hidden */
  function getPageIndex(d, s) {
      var refDate = s.refDate ? makeDate(s.refDate) : REF_DATE;
      var getYear = s.getYear;
      var getMonth = s.getMonth;
      var pageType = s.showCalendar ? s.calendarType : s.eventRange;
      var pageSize = (s.showCalendar ? (pageType === 'year' ? 1 : pageType === 'week' ? s.weeks : s.size) : s.eventRangeSize) || 1;
      var diff;
      switch (pageType) {
          case 'year':
              diff = getYear(d) - getYear(refDate);
              break;
          case 'week':
              diff = getDayDiff(getFirstDayOfWeek(refDate, s), getFirstDayOfWeek(d, s)) / 7;
              break;
          case 'day':
              diff = getDayDiff(refDate, d);
              break;
          case 'month':
              diff = getMonth(d) - getMonth(refDate) + (getYear(d) - getYear(refDate)) * 12;
              break;
          default:
              return UNDEFINED;
      }
      return floor(diff / pageSize);
  }
  /** @hidden */
  function getYearsIndex(d, s) {
      var refDate = s.refDate ? makeDate(s.refDate) : REF_DATE;
      return floor((s.getYear(d) - s.getYear(refDate)) / 12);
  }
  /** @hidden */
  function getYearIndex(d, s) {
      var refDate = s.refDate ? makeDate(s.refDate) : REF_DATE;
      return s.getYear(d) - s.getYear(refDate);
  }
  /** @hidden */
  function compareEvents(a, b) {
      var start1 = makeDate(a.start || a.date);
      var start2 = makeDate(b.start || a.date);
      var text1 = a.title || a.text;
      var text2 = b.title || b.text;
      // For non all-day events we multiply the timestamp by 10 to make sure they appear under the all-day events
      var weight1 = !start1 ? 0 : +start1 * (a.allDay ? 1 : 10);
      var weight2 = !start2 ? 0 : +start2 * (b.allDay ? 1 : 10);
      // In case of same weights, order by event title
      if (weight1 === weight2) {
          return text1 > text2 ? 1 : -1;
      }
      return weight1 - weight2;
  }
  /** @hidden */
  function getPageNr(pages, width) {
      return pages === 'auto' // Exact month number from setting
          ? Math.max(1, // Min 1 month
          Math.min(3, // Max 3 months
          Math.floor(width ? width / PAGE_WIDTH : 1)))
          : pages
              ? +pages
              : 1;
  }
  /** @hidden */
  function getLabels(s, labelsObj, start, end, maxLabels, days, allDayOnly, firstWeekDay, isMultiRow, eventOrder, noOuterDays, showLabelCount, moreEventsText, moreEventsPluralText) {
      labelsObj = labelsObj || {};
      var dayLabels = {};
      var eventDays = new Map();
      var eventRows = {};
      var day = start;
      var uid = 0;
      var max = maxLabels;
      var rowEnd = end;
      while (day < end) {
          var dateKey = getDateStr(day);
          var weekDay = day.getDay();
          var monthDay = s.getDay(day);
          var lastDayOfMonth = noOuterDays && s.getDate(s.getYear(day), s.getMonth(day) + 1, 0);
          var isRowStart = (isMultiRow && (weekDay === firstWeekDay || (monthDay === 1 && noOuterDays))) || +day === +start;
          var firstDay = getFirstDayOfWeek(day, s);
          var events = sortEvents(labelsObj[dateKey] || [], eventOrder);
          var prevEvent = void 0;
          var prevEventDays = void 0;
          var prevIndex = void 0;
          var row = 0;
          var displayed = 0;
          var i = 0;
          if (isRowStart) {
              eventRows = {};
              rowEnd = isMultiRow ? addDays(firstDay, days) : end;
          }
          if (allDayOnly) {
              events = events.filter(function (ev) { return ev.allDay; });
          }
          // maxLabels -1 means to display all labels
          if (maxLabels === -1) {
              max = events.length + 1;
          }
          var eventsNr = events.length;
          var data = [];
          if (showLabelCount) {
              data.push({ id: 'count_' + +day, count: eventsNr, placeholder: eventsNr === 0 });
              row = max;
          }
          while (eventsNr && row < max) {
              prevEvent = null;
              // Check  if there are any events already in this row
              for (var j = 0; j < events.length; j++) {
                  if (eventRows[row] === events[j]) {
                      prevEvent = events[j];
                      prevIndex = j;
                  }
              }
              prevEventDays = prevEvent ? eventDays.get(prevEvent) || [] : [];
              if (row === max - 1 && (displayed < eventsNr - 1 || (i === eventsNr && !prevEvent)) && maxLabels !== -1) {
                  var nr = eventsNr - displayed;
                  var moreText = moreEventsText || '';
                  var text = (nr > 1 ? moreEventsPluralText || moreText : moreText).replace(/{count}/, nr);
                  if (nr) {
                      data.push({ id: 'more_' + ++uid, more: text, label: text });
                  }
                  // Remove event from previous days and replace it with more label
                  if (prevEvent) {
                      eventRows[row] = null;
                      for (var _i = 0, prevEventDays_1 = prevEventDays; _i < prevEventDays_1.length; _i++) {
                          var d = prevEventDays_1[_i];
                          var t = moreText.replace(/{count}/, '1');
                          dayLabels[getDateStr(d)].data[row] = { id: 'more_' + ++uid, more: t, label: t };
                      }
                  }
                  displayed++;
                  row++;
              }
              else if (prevEvent) {
                  if (prevIndex === i) {
                      i++;
                  }
                  if (isSameDay(day, makeDate(prevEvent.end, getTzOpt(s, prevEvent)))) {
                      eventRows[row] = null;
                  }
                  data.push({ id: prevEvent.occurrenceId || prevEvent.id, event: prevEvent });
                  row++;
                  displayed++;
                  prevEventDays.push(day);
              }
              else if (i < eventsNr) {
                  var event_1 = events[i];
                  var allDay = event_1.allDay;
                  var tzOpt = getTzOpt(s, event_1);
                  var startTime = event_1.start && makeDate(event_1.start, tzOpt);
                  if (!startTime || // all day event
                      isSameDay(day, startTime) || // event start day
                      isRowStart // event started previously, but continues in this row as well
                  ) {
                      var eventEnd = event_1.end && makeDate(event_1.end, tzOpt);
                      var endTime = getEndDate(s, allDay, startTime, eventEnd, true);
                      var multiDay = endTime && !isSameDay(startTime, endTime);
                      var labelEnd = lastDayOfMonth && lastDayOfMonth < endTime ? lastDayOfMonth : endTime;
                      var startStr = startTime ? ', ' + s.fromText + ': ' + formatDate('DDDD, MMMM D, YYYY', startTime, s) : '';
                      var endStr = endTime ? ', ' + s.toText + ': ' + formatDate('DDDD, MMMM D, YYYY', endTime, s) : '';
                      if (event_1.id === UNDEFINED) {
                          event_1.id = 'mbsc_' + labelGuid++;
                      }
                      if (multiDay) {
                          eventRows[row] = event_1;
                      }
                      eventDays.set(event_1, [day]);
                      data.push({
                          event: event_1,
                          id: event_1.occurrenceId || event_1.id,
                          label: (event_1.title || event_1.text || '') + startStr + endStr,
                          lastDay: lastDayOfMonth ? addDays(lastDayOfMonth, 1) : UNDEFINED,
                          multiDay: multiDay,
                          showText: true,
                          width: multiDay ? Math.min(getDayDiff(day, labelEnd) + 1, getDayDiff(day, rowEnd)) * 100 : 100,
                      });
                      row++;
                      displayed++;
                  }
                  i++;
              }
              else {
                  if (displayed < eventsNr) {
                      data.push({ id: 'ph_' + ++uid, placeholder: true });
                  }
                  row++;
              }
          }
          dayLabels[dateKey] = { data: data, events: events };
          day = getDateOnly(addDays(day, 1));
      }
      return dayLabels;
  }
  /** @hidden */
  function sortEvents(events, eventOrder) {
      return events && events.slice(0).sort(eventOrder || compareEvents);
  }
  /** @hidden */
  function computeEventResize(eventResize, calendarResize, resourceResize) {
      if (eventResize === false || resourceResize === false || !calendarResize) {
          return false;
      }
      return true;
  }
  /** @hidden */
  function computeEventDragInTime(eventDragInTime, resourceDragInTime, calendarDragInTime) {
      if (eventDragInTime === false || resourceDragInTime === false || calendarDragInTime === false) {
          return false;
      }
      return true;
  }
  /** @hidden */
  function computeEventDragBetweenResources(eventDragBetweenResources, resourceDragBetweenResources, calendarDragBetweenResources) {
      if (eventDragBetweenResources === false || resourceDragBetweenResources === false || calendarDragBetweenResources === false) {
          return false;
      }
      return true;
  }

  // tslint:disable no-non-null-assertion
  var MbscCalendarNavService = /*#__PURE__*/ (function () {
      function MbscCalendarNavService() {
          this.pageSize = 0;
          // tslint:disable-next-line: variable-name
          this._prevS = {};
          // tslint:disable-next-line: variable-name
          this._s = {};
      }
      MbscCalendarNavService.prototype.options = function (news, forcePageLoading) {
          var s = (this._s = __assign({}, this._s, news));
          var prevS = this._prevS;
          var getDate = s.getDate;
          var getYear = s.getYear;
          var getMonth = s.getMonth;
          var showCalendar = s.showCalendar;
          var calendarType = s.calendarType;
          var startDay = s.startDay;
          var endDay = s.endDay;
          var firstWeekDay = s.firstDay;
          var isWeekView = calendarType === 'week';
          var weeks = showCalendar ? (isWeekView ? s.weeks : 6) : 0;
          var minDate = s.min !== prevS.min || !this.minDate ? (!isEmpty(s.min) ? makeDate(s.min) : -Infinity) : this.minDate;
          var maxDate = s.max !== prevS.max || !this.maxDate ? (!isEmpty(s.max) ? makeDate(s.max) : Infinity) : this.maxDate;
          var initialActive = s.activeDate || +new Date();
          var activeDate = constrain(initialActive, +minDate, +maxDate);
          var forcePageChange = this.forcePageChange || activeDate !== initialActive;
          var d = new Date(activeDate);
          var activeChanged = activeDate !== prevS.activeDate;
          var viewChanged = s.calendarType !== prevS.calendarType ||
              s.eventRange !== prevS.eventRange ||
              s.firstDay !== prevS.firstDay ||
              s.eventRangeSize !== prevS.eventRangeSize ||
              s.refDate !== prevS.refDate ||
              showCalendar !== prevS.showCalendar ||
              s.size !== prevS.size ||
              s.weeks !== prevS.weeks;
          var pageIndex = forcePageChange ||
              this.pageIndex === UNDEFINED ||
              viewChanged ||
              (!this.preventPageChange && activeChanged && (activeDate < +this.firstDay || activeDate >= +this.lastDay))
              ? getPageIndex(d, s)
              : this.pageIndex;
          var size = calendarType === 'year' ? 12 : s.size || 1;
          var isGrid = size > 1 && !isWeekView;
          var pageNr = isGrid ? 1 : getPageNr(s.pages, this.pageSize);
          var isVertical = s.calendarScroll === 'vertical' && s.pages !== 'auto' && (s.pages === UNDEFINED || s.pages === 1);
          var showOuter = s.showOuterDays !== UNDEFINED ? s.showOuterDays : !isVertical && pageNr < 2 && (isWeekView || !size || size < 2);
          var pageBuffer = isGrid ? 0 : 1;
          var firstDay = getFirstPageDay(pageIndex, s);
          var lastDay = getFirstPageDay(pageIndex + pageNr, s);
          // In case of scheduler and timeline, if startDay & endDay is specified, calculate first and last days based on that
          if (!showCalendar && s.eventRange === 'week' && startDay !== UNDEFINED && endDay !== UNDEFINED) {
              firstDay = addDays(firstDay, startDay - firstWeekDay + (startDay < firstWeekDay ? 7 : 0));
              lastDay = addDays(firstDay, 7 * s.eventRangeSize + endDay - startDay + 1 - (endDay < startDay ? 0 : 7));
          }
          var firstPageDay = showCalendar && showOuter ? getFirstDayOfWeek(firstDay, s) : firstDay;
          var lastPage = isGrid ? getDate(getYear(lastDay), getMonth(lastDay) - 1, 1) : getFirstPageDay(pageIndex + pageNr - 1, s);
          var lastPageDay = showCalendar && showOuter ? addDays(getFirstDayOfWeek(lastPage, s), weeks * 7) : lastDay;
          var start = showCalendar ? getFirstDayOfWeek(getFirstPageDay(pageIndex - pageBuffer, s), s) : firstDay;
          var last = showCalendar ? getFirstDayOfWeek(getFirstPageDay(pageIndex + pageNr + pageBuffer - 1, s), s) : lastDay;
          var end = showCalendar ? addDays(isGrid ? getFirstDayOfWeek(lastPage, s) : last, weeks * 7) : lastDay;
          var initialRun = this.pageIndex === UNDEFINED;
          var viewStart = start;
          var viewEnd = end;
          if (!showCalendar && s.resolution === 'week' && (s.eventRange === 'year' || s.eventRange === 'month')) {
              var length_1 = endDay - startDay + 1 + (endDay < startDay ? 7 : 0);
              if (firstDay.getDay() !== startDay) {
                  var weekStart = getFirstDayOfWeek(firstDay, s, startDay);
                  var weekEnd = addDays(weekStart, length_1);
                  viewStart = weekEnd <= firstDay ? addDays(weekStart, 7) : weekStart;
              }
              if (lastDay.getDay() !== startDay) {
                  var weekStart = getFirstDayOfWeek(lastDay, s, startDay);
                  var weekEnd = addDays(weekStart, length_1);
                  viewEnd = weekStart > lastDay ? addDays(weekEnd, -7) : weekEnd;
              }
          }
          var pageChange = false;
          if (pageIndex !== UNDEFINED) {
              pageChange = +viewStart !== +this.viewStart || +viewEnd !== +this.viewEnd;
              this.pageIndex = pageIndex;
          }
          this.firstDay = firstDay;
          this.lastDay = lastDay;
          this.firstPageDay = firstPageDay;
          this.lastPageDay = lastPageDay;
          this.viewStart = viewStart;
          this.viewEnd = viewEnd;
          this.forcePageChange = false;
          this.preventPageChange = false;
          this.minDate = minDate;
          this.maxDate = maxDate;
          this._prevS = s;
          if (pageIndex !== UNDEFINED && (pageChange || forcePageLoading)) {
              if (pageChange && !initialRun) {
                  this.pageChange();
              }
              this.pageLoading(pageChange);
          }
      };
      MbscCalendarNavService.prototype.pageChange = function () {
          if (this._s.onPageChange) {
              this._s.onPageChange({
                  firstDay: this.firstPageDay,
                  lastDay: this.lastPageDay,
                  month: this._s.calendarType === 'month' ? this.firstDay : UNDEFINED,
                  type: 'onPageChange',
                  viewEnd: this.viewEnd,
                  viewStart: this.viewStart,
              }, null);
          }
      };
      MbscCalendarNavService.prototype.pageLoading = function (viewChanged) {
          if (this._s.onPageLoading) {
              this._s.onPageLoading({
                  firstDay: this.firstPageDay,
                  lastDay: this.lastPageDay,
                  month: this._s.calendarType === 'month' ? this.firstDay : UNDEFINED,
                  type: 'onPageLoading',
                  viewChanged: viewChanged,
                  viewEnd: this.viewEnd,
                  viewStart: this.viewStart,
              }, null);
          }
      };
      return MbscCalendarNavService;
  }());

  // tslint:disable no-non-null-assertion
  /**
   * Checks if a date is invalid or not.
   * @param s Options object for the exclusiveEndDates and timezone options used
   * @param d The date to check.
   * @param invalids Object map containing the invalids.
   * @param valids Object map containing the valids.
   * @param min Timestamp of the min date.
   * @param max Timestamp of the max date.
   */
  function isInvalid(s, d, invalids, valids, min, max) {
      var key = getDateStr(d); // +getDateOnly(d);
      if ((min && +d < min) || (max && +d > max)) {
          return true;
      }
      if (valids && valids[key]) {
          return false;
      }
      var invalidsForDay = invalids && invalids[key];
      if (invalidsForDay) {
          for (var _i = 0, invalidsForDay_1 = invalidsForDay; _i < invalidsForDay_1.length; _i++) {
              var invalid = invalidsForDay_1[_i];
              var start = invalid.start, end = invalid.end, allDay = invalid.allDay;
              if (start && end && !allDay) {
                  var endDate = getEndDate(s, allDay, start, end);
                  var dayStart = getDayStart(s, d);
                  var dayEnd = getDayEnd(s, endDate);
                  if (!isSameDay(start, end) &&
                      (+start === +dayStart || +endDate === +dayEnd || (!isSameDay(d, start) && !isSameDay(d, end) && d > start && d < end))
                  // d <= end???
                  ) {
                      return invalid;
                  }
              }
              else {
                  return invalid;
              }
          }
      }
      return false;
  }
  /**
   * Returns the closest valid date. Actually gets the closest valid only if the next or the previous valid is in
   * the other month. Otherwise it gets the next valid (when not given direction), regardless if the previous valid is closer.
   * @param d Initial date.
   * @param s Date & time options.
   * @param min Timestamp of the min date.
   * @param max Timestamp of the max date.
   * @param invalids Object map containing the invalids.
   * @param valids Object map containing the valids.
   * @param dir Direction to find the next valid date. If 1, it will search forwards, if -1, backwards,
   * otherwise will search both directions and return the closest one.
   */
  function getClosestValidDate(d, s, min, max, invalids, valids, dir) {
      var next;
      var prev;
      var nextInvalid = true;
      var prevInvalid = true;
      var up = 0;
      var down = 0;
      if (+d < min) {
          d = createDate(s, min);
      }
      if (+d > max) {
          d = createDate(s, max);
      }
      var year = s.getYear(d);
      var month = s.getMonth(d);
      var start = s.getDate(year, month - 1, 1);
      var end = s.getDate(year, month + 2, 1);
      var from = +start > min ? +start : min;
      var until = +end < max ? +end : max;
      // If invalids are not passed we create the invalids map for +/- 1 month
      if (!invalids) {
          // Map the valids and invalids for prev and next months
          valids = getEventMap(s.valid, start, end, s, true);
          invalids = getEventMap(s.invalid, start, end, s, true);
      }
      if (!isInvalid(s, d, invalids, valids, min, max)) {
          return d;
      }
      next = d;
      prev = d;
      // Find next valid value
      while (nextInvalid && +next < until && up < 100) {
          next = addDays(next, 1);
          nextInvalid = isInvalid(s, next, invalids, valids, min, max);
          up++;
      }
      // Find previous valid value
      while (prevInvalid && +prev > from && down < 100) {
          prev = addDays(prev, -1);
          prevInvalid = isInvalid(s, prev, invalids, valids, min, max);
          down++;
      }
      // If no valid value found, return the invalid value
      if (nextInvalid && prevInvalid) {
          return d;
      }
      if (dir === 1 && !nextInvalid) {
          return next;
      }
      if (dir === -1 && !prevInvalid) {
          return prev;
      }
      // if (viewStart && viewEnd) {
      //   if (+next >= viewStart && +next < viewEnd) {
      //     return next;
      //   }
      //   if (+prev >= viewStart && +prev < viewEnd) {
      //     return prev;
      //   }
      // }
      if (isSameMonth(d, next, s) && !nextInvalid) {
          return next;
      }
      if (isSameMonth(d, prev, s) && !prevInvalid) {
          return prev;
      }
      return prevInvalid || (down >= up && !nextInvalid) ? next : prev;
  }

  var BLUR = 'blur';
  var CHANGE = 'change';
  var CLICK = 'click';
  var CONTEXTMENU = 'contextmenu';
  var DOUBLE_CLICK = 'dblclick';
  var FOCUS = 'focus';
  var FOCUS_IN = 'focusin';
  var INPUT = 'input';
  var KEY_DOWN = 'keydown';
  var MOUSE_DOWN = 'mousedown';
  var MOUSE_MOVE = 'mousemove';
  var MOUSE_UP = 'mouseup';
  var MOUSE_OVER = 'mousedown';
  var MOUSE_ENTER = 'mouseenter';
  var MOUSE_LEAVE = 'mouseleave';
  var MOUSE_WHEEL = 'mousewheel';
  var SCROLL = 'scroll';
  var TOUCH_START = 'touchstart';
  var TOUCH_MOVE = 'touchmove';
  var TOUCH_END = 'touchend';
  var TOUCH_CANCEL = 'touchcancel';
  var WHEEL = 'wheel';

  var BACKSPACE = 8;
  var TAB = 9;
  var ENTER = 13;
  var ESC = 27;
  var SPACE = 32;
  var PAGE_UP = 33;
  var PAGE_DOWN = 34;
  var END = 35;
  var HOME = 36;
  var LEFT_ARROW = 37;
  var UP_ARROW = 38;
  var RIGHT_ARROW = 39;
  var DOWN_ARROW = 40;
  var DELETE = 46;

  // tslint:disable no-non-null-assertion
  var tapped = 0;
  var allowQuick;
  /**
   * Returns the X or Y coordinate from a touch or mouse event.
   * @hidden
   * @param ev
   * @param axis
   * @param page
   * @returns
   */
  function getCoord(ev, axis, page) {
      // const ev = e.originalEvent || e;
      var prop = (page ? 'page' : 'client') + axis;
      // Multi touch support
      if (ev.targetTouches && ev.targetTouches[0]) {
          return ev.targetTouches[0][prop];
      }
      if (ev.changedTouches && ev.changedTouches[0]) {
          return ev.changedTouches[0][prop];
      }
      return ev[prop];
  }
  /** @hidden */
  function preventClick() {
      // Prevent ghost click
      tapped++;
      setTimeout(function () {
          tapped--;
      }, 500);
  }
  /** @hidden */
  function triggerClick(ev, control) {
      // Prevent duplicate triggers on the same element
      // e.g. a form checkbox inside a listview item
      if (control.mbscClick) {
          return;
      }
      var touch = (ev.originalEvent || ev).changedTouches[0];
      var evt = document.createEvent('MouseEvents');
      evt.initMouseEvent('click', true, true, window, 1, touch.screenX, touch.screenY, touch.clientX, touch.clientY, false, false, false, false, 0, null);
      evt.isMbscTap = true;
      // Prevent ionic to bust our click
      // This works for Ionic 1 - 3, not sure about 4
      evt.isIonicTap = true;
      // This will allow a click fired together with this click
      // We need this, because clicking on a label will trigger a click
      // on the associated input as well, which should not be busted
      allowQuick = true;
      control.mbscChange = true;
      control.mbscClick = true;
      control.dispatchEvent(evt);
      allowQuick = false;
      // Prevent ghost click
      preventClick();
      setTimeout(function () {
          delete control.mbscClick;
      });
  }
  /**
   * Prevent standard behaviour on click
   * @hidden
   * @param ev
   */
  function bustClick(ev) {
      // Textarea needs the mousedown event
      if (tapped && !allowQuick && !ev.isMbscTap && !(ev.target.nodeName === 'TEXTAREA' && ev.type === MOUSE_DOWN)) {
          ev.stopPropagation();
          ev.preventDefault();
      }
  }
  if (isBrowser) {
      [MOUSE_OVER, MOUSE_ENTER, MOUSE_DOWN, MOUSE_UP, CLICK].forEach(function (ev) {
          doc.addEventListener(ev, bustClick, true);
      });
      if (os === 'android' && majorVersion < 5) {
          doc.addEventListener(CHANGE, function (ev) {
              var target = ev.target;
              if (tapped && target.type === 'checkbox' && !target.mbscChange) {
                  ev.stopPropagation();
                  ev.preventDefault();
              }
              delete target.mbscChange;
          }, true);
      }
  }

  // tslint:disable no-non-null-assertion
  var wasTouched;
  /** @hidden */
  function setFocusInvisible(ev) {
      var win = getWindow(ev.target);
      win.__mbscFocusVisible = false;
  }
  /** @hidden */
  function setFocusVisible(ev) {
      var win = getWindow(ev.target);
      win.__mbscFocusVisible = true;
  }
  /** @hidden */
  function addRipple(elm, x, y) {
      var rect = elm.getBoundingClientRect();
      var left = x - rect.left;
      var top = y - rect.top;
      var width = Math.max(left, elm.offsetWidth - left);
      var height = Math.max(top, elm.offsetHeight - top);
      var size = 2 * Math.sqrt(Math.pow(width, 2) + Math.pow(height, 2));
      var ripple = doc.createElement('span');
      ripple.classList.add('mbsc-ripple');
      var style = ripple.style;
      style.backgroundColor = getComputedStyle(elm).color;
      style.width = size + 'px';
      style.height = size + 'px';
      style.top = y - rect.top - size / 2 + 'px';
      style.left = x - rect.left - size / 2 + 'px';
      elm.appendChild(ripple);
      // raf(() => {
      setTimeout(function () {
          style.opacity = '.2';
          style.transform = 'scale(1)';
          style.transition = 'opacity linear .1s, transform cubic-bezier(0, 0, 0.2, 1) .4s';
      }, 30);
      return ripple;
  }
  /** @hidden */
  function removeRipple(r) {
      if (r) {
          setTimeout(function () {
              r.style.opacity = '0';
              r.style.transition = 'opacity linear .4s';
              setTimeout(function () {
                  if (r && r.parentNode) {
                      r.parentNode.removeChild(r);
                  }
              }, 400);
          }, 200);
      }
  }
  /** @hidden */
  function gestureListener(elm, options) {
      var args = {};
      var win = getWindow(elm);
      var document = getDocument(elm);
      var active;
      var activeable;
      var activeTimer;
      var ripple;
      var hasFocus;
      var hasHover;
      var hasRipple;
      var moved;
      var startX;
      var startY;
      var endX;
      var endY;
      var deltaX;
      var deltaY;
      var started;
      function skipMouseEvent(ev) {
          if (ev.type === TOUCH_START) {
              wasTouched = true;
          }
          else if (wasTouched) {
              if (ev.type === MOUSE_DOWN) {
                  wasTouched = false;
              }
              return true;
          }
          return false;
      }
      function activate() {
          if (hasRipple) {
              removeRipple(ripple);
              ripple = addRipple(elm, endX, endY);
          }
          options.onPress();
          active = true;
      }
      function deactivate(r, time) {
          activeable = false;
          removeRipple(r);
          clearTimeout(activeTimer);
          activeTimer = setTimeout(function () {
              if (active) {
                  options.onRelease();
                  active = false;
              }
          }, time);
      }
      function onStart(ev) {
          // Skip if mouse down event was fired after touch
          if (skipMouseEvent(ev)) {
              return;
          }
          // Skip mousedown event if right click
          if (ev.type === MOUSE_DOWN && (ev.button !== 0 || ev.ctrlKey)) {
              return;
          }
          startX = getCoord(ev, 'X');
          startY = getCoord(ev, 'Y');
          endX = startX;
          endY = startY;
          active = false;
          activeable = false;
          moved = false;
          started = true;
          args.moved = moved;
          args.startX = startX;
          args.startY = startY;
          args.endX = endX;
          args.endY = endY;
          args.deltaX = 0;
          args.deltaY = 0;
          args.domEvent = ev;
          args.isTouch = wasTouched;
          removeRipple(ripple);
          if (options.onStart) {
              var ret = options.onStart(args);
              hasRipple = ret && ret.ripple;
          }
          if (options.onPress) {
              activeable = true;
              clearTimeout(activeTimer);
              activeTimer = setTimeout(activate, 50);
          }
          if (ev.type === MOUSE_DOWN) {
              listen(document, MOUSE_MOVE, onMove);
              listen(document, MOUSE_UP, onEnd);
          }
          listen(document, CONTEXTMENU, onContextMenu);
      }
      function onMove(ev) {
          if (!started) {
              return;
          }
          endX = getCoord(ev, 'X');
          endY = getCoord(ev, 'Y');
          deltaX = endX - startX;
          deltaY = endY - startY;
          if (!moved && (Math.abs(deltaX) > 9 || Math.abs(deltaY) > 9)) {
              moved = true;
              deactivate(ripple);
          }
          args.moved = moved;
          args.endX = endX;
          args.endY = endY;
          args.deltaX = deltaX;
          args.deltaY = deltaY;
          args.domEvent = ev;
          args.isTouch = ev.type === TOUCH_MOVE;
          if (options.onMove) {
              options.onMove(args);
          }
      }
      function onEnd(ev) {
          if (!started) {
              return;
          }
          if (activeable && !active) {
              clearTimeout(activeTimer);
              activate();
          }
          args.domEvent = ev;
          args.isTouch = ev.type === TOUCH_END;
          if (options.onEnd) {
              options.onEnd(args);
          }
          deactivate(ripple, 75);
          started = false;
          if (ev.type === TOUCH_END && options.click && hasGhostClick && !moved) {
              triggerClick(ev, ev.target);
          }
          if (ev.type === MOUSE_UP) {
              unlisten(document, MOUSE_MOVE, onMove);
              unlisten(document, MOUSE_UP, onEnd);
          }
          unlisten(document, CONTEXTMENU, onContextMenu);
      }
      function onHoverIn(ev) {
          if (skipMouseEvent(ev)) {
              return;
          }
          hasHover = true;
          options.onHoverIn(ev);
      }
      function onHoverOut(ev) {
          if (hasHover) {
              options.onHoverOut(ev);
          }
          hasHover = false;
      }
      function onKeyDown(ev) {
          options.onKeyDown(ev);
      }
      function onFocus(ev) {
          if (options.keepFocus || win.__mbscFocusVisible) {
              hasFocus = true;
              options.onFocus(ev);
          }
      }
      function onBlur(ev) {
          if (hasFocus) {
              options.onBlur(ev);
          }
          hasFocus = false;
      }
      function onChange(ev) {
          options.onChange(ev);
      }
      function onInput(ev) {
          options.onInput(ev);
      }
      function onDoubleClick(ev) {
          args.domEvent = ev;
          if (!wasTouched) {
              options.onDoubleClick(args);
          }
      }
      function onContextMenu(ev) {
          if (wasTouched) {
              ev.preventDefault();
          }
      }
      // Set up listeners
      listen(elm, MOUSE_DOWN, onStart);
      listen(elm, TOUCH_START, onStart, { passive: true });
      listen(elm, TOUCH_MOVE, onMove, { passive: false });
      listen(elm, TOUCH_END, onEnd);
      listen(elm, TOUCH_CANCEL, onEnd);
      if (options.onChange) {
          listen(elm, CHANGE, onChange);
      }
      if (options.onInput) {
          listen(elm, INPUT, onInput);
      }
      if (options.onHoverIn) {
          listen(elm, MOUSE_ENTER, onHoverIn);
      }
      if (options.onHoverOut) {
          listen(elm, MOUSE_LEAVE, onHoverOut);
      }
      if (options.onKeyDown) {
          listen(elm, KEY_DOWN, onKeyDown);
      }
      if (options.onFocus && win) {
          listen(elm, FOCUS, onFocus);
          if (!options.keepFocus) {
              var focusCount = win.__mbscFocusCount || 0;
              if (focusCount === 0) {
                  listen(win, MOUSE_DOWN, setFocusInvisible, true);
                  listen(win, KEY_DOWN, setFocusVisible, true);
              }
              win.__mbscFocusCount = ++focusCount;
          }
      }
      if (options.onBlur) {
          listen(elm, BLUR, onBlur);
      }
      if (options.onDoubleClick) {
          listen(elm, DOUBLE_CLICK, onDoubleClick);
      }
      return function () {
          clearTimeout(activeTimer);
          if (options.onFocus && win && !options.keepFocus) {
              var focusCount = win.__mbscFocusCount || 0;
              win.__mbscFocusCount = --focusCount;
              if (focusCount <= 0) {
                  unlisten(win, MOUSE_DOWN, setFocusInvisible);
                  unlisten(win, KEY_DOWN, setFocusVisible);
              }
          }
          unlisten(elm, INPUT, onInput);
          unlisten(elm, MOUSE_DOWN, onStart);
          unlisten(elm, TOUCH_START, onStart, { passive: true });
          unlisten(elm, TOUCH_MOVE, onMove, { passive: false });
          unlisten(elm, TOUCH_END, onEnd);
          unlisten(elm, TOUCH_CANCEL, onEnd);
          unlisten(document, MOUSE_MOVE, onMove);
          unlisten(document, MOUSE_UP, onEnd);
          unlisten(document, CONTEXTMENU, onContextMenu);
          unlisten(elm, CHANGE, onChange);
          unlisten(elm, MOUSE_ENTER, onHoverIn);
          unlisten(elm, MOUSE_LEAVE, onHoverOut);
          unlisten(elm, KEY_DOWN, onKeyDown);
          unlisten(elm, FOCUS, onFocus);
          unlisten(elm, BLUR, onBlur);
          unlisten(elm, DOUBLE_CLICK, onDoubleClick);
      };
  }

  // tslint:disable no-non-null-assertion
  // tslint:disable no-inferrable-types
  // tslint:disable directive-class-suffix
  // tslint:disable directive-selector
  var dragObservable = new Observable();
  function subscribeExternalDrag(handler) {
      return dragObservable.subscribe(handler);
  }
  function unsubscribeExternalDrag(key) {
      dragObservable.unsubscribe(key);
  }
  function moveClone(ev, clone) {
      clone.style.left = ev.endX + 'px';
      clone.style.top = ev.endY + 'px';
  }
  /** @hidden */
  var DraggableBase = /*#__PURE__*/ (function (_super) {
      __extends(DraggableBase, _super);
      function DraggableBase() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      // tslint:enable variable-name
      DraggableBase.prototype._render = function (s) {
          if (s.dragData !== this._prevS.dragData) {
              this._dragData = isString(s.dragData) ? JSON.parse(s.dragData.toString()) : s.dragData;
          }
      };
      DraggableBase.prototype._updated = function () {
          var _this = this;
          var el = this.s.element || this._el;
          if (this._unlisten === UNDEFINED && el) {
              el.classList.add('mbsc-draggable');
              var clone_1;
              var isDrag_1;
              var touchTimer_1;
              var body_1 = getDocument(el).body;
              this._unlisten = gestureListener(el, {
                  onEnd: function (ev) {
                      if (isDrag_1) {
                          var args = __assign({}, ev);
                          // Will prevent mousedown event on doc
                          args.domEvent.preventDefault();
                          args.action = 'externalDrop';
                          args.event = _this._dragData;
                          args.create = true;
                          args.external = true;
                          args.eventName = 'onDragEnd';
                          dragObservable.next(args);
                          isDrag_1 = false;
                          body_1.removeChild(clone_1);
                      }
                      clearTimeout(touchTimer_1);
                  },
                  onMove: function (ev) {
                      var args = __assign({}, ev);
                      args.event = _this._dragData;
                      args.clone = clone_1;
                      args.create = true;
                      args.external = true;
                      if (isDrag_1 || !args.isTouch) {
                          // Prevents page scroll on touch and text selection with mouse
                          args.domEvent.preventDefault();
                      }
                      if (isDrag_1) {
                          moveClone(ev, clone_1);
                          args.eventName = 'onDragMove';
                          dragObservable.next(args);
                      }
                      else if (Math.abs(args.deltaX) > 7 || Math.abs(args.deltaY) > 7) {
                          clearTimeout(touchTimer_1);
                          if (!args.isTouch) {
                              moveClone(ev, clone_1);
                              body_1.appendChild(clone_1);
                              args.eventName = 'onDragStart';
                              dragObservable.next(args);
                              isDrag_1 = true;
                          }
                      }
                  },
                  onStart: function (ev) {
                      var args = __assign({}, ev);
                      if (!isDrag_1) {
                          clone_1 = el.cloneNode(true);
                          clone_1.classList.add('mbsc-drag-clone');
                          args.event = _this._dragData;
                          args.create = true;
                          args.external = true;
                          if (args.isTouch) {
                              touchTimer_1 = setTimeout(function () {
                                  moveClone(ev, clone_1);
                                  body_1.appendChild(clone_1);
                                  args.clone = clone_1;
                                  args.eventName = 'onDragModeOn';
                                  dragObservable.next(args);
                                  args.eventName = 'onDragStart';
                                  dragObservable.next(args);
                                  isDrag_1 = true;
                              }, 350);
                          }
                      }
                  },
              });
          }
      };
      DraggableBase.prototype._destroy = function () {
          if (this._unlisten) {
              this._unlisten();
              this._unlisten = UNDEFINED;
          }
      };
      // tslint:disable variable-name
      DraggableBase._name = 'Draggable';
      return DraggableBase;
  }(BaseComponent));

  // tslint:disable no-non-null-assertion
  var uid = 1;
  /** @hidden */
  function getDataInRange(data, s, firstDay, lastDay, start, end) {
      var startDate = firstDay;
      var endDate = lastDay;
      var map = new Map();
      var dataInRange = [];
      if (start) {
          startDate = makeDate(start, s);
      }
      if (end) {
          endDate = makeDate(end, s);
      }
      else if (start) {
          endDate = addDays(startDate, 1);
      }
      var events = getEventMap(data, startDate, endDate, s);
      for (var date in events) {
          if (date) {
              for (var _i = 0, _a = events[date]; _i < _a.length; _i++) {
                  var event_1 = _a[_i];
                  if (!event_1.start) {
                      // Single date only (in case of invalids)
                      dataInRange.push(event_1);
                  }
                  else if (!map.has(event_1)) {
                      var eventStart = makeDate(event_1.start, s);
                      var eventEnd = makeDate(event_1.end, s) || eventStart;
                      if (event_1.allDay) {
                          eventStart = createDate(s, eventStart.getFullYear(), eventStart.getMonth(), eventStart.getDate());
                          eventEnd = getEndDate(s, true, eventStart, eventEnd);
                          eventEnd = createDate(s, eventEnd.getFullYear(), eventEnd.getMonth(), eventEnd.getDate(), 23, 59, 59, 999);
                      }
                      if (checkDateRangeOverlap(startDate, endDate, eventStart, eventEnd)) {
                          var eventCopy = __assign({}, event_1);
                          if (s.dataTimezone || s.displayTimezone) {
                              eventCopy.start = eventStart.toISOString();
                              eventCopy.end = eventEnd.toISOString();
                          }
                          map.set(event_1, true);
                          dataInRange.push(eventCopy);
                      }
                  }
              }
          }
      }
      return dataInRange;
  }
  /** @hidden */
  function getEventId() {
      return "mbsc_" + uid++;
  }
  /** @hidden */
  function getEventData(s, event, eventDay, colorEvent, resource, isList, isMultipart, isDailyResolution, skipLabels) {
      var color = event.color || (resource && resource.color);
      var st = event.start || event.date;
      var origStart = event.recurring ? event.original.start : event.start;
      var allDay = event.allDay || !origStart;
      var tzOpt = getTzOpt(s, event);
      var start = st ? makeDate(st, tzOpt) : null;
      var end = event.end ? makeDate(event.end, tzOpt) : null;
      var endDate = getEndDate(s, event.allDay, start, end, isList);
      var isMultiDay = start && endDate && !isSameDay(start, endDate);
      var isFirstDay = isMultiDay ? isSameDay(start, eventDay) : true;
      var isLastDay = isMultiDay ? isSameDay(endDate, eventDay) : true;
      var fillsAllDay = allDay || (isMultipart && isMultiDay && !isFirstDay && !isLastDay);
      var startTime = '';
      var endTime = '';
      if (!skipLabels && start && end) {
          if (!isMultipart && !isDailyResolution) {
              startTime = formatDate(s.dateFormat, start, s);
              endTime = formatDate(s.dateFormat, endDate, s);
          }
          else if (!allDay) {
              startTime = formatDate(s.timeFormat, start, s);
              endTime = formatDate(s.timeFormat, end, s);
          }
      }
      var eventStart = !fillsAllDay && (isFirstDay || !isMultipart) ? startTime : '';
      var eventEnd = !fillsAllDay && (isLastDay || !isMultipart) ? endTime : '';
      var html = event.title || event.text || '';
      var title = html; // htmlToText(html);
      var tooltip = title + (fillsAllDay ? '' : ', ' + eventStart + ' - ' + eventEnd);
      var format = 'DDDD, MMMM D, YYYY';
      var startStr = !skipLabels && start ? ', ' + s.fromText + ': ' + formatDate(format, start, s) + (allDay ? '' : ', ' + startTime) : '';
      var endStr = !skipLabels && end ? ', ' + s.toText + ': ' + formatDate(format, end, s) + (allDay ? '' : ', ' + endTime) : '';
      var resourceStr = resource && resource.name ? ', ' + resource.name : '';
      return {
          allDay: allDay,
          allDayText: fillsAllDay ? s.allDayText : '',
          ariaLabel: title + resourceStr + startStr + endStr,
          color: color,
          currentResource: resource,
          date: +eventDay,
          end: eventEnd,
          endDate: end ? end : start ? new Date(start) : null,
          html: html,
          id: event.id,
          isMultiDay: isMultiDay,
          lastDay: !fillsAllDay && isMultiDay && isLastDay ? s.toText : '',
          original: event,
          position: {},
          resource: event.resource,
          slot: event.slot,
          start: eventStart,
          startDate: start,
          style: {
              background: color,
              color: colorEvent && color ? getTextColor(color) : '',
          },
          title: title,
          tooltip: s.showEventTooltip ? event.tooltip || tooltip : UNDEFINED,
          // uid will contain the start date as well in case of recurring events
          uid: event.occurrenceId ? event.occurrenceId : event.id,
      };
  }
  /** @hidden */
  function prepareEvents(events) {
      var data = [];
      if (events) {
          for (var _i = 0, events_1 = events; _i < events_1.length; _i++) {
              var event_2 = events_1[_i];
              if (event_2.id === UNDEFINED) {
                  event_2.id = getEventId();
              }
              data.push(event_2);
          }
      }
      return data;
  }
  /** @hidden */
  function checkInvalidCollision(s, invalids, valids, start, end, min, max, invalidateEvent, exclusiveEndDates) {
      if (invalidateEvent === 'start-end') {
          var invalidStart = isInvalid(s, start, invalids, valids, min, max);
          var invalidEnd = isInvalid(s, end, invalids, valids, min, max);
          if (invalidStart) {
              return invalidStart;
          }
          if (invalidEnd) {
              return invalidEnd;
          }
      }
      else {
          var until = exclusiveEndDates ? end : getDateOnly(addDays(end, 1));
          for (var d = getDateOnly(start); d < until; d.setDate(d.getDate() + 1)) {
              var invalid = isInvalid(s, d, invalids, valids, min, max);
              if (invalid) {
                  return invalid;
              }
          }
      }
      return false;
  }

  // tslint:disable no-non-null-assertion
  // tslint:disable no-inferrable-types
  // tslint:disable directive-class-suffix
  // tslint:disable directive-selector
  /** @hidden */
  var EventcalendarBase = /*#__PURE__*/ (function (_super) {
      __extends(EventcalendarBase, _super);
      function EventcalendarBase() {
          var _this = _super !== null && _super.apply(this, arguments) || this;
          _this.print = noop;
          /** @hidden */
          _this._checkSize = 0;
          /** @hidden */
          _this._navService = new MbscCalendarNavService();
          /** @hidden */
          _this._pageLoad = 0;
          /** @hidden */
          _this._selectedDates = {};
          /** @hidden */
          _this._shouldScrollSchedule = 0;
          /** @hidden */
          _this._update = 0;
          /** @hidden */
          _this._onScroll = throttle(function () {
              if (!_this._isListScrolling && !_this._viewChanged) {
                  for (var timestamp in _this._listDays) {
                      if (_this._listDays[timestamp]) {
                          var day = _this._listDays[timestamp];
                          var bottom = day.offsetTop + day.offsetHeight - _this._list.scrollTop;
                          if (bottom > 0) {
                              if (+timestamp !== _this._selected) {
                                  _this._shouldSkipScroll = true;
                                  _this._selectedChange(+timestamp);
                              }
                              break;
                          }
                      }
                  }
              }
          });
          _this._isListScrolling = 0;
          /** @hidden */
          _this._onWeekDayClick = function (d) {
              if (d !== _this._selected) {
                  _this._skipScheduleScroll = true;
                  _this._selectedChange(d);
              }
          };
          /** @hidden */
          _this._onDayClick = function (args) {
              var date = args.date;
              var d = +date;
              var key = getDateStr(date);
              var state = _this.state;
              var events = sortEvents(_this._eventMap[key], _this.s.eventOrder);
              var showEventPopover = _this._showEventPopover;
              var computed = showEventPopover === UNDEFINED ? !_this._showEventLabels && !_this._showEventList && !_this._showSchedule : showEventPopover;
              var showMore = showEventPopover !== false && _this._moreLabelClicked;
              var showPopover = (computed || showMore) && // Popover is enabled
                  (!state.showPopover || (state.showPopover && d !== state.popoverDate)) && // Check if popover is already opened for the date
                  events &&
                  events.length > 0; // Has events
              args.events = events;
              if (!_this._isEventClick) {
                  _this._resetSelection();
              }
              _this._hook('onCellClick', args);
              _this._moreLabelClicked = false;
              if (!args.disabled && d !== _this._selected) {
                  _this._navService.preventPageChange = !_this._showEventList;
                  _this._skipScheduleScroll = true;
                  _this._selectedChange(d);
              }
              if (showPopover) {
                  // Wait for the popover to hide (if any)
                  setTimeout(function () {
                      _this._anchor = args.target;
                      _this._popoverClass = _this._popoverClass.replace(' mbsc-popover-hidden', '');
                      _this.setState({
                          popoverDate: d,
                          popoverList: events.map(function (event) { return _this._getEventData(event, date); }),
                          showPopover: true,
                      });
                  });
              }
              _this._isEventClick = false;
          };
          /** @hidden */
          _this._onActiveChange = function (args) {
              if (args.scroll) {
                  _this._viewDate = +args.date;
                  return;
              }
              var d = _this._getValidDay(args.date, args.dir);
              // We set the active date in the state as well, to trigger re-render
              // Note: we cannot use the state only, because the active date will be updated when the selected date changes,
              // but we don't want an extra setState call on selected date change
              var newState = {
                  activeDate: d,
              };
              _this._active = d;
              _this._viewDate = d;
              _this._update++; // Force update in case of Angular, if active date is the same as previous active date
              _this._skipScheduleScroll = args.pageChange && !args.nav;
              // If page is changed or today button is clicked, also update the selected date
              if (args.pageChange || args.today) {
                  newState.selectedDate = d;
                  _this._selectedChange(d, true);
                  _this._navService.forcePageChange = true;
              }
              _this.setState(newState);
          };
          /** @hidden */
          _this._onGestureStart = function (args) {
              _this._hidePopover();
          };
          /** @hidden */
          _this._onDayDoubleClick = function (args) {
              _this._dayClick('onCellDoubleClick', args);
          };
          /** @hidden */
          _this._onDayRightClick = function (args) {
              _this._dayClick('onCellRightClick', args);
          };
          /** @hidden */
          _this._onCellHoverIn = function (args) {
              args.events = _this._eventMap[getDateStr(args.date)];
              _this._hook('onCellHoverIn', args);
          };
          /** @hidden */
          _this._onCellHoverOut = function (args) {
              args.events = _this._eventMap[getDateStr(args.date)];
              _this._hook('onCellHoverOut', args);
          };
          /** @hidden */
          _this._onEventHoverIn = function (args) {
              _this._hoverTimer = setTimeout(function () {
                  _this._isHover = true;
                  _this._eventClick('onEventHoverIn', args);
              }, 150);
          };
          /** @hidden */
          _this._onEventHoverOut = function (args) {
              clearTimeout(_this._hoverTimer);
              if (_this._isHover) {
                  _this._isHover = false;
                  _this._eventClick('onEventHoverOut', args);
              }
          };
          /** @hidden */
          _this._onEventClick = function (args) {
              var s = _this.s;
              _this._handleMultipleSelect(args);
              var close = _this._eventClick('onEventClick', args);
              if (close !== false && !(s.selectMultipleEvents || s.eventDelete || ((s.dragToCreate || s.clickToCreate) && s.eventDelete !== false))) {
                  _this._hidePopover();
              }
          };
          /** @hidden */
          _this._onEventDoubleClick = function (args) {
              _this._eventClick('onEventDoubleClick', args);
          };
          /** @hidden */
          _this._onEventRightClick = function (args) {
              _this._eventClick('onEventRightClick', args);
          };
          /** @hidden */
          _this._onEventDragEnd = function (args) {
              _this._hook('onEventDragEnd', args);
          };
          /** @hidden */
          _this._onEventDragStart = function (args) {
              _this._hook('onEventDragStart', args);
          };
          /** @hidden */
          _this._onEventDragEnter = function (args) {
              _this._hook('onEventDragEnter', args);
          };
          /** @hidden */
          _this._onEventDragLeave = function (args) {
              _this._hook('onEventDragLeave', args);
          };
          /** @hidden */
          _this._onLabelHoverIn = function (args) {
              _this._hoverTimer = setTimeout(function () {
                  _this._isHover = true;
                  _this._labelClick('onEventHoverIn', args);
              }, 150);
          };
          /** @hidden */
          _this._onLabelHoverOut = function (args) {
              clearTimeout(_this._hoverTimer);
              if (_this._isHover) {
                  _this._isHover = false;
                  _this._labelClick('onEventHoverOut', args);
              }
          };
          /** @hidden */
          _this._onLabelClick = function (args) {
              _this._handleMultipleSelect(args);
              _this._hook('onLabelClick', args);
              _this._labelClick('onEventClick', args);
              _this._isEventClick = true;
              if (!args.label) {
                  _this._moreLabelClicked = true;
              }
          };
          /** @hidden */
          _this._onLabelDoubleClick = function (args) {
              _this._labelClick('onEventDoubleClick', args);
          };
          /** @hidden */
          _this._onLabelRightClick = function (args) {
              _this._labelClick('onEventRightClick', args);
          };
          /** @hidden */
          _this._onCellClick = function (args) {
              _this._resetSelection();
              _this._cellClick('onCellClick', args);
          };
          /** @hidden */
          _this._onCellDoubleClick = function (args) {
              _this._cellClick('onCellDoubleClick', args);
          };
          /** @hidden */
          _this._onCellRightClick = function (args) {
              _this._cellClick('onCellRightClick', args);
          };
          /** @hidden */
          _this._proxy = function (args) {
              // Needed to set the event calendar instance on any emitted event
              _this._hook(args.type, args);
          };
          /** @hidden */
          _this._onPageChange = function (args) {
              // Next cycle
              setTimeout(function () {
                  _this._hidePopover();
              });
              _this._isPageChange = true;
              _this._hook('onPageChange', args);
          };
          /** @hidden */
          _this._onPageLoading = function (args) {
              var s = _this.s;
              var eventMap = getEventMap(_this._events, args.viewStart, args.viewEnd, s);
              _this._colorsMap = getEventMap(s.colors, args.viewStart, args.viewEnd, s);
              _this._invalidsMap = getEventMap(s.invalid, args.viewStart, args.viewEnd, s, true);
              _this._validsMap = getEventMap(s.valid, args.viewStart, args.viewEnd, s, true);
              _this._eventMap = eventMap;
              _this._firstDay = getFirstDayOfWeek(args.firstDay, s, _this._firstWeekDay);
              _this._lastDay = args.lastDay;
              _this._labelsMap = _this._marksMap = UNDEFINED;
              if (!s.labels && (_this._showEventLabels || _this._showEventCount)) {
                  _this._labelsMap = eventMap;
              }
              else if (!s.marked) {
                  _this._marksMap = eventMap;
              }
              if (args.viewChanged) {
                  _this._hook('onPageLoading', args);
              }
          };
          /** @hidden */
          _this._onPageLoaded = function (args) {
              _this._shouldAnimateScroll = _this._isPageChange;
              _this._isPageChange = false;
              var viewType = _this._eventListType;
              // Generate event list
              if (_this._showEventList && !(_this._showCalendar && viewType === 'day')) {
                  var s = _this.s;
                  var month = args.month;
                  var isMonthList = _this._showEventList && month && viewType === 'month';
                  var firstDay = isMonthList ? month : args.firstDay;
                  var lastDay = isMonthList ? s.getDate(s.getYear(month), s.getMonth(month) + _this._eventListSize, 1) : args.lastDay;
                  _this._setEventList(firstDay, lastDay);
              }
              _this._hook('onPageLoaded', args);
          };
          /** @hidden */
          _this._onPopoverClose = function () {
              _this._hidePopover();
          };
          /** @hidden */
          _this._onResize = function (ev) {
              var isListScrollable;
              if (_this._showEventList && isBrowser) {
                  // Calculate the available height for the event list
                  var cal = ev.target;
                  var height = cal.offsetHeight;
                  var calTop = cal.getBoundingClientRect().top;
                  var listTop = _this._list.getBoundingClientRect().top;
                  isListScrollable = height - listTop + calTop > 170;
              }
              _this.setState({ height: ev.height, isListScrollable: isListScrollable, width: ev.width });
          };
          /** @hidden */
          _this._onSelectedEventsChange = function (events) {
              _this._emit('selectedEventsChange', events); // needed for the two-way binding to work (copied from _selectedChange)
              _this._hook('onSelectedEventsChange', { events: events });
          };
          //#region Drag & Drop
          /** @hidden */
          _this._getDragDates = function (start, end, event) {
              var draggedDates = {};
              var firstWeekDay = _this._firstWeekDay;
              var endDate = getEndDate(_this.s, event.allDay, start, end, true);
              var until = getDateOnly(addDays(endDate, 1));
              for (var d = getDateOnly(start); d < until; d.setDate(d.getDate() + 1)) {
                  var weekDay = d.getDay();
                  var offset = firstWeekDay - weekDay > 0 ? 7 : 0;
                  if (isSameDay(start, d) || weekDay === firstWeekDay) {
                      draggedDates[getDateStr(d)] = {
                          event: event,
                          width: Math.min(getDayDiff(d, endDate) + 1, 7 + firstWeekDay - weekDay - offset) * 100,
                      };
                  }
                  else {
                      draggedDates[getDateStr(d)] = {};
                  }
              }
              return draggedDates;
          };
          /** @hidden */
          _this._onLabelUpdateModeOn = function (args) {
              var event = args.create ? _this._tempEvent : args.data;
              if (event) {
                  var start = makeDate(event.start);
                  var end = makeDate(event.end || start);
                  _this.setState({
                      isTouchDrag: true,
                      labelDragData: {
                          draggedEvent: event,
                          originDates: args.external ? UNDEFINED : _this._getDragDates(start, end, event),
                      },
                  });
              }
          };
          /** @hidden */
          _this._onLabelUpdateModeOff = function (args) {
              var event = args.create ? _this._tempEvent : args.data;
              _this._hook('onEventDragEnd', {
                  domEvent: args.domEvent,
                  event: event,
                  source: 'calendar',
              });
              _this.setState({
                  isTouchDrag: false,
                  labelDragData: UNDEFINED,
              });
          };
          /** @hidden */
          _this._onLabelUpdateStart = function (args) {
              var s = _this.s;
              var el = _this._el;
              if (s.externalDrag && args.drag && !args.create) {
                  var eventEl = el.querySelector(".mbsc-calendar-label[data-id='" + args.data.id + "']") || closest(args.domEvent.target, '.mbsc-list-item');
                  if (eventEl) {
                      var clone = eventEl.cloneNode(true);
                      var cloneClass = clone.classList;
                      clone.style.display = 'none';
                      cloneClass.add('mbsc-drag-clone', 'mbsc-schedule-drag-clone', 'mbsc-font');
                      cloneClass.remove('mbsc-calendar-label-hover', 'mbsc-hover', 'mbsc-focus', 'mbsc-active');
                      _this._clone = clone;
                      _this._body = getDocument(el).body;
                      _this._body.appendChild(clone);
                      _this._eventDropped = false;
                      dragObservable.next(__assign({}, args, { create: true, event: args.data, eventName: 'onDragStart', external: true, from: _this }));
                  }
              }
              var weekNumWidth = _this._showWeekNumbers ? el.querySelector('.mbsc-calendar-week-nr').getBoundingClientRect().width : 0;
              var slide = el.querySelectorAll('.mbsc-calendar-slide')[_this._calendarLabelList === 'all' || isNumeric(_this._calendarLabelList) ? 0 : 1];
              var slideRect = slide.getBoundingClientRect();
              var weeksCont = el.querySelector('.mbsc-calendar-week-days');
              var rows = slide.querySelectorAll('.mbsc-calendar-row');
              var isClick = /click/.test(args.domEvent.type);
              _this._areaTop = 0;
              if (weeksCont) {
                  var weeksRect = weeksCont.getBoundingClientRect();
                  _this._areaTop = weeksRect.top + weeksRect.height;
              }
              _this._areaLeft = slideRect.left + (s.rtl ? 0 : weekNumWidth);
              _this._areaBottom = slideRect.top + slideRect.height;
              _this._areaRight = _this._areaLeft + slideRect.width - (s.rtl ? weekNumWidth : 0);
              _this._calCellWidth = (_this._areaRight - _this._areaLeft) / 7;
              var newWeek = 0;
              _this._rowTops = [];
              rows.forEach(function (r, i) {
                  var rowTop = r.getBoundingClientRect().top - _this._areaTop;
                  _this._rowTops.push(rowTop);
                  if (args.endY - _this._areaTop > rowTop) {
                      newWeek = i;
                  }
              });
              if (args.create) {
                  var newDay = floor((s.rtl ? _this._areaRight - args.endX : args.endX - _this._areaLeft) / _this._calCellWidth);
                  var newStartDay = addDays(_this._firstDay, newWeek * 7 + newDay);
                  var newStart = new Date(newStartDay.getFullYear(), newStartDay.getMonth(), newStartDay.getDate());
                  var nextDay = addDays(newStart, 1);
                  var newEnd = s.exclusiveEndDates ? nextDay : new Date(+nextDay - 1);
                  var eventData = s.extendDefaultEvent ? s.extendDefaultEvent({ start: newStart }) : UNDEFINED;
                  _this._tempEvent = __assign({ allDay: true, end: newEnd, id: getEventId(), start: newStart, title: s.newEventText }, args.event, eventData);
              }
              if (!isClick) {
                  _this._hook('onEventDragStart', {
                      action: args.create ? 'create' : args.resize ? 'resize' : 'move',
                      domEvent: args.domEvent,
                      event: args.create ? _this._tempEvent : args.data,
                      source: 'calendar',
                  });
              }
          };
          /** @hidden */
          _this._onLabelUpdateMove = function (args) {
              var s = _this.s;
              var event = args.create ? _this._tempEvent : args.data;
              var draggedEvent = __assign({}, event);
              var labelDragData = _this.state.labelDragData;
              var tzOpt = event.allDay ? UNDEFINED : s;
              if (s.externalDrag && args.drag && !args.create && _this._clone) {
                  dragObservable.next(__assign({}, args, { clone: _this._clone, create: true, event: args.data, eventName: 'onDragMove', external: true, from: _this }));
                  if (!_this._onCalendar) {
                      moveClone(args, _this._clone);
                      if (!labelDragData || !labelDragData.draggedEvent) {
                          // In case of instant drag the dragged event is not set
                          _this.setState({ labelDragData: { draggedEvent: draggedEvent } });
                      }
                      return;
                  }
              }
              if (args.endY > _this._areaTop && args.endY < _this._areaBottom && args.endX > _this._areaLeft && args.endX < _this._areaRight) {
                  var newDay = floor((s.rtl ? _this._areaRight - args.endX : args.endX - _this._areaLeft) / _this._calCellWidth);
                  var oldDay = floor((s.rtl ? _this._areaRight - args.startX : args.startX - _this._areaLeft) / _this._calCellWidth);
                  var newWeek_1 = 0;
                  var oldWeek_1 = 0;
                  _this._rowTops.forEach(function (rowTop, i) {
                      if (args.startY - _this._areaTop > rowTop) {
                          oldWeek_1 = i;
                      }
                      if (args.endY - _this._areaTop > rowTop) {
                          newWeek_1 = i;
                      }
                  });
                  var dayDelta = (newWeek_1 - oldWeek_1) * 7 + (newDay - oldDay);
                  if (newDay !== _this._tempDay || newWeek_1 !== _this._tempWeek) {
                      var start = makeDate(event.start, tzOpt);
                      var end = makeDate(event.end, tzOpt) || start;
                      var newStart = start;
                      var newEnd = end;
                      if (args.external) {
                          var ms = getDayMilliseconds(start);
                          var duration = +end - +start;
                          newStart = createDate(s, +addDays(_this._firstDay, newWeek_1 * 7 + newDay) + ms);
                          newEnd = createDate(s, +newStart + duration);
                      }
                      else if (args.drag) {
                          // Drag
                          if (!computeEventDragInTime(event.dragInTime, UNDEFINED, s.dragInTime)) {
                              return;
                          }
                          newStart = addDays(start, dayDelta);
                          newEnd = addDays(end, dayDelta);
                      }
                      else {
                          // Resize, create
                          var rtl = s.rtl ? -1 : 1;
                          var endResize = args.create ? (newWeek_1 === oldWeek_1 ? args.deltaX * rtl > 0 : dayDelta > 0) : args.direction === 'end';
                          var days = getDayDiff(start, end);
                          if (endResize) {
                              newEnd = addDays(end, Math.max(-days, dayDelta));
                          }
                          else {
                              newStart = addDays(start, Math.min(days, dayDelta));
                          }
                          // Don't allow end date before start date when resizing
                          if (newEnd < newStart) {
                              if (endResize) {
                                  newEnd = createDate(tzOpt, newStart);
                              }
                              else {
                                  newStart = createDate(tzOpt, newEnd);
                              }
                          }
                      }
                      draggedEvent.start = newStart;
                      draggedEvent.end = newEnd;
                      if (!/mbsc-popover-hidden/.test(_this._popoverClass)) {
                          _this._popoverClass = _this._popoverClass + ' mbsc-popover-hidden';
                      }
                      _this.setState({
                          labelDragData: {
                              draggedDates: _this._getDragDates(newStart, newEnd, draggedEvent),
                              draggedEvent: draggedEvent,
                              originDates: labelDragData && labelDragData.originDates,
                          },
                      });
                      _this._tempDay = newDay;
                      _this._tempWeek = newWeek_1;
                  }
              }
          };
          /** @hidden */
          _this._onLabelUpdateEnd = function (args) {
              var state = _this.state;
              var isCreating = args.create;
              var dragData = state.labelDragData || {};
              var event = isCreating ? _this._tempEvent : args.data;
              var draggedEvent = dragData.draggedEvent || event;
              var origStart = makeDate(event.start);
              var origEnd = makeDate(event.end);
              var newStart = makeDate(draggedEvent.start);
              var newEnd = makeDate(draggedEvent.end);
              var changed = isCreating || +origStart !== +newStart || +origEnd !== +newEnd;
              var draggedEventData = {
                  allDay: event.allDay,
                  endDate: newEnd,
                  original: event,
                  startDate: newStart,
              };
              var eventLeft = false;
              if (_this.s.externalDrag && args.drag && !args.create && _this._clone) {
                  dragObservable.next(__assign({}, args, { action: 'externalDrop', create: true, event: args.data, eventName: 'onDragEnd', external: true, from: _this }));
                  _this._body.removeChild(_this._clone);
                  _this._clone = UNDEFINED;
                  if (!_this._onCalendar) {
                      eventLeft = true;
                      if (_this._eventDropped) {
                          args.event = event;
                          _this._onEventDelete(args);
                      }
                  }
              }
              var action = args.action || (dragData.draggedEvent ? 'drag' : 'click');
              var allowUpdate = !eventLeft &&
                  (changed
                      ? _this._onEventDragStop({
                          action: action,
                          collision: checkInvalidCollision(_this.s, _this._invalidsMap, _this._validsMap, newStart, newEnd, _this._minDate, _this._maxDate, _this.s.invalidateEvent, _this.s.exclusiveEndDates),
                          create: isCreating,
                          domEvent: args.domEvent,
                          event: draggedEventData,
                          from: args.from,
                          source: 'calendar',
                      })
                      : true);
              var keepDragMode = state.isTouchDrag && !eventLeft && (!isCreating || allowUpdate);
              if (!keepDragMode && action !== 'click') {
                  _this._hook('onEventDragEnd', {
                      domEvent: args.domEvent,
                      event: event,
                      source: 'calendar',
                  });
              }
              _this.setState({
                  isTouchDrag: keepDragMode,
                  labelDragData: keepDragMode
                      ? {
                          draggedEvent: allowUpdate ? draggedEvent : __assign({}, event),
                          originDates: allowUpdate ? _this._getDragDates(newStart, newEnd, draggedEventData.original) : dragData.originDates,
                      }
                      : {},
              });
              if (args.drag) {
                  _this._hidePopover();
              }
              _this._tempWeek = -1;
              _this._tempDay = -1;
          };
          /** @hidden */
          _this._onEventDragStop = function (args) {
              var s = _this.s;
              var action = args.action;
              var resource = args.resource;
              var slot = args.slot;
              var invalidCollision = args.collision;
              var isCreating = args.create;
              var source = args.source;
              var draggedEvent = args.event;
              var event = draggedEvent.original;
              // In case of recurring event original refers to the occurrence,
              var originalEvent = event.recurring ? event.original : event;
              var oldEvent = __assign({}, originalEvent);
              var eventCopy = __assign({}, originalEvent);
              var eventTz = event.timezone;
              var origStart = convertTimezone(event.start, s, eventTz);
              var start = convertTimezone(draggedEvent.startDate, s, eventTz);
              var end = convertTimezone(draggedEvent.endDate, s, eventTz);
              var allDay = draggedEvent.allDay;
              var isRecurring = eventCopy.recurring;
              if (isRecurring) {
                  // add original start date to exceptions
                  eventCopy.recurringException = getExceptionList(eventCopy.recurringException).concat([origStart]);
              }
              else {
                  // Update the copy of the original event
                  eventCopy.allDay = allDay;
                  eventCopy.start = start;
                  eventCopy.end = end;
                  if (resource !== UNDEFINED) {
                      eventCopy.resource = resource;
                  }
                  if (slot !== UNDEFINED) {
                      eventCopy.slot = slot;
                  }
              }
              var allowUpdate = false;
              var newEvent = isRecurring ? __assign({}, originalEvent) : originalEvent;
              if (isCreating || isRecurring) {
                  if (isRecurring) {
                      // remove recurring property
                      delete newEvent.recurring;
                  }
                  if (isRecurring || newEvent.id === UNDEFINED) {
                      newEvent.id = getEventId();
                  }
                  if (resource !== UNDEFINED) {
                      newEvent.resource = resource;
                  }
                  if (slot !== UNDEFINED) {
                      newEvent.slot = slot;
                  }
                  newEvent.start = start;
                  newEvent.end = end;
                  newEvent.allDay = allDay;
                  allowUpdate =
                      _this._hook('onEventCreate', __assign({ action: action, domEvent: args.domEvent, event: newEvent, source: source }, (isRecurring && { originEvent: event }))) !== false;
                  if (invalidCollision !== false) {
                      allowUpdate = false;
                      _this._hook('onEventCreateFailed', __assign({ action: action, event: newEvent, invalid: invalidCollision, source: source }, (isRecurring && { originEvent: event })));
                  }
              }
              if (!isCreating || isRecurring) {
                  allowUpdate =
                      _this._hook('onEventUpdate', __assign({ domEvent: args.domEvent, event: eventCopy, oldEvent: oldEvent,
                          source: source }, (isRecurring && { newEvent: newEvent, oldEventOccurrence: event }))) !== false;
                  if (invalidCollision !== false) {
                      allowUpdate = false;
                      _this._hook('onEventUpdateFailed', __assign({ event: eventCopy, invalid: invalidCollision, oldEvent: oldEvent,
                          source: source }, (isRecurring && { newEvent: newEvent, oldEventOccurrence: event })));
                  }
              }
              if (allowUpdate) {
                  if (args.from) {
                      // If the external item comes from another calendar, notify the instance from the drop
                      args.from._eventDropped = true;
                  }
                  if (isCreating || isRecurring) {
                      _this._events.push(newEvent);
                      _this._triggerCreated = {
                          action: action,
                          event: newEvent,
                          source: source,
                      };
                  }
                  if (!isCreating || isRecurring) {
                      // Handle recurring event
                      if (isRecurring) {
                          draggedEvent.id = newEvent.id;
                          draggedEvent.original = newEvent;
                          originalEvent.recurringException = eventCopy.recurringException;
                      }
                      else {
                          // Update the original event
                          originalEvent.start = start;
                          originalEvent.end = end;
                          originalEvent.allDay = allDay;
                          if (resource !== UNDEFINED) {
                              originalEvent.resource = resource;
                          }
                          if (slot !== UNDEFINED) {
                              originalEvent.slot = slot;
                          }
                      }
                      _this._triggerUpdated = {
                          event: originalEvent,
                          oldEvent: oldEvent,
                          source: source,
                      };
                  }
                  _this._refresh = true;
                  if (source !== 'calendar') {
                      _this.forceUpdate();
                  }
              }
              return allowUpdate;
          };
          /** @hidden */
          _this._onExternalDrag = function (args) {
              var s = _this.s;
              var clone = args.clone;
              var isSelf = args.from === _this;
              var externalDrop = !isSelf && s.externalDrop;
              var instantDrag = isSelf && s.externalDrag && !s.dragToMove;
              var dragData = _this.state.labelDragData;
              if (_this._showCalendar && (externalDrop || s.externalDrag)) {
                  var isInArea = !instantDrag &&
                      args.endY > _this._areaTop &&
                      args.endY < _this._areaBottom &&
                      args.endX > _this._areaLeft &&
                      args.endX < _this._areaRight;
                  switch (args.eventName) {
                      case 'onDragModeOff':
                          if (externalDrop) {
                              _this._onLabelUpdateModeOff(args);
                          }
                          break;
                      case 'onDragModeOn':
                          if (externalDrop) {
                              _this._onLabelUpdateModeOn(args);
                          }
                          break;
                      case 'onDragStart':
                          if (externalDrop) {
                              _this._onLabelUpdateStart(args);
                          }
                          else if (isSelf) {
                              _this._onCalendar = true;
                          }
                          break;
                      case 'onDragMove':
                          if (!isSelf && !externalDrop) {
                              return;
                          }
                          if (isInArea) {
                              if (!_this._onCalendar) {
                                  _this._hook('onEventDragEnter', {
                                      domEvent: args.domEvent,
                                      event: args.event,
                                      source: 'calendar',
                                  });
                              }
                              if (isSelf || externalDrop) {
                                  clone.style.display = 'none';
                              }
                              if (externalDrop) {
                                  _this._onLabelUpdateMove(args);
                              }
                              _this._onCalendar = true;
                          }
                          else if (_this._onCalendar) {
                              _this._hook('onEventDragLeave', {
                                  domEvent: args.domEvent,
                                  event: args.event,
                                  source: 'calendar',
                              });
                              clone.style.display = 'table';
                              if (!isSelf || (dragData && dragData.draggedEvent)) {
                                  _this.setState({
                                      labelDragData: {
                                          draggedDates: {},
                                          draggedEvent: isSelf ? dragData && dragData.draggedEvent : UNDEFINED,
                                          originDates: isSelf ? dragData && dragData.originDates : UNDEFINED,
                                      },
                                  });
                              }
                              _this._tempWeek = -1;
                              _this._tempDay = -1;
                              _this._onCalendar = false;
                          }
                          break;
                      case 'onDragEnd':
                          // this is needed, otherwise it creates event on drag click
                          if (externalDrop) {
                              if (!isInArea) {
                                  _this.setState({
                                      labelDragData: UNDEFINED,
                                  });
                                  _this._hook('onEventDragEnd', {
                                      domEvent: args.domEvent,
                                      event: args.event,
                                      source: 'calendar',
                                  });
                              }
                              else {
                                  _this._onLabelUpdateEnd(args);
                              }
                          }
                          break;
                  }
              }
          };
          //#endregion Drag & drop
          /** @hidden */
          _this._onEventDelete = function (args) {
              var _a;
              var s = _this.s;
              if ((s.eventDelete === UNDEFINED && !s.dragToCreate && !s.clickToCreate) || s.eventDelete === false) {
                  return;
              }
              var changed = false;
              var hasRecurring = false;
              var hasNonRecurring = false;
              var originalEvent;
              var oldEvent;
              var eventCopy;
              var event = args.event;
              var occurrence = event;
              var isMultiple = s.selectMultipleEvents;
              var selectedEventsMap = isMultiple ? _this._selectedEventsMap : (_a = {}, _a[event.id] = event, _a);
              var selectedEvents = toArray(selectedEventsMap);
              var oldEvents = [];
              var recurringEvents = [];
              var updatedEvents = [];
              var updatedEventsMap = {};
              var events = [];
              for (var _i = 0, selectedEvents_1 = selectedEvents; _i < selectedEvents_1.length; _i++) {
                  var selectedEvent = selectedEvents_1[_i];
                  if (selectedEvent.recurring) {
                      occurrence = selectedEvent;
                      originalEvent = selectedEvent.original;
                      hasRecurring = true;
                      var id = originalEvent.id;
                      if (updatedEventsMap[id]) {
                          eventCopy = updatedEventsMap[id];
                      }
                      else {
                          oldEvent = __assign({}, originalEvent);
                          eventCopy = __assign({}, originalEvent);
                          recurringEvents.push(originalEvent);
                          oldEvents.push(oldEvent);
                          updatedEvents.push(eventCopy);
                          updatedEventsMap[id] = eventCopy;
                      }
                      // add original start date to exceptions
                      var origStart = convertTimezone(selectedEvent.start, s);
                      eventCopy.recurringException = getExceptionList(eventCopy.recurringException).concat([origStart]);
                  }
                  else {
                      hasNonRecurring = true;
                      event = selectedEvent;
                      events.push(selectedEvent);
                  }
              }
              if (hasRecurring) {
                  var allowUpdate = _this._hook('onEventUpdate', {
                      domEvent: args.domEvent,
                      event: eventCopy,
                      events: isMultiple ? updatedEvents : UNDEFINED,
                      isDelete: true,
                      oldEvent: isMultiple ? UNDEFINED : oldEvent,
                      oldEventOccurrence: occurrence,
                      oldEvents: isMultiple ? oldEvents : UNDEFINED,
                  }) !== false;
                  if (allowUpdate) {
                      changed = true;
                      for (var _b = 0, recurringEvents_1 = recurringEvents; _b < recurringEvents_1.length; _b++) {
                          var recurringEvent = recurringEvents_1[_b];
                          var updatedEvent = updatedEventsMap[recurringEvent.id];
                          recurringEvent.recurringException = updatedEvent.recurringException;
                      }
                      _this._triggerUpdated = {
                          event: originalEvent,
                          events: isMultiple ? recurringEvents : UNDEFINED,
                          oldEvent: isMultiple ? UNDEFINED : oldEvent,
                          oldEvents: isMultiple ? oldEvents : UNDEFINED,
                          source: args.source,
                      };
                  }
              }
              if (hasNonRecurring) {
                  var allowDelete = _this._hook('onEventDelete', {
                      domEvent: args.domEvent,
                      event: event,
                      events: isMultiple ? events : UNDEFINED,
                  }) !== false;
                  if (allowDelete) {
                      changed = true;
                      _this._events = _this._events.filter(function (e) { return !selectedEventsMap[e.id]; });
                      _this._selectedEventsMap = {};
                      _this._triggerDeleted = {
                          event: event,
                          events: isMultiple ? events : UNDEFINED,
                          source: args.source,
                      };
                  }
              }
              if (changed) {
                  _this._hidePopover();
                  _this.refresh();
              }
          };
          /** @hidden */
          _this._setEl = function (el) {
              _this._el = el ? el._el || el : null;
              _this._calendarView = el;
              // this._instanceService.instance = this;
          };
          /** @hidden */
          _this._setList = function (el) {
              _this._list = el;
          };
          /** @hidden */
          _this._setPopoverList = function (list) {
              _this._popoverList = list && list._el;
          };
          // tslint:disable-next-line: variable-name
          _this._onKeyDown = function (ev) {
              if (ev.keyCode === TAB) {
                  _this._resetSelection();
              }
          };
          return _this;
      }
      /**
       * @hidden
       * Adds one or more events to the calendar
       *
       * @param events - Object or Array containing the events.
       * @returns - An array containing the list of IDs generated for the events.
       */
      EventcalendarBase.prototype.addEvent = function (events) {
          // TODO: check if id exists already?
          var eventsToAdd = isArray(events) ? events : [events];
          var ids = [];
          var data = prepareEvents(eventsToAdd);
          for (var _i = 0, data_1 = data; _i < data_1.length; _i++) {
              var event_1 = data_1[_i];
              ids.push('' + event_1.id);
              this._events.push(event_1);
          }
          this.refresh();
          return ids;
      };
      /**
       * Returns the [events](#opt-data) between two dates. If `start` and `end` are not specified,
       * it defaults to the start and end days of the current view.
       * If `end` is not specified, it defaults to start date + 1 day.
       *
       * @param start - Start date of the specified interval.
       * @param end  - End date of the specified interval.
       * @returns - An array containing the event objects.
       */
      EventcalendarBase.prototype.getEvents = function (start, end) {
          return getDataInRange(this._events, this.s, this._firstDay, this._lastDay, start, end);
      };
      /**
       * Returns the [invalids](#opt-invalid) between two dates. If `start` and `end` are not specified,
       * it defaults to the start and end days of the current view.
       * If `end` is not specified, it defaults to start date + 1 day.
       *
       * @param start - Start date of the specified interval.
       * @param end  - End date of the specified interval.
       * @returns - An array containing the invalid objects.
       */
      EventcalendarBase.prototype.getInvalids = function (start, end) {
          return getDataInRange(this.s.invalid, this.s, this._firstDay, this._lastDay, start, end);
      };
      /**
       * @hidden
       * Returns the selected events.
       *
       * @returns - An array containing the selected events.
       *
       * The selected events can be set with the [setSelectedEvents](#method-setSelectedEvents) method.
       * Multiple event selection can be turned on with the [selectMultipleEvents](#opt-selectMultipleEvents) option.
       */
      EventcalendarBase.prototype.getSelectedEvents = function () {
          return toArray(this._selectedEventsMap);
      };
      /**
       * @hidden
       * Set the events for the calendar. The previous events will be overwritten.
       * Returns the list of IDs generated for the events.
       *
       * @param events - An array containing the events.
       * @returns - An array containing the event IDs.
       */
      EventcalendarBase.prototype.setEvents = function (events) {
          var ids = [];
          var data = prepareEvents(events);
          for (var _i = 0, data_2 = data; _i < data_2.length; _i++) {
              var event_2 = data_2[_i];
              ids.push('' + event_2.id);
          }
          this._events = data;
          this.refresh();
          return ids;
      };
      /**
       * @hidden
       * Sets the selected events.
       *
       * @param selectedEvents - An array containing the selected events.
       *
       * The selected events are returned by the [getSelectedEvents](#method-getSelectedEvents) method.
       * Multiple event selection can be turned on with the [selectMultipleEvents](#opt-selectMultipleEvents) option.
       */
      EventcalendarBase.prototype.setSelectedEvents = function (selectedEvents) {
          this._selectedEventsMap = (selectedEvents || []).reduce(function (map, ev) {
              if (ev.occurrenceId) {
                  map[ev.occurrenceId] = ev;
              }
              else {
                  map[ev.id] = ev;
              }
              return map;
          }, {});
          this.forceUpdate();
      };
      /**
       * @hidden
       * Removes one or more events from the event list based on IDs. For events without IDs, the IDs are generated internally.
       * The generated ids are returned by the [addEvent](#method-addEvent) or [getEvents](#method-getEvents) methods.
       *
       * @param events - An array containing IDs or the event objects to be deleted.
       */
      EventcalendarBase.prototype.removeEvent = function (events) {
          var eventsToRemove = isArray(events) ? events : [events];
          var data = this._events;
          var len = data.length;
          for (var _i = 0, eventsToRemove_1 = eventsToRemove; _i < eventsToRemove_1.length; _i++) {
              var eventToRemove = eventsToRemove_1[_i];
              var found = false;
              var i = 0;
              while (!found && i < len) {
                  var event_3 = data[i];
                  if (event_3.id === eventToRemove || event_3.id === eventToRemove.id) {
                      found = true;
                      data.splice(i, 1);
                  }
                  i++;
              }
          }
          this.refresh();
      };
      /**
       * Navigates to the specified event on the calendar.
       * @param event - The event object. The `id`, `start` and `resource`
       * (in case if resources are used in timeline or schedule views) properties must be present in the object.
       */
      EventcalendarBase.prototype.navigateToEvent = function (event) {
          this._navigateToEvent = event;
          this._shouldScrollSchedule++;
          this.navigate(event.start, true);
      };
      /**
       * Navigates to the specified date on the calendar.
       * @param date - Date to navigate to.
       */
      EventcalendarBase.prototype.navigate = function (date, animate) {
          var d = +makeDate(date);
          var isNavigate = this._navigateToEvent !== UNDEFINED;
          var changed = d !== this._selectedDateTime;
          if (changed || isNavigate) {
              this._shouldAnimateScroll = !!animate;
          }
          if (this.s.selectedDate === UNDEFINED) {
              if ((this._showSchedule || this._showTimeline) && !changed) {
                  // If we navigate to the already selected date, we should still force a scroll on the view
                  this._shouldScrollSchedule++;
                  this.forceUpdate();
              }
              else {
                  this.setState({ selectedDate: d });
              }
          }
          else if (changed || isNavigate) {
              // In controlled mode just trigger a selected date change event
              this._selectedChange(d);
          }
      };
      /**
       * @hidden
       * Updates one or more events in the event calendar.
       * @param events - The event or events to update.
       */
      EventcalendarBase.prototype.updateEvent = function (events) {
          var eventsToUpdate = isArray(events) ? events : [events];
          var data = this._events;
          var len = data.length;
          for (var _i = 0, eventsToUpdate_1 = eventsToUpdate; _i < eventsToUpdate_1.length; _i++) {
              var eventToUpdate = eventsToUpdate_1[_i];
              var found = false;
              var i = 0;
              while (!found && i < len) {
                  var event_4 = data[i];
                  if (event_4.id === eventToUpdate.id) {
                      found = true;
                      data.splice(i, 1, __assign({}, eventToUpdate));
                  }
                  i++;
              }
          }
          this.refresh();
      };
      /**
       * @hidden
       * Refreshes the view.
       */
      EventcalendarBase.prototype.refresh = function () {
          this._refresh = true;
          this.forceUpdate();
      };
      // tslint:enable variable-name
      // ---
      EventcalendarBase.prototype._render = function (s, state) {
          var _this = this;
          var prevProps = this._prevS;
          var showDate = this._showDate;
          var timezonesChanged = s.displayTimezone !== prevProps.displayTimezone || s.dataTimezone !== prevProps.dataTimezone;
          var renderList = false;
          var selectedChanged = false;
          var selectedDateTime;
          this._colorEventList = s.eventTemplate !== UNDEFINED || s.renderEvent !== UNDEFINED ? false : s.colorEventList;
          // If we have display timezone set, default to exclusive end dates
          if (s.exclusiveEndDates === UNDEFINED) {
              s.exclusiveEndDates = !!s.displayTimezone;
          }
          if (!isEmpty(s.min)) {
              if (prevProps.min !== s.min) {
                  this._minDate = +makeDate(s.min);
              }
          }
          else {
              this._minDate = -Infinity;
          }
          if (!isEmpty(s.max)) {
              if (prevProps.max !== s.max) {
                  this._maxDate = +makeDate(s.max);
              }
          }
          else {
              this._maxDate = Infinity;
          }
          // Load selected date from prop or state
          if (s.selectedDate !== UNDEFINED) {
              selectedDateTime = +makeDate(s.selectedDate);
          }
          else {
              if (!this._defaultDate) {
                  // Need to save the default date, otherwise if no default selected is specified, new Date will always create a later timestamp
                  this._defaultDate = +(s.defaultSelectedDate !== UNDEFINED ? makeDate(s.defaultSelectedDate) : removeTimezone(createDate(s)));
              }
              selectedDateTime = state.selectedDate || this._defaultDate;
          }
          this.eventList = state.eventList || [];
          if (s.data !== prevProps.data) {
              this._events = prepareEvents(s.data);
              this._refresh = true;
          }
          if (s.invalid !== prevProps.invalid || s.colors !== prevProps.colors || timezonesChanged) {
              this._refresh = true;
          }
          // Process the view option
          if (s.view !== prevProps.view || s.firstDay !== prevProps.firstDay) {
              var firstDay = s.firstDay;
              var view = s.view || {};
              var agenda = view.agenda || {};
              var calendar = view.calendar || {};
              var schedule = view.schedule || {};
              var timeline = view.timeline || {};
              var eventListSize = +(agenda.size || 1);
              var eventListType = agenda.type || 'month';
              var scheduleSize = +(schedule.size || 1);
              var scheduleType = schedule.type || 'week';
              var scheduleStartDay = schedule.startDay !== UNDEFINED ? schedule.startDay : firstDay;
              var scheduleEndDay = schedule.endDay !== UNDEFINED ? schedule.endDay : (firstDay + 6) % 7;
              var scheduleStartTime = schedule.startTime;
              var scheduleEndTime = schedule.endTime;
              var scheduleTimeCellStep = schedule.timeCellStep || 60;
              var scheduleTimeLabelStep = schedule.timeLabelStep || 60;
              var scheduleTimezones = schedule.timezones;
              var showCalendar = !!view.calendar;
              var showEventCount = calendar.count;
              var showEventList = !!view.agenda;
              var showSchedule = !!view.schedule;
              var showScheduleDays = schedule.days !== UNDEFINED
                  ? schedule.days
                  : !showCalendar && showSchedule && !(scheduleType === 'day' && s.resources && s.resources.length > 0 && scheduleSize < 2);
              var showTimeline = !!view.timeline;
              var showTimelineWeekNumbers = timeline.weekNumbers;
              var hasSlots = showTimeline && !!s.slots && s.slots.length > 0;
              var currentTimeIndicator = showTimeline ? timeline.currentTimeIndicator : schedule.currentTimeIndicator;
              var timelineType = timeline.type || 'week';
              var resX = timeline.resolutionHorizontal || timeline.resolution;
              var resXDef = resX || 'hour';
              var timelineResolutionVertical = timeline.resolutionVertical;
              var timelineResolution = timelineResolutionVertical === 'day' && !/hour|day/.test(resXDef) ? 'hour' : resXDef;
              // const timelineResolution = hasSlots ? 'day' : timeline.resolution ||
              //   (timelineType === 'month' || timelineType === 'year' ? 'day' : 'hour');
              var timelineStartDay = timeline.startDay !== UNDEFINED && /hour|day|week/.test(timelineResolution) ? timeline.startDay : firstDay;
              var timelineEndDay = timeline.endDay !== UNDEFINED && /hour|day|week/.test(timelineResolution) ? timeline.endDay : (firstDay + 6) % 7;
              var timelineStartTime = timeline.startTime;
              var timelineEndTime = timeline.endTime;
              var timelineListing = timeline.eventList || hasSlots;
              var timelineSize = +(timeline.size || 1);
              var timelineStep = (/month|year/.test(timelineType) && !resX && timelineResolutionVertical !== 'day') ||
                  timelineResolution === 'day' ||
                  timelineListing
                  ? 1440
                  : 60;
              var timelineTimeCellStep = hasSlots || timelineResolution === 'day' ? timelineStep : timeline.timeCellStep || timelineStep;
              var timelineTimeLabelStep = hasSlots || timelineResolution === 'day' ? timelineStep : timeline.timeLabelStep || timelineStep;
              var calendarType = calendar.type || 'month';
              var showEventLabels = calendar.labels !== UNDEFINED
                  ? !!calendar.labels
                  : !showEventList &&
                      !showSchedule &&
                      !showTimeline &&
                      !s.marked &&
                      !(calendarType === 'year' || (calendarType === 'month' && calendar.size));
              this._calendarScroll = calendar.scroll;
              this._calendarSize = calendar.size || 1;
              this._calendarLabelList = calendar.labels;
              this._calendarType = calendarType;
              this._dragTimeStep = s.dragTimeStep !== UNDEFINED ? s.dragTimeStep : /hour|day/.test(timelineResolution) ? 15 : 1440;
              this._showEventPopover = calendar.popover;
              this._showOuterDays = calendar.outerDays;
              this._showWeekNumbers = calendar.weekNumbers;
              this._popoverClass = calendar.popoverClass || '';
              this._showScheduleAllDay = schedule.allDay !== UNDEFINED ? schedule.allDay : true;
              if (eventListSize !== this._eventListSize ||
                  eventListType !== this._eventListType ||
                  showCalendar !== this._showCalendar ||
                  showEventCount !== this._showEventCount ||
                  showEventLabels !== this._showEventLabels ||
                  showEventList !== this._showEventList ||
                  scheduleSize !== this._scheduleSize ||
                  scheduleType !== this._scheduleType ||
                  showSchedule !== this._showSchedule ||
                  showScheduleDays !== this._showScheduleDays ||
                  scheduleStartDay !== this._scheduleStartDay ||
                  scheduleEndDay !== this._scheduleEndDay ||
                  scheduleStartTime !== this._scheduleStartTime ||
                  scheduleEndTime !== this._scheduleEndTime ||
                  scheduleTimeCellStep !== this._scheduleTimeCellStep ||
                  scheduleTimeLabelStep !== this._scheduleTimeLabelStep ||
                  showTimeline !== this._showTimeline ||
                  timelineStartDay !== this._timelineStartDay ||
                  timelineEndDay !== this._timelineEndDay ||
                  timelineStartTime !== this._timelineStartTime ||
                  timelineEndTime !== this._timelineEndTime ||
                  timelineSize !== this._timelineSize ||
                  timelineType !== this._timelineType ||
                  timelineTimeCellStep !== this._timelineTimeCellStep ||
                  timelineTimeLabelStep !== this._timelineTimeLabelStep ||
                  timelineListing !== this._timelineListing ||
                  timelineResolution !== this._timelineResolution ||
                  timelineResolutionVertical !== this._timelineResolutionVertical) {
                  this._refresh = true;
                  this._viewChanged = true;
              }
              this._currentTimeIndicator = currentTimeIndicator;
              this._eventListSize = eventListSize;
              this._eventListType = eventListType;
              this._scheduleType = scheduleType;
              this._showCalendar = showCalendar;
              this._showEventCount = showEventCount;
              this._showEventLabels = showEventLabels;
              this._showEventList = showEventList;
              this._showSchedule = showSchedule;
              this._showScheduleDays = showScheduleDays;
              this._scheduleStartDay = scheduleStartDay;
              this._scheduleEndDay = scheduleEndDay;
              this._scheduleStartTime = scheduleStartTime;
              this._scheduleEndTime = scheduleEndTime;
              this._scheduleSize = scheduleSize;
              this._scheduleTimeCellStep = scheduleTimeCellStep;
              this._scheduleTimeLabelStep = scheduleTimeLabelStep;
              this._scheduleTimezones = scheduleTimezones;
              this._showTimeline = showTimeline;
              this._showTimelineWeekNumbers = showTimelineWeekNumbers;
              this._timelineSize = timelineSize;
              this._timelineType = timelineType;
              this._timelineStartDay = timelineStartDay;
              this._timelineEndDay = timelineEndDay;
              this._timelineListing = timelineListing;
              this._timelineStartTime = timelineStartTime;
              this._timelineEndTime = timelineEndTime;
              this._timelineTimeCellStep = timelineTimeCellStep;
              this._timelineTimeLabelStep = timelineTimeLabelStep;
              this._timelineRowHeight = timeline.rowHeight;
              this._timelineResolution = timelineResolution;
              this._timelineResolutionVertical = timelineResolutionVertical;
              this._rangeType = showSchedule ? scheduleType : showTimeline ? timelineType : eventListType;
              this._rangeStartDay = showSchedule ? scheduleStartDay : showTimeline ? timelineStartDay : UNDEFINED;
              this._rangeEndDay = showSchedule ? scheduleEndDay : showTimeline ? timelineEndDay : UNDEFINED;
              this._firstWeekDay = showSchedule ? scheduleStartDay : showTimeline ? timelineStartDay : firstDay;
          }
          this._showDate =
              !this._showScheduleDays &&
                  ((this._showSchedule && this._scheduleType === 'day') ||
                      (this._showEventList && this._eventListType === 'day' && this._eventListSize < 2));
          // Check if page reload needed
          var lastPageLoad = this._pageLoad;
          if (this._refresh || s.locale !== prevProps.locale || s.theme !== prevProps.theme) {
              renderList = true;
              this._pageLoad++;
          }
          if (s.resources !== prevProps.resources) {
              this._resourcesMap = (s.resources || []).reduce(function (map, res) {
                  map[res.id] = res;
                  return map;
              }, {});
          }
          if (s.selectMultipleEvents) {
              if (s.selectedEvents !== prevProps.selectedEvents) {
                  this._selectedEventsMap = (s.selectedEvents || []).reduce(function (map, ev) {
                      if (ev.occurrenceId) {
                          map[ev.occurrenceId] = ev;
                      }
                      else {
                          map[ev.id] = ev;
                      }
                      return map;
                  }, {});
              }
          }
          if (this._selectedEventsMap === UNDEFINED) {
              this._selectedEventsMap = {};
          }
          if (s.refDate !== prevProps.refDate) {
              this._refDate = makeDate(s.refDate);
          }
          if (!this._refDate && !this._showCalendar && (this._showSchedule || this._showTimeline)) {
              this._refDate = getDateOnly(new Date());
          }
          if (selectedDateTime !== this._selectedDateTime) {
              this._viewDate = selectedDateTime;
          }
          if (s.cssClass !== prevProps.cssClass || s.className !== prevProps.className || s.class !== prevProps.class) {
              this._checkSize++;
              this._viewChanged = true;
          }
          if (this._viewChanged && this._viewDate && selectedDateTime !== this._viewDate) {
              selectedChanged = true;
              selectedDateTime = this._viewDate;
          }
          // Check if selected date & time changed
          if (selectedDateTime !== this._selectedDateTime || this._viewChanged) {
              var validated = this._showCalendar && (this._showSchedule || this._showTimeline || this._showEventList)
                  ? +getClosestValidDate(new Date(selectedDateTime), s, this._minDate, this._maxDate, UNDEFINED, UNDEFINED, 1)
                  : constrain(selectedDateTime, this._minDate, this._maxDate);
              // In day view (scheduler/timeline), if only certain week days are displayed,
              // we need to change the loaded day, if it's outside of the displayed week days.
              validated = this._getValidDay(validated);
              // Emit selected change event, if change happened.
              if (selectedDateTime !== validated || selectedChanged) {
                  selectedDateTime = validated;
                  setTimeout(function () {
                      _this._selectedChange(selectedDateTime);
                  });
              }
              if (!this._skipScheduleScroll) {
                  this._shouldScrollSchedule++;
              }
              this._selectedDateTime = selectedDateTime;
          }
          var selectedDate = getDateOnly(new Date(selectedDateTime));
          var selected = +selectedDate;
          // Re-format selected date if displayed
          if (selected !== this._selected ||
              showDate !== this._showDate ||
              s.locale !== prevProps.locale ||
              prevProps.dateFormatLong !== s.dateFormatLong) {
              this._selectedDateHeader = this._showDate ? formatDate(s.dateFormatLong, selectedDate, s) : '';
          }
          // Check if selected changed
          if (selected !== this._selected || s.dataTimezone !== prevProps.dataTimezone || s.displayTimezone !== prevProps.displayTimezone) {
              this._shouldScroll = !this._isPageChange && !this._shouldSkipScroll;
              this._shouldAnimateScroll = this._shouldAnimateScroll !== UNDEFINED ? this._shouldAnimateScroll : this._selected !== UNDEFINED;
              this._selected = selected;
              this._selectedDates = {};
              this._selectedDates[+addTimezone(s, new Date(selected))] = true;
              // If the selected date changes, update the active date as well
              this._active = selected;
              renderList = true;
          }
          if (renderList &&
              this._showCalendar &&
              (this._eventListType === 'day' || this._scheduleType === 'day' || this._timelineType === 'day')) {
              this._setEventList(selectedDate, addDays(selectedDate, 1));
          }
          if (this._refresh && state.showPopover) {
              setTimeout(function () {
                  _this._hidePopover();
              });
          }
          this._refresh = false;
          this._cssClass =
              this._className +
                  ' mbsc-eventcalendar' +
                  (this._showEventList ? ' mbsc-eventcalendar-agenda' : '') +
                  (this._showSchedule ? ' mbsc-eventcalendar-schedule' : '') +
                  (this._showTimeline ? ' mbsc-eventcalendar-timeline' : '');
          this._navService.options({
              activeDate: this._active,
              calendarType: this._calendarType,
              endDay: this._showSchedule ? this._scheduleEndDay : this._showTimeline ? this._timelineEndDay : this._rangeEndDay,
              eventRange: this._rangeType,
              eventRangeSize: this._showSchedule ? this._scheduleSize : this._showTimeline ? this._timelineSize : this._eventListSize,
              firstDay: s.firstDay,
              getDate: s.getDate,
              getDay: s.getDay,
              getMonth: s.getMonth,
              getYear: s.getYear,
              max: s.max,
              min: s.min,
              onPageChange: this._onPageChange,
              onPageLoading: this._onPageLoading,
              refDate: this._refDate,
              resolution: this._timelineResolution,
              showCalendar: this._showCalendar,
              showOuterDays: this._showOuterDays,
              size: this._calendarSize,
              startDay: this._rangeStartDay,
              weeks: this._calendarSize,
          }, this._pageLoad !== lastPageLoad);
      };
      EventcalendarBase.prototype._mounted = function () {
          this._unsubscribe = subscribeExternalDrag(this._onExternalDrag);
          listen(this._el, KEY_DOWN, this._onKeyDown);
      };
      EventcalendarBase.prototype._updated = function () {
          var _this = this;
          // Scroll to selected date in the list
          if (this._shouldScroll && this.state.isListScrollable) {
              this._scrollToDay();
              this._shouldScroll = false;
              this._shouldAnimateScroll = UNDEFINED;
          }
          if (this._shouldLoadDays) {
              // In case of custom event listing in jQuery and plain js we need to find
              // the day containers and store them, this is needed to scroll the event list
              // to the selected day, when a day is clicked
              this._shouldLoadDays = false;
              forEach(this._list.querySelectorAll('[mbsc-timestamp]'), function (listItem) {
                  _this._listDays[listItem.getAttribute('mbsc-timestamp')] = listItem;
              });
          }
          if (this._shouldEnhance) {
              this._shouldEnhance = this._shouldEnhance === 'popover' ? this._popoverList : this._list;
          }
          if (this._triggerCreated) {
              var created = this._triggerCreated;
              var target = created.source === 'calendar'
                  ? this._calendarView._body.querySelector(".mbsc-calendar-table-active .mbsc-calendar-text[data-id=\"" + created.event.id + "\"]")
                  : this._el.querySelector(".mbsc-schedule-event[data-id=\"" + created.event.id + "\"]");
              this._hook('onEventCreated', __assign({}, this._triggerCreated, { target: target }));
              this._triggerCreated = null;
          }
          if (this._triggerUpdated) {
              var updated = this._triggerUpdated;
              var target = updated.source === 'calendar'
                  ? this._calendarView._body.querySelector(".mbsc-calendar-table-active .mbsc-calendar-text[data-id=\"" + updated.event.id + "\"]")
                  : this._el.querySelector(".mbsc-schedule-event[data-id=\"" + updated.event.id + "\"]");
              this._hook('onEventUpdated', __assign({}, this._triggerUpdated, { target: target }));
              this._triggerUpdated = null;
          }
          if (this._triggerDeleted) {
              this._hook('onEventDeleted', __assign({}, this._triggerDeleted));
              this._triggerDeleted = null;
          }
          if (this._viewChanged) {
              // setTimeout needed because the scroll event will fire later
              setTimeout(function () {
                  _this._viewChanged = false;
              }, 10);
          }
          if (this._shouldSkipScroll) {
              setTimeout(function () {
                  _this._shouldSkipScroll = false;
              });
          }
          this._skipScheduleScroll = false;
          this._navigateToEvent = UNDEFINED;
      };
      EventcalendarBase.prototype._destroy = function () {
          if (this._unsubscribe) {
              unsubscribeExternalDrag(this._unsubscribe);
          }
          unlisten(this._el, KEY_DOWN, this._onKeyDown);
      };
      EventcalendarBase.prototype._resetSelection = function () {
          // reset selected events if there are any selected
          if (this.s.selectMultipleEvents && Object.keys(this._selectedEventsMap).length > 0) {
              this._selectedEventsMap = {};
              this._onSelectedEventsChange([]);
              this.forceUpdate();
          }
      };
      EventcalendarBase.prototype._getAgendaEvents = function (firstDay, lastDay, eventMap) {
          var _this = this;
          var events = [];
          var s = this.s;
          if (eventMap && this._showEventList) {
              var _loop_1 = function (d) {
                  var eventsForDay = eventMap[getDateStr(d)];
                  if (eventsForDay && eventsForDay.length) {
                      var sorted = sortEvents(eventsForDay, s.eventOrder);
                      events.push({
                          date: formatDate(s.dateFormatLong, d, s),
                          events: sorted.map(function (event) { return _this._getEventData(event, d); }),
                          timestamp: +d,
                      });
                  }
              };
              for (var d = getDateOnly(firstDay); d < lastDay; d.setDate(d.getDate() + 1)) {
                  _loop_1(d);
              }
          }
          return events;
      };
      EventcalendarBase.prototype._getEventData = function (event, d) {
          var s = this.s;
          var res;
          if (!event.color && event.resource) {
              var resItem = isArray(event.resource) ? event.resource : [event.resource];
              res = (this._resourcesMap || {})[resItem[0]];
          }
          var ev = getEventData(s, event, d, this._colorEventList, res, true, true);
          ev.html = this._safeHtml(ev.html);
          return ev;
      };
      /**
       * Returns the timestamp of the closest day which falls between the specified start and end weekdays.
       * @param timestamp The timestamp of the date to validate.
       * @param dir Navigation direction. If not specified, it will return the next valid day, otherwise the next or prev, based on direction.
       */
      EventcalendarBase.prototype._getValidDay = function (timestamp, dir) {
          if (dir === void 0) { dir = 1; }
          var startDay = this._rangeStartDay;
          var endDay = this._rangeEndDay;
          if (!this._showCalendar && this._rangeType === 'day' && startDay !== UNDEFINED && endDay !== UNDEFINED) {
              var date = new Date(timestamp);
              var day = date.getDay();
              var diff = 0;
              // Case 1: endDay < startDay, e.g. Friday -> Monday (5-1)
              // Case 2: endDay >= startDay, e.g. Tuesday -> Friday (2-5)
              if (endDay < startDay ? day > endDay && day < startDay : day > endDay || day < startDay) {
                  // If navigating backwards, we go to end day, otherwise to start day
                  diff = dir < 0 ? endDay - day : startDay - day;
              }
              if (diff) {
                  diff += dir < 0 ? (diff > 0 ? -7 : 0) : diff < 0 ? 7 : 0;
                  return +addDays(date, diff);
              }
          }
          return timestamp;
      };
      EventcalendarBase.prototype._setEventList = function (firstDay, lastDay) {
          var _this = this;
          setTimeout(function () {
              _this._eventListHTML = UNDEFINED;
              _this._shouldScroll = true;
              _this._listDays = null;
              _this._scrollToDay(0);
              _this.setState({
                  eventList: _this._getAgendaEvents(firstDay, lastDay, _this._eventMap),
              });
          });
      };
      EventcalendarBase.prototype._hidePopover = function () {
          if (this.state.showPopover) {
              this.setState({
                  showPopover: false,
              });
          }
      };
      EventcalendarBase.prototype._scrollToDay = function (pos) {
          var _this = this;
          if (this._list) {
              var to = pos;
              var animate = void 0;
              if (pos === UNDEFINED && this._listDays) {
                  var day = this._listDays[this._selected];
                  var eventId = this._navigateToEvent && this._navigateToEvent.id;
                  if (day) {
                      if (eventId !== UNDEFINED) {
                          var event_5 = day.querySelector(".mbsc-event[data-id=\"" + eventId + "\"]");
                          var dayHeader = day.querySelector('.mbsc-event-day');
                          if (event_5) {
                              to = event_5.offsetTop - (dayHeader ? dayHeader.offsetHeight : 0) + 1;
                          }
                      }
                      else {
                          to = day.offsetTop;
                      }
                  }
                  if (to !== UNDEFINED) {
                      animate = this._shouldAnimateScroll;
                  }
              }
              if (to !== UNDEFINED) {
                  this._isListScrolling++;
                  smoothScroll(this._list, UNDEFINED, to, animate, false, function () {
                      setTimeout(function () {
                          _this._isListScrolling--;
                      }, 150);
                  });
              }
          }
      };
      EventcalendarBase.prototype._selectedChange = function (d, skipState) {
          var date = new Date(d);
          if (this.s.selectedDate === UNDEFINED && !skipState) {
              this.setState({ selectedDate: +d });
          }
          this._emit('selectedDateChange', date); // needed for the two-way binding to work - the argument needs to be the value only
          this._hook('onSelectedDateChange', { date: date });
      };
      EventcalendarBase.prototype._cellClick = function (name, args) {
          this._hook(name, __assign({ target: args.domEvent.currentTarget }, args));
      };
      EventcalendarBase.prototype._dayClick = function (name, args) {
          var d = getDateStr(args.date);
          var events = sortEvents(this._eventMap[d], this.s.eventOrder);
          args.events = events;
          this._hook(name, args);
      };
      EventcalendarBase.prototype._labelClick = function (name, args) {
          if (args.label) {
              this._hook(name, {
                  date: args.date,
                  domEvent: args.domEvent,
                  event: args.label,
                  source: 'calendar',
              });
          }
      };
      EventcalendarBase.prototype._eventClick = function (name, args) {
          args.date = new Date(args.date);
          return this._hook(name, args);
      };
      /**
       * Handles multiple event selection on label/event click.
       */
      EventcalendarBase.prototype._handleMultipleSelect = function (args) {
          var event = args.label || args.event;
          if (event && this.s.selectMultipleEvents) {
              var domEvent = args.domEvent;
              var selectedEvents = !domEvent.shiftKey && !domEvent.ctrlKey && !domEvent.metaKey ? {} : this._selectedEventsMap;
              var eventId = event.occurrenceId || event.id;
              if (selectedEvents[eventId]) {
                  delete selectedEvents[eventId];
              }
              else {
                  selectedEvents[eventId] = event;
              }
              this._selectedEventsMap = __assign({}, selectedEvents);
              this._onSelectedEventsChange(toArray(selectedEvents));
              if (this.s.selectedEvents === UNDEFINED) {
                  this.forceUpdate();
              }
          }
      };
      /** @hidden */
      EventcalendarBase.defaults = __assign({}, calendarViewDefaults, { actionableEvents: true, allDayText: 'All-day', data: [], newEventText: 'New event', noEventsText: 'No events', showControls: true, showEventTooltip: true, view: { calendar: { type: 'month' } } });
      // tslint:disable variable-name
      EventcalendarBase._name = 'Eventcalendar';
      return EventcalendarBase;
  }(BaseComponent));

  var InstanceServiceBase = /*#__PURE__*/ (function () {
      function InstanceServiceBase() {
          this.onInstanceReady = new Observable();
          this.onComponentChange = new Observable();
      }
      Object.defineProperty(InstanceServiceBase.prototype, "instance", {
          get: function () {
              return this.inst;
          },
          set: function (inst) {
              this.inst = inst;
              this.onInstanceReady.next(inst);
          },
          enumerable: true,
          configurable: true
      });
      return InstanceServiceBase;
  }());

  // tslint:enable interface-name
  /** @hidden */
  var ListBase = /*#__PURE__*/ (function (_super) {
      __extends(ListBase, _super);
      function ListBase() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      // tslint:enable variable-name
      ListBase.prototype._render = function (s) {
          this._cssClass = this._className + this._rtl + ' mbsc-font mbsc-list' + this._theme;
      };
      return ListBase;
  }(BaseComponent));

  function template(s, inst, content) {
      return (createElement("div", { ref: inst._setEl, className: inst._cssClass }, content));
  }
  /**
   * The List component
   *
   * Usage:
   *
   * ```
   * <List theme="ios">
   *   <ListItem>Items inside</ListItem>
   * </List>
   * ```
   */
  var List = /*#__PURE__*/ (function (_super) {
      __extends(List, _super);
      function List() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      List.prototype._template = function (s) {
          return template(s, this, s.children);
      };
      return List;
  }(ListBase));

  /** @hidden */
  var ListHeaderBase = /*#__PURE__*/ (function (_super) {
      __extends(ListHeaderBase, _super);
      function ListHeaderBase() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      ListHeaderBase.prototype._render = function (s) {
          this._cssClass = this._className + ' mbsc-list-header' + this._theme + this._hb;
      };
      return ListHeaderBase;
  }(BaseComponent));

  function template$1(s, inst, content) {
      return (createElement("div", { ref: inst._setEl, className: inst._cssClass }, content));
  }
  /**
   * The ListItem component
   */
  var ListHeader = /*#__PURE__*/ (function (_super) {
      __extends(ListHeader, _super);
      function ListHeader() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      ListHeader.prototype._template = function (s) {
          return template$1(s, this, s.children);
      };
      return ListHeader;
  }(ListHeaderBase));

  /** @hidden */
  var ListItemBase = /*#__PURE__*/ (function (_super) {
      __extends(ListItemBase, _super);
      function ListItemBase() {
          var _this = _super !== null && _super.apply(this, arguments) || this;
          _this._onClick = function (ev) {
              _this._hook('onClick', { domEvent: ev });
              if (_this.s.selected) {
                  _this.setState({ hasFocus: false });
              }
          };
          return _this;
      }
      // tslint:enable variable-name
      ListItemBase.prototype._mounted = function () {
          var _this = this;
          var isDrag;
          var touchTimer;
          this._unlisten = gestureListener(this._el, {
              click: true,
              keepFocus: true,
              onBlur: function () {
                  _this.setState({ hasFocus: false });
              },
              onEnd: function (ev) {
                  if (isDrag) {
                      var args = __assign({}, ev);
                      // Will prevent mousedown event on doc
                      args.domEvent.preventDefault();
                      args.data = _this.s.data;
                      args.drag = true;
                      _this._hook('onDragEnd', args);
                      isDrag = false;
                  }
                  clearTimeout(touchTimer);
              },
              onFocus: function () {
                  _this.setState({ hasFocus: true });
              },
              onHoverIn: function (ev) {
                  if (_this.s.actionable) {
                      _this.setState({ hasHover: true });
                  }
                  _this._hook('onHoverIn', { domEvent: ev });
              },
              onHoverOut: function (ev) {
                  _this.setState({ hasHover: false });
                  _this._hook('onHoverOut', { domEvent: ev });
              },
              onKeyDown: function (ev) {
                  var event = _this.s.data;
                  switch (ev.keyCode) {
                      case ENTER:
                      case SPACE:
                          _this._el.click();
                          ev.preventDefault();
                          break;
                      case BACKSPACE:
                      case DELETE:
                          if (event && event.editable !== false) {
                              _this._hook('onDelete', {
                                  domEvent: ev,
                                  event: event,
                                  source: 'agenda',
                              });
                          }
                          break;
                  }
              },
              onMove: function (ev) {
                  var s = _this.s;
                  var args = __assign({}, ev);
                  args.data = s.data;
                  args.drag = true;
                  args.external = true;
                  if (isDrag || !args.isTouch) {
                      // Prevents page scroll on touch and text selection with mouse
                      args.domEvent.preventDefault();
                  }
                  if (isDrag) {
                      _this._hook('onDragMove', args);
                  }
                  else if (Math.abs(args.deltaX) > 7 || Math.abs(args.deltaY) > 7) {
                      clearTimeout(touchTimer);
                      if (!args.isTouch && s.drag && s.data.editable !== false) {
                          isDrag = true;
                          _this._hook('onDragStart', args);
                      }
                  }
              },
              onPress: function () {
                  if (_this.s.actionable) {
                      _this.setState({ isActive: true });
                  }
              },
              onRelease: function () {
                  _this.setState({ isActive: false });
              },
              onStart: function (ev) {
                  var s = _this.s;
                  if (ev.isTouch && s.drag && s.data.editable !== false && !isDrag) {
                      touchTimer = setTimeout(function () {
                          var args = __assign({}, ev);
                          args.data = s.data;
                          args.drag = true;
                          _this._hook('onDragModeOn', args);
                          _this._hook('onDragStart', args);
                          isDrag = true;
                      }, 350);
                  }
                  return { ripple: s.actionable && s.ripple };
              },
          });
      };
      ListItemBase.prototype._render = function (s, state) {
          this._cssClass =
              this._className +
                  ' mbsc-list-item' +
                  this._theme +
                  this._hb +
                  this._rtl +
                  (s.actionable ? ' mbsc-list-item-actionable' : '') +
                  (state.hasFocus ? ' mbsc-focus' : '') +
                  (state.hasHover ? ' mbsc-hover' : '') +
                  (state.isActive ? ' mbsc-active' : '') +
                  (s.selected ? ' mbsc-selected' : '');
      };
      ListItemBase.prototype._destroy = function () {
          if (this._unlisten) {
              this._unlisten();
          }
      };
      ListItemBase.defaults = {
          actionable: true,
          ripple: false,
      };
      // tslint:disable variable-name
      ListItemBase._name = 'ListItem';
      return ListItemBase;
  }(BaseComponent));

  function template$2(s, inst, content) {
      var _a = inst.props; _a.actionable; _a.children; _a.className; _a.data; _a.drag; _a.ripple; _a.rtl; var theme = _a.theme; _a.themeVariant; _a.onHoverIn; _a.onHoverOut; _a.onDragEnd; _a.onDragMove; _a.onDragStart; _a.onDragModeOn; _a.onDragModeOff; _a.onDelete; _a.onClick; var other = __rest(_a, ["actionable", "children", "className", "data", "drag", "ripple", "rtl", "theme", "themeVariant", "onHoverIn", "onHoverOut", "onDragEnd", "onDragMove", "onDragStart", "onDragModeOn", "onDragModeOff", "onDelete", "onClick"]);
      return (createElement("div", __assign({ tabIndex: 0, ref: inst._setEl, onClick: inst._onClick, className: inst._cssClass }, other),
          content,
          createElement("div", { className: 'mbsc-list-item-background mbsc-' + theme })));
  }
  /**
   * The ListItem component
   */
  var ListItem = /*#__PURE__*/ (function (_super) {
      __extends(ListItem, _super);
      function ListItem() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      ListItem.prototype._template = function (s) {
          return template$2(s, this, s.children);
      };
      return ListItem;
  }(ListItemBase));

  /** @hidden */
  var IconBase = /*#__PURE__*/ (function (_super) {
      __extends(IconBase, _super);
      function IconBase() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      // tslint:enable variable-name
      IconBase.prototype._render = function (s) {
          // The icon might be custom markup as well
          this._hasChildren = !isString(s.name);
          this._cssClass =
              this._className +
                  ' mbsc-icon' +
                  this._theme +
                  (s.name && !this._hasChildren
                      ? // If the icon name contains a space, we consider it as a 3rd party font icon,
                          // (e.g. FA: 'fas fa-camera', or Ionicon: 'icon ion-md-heart').
                          // Otherwas we add the 'mbsc-icon-' prefix to use our font.
                          s.name.indexOf(' ') !== -1
                              ? ' ' + s.name
                              : ' mbsc-font-icon mbsc-icon-' + s.name
                      : '');
          this._svg = s.svg ? this._safeHtml(s.svg) : UNDEFINED;
      };
      return IconBase;
  }(BaseComponent));

  function template$3(s, inst) {
      return (createElement("span", { onClick: s.onClick, className: inst._cssClass, dangerouslySetInnerHTML: inst._svg, "v-html": UNDEFINED }, inst._hasChildren && s.name));
  }
  /**
   * The Icon component.
   *
   * Usage:
   *
   * ```
   * <Icon name="home" />
   * ```
   */
  var Icon = /*#__PURE__*/ (function (_super) {
      __extends(Icon, _super);
      function Icon() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      Icon.prototype._template = function (s) {
          return template$3(s, this);
      };
      return Icon;
  }(IconBase));

  /** @hidden */
  var ButtonBase = /*#__PURE__*/ (function (_super) {
      __extends(ButtonBase, _super);
      function ButtonBase() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      // tslint:enable variable-name
      ButtonBase.prototype._mounted = function () {
          var _this = this;
          this._unlisten = gestureListener(this._el, {
              click: true,
              onBlur: function () {
                  _this.setState({ hasFocus: false });
              },
              onFocus: function () {
                  _this.setState({ hasFocus: true });
              },
              onHoverIn: function () {
                  if (!_this.s.disabled) {
                      _this.setState({ hasHover: true });
                  }
              },
              onHoverOut: function () {
                  _this.setState({ hasHover: false });
              },
              onKeyDown: function (ev) {
                  switch (ev.keyCode) {
                      case ENTER:
                      case SPACE:
                          _this._el.click();
                          ev.preventDefault();
                          break;
                  }
              },
              onPress: function () {
                  _this.setState({ isActive: true });
              },
              onRelease: function () {
                  _this.setState({ isActive: false });
              },
              onStart: function () {
                  return { ripple: _this.s.ripple && !_this.s.disabled };
              },
          });
      };
      ButtonBase.prototype._render = function (s, state) {
          var _this = this;
          var disabled = s.disabled;
          this._isIconOnly = !!(s.icon || s.iconSvg);
          this._hasStartIcon = !!(s.startIcon || s.startIconSvg);
          this._hasEndIcon = !!(s.endIcon || s.endIconSvg);
          this._tabIndex = disabled ? UNDEFINED : s.tabIndex || 0;
          this._cssClass =
              this._className +
                  ' mbsc-reset mbsc-font mbsc-button' +
                  this._theme +
                  this._rtl +
                  ' mbsc-button-' +
                  s.variant +
                  (this._isIconOnly ? ' mbsc-icon-button' : '') +
                  (disabled ? ' mbsc-disabled' : '') +
                  (s.color ? ' mbsc-button-' + s.color : '') +
                  (state.hasFocus && !disabled ? ' mbsc-focus' : '') +
                  (state.isActive && !disabled ? ' mbsc-active' : '') +
                  (state.hasHover && !disabled ? ' mbsc-hover' : '');
          this._iconClass = 'mbsc-button-icon' + this._rtl;
          this._startIconClass = this._iconClass + ' mbsc-button-icon-start';
          this._endIconClass = this._iconClass + ' mbsc-button-icon-end';
          // Workaround for mouseleave not firing on disabled button
          if (s.disabled && state.hasHover) {
              setTimeout(function () {
                  _this.setState({ hasHover: false });
              });
          }
      };
      ButtonBase.prototype._destroy = function () {
          if (this._unlisten) {
              this._unlisten();
          }
      };
      /** @hidden */
      // tslint:disable variable-name
      ButtonBase.defaults = {
          ripple: false,
          role: 'button',
          tag: 'button',
          variant: 'standard',
      };
      ButtonBase._name = 'Button';
      return ButtonBase;
  }(BaseComponent));

  function template$4(s, inst, content) {
      var _a = inst.props, ariaLabel = _a.ariaLabel; _a.children; _a.className; _a.color; var endIcon = _a.endIcon; _a.endIconSrc; var endIconSvg = _a.endIconSvg; _a.hasChildren; var icon = _a.icon; _a.iconSrc; var iconSvg = _a.iconSvg; _a.ripple; _a.rtl; var role = _a.role, startIcon = _a.startIcon; _a.startIconSrc; var startIconSvg = _a.startIconSvg; _a.tag; _a.tabIndex; _a.theme; _a.themeVariant; _a.variant; var other = __rest(_a, ["ariaLabel", "children", "className", "color", "endIcon", "endIconSrc", "endIconSvg", "hasChildren", "icon", "iconSrc", "iconSvg", "ripple", "rtl", "role", "startIcon", "startIconSrc", "startIconSvg", "tag", "tabIndex", "theme", "themeVariant", "variant"]);
      // Need to use props here, otherwise all inherited settings will be included in ...other,
      // which will end up on the native element, resulting in invalid DOM
      var props = __assign({ 'aria-label': ariaLabel, className: inst._cssClass, ref: inst._setEl }, other);
      var inner = (createElement(Fragment, null,
          inst._isIconOnly && createElement(Icon, { className: inst._iconClass, name: icon, svg: iconSvg, theme: s.theme }),
          inst._hasStartIcon && createElement(Icon, { className: inst._startIconClass, name: startIcon, svg: startIconSvg, theme: s.theme }),
          content,
          inst._hasEndIcon && createElement(Icon, { className: inst._endIconClass, name: endIcon, svg: endIconSvg, theme: s.theme })));
      if (s.tag === 'span') {
          return (createElement("span", __assign({ role: role, "aria-disabled": s.disabled, tabIndex: inst._tabIndex }, props), inner));
      }
      if (s.tag === 'a') {
          return (createElement("a", __assign({ "aria-disabled": s.disabled, tabIndex: inst._tabIndex }, props), inner));
      }
      return (createElement("button", __assign({ role: role, tabIndex: inst._tabIndex }, props), inner));
  }
  /**
   * The Button component.
   *
   * Usage:
   *
   * ```
   * <Button icon="home">A button</Button>
   * ```
   */
  var Button = /*#__PURE__*/ (function (_super) {
      __extends(Button, _super);
      function Button() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      Button.prototype._template = function (s) {
          return template$4(s, this, s.children);
      };
      return Button;
  }(ButtonBase));

  /**
   * @param {import('../../src/index').RenderableProps<{ context: any }>} props
   */

  function ContextProvider(props) {
    this.getChildContext = function () {
      return props.context;
    };

    return props.children;
  }
  /**
   * Portal component
   * @this {import('./internal').Component}
   * @param {object | null | undefined} props
   *
   * TODO: use createRoot() instead of fake root
   */


  function Portal(props) {
    var _this = this;

    var container = props._container;

    _this.componentWillUnmount = function () {
      render(null, _this._temp);
      _this._temp = null;
      _this._container = null;
    }; // When we change container we should clear our old container and
    // indicate a new mount.


    if (_this._container && _this._container !== container) {
      _this.componentWillUnmount();
    } // When props.vnode is undefined/false/null we are dealing with some kind of
    // conditional vnode. This should not trigger a render.


    if (props._vnode) {
      if (!_this._temp) {
        _this._container = container; // Create a fake DOM parent node that manages a subset of `container`'s children:

        _this._temp = {
          nodeType: 1,
          parentNode: container,
          childNodes: [],
          appendChild: function appendChild(child) {
            this.childNodes.push(child);

            _this._container.appendChild(child);
          },
          insertBefore: function insertBefore(child, before) {
            this.childNodes.push(child);

            _this._container.appendChild(child);
          },
          removeChild: function removeChild(child) {
            this.childNodes.splice(this.childNodes.indexOf(child) >>> 1, 1);

            _this._container.removeChild(child);
          }
        };
      } // Render our wrapping element into temp.


      render(createElement(ContextProvider, {
        context: _this.context
      }, props._vnode), _this._temp);
    } // When we come from a conditional render, on a mounted
    // portal we should clear the DOM.
    else if (_this._temp) {
      _this.componentWillUnmount();
    }
  }
  /**
   * Create a `Portal` to continue rendering the vnode tree at a different DOM node
   * @param {import('./internal').VNode} vnode The vnode to render
   * @param {import('./internal').PreactElement} container The DOM node to continue rendering in to.
   */


  function createPortal(vnode, container) {
    return createElement(Portal, {
      _vnode: vnode,
      _container: container
    });
  }

  /** @hidden */
  var Portal$1 = /*#__PURE__*/ (function (_super) {
      __extends(Portal, _super);
      function Portal() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      Portal.prototype.render = function () {
          var context = this.props.context;
          return context ? createPortal(this.props.children, context) : null;
      };
      return Portal;
  }(Component));

  var markup = '<div class="mbsc-resize"><div class="mbsc-resize-i mbsc-resize-x"></div></div>' +
      '<div class="mbsc-resize"><div class="mbsc-resize-i mbsc-resize-y"></div></div>';
  var observer;
  var count = 0;
  function resizeObserver(el, callback, zone) {
      var expand;
      var expandChild;
      var helper;
      var hiddenRafId;
      var rafId;
      var shrink;
      var stopCheck;
      var lastCheck = 0;
      function reset() {
          expandChild.style.width = '100000px';
          expandChild.style.height = '100000px';
          expand.scrollLeft = 100000;
          expand.scrollTop = 100000;
          shrink.scrollLeft = 100000;
          shrink.scrollTop = 100000;
      }
      function checkHidden() {
          var now = +new Date();
          hiddenRafId = 0;
          if (!stopCheck) {
              if (now - lastCheck > 200 && !expand.scrollTop && !expand.scrollLeft) {
                  lastCheck = now;
                  reset();
              }
              if (!hiddenRafId) {
                  hiddenRafId = raf(checkHidden);
              }
          }
      }
      function onScroll() {
          if (!rafId) {
              rafId = raf(onResize);
          }
      }
      function onResize() {
          rafId = 0;
          reset();
          callback();
      }
      if (win && win.ResizeObserver) {
          if (!observer) {
              observer = new win.ResizeObserver(function (entries) {
                  if (!rafId) {
                      rafId = raf(function () {
                          for (var _i = 0, entries_1 = entries; _i < entries_1.length; _i++) {
                              var entry = entries_1[_i];
                              // Sometimes this fires after unobserve has been called,
                              // so we need to check if the callback function is still there
                              if (entry.target.__mbscResize) {
                                  entry.target.__mbscResize();
                              }
                          }
                          rafId = 0;
                      });
                  }
              });
          }
          count++;
          el.__mbscResize = function () {
              if (zone) {
                  zone.run(callback);
              }
              else {
                  callback();
              }
          };
          observer.observe(el);
      }
      else {
          helper = doc && doc.createElement('div');
      }
      if (helper) {
          helper.innerHTML = markup;
          helper.dir = 'ltr'; // Need this to work in rtl as well;
          shrink = helper.childNodes[1];
          expand = helper.childNodes[0];
          expandChild = expand.childNodes[0];
          el.appendChild(helper);
          listen(expand, 'scroll', onScroll);
          listen(shrink, 'scroll', onScroll);
          if (zone) {
              zone.runOutsideAngular(function () {
                  raf(checkHidden);
              });
          }
          else {
              raf(checkHidden);
          }
      }
      return {
          detach: function () {
              if (observer) {
                  count--;
                  delete el.__mbscResize;
                  observer.unobserve(el);
                  if (!count) {
                      observer = UNDEFINED;
                  }
              }
              else {
                  if (helper) {
                      unlisten(expand, 'scroll', onScroll);
                      unlisten(shrink, 'scroll', onScroll);
                      el.removeChild(helper);
                      rafc(rafId);
                      helper = UNDEFINED;
                  }
                  stopCheck = true;
              }
          },
      };
  }

  // tslint:disable no-non-null-assertion
  // tslint:disable directive-class-suffix
  // tslint:disable directive-selector
  var activeModal;
  var EDITABLE = 'input,select,textarea,button';
  var ALLOW_ENTER = 'textarea,button,input[type="button"],input[type="submit"]';
  var FOCUSABLE = EDITABLE + ',[tabindex="0"]';
  var MAX_WIDTH = 600;
  var KEY_CODES = {
      enter: ENTER,
      esc: ESC,
      space: SPACE,
  };
  var needsFixed = isBrowser && /(iphone|ipod)/i.test(userAgent) && majorVersion >= 7 && majorVersion < 15;
  /** @hidden */
  function processButtons(inst, buttons) {
      var s = inst.s; // needed for localization settings
      var processedButtons = [];
      var predefinedButtons = {
          cancel: {
              cssClass: 'mbsc-popup-button-close',
              name: 'cancel',
              text: s.cancelText,
          },
          close: {
              cssClass: 'mbsc-popup-button-close',
              name: 'close',
              text: s.closeText,
          },
          ok: {
              cssClass: 'mbsc-popup-button-primary',
              keyCode: ENTER,
              name: 'ok',
              text: s.okText,
          },
          set: {
              cssClass: 'mbsc-popup-button-primary',
              keyCode: ENTER,
              name: 'set',
              text: s.setText,
          },
      };
      if (buttons && buttons.length) {
          buttons.forEach(function (btn) {
              var button = isString(btn) ? predefinedButtons[btn] || { text: btn } : btn;
              if (!button.handler || isString(button.handler)) {
                  if (isString(button.handler)) {
                      button.name = button.handler;
                  }
                  button.handler = function (domEvent) {
                      inst._onButtonClick({ domEvent: domEvent, button: button });
                  };
              }
              processedButtons.push(button);
          });
          return processedButtons;
      }
      return UNDEFINED;
  }
  function getPrevActive(modal, i) {
      if (i === void 0) { i = 0; }
      var prevModal = modal._prevModal;
      if (prevModal && prevModal !== modal && i < 10) {
          if (prevModal.isVisible()) {
              return prevModal;
          }
          return getPrevActive(prevModal, i + 1);
      }
      return UNDEFINED;
  }
  /**
   * @hidden
   */
  var PopupBase = /*#__PURE__*/ (function (_super) {
      __extends(PopupBase, _super);
      function PopupBase() {
          var _this = _super !== null && _super.apply(this, arguments) || this;
          _this._lastFocus = +new Date();
          /** @hidden */
          _this._setActive = function (el) {
              _this._active = el;
          };
          /** @hidden */
          _this._setContent = function (el) {
              _this._content = el;
          };
          /** @hidden */
          _this._setLimitator = function (el) {
              _this._limitator = el;
          };
          /** @hidden */
          _this._setPopup = function (el) {
              _this._popup = el;
          };
          /** @hidden */
          _this._setWrapper = function (el) {
              _this._wrapper = el;
          };
          /** @hidden */
          _this._onOverlayClick = function () {
              if (_this._isOpen && _this.s.closeOnOverlayClick && !_this._preventClose) {
                  _this._close('overlay');
              }
              _this._preventClose = false;
          };
          /** @hidden */
          _this._onDocClick = function (ev) {
              if (!_this.s.showOverlay && ev.target !== _this.s.focusElm && activeModal === _this) {
                  _this._onOverlayClick();
              }
          };
          /** @hidden */
          _this._onMouseDown = function (ev) {
              if (!_this.s.showOverlay) {
                  _this._target = ev.target;
              }
          };
          /** @hidden */
          _this._onMouseUp = function (ev) {
              if (_this._target && _this._popup && _this._popup.contains(_this._target) && !_this._popup.contains(ev.target)) {
                  _this._preventClose = true;
              }
              _this._target = false;
          };
          /** @hidden */
          _this._onPopupClick = function () {
              if (!_this.s.showOverlay) {
                  _this._preventClose = true;
              }
          };
          /** @hidden */
          _this._onAnimationEnd = function (ev) {
              if (ev.target === _this._popup) {
                  if (_this._isClosing) {
                      _this._onClosed();
                      _this._isClosing = false;
                      if (_this.state.isReady) {
                          _this.setState({ isReady: false });
                      }
                      else {
                          _this.forceUpdate();
                      }
                  }
                  if (_this._isOpening) {
                      _this._onOpened();
                      _this._isOpening = false;
                      _this.forceUpdate();
                  }
              }
          };
          /** @hidden */
          _this._onButtonClick = function (_a) {
              var domEvent = _a.domEvent, button = _a.button;
              _this._hook('onButtonClick', { domEvent: domEvent, button: button });
              if (/cancel|close|ok|set/.test(button.name)) {
                  _this._close(button.name);
              }
          };
          /** @hidden */
          _this._onFocus = function (ev) {
              var now = +new Date();
              // If an element outside of the modal is focused, put the focus back inside the modal
              // Last focus time is tracked, to avoid infinite loop for focus,
              // if there's another modal present on page, e.g. Ionic or Bootstrap
              // https://github.com/acidb/mobiscroll/issues/341
              if (activeModal === _this &&
                  ev.target.nodeType &&
                  _this._ctx.contains(ev.target) &&
                  _this._popup &&
                  !_this._popup.contains(ev.target) &&
                  now - _this._lastFocus > 100 &&
                  ev.target !== _this.s.focusElm) {
                  _this._lastFocus = now;
                  _this._active.focus();
              }
          };
          /** @hidden */
          _this._onKeyDown = function (ev) {
              var s = _this.s;
              var keyCode = ev.keyCode;
              var focusElm = s.focusElm && !s.focusOnOpen ? s.focusElm : UNDEFINED;
              // Prevent scroll on Space key
              if ((keyCode === SPACE && !matches(ev.target, EDITABLE)) || (_this._lock && (keyCode === UP_ARROW || keyCode === DOWN_ARROW))) {
                  ev.preventDefault();
              }
              // Trap the focus inside the modal
              if (s.focusTrap && keyCode === TAB) {
                  var all = _this._popup.querySelectorAll(FOCUSABLE);
                  var focusable_1 = [];
                  var end_1 = -1;
                  var target = 0;
                  var current_1 = -1;
                  var targetElm = UNDEFINED;
                  // Filter truly focusable elements
                  forEach(all, function (elm) {
                      if (!elm.disabled && (elm.offsetHeight || elm.offsetWidth)) {
                          focusable_1.push(elm);
                          end_1++;
                          // Save the index of the currently focused element
                          if (elm === _this._doc.activeElement) {
                              current_1 = end_1;
                          }
                      }
                  });
                  // If shift is also pressed, means we're going backwards,
                  // so we target the last focusable element if the current active is the first
                  if (ev.shiftKey) {
                      target = end_1;
                      end_1 = 0;
                  }
                  // If current active is first or last, move focus to last or first focusable element
                  if (current_1 === end_1) {
                      targetElm = focusElm || focusable_1[target];
                  }
                  else if (ev.target === focusElm) {
                      targetElm = focusable_1[target];
                  }
                  if (targetElm) {
                      targetElm.focus();
                      ev.preventDefault();
                  }
              }
          };
          /** @hidden */
          _this._onContentScroll = function (ev) {
              if (_this._lock && (ev.type !== TOUCH_MOVE || ev.touches[0].touchType !== 'stylus')) {
                  ev.preventDefault();
              }
          };
          /** @hidden */
          _this._onScroll = function (ev) {
              var s = _this.s;
              if (s.closeOnScroll) {
                  _this._close('scroll');
              }
              else if (_this._hasContext || s.display === 'anchored') {
                  _this.position();
              }
          };
          /** @hidden */
          _this._onWndKeyDown = function (ev) {
              var s = _this.s;
              var keyCode = ev.keyCode;
              // keyCode is not defined if Chrome triggers keydown when a field is autofilled
              if (activeModal === _this && keyCode !== UNDEFINED) {
                  _this._hook('onKeyDown', { keyCode: keyCode });
                  if (s.closeOnEsc && keyCode === ESC) {
                      _this._close('esc');
                  }
                  if (keyCode === ENTER && matches(ev.target, ALLOW_ENTER) && !ev.shiftKey) {
                      return;
                  }
                  if (_this._buttons) {
                      for (var _i = 0, _a = _this._buttons; _i < _a.length; _i++) {
                          var button = _a[_i];
                          var buttonKeyCodes = isArray(button.keyCode) ? button.keyCode : [button.keyCode];
                          for (var _b = 0, buttonKeyCodes_1 = buttonKeyCodes; _b < buttonKeyCodes_1.length; _b++) {
                              var key = buttonKeyCodes_1[_b];
                              if (!button.disabled && key !== UNDEFINED && (key === keyCode || KEY_CODES[key] === keyCode)) {
                                  button.handler(ev);
                                  return;
                              }
                          }
                      }
                  }
              }
          };
          /** @hidden */
          _this._onResize = function () {
              var wrapper = _this._wrapper;
              var hasContext = _this._hasContext;
              if (!wrapper) {
                  return;
              }
              _this._vpWidth = Math.min(wrapper.clientWidth, hasContext ? Infinity : _this._win.innerWidth);
              _this._vpHeight = Math.min(wrapper.clientHeight, hasContext ? Infinity : _this._win.innerHeight);
              _this._maxWidth = _this._limitator.offsetWidth;
              _this._maxHeight = _this.s.maxHeight !== UNDEFINED || _this._vpWidth < 768 || _this._vpHeight < 650 ? _this._limitator.offsetHeight : 600;
              _this._round = _this.s.touchUi === false || (_this._popup.offsetWidth < _this._vpWidth && _this._vpWidth > _this._maxWidth);
              var args = {
                  isLarge: _this._round,
                  maxPopupHeight: _this._maxHeight,
                  maxPopupWidth: _this._maxWidth,
                  target: wrapper,
                  windowHeight: _this._vpHeight,
                  windowWidth: _this._vpWidth,
              };
              if (_this._hook('onResize', args) !== false && !args.cancel) {
                  _this.position();
              }
          };
          return _this;
      }
      // tslint:enable variable-name
      // ---
      /**
       * @hidden
       * Opens the component.
       */
      PopupBase.prototype.open = function () {
          if (!this._isOpen) {
              this.setState({
                  isOpen: true,
              });
          }
      };
      /**
       * @hidden
       * Closes the component.
       */
      PopupBase.prototype.close = function () {
          this._close();
      };
      /**
       * @hidden
       * Returns if the component is opened or not.
       */
      PopupBase.prototype.isVisible = function () {
          return !!this._isOpen;
      };
      /**
       * Recalculates the position of the component.
       */
      PopupBase.prototype.position = function () {
          if (!this._isOpen) {
              return;
          }
          var s = this.s;
          var state = this.state;
          var wrapper = this._wrapper;
          var popup = this._popup;
          var hasContext = this._hasContext;
          var anchor = s.anchor;
          var anchorAlign = s.anchorAlign;
          var rtl = s.rtl;
          var scrollTop = getScrollTop(this._scrollCont);
          var scrollLeft = getScrollLeft(this._scrollCont);
          var viewportWidth = this._vpWidth;
          var viewportHeight = this._vpHeight;
          var maxWidth = this._maxWidth;
          var maxHeight = this._maxHeight;
          var popupWidth = Math.min(popup.offsetWidth, maxWidth);
          var popupHeight = Math.min(popup.offsetHeight, maxHeight);
          var showArrow = s.showArrow;
          this._lock = s.scrollLock && this._content.scrollHeight <= this._content.clientHeight;
          // this._short = popupHeight >= (viewportHeight - 50);
          if (hasContext) {
              wrapper.style.top = scrollTop + 'px';
              wrapper.style.left = scrollLeft + 'px';
          }
          var skip = this._hook('onPosition', {
              isLarge: this._round,
              maxPopupHeight: maxHeight,
              maxPopupWidth: maxWidth,
              target: this._wrapper,
              windowHeight: viewportHeight,
              windowWidth: viewportWidth,
          }) === false;
          if (s.display === 'anchored' && !skip) {
              var ctxLeft = 0;
              var ctxTop = 0;
              var left = constrain(state.modalLeft || 0, 8, viewportWidth - popupWidth - 8);
              var top_1 = state.modalTop || 8;
              var bubblePos = 'bottom';
              var arrowPos = {};
              var arrowHeight = showArrow ? 16 : 4;
              var fullWidth = wrapper.offsetWidth;
              var fullHeight = wrapper.offsetHeight;
              var widthOffset = (fullWidth - viewportWidth) / 2;
              var heightOffset = (fullHeight - viewportHeight) / 2;
              if (hasContext) {
                  var ctxBox = this._ctx.getBoundingClientRect();
                  ctxTop = ctxBox.top;
                  ctxLeft = ctxBox.left;
              }
              // Check if anchor exists and it's inside the context
              if (anchor && this._ctx.contains(anchor)) {
                  var box = anchor.getBoundingClientRect();
                  var anchorTop = box.top - ctxTop;
                  var anchorLeft = box.left - ctxLeft;
                  var anchorWidth = anchor.offsetWidth;
                  var anchorHeight = anchor.offsetHeight;
                  if ((anchorAlign === 'start' && !rtl) || (anchorAlign === 'end' && rtl)) {
                      // Position to the left of the anchor
                      left = anchorLeft;
                  }
                  else if ((anchorAlign === 'end' && !rtl) || (anchorAlign === 'start' && rtl)) {
                      // Position to the right of the anchor
                      left = anchorLeft + anchorWidth - popupWidth;
                  }
                  else {
                      // Position to the center of the anchor
                      left = anchorLeft - (popupWidth - anchorWidth) / 2;
                  }
                  // Make sure to remain in the viewport
                  left = constrain(left, 8, viewportWidth - popupWidth - 8);
                  // By default position the popup to the bottom of the anchor
                  top_1 = anchorTop + anchorHeight + arrowHeight;
                  arrowPos = {
                      left: constrain(anchorLeft + anchorWidth / 2 - left - widthOffset, 30, popupWidth - 30) + 'px',
                  };
                  // if there's no space below
                  if (top_1 + popupHeight + arrowHeight > viewportHeight) {
                      if (anchorTop - popupHeight - arrowHeight > 0) {
                          // check if above the anchor is enough space
                          bubblePos = 'top';
                          top_1 = anchorTop - popupHeight - arrowHeight;
                      }
                      else if (!s.disableLeftRight) {
                          var leftPos = anchorLeft - popupWidth - 8 > 0; // check if there's space on the left
                          var rightPos = anchorLeft + anchorWidth + popupWidth + 8 <= viewportWidth; // check if there's space on the right
                          // calculations are almost the same for the left and right position, so we group them together
                          if (leftPos || rightPos) {
                              top_1 = constrain(anchorTop - (popupHeight - anchorHeight) / 2, 8, viewportHeight - popupHeight - 8);
                              // Make sure it stays in the viewport
                              if (top_1 + popupHeight + 8 > viewportHeight) {
                                  // the top position can be negative because of the -16px spacing
                                  top_1 = Math.max(viewportHeight - popupHeight - 8, 0);
                              }
                              arrowPos = {
                                  top: constrain(anchorTop + anchorHeight / 2 - top_1 - heightOffset, 30, popupHeight - 30) + 'px',
                              };
                              bubblePos = leftPos ? 'left' : 'right';
                              left = leftPos ? anchorLeft - popupWidth : anchorLeft + anchorWidth;
                          }
                      }
                  }
              }
              if (bubblePos === 'top' || bubblePos === 'bottom') {
                  // Make sure it stays in the viewport
                  if (top_1 + popupHeight + arrowHeight > viewportHeight) {
                      // the top position can be negative because of the -16px spacing
                      top_1 = Math.max(viewportHeight - popupHeight - arrowHeight, 0);
                      showArrow = false;
                  }
              }
              this.setState({
                  arrowPos: arrowPos,
                  bubblePos: bubblePos,
                  height: viewportHeight,
                  isReady: true,
                  modalLeft: left,
                  modalTop: top_1,
                  showArrow: showArrow,
                  width: viewportWidth,
              });
          }
          else {
              this.setState({
                  height: viewportHeight,
                  isReady: true,
                  showArrow: showArrow,
                  width: viewportWidth,
              });
          }
      };
      PopupBase.prototype._render = function (s, state) {
          // 'bubble' is deprecated, renamed to 'anchored'
          if (s.display === 'bubble') {
              s.display = 'anchored';
          }
          var animation = s.animation;
          var display = s.display;
          var prevProps = this._prevS;
          var hasPos = display === 'anchored';
          var isModal = display !== 'inline';
          var isFullScreen = s.fullScreen && isModal;
          var isOpen = isModal ? (s.isOpen === UNDEFINED ? state.isOpen : s.isOpen) : false;
          if (isOpen &&
              (s.windowWidth !== prevProps.windowWidth ||
                  s.display !== prevProps.display ||
                  s.showArrow !== prevProps.showArrow ||
                  (s.anchor !== prevProps.anchor && s.display === 'anchored'))) {
              this._shouldPosition = true;
          }
          this._limits = {
              maxHeight: addPixel(s.maxHeight),
              maxWidth: addPixel(s.maxWidth),
          };
          this._style = {
              height: isFullScreen ? '100%' : addPixel(s.height),
              left: hasPos && state.modalLeft ? state.modalLeft + 'px' : '',
              maxHeight: addPixel(this._maxHeight || s.maxHeight),
              maxWidth: addPixel(this._maxWidth || s.maxWidth),
              top: hasPos && state.modalTop ? state.modalTop + 'px' : '',
              width: isFullScreen ? '100%' : addPixel(s.width),
          };
          this._hasContext = s.context !== 'body' && s.context !== UNDEFINED;
          this._needsLock = needsFixed && !this._hasContext && display !== 'anchored' && s.scrollLock;
          this._isModal = isModal;
          this._flexButtons = display === 'center' || (!this._touchUi && !isFullScreen && (display === 'top' || display === 'bottom'));
          if (animation !== UNDEFINED && animation !== true) {
              this._animation = isString(animation) ? animation : '';
          }
          else {
              switch (display) {
                  case 'bottom':
                      this._animation = 'slide-up';
                      break;
                  case 'top':
                      this._animation = 'slide-down';
                      break;
                  default:
                      this._animation = 'pop';
              }
          }
          if (s.buttons) {
              if (s.buttons !== prevProps.buttons) {
                  this._buttons = processButtons(this, s.buttons);
              }
          }
          else {
              this._buttons = UNDEFINED;
          }
          if (s.headerText !== prevProps.headerText) {
              this._headerText = s.headerText ? this._safeHtml(s.headerText) : UNDEFINED;
          }
          // Will open
          if (isOpen && !this._isOpen) {
              this._onOpen();
          }
          // Will close
          if (!isOpen && this._isOpen) {
              this._onClose();
          }
          this._isOpen = isOpen;
          this._isVisible = isOpen || this._isClosing;
      };
      PopupBase.prototype._updated = function () {
          var _this = this;
          var s = this.s;
          var wrapper = this._wrapper;
          if (doc && (s.context !== this._prevS.context || !this._ctx)) {
              // TODO: refactor for React Native
              var ctx = isString(s.context) ? doc.querySelector(s.context) : s.context;
              if (!ctx) {
                  ctx = doc.body;
              }
              ctx.__mbscLock = ctx.__mbscLock || 0;
              ctx.__mbscIOSLock = ctx.__mbscIOSLock || 0;
              ctx.__mbscModals = ctx.__mbscModals || 0;
              this._ctx = ctx;
              // If we just got the context and at the same time the popup was opened,
              // we need an update for the Portal to render the content of the popup
              if (this._justOpened) {
                  ngSetTimeout(this, function () {
                      _this.forceUpdate();
                  });
                  return;
              }
          }
          if (!wrapper) {
              return;
          }
          if (this._justOpened) {
              var ctx = this._ctx;
              var hasContext = this._hasContext;
              var doc_1 = (this._doc = getDocument(wrapper));
              var win = (this._win = getWindow(wrapper));
              var activeElm_1 = doc_1.activeElement;
              // If we have responsive setting, we need to make sure to pass the width to the state,
              // and re-render so we have the correct calculated settings, which is based on the width.
              if (!this._hasWidth && s.responsive) {
                  var viewportWidth_1 = Math.min(wrapper.clientWidth, hasContext ? Infinity : win.innerWidth);
                  var viewportHeight_1 = Math.min(wrapper.clientHeight, hasContext ? Infinity : win.innerHeight);
                  this._hasWidth = true;
                  if (viewportWidth_1 !== this.state.width || viewportHeight_1 !== this.state.height) {
                      ngSetTimeout(this, function () {
                          _this.setState({
                              height: viewportHeight_1,
                              width: viewportWidth_1,
                          });
                      });
                      return;
                  }
              }
              this._scrollCont = hasContext ? ctx : win;
              this._observer = resizeObserver(wrapper, this._onResize, this._zone);
              this._prevFocus = s.focusElm || activeElm_1;
              ctx.__mbscModals++;
              // Scroll locking
              if (this._needsLock) {
                  if (!ctx.__mbscIOSLock) {
                      var scrollTop = getScrollTop(this._scrollCont);
                      var scrollLeft = getScrollLeft(this._scrollCont);
                      ctx.style.left = -scrollLeft + 'px';
                      ctx.style.top = -scrollTop + 'px';
                      ctx.__mbscScrollLeft = scrollLeft;
                      ctx.__mbscScrollTop = scrollTop;
                      ctx.classList.add('mbsc-popup-open-ios');
                      ctx.parentNode.classList.add('mbsc-popup-open-ios');
                  }
                  ctx.__mbscIOSLock++;
              }
              if (hasContext) {
                  ctx.classList.add('mbsc-popup-ctx');
              }
              if (s.focusTrap) {
                  listen(win, FOCUS_IN, this._onFocus);
              }
              if (s.focusElm && !s.focusOnOpen) {
                  listen(s.focusElm, KEY_DOWN, this._onKeyDown);
              }
              listen(this._scrollCont, TOUCH_MOVE, this._onContentScroll, { passive: false });
              listen(this._scrollCont, WHEEL, this._onContentScroll, { passive: false });
              listen(this._scrollCont, MOUSE_WHEEL, this._onContentScroll, { passive: false });
              setTimeout(function () {
                  if (s.focusOnOpen && activeElm_1) {
                      // TODO investigate on this, maybe it hides the virtual keyboard?
                      activeElm_1.blur();
                  }
                  if (!hasAnimation || !_this._animation) {
                      _this._onOpened();
                  }
                  // Need to be inside setTimeout to prevent immediate close
                  listen(doc_1, MOUSE_DOWN, _this._onMouseDown);
                  listen(doc_1, MOUSE_UP, _this._onMouseUp);
                  listen(doc_1, CLICK, _this._onDocClick);
              });
              this._hook('onOpen', { target: this._wrapper });
          }
          if (this._shouldPosition) {
              ngSetTimeout(this, function () {
                  // this.position();
                  _this._onResize();
              });
          }
          this._justOpened = false;
          this._justClosed = false;
          this._shouldPosition = false;
      };
      PopupBase.prototype._destroy = function () {
          if (this._isOpen) {
              this._onClosed();
              this._unlisten();
              if (activeModal === this) {
                  activeModal = getPrevActive(this);
              }
          }
      };
      PopupBase.prototype._onOpen = function () {
          var _this = this;
          if (hasAnimation && this._animation) {
              this._isOpening = true;
              this._isClosing = false;
          }
          this._justOpened = true;
          this._preventClose = false;
          if (this.s.setActive && activeModal !== this) {
              // Wait for the click to propagate,
              // because if another popup needs to be closed on doc click, we don't want to override
              // the activeModal variable.
              setTimeout(function () {
                  _this._prevModal = activeModal;
                  activeModal = _this;
              });
          }
      };
      PopupBase.prototype._onClose = function () {
          var _this = this;
          // const activeElm = this._doc!.activeElement as HTMLElement;
          // if (activeElm) {
          // There's a weird issue on Safari, where the page scrolls up when
          // 1) A readonly input inside the popup has the focus
          // 2) The popup is closed by clicking on a `button` element (built in popup buttons, or a button in the popup content)
          // To prevent this, blur the active element when closing the popup.
          // setTimeout is needed to prevent to avoid the "Cannot flush updates when React is already rendering" error in React
          // setTimeout(() => {
          // activeElm.blur();
          // });
          // }
          if (hasAnimation && this._animation) {
              this._isClosing = true;
              this._isOpening = false;
          }
          else {
              setTimeout(function () {
                  _this._onClosed();
                  _this.setState({ isReady: false });
              });
          }
          this._hasWidth = false;
          this._unlisten();
      };
      PopupBase.prototype._onOpened = function () {
          var s = this.s;
          if (s.focusOnOpen) {
              var activeElm = s.activeElm;
              var active = activeElm
                  ? isString(activeElm)
                      ? this._popup.querySelector(activeElm) || this._active
                      : activeElm
                  : this._active;
              if (active && active.focus) {
                  active.focus();
              }
          }
          listen(this._win, KEY_DOWN, this._onWndKeyDown);
          listen(this._scrollCont, SCROLL, this._onScroll);
      };
      PopupBase.prototype._onClosed = function () {
          var _this = this;
          var ctx = this._ctx;
          var prevFocus = this._prevFocus;
          // 'as any' is needed for Typescript 4 - we do want to check the existence of the focus method because of IE11
          var shouldFocus = this.s.focusOnClose && prevFocus && prevFocus.focus && prevFocus !== this._doc.activeElement;
          ctx.__mbscModals--;
          this._justClosed = true;
          if (this._needsLock) {
              ctx.__mbscIOSLock--;
              if (!ctx.__mbscIOSLock) {
                  ctx.classList.remove('mbsc-popup-open-ios');
                  ctx.parentNode.classList.remove('mbsc-popup-open-ios');
                  ctx.style.left = '';
                  ctx.style.top = '';
                  setScrollLeft(this._scrollCont, ctx.__mbscScrollLeft);
                  setScrollTop(this._scrollCont, ctx.__mbscScrollTop);
              }
          }
          if (this._hasContext && !ctx.__mbscModals) {
              ctx.classList.remove('mbsc-popup-ctx');
          }
          this._hook('onClosed', { focus: shouldFocus });
          if (shouldFocus) {
              prevFocus.focus();
          }
          setTimeout(function () {
              if (activeModal === _this) {
                  activeModal = getPrevActive(_this);
              }
          });
      };
      PopupBase.prototype._unlisten = function () {
          unlisten(this._win, KEY_DOWN, this._onWndKeyDown);
          unlisten(this._scrollCont, SCROLL, this._onScroll);
          unlisten(this._scrollCont, TOUCH_MOVE, this._onContentScroll, { passive: false });
          unlisten(this._scrollCont, WHEEL, this._onContentScroll, { passive: false });
          unlisten(this._scrollCont, MOUSE_WHEEL, this._onContentScroll, { passive: false });
          unlisten(this._doc, MOUSE_DOWN, this._onMouseDown);
          unlisten(this._doc, MOUSE_UP, this._onMouseUp);
          unlisten(this._doc, CLICK, this._onDocClick);
          if (this.s.focusTrap) {
              unlisten(this._win, FOCUS_IN, this._onFocus);
          }
          if (this.s.focusElm) {
              unlisten(this.s.focusElm, KEY_DOWN, this._onKeyDown);
          }
          if (this._observer) {
              this._observer.detach();
              this._observer = null;
          }
      };
      PopupBase.prototype._close = function (source) {
          if (this._isOpen) {
              if (this.s.isOpen === UNDEFINED) {
                  this.setState({
                      isOpen: false,
                  });
              }
              this._hook('onClose', { source: source });
          }
      };
      /** @hidden */
      PopupBase.defaults = {
          buttonVariant: 'flat',
          cancelText: 'Cancel',
          closeOnEsc: true,
          closeOnOverlayClick: true,
          closeText: 'Close',
          contentPadding: true,
          display: 'center',
          focusOnClose: true,
          focusOnOpen: true,
          focusTrap: true,
          maxWidth: MAX_WIDTH,
          okText: 'Ok',
          scrollLock: true,
          setActive: true,
          setText: 'Set',
          showArrow: true,
          showOverlay: true,
      };
      return PopupBase;
  }(BaseComponent));

  var Portal$2 = Portal$1;
  function template$5(s, state, inst, content) {
      var _a, _b;
      var hb = inst._hb;
      var rtl = inst._rtl;
      var theme = inst._theme;
      var display = s.display;
      var keydown = (_a = {}, _a[ON_KEY_DOWN] = inst._onKeyDown, _a);
      var animationEnd = (_b = {}, _b[ON_ANIMATION_END] = inst._onAnimationEnd, _b);
      return inst._isModal ? (inst._isVisible ? (createElement(Portal$2, { context: inst._ctx },
          createElement("div", __assign({ className: 'mbsc-font mbsc-flex mbsc-popup-wrapper mbsc-popup-wrapper-' +
                  display +
                  theme +
                  rtl +
                  ' ' +
                  inst._className +
                  (s.fullScreen ? ' mbsc-popup-wrapper-' + display + '-full' : '') +
                  (inst._touchUi ? '' : ' mbsc-popup-pointer') +
                  (inst._round ? ' mbsc-popup-round' : '') +
                  (inst._hasContext ? ' mbsc-popup-wrapper-ctx' : '') +
                  (state.isReady ? '' : ' mbsc-popup-hidden'), ref: inst._setWrapper }, keydown),
              s.showOverlay && (createElement("div", { className: 'mbsc-popup-overlay mbsc-popup-overlay-' +
                      display +
                      theme +
                      (inst._isClosing ? ' mbsc-popup-overlay-out' : '') +
                      (inst._isOpening && state.isReady ? ' mbsc-popup-overlay-in' : ''), onClick: inst._onOverlayClick })),
              createElement("div", { className: 'mbsc-popup-limits mbsc-popup-limits-' + display, ref: inst._setLimitator, style: inst._limits }),
              createElement("div", __assign({ className: 'mbsc-flex-col mbsc-popup mbsc-popup-' +
                      display +
                      theme +
                      hb +
                      (s.fullScreen ? '-full' : '') +
                      // (this._short ? ' mbsc-popup-short' : '') +
                      (state.bubblePos && state.showArrow && display === 'anchored' ? ' mbsc-popup-anchored-' + state.bubblePos : '') +
                      (inst._isClosing ? ' mbsc-popup-' + inst._animation + '-out' : '') +
                      (inst._isOpening && state.isReady ? ' mbsc-popup-' + inst._animation + '-in' : ''), role: "dialog", "aria-modal": "true", ref: inst._setPopup, style: inst._style, onClick: inst._onPopupClick }, animationEnd),
                  display === 'anchored' && state.showArrow && (createElement("div", { className: 'mbsc-popup-arrow-wrapper mbsc-popup-arrow-wrapper-' + state.bubblePos + theme },
                      createElement("div", { className: 'mbsc-popup-arrow mbsc-popup-arrow-' + state.bubblePos + theme, style: state.arrowPos }))),
                  createElement("div", { className: "mbsc-popup-focus", tabIndex: -1, ref: inst._setActive }),
                  createElement("div", { className: 'mbsc-flex-col mbsc-flex-1-1 mbsc-popup-body mbsc-popup-body-' +
                          display +
                          theme +
                          hb +
                          (s.fullScreen ? ' mbsc-popup-body-' + display + '-full' : '') +
                          // (this._short ? ' mbsc-popup-short' : '') +
                          (inst._round ? ' mbsc-popup-body-round' : '') },
                      inst._headerText && (createElement("div", { className: 'mbsc-flex-none mbsc-popup-header mbsc-popup-header-' +
                              display +
                              theme +
                              hb +
                              (inst._buttons ? '' : ' mbsc-popup-header-no-buttons'), dangerouslySetInnerHTML: inst._headerText, "v-html": UNDEFINED })),
                      createElement("div", { className: 'mbsc-flex-1-1 mbsc-popup-content' + (s.contentPadding ? ' mbsc-popup-padding' : ''), ref: inst._setContent }, content),
                      inst._buttons && (createElement("div", { className: 'mbsc-flex-none mbsc-popup-buttons mbsc-popup-buttons-' +
                              display +
                              theme +
                              rtl +
                              hb +
                              (inst._flexButtons ? ' mbsc-popup-buttons-flex mbsc-flex' : '') +
                              (s.fullScreen ? ' mbsc-popup-buttons-' + display + '-full' : '') }, inst._buttons.map(function (btn, i) {
                          return (createElement(Button, { color: btn.color, className: 'mbsc-popup-button mbsc-popup-button-' +
                                  display +
                                  rtl +
                                  hb +
                                  (inst._flexButtons ? ' mbsc-popup-button-flex' : '') +
                                  ' ' +
                                  (btn.cssClass || ''), icon: btn.icon, disabled: btn.disabled, key: i, theme: s.theme, themeVariant: s.themeVariant, variant: btn.variant || s.buttonVariant, onClick: btn.handler }, btn.text));
                      })))))))) : null) : (createElement(Fragment, null, content));
  }
  var Popup = /*#__PURE__*/ (function (_super) {
      __extends(Popup, _super);
      function Popup() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      Popup.prototype._template = function (s, state) {
          return template$5(s, state, this, s.children);
      };
      return Popup;
  }(PopupBase));

  // tslint:disable no-non-null-assertion
  var DEF_ID = 'mbsc-def';
  /** @hidden */
  function checkInvalidCollision$1(invalids, start, end, allDay, invalidateEvent, exclusiveEndDates) {
      var onlyStartEnd = invalidateEvent === 'start-end';
      var until = exclusiveEndDates ? end : getDateOnly(addDays(end, 1));
      for (var _i = 0, _a = Object.keys(invalids); _i < _a.length; _i++) {
          var r = _a[_i];
          var invalidMap = invalids[r];
          for (var d = getDateOnly(start); d < until; d.setDate(d.getDate() + 1)) {
              var invalidsForDay = invalidMap[getDateStr(d)];
              if (invalidsForDay) {
                  if (invalidsForDay.allDay && (!onlyStartEnd || isSameDay(d, start) || isSameDay(d, end))) {
                      return invalidsForDay.allDay.original;
                  }
                  if (!allDay) {
                      for (var _b = 0, _c = invalidsForDay.invalids; _b < _c.length; _b++) {
                          var inv = _c[_b];
                          if (onlyStartEnd) {
                              if (checkDateRangeOverlap(inv.startDate, inv.endDate, start, start, true)) {
                                  return inv.original;
                              }
                              if (checkDateRangeOverlap(inv.startDate, inv.endDate, end, end)) {
                                  return inv.original;
                              }
                          }
                          else if (checkDateRangeOverlap(inv.startDate, inv.endDate, start, end)) {
                              return inv.original;
                          }
                      }
                  }
              }
          }
      }
      return false;
  }
  function getEventLayoutStart(event, s, isListing, isTimeline, isDailyResolution, firstDay, cols, colIndexMap) {
      var eventAllDay = event.allDay || isListing;
      var startDate = event.startDate;
      if (isTimeline && isListing && !isDailyResolution) {
          var startCol = colIndexMap[getDateStr(startDate)];
          return startDate < firstDay ? firstDay : cols[startCol + (isInWeek(startDate.getDay(), s.startDay, s.endDay) ? 0 : 1)].date;
      }
      return eventAllDay ? createDate(s, startDate.getFullYear(), startDate.getMonth(), startDate.getDate()) : startDate;
  }
  function getEventLayoutEnd(event, s, isListing, isTimeline, isDailyResolution, lastDay, cols, colIndexMap) {
      var eventAllDay = event.allDay || isListing;
      var endDate = event.endDate;
      if (isTimeline && isListing && !isDailyResolution) {
          var endCol = colIndexMap[getDateStr(getEndDate(s, event.allDay, event.startDate, endDate))];
          var endD = endDate >= lastDay || endCol >= cols.length - 1 ? lastDay : cols[endCol + 1].date;
          return getEndDate(s, false, event.startDate, endD);
      }
      var eEnd = eventAllDay ? getEndDate(s, event.allDay, event.startDate, endDate) : endDate;
      return eventAllDay ? createDate(s, eEnd.getFullYear(), eEnd.getMonth(), eEnd.getDate(), 23, 59, 59, 999) : eEnd;
  }
  /** @hidden */
  function calcLayout(s, groups, event, next, isListing, isTimeline, isDailyResolution, firstDay, firstDayTz, lastDay, lastDayTz, cols, colIndexMap) {
      // Layout algorithm
      // Overlapping events are organized in groups, groups are organized in levels (columns)
      var first = event.allDay ? firstDay : firstDayTz;
      var last = event.allDay ? lastDay : lastDayTz;
      var eventStart = getEventLayoutStart(event, s, isListing, isTimeline, isDailyResolution, first, cols, colIndexMap);
      var eventEnd = getEventLayoutEnd(event, s, isListing, isTimeline, isDailyResolution, last, cols, colIndexMap);
      var pushed = false;
      for (var _i = 0, groups_1 = groups; _i < groups_1.length; _i++) {
          var group = groups_1[_i];
          var i = 0;
          var groupOverlap = false;
          var groupLevel = void 0;
          for (var _a = 0, group_1 = group; _a < group_1.length; _a++) {
              var level = group_1[_a];
              var overlap = false;
              for (var _b = 0, level_1 = level; _b < level_1.length; _b++) {
                  var item = level_1[_b];
                  // The collision check works on timestamps, so the right timestamp for allDay events will be the start of the start day
                  // and end of the end day (1ms is already removed in case of exclusive end dates).
                  // In case of timezones, the allDay event dates are always in the display timezone (meaning
                  // they don't use timezones at all) and the times are not taken into account.
                  var firstD = item.allDay ? firstDay : firstDayTz;
                  var lastD = item.allDay ? lastDay : lastDayTz;
                  var itemStart = getEventLayoutStart(item, s, isListing, isTimeline, isDailyResolution, firstD, cols, colIndexMap);
                  var itemEnd = getEventLayoutEnd(item, s, isListing, isTimeline, isDailyResolution, lastD, cols, colIndexMap);
                  if (checkDateRangeOverlap(itemStart, itemEnd, eventStart, eventEnd, true)) {
                      overlap = true;
                      groupOverlap = true;
                      if (groupLevel) {
                          next[event.uid] = next[event.uid] || i;
                      }
                      else {
                          next[item.uid] = i + 1;
                      }
                  }
              }
              // There is place on this level, if the event belongs to this group, will be added here
              if (!overlap && !groupLevel) {
                  groupLevel = level;
              }
              i++;
          }
          // If event belongs to this group
          if (groupOverlap) {
              if (groupLevel) {
                  // Add to existing level
                  groupLevel.push(event);
              }
              else {
                  // Add to new level
                  group.push([event]);
              }
              pushed = true;
          }
      }
      // Create a new group
      if (!pushed) {
          next[event.uid] = 0;
          groups.push([[event]]);
      }
  }
  /** @hidden */
  function roundStep(v) {
      // Don't allow negative values
      v = Math.abs(round(v));
      if (v > 60) {
          return round(v / 60) * 60;
      }
      if (60 % v === 0) {
          return v;
      }
      return [6, 10, 12, 15, 20, 30].reduce(function (a, b) {
          return Math.abs(b - v) < Math.abs(a - v) ? b : a;
      });
  }
  /** @hidden */
  function getEventHeight(startDate, endDate, displayedTime, startTime, endTime) {
      var start = getDayMilliseconds(startDate);
      var end = getDayMilliseconds(endDate);
      if (startTime > start) {
          start = startTime;
      }
      if (endTime < end) {
          end = endTime;
      }
      return ((end - start) * 100) / displayedTime;
  }
  /** @hidden */
  function getEventWidth(startDate, endDate, displayedTime, viewStart, viewEnd, startTime, endTime, startDay, endDay, fullDay) {
      var startD = startDate;
      var endD = endDate;
      var until = addDays(getDateOnly(endD), 1);
      if (startD < viewStart) {
          startD = viewStart;
      }
      if (endD > viewEnd) {
          endD = until = viewEnd;
      }
      var start = getDayMilliseconds(startD);
      var end = getDayMilliseconds(endD);
      // limit the start/end time of the events
      if (startTime > start) {
          start = startTime;
      }
      if (endTime < end) {
          end = endTime;
      }
      // in case of multi-day events limit the start/end hours if moved to a position without a cursor date change
      if (!isSameDay(startD, endD)) {
          if (start > endTime) {
              start = endTime;
          }
          if (end < startTime) {
              end = startTime;
          }
      }
      var time = 0;
      if (isSameDay(startD, endD)) {
          time = fullDay ? displayedTime : end - start;
      }
      else {
          for (var d = getDateOnly(startD); d < until; d.setDate(d.getDate() + 1)) {
              if (isInWeek(d.getDay(), startDay, endDay)) {
                  if (!fullDay && isSameDay(d, startD)) {
                      time += displayedTime - start + startTime;
                  }
                  else if (!fullDay && isSameDay(d, endD)) {
                      time += end - startTime;
                  }
                  else {
                      time += displayedTime;
                  }
              }
          }
      }
      return (time * 100) / displayedTime;
  }
  /** @hidden */
  function getEventStart(startDate, startTime, displayedTime, viewStart, startDay, endDay) {
      if (viewStart && viewStart > startDate) {
          startDate = viewStart;
      }
      var start = getDayMilliseconds(startDate);
      if (startTime > start || (startDay !== UNDEFINED && endDay !== UNDEFINED && !isInWeek(startDate.getDay(), startDay, endDay))) {
          start = startTime;
      }
      return ((start - startTime) * 100) / displayedTime;
  }
  /** @hidden */
  function getResourceMap(eventsMap, resources, slots, hasResources, hasSlots) {
      eventsMap = eventsMap || {};
      var eventKeys = Object.keys(eventsMap);
      var resourceMap = {};
      var resourceIds = resources.map(function (resource) { return resource.id; });
      var slotIds = slots.map(function (s) { return s.id; });
      resourceIds.forEach(function (rid) {
          resourceMap[rid] = {};
          slotIds.forEach(function (sid) {
              resourceMap[rid][sid] = {};
          });
      });
      var _loop_1 = function (timestamp) {
          var events = eventsMap[timestamp];
          var _loop_2 = function (event_1) {
              var eventResource = event_1.resource;
              var eventSlot = event_1.slot;
              // If resources are not passed at all (null or undefined), we'll show all events.
              // If the event has not resource specified, show it for all resources.
              var res = eventResource === UNDEFINED || !hasResources ? resourceIds : isArray(eventResource) ? eventResource : [eventResource];
              var slot = eventSlot === UNDEFINED || !hasSlots ? slotIds : [eventSlot];
              res.forEach(function (rid) {
                  var map = resourceMap[rid];
                  if (map) {
                      slot.forEach(function (sid) {
                          var slotMap = map[sid];
                          if (slotMap) {
                              if (!slotMap[timestamp]) {
                                  slotMap[timestamp] = [];
                              }
                              slotMap[timestamp].push(event_1);
                          }
                      });
                  }
              });
          };
          for (var _i = 0, events_1 = events; _i < events_1.length; _i++) {
              var event_1 = events_1[_i];
              _loop_2(event_1);
          }
      };
      for (var _i = 0, eventKeys_1 = eventKeys; _i < eventKeys_1.length; _i++) {
          var timestamp = eventKeys_1[_i];
          _loop_1(timestamp);
      }
      return resourceMap;
  }
  /** @hidden */
  function getCellDate(timestamp, ms) {
      var d = new Date(timestamp);
      var time = new Date(+REF_DATE + ms); // Date with no DST
      return new Date(d.getFullYear(), d.getMonth(), d.getDate(), time.getHours(), time.getMinutes());
  }

  var stateObservables = {};
  /** @hidden */
  var ScheduleEventBase = /*#__PURE__*/ (function (_super) {
      __extends(ScheduleEventBase, _super);
      function ScheduleEventBase() {
          var _this = _super !== null && _super.apply(this, arguments) || this;
          // tslint:enable variable-name
          // tslint:disable-next-line: variable-name
          _this._onClick = function (ev) {
              _this._triggerClick('onClick', ev);
              var s = _this.s;
              var observable = stateObservables[s.event.uid];
              if (observable && s.selected) {
                  observable.next({ hasFocus: false });
              }
          };
          // tslint:disable-next-line: variable-name
          _this._onRightClick = function (ev) {
              _this._triggerClick('onRightClick', ev);
          };
          // tslint:disable-next-line: variable-name
          _this._onDocTouch = function (ev) {
              unlisten(_this._doc, TOUCH_START, _this._onDocTouch);
              unlisten(_this._doc, MOUSE_DOWN, _this._onDocTouch);
              _this._isDrag = false;
              _this._hook('onDragModeOff', {
                  domEvent: ev,
                  event: _this.s.event.original,
              });
          };
          // tslint:disable-next-line: variable-name
          _this._updateState = function (args) {
              _this.setState(args);
          };
          return _this;
      }
      ScheduleEventBase.prototype._render = function (s, state) {
          var event = s.event;
          var day = new Date(event.date);
          var pos = event.position;
          var startDate = event.startDate;
          var endDate = getEndDate(s, event.allDay, startDate, event.endDate);
          var isTimeline = s.isTimeline;
          var isTimelineListing = s.isListing;
          var isAllDay = isTimelineListing || event.allDay;
          var isMultiDay = !isSameDay(startDate, endDate);
          var isMultiDayStart = isMultiDay && isSameDay(startDate, day);
          var isMultiDayEnd = isMultiDay && isSameDay(endDate, day);
          var allDayStyle = isAllDay && (!isTimeline || isTimelineListing);
          var host = isTimeline ? 'timeline' : 'schedule';
          var gridStartTime = s.gridStartTime;
          var gridEndTime = s.gridEndTime;
          var startTime = getDayMilliseconds(startDate);
          var endTime = getDayMilliseconds(endDate);
          var hasSlots = isTimeline && s.slot !== DEF_ID;
          var isEndInWeek = isInWeek(endDate.getDay(), s.startDay, s.endDay);
          var lastDay = s.singleDay ? addDays(day, 1) : new Date(s.lastDay);
          if (!event.allDay) {
              lastDay = addTimezone(s, lastDay);
          }
          this._isStart = hasSlots || !isMultiDay || isMultiDayStart;
          this._isEnd = hasSlots || !isMultiDay || (isAllDay || (isTimeline && !s.hasResY) ? endDate < lastDay && isEndInWeek : isMultiDayEnd);
          if (!hasSlots && !isAllDay && (gridStartTime > startTime || gridEndTime < startTime)) {
              this._isStart = false;
          }
          if (!hasSlots && !isAllDay && (gridEndTime < endTime || gridStartTime > endTime)) {
              this._isEnd = false;
          }
          // check for custom templates
          this._isDrag = this._isDrag || s.isDrag;
          this._content = UNDEFINED;
          this._rangeText = event.start + ' - ' + event.end;
          this._isAllDay = allDayStyle;
          this._host = host;
          if (event.allDay || ((!isTimeline || s.hasResY) && isMultiDay && !isMultiDayStart && !isMultiDayEnd)) {
              this._rangeText = event.allDayText || '\u00A0';
          }
          this._cssClass =
              'mbsc-schedule-event' +
                  this._theme +
                  this._rtl +
                  (s.render || s.template ? ' mbsc-schedule-event-custom' : '') +
                  (isTimeline ? ' mbsc-timeline-event' : '') +
                  (isTimelineListing ? ' mbsc-timeline-event-listing' : '') +
                  (this._isStart ? " mbsc-" + host + "-event-start" : '') +
                  (this._isEnd ? " mbsc-" + host + "-event-end" : '') +
                  (allDayStyle ? ' mbsc-schedule-event-all-day' : '') +
                  (hasSlots ? ' mbsc-timeline-event-slot' : '') +
                  ((state.hasFocus && !s.inactive && !s.selected) || s.selected ? ' mbsc-schedule-event-active' : '') +
                  (state.hasHover && !s.inactive && !this._isDrag ? ' mbsc-schedule-event-hover' : '') +
                  (s.isDrag ? ' mbsc-schedule-event-dragging' + (isTimeline ? ' mbsc-timeline-event-dragging' : '') : '') +
                  (s.hidden ? ' mbsc-schedule-event-hidden' : '') +
                  (s.inactive ? ' mbsc-schedule-event-inactive' : '') +
                  (event.original.editable === false ? ' mbsc-readonly-event' : '') +
                  (event.original.cssClass ? ' ' + event.original.cssClass : '');
          this._style = __assign({}, pos, { color: event.color, top: s.eventHeight && pos.top !== UNDEFINED ? pos.top * s.eventHeight + 'px' : pos.top });
          var renderer = s.render || s.renderContent;
          var text;
          if (renderer) {
              var content = renderer(event);
              if (isString(content)) {
                  text = content;
              }
              else {
                  this._content = content;
              }
          }
          else if (!s.contentTemplate) {
              text = event.html;
          }
          if (text !== this._text) {
              this._text = text;
              this._html = text ? this._safeHtml(text) : UNDEFINED;
              this._shouldEnhance = text && !!renderer;
          }
      };
      ScheduleEventBase.prototype._mounted = function () {
          var _this = this;
          var id = this.s.event.uid;
          var resizeDir;
          var observable = stateObservables[id];
          var startDomEvent;
          var touchTimer;
          if (!observable) {
              observable = new Observable();
              stateObservables[id] = observable;
          }
          this._unsubscribe = observable.subscribe(this._updateState);
          this._doc = getDocument(this._el);
          this._unlisten = gestureListener(this._el, {
              keepFocus: true,
              onBlur: function () {
                  observable.next({ hasFocus: false });
              },
              onDoubleClick: function (ev) {
                  // Prevent event creation on label double click
                  ev.domEvent.stopPropagation();
                  _this._triggerClick('onDoubleClick', ev.domEvent);
              },
              onEnd: function (ev) {
                  if (_this._isDrag) {
                      var s = _this.s;
                      var args = __assign({}, ev);
                      // Will prevent mousedown event on doc
                      args.domEvent.preventDefault();
                      args.event = s.event;
                      args.resource = s.resource;
                      args.slot = s.slot;
                      if (s.resize && resizeDir) {
                          args.resize = true;
                          args.direction = resizeDir;
                      }
                      else if (s.drag) {
                          args.drag = true;
                      }
                      _this._hook('onDragEnd', args);
                      // Turn off update, unless we're in touch update mode
                      if (!s.isDrag) {
                          _this._isDrag = false;
                      }
                      if (_this._el && args.moved) {
                          _this._el.blur();
                      }
                  }
                  clearTimeout(touchTimer);
                  resizeDir = UNDEFINED;
              },
              onFocus: function () {
                  observable.next({ hasFocus: true });
              },
              onHoverIn: function (ev) {
                  observable.next({ hasHover: true });
                  _this._triggerClick('onHoverIn', ev);
              },
              onHoverOut: function (ev) {
                  observable.next({ hasHover: false });
                  _this._triggerClick('onHoverOut', ev);
              },
              onKeyDown: function (ev) {
                  var event = _this.s.event.original;
                  switch (ev.keyCode) {
                      case ENTER:
                      case SPACE:
                          _this._el.click();
                          ev.preventDefault();
                          break;
                      case BACKSPACE:
                      case DELETE:
                          if (event.editable !== false) {
                              _this._hook('onDelete', {
                                  domEvent: ev,
                                  event: event,
                                  source: _this._host,
                              });
                          }
                          break;
                  }
              },
              onMove: function (ev) {
                  var s = _this.s;
                  var args = __assign({}, ev);
                  args.event = s.event;
                  args.resource = s.resource;
                  args.slot = s.slot;
                  if (resizeDir) {
                      args.resize = true;
                      args.direction = resizeDir;
                  }
                  else if (s.drag) {
                      args.drag = true;
                  }
                  else {
                      return;
                  }
                  if (s.event.original.editable === false) {
                      return;
                  }
                  if (_this._isDrag || !args.isTouch) {
                      // Prevents page scroll on touch and text selection with mouse
                      args.domEvent.preventDefault();
                  }
                  if (_this._isDrag) {
                      _this._hook('onDragMove', args);
                  }
                  else if (Math.abs(args.deltaX) > 7 || Math.abs(args.deltaY) > 7) {
                      clearTimeout(touchTimer);
                      if (!args.isTouch) {
                          args.domEvent = startDomEvent;
                          _this._isDrag = true;
                          _this._hook('onDragStart', args);
                      }
                  }
              },
              onStart: function (ev) {
                  startDomEvent = ev.domEvent;
                  var s = _this.s;
                  var args = __assign({}, ev);
                  var target = startDomEvent.target;
                  args.event = s.event;
                  args.resource = s.resource;
                  args.slot = s.slot;
                  if (s.resize && target.classList.contains('mbsc-schedule-event-resize')) {
                      resizeDir = target.classList.contains('mbsc-schedule-event-resize-start') ? 'start' : 'end';
                      args.resize = true;
                      args.direction = resizeDir;
                  }
                  else if (s.drag) {
                      args.drag = true;
                  }
                  else {
                      return;
                  }
                  if (s.event.original.editable === false) {
                      return;
                  }
                  if (_this._isDrag) {
                      startDomEvent.stopPropagation();
                      _this._hook('onDragStart', args);
                  }
                  else if (args.isTouch) {
                      touchTimer = setTimeout(function () {
                          _this._hook('onDragModeOn', args);
                          _this._hook('onDragStart', args);
                          _this._isDrag = true;
                      }, 350);
                  }
              },
          });
          if (this._isDrag) {
              listen(this._doc, TOUCH_START, this._onDocTouch);
              listen(this._doc, MOUSE_DOWN, this._onDocTouch);
          }
      };
      ScheduleEventBase.prototype._destroy = function () {
          if (this._el) {
              this._el.blur();
          }
          if (this._unsubscribe) {
              var id = this.s.event.uid;
              var observable = stateObservables[id];
              if (observable) {
                  observable.unsubscribe(this._unsubscribe);
                  if (!observable.nr) {
                      delete stateObservables[id];
                  }
              }
          }
          if (this._unlisten) {
              this._unlisten();
          }
          unlisten(this._doc, TOUCH_START, this._onDocTouch);
          unlisten(this._doc, MOUSE_DOWN, this._onDocTouch);
      };
      ScheduleEventBase.prototype._triggerClick = function (name, domEvent) {
          var s = this.s;
          this._hook(name, {
              date: s.event.date,
              domEvent: domEvent,
              event: s.event.original,
              resource: s.resource,
              slot: s.slot,
              source: this._host,
          });
      };
      return ScheduleEventBase;
  }(BaseComponent));

  function template$6(s, inst) {
      var _a;
      var event = s.event;
      var isAllDay = inst._isAllDay;
      var isTimeline = s.isTimeline;
      var theme = inst._theme;
      var editable = s.resize && event.original.editable !== false;
      var rightClick = (_a = {}, _a[ON_CONTEXT_MENU] = inst._onRightClick, _a);
      return (createElement("div", __assign({ tabIndex: 0, className: inst._cssClass, "data-id": event.id, style: inst._style, ref: inst._setEl, title: event.tooltip, onClick: inst._onClick }, rightClick),
          inst._isStart && editable && (createElement("div", { className: 'mbsc-schedule-event-resize mbsc-schedule-event-resize-start' +
                  (isTimeline ? ' mbsc-timeline-event-resize' : '') +
                  inst._rtl +
                  (s.isDrag ? ' mbsc-schedule-event-resize-start-touch' : '') })),
          inst._isEnd && editable && (createElement("div", { className: 'mbsc-schedule-event-resize mbsc-schedule-event-resize-end' +
                  (isTimeline ? ' mbsc-timeline-event-resize' : '') +
                  inst._rtl +
                  (s.isDrag ? ' mbsc-schedule-event-resize-end-touch' : '') })),
          s.render ? (
          // Full custom template (_content is vdom markup, _html is string)
          inst._html ? (createElement("div", { style: { height: '100%' }, dangerouslySetInnerHTML: inst._html })) : (inst._content)) : (
          // Default template
          createElement(Fragment, null,
              !isAllDay && !isTimeline && createElement("div", { className: 'mbsc-schedule-event-bar' + theme + inst._rtl }),
              createElement("div", { className: 'mbsc-schedule-event-background' +
                      (isTimeline ? ' mbsc-timeline-event-background' : '') +
                      (isAllDay ? ' mbsc-schedule-event-all-day-background' : '') +
                      theme, style: { background: event.style.background } }),
              createElement("div", { "aria-hidden": "true", className: 'mbsc-schedule-event-inner' + theme + (isAllDay ? ' mbsc-schedule-event-all-day-inner' : '') + (event.cssClass || ''), style: { color: event.style.color } },
                  createElement("div", { className: 'mbsc-schedule-event-title' + (isAllDay ? ' mbsc-schedule-event-all-day-title' : '') + theme, dangerouslySetInnerHTML: inst._html }, inst._content),
                  !isAllDay && createElement("div", { className: 'mbsc-schedule-event-range' + theme }, inst._rangeText)),
              event.ariaLabel && createElement("div", { className: "mbsc-hidden-content" }, event.ariaLabel)))));
  }
  var ScheduleEvent = /*#__PURE__*/ (function (_super) {
      __extends(ScheduleEvent, _super);
      function ScheduleEvent() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      ScheduleEvent.prototype._template = function (s) {
          return template$6(s, this);
      };
      return ScheduleEvent;
  }(ScheduleEventBase));

  /** @hidden */
  var TimeIndicatorBase = /*#__PURE__*/ (function (_super) {
      __extends(TimeIndicatorBase, _super);
      function TimeIndicatorBase() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      // tslint:enable variable-name
      TimeIndicatorBase.prototype._mounted = function () {
          var _this = this;
          clearInterval(this._timer);
          this._timer = setInterval(function () {
              if (_this._zone) {
                  _this._zone.runOutsideAngular(function () {
                      _this.forceUpdate();
                  });
              }
              else {
                  _this.forceUpdate();
              }
          }, 10000);
      };
      TimeIndicatorBase.prototype._destroy = function () {
          clearInterval(this._timer);
      };
      TimeIndicatorBase.prototype._render = function (s) {
          var now = createDate(s);
          var rtl = s.rtl;
          var displayedDays = s.displayedDays;
          var displayedTime = s.displayedTime;
          var startTime = s.startTime;
          var time = floor(getDayMilliseconds(now) / ONE_MIN) * ONE_MIN;
          var timezones = s.timezones;
          var formatOpt = { amText: s.amText, pmText: s.pmText };
          if (timezones && isMBSCDate(now)) {
              this._times = [];
              for (var _i = 0, timezones_1 = timezones; _i < timezones_1.length; _i++) {
                  var t = timezones_1[_i];
                  var tzNow = now.clone();
                  tzNow.setTimezone(t.timezone);
                  this._times.push(formatDate(s.timeFormat, tzNow, formatOpt));
              }
          }
          else {
              this._time = formatDate(s.timeFormat, now, formatOpt);
          }
          this._cssClass =
              'mbsc-schedule-time-indicator mbsc-schedule-time-indicator-' +
                  s.orientation +
                  this._theme +
                  this._rtl +
                  ' ' +
                  // (s.cssClass || '') +
                  (time < startTime || time > startTime + displayedTime || !isInWeek(now.getDay(), s.startDay, s.endDay) ? ' mbsc-hidden' : '');
          var dayIndex = s.hasResY ? 0 : getGridDayDiff(s.firstDay, now, s.startDay, s.endDay);
          if (s.orientation === 'x') {
              var horiz = (dayIndex * 100) / displayedDays + '%';
              var multiPos = timezones && 4.25 * timezones.length + 'em';
              this._pos = {
                  left: timezones && !rtl ? multiPos : UNDEFINED,
                  right: timezones && rtl ? multiPos : UNDEFINED,
                  top: ((time - startTime) * 100) / displayedTime + '%',
              };
              this._dayPos = {
                  left: rtl ? '' : horiz,
                  right: rtl ? horiz : '',
                  width: 100 / displayedDays + '%',
              };
          }
          else {
              var pos = ((dayIndex * displayedTime + time - startTime) * 100) / (displayedDays * displayedTime) + '%';
              this._pos = {
                  left: rtl ? '' : pos,
                  right: rtl ? pos : '',
              };
          }
      };
      return TimeIndicatorBase;
  }(BaseComponent));

  function template$7(s, inst) {
      var timezones = s.timezones;
      return (createElement("div", { "aria-hidden": "true", className: inst._cssClass, style: inst._pos },
          createElement("div", { className: (timezones ? 'mbsc-flex ' : '') +
                  'mbsc-schedule-time-indicator-time mbsc-schedule-time-indicator-time-' +
                  s.orientation +
                  inst._theme +
                  inst._rtl }, timezones
              ? timezones.map(function (t, i) {
                  return (createElement("div", { key: i, className: 'mbsc-schedule-time-indicator-tz' + inst._theme + inst._rtl }, inst._times[i]));
              })
              : inst._time),
          s.showDayIndicator && createElement("div", { className: 'mbsc-schedule-time-indicator-day' + inst._theme + inst._rtl, style: inst._dayPos })));
  }
  // tslint:disable no-non-null-assertion
  /**
   * The TimeIndicator component.
   *
   * Usage:
   *
   * ```
   * <TimeIndicator scheduleTimeIndicatorPosition="{}" />
   * ```
   */
  var TimeIndicator = /*#__PURE__*/ (function (_super) {
      __extends(TimeIndicator, _super);
      function TimeIndicator() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      TimeIndicator.prototype._template = function (s) {
          return template$7(s, this);
      };
      return TimeIndicator;
  }(TimeIndicatorBase));

  /** @hidden */
  var WeekDayBase = /*#__PURE__*/ (function (_super) {
      __extends(WeekDayBase, _super);
      function WeekDayBase() {
          var _this = _super !== null && _super.apply(this, arguments) || this;
          // tslint:enable variable-name
          // tslint:disable-next-line variable-name
          _this._onClick = function () {
              var s = _this.s;
              if (s.selectable) {
                  s.onClick(s.timestamp);
              }
          };
          return _this;
      }
      WeekDayBase.prototype._render = function (s, state) {
          var date = new Date(s.timestamp);
          this._cssClass =
              'mbsc-schedule-header-item ' +
                  this._className +
                  this._theme +
                  this._rtl +
                  this._hb +
                  (s.largeNames ? ' mbsc-schedule-header-item-large' : '') +
                  (s.selected ? ' mbsc-selected' : '') +
                  (state.hasHover ? ' mbsc-hover' : '');
          this._data = {
              date: date,
              events: s.events || [],
              resource: s.resource,
              selected: s.selected,
          };
          this._day = date.getDay();
      };
      WeekDayBase.prototype._mounted = function () {
          var _this = this;
          this._unlisten = gestureListener(this._el, {
              onHoverIn: function () {
                  if (_this.s.selectable) {
                      _this.setState({ hasHover: true });
                  }
              },
              onHoverOut: function () {
                  if (_this.s.selectable) {
                      _this.setState({ hasHover: false });
                  }
              },
          });
      };
      WeekDayBase.prototype._destroy = function () {
          if (this._unlisten) {
              this._unlisten();
          }
      };
      return WeekDayBase;
  }(BaseComponent));

  function template$8(s, state, inst) {
      var content;
      if (s.renderDay) {
          content = s.renderDay(inst._data);
      }
      if (s.renderDayContent) {
          content = s.renderDayContent(inst._data);
      }
      if (isString(content)) {
          content = createElement("div", { dangerouslySetInnerHTML: inst._safeHtml(content) });
          inst._shouldEnhance = true;
      }
      return (createElement("div", { ref: inst._setEl, className: inst._cssClass, onClick: inst._onClick }, s.renderDay ? (content) : (createElement(Fragment, null,
          createElement("div", { "aria-hidden": "true", className: 'mbsc-schedule-header-dayname' +
                  inst._theme +
                  (s.selected ? ' mbsc-selected' : '') +
                  (s.isToday ? ' mbsc-schedule-header-dayname-curr' : '') }, s.dayNames[inst._day]),
          createElement("div", { "aria-hidden": "true", className: 'mbsc-schedule-header-day' +
                  inst._theme +
                  inst._rtl +
                  (s.selected ? ' mbsc-selected' : '') +
                  (s.isToday ? ' mbsc-schedule-header-day-today' : '') +
                  (state.hasHover ? ' mbsc-hover' : '') }, s.day),
          s.label && (createElement("div", { className: "mbsc-hidden-content", "aria-pressed": s.selectable ? (s.selected ? 'true' : 'false') : UNDEFINED, role: s.selectable ? 'button' : UNDEFINED }, s.label)),
          s.renderDayContent && content))));
  }
  var WeekDay = /*#__PURE__*/ (function (_super) {
      __extends(WeekDay, _super);
      function WeekDay() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      WeekDay.prototype._template = function (s, state) {
          return template$8(s, state, this);
      };
      return WeekDay;
  }(WeekDayBase));

  // tslint:disable no-non-null-assertion
  // tslint:disable no-inferrable-types
  // tslint:disable directive-class-suffix
  // tslint:disable directive-selector
  /** @hidden */
  var STBase = /*#__PURE__*/ (function (_super) {
      __extends(STBase, _super);
      function STBase() {
          var _this = _super !== null && _super.apply(this, arguments) || this;
          _this._isScrolling = 0;
          // tslint:disable-next-line: variable-name no-empty
          _this._onScroll = function () { };
          // tslint:disable-next-line: variable-name
          _this._onMouseLeave = function (ev, force) {
              if (_this._cursorTimeCont && (!_this.state.dragData || force)) {
                  _this._cursorTimeCont.style.visibility = 'hidden';
                  _this._isCursorTimeVisible = false;
              }
          };
          // tslint:disable-next-line: variable-name
          _this._onMouseMove = function (ev) {
              if (_this._showCursorTime) {
                  var s = _this.s;
                  var rtl = s.rtl;
                  var isTimeline = _this._isTimeline;
                  var timeCont = _this._cursorTimeCont;
                  if (!_this._isTouch || _this._tempStart) {
                      if (!_this._isCursorTimeVisible && ev) {
                          timeCont.style.visibility = 'visible';
                          _this._isCursorTimeVisible = true;
                      }
                  }
                  else {
                      timeCont.style.visibility = 'hidden';
                      _this._isCursorTimeVisible = false;
                  }
                  if (_this._isCursorTimeVisible && _this._colWidth) {
                      var gridCont = _this._gridCont;
                      var gridRect = gridCont.getBoundingClientRect();
                      var clientX = ev ? ev.clientX : _this._cursorX || 0;
                      var clientY = ev ? ev.clientY : _this._cursorY || 0;
                      var posX = rtl ? gridRect.right - clientX : clientX - gridRect.left;
                      var posY = constrain(clientY - gridRect.top, 8, _this._colHeight);
                      var dayIndex = void 0;
                      var date = void 0;
                      var time = void 0;
                      if (_this._dragDelta !== UNDEFINED) {
                          // use the _tempStart/_tempEnd since that is already calculated
                          date = createDate(s, _this._dragDelta < 0 ? _this._tempStart : _this._tempEnd);
                          dayIndex = isTimeline && !_this._hasResY ? _this._dayIndexMap[getDateStr(date)] : 0;
                          time = getDayMilliseconds(date);
                          time = time === 0 ? (_this._dragDelta < 0 ? time : ONE_DAY) : time;
                      }
                      else {
                          dayIndex = isTimeline && !_this._hasResY ? constrain(floor(posX / _this._colWidth), 0, _this._daysNr - 1) : 0;
                          time =
                              _this._startTime +
                                  step(isTimeline
                                      ? floor((_this._time * (posX - dayIndex * _this._colWidth)) / _this._colWidth)
                                      : floor((_this._time * (posY - 8)) / (_this._colHeight - 16)), s.dragTimeStep * ONE_MIN); // Remove 16px for top and bottom spacing
                          var day = _this._days[dayIndex].date;
                          var d = new Date(+REF_DATE + time); // Date with no DST
                          date = createDate(s, day.getFullYear(), day.getMonth(), day.getDate(), d.getHours(), d.getMinutes());
                      }
                      var milliSeconds = _this._time * (isTimeline ? _this._daysNr : 1);
                      var pos = isTimeline ? (rtl ? 'right' : 'left') : 'top';
                      var timeContStyle = timeCont.style;
                      timeContStyle[pos] = ((dayIndex * _this._time + time - _this._startTime) * 100) / milliSeconds + '%';
                      timeContStyle[rtl ? 'left' : 'right'] = '';
                      timeCont.textContent = formatDate(s.timeFormat, date, s);
                      _this._cursorX = clientX;
                      _this._cursorY = clientY;
                  }
              }
          };
          // #region Drag & Drop
          // tslint:disable-next-line: variable-name
          _this._onEventDragModeOn = function (args) {
              if (_this.s.externalDrag && args.drag && !args.create) {
                  dragObservable.next(__assign({}, args, { create: true, eventName: 'onDragModeOn', external: true, from: _this }));
              }
              var event = args.create ? _this._tempEvent : args.event;
              var resource = args.create ? _this._tempResource : args.resource;
              var slot = args.create ? _this._tempSlot : args.slot;
              _this.setState({
                  dragData: {
                      draggedEvent: event,
                      originDates: args.external ? UNDEFINED : _this._getDragDates(event, resource, slot),
                      resource: resource,
                  },
                  isTouchDrag: true,
              });
          };
          // tslint:disable-next-line: variable-name
          _this._onEventDragModeOff = function (args) {
              _this._hook('onEventDragEnd', {
                  domEvent: args.domEvent,
                  event: args.event,
                  resource: _this._tempResource !== DEF_ID ? _this._tempResource : UNDEFINED,
                  slot: _this._tempSlot !== DEF_ID ? _this._tempSlot : UNDEFINED,
                  source: _this._isTimeline ? 'timeline' : 'schedule',
              });
              _this.setState({
                  dragData: UNDEFINED,
                  isTouchDrag: false,
              });
          };
          // tslint:disable-next-line: variable-name
          _this._onEventDragStart = function (args) {
              var s = _this.s;
              var isClick = args.click;
              var isListing = s.eventList;
              var isTimeline = _this._isTimeline;
              var resources = _this._visibleResources;
              var slots = _this._slots;
              var timeStep = s.dragTimeStep;
              var startX = args.startX;
              var startY = args.startY;
              _this._isTouch = args.isTouch;
              _this._scrollY = 0;
              _this._scrollX = 0;
              _this._calcGridSizes();
              var posX = s.rtl ? _this._gridRight - startX : startX - _this._gridLeft;
              var posY = constrain(startY - _this._gridTop, 8, _this._colHeight - 9); // There's 8px top and 8px bottom spacing
              var cols = isListing ? _this._cols : _this._days;
              var colsNr = cols.length;
              var colWidth = _this._colWidth;
              var colIndex = colWidth ? floor(posX / colWidth) : 1;
              var resourceTops = _this._resourceTops;
              var resourceIndex = 0;
              var dayIndex = colIndex;
              var slotIndex = 0;
              if (s.externalDrag && args.drag && !args.create) {
                  var eventEl = closest(args.domEvent.target, '.mbsc-schedule-event', _this._el);
                  var clone = eventEl.cloneNode(true);
                  var cloneClass = clone.classList;
                  clone.style.display = 'none';
                  cloneClass.add('mbsc-drag-clone', 'mbsc-schedule-drag-clone', 'mbsc-font');
                  cloneClass.remove('mbsc-schedule-event-hover');
                  _this._clone = clone;
                  _this._body = getDocument(_this._el).body;
                  _this._body.appendChild(clone);
                  _this._eventDropped = false;
                  dragObservable.next(__assign({}, args, { create: true, event: args.event.original, eventName: 'onDragStart', external: true, from: _this }));
              }
              if (!isTimeline) {
                  var groupByResource = _this._groupByResource;
                  var groupCount = groupByResource ? colsNr : _this._hasSlots ? _this._slots.length : resources.length;
                  resourceIndex = groupByResource ? floor(colIndex / groupCount) : colIndex % groupCount;
                  dayIndex = groupByResource ? colIndex % groupCount : floor(colIndex / groupCount);
              }
              else {
                  slotIndex = colWidth ? floor(posX / (colWidth / slots.length)) % slots.length : 1;
                  if (_this._hasResY) {
                      cols.forEach(function (d, i) {
                          resources.forEach(function (r, j) {
                              if (posY > resourceTops[d.dateKey + '-' + r.id]) {
                                  dayIndex = i;
                                  resourceIndex = j;
                              }
                          });
                      });
                  }
                  else {
                      resources.forEach(function (r, i) {
                          if (posY > resourceTops[r.id]) {
                              resourceIndex = i;
                          }
                      });
                  }
              }
              var resource = args.external ? UNDEFINED : resources[resourceIndex];
              var resourceId = resource ? resource.id : UNDEFINED;
              var slot = args.external ? UNDEFINED : slots[slotIndex];
              var slotId = slot ? slot.id : UNDEFINED;
              if (resource && resource.eventCreation === false) {
                  return false;
              }
              if (args.create) {
                  dayIndex = constrain(dayIndex, 0, colsNr - 1);
                  // It's enough to check the bottom of the all day area,
                  // because the create gesture is surely started on an all day cell or a time cell
                  var allDay = !isTimeline && s.showAllDay && args.endY < _this._gridContTop;
                  var eventDay = s.type === 'day' && s.size === 1 ? _this._firstDay : cols[dayIndex].date;
                  var eventLength = !isListing && (args.external || isClick) ? _this._stepCell : timeStep * ONE_MIN;
                  var gridTime = _this._getGridTime(eventDay, posX, posY, dayIndex, isClick ? _this._stepCell / ONE_MIN : timeStep);
                  var newStart = !_this._isDailyResolution || allDay || isListing ? (allDay ? eventDay : addTimezone(s, eventDay)) : gridTime;
                  var nextDay = s.resolution === 'year'
                      ? addMonths(newStart, 12, s)
                      : s.resolution === 'month'
                          ? addMonths(newStart, 1, s)
                          : s.resolution === 'week'
                              ? addDays(newStart, s.endDay - s.startDay + 1 + (s.endDay < s.startDay ? 7 : 0))
                              : addDays(newStart, 1);
                  var allDayEnd = s.exclusiveEndDates ? nextDay : new Date(+nextDay - 1);
                  var newEnd = allDay || isListing ? allDayEnd : roundTime(createDate(s, +newStart + eventLength), isClick ? 1 : timeStep);
                  var eventData = s.extendDefaultEvent
                      ? s.extendDefaultEvent({
                          resource: resourceId,
                          slot: slotId,
                          start: newStart,
                      })
                      : UNDEFINED;
                  var newEvent = __assign({ allDay: allDay, end: newEnd, id: getEventId(), resource: resource && resourceId !== DEF_ID ? resourceId : UNDEFINED, slot: slot && slotId !== DEF_ID ? slotId : UNDEFINED, start: newStart, title: s.newEventText }, eventData, args.event);
                  var ev = _this._getEventData(newEvent, eventDay, resource);
                  if (isTimeline && resourceId !== UNDEFINED && _this._setRowHeight) {
                      ev.position.top = constrain(floor((posY - resourceTops[resourceId]) / _this._eventHeight), 0, _this._eventRows[resourceId] - 1);
                  }
                  if (args.event) {
                      var eventDuration = +ev.endDate - +ev.startDate;
                      ev.startDate = eventDay;
                      ev.endDate = new Date(+eventDay + eventDuration);
                  }
                  _this._tempEvent = ev;
                  _this._tempResource = resourceId;
                  _this._tempSlot = slotId;
              }
              if (!isClick) {
                  _this._hook('onEventDragStart', {
                      action: args.create ? 'create' : args.resize ? 'resize' : 'move',
                      domEvent: args.domEvent,
                      event: (args.create ? _this._tempEvent : args.event).original,
                      resource: resourceId !== DEF_ID ? resourceId : UNDEFINED,
                      slot: slotId !== DEF_ID ? slotId : UNDEFINED,
                      source: isTimeline ? 'timeline' : 'schedule',
                  });
              }
              return true;
          };
          // tslint:disable-next-line: variable-name
          _this._onEventDragMove = function (args) {
              clearTimeout(_this._scrollTimer);
              var s = _this.s;
              var rtl = s.rtl;
              var rtlNr = rtl ? -1 : 1;
              var isTimeline = _this._isTimeline;
              var isListing = s.eventList;
              var isMonthYearResolution = s.resolution === 'month' || s.resolution === 'year';
              var cols = isListing ? _this._cols : _this._days;
              var colWidth = _this._colWidth;
              var colsNr = cols.length;
              var slots = _this._slots;
              var groupByResource = _this._groupByResource;
              var resources = _this._visibleResources;
              var dragData = _this.state.dragData;
              var timeStep = s.dragTimeStep;
              var timeFormat = s.timeFormat;
              // Limit coordinates to the droppable area
              var startX = args.startX;
              var endX = constrain(args.endX, _this._gridContLeft, _this._gridContRight - 1);
              var endY = constrain(args.endY, _this._gridContTop, _this._gridContBottom - 1);
              var deltaY = endY - args.startY + _this._scrollY;
              var deltaX = rtl ? startX - endX + _this._scrollX : endX - startX + _this._scrollX;
              var delta = isTimeline ? deltaX : deltaY;
              var daySize = isTimeline ? colWidth : _this._colHeight - 16; // Extract 16 to compensate for top/bottom spacing
              var gridWidth = _this._gridRight - _this._gridLeft - 1;
              var startPosY = constrain(args.startY - _this._gridTop, 8, _this._colHeight - 9); // There's 8px top and 8px bottom spacing
              var posX = constrain(rtl ? _this._gridRight + _this._scrollX - endX : endX - _this._gridLeft + _this._scrollX, 0, gridWidth);
              var posY = constrain(endY - _this._gridTop + _this._scrollY, 8, _this._colHeight - 9); // There's 8px top and 8px bottom spacing
              var oldIndex = floor((rtl ? _this._gridRight - startX : startX - _this._gridLeft) / colWidth);
              var newIndex = floor(posX / colWidth);
              var inAllDay = s.showAllDay && args.endY < _this._gridContTop;
              var scrollCont = _this._scrollCont;
              var hasResY = _this._hasResY;
              var event = args.create ? _this._tempEvent : args.event;
              var draggedEvent = __assign({}, event);
              var oldDayIndex = oldIndex;
              var newDayIndex = newIndex;
              var resourceIndex = 0;
              var slotIndex = 0;
              var hasScroll = false;
              var distBottom = _this._gridContBottom - args.endY;
              var distTop = args.endY - _this._gridContTop;
              var distLeft = args.endX - _this._gridContLeft;
              var distRight = _this._gridContRight - args.endX;
              var maxScrollH = (scrollCont.scrollWidth - scrollCont.clientWidth) * rtlNr;
              var rightLimit = rtl ? 0 : maxScrollH;
              var leftLimit = rtl ? maxScrollH : 0;
              if (s.externalDrag && args.drag && !args.create) {
                  dragObservable.next(__assign({}, args, { clone: _this._clone, create: true, event: args.event.original, eventName: 'onDragMove', external: true, from: _this }));
                  if (!_this._onCalendar) {
                      moveClone(args, _this._clone);
                      if (!dragData) {
                          // In case of instant drag the dragged event is not set
                          _this.setState({ dragData: { draggedEvent: draggedEvent } });
                      }
                      return;
                  }
              }
              // Vertical scroll
              if (distBottom < 30 && scrollCont.scrollTop < scrollCont.scrollHeight - scrollCont.clientHeight) {
                  scrollCont.scrollTop += 5;
                  _this._scrollY += 5;
                  hasScroll = true;
              }
              if (distTop < 30 && !inAllDay && scrollCont.scrollTop > 0) {
                  scrollCont.scrollTop -= 5;
                  _this._scrollY -= 5;
                  hasScroll = true;
              }
              // Horizontal scroll
              if (distLeft < 30 && scrollCont.scrollLeft > leftLimit) {
                  scrollCont.scrollLeft -= 5;
                  _this._scrollX -= 5 * rtlNr;
                  hasScroll = true;
              }
              if (distRight < 30 && scrollCont.scrollLeft < rightLimit) {
                  scrollCont.scrollLeft += 5;
                  _this._scrollX += 5 * rtlNr;
                  hasScroll = true;
              }
              if (hasScroll) {
                  _this._scrollTimer = setTimeout(function () {
                      _this._onEventDragMove(args);
                  }, 20);
              }
              if (!isTimeline) {
                  var groupCount = groupByResource ? colsNr : _this._resources.length;
                  oldDayIndex = groupByResource ? oldIndex % groupCount : floor(oldIndex / groupCount);
                  newDayIndex = groupByResource ? newIndex % groupCount : floor(newIndex / groupCount);
                  resourceIndex = groupByResource ? floor(newIndex / groupCount) : newIndex % groupCount;
              }
              else {
                  slotIndex = floor(posX / (colWidth / slots.length)) % slots.length;
                  if (hasResY) {
                      cols.forEach(function (d, i) {
                          resources.forEach(function (r, j) {
                              if (startPosY > _this._resourceTops[d.dateKey + '-' + r.id]) {
                                  oldDayIndex = i;
                              }
                              if (posY > _this._resourceTops[d.dateKey + '-' + r.id]) {
                                  newDayIndex = i;
                                  resourceIndex = j;
                              }
                          });
                      });
                  }
                  else {
                      resources.forEach(function (r, i) {
                          if (posY > _this._resourceTops[r.id]) {
                              resourceIndex = i;
                          }
                      });
                  }
              }
              oldDayIndex = constrain(oldDayIndex, 0, colsNr - 1);
              newDayIndex = constrain(newDayIndex, 0, colsNr - 1);
              var start = event.startDate;
              var end = event.endDate;
              var duration = +end - +start;
              var ms = _this._time;
              var timeDelta = floor((ms * delta) / daySize);
              var slotId = slots[slotIndex].id;
              var resource = resources[resourceIndex];
              var startResource = args.create ? _this._tempResource : args.resource;
              // On external drag don't create the event if dragged on a resource with event creation disabled
              if (resource.eventCreation === false && _this._tempResource === UNDEFINED) {
                  return false;
              }
              var resourceId = resource.eventCreation !== false ? resource.id : _this._tempResource;
              var allDay = event.allDay;
              var tzOpt = allDay ? UNDEFINED : s;
              var addDayOnly = allDay || isListing;
              var newStart = start;
              var newEnd = end;
              var newDate;
              var isEventDraggableBetweenResources = true;
              var isEventDraggableInTime = true;
              var oldDay = cols[oldDayIndex].date;
              var newDay = cols[newDayIndex].date;
              var dayDelta = s.type === 'day' && s.size === 1 ? 0 : getDayDiff(oldDay, newDay);
              var colDelta = newDayIndex - oldDayIndex;
              var months = s.resolution === 'year' ? 12 : 1;
              var deltaDiff = dayDelta - colDelta;
              if (args.drag && !args.create) {
                  isEventDraggableBetweenResources = computeEventDragBetweenResources(event.original.dragBetweenResources, _this._resourcesMap[startResource].eventDragBetweenResources, s.dragBetweenResources);
                  isEventDraggableInTime = computeEventDragInTime(event.original.dragInTime, _this._resourcesMap[startResource].eventDragInTime, s.dragInTime);
              }
              if (args.drag || args.external) {
                  if (!isTimeline && !isEventDraggableBetweenResources && startResource !== resourceId) {
                      // preserve the previous dayDelta if it is moved out from the resource group
                      dayDelta = _this._dragDayDelta;
                  }
                  // Drag
                  if (isTimeline && isListing && isMonthYearResolution) {
                      newStart = addMonths(start, colDelta * months, s);
                      newEnd = addMonths(end, colDelta * months, s);
                  }
                  else {
                      // Only allow changing between all-day / not all-day in case of drag (not resize or create)
                      allDay = inAllDay || (isTimeline && event.allDay);
                      addDayOnly = allDay || isListing;
                      tzOpt = allDay ? UNDEFINED : s;
                      if ((!isTimeline && !inAllDay && (event.allDay || args.external)) || (isTimeline && args.external && !event.allDay)) {
                          var day = getDateOnly(addDays(start, dayDelta));
                          newStart = _this._getGridTime(day, posX, posY, newDayIndex, timeStep);
                      }
                      else {
                          if (isTimeline && !addDayOnly && !hasResY) {
                              newStart = roundTime(createDate(s, +start + timeDelta + (ONE_DAY - ms) * dayDelta + ms * deltaDiff), timeStep);
                          }
                          else {
                              newDate = addDays(start, dayDelta);
                              newStart = addDayOnly ? newDate : roundTime(createDate(tzOpt, +newDate + timeDelta), timeStep);
                          }
                      }
                      if (resource.eventCreation === false && !isTimeline) {
                          newStart = createDate(s, _this._tempStart);
                      }
                      // if (end.getMilliseconds() === 999) {
                      //   // TODO: this should be removed when non-inclusive end dates are implemented
                      //   duration += 1;
                      // }
                      newEnd = createDate(tzOpt, +newStart + duration);
                  }
              }
              else {
                  // Resize, create
                  var gridDelta = isTimeline ? colDelta : newIndex - oldIndex;
                  var endResize = args.create ? (gridDelta ? gridDelta > 0 : delta > 0) : args.direction === 'end';
                  var days = getDayDiff(start, end);
                  if (!isTimeline && groupByResource && startResource !== resourceId) {
                      // preserve the previous dayDelta if it is moved out from the resource group
                      dayDelta = _this._dragDayDelta;
                  }
                  if (endResize) {
                      if (isTimeline && isListing && isMonthYearResolution) {
                          newEnd = addMonths(end, colDelta * months, s);
                      }
                      else if (isTimeline && !addDayOnly && !hasResY) {
                          newEnd = roundTime(createDate(s, +end + timeDelta + dayDelta * (ONE_DAY - ms) + ms * deltaDiff), timeStep);
                      }
                      else {
                          newDate = addDays(end, Math.max(-days, dayDelta));
                          newEnd = addDayOnly ? newDate : roundTime(createDate(tzOpt, +newDate + timeDelta), timeStep);
                          // Ensure that end time remains between visible hours
                          // TODO: this should be simpler
                          if (!addDayOnly && (getDayMilliseconds(newEnd) > _this._endTime + 1 || newEnd >= addDays(getDateOnly(newDate), 1))) {
                              newEnd = createDate(s, +getDateOnly(newDate) + _this._endTime + 1);
                          }
                      }
                  }
                  else {
                      if (isTimeline && isListing && isMonthYearResolution) {
                          newStart = addMonths(start, colDelta * months, s);
                      }
                      else if (isTimeline && !addDayOnly && !hasResY) {
                          newStart = roundTime(createDate(s, +start + timeDelta + dayDelta * (ONE_DAY - ms) + ms * deltaDiff), timeStep);
                      }
                      else {
                          newDate = addDays(start, Math.min(days, dayDelta));
                          newStart = addDayOnly ? newDate : roundTime(createDate(tzOpt, +newDate + timeDelta), timeStep);
                          // Ensure that start time remains between visible hours
                          // TODO: this should be simpler
                          if (!addDayOnly && (getDayMilliseconds(newStart) < _this._startTime || newStart < getDateOnly(newDate))) {
                              newStart = createDate(s, +getDateOnly(newDate) + _this._startTime);
                          }
                      }
                  }
                  resourceId = startResource; // set the resource back to the starting resource
                  // Don't allow end date before start date when resizing all day events
                  if (addDayOnly && newEnd < newStart) {
                      if (endResize) {
                          newEnd = createDate(s, newStart);
                      }
                      else {
                          newStart = createDate(s, newEnd);
                      }
                  }
                  // Let's have dragTimeStep minutes minimum duration
                  if (!addDayOnly && (newEnd < newStart || Math.abs(+newEnd - +newStart) < timeStep * ONE_MIN)) {
                      if (endResize) {
                          newEnd = createDate(s, +newStart + timeStep * ONE_MIN);
                      }
                      else {
                          newStart = createDate(s, +newEnd - timeStep * ONE_MIN);
                      }
                  }
              }
              if (args.drag) {
                  // Check if event not movable in time -
                  if (!isEventDraggableInTime) {
                      newStart = start;
                      newEnd = end;
                      allDay = _this._tempAllDay;
                  }
                  // Check if event not movable between resources
                  if (!isEventDraggableBetweenResources) {
                      resourceId = startResource;
                  }
              }
              // Check if dates changed since last move
              if (_this._tempStart !== +newStart ||
                  _this._tempEnd !== +newEnd ||
                  _this._tempAllDay !== allDay ||
                  _this._tempResource !== resourceId ||
                  _this._tempSlot !== slotId) {
                  var startStr = void 0;
                  var endStr = void 0;
                  if (!_this._isDailyResolution) {
                      startStr = formatDate(s.dateFormat, newStart, s);
                      endStr = formatDate(s.dateFormat, getEndDate(s, allDay, newStart, newEnd), s);
                  }
                  else {
                      startStr = formatDate(timeFormat, newStart, s);
                      endStr = formatDate(timeFormat, newEnd, s);
                  }
                  // Modify the dates
                  draggedEvent.startDate = newStart;
                  draggedEvent.endDate = newEnd;
                  draggedEvent.start = startStr;
                  draggedEvent.end = endStr;
                  draggedEvent.allDay = allDay;
                  draggedEvent.date = +newDay;
                  _this._tempStart = +newStart;
                  _this._tempEnd = +newEnd;
                  _this._tempAllDay = allDay;
                  _this._tempResource = resourceId;
                  _this._tempSlot = slotId;
                  _this._dragDelta = args.drag || args.external ? -1 : args.direction ? (args.direction === 'end' ? 1 : -1) : delta;
                  _this._dragDayDelta = dayDelta;
                  // Call mouse move to display the time during drag
                  if (!allDay) {
                      _this._onMouseMove(args.domEvent);
                  }
                  _this.setState({
                      dragData: {
                          draggedDates: _this._getDragDates(draggedEvent, resourceId, slotId),
                          draggedEvent: draggedEvent,
                          originDate: event.date,
                          originDates: dragData && dragData.originDates,
                          originResource: args.external ? UNDEFINED : startResource,
                          resource: resourceId,
                          slot: slotId,
                      },
                  });
              }
              return true;
          };
          // tslint:disable-next-line: variable-name
          _this._onEventDragEnd = function (args) {
              clearTimeout(_this._scrollTimer);
              var s = _this.s;
              var isCreating = args.create;
              var state = _this.state;
              var dragData = state.dragData;
              var eventLeft = false;
              if (s.externalDrag && args.drag && !args.create) {
                  dragObservable.next(__assign({}, args, { action: 'externalDrop', create: true, event: args.event.original, eventName: 'onDragEnd', external: true, from: _this }));
                  _this._body.removeChild(_this._clone);
                  if (!_this._onCalendar) {
                      eventLeft = true;
                      if (_this._eventDropped) {
                          s.onEventDelete(args);
                      }
                  }
              }
              if (isCreating && !dragData) {
                  // if there was no drag move create dummy object for create on click to work
                  dragData = {};
                  dragData.draggedEvent = _this._tempEvent;
              }
              if (dragData && dragData.draggedEvent) {
                  var event_1 = args.event;
                  var draggedEvent = dragData.draggedEvent;
                  var newStart = draggedEvent.startDate;
                  var newEnd = draggedEvent.endDate;
                  var allDay = draggedEvent.allDay;
                  var origEvent = draggedEvent.original;
                  var oldResource = args.resource;
                  var newResource_1 = dragData.resource === UNDEFINED ? oldResource : dragData.resource;
                  var eventResource = origEvent.resource === UNDEFINED ? newResource_1 : origEvent.resource;
                  var oldSlot = isCreating ? _this._tempSlot : args.slot;
                  var newSlot = dragData.slot === UNDEFINED ? oldSlot : dragData.slot;
                  var invalids = {};
                  var source = _this._isTimeline ? 'timeline' : 'schedule';
                  var changed = isCreating ||
                      +newStart !== +event_1.startDate ||
                      +newEnd !== +event_1.endDate ||
                      allDay !== event_1.allDay ||
                      oldResource !== newResource_1 ||
                      oldSlot !== newSlot;
                  var updatedResource = eventResource;
                  var invalidResources = void 0;
                  if (oldResource !== newResource_1 && (!isCreating || args.external) && !_this._isSingleResource) {
                      if (isArray(eventResource) && eventResource.length && newResource_1) {
                          var indx = eventResource.indexOf(oldResource);
                          if (eventResource.indexOf(newResource_1) === -1) {
                              // Don't allow to two resource combine
                              updatedResource = eventResource.slice();
                              updatedResource.splice(indx, 1, newResource_1);
                          }
                      }
                      else {
                          updatedResource = newResource_1;
                      }
                  }
                  if (!updatedResource || !s.resources) {
                      // if the event is not tied to a resource, process all invalids
                      invalidResources = _this._resources.map(function (r) { return r.id; });
                  }
                  else {
                      invalidResources = isArray(updatedResource) ? updatedResource : [updatedResource];
                  }
                  for (var _i = 0, invalidResources_1 = invalidResources; _i < invalidResources_1.length; _i++) {
                      var r = invalidResources_1[_i];
                      if (_this._invalids[r]) {
                          invalids[r] = _this._invalids[r][newSlot];
                      }
                  }
                  var action = args.action || (state.dragData ? 'drag' : 'click');
                  var allowUpdate = !eventLeft &&
                      (changed
                          ? s.eventDragEnd({
                              action: action,
                              collision: checkInvalidCollision$1(invalids, newStart, newEnd, allDay, s.invalidateEvent, s.exclusiveEndDates),
                              create: isCreating,
                              domEvent: args.domEvent,
                              event: draggedEvent,
                              from: args.from,
                              resource: updatedResource !== DEF_ID ? updatedResource : UNDEFINED,
                              slot: newSlot !== DEF_ID ? newSlot : UNDEFINED,
                              source: source,
                          })
                          : true);
                  var keepDragMode = state.isTouchDrag && !eventLeft && (!isCreating || allowUpdate);
                  if (allowUpdate && keepDragMode && oldResource !== newResource_1 && !origEvent.color) {
                      var newRes = find(_this._resources, function (r) { return r.id === newResource_1; });
                      var resColor = newRes && newRes.color;
                      // update drag mode event color manually
                      if (resColor) {
                          draggedEvent.color = resColor;
                          draggedEvent.style.background = resColor;
                          draggedEvent.style.color = getTextColor(resColor);
                      }
                      else {
                          draggedEvent.color = UNDEFINED;
                          draggedEvent.style = {};
                      }
                  }
                  if (!keepDragMode && action !== 'click') {
                      _this._hook('onEventDragEnd', {
                          domEvent: args.domEvent,
                          event: (isCreating ? _this._tempEvent : args.event).original,
                          resource: newResource_1 !== DEF_ID ? newResource_1 : UNDEFINED,
                          slot: newSlot !== DEF_ID ? newSlot : UNDEFINED,
                          source: source,
                      });
                  }
                  _this.setState({
                      dragData: keepDragMode
                          ? {
                              draggedEvent: allowUpdate ? draggedEvent : __assign({}, event_1),
                              originDate: allowUpdate ? draggedEvent.date : event_1.date,
                              originDates: allowUpdate ? _this._getDragDates(draggedEvent, newResource_1, newSlot) : dragData.originDates,
                              originResource: allowUpdate ? newResource_1 : dragData.originResource,
                          }
                          : UNDEFINED,
                      isTouchDrag: keepDragMode,
                  });
                  _this._tempStart = 0;
                  _this._tempEnd = 0;
                  _this._tempAllDay = UNDEFINED;
                  _this._dragDelta = UNDEFINED;
                  _this._onMouseMove(args.domEvent);
                  _this._isTouch = false;
              }
          };
          // tslint:disable-next-line: variable-name
          _this._onExternalDrag = function (args) {
              var s = _this.s;
              var clone = args.clone;
              var isSelf = args.from === _this;
              var externalDrop = !isSelf && s.externalDrop;
              var instantDrag = isSelf && s.externalDrag && !s.dragToMove;
              var dragData = _this.state.dragData;
              if (externalDrop || s.externalDrag) {
                  var isInArea = !instantDrag &&
                      args.endY < _this._gridContBottom &&
                      args.endY > _this._allDayTop &&
                      args.endX > _this._gridContLeft &&
                      args.endX < _this._gridContRight;
                  switch (args.eventName) {
                      case 'onDragModeOff':
                          if (externalDrop) {
                              _this._onEventDragModeOff(args);
                          }
                          break;
                      case 'onDragModeOn':
                          if (externalDrop) {
                              _this._onEventDragModeOn(args);
                          }
                          break;
                      case 'onDragStart':
                          if (externalDrop) {
                              _this._onEventDragStart(args);
                          }
                          else if (isSelf) {
                              _this._onCalendar = true;
                          }
                          break;
                      case 'onDragMove':
                          if (!isSelf && !externalDrop) {
                              return;
                          }
                          if (isInArea) {
                              if (!_this._onCalendar) {
                                  _this._hook('onEventDragEnter', {
                                      domEvent: args.domEvent,
                                      event: args.event,
                                      source: _this._isTimeline ? 'timeline' : 'schedule',
                                  });
                              }
                              if (isSelf || (externalDrop && _this._onEventDragMove(args) !== false)) {
                                  clone.style.display = 'none';
                              }
                              _this._onCalendar = true;
                          }
                          else {
                              if (_this._onCalendar) {
                                  _this._hook('onEventDragLeave', {
                                      domEvent: args.domEvent,
                                      event: args.event,
                                      source: _this._isTimeline ? 'timeline' : 'schedule',
                                  });
                                  clearTimeout(_this._scrollTimer);
                                  clone.style.display = 'table';
                                  if (!isSelf || dragData) {
                                      _this.setState({
                                          dragData: {
                                              draggedDates: {},
                                              draggedEvent: isSelf ? dragData && dragData.draggedEvent : UNDEFINED,
                                              originDates: isSelf ? dragData && dragData.originDates : UNDEFINED,
                                          },
                                      });
                                  }
                                  _this._tempStart = 0;
                                  _this._tempEnd = 0;
                                  _this._tempAllDay = UNDEFINED;
                                  _this._tempResource = UNDEFINED;
                                  _this._dragDelta = UNDEFINED;
                                  _this._onCalendar = false;
                                  _this._onMouseLeave(UNDEFINED, true);
                              }
                          }
                          break;
                      case 'onDragEnd':
                          if (externalDrop) {
                              // This is needed, otherwise it creates event on drag click,
                              // also, the temp resource might be undefined if dragged over a resource with event creation disabled
                              if (isInArea && _this._tempResource !== UNDEFINED) {
                                  _this._onEventDragEnd(args);
                              }
                              else {
                                  _this.setState({
                                      dragData: UNDEFINED,
                                      isTouchDrag: false,
                                  });
                                  _this._hook('onEventDragEnd', {
                                      domEvent: args.domEvent,
                                      event: args.event,
                                      resource: args.resource,
                                      slot: args.slot,
                                      source: args.source,
                                  });
                              }
                          }
                          break;
                  }
              }
          };
          return _this;
      }
      // tslint:enable variable-name
      STBase.prototype._isToday = function (d) {
          return isSameDay(new Date(d), createDate(this.s));
      };
      STBase.prototype._formatTime = function (v, timezone) {
          var s = this.s;
          var format = s.timeFormat;
          var timeFormat = /a/i.test(format) && this._stepLabel === ONE_HOUR && v % ONE_HOUR === 0 ? format.replace(/.[m]+/i, '') : format;
          var d = new Date(+REF_DATE + v);
          var dd = createDate(s, d.getFullYear(), d.getMonth(), d.getDate(), d.getHours(), d.getMinutes());
          if (isMBSCDate(dd) && timezone) {
              dd.setTimezone(timezone);
          }
          return formatDate(timeFormat, dd, s);
      };
      // #endregion Drag & Drop
      STBase.prototype._getEventPos = function (event, day, dateKey, displayedMap) {
          var s = this.s;
          var tzOpt = event.allDay ? UNDEFINED : s;
          var d = createDate(tzOpt, day.getFullYear(), day.getMonth(), day.getDate());
          var nextDay = getDateOnly(addDays(d, 1));
          var firstDay = tzOpt ? this._firstDayTz : this._firstDay;
          var lastDay = tzOpt ? this._lastDayTz : this._lastDay;
          var isTimeline = this._isTimeline;
          var groupByDate = !isTimeline && !this._groupByResource;
          var isAllDay = event.allDay;
          var startTime = this._startTime;
          var endTime = this._endTime + 1;
          var displayedTime = this._time;
          var hasSlots = this._hasSlots;
          var hasResY = this._hasResY;
          var isDailyResolution = this._isDailyResolution;
          var isListing = s.eventList;
          var dayIndex = hasResY ? 0 : this._dayIndexMap[dateKey];
          var start = event.start;
          var end = event.end;
          var startDate = getEventLayoutStart(event, s, isListing, isTimeline, isDailyResolution, firstDay, this._cols, this._colIndexMap);
          var endDate = getEventLayoutEnd(event, s, isListing, isTimeline, isDailyResolution, lastDay, this._cols, this._colIndexMap);
          // Increase endDate by 1ms if start equals with end, to make sure we display
          // 0 length events at the beginning of displayed time range
          var adjust = +startDate === +endDate ? 1 : 0;
          if (!(isAllDay || isTimeline) || (hasResY && !hasSlots)) {
              if (startDate < d) {
                  start = '';
                  startDate = createDate(s, d);
              }
              if (endDate >= nextDay) {
                  end = '';
                  endDate = createDate(s, +nextDay - 1);
              }
              if (endDate >= nextDay) {
                  endDate = createDate(s, +nextDay - 1);
              }
          }
          if (isAllDay || isTimeline) {
              if (!displayedMap.get(event.original) || hasSlots || hasResY || groupByDate) {
                  var startDay = s.startDay;
                  var endDay = s.endDay;
                  var isFullDay = isAllDay || isListing;
                  var isMultiDay = !isSameDay(startDate, endDate);
                  var daysNr = this._daysNr;
                  if (isTimeline && isMultiDay && getDayMilliseconds(startDate) >= endTime) {
                      startDate = createDate(s, +getDateOnly(startDate) + endTime);
                  }
                  var leftPos = getEventStart(startDate, startTime, displayedTime, firstDay, startDay, endDay);
                  var width = getEventWidth(startDate, endDate, displayedTime, firstDay, lastDay, startTime, endTime, startDay, endDay, isFullDay);
                  if (isTimeline) {
                      var diff = 0;
                      if (isListing && !isDailyResolution) {
                          dayIndex = this._dayIndexMap[getDateStr(startDate)];
                      }
                      if (s.resolution === 'month') {
                          var startDayDiff = this._days[dayIndex].dayDiff;
                          var endKey = getDateStr(endDate >= lastDay ? addDays(lastDay, -1) : endDate);
                          var endIndex = this._dayIndexMap[endKey];
                          var endDayDiff = this._days[endIndex].dayDiff;
                          diff = endDayDiff - startDayDiff;
                      }
                      width = (width + diff * 100) / daysNr;
                      leftPos = (leftPos + dayIndex * 100) / daysNr;
                  }
                  var position = isTimeline
                      ? isFullDay
                          ? {
                              left: s.rtl ? '' : (hasSlots ? '' : (dayIndex * 100) / daysNr) + '%',
                              right: s.rtl ? (hasSlots ? '' : (dayIndex * 100) / daysNr) + '%' : '',
                              width: (hasSlots ? '' : width) + '%',
                          }
                          : {
                              height: this._setRowHeight ? '' : '100%',
                              left: s.rtl ? '' : leftPos + '%',
                              right: s.rtl ? leftPos + '%' : '',
                              top: '0',
                              width: width + '%',
                          }
                      : {
                          width: (isMultiDay && !groupByDate ? width : 100) + '%',
                      };
                  var isStartInView = getDayMilliseconds(startDate) < endTime && endDate > firstDay;
                  var isEndInView = getDayMilliseconds(endDate) + adjust > startTime;
                  // Skip events not in view
                  if (isFullDay || (isMultiDay && width > 0) || (isStartInView && isEndInView)) {
                      displayedMap.set(event.original, true);
                      return {
                          end: end,
                          endDate: endDate,
                          position: position,
                          start: start,
                          startDate: startDate,
                      };
                  }
              }
          }
          else {
              // Skip events not in view
              if (getDayMilliseconds(startDate) < endTime && getDayMilliseconds(endDate) + adjust > startTime && endDate >= startDate) {
                  // Need to use the original (inclusive) end date for proper height   on DST day
                  var eventHeight = getEventHeight(startDate, endDate, displayedTime, startTime, endTime);
                  return {
                      cssClass: eventHeight < 2 ? ' mbsc-schedule-event-small-height' : '',
                      end: end,
                      endDate: endDate,
                      position: {
                          height: eventHeight + '%',
                          top: getEventStart(startDate, startTime, displayedTime) + '%',
                          width: '100%',
                      },
                      start: start,
                      startDate: startDate,
                  };
              }
          }
          return UNDEFINED;
      };
      STBase.prototype._getEventData = function (event, d, resource, skipLabels) {
          var s = this.s;
          var ev = getEventData(s, event, d, true, resource, false, !this._isTimeline || this._hasResY, this._isDailyResolution, skipLabels);
          if (event.allDay && s.exclusiveEndDates && +ev.endDate === +ev.startDate) {
              ev.endDate = getDateOnly(addDays(ev.startDate, 1));
          }
          return ev;
      };
      STBase.prototype._getEvents = function (eventMap) {
          var _this = this;
          var s = this.s;
          var resources = this._resources;
          var slots = this._slots;
          var hasSlots = this._hasSlots;
          var hasResY = this._hasResY;
          var isTimeline = this._isTimeline;
          var isSchedule = !isTimeline;
          var events = {};
          var eventMaps = getResourceMap(eventMap, resources, slots, !!s.resources, !!s.slots);
          var eventLabels = {};
          var firstDay = this._firstDay;
          var lastDay = this._lastDay;
          var variableRow = this._setRowHeight;
          var connectionMap = {};
          var cols = this._cols;
          var createEventMaps = this._createEventMaps ||
              s.renderHour ||
              s.renderHourFooter ||
              s.renderDay ||
              s.renderDayFooter ||
              s.renderWeek ||
              s.renderWeekFooter ||
              s.renderMonth ||
              s.renderMonthFooter ||
              s.renderYear ||
              s.renderYearFooter;
          if (createEventMaps) {
              // reset event list calculated for columns
              cols.forEach(function (c) { return (c.eventMap = {}); });
          }
          if (s.connections) {
              for (var _i = 0, _a = s.connections; _i < _a.length; _i++) {
                  var c = _a[_i];
                  connectionMap[c.from] = true;
                  connectionMap[c.to] = true;
              }
          }
          var _loop_1 = function (resource) {
              var resourceId = resource.id;
              var eventDisplayMap = new Map();
              var eventsForRange = [];
              var eventRows = 0;
              events[resourceId] = {};
              var _loop_2 = function (slot) {
                  var slotId = slot.id;
                  var eventsForSlot = eventMaps[resourceId][slotId];
                  var eventKeys = Object.keys(eventsForSlot).sort();
                  events[resourceId][slotId] = { all: { allDay: [], events: [] } };
                  if (isSchedule) {
                      eventLabels[slotId] = getLabels(s, eventsForSlot, firstDay, lastDay, -1, this_1._daysNr, true, s.startDay, false, s.eventOrder);
                  }
                  var _loop_4 = function (dateKey) {
                      // The date object is stored on the array for performance reasons, so we don't have to parse it all over again
                      // TODO: do this with proper types
                      var d = eventMap[dateKey].date;
                      if (this_1._dayIndexMap[dateKey] !== UNDEFINED && isInWeek(d.getDay(), s.startDay, s.endDay)) {
                          var eventsForDay = sortEvents(eventsForSlot[dateKey]) || [];
                          var groups = [];
                          var next = {};
                          var key_1 = !hasResY && !hasSlots && isTimeline ? 'all' : dateKey;
                          var eventNr = 0;
                          if (isSchedule || hasSlots || hasResY) {
                              events[resourceId][slotId][key_1] = { allDay: [], events: [] };
                          }
                          if (hasResY) {
                              eventRows = this_1._eventRows[dateKey + '-' + resourceId] || 0;
                          }
                          for (var _i = 0, eventsForDay_1 = eventsForDay; _i < eventsForDay_1.length; _i++) {
                              var ev = eventsForDay_1[_i];
                              if (!ev.allDay || isTimeline) {
                                  var event_2 = this_1._getEventData(ev, d, resource);
                                  var pos = this_1._getEventPos(event_2, d, dateKey, eventDisplayMap);
                                  if (pos) {
                                      event_2.cssClass = pos.cssClass;
                                      event_2.position = pos.position;
                                      if (isSchedule || hasResY) {
                                          event_2.showText = true;
                                          calcLayout(s, groups, event_2, next, s.eventList);
                                      }
                                      events[resourceId][slotId][key_1].events.push(event_2);
                                      eventsForRange.push(event_2);
                                      eventNr++;
                                      this_1._eventMap[event_2.id] = event_2;
                                      if (createEventMaps) {
                                          var timeStep = this_1._stepCell;
                                          var isHoursResolution = this_1._isDailyResolution && timeStep < 1440 * ONE_MIN;
                                          var firstDayTz = ev.allDay ? firstDay : addTimezone(s, firstDay);
                                          var first = event_2.startDate > firstDayTz ? event_2.startDate : firstDayTz;
                                          var colIndex = this_1._colIndexMap[getDateStr(first)];
                                          var overlap = true;
                                          while (overlap && colIndex < cols.length) {
                                              var col = cols[colIndex];
                                              var dtStart = col.date;
                                              var dtEnd = colIndex < cols.length - 1 ? cols[colIndex + 1].date : lastDay;
                                              var start = dtStart;
                                              while (start < dtEnd) {
                                                  var ts = +start;
                                                  var end = isHoursResolution ? new Date(ts + timeStep) : dtEnd;
                                                  var colStart = ev.allDay ? dtStart : addTimezone(s, start);
                                                  var colEnd = ev.allDay ? dtEnd : addTimezone(s, end);
                                                  if (checkDateRangeOverlap(event_2.startDate, event_2.endDate, colStart, colEnd)) {
                                                      if (!col.eventMap[ts]) {
                                                          col.eventMap[ts] = [];
                                                      }
                                                      col.eventMap[ts].push(event_2.original);
                                                      overlap = true;
                                                  }
                                                  else {
                                                      overlap = false;
                                                  }
                                                  start = end;
                                              }
                                              colIndex++;
                                          }
                                      }
                                  }
                              }
                          }
                          if (hasSlots && eventNr > eventRows) {
                              eventRows = eventNr;
                          }
                          if (isSchedule || (hasResY && !hasSlots)) {
                              // All day events
                              if (isSchedule && eventLabels[slotId][dateKey]) {
                                  eventLabels[slotId][dateKey].data.forEach(function (_a) {
                                      var event = _a.event, width = _a.width;
                                      if (event) {
                                          var ev = _this._getEventData(event, d, resource);
                                          var pos = _this._getEventPos(ev, d, dateKey, eventDisplayMap);
                                          ev.position = { width: pos ? pos.position.width : width };
                                          ev.showText = !!pos;
                                          events[resourceId][slotId][key_1].allDay.push(ev);
                                      }
                                  });
                              }
                              // Set the width and left of the non all-day events, based on the final layout
                              for (var _a = 0, groups_2 = groups; _a < groups_2.length; _a++) {
                                  var group = groups_2[_a];
                                  var nr = group.length;
                                  if (variableRow && nr > eventRows) {
                                      eventRows = nr;
                                  }
                                  for (var i = 0; i < nr; i++) {
                                      for (var _b = 0, _c = group[i]; _b < _c.length; _b++) {
                                          var event_3 = _c[_b];
                                          var dimension = (((next[event_3.uid] || nr) - i) / nr) * 100;
                                          if (isSchedule) {
                                              event_3.position.width = dimension + '%';
                                              event_3.position[s.rtl ? 'right' : 'left'] = (i * 100) / nr + '%';
                                              event_3.position[s.rtl ? 'left' : 'right'] = 'auto';
                                          }
                                          else {
                                              event_3.position.height = variableRow ? '' : dimension + '%';
                                              event_3.position.top = variableRow ? i : (i * 100) / nr + '%';
                                          }
                                      }
                                  }
                              }
                          }
                          if (hasResY) {
                              this_1._eventRows[dateKey + '-' + resourceId] = eventRows || 1;
                          }
                      }
                      else if (s.connections) {
                          // Process the events which are part of a connection, but not shown on the view
                          var eventsForDay = eventsForSlot[dateKey] || [];
                          for (var _d = 0, eventsForDay_2 = eventsForDay; _d < eventsForDay_2.length; _d++) {
                              var event_4 = eventsForDay_2[_d];
                              var id = event_4.id;
                              if (!this_1._eventMap[id] && connectionMap[id]) {
                                  this_1._eventMap[id] = this_1._getEventData(event_4, d, resource);
                              }
                          }
                      }
                  };
                  for (var _i = 0, eventKeys_1 = eventKeys; _i < eventKeys_1.length; _i++) {
                      var dateKey = eventKeys_1[_i];
                      _loop_4(dateKey);
                  }
              };
              for (var _i = 0, slots_1 = slots; _i < slots_1.length; _i++) {
                  var slot = slots_1[_i];
                  _loop_2(slot);
              }
              // In case of the timeline, calculate the layout for the whole displayed view, not just per day
              if (isTimeline && !hasSlots && !hasResY) {
                  var groups = [];
                  var next_1 = {};
                  for (var _a = 0, eventsForRange_1 = eventsForRange; _a < eventsForRange_1.length; _a++) {
                      var event_5 = eventsForRange_1[_a];
                      calcLayout(s, groups, event_5, next_1, s.eventList, isTimeline, this_1._isDailyResolution, firstDay, this_1._firstDayTz, lastDay, this_1._lastDayTz, this_1._cols, this_1._colIndexMap);
                  }
                  var _loop_3 = function (group) {
                      var nr = group.length;
                      if (variableRow && nr > eventRows) {
                          eventRows = nr;
                      }
                      group.forEach(function (level, i) {
                          for (var _i = 0, level_1 = level; _i < level_1.length; _i++) {
                              var event_6 = level_1[_i];
                              var dimension = (((next_1[event_6.uid] || nr) - i) / nr) * 100;
                              event_6.position.height = variableRow ? '' : dimension + '%';
                              event_6.position.top = variableRow ? i : (i * 100) / nr + '%';
                          }
                      });
                  };
                  for (var _b = 0, groups_1 = groups; _b < groups_1.length; _b++) {
                      var group = groups_1[_b];
                      _loop_3(group);
                  }
              }
              if (!hasResY) {
                  this_1._eventRows[resourceId] = eventRows || 1; // make sure the min-height will be at least 1 event tall
              }
          };
          var this_1 = this;
          for (var _b = 0, resources_1 = resources; _b < resources_1.length; _b++) {
              var resource = resources_1[_b];
              _loop_1(resource);
          }
          return events;
      };
      STBase.prototype._getInvalids = function (invalidMap) {
          var _a;
          var s = this.s;
          var isListing = s.eventList;
          var map = invalidMap || {};
          var invalids = {};
          var minDate = isListing ? getDateOnly(new Date(s.minDate)) : new Date(s.minDate);
          var maxDate = isListing ? getDateOnly(addDays(new Date(s.maxDate), 1)) : new Date(s.maxDate);
          var isTimeline = this._isTimeline;
          if (s.minDate) {
              for (var d = getDateOnly(this._firstDay); d < minDate; d.setDate(d.getDate() + 1)) {
                  var dateKey = getDateStr(d);
                  var invalidsForDay = map[dateKey] || [];
                  invalidsForDay.push({
                      end: minDate,
                      start: new Date(d),
                  });
                  map[dateKey] = invalidsForDay;
              }
          }
          if (s.maxDate) {
              for (var d = getDateOnly(maxDate); d < this._lastDay; d.setDate(d.getDate() + 1)) {
                  var dateKey = getDateStr(d);
                  var invalidsForDay = map[dateKey] || [];
                  invalidsForDay.push({
                      end: new Date(this._lastDay),
                      start: maxDate,
                  });
                  map[dateKey] = invalidsForDay;
              }
          }
          var invalidMaps = getResourceMap(map, this._resources, this._slots, !!s.resources, !!s.slots);
          var invalidKeys = Object.keys(map).sort();
          for (var _i = 0, _b = this._resources; _i < _b.length; _i++) {
              var resource = _b[_i];
              var resourceId = resource.id;
              var invalidDisplayedMap = new Map();
              invalids[resourceId] = {};
              for (var _c = 0, _d = this._slots; _c < _d.length; _c++) {
                  var slot = _d[_c];
                  var slotId = slot.id;
                  var allInvalids = { invalids: [] };
                  invalids[resourceId][slotId] = { all: allInvalids };
                  for (var _e = 0, invalidKeys_1 = invalidKeys; _e < invalidKeys_1.length; _e++) {
                      var dateKey = invalidKeys_1[_e];
                      var d = makeDate(dateKey);
                      if (this._dayIndexMap[dateKey] !== UNDEFINED && isInWeek(d.getDay(), s.startDay, s.endDay)) {
                          var invalidsForDay = invalidMaps[resourceId][slotId][dateKey] || [];
                          // Contains all invalids for the day
                          var allDailyInvalids = { invalids: [] };
                          // Only contains invalids beginning on the day, for the timeline
                          var dailyInvalids = [];
                          invalids[resourceId][slotId][dateKey] = allDailyInvalids;
                          for (var _f = 0, invalidsForDay_1 = invalidsForDay; _f < invalidsForDay_1.length; _f++) {
                              var invalid = invalidsForDay_1[_f];
                              // if a string or a date object is passed
                              if (isString(invalid) || isDate(invalid)) {
                                  var start = makeDate(invalid);
                                  var end = new Date(start);
                                  invalid = { allDay: true, end: end, start: start };
                              }
                              var invalidData = this._getEventData(invalid, d, resource, true);
                              invalidData.cssClass = invalid.cssClass ? ' ' + invalid.cssClass : '';
                              invalidData.position = UNDEFINED;
                              var pos = this._getEventPos(invalidData, d, dateKey, invalidDisplayedMap);
                              if (pos) {
                                  // If the invalid spans across the whole day, make it invalid
                                  if (!isTimeline && getDayMilliseconds(pos.startDate) === 0 && new Date(+pos.endDate + 1) >= addDays(d, 1)) {
                                      invalidData.allDay = true;
                                  }
                                  else {
                                      invalidData.position = pos.position;
                                      if (getDayMilliseconds(pos.startDate) <= this._startTime) {
                                          invalidData.cssClass += ' mbsc-schedule-invalid-start';
                                      }
                                      if (getDayMilliseconds(pos.endDate) >= this._endTime) {
                                          invalidData.cssClass += ' mbsc-schedule-invalid-end';
                                      }
                                  }
                                  dailyInvalids.push(invalidData);
                              }
                              allDailyInvalids.invalids.push(invalidData);
                              if (invalidData.allDay) {
                                  if (!isTimeline) {
                                      invalidData.position = {};
                                  }
                                  allDailyInvalids.allDay = invalidData;
                                  allDailyInvalids.invalids = [invalidData];
                                  dailyInvalids = [invalidData];
                                  break;
                              }
                          }
                          (_a = allInvalids.invalids).push.apply(_a, dailyInvalids);
                      }
                  }
              }
          }
          return invalids;
      };
      STBase.prototype._getColors = function (colorMap) {
          var s = this.s;
          var colors = {};
          var colorMaps = getResourceMap(colorMap, this._resources, this._slots, !!s.resources, !!s.slots);
          var colorKeys = Object.keys(colorMap || {}).sort();
          var hasSlots = this._hasSlots;
          var isTimeline = this._isTimeline;
          var hasResY = this._hasResY;
          for (var _i = 0, _a = this._resources; _i < _a.length; _i++) {
              var resource = _a[_i];
              var resourceId = resource.id;
              var colorDisplayedMap = new Map();
              colors[resourceId] = {};
              for (var _b = 0, _c = this._slots; _b < _c.length; _b++) {
                  var slot = _c[_b];
                  var slotId = slot.id;
                  colors[resourceId][slotId] = { all: { colors: [] } };
                  for (var _d = 0, colorKeys_1 = colorKeys; _d < colorKeys_1.length; _d++) {
                      var dateKey = colorKeys_1[_d];
                      var d = makeDate(dateKey);
                      if (this._dayIndexMap[dateKey] !== UNDEFINED && isInWeek(d.getDay(), s.startDay, s.endDay)) {
                          var colorsForDay = colorMaps[resourceId][slotId][dateKey] || [];
                          var key = !hasResY && !hasSlots && isTimeline ? 'all' : dateKey;
                          if (!isTimeline || hasSlots || hasResY) {
                              colors[resourceId][slotId][key] = { colors: [] };
                          }
                          var dailyColors = colors[resourceId][slotId][key];
                          for (var _e = 0, colorsForDay_1 = colorsForDay; _e < colorsForDay_1.length; _e++) {
                              var color = colorsForDay_1[_e];
                              var colorData = this._getEventData(color, d, resource, true);
                              colorData.cssClass = color.cssClass ? ' ' + color.cssClass : '';
                              if (colorData.allDay && !isTimeline) {
                                  dailyColors.allDay = colorData;
                              }
                              else {
                                  var pos = this._getEventPos(colorData, d, dateKey, colorDisplayedMap);
                                  if (pos) {
                                      colorData.position = pos.position;
                                      if (getDayMilliseconds(pos.startDate) <= this._startTime) {
                                          colorData.cssClass += ' mbsc-schedule-color-start';
                                      }
                                      if (getDayMilliseconds(pos.endDate) >= this._endTime) {
                                          colorData.cssClass += ' mbsc-schedule-color-end';
                                      }
                                      dailyColors.colors.push(colorData);
                                  }
                              }
                              colorData.position.background = color.background;
                              colorData.position.color = color.textColor ? color.textColor : getTextColor(color.background);
                          }
                      }
                  }
              }
          }
          return colors;
      };
      STBase.prototype._flattenResources = function (resources, flat, depth, all) {
          var res = resources && resources.length ? resources : [{ id: DEF_ID }];
          for (var _i = 0, res_1 = res; _i < res_1.length; _i++) {
              var r = res_1[_i];
              r.depth = depth;
              r.isParent = !!(r.children && r.children.length);
              flat.push(r);
              this._resourcesMap[r.id] = r;
              if (r.isParent) {
                  this._hasHierarchy = true;
                  if (!r.collapsed || all) {
                      this._flattenResources(r.children, flat, depth + 1, all);
                  }
              }
          }
          return flat;
      };
      // #region Lifecycle hooks
      STBase.prototype._render = function (s, state) {
          var _this = this;
          var prevS = this._prevS;
          var isTimeline = this._isTimeline;
          var selected = new Date(s.selected);
          var size = +s.size;
          var stepLabel = roundStep(s.timeLabelStep);
          var stepCell = roundStep(s.timeCellStep);
          var firstDay = s.firstDay;
          var startDay = s.startDay;
          var endDay = s.endDay;
          var resources = s.resources;
          var slots = s.slots;
          var disableVirtual = s.virtualScroll === false;
          var resolution = s.resolution;
          var isDailyResolution = resolution === 'day' || resolution === 'hour' || !isTimeline;
          var hasResY = s.resolutionVertical === 'day';
          var calcDays = false;
          var viewChanged = false;
          var reloadData = false;
          var startTime = this._startTime;
          var endTime = this._endTime;
          if (startDay !== prevS.startDay ||
              endDay !== prevS.endDay ||
              s.checkSize !== prevS.checkSize ||
              s.eventList !== prevS.eventList ||
              s.refDate !== prevS.refDate ||
              s.size !== prevS.size ||
              s.type !== prevS.type ||
              s.resolution !== prevS.resolution ||
              s.resolutionVertical !== prevS.resolutionVertical ||
              s.displayTimezone !== prevS.displayTimezone ||
              s.weekNumbers !== prevS.weekNumbers) {
              calcDays = true;
              viewChanged = true;
          }
          if (calcDays ||
              s.rtl !== prevS.rtl ||
              s.dateFormat !== prevS.dateFormat ||
              s.getDay !== prevS.getDay ||
              s.rowHeight !== prevS.rowHeight) {
              reloadData = true;
          }
          if (s.startTime !== prevS.startTime ||
              s.endTime !== prevS.endTime ||
              s.timeLabelStep !== prevS.timeLabelStep ||
              s.timeCellStep !== prevS.timeCellStep ||
              s.timeFormat !== prevS.timeFormat ||
              this._startTime === UNDEFINED ||
              this._endTime === UNDEFINED) {
              var start = makeDate(s.startTime || '00:00');
              var end = new Date(+makeDate(s.endTime || '00:00') - 1);
              this._startTime = startTime = getDayMilliseconds(start);
              this._endTime = endTime = getDayMilliseconds(end);
              this._time = endTime - startTime + 1;
              this._timesBetween = getArray(floor(stepCell / stepLabel) - 1);
              this._times = [];
              this._timeLabels = {};
              var timeStep = stepCell * ONE_MIN;
              var timesFrom = floor(startTime / timeStep) * timeStep;
              var _loop_5 = function (d) {
                  this_2._times.push(d);
                  if (isTimeline) {
                      // Pre-generate time labels to prevent in on every render
                      var first = d === timesFrom;
                      this_2._timeLabels[d] = first || d % (stepLabel * ONE_MIN) === 0 ? this_2._formatTime(first ? startTime : d) : '';
                      this_2._timesBetween.forEach(function (tb, i) {
                          var ms = d + (i + 1) * stepLabel * ONE_MIN;
                          _this._timeLabels[ms] = _this._formatTime(ms);
                      });
                  }
              };
              var this_2 = this;
              for (var d = timesFrom; d <= endTime; d += timeStep) {
                  _loop_5(d);
              }
              viewChanged = true;
              reloadData = true;
          }
          if (s.slots !== prevS.slots || this._slots === UNDEFINED) {
              this._hasSlots = isTimeline && !!slots && slots.length > 0;
              this._slots = slots && slots.length ? slots : [{ id: DEF_ID }];
              reloadData = true;
          }
          if (resources !== prevS.resources || this._resources === UNDEFINED) {
              this._hasResources = !!resources && resources.length > 0;
              this._hasHierarchy = false;
              this._resourcesMap = {};
              this._resources = this._flattenResources(resources, [], 0, true);
              this._visibleResources = this._flattenResources(resources, [], 0);
              this._isSingleResource = this._resources.length === 1;
              reloadData = true;
          }
          if (calcDays ||
              s.selected !== prevS.selected ||
              s.getDay !== prevS.getDay ||
              s.monthNames !== prevS.monthNames ||
              s.dateFormat !== prevS.dateFormat ||
              s.currentTimeIndicator !== prevS.currentTimeIndicator) {
              var now = removeTimezone(createDate(s));
              var isDaily = s.type === 'day';
              var isMonthly = s.type === 'month';
              var isYearly = s.type === 'year';
              var isDayViewOnly = isDaily && size < 2;
              var navService = s.navigationService;
              var monthPos = s.dateFormat.search(/m/i);
              var yearPos = s.dateFormat.search(/y/i);
              var datePos = s.dateFormat.search(/d/i);
              var yearFirst = yearPos < monthPos;
              var dayFirst = datePos < monthPos;
              var firstGridDay = void 0;
              var lastGridDay = void 0;
              var firstHeaderDay = void 0;
              var lastHeaderDay = void 0;
              if (size > 1 || isYearly || isMonthly) {
                  firstHeaderDay = firstGridDay = navService.firstDay;
                  lastHeaderDay = lastGridDay = navService.lastDay;
              }
              else {
                  var firstWeekDay = getFirstDayOfWeek(selected, s);
                  firstHeaderDay = addDays(firstWeekDay, startDay - firstDay + (startDay < firstDay ? 7 : 0));
                  if (isDaily) {
                      // When startDay is different from the locale firstDay, the selected day might end up
                      // outside of the week defined by startDay and end Day
                      if (selected < firstHeaderDay) {
                          firstHeaderDay = addDays(firstHeaderDay, -7);
                      }
                      if (selected >= addDays(firstHeaderDay, 7)) {
                          firstHeaderDay = addDays(firstHeaderDay, 7);
                      }
                  }
                  lastHeaderDay = addDays(firstHeaderDay, endDay - startDay + 1 + (endDay < startDay ? 7 : 0));
                  firstGridDay = isDaily ? getDateOnly(selected) : firstHeaderDay;
                  lastGridDay = isDaily ? addDays(firstGridDay, 1) : lastHeaderDay;
              }
              if (isTimeline && resolution === 'week' && (isYearly || isMonthly)) {
                  firstGridDay = navService.viewStart;
                  lastGridDay = navService.viewEnd;
              }
              this._isMulti = size > 1 || isYearly;
              this._isDailyResolution = isDailyResolution;
              this._hasResY = hasResY;
              this._firstDay = firstGridDay;
              this._lastDay = lastGridDay;
              this._firstDayTz = createDate(s, firstGridDay.getFullYear(), firstGridDay.getMonth(), firstGridDay.getDate());
              this._lastDayTz = createDate(s, lastGridDay.getFullYear(), lastGridDay.getMonth(), lastGridDay.getDate());
              this._selectedDay = +getDateOnly(selected);
              this._setRowHeight = s.eventList || s.rowHeight !== 'equal';
              this._shouldAnimateScroll = prevS.selected !== UNDEFINED && s.selected !== prevS.selected && !viewChanged;
              this._showTimeIndicator =
                  !s.eventList &&
                      (s.currentTimeIndicator === UNDEFINED ? !isTimeline || (isDailyResolution && stepCell < 1440) : s.currentTimeIndicator) &&
                      (isDaily && size < 2 ? isSameDay(now, selected) : firstGridDay <= now && lastGridDay >= now);
              // Generate day data
              this._colIndexMap = {};
              this._cols = [];
              this._dayIndexMap = {};
              this._days = [];
              this._headerDays = [];
              var i = 0;
              var j = -1;
              var dayDiff = 0;
              var daysInMonth = 0;
              var year = -1;
              var columnTitle = '';
              var month = -1;
              var monthIndex = -1;
              var monthText = '';
              var week = -1;
              var weekIndex = -1;
              var weekText = '';
              var first = firstGridDay;
              var last = lastGridDay;
              var lastColStart = 0;
              var weekEndDay = UNDEFINED;
              var newWeek = 0;
              if (!isTimeline && isDayViewOnly) {
                  first = firstHeaderDay;
                  last = lastHeaderDay;
              }
              for (var d = getDateOnly(first); d < getDateOnly(last); d.setDate(d.getDate() + 1)) {
                  var dateKey = getDateStr(d);
                  var weekDay = d.getDay();
                  this._dayIndexMap[dateKey] = i;
                  if (isInWeek(weekDay, startDay, endDay)) {
                      var lastOfMonth = void 0;
                      var monthTitle = '';
                      var weekTitle = '';
                      var columnChange = isDailyResolution;
                      if (isTimeline && !hasResY) {
                          newWeek = s.getWeekNumber(addDays(d, (7 - firstDay + 1) % 7));
                          var newDay = s.getDay(d);
                          var newMonth = s.getMonth(d);
                          var newYear = s.getYear(d);
                          var monthName = s.monthNames[newMonth];
                          if (year !== newYear) {
                              year = newYear;
                              if (resolution === 'year') {
                                  columnChange = true;
                                  columnTitle = '' + year;
                              }
                          }
                          if (month !== newMonth) {
                              if (resolution === 'month') {
                                  columnTitle = isYearly && size < 2 ? monthName : yearFirst ? newYear + ' ' + monthName : monthName + ' ' + newYear;
                                  columnChange = true;
                              }
                              else if (isDailyResolution) {
                                  monthText = yearFirst ? newYear + ' ' + monthName : monthName + ' ' + newYear;
                                  monthTitle = monthText;
                              }
                              monthIndex = i;
                              month = newMonth;
                              daysInMonth = s.getMaxDayOfMonth(year, month);
                          }
                          if (week !== newWeek) {
                              weekIndex = i;
                              week = newWeek;
                              weekText = s.weekText.replace(/{count}/, week);
                              weekTitle = weekText;
                              if (i > 0) {
                                  this._days[i - 1].lastOfWeek = true;
                              }
                          }
                          if ((weekDay === startDay || !i) && resolution === 'week') {
                              var dateFormat = dayFirst ? 'D MMM' : 'MMM D';
                              weekEndDay = addDays(d, endDay - startDay + (endDay < startDay ? 7 : 0));
                              columnTitle = formatDate(dateFormat, d, s) + ' - ' + formatDate(dateFormat, weekEndDay, s);
                              columnChange = true;
                          }
                          var hiddenWeekDays = (startDay - endDay - 1 + 7) % 7;
                          lastOfMonth = newDay === daysInMonth || (weekDay === endDay && hiddenWeekDays >= daysInMonth - newDay);
                          if (lastOfMonth && resolution === 'month') {
                              dayDiff += 31 - daysInMonth;
                          }
                      }
                      var dayData = {
                          columnTitle: columnTitle,
                          date: new Date(d),
                          dateIndex: i,
                          dateKey: dateKey,
                          dateText: formatDate(hasResY
                              ? isMonthly && !this._isMulti
                                  ? 'D DDD'
                                  : resources
                                      ? s.dateFormatLong
                                      : s.dateFormat
                              : isMonthly || this._isMulti
                                  ? 'D DDD'
                                  : s.dateFormatLong, d, s),
                          day: s.getDay(d),
                          dayDiff: dayDiff,
                          endDate: weekEndDay,
                          eventMap: {},
                          label: formatDate('DDDD, MMMM D, YYYY', d, s),
                          lastOfMonth: lastOfMonth,
                          monthIndex: monthIndex,
                          monthText: monthText,
                          monthTitle: monthTitle,
                          timestamp: +getDateOnly(d),
                          weekIndex: weekIndex,
                          weekNr: newWeek,
                          weekText: weekText,
                          weekTitle: weekTitle,
                      };
                      if (columnChange) {
                          dayData.isActive = d <= now && now < last;
                          if (lastColStart) {
                              this._cols[j].isActive = lastColStart <= +now && now < d;
                          }
                          lastColStart = +d;
                          this._cols.push(dayData);
                          j++;
                      }
                      if (isDayViewOnly) {
                          this._headerDays.push(dayData);
                      }
                      if (!isDayViewOnly || this._selectedDay === +d) {
                          this._days.push(dayData);
                      }
                      if (lastOfMonth && resolution === 'month') {
                          // Since month widths are equal, we handle each month as 31 days,
                          // and fill the remaining days with the data of the last day of the month
                          for (var k = daysInMonth; k < 31; k++) {
                              this._days.push(dayData);
                              i++;
                          }
                      }
                      i++;
                  }
                  this._colIndexMap[dateKey] = j < 0 ? 0 : j;
              }
              this._colsNr = hasResY ? 1 : j + 1;
              this._daysNr = hasResY || isDayViewOnly ? 1 : i;
          }
          this._groupByResource = (s.groupBy !== 'date' && !(s.type === 'day' && size < 2)) || this._isSingleResource;
          this._stepCell = stepCell * ONE_MIN;
          this._stepLabel = stepLabel * ONE_MIN;
          this._dayNames = state.dayNameWidth > 49 ? s.dayNamesShort : s.dayNamesMin;
          this._displayTime = stepLabel < 1440 && isDailyResolution;
          this._eventHeight = state.eventHeight || (s.eventList ? 24 : 46);
          this._showCursorTime = this._displayTime && !!(s.dragToCreate || s.dragToMove || s.dragToResize);
          this._viewChanged = viewChanged;
          if (s.colorsMap !== prevS.colorsMap || reloadData) {
              this._colors = this._getColors(s.colorsMap);
          }
          if (s.eventMap !== prevS.eventMap || reloadData || !this._events) {
              this._eventMap = {};
              this._eventRows = {};
              this._events = this._getEvents(s.eventMap);
          }
          if (s.invalidsMap !== prevS.invalidsMap || reloadData) {
              this._invalids = this._getInvalids(s.invalidsMap);
          }
          // We need to check the event height in case of timeline
          var checkEventHeight = isTimeline && s.eventMap !== prevS.eventMap;
          if (s.height !== prevS.height || s.width !== prevS.width || checkEventHeight || reloadData) {
              this._shouldCheckSize = isBrowser && !!s.height && !!s.width;
          }
          if (s.scroll !== prevS.scroll) {
              this._shouldScroll = true;
          }
          if (s.height !== UNDEFINED) {
              // Only set sticky on the second render, to solve SSR different markup issues
              this._hasSideSticky = hasSticky && !s.rtl;
              this._hasSticky = hasSticky;
          }
          // Calculate day batches for virtual scroll
          if (isTimeline) {
              var colsNr = this._colsNr;
              var daysBatch = [];
              // limit rendered days to min 1 day & max 30(maxBatchDay) days
              var daysBatchNr = this._daysBatchNr === UNDEFINED ? constrain(floor(this._stepCell / (this._time / 30)), 1, 30) : this._daysBatchNr;
              var dayIndex = this._dayIndexMap[getDateStr(selected)] || 0;
              var batchIndexX = state.batchIndexX !== UNDEFINED ? state.batchIndexX : round(dayIndex / daysBatchNr);
              // limit the batch day index within the displayed days (it can be bigger if switching from a big view to a smaller one)
              var batchDayIndex = Math.min(batchIndexX * daysBatchNr, colsNr - 1);
              var batchStart = disableVirtual ? 0 : Math.max(0, batchDayIndex - floor((daysBatchNr * 3) / 2));
              var batchEnd = disableVirtual ? colsNr : Math.min(batchStart + 3 * daysBatchNr, colsNr);
              var batchStartDay = this._cols[batchStart].date;
              var batchEndDay = batchEnd < colsNr ? this._cols[batchEnd].date : this._lastDay;
              for (var i = batchStart; i < batchEnd; i++) {
                  daysBatch.push(this._cols[i]);
              }
              this._batchStart = createDate(s, batchStartDay.getFullYear(), batchStartDay.getMonth(), batchStartDay.getDate());
              this._batchEnd = createDate(s, batchEndDay.getFullYear(), batchEndDay.getMonth(), batchEndDay.getDate());
              this._daysBatch = daysBatch;
              this._daysBatchNr = daysBatchNr;
              this._placeholderSizeX = state.dayWidth * round(Math.max(0, batchDayIndex - (daysBatchNr * 3) / 2)) || 0;
              this._rowHeights = {};
              this._dragRow = '';
              // vertical virtual scroll
              var gridContHeight_1 = (state.scrollContHeight || 0) - (state.headerHeight || 0) - (state.footerHeight || 0);
              var rowHeight_1 = state.rowHeight || 52;
              var parentRowHeight_1 = state.parentRowHeight || 52;
              var gutterHeight_1 = state.gutterHeight !== UNDEFINED ? state.gutterHeight : 16;
              var batchIndexY = state.batchIndexY || 0;
              var visibleResources_1 = this._visibleResources;
              var verticalDays = hasResY ? this._days : [{}];
              var totalRows = visibleResources_1.length * verticalDays.length;
              var rows_1 = [];
              var rowBatch = [];
              var addedGroups = {};
              var addedRows = {};
              var resourceMap_1 = {};
              var virtualPagesY_1 = [];
              var pageIndex_1 = -1;
              var gridHeight_1 = 0;
              // calculate virtual pages for vertical scroll
              if (state.hasScrollY) {
                  this._resourceTops = {};
              }
              verticalDays.forEach(function (d, i) {
                  visibleResources_1.forEach(function (r, j) {
                      var key = (hasResY ? d.dateKey + '-' : '') + r.id;
                      resourceMap_1[key] = r;
                      if (gridContHeight_1) {
                          // in case of event listing the default calculated height is less then css min-height
                          var currRowHeight = r.children ? parentRowHeight_1 : rowHeight_1;
                          var resHeight = _this._setRowHeight
                              ? r.eventCreation === false
                                  ? currRowHeight
                                  : Math.max((_this._eventRows[key] || 1) * _this._eventHeight + gutterHeight_1, currRowHeight)
                              : currRowHeight;
                          _this._rowHeights[key] = _this._setRowHeight ? resHeight + 'px' : UNDEFINED;
                          var currPageIndex = floor(gridHeight_1 / gridContHeight_1);
                          if (state.hasScrollY) {
                              // Store resource row tops if there is vertical scroll
                              _this._resourceTops[key] = gridHeight_1;
                          }
                          if (currPageIndex !== pageIndex_1) {
                              virtualPagesY_1.push({
                                  startIndex: i * visibleResources_1.length + j,
                                  top: gridHeight_1,
                              });
                              pageIndex_1 = currPageIndex;
                          }
                          gridHeight_1 += resHeight;
                      }
                      rows_1.push({ dayIndex: i, key: key, resource: r });
                  });
              });
              var startPage = virtualPagesY_1[batchIndexY - 1];
              var endPage = virtualPagesY_1[batchIndexY + 2];
              var batchStartY = startPage ? startPage.startIndex : 0;
              // Render max 30 resources on the initial render
              var batchEndY = endPage ? endPage.startIndex : gridHeight_1 ? totalRows : 30;
              // When there's no scroll, render all rows
              if (disableVirtual || (gridHeight_1 && gridHeight_1 <= gridContHeight_1)) {
                  batchStartY = 0;
                  batchEndY = totalRows;
              }
              var rowGroup = [];
              var lastDayIndex = -1;
              for (var i = batchStartY; i < batchEndY; i++) {
                  var row = rows_1[i];
                  if (row) {
                      var currDayIndex = row.dayIndex;
                      if (lastDayIndex !== currDayIndex) {
                          rowGroup = [];
                          rowBatch.push({ day: hasResY ? this._days[currDayIndex] : UNDEFINED, rows: rowGroup });
                          lastDayIndex = currDayIndex;
                          addedGroups[currDayIndex] = rowBatch[rowBatch.length - 1];
                      }
                      addedRows[row.key] = true;
                      rowGroup.push(row.resource);
                  }
              }
              // Add the row of the dragged event, if not on the virtual page, otherwise mouse and touch events will stop firing
              if (state.dragData && state.dragData.originResource !== UNDEFINED) {
                  var resource = state.dragData.originResource;
                  var dateKey = getDateStr(new Date(state.dragData.originDate));
                  var key = (hasResY ? dateKey + '-' : '') + resource;
                  var groupIndex = hasResY ? this._dayIndexMap[dateKey] : 0;
                  if (!addedRows[key]) {
                      var group = addedGroups[groupIndex];
                      if (!group) {
                          group = { day: hasResY ? this._days[groupIndex] : UNDEFINED, hidden: true, rows: [] };
                          rowBatch.push(group);
                      }
                      group.rows.push(resourceMap_1[key]);
                      this._dragRow = key;
                  }
              }
              this._gridHeight = gridHeight_1;
              this._virtualPagesY = virtualPagesY_1;
              this._colClass = resources || !hasResY ? 'mbsc-timeline-resource-col' : 'mbsc-timeline-date-col';
              this._hasRows = this._hasResources || hasResY;
              this._rows = rows_1;
              this._rowBatch = rowBatch;
              this._placeholderSizeY = startPage && !disableVirtual ? startPage.top : 0;
          }
      };
      STBase.prototype._mounted = function () {
          var _this = this;
          var allowCreate;
          var allowStart;
          var validTarget;
          this._unlisten = gestureListener(this._el, {
              onDoubleClick: function (args) {
                  var s = _this.s;
                  if (validTarget && s.clickToCreate && s.clickToCreate !== 'single') {
                      args.click = true;
                      if (_this._onEventDragStart(args)) {
                          _this._onEventDragEnd(args);
                      }
                  }
              },
              onEnd: function (args) {
                  if (!allowCreate && allowStart && _this.s.clickToCreate === 'single') {
                      args.click = true;
                      if (_this._onEventDragStart(args)) {
                          allowCreate = true;
                      }
                  }
                  if (allowCreate) {
                      // Will prevent mousedown event on doc, which would exit drag mode
                      args.domEvent.preventDefault();
                      _this._onEventDragEnd(args);
                  }
                  clearTimeout(_this._touchTimer);
                  allowCreate = false;
                  allowStart = false;
              },
              onMove: function (args) {
                  var s = _this.s;
                  if (allowCreate && s.dragToCreate) {
                      args.domEvent.preventDefault();
                      _this._onEventDragMove(args);
                  }
                  else if (allowStart && s.dragToCreate && (Math.abs(args.deltaX) > 7 || Math.abs(args.deltaY) > 7)) {
                      if (_this._onEventDragStart(args)) {
                          allowCreate = true;
                      }
                      else {
                          allowStart = false;
                      }
                  }
                  else {
                      clearTimeout(_this._touchTimer);
                  }
              },
              onStart: function (args) {
                  var s = _this.s;
                  args.create = true;
                  args.click = false;
                  _this._isTouch = args.isTouch;
                  if (!allowCreate && (s.dragToCreate || s.clickToCreate)) {
                      var targetClasses = (args.domEvent.target && args.domEvent.target.classList) || [];
                      validTarget =
                          targetClasses.contains('mbsc-schedule-item') ||
                              targetClasses.contains('mbsc-schedule-all-day-item') ||
                              targetClasses.contains('mbsc-timeline-column');
                      if (validTarget) {
                          if (args.isTouch && s.dragToCreate) {
                              _this._touchTimer = setTimeout(function () {
                                  if (_this._onEventDragStart(args)) {
                                      _this._onEventDragModeOn(args);
                                      allowCreate = true;
                                  }
                              }, 350);
                          }
                          else {
                              allowStart = !args.isTouch;
                          }
                      }
                  }
              },
          });
          this._unsubscribe = subscribeExternalDrag(this._onExternalDrag);
      };
      STBase.prototype._updated = function () {
          var _this = this;
          var s = this.s;
          var state = this.state;
          if (this._shouldCheckSize) {
              ngSetTimeout(this, function () {
                  var resCont = _this._resCont;
                  var headerCont = _this._headerCont;
                  var footerCont = _this._footerCont;
                  var sidebarCont = _this._sidebarCont;
                  var stickyFooter = _this._stickyFooter;
                  var headerHeight = headerCont.offsetHeight;
                  var resContWidth = resCont ? resCont.offsetWidth : 0;
                  var sidebarWidth = sidebarCont ? sidebarCont.offsetWidth : 0;
                  var footerHeight = footerCont ? footerCont.offsetHeight : 0;
                  var scrollCont = _this._scrollCont;
                  var scrollContWidth = scrollCont.offsetWidth;
                  var scrollContHeight = scrollCont.offsetHeight;
                  var scrollClientWidth = scrollCont.clientWidth;
                  var scrollClientHeight = scrollCont.clientHeight;
                  var gridContWidth = scrollClientWidth - resContWidth - sidebarWidth; // Available space for grid
                  var gridContHeight = scrollClientHeight - headerHeight - footerHeight;
                  var scrollBarSizeY = scrollContWidth - scrollClientWidth;
                  var scrollBarSizeX = scrollContHeight - scrollClientHeight;
                  var hasScrollY = scrollCont.scrollHeight > scrollClientHeight;
                  var hasScrollX = scrollCont.scrollWidth > scrollClientWidth;
                  var cellHeight;
                  var cellWidth;
                  var dayWidth;
                  var dayNameWidth;
                  var gridWidth;
                  var rowHeight;
                  var parentRowHeight;
                  var gutterHeight;
                  var eventHeight = state.eventHeight;
                  _this._calcGridSizes();
                  if (_this._isTimeline) {
                      var day = scrollCont.querySelector('.mbsc-timeline-day');
                      var gridRow = scrollCont.querySelector('.mbsc-timeline-empty-row');
                      var parentRow = scrollCont.querySelector('.mbsc-timeline-empty-parent');
                      var gutter = scrollCont.querySelector('.mbsc-timeline-row-gutter');
                      var colsNr = _this._colsNr;
                      dayWidth = day ? day.offsetWidth : 64;
                      rowHeight = gridRow ? gridRow.offsetHeight : 52;
                      parentRowHeight = parentRow ? parentRow.offsetHeight : 52;
                      gutterHeight = gutter ? gutter.offsetHeight : 16;
                      // Since the width of the grid is set in case of virtual scroll, we need to double check, if there will be horizontal scroll
                      if (dayWidth * colsNr < gridContWidth) {
                          hasScrollX = false;
                          scrollBarSizeX = 0;
                      }
                      if (_this._gridHeight && _this._gridHeight < gridContHeight) {
                          hasScrollY = false;
                          scrollBarSizeY = 0;
                      }
                      dayWidth = hasScrollX ? dayWidth : round(gridContWidth / colsNr);
                      gridWidth = hasScrollX ? dayWidth * colsNr : gridContWidth;
                      cellWidth = (_this._stepCell * dayWidth) / _this._time;
                      _this._gridWidth = gridWidth;
                      // Day width might be 0, if the calendar container is removed while rendering
                      _this._daysBatchNr = Math.max(1, Math.ceil(gridContWidth / (dayWidth || 1)));
                      if (!_this._hasSticky) {
                          headerCont.style[s.rtl ? 'left' : 'right'] = scrollBarSizeY + 'px';
                          if (footerCont) {
                              footerCont.style[s.rtl ? 'left' : 'right'] = scrollBarSizeY + 'px';
                              footerCont.style.bottom = scrollBarSizeX + 'px';
                          }
                      }
                      if (!_this._hasSideSticky) {
                          if (resCont) {
                              resCont.style.bottom = scrollBarSizeX + 'px';
                          }
                          if (sidebarCont) {
                              sidebarCont.style[s.rtl ? 'left' : 'right'] = scrollBarSizeY + 'px';
                          }
                      }
                      if (stickyFooter) {
                          stickyFooter.style.bottom = scrollBarSizeX + 'px';
                      }
                      if (_this._setRowHeight) {
                          var event_7 = scrollCont.querySelector('.mbsc-schedule-event');
                          eventHeight = event_7 ? event_7.clientHeight : eventHeight || (s.eventList ? 24 : 46);
                      }
                      if (!hasScrollY && state.rowHeight) {
                          // Calculate tops of the resource rows, when there's no vertical scroll
                          _this._resourceTops = {};
                          var grid = _this._gridCont;
                          var gridRect_1 = grid.getBoundingClientRect();
                          var rows = grid.querySelectorAll('.mbsc-timeline-row');
                          rows.forEach(function (r, i) {
                              _this._resourceTops[_this._rows[i].key] = r.getBoundingClientRect().top - gridRect_1.top;
                          });
                      }
                  }
                  else {
                      var gridCol = _this._el.querySelector('.mbsc-schedule-column-inner');
                      var dayName = _this._el.querySelector('.mbsc-schedule-header-item');
                      cellHeight = gridCol ? (_this._stepCell * gridCol.offsetHeight) / _this._time : 0;
                      dayNameWidth = dayName ? dayName.offsetWidth : 0;
                  }
                  // Make sure scroll remains in sync
                  _this._onScroll();
                  _this._calcConnections = !!s.connections && (_this._isParentClick || _this._calcConnections || !hasScrollY);
                  // We need another round here to calculate the correct resource tops, after the row heights are set
                  _this._shouldCheckSize = rowHeight !== state.rowHeight || eventHeight !== state.eventHeight;
                  _this._isCursorTimeVisible = false;
                  _this.setState({
                      cellHeight: cellHeight,
                      cellWidth: cellWidth,
                      dayNameWidth: dayNameWidth,
                      dayWidth: dayWidth,
                      eventHeight: eventHeight,
                      footerHeight: footerHeight,
                      gridWidth: gridWidth,
                      gutterHeight: gutterHeight,
                      hasScrollX: hasScrollX,
                      hasScrollY: hasScrollY,
                      headerHeight: headerHeight,
                      parentRowHeight: parentRowHeight,
                      rowHeight: rowHeight,
                      scrollContHeight: hasScrollX ? scrollClientHeight : scrollContHeight,
                      // Force update if connection calculation is needed
                      update: _this._calcConnections ? (state.update || 0) + 1 : state.update,
                  });
              });
          }
          // only scroll to time when the dayWidth is set in case of timeline
          if (this._shouldScroll && (state.dayWidth || !this._isTimeline)) {
              setTimeout(function () {
                  _this._scrollToTime(_this._shouldAnimateScroll);
                  _this._shouldAnimateScroll = false;
              });
              this._shouldScroll = false;
          }
          if (this._viewChanged) {
              setTimeout(function () {
                  _this._viewChanged = false;
              }, 10);
          }
      };
      STBase.prototype._destroy = function () {
          if (this._unlisten) {
              this._unlisten();
          }
          if (this._unsubscribe) {
              unsubscribeExternalDrag(this._unsubscribe);
          }
      };
      // #endregion Lifecycle hooks
      STBase.prototype._calcGridSizes = function () {
          var s = this.s;
          var resources = this._resources;
          var isTimeline = this._isTimeline;
          var daysNr = this._daysNr * (isTimeline ? 1 : resources.length);
          var grid = this._gridCont;
          var scrollCont = this._scrollCont;
          var allDayCont = !isTimeline && this._el.querySelector('.mbsc-schedule-all-day-wrapper');
          var allDayRect = allDayCont && allDayCont.getBoundingClientRect();
          var rect = grid.getBoundingClientRect();
          var gridRect = scrollCont.getBoundingClientRect();
          var gridWidth = isTimeline ? rect.width : grid.scrollWidth;
          var resWidth = this._resCont ? this._resCont.offsetWidth : 0;
          this._gridLeft = s.rtl ? rect.right - gridWidth : rect.left;
          this._gridRight = s.rtl ? rect.right : rect.left + gridWidth;
          this._gridTop = rect.top;
          this._gridContTop = gridRect.top;
          this._gridContBottom = gridRect.bottom;
          this._gridContLeft = gridRect.left + (s.rtl ? 0 : resWidth);
          this._gridContRight = gridRect.right - (s.rtl ? resWidth : 0);
          this._allDayTop = allDayRect ? allDayRect.top : this._gridContTop;
          this._colWidth = gridWidth / (s.eventList ? this._colsNr : daysNr);
          this._colHeight = rect.height;
      };
      STBase.prototype._getDragDates = function (event, resourceId, slotId) {
          var s = this.s;
          var dates = {};
          var dragDisplayedMap = new Map();
          var first = event.allDay ? this._firstDay : this._firstDayTz;
          var start = event.startDate;
          var end = event.endDate;
          start = getDateOnly(start);
          start = start < first ? first : start;
          end = getEndDate(s, event.allDay || s.eventList, start, end);
          // If event has no duration, it should still be added to the start day
          while (start <= end) {
              var eventForDay = __assign({}, event);
              var dateKey = getDateStr(start);
              var pos = isInWeek(start.getDay(), s.startDay, s.endDay) && this._getEventPos(event, start, dateKey, dragDisplayedMap);
              if (pos) {
                  var eventResource = eventForDay.resource;
                  if (this._isTimeline &&
                      this._setRowHeight &&
                      (isArray(eventResource) ? eventResource : [eventResource]).indexOf(this._tempResource) !== -1) {
                      // update the dragged event with the original event's top position to remain on the same place
                      pos.position.top = eventForDay.position.top;
                  }
                  var key = this._isTimeline && !this._hasSlots && !this._hasResY ? 'all' : dateKey;
                  eventForDay.date = +getDateOnly(start, true);
                  eventForDay.cssClass = pos.cssClass;
                  eventForDay.start = pos.start;
                  eventForDay.end = pos.end;
                  eventForDay.position = pos.position;
                  // Add the data for the day
                  dates[resourceId + '__' + (this._isTimeline ? slotId + '__' : '') + key] = eventForDay;
              }
              start = addDays(start, 1);
          }
          return dates;
      };
      /**
       * Returns a date with the time based on the coordinates on the grid.
       * @param day We're on this day.
       * @param posX X coord - for timeline.
       * @param posY Y coord - for schedule.
       * @param dayIndex Index of the day on the timeline.
       * @param timeStep Time step in minutes.
       */
      STBase.prototype._getGridTime = function (day, posX, posY, dayIndex, timeStep) {
          var dayIdx = this._hasResY ? 0 : dayIndex;
          var ms = step(this._isTimeline
              ? floor((this._time * (posX - dayIdx * this._colWidth)) / this._colWidth)
              : floor((this._time * (posY - 8)) / (this._colHeight - 16)), timeStep * ONE_MIN); // Remove 16px for top and bottom spacing
          var time = new Date(+REF_DATE + this._startTime + ms); // Date with no DST
          return createDate(this.s, day.getFullYear(), day.getMonth(), day.getDate(), time.getHours(), time.getMinutes());
      };
      STBase.prototype._scrollToTime = function (animate) {
          var _this = this;
          var el = this._scrollCont;
          var gridCont = this._gridCont;
          var isTimeline = this._isTimeline;
          if (el) {
              var s = this.s;
              var hasResY = this._hasResY;
              var targetEvent = s.navigateToEvent;
              var targetDate = targetEvent && targetEvent.start
                  ? roundTime(new Date(+makeDate(targetEvent.start, s) - this._stepCell), this._stepCell / ONE_MIN)
                  : new Date(s.selected); // : createDate(s, s.selected);
              var colIndex = this._colIndexMap[getDateStr(targetDate)];
              if (colIndex !== UNDEFINED && isTimeline && !hasResY && (s.resolution !== 'hour' || this._stepCell === ONE_DAY || s.eventList)) {
                  targetDate = this._cols[colIndex].date;
              }
              else {
                  targetDate.setHours(s.eventList ? 0 : targetDate.getHours(), 0);
              }
              var timeStart = getEventStart(targetDate, this._startTime, this._time * (isTimeline ? this._daysNr : 1));
              var dayDiff = hasResY ? 0 : getGridDayDiff(this._firstDay, targetDate, s.startDay, s.endDay);
              var width = isTimeline ? gridCont.offsetWidth : gridCont.scrollWidth;
              var newScrollPosX = (width * ((dayDiff * 100) / this._daysNr + (isTimeline ? timeStart : 0))) / 100 + 1;
              var newScrollPosY = void 0;
              if (targetEvent || hasResY) {
                  var resources = this._visibleResources;
                  var resource = targetEvent ? targetEvent.resource : resources[0].id;
                  var targetResource_1 = isArray(resource) ? resource[0] : resource;
                  if (targetResource_1) {
                      if (isTimeline) {
                          var key = (hasResY ? getDateStr(targetDate) + '-' : '') + targetResource_1;
                          newScrollPosY = this._resourceTops && this._resourceTops[key];
                      }
                      else {
                          var colWidth = this._colWidth;
                          var resourceIndex = findIndex(resources, function (r) { return r.id === targetResource_1; }) || 0;
                          if (this._groupByResource && !this._isSingleResource) {
                              newScrollPosX = this._daysNr * colWidth * resourceIndex + colWidth * dayDiff;
                          }
                          else {
                              newScrollPosX = resources.length * dayDiff * colWidth + resourceIndex * colWidth;
                          }
                      }
                  }
              }
              if (!isTimeline) {
                  var gridCol = el.querySelector('.mbsc-schedule-column-inner');
                  newScrollPosY = gridCol ? (gridCol.offsetHeight * timeStart) / 100 : 0;
                  if (this._groupByResource && !this._isSingleResource && !targetEvent) {
                      newScrollPosX = UNDEFINED;
                  }
              }
              this._isScrolling++;
              smoothScroll(el, newScrollPosX, newScrollPosY, animate, s.rtl, function () {
                  setTimeout(function () {
                      _this._isScrolling--;
                  }, 150);
              });
          }
      };
      return STBase;
  }(BaseComponent));

  /** @hidden */
  var SchedulerBase = /*#__PURE__*/ (function (_super) {
      __extends(SchedulerBase, _super);
      function SchedulerBase() {
          var _this = _super !== null && _super.apply(this, arguments) || this;
          // tslint:disable-next-line: variable-name
          _this._onScroll = function () {
              var grid = _this._scrollCont;
              if (grid) {
                  var scrollTop = grid.scrollTop;
                  var scrollLeft = 'translateX(' + -grid.scrollLeft + 'px)';
                  var timeCont = _this._timeCont;
                  var allDay = _this._allDayCont;
                  var header = _this._headerCont;
                  var transform = (jsPrefix ? jsPrefix + 'T' : 't') + 'ransform';
                  if (allDay) {
                      allDay.style[transform] = scrollLeft;
                  }
                  if (timeCont) {
                      timeCont.style.marginTop = -scrollTop + 'px';
                  }
                  if (header) {
                      header.style[transform] = scrollLeft;
                  }
                  if (scrollTop === 0) {
                      _this.setState({ showShadow: false });
                  }
                  else if (!_this.state.showShadow) {
                      _this.setState({ showShadow: true });
                  }
                  _this._onMouseMove();
              }
          };
          // tslint:disable-next-line: variable-name
          _this._setCont = function (el) {
              _this._scrollCont = el;
          };
          // tslint:disable-next-line: variable-name
          _this._setTimeCont = function (el) {
              _this._timeCont = el;
          };
          // tslint:disable-next-line: variable-name
          _this._setAllDayCont = function (el) {
              _this._allDayCont = el;
          };
          // tslint:disable-next-line: variable-name
          _this._setGridCont = function (el) {
              _this._gridCont = el;
          };
          // tslint:disable-next-line: variable-name
          _this._setHeaderCont = function (el) {
              _this._headerCont = el;
          };
          // tslint:disable-next-line: variable-name
          _this._setCursorTimeCont = function (el) {
              _this._cursorTimeCont = el;
          };
          return _this;
      }
      SchedulerBase.prototype._render = function (s, state) {
          _super.prototype._render.call(this, s, state);
          var prevS = this._prevS;
          var timezones = s.timezones;
          var stepCell = this._stepCell / ONE_MIN;
          var startMinutes = floor(this._startTime / ONE_MIN) % stepCell;
          var endMinutes = (floor(this._endTime / ONE_MIN) % stepCell) + 1;
          if (timezones !== prevS.timezones) {
              this._timeWidth = timezones ? { width: timezones.length * 4.25 + 'em' } : UNDEFINED;
              this._timezones = UNDEFINED;
              if (timezones) {
                  var tz = [];
                  for (var _i = 0, timezones_1 = timezones; _i < timezones_1.length; _i++) {
                      var t = timezones_1[_i];
                      var tzProps = void 0;
                      if (isString(t)) {
                          var d = createDate(s, 1970, 0, 1);
                          if (isMBSCDate(d)) {
                              d.setTimezone(t);
                          }
                          var offset = (d.getTimezoneOffset() / 60) * -1;
                          tzProps = {
                              label: 'UTC' + (offset > 0 ? '+' : '') + offset,
                              timezone: t,
                          };
                      }
                      else {
                          tzProps = t;
                      }
                      tz.push(tzProps);
                  }
                  this._timezones = tz;
              }
          }
          this._largeDayNames = state.dayNameWidth > 99;
          this._startCellStyle =
              startMinutes % stepCell !== 0
                  ? {
                      height: (state.cellHeight || 50) * (((stepCell - startMinutes) % stepCell) / stepCell) + 'px',
                  }
                  : UNDEFINED;
          this._endCellStyle =
              endMinutes % stepCell !== 0
                  ? {
                      height: ((state.cellHeight || 50) * (endMinutes % stepCell)) / stepCell + 'px',
                  }
                  : UNDEFINED;
      };
      return SchedulerBase;
  }(STBase));

  function template$9(s, state, inst) {
      var _a;
      var colors = inst._colors;
      var dragData = state.dragData;
      var draggedEventId = dragData && dragData.draggedEvent && dragData.draggedEvent.id;
      var events = inst._events;
      var invalids = inst._invalids;
      var hb = inst._hb;
      var rtl = inst._rtl;
      var times = inst._times;
      var startTime = inst._startTime;
      var endTime = inst._endTime;
      var startCellStyle = inst._startCellStyle;
      var endCellStyle = inst._endCellStyle;
      var stepLabel = inst._stepLabel;
      var theme = inst._theme;
      var isSingleResource = inst._isSingleResource;
      var eventMap = s.eventMap || {};
      var source = 'schedule';
      var groupClass = ' mbsc-flex-1-0 mbsc-schedule-resource-group' + theme + rtl;
      var timezones = inst._timezones;
      var groupByResource = inst._groupByResource;
      var days = inst._days;
      var resources = inst._resources;
      var handlers = (_a = {}, _a[ON_MOUSE_MOVE] = inst._onMouseMove, _a[ON_MOUSE_LEAVE] = inst._onMouseLeave, _a);
      var weekDayProps = {
          dayNames: inst._dayNames,
          largeNames: inst._largeDayNames,
          onClick: s.onWeekDayClick,
          renderDay: s.renderDay,
          renderDayContent: s.renderDayContent,
          rtl: s.rtl,
          theme: s.theme,
      };
      var renderResource = function (resource) {
          var content = resource.name;
          var html;
          if (s.renderResource) {
              content = s.renderResource(resource);
              if (isString(content)) {
                  html = inst._safeHtml(content);
                  inst._shouldEnhance = true;
              }
          }
          return (content && (createElement("div", { key: resource.id, className: 'mbsc-schedule-resource' +
                  theme +
                  rtl +
                  hb +
                  (!groupByResource || (s.type === 'day' && s.size === 1) ? ' mbsc-flex-1-0 mbsc-schedule-col-width' : '') },
              createElement("div", { dangerouslySetInnerHTML: html, className: "mbsc-schedule-resource-title" }, content))));
      };
      var renderEvents = function (data, dateKey, resource, allDay) {
          var resourceResize = inst._resourcesMap[resource].eventResize;
          var dragKey = resource + '__' + dateKey;
          var dragResize = computeEventResize(dragData && dragData.draggedEvent && dragData.draggedEvent.original.resize, s.dragToResize, resourceResize);
          var eventProps = {
              displayTimezone: s.displayTimezone,
              drag: s.dragToMove || s.externalDrag,
              endDay: s.endDay,
              exclusiveEndDates: s.exclusiveEndDates,
              gridEndTime: endTime,
              gridStartTime: startTime,
              lastDay: +inst._lastDay,
              render: s.renderEvent,
              renderContent: s.renderEventContent,
              resource: resource,
              rtl: s.rtl,
              singleDay: !groupByResource,
              slot: DEF_ID,
              startDay: s.startDay,
              theme: s.theme,
              timezonePlugin: s.timezonePlugin,
          };
          return (createElement(Fragment, null,
              data.map(function (event) {
                  return event.showText ? (createElement(ScheduleEvent, __assign({}, eventProps, { event: event, key: event.uid, inactive: draggedEventId === event.id, resize: computeEventResize(event.original.resize, s.dragToResize, resourceResize), selected: !!(s.selectedEventsMap[event.uid] || s.selectedEventsMap[event.id]), onClick: s.onEventClick, onDoubleClick: s.onEventDoubleClick, onRightClick: s.onEventRightClick, onDelete: s.onEventDelete, onHoverIn: s.onEventHoverIn, onHoverOut: s.onEventHoverOut, onDragStart: inst._onEventDragStart, onDragMove: inst._onEventDragMove, onDragEnd: inst._onEventDragEnd, onDragModeOn: inst._onEventDragModeOn, onDragModeOff: inst._onEventDragModeOff }))) : (createElement("div", { key: event.uid, className: "mbsc-schedule-event mbsc-schedule-event-all-day mbsc-schedule-event-all-day-placeholder" },
                      createElement("div", { className: 'mbsc-schedule-event-all-day-inner' + theme })));
              }),
              dragData && dragData.originDates && dragData.originDates[dragKey] && !!dragData.originDates[dragKey].allDay === !!allDay && (createElement(ScheduleEvent, __assign({}, eventProps, { event: dragData.originDates[dragKey], hidden: dragData && !!dragData.draggedDates, isDrag: true, resize: dragResize, onDragStart: inst._onEventDragStart, onDragMove: inst._onEventDragMove, onDragEnd: inst._onEventDragEnd, onDragModeOff: inst._onEventDragModeOff }))),
              dragData && dragData.draggedDates && dragData.draggedDates[dragKey] && !!dragData.draggedDates[dragKey].allDay === !!allDay && (createElement(ScheduleEvent, __assign({}, eventProps, { event: dragData.draggedDates[dragKey], isDrag: true, resize: dragResize })))));
      };
      var renderTimes = function (timezone) {
          return times.map(function (v, i) {
              var first = !i;
              var last = i === times.length - 1;
              return (createElement("div", { key: i, className: 'mbsc-flex-col mbsc-flex-1-0 mbsc-schedule-time-wrapper' +
                      theme +
                      rtl +
                      (last ? ' mbsc-schedule-time-wrapper-end' : '') +
                      ((first && !last && startCellStyle) || (last && !first && endCellStyle) ? ' mbsc-flex-none' : ''), style: first && !last ? startCellStyle : last && !first ? endCellStyle : UNDEFINED },
                  createElement("div", { className: 'mbsc-flex-1-1 mbsc-schedule-time' + theme + rtl }, first || v % stepLabel === 0 ? inst._formatTime(first ? startTime : v, timezone) : ''),
                  inst._timesBetween.map(function (t, j) {
                      var ms = v + (j + 1) * stepLabel;
                      return (ms > startTime &&
                          ms < endTime && (createElement("div", { key: j, className: 'mbsc-flex-1-1 mbsc-schedule-time' + theme + rtl }, inst._formatTime(ms, timezone))));
                  }),
                  last && (createElement("div", { className: 'mbsc-schedule-time mbsc-schedule-time-end' + theme + rtl }, inst._formatTime(endTime + 1, timezone)))));
          });
      };
      var renderAllDayData = function (resource, dateKey, i, timestamp) {
          var invalid = invalids[resource][DEF_ID][dateKey] && invalids[resource][DEF_ID][dateKey].allDay;
          var color = colors[resource][DEF_ID][dateKey] && colors[resource][DEF_ID][dateKey].allDay;
          var dayEvents = events[resource][DEF_ID][dateKey] && events[resource][DEF_ID][dateKey].allDay;
          return (createElement("div", { key: i + '-' + timestamp, className: 'mbsc-schedule-all-day-item mbsc-schedule-col-width mbsc-flex-1-0' + theme + rtl + hb },
              renderEvents(dayEvents || [], dateKey, resource, true),
              invalid && (createElement("div", { className: 'mbsc-schedule-invalid mbsc-schedule-invalid-all-day' + invalid.cssClass + theme },
                  createElement("div", { className: "mbsc-schedule-invalid-text" }, invalid.title))),
              color && (createElement("div", { className: 'mbsc-schedule-color mbsc-schedule-color-all-day' + color.cssClass + theme, style: color.position },
                  createElement("div", { className: "mbsc-schedule-color-text" }, color.title)))));
      };
      var renderDayData = function (resource, dateKey, i, timestamp) {
          var dayInvalids = invalids[resource][DEF_ID][dateKey] && invalids[resource][DEF_ID][dateKey].invalids;
          var dayColors = colors[resource][DEF_ID][dateKey] && colors[resource][DEF_ID][dateKey].colors;
          var dayEvents = events[resource][DEF_ID][dateKey] && events[resource][DEF_ID][dateKey].events;
          return (createElement("div", { key: i + '-' + timestamp, className: 'mbsc-flex-col mbsc-flex-1-0 mbsc-schedule-column mbsc-schedule-col-width' + theme + rtl + hb },
              createElement("div", { className: 'mbsc-flex-col mbsc-flex-1-1 mbsc-schedule-column-inner' + theme + rtl + hb },
                  createElement("div", { className: 'mbsc-schedule-events' + rtl }, renderEvents(dayEvents || [], dateKey, resource)),
                  dayInvalids &&
                      dayInvalids.map(function (invalid, j) {
                          return (invalid.position && (createElement("div", { key: j, className: 'mbsc-schedule-invalid' + invalid.cssClass + theme, style: invalid.position },
                              createElement("div", { className: "mbsc-schedule-invalid-text" }, invalid.allDay ? '' : invalid.title || ''))));
                      }),
                  dayColors &&
                      dayColors.map(function (color, j) {
                          return (createElement("div", { key: j, className: 'mbsc-schedule-color' + color.cssClass + theme, style: color.position },
                              createElement("div", { className: "mbsc-schedule-color-text" }, color.title)));
                      }),
                  times.map(function (v, j) {
                      var _a;
                      var date = getCellDate(timestamp, v);
                      var first = !j;
                      var last = j === times.length - 1;
                      var cellHandlers = (_a = {},
                          _a[ON_DOUBLE_CLICK] = function (domEvent) { return s.onCellDoubleClick({ date: date, domEvent: domEvent, resource: resource, source: source }); },
                          _a[ON_CONTEXT_MENU] = function (domEvent) { return s.onCellRightClick({ date: date, domEvent: domEvent, resource: resource, source: source }); },
                          _a);
                      return (createElement("div", __assign({ key: j, className: 'mbsc-schedule-item mbsc-flex-1-0' +
                              theme +
                              hb +
                              (last ? ' mbsc-schedule-item-last' : '') +
                              ((first && !last && startCellStyle) || (last && !first && endCellStyle) ? ' mbsc-flex-none' : ''), 
                          // tslint:disable-next-line: jsx-no-lambda
                          onClick: function (domEvent) { return s.onCellClick({ date: date, domEvent: domEvent, resource: resource, source: source }); }, style: first && !last ? startCellStyle : last && !first ? endCellStyle : UNDEFINED }, cellHandlers)));
                  }))));
      };
      return (createElement("div", { ref: inst._setEl, className: 'mbsc-flex-col mbsc-flex-1-1 mbsc-schedule-wrapper' + theme + (inst._daysNr > 7 ? ' mbsc-schedule-wrapper-multi' : '') },
          createElement("div", { className: 'mbsc-schedule-header mbsc-flex mbsc-flex-none' + theme + hb },
              createElement("div", { className: 'mbsc-schedule-time-col mbsc-schedule-time-col-empty' + theme + rtl + hb, style: inst._timeWidth }),
              createElement("div", { className: "mbsc-flex-1-1 mbsc-schedule-header-wrapper" },
                  createElement("div", { ref: inst._setHeaderCont, className: "mbsc-flex" }, s.type === 'day' && s.size === 1 ? (createElement("div", { className: groupClass },
                      createElement("div", { className: "mbsc-flex" }, s.showDays &&
                          inst._headerDays.map(function (dayData) {
                              var timestamp = dayData.timestamp;
                              return (createElement(WeekDay, __assign({}, weekDayProps, { key: timestamp, cssClass: "mbsc-flex-1-1", day: dayData.day, events: eventMap[dayData.dateKey], isToday: inst._isToday(timestamp), label: dayData.label, selectable: true, selected: inst._selectedDay === timestamp, timestamp: timestamp })));
                          })),
                      s.resources && createElement("div", { className: "mbsc-flex" }, resources.map(renderResource)))) : groupByResource ? (resources.map(function (resource, i) {
                      return (createElement("div", { key: i, className: groupClass },
                          renderResource(resource),
                          createElement("div", { className: "mbsc-flex" }, s.showDays &&
                              days.map(function (dayData) {
                                  var timestamp = dayData.timestamp;
                                  return (createElement(WeekDay, __assign({}, weekDayProps, { key: timestamp, cssClass: "mbsc-flex-1-0 mbsc-schedule-col-width", day: dayData.day, events: eventMap[dayData.dateKey], isToday: isSingleResource && inst._isToday(timestamp), label: dayData.label, resource: resource.id, selectable: false, selected: isSingleResource && inst._isToday(timestamp), timestamp: timestamp })));
                              }))));
                  })) : (days.map(function (dayData, i) {
                      var timestamp = dayData.timestamp;
                      return (createElement("div", { key: i, className: groupClass },
                          s.showDays && (createElement(WeekDay, __assign({}, weekDayProps, { key: timestamp, day: dayData.day, events: eventMap[dayData.dateKey], isToday: isSingleResource && inst._isToday(timestamp), label: dayData.label, selectable: false, selected: inst._isToday(timestamp), timestamp: timestamp }))),
                          s.resources && createElement("div", { className: "mbsc-flex" }, resources.map(renderResource))));
                  })))),
              createElement("div", { className: "mbsc-schedule-fake-scroll-y" })),
          createElement("div", { className: 'mbsc-schedule-all-day-cont' + (state.showShadow ? ' mbsc-schedule-all-day-wrapper-shadow' : '') + theme },
              timezones && (createElement("div", { className: "mbsc-flex mbsc-schedule-timezone-labels", style: inst._timeWidth }, timezones.map(function (tz, i) {
                  return (createElement("div", { key: i, className: 'mbsc-flex-1-0-0 mbsc-schedule-timezone-label' + theme + rtl }, tz.label));
              }))),
              s.showAllDay && (createElement("div", { className: 'mbsc-schedule-all-day-wrapper mbsc-flex-none' + theme + hb },
                  createElement("div", { className: 'mbsc-flex mbsc-schedule-all-day' + theme },
                      createElement("div", { className: 'mbsc-schedule-time-col' + theme + rtl, style: inst._timeWidth }, !timezones && createElement("div", { className: 'mbsc-schedule-all-day-text' + theme + rtl }, s.allDayText)),
                      createElement("div", { className: "mbsc-flex-col mbsc-flex-1-1 mbsc-schedule-all-day-group-wrapper" },
                          createElement("div", { ref: inst._setAllDayCont, className: "mbsc-flex mbsc-flex-1-1" }, groupByResource
                              ? resources.map(function (resource, i) {
                                  return (createElement("div", { key: i, className: 'mbsc-flex' + groupClass }, days.map(function (day, j) {
                                      return renderAllDayData(resource.id, day.dateKey, j, day.timestamp);
                                  })));
                              })
                              : days.map(function (day, i) {
                                  return (createElement("div", { key: i, className: 'mbsc-flex' + groupClass }, resources.map(function (resource, j) {
                                      return renderAllDayData(resource.id, day.dateKey, j, day.timestamp);
                                  })));
                              }))))))),
          createElement("div", { className: 'mbsc-flex mbsc-flex-1-1 mbsc-schedule-grid-wrapper' + theme },
              createElement("div", { "aria-hidden": "true", className: 'mbsc-flex-col mbsc-schedule-time-col mbsc-schedule-time-cont' + theme + rtl, style: inst._timeWidth, ref: inst._setTimeCont },
                  createElement("div", { className: "mbsc-flex mbsc-schedule-time-cont-inner" },
                      createElement("div", { className: "mbsc-flex-col mbsc-flex-1-1" },
                          createElement("div", { className: 'mbsc-flex-1-1 mbsc-schedule-time-cont-pos' +
                                  theme +
                                  (timezones ? ' mbsc-flex' : ' mbsc-flex-col mbsc-schedule-time-col-last') },
                              timezones
                                  ? timezones.map(function (tz, i) {
                                      return (createElement("div", { key: i, className: 'mbsc-flex-col' + theme + (i === timezones.length - 1 ? ' mbsc-schedule-time-col-last' : '') }, renderTimes(tz.timezone)));
                                  })
                                  : renderTimes(),
                              inst._showTimeIndicator && (createElement(TimeIndicator, { amText: s.amText, displayedTime: inst._time, displayedDays: inst._daysNr, displayTimezone: s.displayTimezone, endDay: s.endDay, firstDay: inst._firstDayTz, orientation: "x", pmText: s.pmText, rtl: s.rtl, showDayIndicator: isSingleResource && !inst._isMulti && s.type === 'week', startDay: s.startDay, startTime: startTime, theme: s.theme, timeFormat: s.timeFormat, timezones: timezones, timezonePlugin: s.timezonePlugin })),
                              inst._showCursorTime && (createElement("div", { ref: inst._setCursorTimeCont, className: 'mbsc-schedule-cursor-time mbsc-schedule-cursor-time-x' + theme + rtl }))),
                          state.hasScrollX && createElement("div", { className: "mbsc-schedule-fake-scroll-x" })),
                      createElement("div", { className: "mbsc-schedule-fake-scroll-y" }))),
              createElement("div", { ref: inst._setCont, className: 'mbsc-flex-col mbsc-flex-1-1 mbsc-schedule-grid-scroll' + theme, onScroll: inst._onScroll },
                  createElement("div", { className: "mbsc-flex mbsc-flex-1-1" },
                      createElement("div", __assign({ className: "mbsc-flex mbsc-flex-1-0 mbsc-schedule-grid", ref: inst._setGridCont }, handlers), groupByResource
                          ? resources.map(function (resource, i) {
                              return (createElement("div", { key: i, className: 'mbsc-flex' + groupClass }, days.map(function (day, j) {
                                  return renderDayData(resource.id, day.dateKey, j, day.timestamp);
                              })));
                          })
                          : days.map(function (day, i) {
                              return (createElement("div", { key: i, className: 'mbsc-flex' + groupClass }, resources.map(function (resource, j) {
                                  return renderDayData(resource.id, day.dateKey, j, day.timestamp);
                              })));
                          }))))),
          dragData && !state.isTouchDrag && createElement("div", { className: "mbsc-calendar-dragging" })));
  }
  var Scheduler = /*#__PURE__*/ (function (_super) {
      __extends(Scheduler, _super);
      function Scheduler() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      Scheduler.prototype._template = function (s, state) {
          return template$9(s, state, this);
      };
      return Scheduler;
  }(SchedulerBase));

  /** @hidden */
  var TimelineBase = /*#__PURE__*/ (function (_super) {
      __extends(TimelineBase, _super);
      function TimelineBase() {
          var _this = _super !== null && _super.apply(this, arguments) || this;
          // tslint:disable variable-name
          _this._isTimeline = true;
          // tslint:enable variable-name
          // tslint:disable-next-line: variable-name
          _this._onScroll = function () {
              var s = _this.s;
              var isRtl = s.rtl;
              var state = _this.state;
              var gridWidth = _this._gridWidth;
              var scrollCont = _this._scrollCont;
              var scrollTop = scrollCont.scrollTop;
              var scrollLeft = scrollCont.scrollLeft;
              var resCont = _this._resCont;
              var sidebarCont = _this._sidebarCont;
              var footer = _this._footerCont;
              var header = _this._headerCont;
              var stickyHeader = _this._stickyHeader;
              var stickyFooter = _this._stickyFooter;
              var colsNr = _this._cols.length; // this._daysNr;
              var rtl = isRtl ? -1 : 1;
              var margin = isRtl ? 'marginRight' : 'marginLeft';
              var batchIndexX = round((scrollLeft * rtl * (colsNr / _this._daysBatchNr)) / gridWidth);
              var dayIndex = 0;
              // Vertical virtual page index
              var virtualPagesY = _this._virtualPagesY || [];
              var batchIndexY = 0;
              var i = 0;
              while (i < virtualPagesY.length && virtualPagesY[i].top - state.scrollContHeight / 2 <= scrollTop) {
                  batchIndexY = i;
                  i++;
              }
              // RTL issue https://bugs.chromium.org/p/chromium/issues/detail?id=1140374
              if (!hasSticky || isRtl) {
                  if (resCont) {
                      resCont.scrollTop = scrollTop;
                  }
                  if (sidebarCont) {
                      sidebarCont.scrollTop = scrollTop;
                  }
              }
              if (stickyHeader && hasSticky) {
                  // Update the sticky header position to handle scroll bounce on touch devices
                  var headerStyle = stickyHeader.style;
                  headerStyle.marginTop = scrollTop < 0 ? -scrollTop + 'px' : '';
                  headerStyle[margin] = scrollLeft * rtl < 0 ? -scrollLeft * rtl + 'px' : '';
              }
              if (stickyFooter && hasSticky) {
                  // Update the sticky footer position to handle scroll bounce on touch devices
                  var footerStyle = stickyFooter.style;
                  footerStyle.marginTop = scrollTop < 0 ? -scrollTop + 'px' : '';
                  footerStyle[margin] = scrollLeft * rtl < 0 ? -scrollLeft * rtl + 'px' : '';
              }
              if (!gridWidth) {
                  return;
              }
              if ((header || footer) && _this._isDailyResolution) {
                  var days_1 = _this._days;
                  var dayWidth_1 = gridWidth / colsNr;
                  dayIndex = constrain(floor((scrollLeft * rtl) / dayWidth_1), 0, colsNr - 1);
                  var updateStickyLabel = function (label, key) {
                      if (label && dayWidth_1) {
                          var labelWidth = label.offsetWidth;
                          var labelStyle = label.style;
                          var nextDayIndex = constrain(floor((scrollLeft * rtl + labelWidth) / dayWidth_1), 0, colsNr - 1);
                          if (days_1[dayIndex][key + 'Index'] !== days_1[nextDayIndex][key + 'Index']) {
                              labelStyle[margin] = -(scrollLeft * rtl + labelWidth - days_1[nextDayIndex][key + 'Index'] * dayWidth_1 + 1) + 'px';
                          }
                          else {
                              labelStyle[margin] = '';
                          }
                      }
                  };
                  updateStickyLabel(_this._stickyDate, 'date');
                  updateStickyLabel(_this._stickyMonth, 'month');
                  updateStickyLabel(_this._stickyWeek, 'week');
                  if (!hasSticky) {
                      if (footer) {
                          footer.scrollLeft = scrollLeft;
                      }
                      if (header) {
                          header.scrollLeft = scrollLeft;
                      }
                  }
              }
              if (batchIndexX !== state.batchIndexX ||
                  batchIndexY !== state.batchIndexY ||
                  ((_this._stickyDate || _this._stickyMonth || _this._stickyWeek) && dayIndex !== state.dayIndex)) {
                  _this.setState({ batchIndexX: batchIndexX, batchIndexY: batchIndexY, dayIndex: dayIndex });
              }
              clearTimeout(_this._scrollDebounce);
              _this._scrollDebounce = setTimeout(function () {
                  if (!_this._isScrolling && !_this._viewChanged && !_this._hasResY) {
                      var time = (scrollLeft * _this._time * _this._daysNr) / gridWidth;
                      _this._hook('onActiveChange', { date: new Date(+_this._firstDay + time), scroll: true });
                  }
              }, 100);
              _this._onMouseMove();
          };
          // tslint:disable-next-line: variable-name
          _this._setStickyHeader = function (el) {
              _this._stickyHeader = el;
          };
          // tslint:disable-next-line: variable-name
          _this._setStickyFooter = function (el) {
              _this._stickyFooter = el;
          };
          // tslint:disable-next-line: variable-name
          _this._setStickyDay = function (el) {
              _this._stickyDate = el;
          };
          // tslint:disable-next-line: variable-name
          _this._setStickyMonth = function (el) {
              _this._stickyMonth = el;
          };
          // tslint:disable-next-line: variable-name
          _this._setStickyWeek = function (el) {
              _this._stickyWeek = el;
          };
          // tslint:disable-next-line: variable-name
          _this._setCont = function (el) {
              _this._scrollCont = el;
          };
          // tslint:disable-next-line: variable-name
          _this._setResCont = function (el) {
              _this._resCont = el;
          };
          // tslint:disable-next-line: variable-name
          _this._setSidebarCont = function (el) {
              _this._sidebarCont = el;
          };
          // tslint:disable-next-line: variable-name
          _this._setGridCont = function (el) {
              _this._gridCont = el;
          };
          // tslint:disable-next-line: variable-name
          _this._setHeaderCont = function (el) {
              _this._headerCont = el;
          };
          // tslint:disable-next-line: variable-name
          _this._setFooterCont = function (el) {
              _this._footerCont = el;
          };
          // tslint:disable-next-line: variable-name
          _this._setCursorTimeCont = function (el) {
              _this._cursorTimeCont = el;
          };
          return _this;
      }
      // tslint:disable-next-line: variable-name
      TimelineBase.prototype._onParentClick = function (domEvent, resource) {
          resource.collapsed = !resource.collapsed;
          this._hook(resource.collapsed ? 'onResourceCollapse' : 'onResourceExpand', { domEvent: domEvent, resource: resource.id });
          this._visibleResources = this._flattenResources(this.s.resources, [], 0);
          this._shouldCheckSize = true;
          this._isParentClick = true;
          this.forceUpdate();
      };
      TimelineBase.prototype._render = function (s, state) {
          _super.prototype._render.call(this, s, state);
          clearTimeout(this._scrollDebounce);
          var prevS = this._prevS;
          var eventMap = this._eventMap;
          var resourceTops = this._resourceTops;
          var stepCell = this._stepCell / ONE_MIN;
          var startMinutes = floor(this._startTime / ONE_MIN) % stepCell;
          var endMinutes = (floor(this._endTime / ONE_MIN) % stepCell) + 1;
          this._stickyDay = this._days[state.dayIndex || 0] || this._days[0];
          this._startCellStyle =
              startMinutes % stepCell !== 0
                  ? {
                      width: (state.cellWidth || 64) * (((stepCell - startMinutes) % stepCell) / stepCell) + 'px',
                  }
                  : UNDEFINED;
          this._endCellStyle =
              endMinutes % stepCell !== 0
                  ? {
                      width: ((state.cellWidth || 64) * (endMinutes % stepCell)) / stepCell + 'px',
                  }
                  : UNDEFINED;
          if (s.connections !== prevS.connections || s.eventMap !== prevS.eventMap || s.theme !== prevS.theme || s.rtl !== prevS.rtl) {
              this._calcConnections = true;
          }
          if (this._hasSlots) {
              this._connections = UNDEFINED;
          }
          if (this._calcConnections && !this._hasSlots && !this._shouldCheckSize && resourceTops) {
              var connections = [];
              var eventHeight = this._eventHeight;
              var gridWidth = this._gridWidth;
              var gridHeight = state.hasScrollY ? this._gridHeight : state.scrollContHeight - state.headerHeight;
              var constLineH = 1500 / gridWidth; // 15 px converted to percent - horizontal
              var isRtl = s.rtl === true;
              var rtl = isRtl ? -1 : 1;
              var arrowH = (750 / gridWidth) * rtl;
              var arrowV = (400 / gridHeight) * rtl;
              var eventHeightInPercent = (100 * eventHeight) / gridHeight;
              for (var _i = 0, _a = s.connections || []; _i < _a.length; _i++) {
                  var connection = _a[_i];
                  var fromEvent = eventMap[connection.from];
                  var toEvent = eventMap[connection.to];
                  var arrow = connection.arrow;
                  var color = connection.color;
                  var cssClass = connection.cssClass || '';
                  var id = connection.from + '__' + connection.to;
                  if (fromEvent && toEvent) {
                      var fromPos = fromEvent.position;
                      var toPos = toEvent.position;
                      var hasFromPos = fromPos.width !== UNDEFINED;
                      var hasToPos = toPos.width !== UNDEFINED;
                      var fromResource = fromEvent.resource;
                      var toResource = toEvent.resource;
                      // one of the to/from positions should be calculated (present on the view)
                      if ((hasFromPos || hasToPos) && resourceTops[fromResource] >= 0 && resourceTops[toResource] >= 0) {
                          var fromEnd = fromEvent.endDate;
                          var toStart = toEvent.startDate;
                          var isToBefore = toStart < fromEnd;
                          var startDate = isToBefore ? toStart : fromEnd;
                          var endDate = isToBefore ? fromEnd : toStart;
                          var fromTop = fromPos.top || 0;
                          var toTop = toPos.top || 0;
                          var positionProp = isRtl ? 'right' : 'left';
                          var fromLeft = hasFromPos ? +fromPos[positionProp].replace('%', '') : isToBefore ? 100 : 0;
                          var toLeft = hasToPos ? +toPos[positionProp].replace('%', '') : isToBefore ? 0 : 100;
                          var fromWidth = hasFromPos ? +fromPos.width.replace('%', '') : 0;
                          var isSameResLine = fromEvent.resource === toEvent.resource && isToBefore && toTop === fromTop;
                          var lineWidth = toLeft - fromLeft - fromWidth - 2 * constLineH;
                          var resourceTopsDiff = resourceTops[toResource] - resourceTops[fromResource];
                          var toUpperResource = resourceTopsDiff < 0 || (!resourceTopsDiff && toTop < fromTop) ? -1 : 1;
                          var lineHeight = (100 *
                              (resourceTopsDiff -
                                  fromTop * eventHeight + // - fromEvent top position
                                  toTop * eventHeight + // + toEvent top position
                                  (isSameResLine ? eventHeight : 0))) /
                              gridHeight;
                          var posX = (isRtl ? 100 - fromLeft : fromLeft) + fromWidth * rtl;
                          var posY = (100 * (resourceTops[fromResource] + fromTop * eventHeight + 3 + eventHeight / 2)) / gridHeight;
                          if (hasFromPos && (arrow === 'from' || arrow === 'bidirectional')) {
                              connections.push({
                                  color: color,
                                  cssClass: 'mbsc-connection-arrow ' + cssClass,
                                  endDate: endDate,
                                  fill: color,
                                  id: id + '__start',
                                  pathD: "M " + posX + ", " + posY + " L " + (posX + arrowH) + " " + (posY - arrowV) + " L " + (posX + arrowH) + " " + (posY + arrowV) + " Z",
                                  startDate: startDate,
                              });
                          }
                          // set the starting position
                          var pathD = "M " + posX + ", " + posY;
                          // adding the starting line
                          posX += constLineH * rtl;
                          // adding vertical line if there is one
                          if (lineHeight) {
                              pathD += " H " + posX;
                              posY += lineHeight - (lineWidth < 0 ? eventHeightInPercent / 2 : 0) * toUpperResource;
                              pathD += " V " + posY;
                          }
                          // adding the horizontal line that connects the two events.
                          posX += lineWidth * rtl;
                          if (lineHeight) {
                              pathD += " H " + posX;
                          }
                          // in case of the toEvents took place before the fromEvent add the second vertical section
                          if (lineHeight && lineWidth < 0) {
                              posY += (eventHeightInPercent / 2) * toUpperResource * (isSameResLine ? -1 : 1);
                              pathD += " V " + posY;
                          }
                          // adding the ending line
                          posX += constLineH * rtl;
                          pathD += " H " + posX;
                          connections.push({ color: color, cssClass: cssClass, id: id, pathD: pathD, startDate: startDate, endDate: endDate });
                          if (hasToPos && (arrow === 'to' || arrow === 'bidirectional' || arrow === true)) {
                              connections.push({
                                  color: color,
                                  cssClass: 'mbsc-connection-arrow ' + cssClass,
                                  endDate: endDate,
                                  fill: color,
                                  id: id + '__end',
                                  pathD: "M " + posX + ", " + posY + " L " + (posX - arrowH) + " " + (posY - arrowV) + " L " + (posX - arrowH) + " " + (posY + arrowV) + " Z",
                                  startDate: startDate,
                              });
                          }
                      }
                  }
              }
              this._connections = connections;
              this._calcConnections = false;
          }
      };
      return TimelineBase;
  }(STBase));

  function template$a(s, state, inst) {
      var _a, _b;
      var dragData = state.dragData;
      var draggedEventId = dragData && dragData.draggedEvent && dragData.draggedEvent.id;
      var hasSlots = inst._hasSlots;
      var hb = inst._hb;
      var rtl = inst._rtl;
      var times = inst._times;
      var theme = inst._theme;
      var startTime = inst._startTime;
      var endTime = inst._endTime;
      var stepLabel = inst._stepLabel;
      var slots = inst._slots;
      var source = 'timeline';
      var isListing = s.eventList;
      var isMonthView = s.type === 'month';
      var isHourly = inst._stepCell < ONE_DAY;
      var startCellStyle = inst._startCellStyle;
      var endCellStyle = inst._endCellStyle;
      var daysBatch = inst._daysBatch;
      var headerHeight = { height: state.headerHeight + 'px' };
      var footerHeight = { height: state.footerHeight + 'px' };
      var days = inst._days;
      var daysNr = inst._daysNr;
      var dayIndex = state.dayIndex || 0;
      var isDailyResolution = inst._isDailyResolution;
      var hasResY = inst._hasResY;
      var hasResources = inst._hasResources;
      var hasFooter = s.renderHourFooter || s.renderDayFooter || s.renderWeekFooter || s.renderMonthFooter || s.renderYearFooter;
      var hasRows = inst._hasRows;
      var colClass = inst._colClass;
      var svgProps = (_a = {}, _a['className'] = 'mbsc-connections' + theme, _a);
      var handlers = (_b = {},
          _b[ON_MOUSE_MOVE] = inst._onMouseMove,
          _b[ON_MOUSE_LEAVE] = inst._onMouseLeave,
          _b);
      var renderSlot = function (args) {
          var slot = args.slot;
          var content = slot.name;
          var html;
          if (s.renderSlot) {
              content = s.renderSlot(args);
              if (isString(content)) {
                  html = inst._safeHtml(content);
                  inst._shouldEnhance = true;
              }
          }
          // eslint-disable-next-line react/no-danger-with-children
          return (createElement("div", { key: slot.id, className: "mbsc-timeline-slot-title", dangerouslySetInnerHTML: html }, content));
      };
      var renderHeaderResolution = function (day) {
          switch (s.resolution) {
              case 'week':
                  if (s.renderWeek) {
                      return renderWeek(day, false);
                  }
                  break;
              case 'month':
                  if (s.renderMonth) {
                      return renderMonth(day, false);
                  }
                  break;
              case 'year':
                  if (s.renderYear) {
                      return renderYear(day);
                  }
                  break;
          }
          return day.columnTitle;
      };
      var renderFooterResolution = function (day) {
          switch (s.resolution) {
              case 'week':
                  if (s.renderWeekFooter) {
                      return renderWeekFooter(day);
                  }
                  break;
              case 'month':
                  if (s.renderMonthFooter) {
                      return renderMonthFooter(day);
                  }
                  break;
              case 'year':
                  if (s.renderYearFooter) {
                      return renderYearFooter(day);
                  }
                  break;
          }
          return;
      };
      var renderHour = function (hour, timestamp) {
          var content;
          var html;
          if (inst._displayTime && inst._timeLabels[timestamp]) {
              if (s.renderHour) {
                  var ms = +hour.date + timestamp;
                  content = s.renderHour({
                      date: new Date(ms),
                      events: hour.eventMap[ms] || [],
                      isActive: hour.isActive,
                  });
                  if (isString(content)) {
                      html = inst._safeHtml(content);
                      inst._shouldEnhance = true;
                  }
              }
              else {
                  content = inst._timeLabels[timestamp];
              }
          }
          return (createElement("div", { key: timestamp, "aria-hidden": "true", className: 'mbsc-timeline-header-time mbsc-flex-1-1' + theme, dangerouslySetInnerHTML: html }, content));
      };
      var renderHourFooter = function (hour, timestamp) {
          var content;
          var html;
          if (s.renderHourFooter && inst._displayTime && inst._timeLabels[timestamp]) {
              var ms = +hour.date + timestamp;
              content = s.renderHourFooter({
                  date: new Date(ms),
                  events: hour.eventMap[ms] || [],
                  isActive: hour.isActive,
              });
              if (isString(content)) {
                  html = inst._safeHtml(content);
                  inst._shouldEnhance = true;
              }
          }
          return (createElement("div", { key: timestamp, className: 'mbsc-timeline-footer-time mbsc-flex-1-1 ' + theme, dangerouslySetInnerHTML: html }, content));
      };
      var renderDay = function (day, sticky) {
          var content;
          var html;
          if (s.renderDay) {
              content = s.renderDay({
                  date: day.date,
                  events: day.eventMap[day.timestamp] || [],
                  isActive: day.isActive,
              });
              if (isString(content)) {
                  html = inst._safeHtml(content);
                  inst._shouldEnhance = true;
              }
          }
          else {
              content = day.dateText;
          }
          return (createElement("div", { ref: sticky ? inst._setStickyDay : UNDEFINED, "aria-hidden": "true", dangerouslySetInnerHTML: html, className: (sticky ? 'mbsc-timeline-header-text' : '') +
                  (day.isActive && !s.renderDay ? ' mbsc-timeline-header-active' : '') +
                  (s.renderDay ? ' mbsc-timeline-header-date-cont' : ' mbsc-timeline-header-date-text') +
                  theme }, content));
      };
      var renderDayFooter = function (day) {
          var content;
          var html;
          if (s.renderDayFooter) {
              content = s.renderDayFooter({
                  date: day.date,
                  events: day.eventMap[day.timestamp] || [],
              });
              if (isString(content)) {
                  html = inst._safeHtml(content);
                  inst._shouldEnhance = true;
              }
          }
          return (createElement("div", { className: "mbsc-timeline-footer-date-cont", dangerouslySetInnerHTML: html }, content));
      };
      var renderWeek = function (week, sticky) {
          var content;
          var html;
          if (s.renderWeek) {
              content = s.renderWeek({
                  date: week.date,
                  endDate: week.endDate,
                  events: week.eventMap[week.timestamp] || [],
                  isActive: week.isActive,
                  startDate: week.date,
                  weekNr: week.weekNr,
              });
              if (isString(content)) {
                  html = inst._safeHtml(content);
                  inst._shouldEnhance = true;
              }
          }
          else {
              content = week.weekText;
          }
          return (createElement("div", { ref: sticky ? inst._setStickyWeek : UNDEFINED, "aria-hidden": "true", dangerouslySetInnerHTML: html, className: (sticky ? 'mbsc-timeline-header-text' : '') +
                  (s.renderWeek ? ' mbsc-timeline-header-week-cont' : ' mbsc-timeline-header-week-text') +
                  (week.lastOfWeek ? '  mbsc-timeline-header-week-text-last' : '') +
                  theme }, content));
      };
      var renderWeekFooter = function (week) {
          var content;
          var html;
          if (s.renderWeekFooter) {
              content = s.renderWeekFooter({
                  date: week.date,
                  endDate: week.endDate,
                  events: week.eventMap[week.timestamp] || [],
                  isActive: week.isActive,
                  startDate: week.date,
                  weekNr: week.weekNr,
              });
              if (isString(content)) {
                  html = inst._safeHtml(content);
                  inst._shouldEnhance = true;
              }
          }
          return (createElement("div", { dangerouslySetInnerHTML: html, className: "mbsc-timeline-footer-week-cont" }, content));
      };
      var renderMonth = function (month, sticky) {
          var content;
          var html;
          if (s.renderMonth) {
              content = s.renderMonth({
                  date: month.date,
                  events: month.eventMap[month.timestamp] || [],
                  isActive: month.isActive,
              });
              if (isString(content)) {
                  html = inst._safeHtml(content);
                  inst._shouldEnhance = true;
              }
          }
          else {
              content = month.monthText;
          }
          return (createElement("div", { ref: sticky ? inst._setStickyMonth : UNDEFINED, "aria-hidden": "true", dangerouslySetInnerHTML: html, className: (sticky ? 'mbsc-timeline-header-text' : '') +
                  (s.renderMonth ? ' mbsc-timeline-header-month-cont' : ' mbsc-timeline-header-month-text') +
                  (month.lastOfMonth ? ' mbsc-timeline-header-month-text-last' : '') +
                  theme }, content));
      };
      var renderMonthFooter = function (month) {
          var content;
          var html;
          if (s.renderMonthFooter) {
              content = s.renderMonthFooter({
                  date: month.date,
                  events: month.eventMap[month.timestamp] || [],
                  isActive: month.isActive,
              });
              if (isString(content)) {
                  html = inst._safeHtml(content);
                  inst._shouldEnhance = true;
              }
          }
          return (createElement("div", { dangerouslySetInnerHTML: html, className: "mbsc-timeline-footer-month-cont" }, content));
      };
      var renderYear = function (year) {
          var content;
          var html;
          if (s.renderYear) {
              content = s.renderYear({
                  date: year.date,
                  events: year.eventMap[year.timestamp] || [],
                  isActive: year.isActive,
              });
              if (isString(content)) {
                  html = inst._safeHtml(content);
                  inst._shouldEnhance = true;
              }
          }
          else {
              content = year.columnTitle;
          }
          return (createElement("div", { "aria-hidden": "true", dangerouslySetInnerHTML: html, className: (year.isActive && !s.renderYear ? ' mbsc-timeline-header-active' : '') +
                  (s.renderYear ? ' mbsc-timeline-header-year-cont' : ' mbsc-timeline-header-year-text') +
                  theme }, content));
      };
      var renderYearFooter = function (year) {
          var content;
          var html;
          if (s.renderYearFooter) {
              content = s.renderYearFooter({
                  date: year.date,
                  events: year.eventMap[year.timestamp] || [],
                  isActive: year.isActive,
              });
              if (isString(content)) {
                  html = inst._safeHtml(content);
                  inst._shouldEnhance = true;
              }
          }
          return (createElement("div", { dangerouslySetInnerHTML: html, className: "mbsc-timeline-footer-year-cont" }, content));
      };
      var renderResourceHeader = function () {
          var content;
          var html;
          if (s.renderResourceHeader) {
              content = s.renderResourceHeader();
              if (isString(content)) {
                  html = inst._safeHtml(content);
                  inst._shouldEnhance = true;
              }
          }
          return (createElement("div", { className: "mbsc-timeline-resource-header", dangerouslySetInnerHTML: html }, content));
      };
      var renderResourceFooter = function () {
          var content;
          var html;
          if (s.renderResourceFooter) {
              content = s.renderResourceFooter();
              if (isString(content)) {
                  html = inst._safeHtml(content);
                  inst._shouldEnhance = true;
              }
          }
          return (createElement("div", { className: "mbsc-timeline-resource-footer", dangerouslySetInnerHTML: html }, content));
      };
      var renderSidebarHeader = function () {
          var content;
          var html;
          if (s.renderSidebarHeader) {
              content = s.renderSidebarHeader();
              if (isString(content)) {
                  html = inst._safeHtml(content);
                  inst._shouldEnhance = true;
              }
          }
          return (createElement("div", { className: "mbsc-timeline-sidebar-header", dangerouslySetInnerHTML: html }, content));
      };
      var renderSidebar = function (resource, dateKey) {
          var isParent = resource.isParent;
          var key = (dateKey ? dateKey + '-' : '') + resource.id;
          var style = {
              minHeight: inst._rowHeights[key],
          };
          var content;
          var html;
          if (s.renderSidebar) {
              content = s.renderSidebar(resource);
              if (isString(content)) {
                  html = inst._safeHtml(content);
                  inst._shouldEnhance = true;
              }
          }
          return (key !== inst._dragRow && (createElement("div", { key: key, className: 'mbsc-timeline-sidebar-resource mbsc-timeline-row mbsc-flex-1-0' +
                  (isParent ? ' mbsc-timeline-parent mbsc-flex' : '') +
                  theme +
                  rtl +
                  hb, style: style },
              createElement("div", { className: "mbsc-timeline-sidebar-resource-title", dangerouslySetInnerHTML: html }, content))));
      };
      var renderSidebarFooter = function () {
          var content;
          var html;
          if (s.renderSidebarFooter) {
              content = s.renderSidebarFooter();
              if (isString(content)) {
                  html = inst._safeHtml(content);
                  inst._shouldEnhance = true;
              }
          }
          return (createElement("div", { className: "mbsc-timeline-sidebar-footer", dangerouslySetInnerHTML: html }, content));
      };
      var renderResource = function (resource, dateKey) {
          var isParent = resource.isParent;
          var padding = inst._hasHierarchy ? resource.depth * 1.75 + 'em' : UNDEFINED;
          var key = (dateKey ? dateKey + '-' : '') + resource.id;
          var style = {
              minHeight: inst._rowHeights[key],
              paddingLeft: s.rtl ? UNDEFINED : padding,
              paddingRight: s.rtl ? padding : UNDEFINED,
          };
          var content = resource.name;
          var html;
          if (s.renderResource) {
              content = s.renderResource(resource);
              if (isString(content)) {
                  html = inst._safeHtml(content);
                  inst._shouldEnhance = true;
              }
          }
          return (key !== inst._dragRow && (createElement("div", { key: key, className: 'mbsc-timeline-resource mbsc-timeline-row mbsc-flex-1-0' +
                  (isParent ? ' mbsc-timeline-parent mbsc-flex' : '') +
                  theme +
                  rtl +
                  hb, style: style },
              isParent && (createElement(Icon, { className: 'mbsc-timeline-resource-icon' + rtl + hb, svg: resource.collapsed ? (s.rtl ? s.nextIconRtl : s.nextIcon) : s.downIcon, theme: s.theme, 
                  // tslint:disable-next-line jsx-no-lambda
                  onClick: function (ev) { return inst._onParentClick(ev, resource); } })),
              createElement("div", { className: 'mbsc-timeline-resource-title' + (isParent ? ' mbsc-flex-1-1' : ''), dangerouslySetInnerHTML: html }, content))));
      };
      var renderData = function (data, key, resource, renderFunc, checkDrag, dateKey) {
          var rangeData = data[resource][DEF_ID][dateKey || 'all'];
          var dataForBatch = [];
          if (rangeData) {
              for (var _i = 0, _a = rangeData[key]; _i < _a.length; _i++) {
                  var item = _a[_i];
                  if ((checkDrag && draggedEventId === item.id) ||
                      // We use <= to work with inclusive end dates as well
                      (inst._batchStart <= item.endDate && inst._batchEnd > item.startDate)) {
                      dataForBatch.push(item);
                  }
              }
          }
          return renderFunc(dataForBatch, dateKey || 'all', resource, DEF_ID);
      };
      var renderColors = function (colors) {
          return colors.map(function (color, i) {
              return (createElement("div", { key: i, className: 'mbsc-schedule-color mbsc-timeline-color' + color.cssClass + theme, style: color.position },
                  createElement("div", { className: "mbsc-schedule-color-text" }, color.title)));
          });
      };
      var renderInvalids = function (invalids) {
          return invalids.map(function (invalid, i) {
              return (invalid.position && (createElement("div", { key: i, className: 'mbsc-schedule-invalid mbsc-timeline-invalid' + invalid.cssClass + theme, style: invalid.position },
                  createElement("div", { className: "mbsc-schedule-invalid-text" }, invalid.title))));
          });
      };
      var renderEvents = function (events, dateKey, resource, slot) {
          var resourceResize = inst._resourcesMap[resource].eventResize;
          var dragKey = resource + '__' + slot + '__' + dateKey;
          var dragResize = computeEventResize(dragData && dragData.draggedEvent && dragData.draggedEvent.original.resize, s.dragToResize, resourceResize);
          var eventProps = {
              displayTimezone: s.displayTimezone,
              drag: s.dragToMove || s.externalDrag,
              endDay: s.endDay,
              eventHeight: inst._setRowHeight ? inst._eventHeight : UNDEFINED,
              exclusiveEndDates: s.exclusiveEndDates,
              gridEndTime: endTime,
              gridStartTime: startTime,
              hasResY: hasResY,
              isListing: isListing,
              isTimeline: true,
              lastDay: +inst._lastDay,
              render: s.renderEvent,
              renderContent: s.renderEventContent,
              resource: resource,
              rtl: s.rtl,
              slot: slot,
              startDay: s.startDay,
              theme: s.theme,
              timezonePlugin: s.timezonePlugin,
          };
          return (createElement(Fragment, null,
              events.map(function (event) {
                  return (createElement(ScheduleEvent, __assign({}, eventProps, { event: event, inactive: draggedEventId === event.id, key: event.uid, resize: computeEventResize(event.original.resize, s.dragToResize, resourceResize), selected: !!(s.selectedEventsMap[event.uid] || s.selectedEventsMap[event.id]), onClick: s.onEventClick, onDoubleClick: s.onEventDoubleClick, onRightClick: s.onEventRightClick, onHoverIn: s.onEventHoverIn, onHoverOut: s.onEventHoverOut, onDelete: s.onEventDelete, onDragStart: inst._onEventDragStart, onDragMove: inst._onEventDragMove, onDragEnd: inst._onEventDragEnd, onDragModeOn: inst._onEventDragModeOn, onDragModeOff: inst._onEventDragModeOff })));
              }),
              dragData && dragData.originDates && dragData.originDates[dragKey] && (createElement(ScheduleEvent, __assign({}, eventProps, { event: dragData.originDates[dragKey], hidden: dragData && !!dragData.draggedDates, isDrag: true, resize: dragResize, onDragStart: inst._onEventDragStart, onDragMove: inst._onEventDragMove, onDragEnd: inst._onEventDragEnd, onDragModeOff: inst._onEventDragModeOff }))),
              dragData && dragData.draggedDates && dragData.draggedDates[dragKey] && (createElement(ScheduleEvent, __assign({}, eventProps, { event: dragData.draggedDates[dragKey], isDrag: true, resize: dragResize })))));
      };
      var renderRow = function (res, dKey) {
          var resource = res.id;
          var key = (dKey ? dKey + '-' : '') + resource;
          return (createElement("div", { key: key, className: 'mbsc-timeline-row mbsc-flex mbsc-flex-1-0' +
                  (res.isParent ? ' mbsc-timeline-parent' : '') +
                  (key === inst._dragRow ? ' mbsc-timeline-hidden' : '') +
                  theme +
                  hb, style: { minHeight: inst._rowHeights[key] } },
              !hasSlots && (createElement(Fragment, null,
                  createElement("div", { className: "mbsc-timeline-events" }, renderData(inst._events, 'events', resource, renderEvents, true, dKey)),
                  renderData(inst._invalids, 'invalids', resource, renderInvalids, undefined, dKey),
                  renderData(inst._colors, 'colors', resource, renderColors, undefined, dKey))),
              createElement("div", { style: { width: inst._placeholderSizeX + 'px' }, className: "mbsc-flex-none" }),
              daysBatch.map(function (dayData) {
                  var timestamp = dayData.timestamp;
                  var dateKey = dKey || dayData.dateKey;
                  return isDailyResolution ? (createElement("div", { key: timestamp, className: 'mbsc-timeline-day mbsc-flex' +
                          theme +
                          rtl +
                          hb +
                          (dayData.dateIndex < daysNr - 1 && (isHourly || dayData.lastOfMonth) ? ' mbsc-timeline-day-border' : '') +
                          (state.hasScrollX ? ' mbsc-flex-none' : ' mbsc-flex-1-0-0') +
                          (isMonthView || inst._isMulti ? ' mbsc-timeline-day-month' : '') }, slots.map(function (sl) {
                      var slot = sl.id;
                      var dayEvents = inst._events[resource][slot][dateKey];
                      var dayColors = inst._colors[resource][slot][dateKey];
                      var dayInvalids = inst._invalids[resource][slot][dateKey];
                      return (createElement("div", { key: slot, className: 'mbsc-flex mbsc-flex-1-1' + (hasSlots ? ' mbsc-timeline-slot' : '') },
                          hasSlots && (createElement(Fragment, null,
                              createElement("div", { className: "mbsc-timeline-events" }, renderEvents(dayEvents ? dayEvents.events : [], dateKey, resource, slot)),
                              dayInvalids && renderInvalids(dayInvalids.invalids),
                              dayColors && renderColors(dayColors.colors))),
                          times.map(function (v, k) {
                              var _a;
                              var date = getCellDate(timestamp, v);
                              var first = !k;
                              var last = k === times.length - 1;
                              var cellHandlers = (_a = {},
                                  _a[ON_DOUBLE_CLICK] = function (domEvent) { return s.onCellDoubleClick({ date: date, domEvent: domEvent, resource: resource, slot: slot, source: source }); },
                                  _a[ON_CONTEXT_MENU] = function (domEvent) { return s.onCellRightClick({ date: date, domEvent: domEvent, resource: resource, slot: slot, source: source }); },
                                  _a);
                              return (createElement("div", __assign({ key: k, className: 'mbsc-timeline-column mbsc-flex-1-1' +
                                      theme +
                                      rtl +
                                      hb +
                                      ((first && !last && startCellStyle) || (last && !first && endCellStyle) ? ' mbsc-flex-none' : ''), 
                                  // tslint:disable-next-line: jsx-no-lambda
                                  onClick: function (domEvent) { return s.onCellClick({ date: date, domEvent: domEvent, resource: resource, slot: slot, source: source }); }, style: first && !last ? startCellStyle : last && !first ? endCellStyle : UNDEFINED }, cellHandlers)));
                          })));
                  }))) : (createElement("div", { key: timestamp, className: 'mbsc-timeline-day mbsc-timeline-column' + theme + rtl + hb + (state.hasScrollX ? ' mbsc-flex-none' : ' mbsc-flex-1-0-0') }));
              })));
      };
      return (createElement("div", { ref: inst._setEl, className: 'mbsc-timeline mbsc-flex-1-1 mbsc-flex-col' +
              (state.cellWidth ? '' : ' mbsc-hidden') +
              (inst._hasSticky ? ' mbsc-has-sticky' : '') +
              (hasRows ? '' : ' mbsc-timeline-no-rows') +
              (hasResources ? '' : ' mbsc-timeline-no-resource') +
              theme +
              rtl },
          createElement("div", { ref: inst._setStickyHeader, className: 'mbsc-timeline-header-sticky mbsc-flex' + theme },
              hasRows && (createElement("div", { className: 'mbsc-timeline-resource-header-cont ' + colClass + theme + rtl + hb, style: headerHeight }, renderResourceHeader())),
              isDailyResolution && (createElement("div", { className: "mbsc-flex-1-1" }, !hasResY && (createElement(Fragment, null,
                  inst._isMulti && (createElement("div", { className: 'mbsc-timeline-header-month mbsc-flex' + theme + rtl + hb }, renderMonth(days[dayIndex] || days[0], true))),
                  s.weekNumbers && (createElement("div", { className: 'mbsc-timeline-header-week mbsc-flex' + theme + rtl + hb }, renderWeek(days[dayIndex] || days[0], true))),
                  (hasSlots || isHourly) && (createElement("div", { className: 'mbsc-timeline-header-date mbsc-flex' + theme + rtl + hb }, renderDay(days[dayIndex] || days[0], true))))))),
              hasRows && s.renderSidebar && (createElement("div", { className: 'mbsc-timeline-sidebar-header-cont mbsc-timeline-sidebar-col' + theme + rtl + hb, style: headerHeight }, renderSidebarHeader())),
              state.hasScrollY && createElement("div", { className: "mbsc-schedule-fake-scroll-y" })),
          hasFooter && (createElement("div", { ref: inst._setStickyFooter, className: 'mbsc-timeline-footer-sticky mbsc-flex' + theme },
              hasRows && (createElement("div", { className: 'mbsc-timeline-resource-footer-cont ' + colClass + theme + rtl + hb, style: footerHeight }, renderResourceFooter())),
              isDailyResolution && createElement("div", { className: "mbsc-flex-1-1" }),
              hasRows && s.renderSidebar && (createElement("div", { className: 'mbsc-timeline-sidebar-footer-cont mbsc-timeline-sidebar-col' + theme + rtl + hb, style: footerHeight }, renderSidebarFooter())),
              state.hasScrollY && createElement("div", { className: "mbsc-schedule-fake-scroll-y" }))),
          createElement("div", { ref: inst._setCont, className: 'mbsc-timeline-grid-scroll mbsc-flex-col mbsc-flex-1-1' + theme + rtl + hb, onScroll: inst._onScroll },
              createElement("div", { className: "mbsc-flex-none", style: inst._hasSticky ? UNDEFINED : headerHeight }),
              createElement("div", { className: 'mbsc-timeline-header mbsc-flex' + theme + rtl + hb, ref: inst._setHeaderCont },
                  hasRows && createElement("div", { className: 'mbsc-timeline-resource-header-cont ' + colClass + theme + rtl + hb }),
                  createElement("div", { className: 'mbsc-timeline-header-bg mbsc-flex-1-0 mbsc-flex' + theme },
                      createElement("div", { className: "mbsc-timeline-time-indicator-cont", style: {
                              height: (state.scrollContHeight || 0) - (state.headerHeight || 0) + 'px',
                              width: state.hasScrollX ? inst._gridWidth + 'px' : UNDEFINED,
                          } },
                          inst._showTimeIndicator && (createElement(TimeIndicator, { amText: s.amText, displayedTime: inst._time, displayedDays: daysNr, displayTimezone: s.displayTimezone, endDay: s.endDay, firstDay: inst._firstDayTz, orientation: "y", pmText: s.pmText, rtl: s.rtl, startDay: s.startDay, startTime: startTime, theme: s.theme, timeFormat: s.timeFormat, timezonePlugin: s.timezonePlugin, hasResY: hasResY })),
                          inst._showCursorTime && (createElement("div", { ref: inst._setCursorTimeCont, className: 'mbsc-schedule-cursor-time mbsc-schedule-cursor-time-y' + theme }))),
                      createElement("div", { className: "mbsc-flex-none", style: { width: inst._placeholderSizeX + 'px' } }),
                      createElement("div", { className: state.hasScrollX ? 'mbsc-flex-none' : 'mbsc-flex-1-1' }, isDailyResolution ? (createElement(Fragment, null,
                          inst._isMulti && !hasResY && (createElement("div", { className: "mbsc-flex" }, daysBatch.map(function (d) {
                              var last = d.lastOfMonth;
                              return (createElement("div", { key: d.timestamp, className: 'mbsc-timeline-month mbsc-flex-1-0-0' +
                                      theme +
                                      rtl +
                                      hb +
                                      (d.dateIndex < daysNr - 1 && last ? ' mbsc-timeline-day mbsc-timeline-day-border' : '') },
                                  createElement("div", { className: 'mbsc-timeline-header-month' + theme + rtl + hb + (last ? ' mbsc-timeline-header-month-last' : '') }, d.monthTitle && renderMonth(d, false))));
                          }))),
                          s.weekNumbers && (createElement("div", { className: "mbsc-flex" }, daysBatch.map(function (d) {
                              var last = d.lastOfWeek;
                              return (createElement("div", { key: d.timestamp, className: 'mbsc-timeline-month mbsc-flex-1-0-0' +
                                      theme +
                                      rtl +
                                      hb +
                                      (d.dateIndex < daysNr - 1 && last && (isHourly || d.lastOfMonth)
                                          ? ' mbsc-timeline-day mbsc-timeline-day-border'
                                          : '') },
                                  createElement("div", { className: 'mbsc-timeline-header-week' + theme + rtl + hb + (last ? ' mbsc-timeline-header-week-last' : '') }, d.weekTitle && renderWeek(d, false))));
                          }))),
                          createElement("div", { className: "mbsc-flex" }, daysBatch.map(function (d) {
                              return (createElement("div", { key: d.timestamp, className: 'mbsc-timeline-day mbsc-flex-1-0-0' +
                                      theme +
                                      rtl +
                                      hb +
                                      (d.dateIndex < daysNr - 1 && (isHourly || d.lastOfMonth) ? ' mbsc-timeline-day-border' : '') +
                                      (isMonthView || inst._isMulti ? ' mbsc-timeline-day-month' : '') },
                                  !hasResY && (createElement("div", { className: 'mbsc-timeline-header-date' + theme + rtl + hb },
                                      renderDay(d),
                                      d.label && createElement("div", { className: "mbsc-hidden-content" }, d.label))),
                                  hasSlots && (createElement("div", { className: 'mbsc-flex mbsc-timeline-slots' + theme }, slots.map(function (slot) {
                                      return (createElement("div", { key: slot.id, className: 'mbsc-timeline-slot mbsc-timeline-slot-header mbsc-flex-1-1' + rtl + theme }, slot.name && renderSlot({ slot: slot, date: d.date })));
                                  }))),
                                  createElement("div", { "aria-hidden": "true", className: "mbsc-flex" }, times.map(function (t, j) {
                                      var first = !j;
                                      var last = j === times.length - 1;
                                      return (createElement("div", { key: j, style: first && !last ? startCellStyle : last && !first ? endCellStyle : UNDEFINED, className: 'mbsc-flex mbsc-flex-1-1 mbsc-timeline-header-column' +
                                              theme +
                                              rtl +
                                              hb +
                                              (!inst._displayTime || hasSlots ? ' mbsc-timeline-no-height' : '') +
                                              (stepLabel > inst._stepCell && times[j + 1] % stepLabel ? ' mbsc-timeline-no-border' : '') +
                                              ((first && startCellStyle) || (last && endCellStyle) ? ' mbsc-flex-none' : '') },
                                          renderHour(d, t),
                                          inst._timesBetween.map(function (tb, k) {
                                              var ms = t + (k + 1) * stepLabel;
                                              return ms > startTime && ms < endTime && renderHour(d, ms);
                                          })));
                                  }))));
                          })))) : (createElement("div", { className: "mbsc-flex" }, daysBatch.map(function (d) {
                          return (createElement("div", { key: d.timestamp, className: 'mbsc-timeline-day mbsc-flex-1-0-0' + theme + rtl + hb },
                              createElement("div", { className: 'mbsc-timeline-header-week mbsc-timeline-header-week-last' + theme + rtl + hb },
                                  createElement("div", { className: 'mbsc-timeline-header-week-text mbsc-timeline-header-week-text-last' +
                                          (d.isActive && !(s.renderWeek || s.renderMonth || s.renderYear) ? ' mbsc-timeline-header-active' : '') +
                                          theme }, renderHeaderResolution(d)))));
                      }))))),
                  hasRows && s.renderSidebar && (createElement("div", { className: 'mbsc-timeline-sidebar-header-cont mbsc-timeline-sidebar-col' + theme + rtl + hb }))),
              createElement("div", { className: "mbsc-flex mbsc-flex-1-1" },
                  createElement("div", { className: "mbsc-flex mbsc-flex-1-1" },
                      hasRows && (createElement("div", { className: 'mbsc-timeline-resources mbsc-flex-col ' + colClass + theme + rtl, ref: inst._setResCont },
                          createElement("div", { className: "mbsc-flex-none", style: inst._hasSideSticky ? UNDEFINED : headerHeight }),
                          createElement("div", { className: 'mbsc-timeline-resource-bg mbsc-flex-1-1' + (inst._hasHierarchy || state.hasScrollY ? '' : ' mbsc-flex-col') + theme },
                              createElement("div", { style: { height: inst._placeholderSizeY + 'px' }, className: "mbsc-flex-none" }),
                              inst._rowBatch.map(function (rowGroup) {
                                  var day = rowGroup.day;
                                  var dateKey = day ? day.dateKey : '';
                                  return !rowGroup.hidden && day ? (hasResources ? (createElement("div", { key: dateKey, className: 'mbsc-timeline-row-group mbsc-flex mbsc-flex-1-0' + theme + hb },
                                      createElement("div", { className: 'mbsc-timeline-row-date mbsc-timeline-row-date-col mbsc-flex-none' + rtl + theme + hb }, renderDay(day)),
                                      createElement("div", { className: "mbsc-timeline-row-resource-col mbsc-flex-1-1 mbsc-flex-col" }, rowGroup.rows.map(function (r) { return renderResource(r, dateKey); })))) : (createElement("div", { key: dateKey, className: 'mbsc-timeline-row-date mbsc-flex-1-0' + rtl + theme + hb, style: {
                                          minHeight: inst._rowHeights[dateKey + '-' + DEF_ID],
                                      } }, renderDay(day)))) : (rowGroup.rows.map(function (r) { return renderResource(r, dateKey); }));
                              })),
                          createElement("div", { className: "mbsc-flex-none", style: inst._hasSideSticky ? UNDEFINED : footerHeight }))),
                      hasRows && createElement("div", { className: inst._hasSideSticky ? '' : colClass }),
                      createElement("div", { className: "mbsc-timeline-hidden" },
                          createElement("div", { className: 'mbsc-timeline-row mbsc-timeline-empty-row' + theme }),
                          createElement("div", { className: 'mbsc-timeline-row mbsc-timeline-parent mbsc-timeline-empty-parent' + theme }),
                          createElement("div", { className: 'mbsc-timeline-row-gutter' + theme })),
                      createElement("div", __assign({ className: 'mbsc-timeline-grid mbsc-flex-1-0' + (inst._hasHierarchy || state.hasScrollY ? '' : ' mbsc-flex-col'), ref: inst._setGridCont, style: {
                              height: state.hasScrollY ? inst._gridHeight + 'px' : UNDEFINED,
                              width: state.hasScrollX ? inst._gridWidth + 'px' : UNDEFINED,
                          } }, handlers),
                          createElement("div", { style: { height: inst._placeholderSizeY + 'px' }, className: "mbsc-flex-none" }),
                          inst._rowBatch.map(function (rowGroup) {
                              var day = rowGroup.day;
                              var dateKey = day ? day.dateKey : '';
                              return day && hasResources ? (createElement("div", { key: dateKey, className: 'mbsc-timeline-row-group mbsc-flex-col mbsc-flex-1-0' + theme + hb }, rowGroup.rows.map(function (r) { return renderRow(r, dateKey); }))) : (createElement(Fragment, { key: dateKey }, rowGroup.rows.map(function (r) { return renderRow(r, dateKey); })));
                          }),
                          inst._connections && (createElement("svg", __assign({ viewBox: "0 0 100 100", preserveAspectRatio: "none" }, svgProps), inst._connections.map(function (c) {
                              var _a;
                              var props = (_a = {},
                                  _a['className'] = 'mbsc-connection ' + c.cssClass + theme,
                                  _a.d = c.pathD,
                                  _a.style = { stroke: c.color, fill: c.fill },
                                  _a['vector-effect' ] = 'non-scaling-stroke',
                                  _a);
                              return (checkDateRangeOverlap(inst._batchStart, inst._batchEnd, c.startDate, c.endDate, true) && (createElement("path", __assign({ key: c.id }, props))));
                          })))),
                      hasRows && s.renderSidebar && (createElement("div", { className: 'mbsc-timeline-sidebar mbsc-timeline-sidebar-col mbsc-flex-col' + theme + rtl, ref: inst._setSidebarCont },
                          createElement("div", { className: "mbsc-flex-none", style: inst._hasSideSticky ? UNDEFINED : headerHeight }),
                          createElement("div", { className: 'mbsc-timeline-resource-bg mbsc-flex-1-1' + (inst._hasHierarchy || state.hasScrollY ? '' : ' mbsc-flex-col') + theme },
                              createElement("div", { style: { height: inst._placeholderSizeY + 'px' }, className: "mbsc-flex-none" }),
                              inst._rowBatch.map(function (rowGroup) {
                                  var day = rowGroup.day;
                                  var dateKey = day ? day.dateKey : '';
                                  return day && hasResources ? (createElement("div", { key: dateKey, className: 'mbsc-timeline-row-group mbsc-flex-col mbsc-flex-1-0' + theme + hb }, rowGroup.rows.map(function (r) { return renderSidebar(r, dateKey); }))) : (rowGroup.rows.map(function (r) { return renderSidebar(r, dateKey); }));
                              })),
                          createElement("div", { className: "mbsc-flex-none", style: inst._hasSideSticky ? UNDEFINED : footerHeight }))),
                      hasRows && s.renderSidebar && createElement("div", { className: inst._hasSideSticky ? '' : 'mbsc-timeline-sidebar-col' }))),
              hasFooter && (createElement(Fragment, null,
                  createElement("div", { className: "mbsc-flex-none", style: inst._hasSticky ? UNDEFINED : footerHeight }),
                  createElement("div", { className: 'mbsc-timeline-footer mbsc-flex' + theme + rtl + hb, ref: inst._setFooterCont },
                      hasRows && createElement("div", { className: 'mbsc-timeline-resource-footer-cont ' + colClass + theme + rtl + hb }),
                      createElement("div", { className: 'mbsc-timeline-footer-bg mbsc-flex-1-0 mbsc-flex' + theme },
                          createElement("div", { className: "mbsc-flex-none", style: { width: inst._placeholderSizeX + 'px' } }),
                          createElement("div", { className: state.hasScrollX ? 'mbsc-flex-none' : 'mbsc-flex-1-1' },
                              createElement("div", { className: "mbsc-flex" }, daysBatch.map(function (d) {
                                  return isDailyResolution ? (createElement("div", { key: d.timestamp, className: 'mbsc-timeline-day mbsc-flex-1-0-0' +
                                          theme +
                                          rtl +
                                          hb +
                                          (d.dateIndex < daysNr - 1 && (isHourly || d.lastOfMonth) ? ' mbsc-timeline-day-border' : '') +
                                          (isMonthView || inst._isMulti ? ' mbsc-timeline-day-month' : '') },
                                      createElement("div", { className: "mbsc-flex" }, times.map(function (t, j) {
                                          var first = !j;
                                          var last = j === times.length - 1;
                                          return (createElement("div", { key: j, style: first && !last ? startCellStyle : last && !first ? endCellStyle : UNDEFINED, className: 'mbsc-flex mbsc-flex-1-1 mbsc-timeline-column mbsc-timeline-footer-column' +
                                                  theme +
                                                  rtl +
                                                  hb +
                                                  (!inst._displayTime || hasSlots ? ' mbsc-timeline-no-height' : '') +
                                                  (stepLabel > inst._stepCell && times[j + 1] % stepLabel ? 'mbsc-timeline-no-border' : '') +
                                                  ((first && !last && startCellStyle) || (last && !first && endCellStyle) ? ' mbsc-flex-none' : '') },
                                              renderHourFooter(d, t),
                                              inst._timesBetween.map(function (tb, k) {
                                                  var ms = t + (k + 1) * stepLabel;
                                                  return ms > startTime && ms < endTime && renderHourFooter(d, ms);
                                              })));
                                      })),
                                      s.renderDayFooter && createElement("div", { className: 'mbsc-timeline-footer-date' + theme + rtl + hb }, renderDayFooter(d)),
                                      hasSlots && (createElement("div", { className: "mbsc-flex" }, slots.map(function (slot) {
                                          return createElement("div", { key: slot.id, className: 'mbsc-timeline-slot mbsc-flex-1-1' + rtl + theme });
                                      }))))) : (createElement("div", { key: d.timestamp, className: 'mbsc-timeline-day mbsc-flex-1-0-0' + theme + rtl + hb },
                                      createElement("div", { className: 'mbsc-timeline-footer-week mbsc-timeline-footer-week-last' + theme + rtl + hb },
                                          createElement("div", { className: 'mbsc-timeline-footer-week-text' + theme }, renderFooterResolution(d)))));
                              })))),
                      hasRows && s.renderSidebar && (createElement("div", { className: 'mbsc-timeline-sidebar-footer-cont mbsc-timeline-sidebar-col' + theme + rtl + hb })))))),
          dragData && !state.isTouchDrag && createElement("div", { className: "mbsc-calendar-dragging" })));
  }
  var Timeline = /*#__PURE__*/ (function (_super) {
      __extends(Timeline, _super);
      function Timeline() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      Timeline.prototype._template = function (s, state) {
          return template$a(s, state, this);
      };
      return Timeline;
  }(TimelineBase));

  var CalendarContext = createContext({});
  var InstanceSubscriber = /*#__PURE__*/ (function (_super) {
      __extends(InstanceSubscriber, _super);
      function InstanceSubscriber() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      // tslint:enable: variable-name
      InstanceSubscriber.prototype.componentWillUnmount = function () {
          if (this._changes) {
              this._changes.unsubscribe(this._handler);
          }
      };
      InstanceSubscriber.prototype.render = function () {
          var _this = this;
          var _a = this.props, host = _a.host, component = _a.component, view = _a.view, other = __rest(_a, ["host", "component", "view"]);
          var calView = view || (host && host._calendarView);
          if (calView && !this._changes) {
              this._changes = calView.s.instanceService.onComponentChange;
              this._handler = this._changes.subscribe(function () {
                  _this.forceUpdate();
              });
          }
          return createElement(CalendarContext.Consumer, null, function (_a) {
              var instance = _a.instance;
              var inst = instance || view || (host && host._calendarView);
              return inst && createElement(component, __assign({ inst: inst }, other));
          });
      };
      return InstanceSubscriber;
  }(PureComponent));
  var CalendarPrevButton = function (_a) {
      var inst = _a.inst, className = _a.className;
      return (createElement(Button, { ariaLabel: inst.s.prevPageText, className: 'mbsc-calendar-button ' + (className || ''), disabled: inst._isPrevDisabled(), iconSvg: inst._prevIcon, onClick: inst.prevPage, theme: inst.s.theme, themeVariant: inst.s.themeVariant, type: "button", variant: "flat" }));
  };
  var CalendarNextButton = function (_a) {
      var inst = _a.inst, className = _a.className;
      return (createElement(Button, { ariaLabel: inst.s.nextPageText, disabled: inst._isNextDisabled(), className: 'mbsc-calendar-button ' + (className || ''), iconSvg: inst._nextIcon, onClick: inst.nextPage, theme: inst.s.theme, themeVariant: inst.s.themeVariant, type: "button", variant: "flat" }));
  };
  var CalendarTodayButton = function (_a) {
      var inst = _a.inst, className = _a.className;
      return (createElement(Button, { className: 'mbsc-calendar-button mbsc-calendar-button-today ' + (className || ''), onClick: inst._onTodayClick, theme: inst.s.theme, themeVariant: inst.s.themeVariant, type: "button", variant: "flat" }, inst.s.todayText));
  };
  var CalendarTitleButton = function (_a) {
      var inst = _a.inst, className = _a.className;
      var s = inst.s;
      var theme = inst._theme;
      var view = inst._view;
      return (createElement("div", { "aria-live": "polite", className: (className || '') + theme }, inst._title.map(function (val, index) {
          return ((inst._pageNr === 1 || index === 0 || inst._hasPicker || view === MONTH_VIEW) && (createElement(Button, { className: 'mbsc-calendar-button' + (inst._pageNr > 1 ? ' mbsc-flex-1-1' : ''), "data-index": index, onClick: inst._onPickerBtnClick, key: index, theme: s.theme, themeVariant: s.themeVariant, type: "button", variant: "flat" },
              (inst._hasPicker || view === MONTH_VIEW) &&
                  (val.title ? (createElement("span", { className: 'mbsc-calendar-title' + theme }, val.title)) : (createElement(Fragment, null,
                      inst._yearFirst && createElement("span", { className: 'mbsc-calendar-title mbsc-calendar-year' + theme }, val.yearTitle),
                      createElement("span", { className: 'mbsc-calendar-title mbsc-calendar-month' + theme }, val.monthTitle),
                      !inst._yearFirst && createElement("span", { className: 'mbsc-calendar-title mbsc-calendar-year' + theme }, val.yearTitle)))),
              !inst._hasPicker && view !== MONTH_VIEW && createElement("span", { className: 'mbsc-calendar-title' + theme }, inst._viewTitle),
              s.downIcon && inst._pageNr === 1 ? createElement(Icon, { svg: view === MONTH_VIEW ? s.downIcon : s.upIcon, theme: s.theme }) : null)));
      })));
  };
  var CalendarPrev = function (_a) {
      var calendar = _a.calendar, view = _a.view, others = __rest(_a, ["calendar", "view"]);
      return createElement(InstanceSubscriber, __assign({ component: CalendarPrevButton, host: calendar, view: view }, others));
  };
  CalendarPrev._name = 'CalendarPrev';
  var CalendarNext = function (_a) {
      var calendar = _a.calendar, view = _a.view, others = __rest(_a, ["calendar", "view"]);
      return createElement(InstanceSubscriber, __assign({ component: CalendarNextButton, host: calendar, view: view }, others));
  };
  CalendarNext._name = 'CalendarNext';
  var CalendarToday = function (_a) {
      var calendar = _a.calendar, view = _a.view, others = __rest(_a, ["calendar", "view"]);
      return createElement(InstanceSubscriber, __assign({ component: CalendarTodayButton, host: calendar, view: view }, others));
  };
  CalendarToday._name = 'CalendarToday';
  var CalendarNav = function (_a) {
      var calendar = _a.calendar, view = _a.view, others = __rest(_a, ["calendar", "view"]);
      return createElement(InstanceSubscriber, __assign({ component: CalendarTitleButton, host: calendar, view: view }, others));
  };
  CalendarNav._name = 'CalendarNav';

  // tslint:disable no-non-null-assertion
  // tslint:disable directive-class-suffix
  // tslint:disable directive-selector
  /** @hidden */
  var CalendarViewBase = /*#__PURE__*/ (function (_super) {
      __extends(CalendarViewBase, _super);
      function CalendarViewBase() {
          var _this = _super !== null && _super.apply(this, arguments) || this;
          _this.state = {
              height: 'sm',
              // maxLabels: 0,
              pageSize: 0,
              pickerSize: 0,
              // view: MONTH_VIEW,
              width: 'sm',
          };
          _this._dim = {};
          _this._months = [1, 2, 3]; // TODO: this is crap
          _this._title = [];
          _this.MONTH_VIEW = MONTH_VIEW;
          _this.YEAR_VIEW = YEAR_VIEW;
          _this.MULTI_YEAR_VIEW = MULTI_YEAR_VIEW;
          // tslint:enable variable-name
          // ---
          /**
           * Navigates to next page
           */
          _this.nextPage = function () {
              _this._prevDocClick();
              switch (_this._view) {
                  case MULTI_YEAR_VIEW:
                      _this._activeYearsChange(1);
                      break;
                  case YEAR_VIEW:
                      _this._activeYearChange(1);
                      break;
                  default:
                      _this._activeChange(1);
              }
          };
          /**
           * Navigates to previous page
           */
          _this.prevPage = function () {
              _this._prevDocClick();
              switch (_this._view) {
                  case MULTI_YEAR_VIEW:
                      _this._activeYearsChange(-1);
                      break;
                  case YEAR_VIEW:
                      _this._activeYearChange(-1);
                      break;
                  default:
                      _this._activeChange(-1);
              }
          };
          // These are public because of the angular template only
          // ---
          // tslint:disable variable-name
          _this._changeView = function (newView) {
              var s = _this.s;
              var view = _this._view;
              var hasPicker = _this._hasPicker;
              var selectView = s.selectView;
              var isYearView = (s.showCalendar ? s.calendarType : s.eventRange) === 'year';
              if (!newView) {
                  switch (view) {
                      case MONTH_VIEW:
                          newView = MULTI_YEAR_VIEW;
                          break;
                      case MULTI_YEAR_VIEW:
                          newView = YEAR_VIEW;
                          break;
                      default:
                          newView = hasPicker || selectView === YEAR_VIEW ? MULTI_YEAR_VIEW : MONTH_VIEW;
                  }
                  if (view === MULTI_YEAR_VIEW && isYearView) {
                      newView = MONTH_VIEW;
                  }
              }
              var skipAnimation = hasPicker && newView === selectView;
              _this.setState({
                  view: newView,
                  viewClosing: skipAnimation ? UNDEFINED : view,
                  viewOpening: skipAnimation ? UNDEFINED : newView,
              });
          };
          _this._onDayHoverIn = function (ev) {
              if (!_this._disableHover) {
                  _this._hook('onDayHoverIn', ev);
                  _this._hoverTimer = setTimeout(function () {
                      var key = getDateStr(ev.date);
                      if (_this._labels) {
                          ev.labels = _this._labels[key];
                      }
                      if (_this._marked) {
                          ev.marked = _this._marked[key];
                      }
                      _this._isHover = true;
                      _this._hook('onCellHoverIn', ev);
                  }, 150);
              }
          };
          _this._onDayHoverOut = function (ev) {
              if (!_this._disableHover) {
                  _this._hook('onDayHoverOut', ev);
                  clearTimeout(_this._hoverTimer);
                  if (_this._isHover) {
                      var key = getDateStr(ev.date);
                      if (_this._labels) {
                          ev.labels = _this._labels[key];
                      }
                      if (_this._marked) {
                          ev.marked = _this._marked[key];
                      }
                      _this._isHover = false;
                      _this._hook('onCellHoverOut', ev);
                  }
              }
          };
          _this._onLabelClick = function (args) {
              _this._isLabelClick = true;
              _this._hook('onLabelClick', args);
          };
          _this._onDayClick = function (args) {
              _this._shouldFocus = !_this._isLabelClick;
              _this._prevAnim = false;
              _this._isLabelClick = false;
              _this._hook('onDayClick', args);
          };
          _this._onTodayClick = function (args) {
              _this._prevAnim = false;
              _this._hook('onActiveChange', {
                  date: +removeTimezone(createDate(_this.s)),
                  today: true,
              });
              _this._hook('onTodayClick', {});
          };
          _this._onMonthClick = function (args) {
              if (args.disabled) {
                  return;
              }
              var d = args.date;
              var s = _this.s;
              if (s.selectView === YEAR_VIEW) {
                  _this._hook('onDayClick', args);
              }
              else {
                  var newIndex = getPageIndex(d, s);
                  _this._prevDocClick();
                  _this._changeView(MONTH_VIEW);
                  _this._shouldFocus = true;
                  _this._prevAnim = !_this._hasPicker;
                  _this._hook('onActiveChange', {
                      date: +d,
                      // it is used for scrolling to the first day of the selected month in case of quick navigation
                      nav: true,
                      pageChange: newIndex !== _this._pageIndex,
                  });
              }
          };
          _this._onYearClick = function (args) {
              if (args.disabled) {
                  return;
              }
              var d = args.date;
              var s = _this.s;
              var view = s.selectView;
              if (view === MULTI_YEAR_VIEW) {
                  _this._hook('onDayClick', args);
              }
              else {
                  _this._shouldFocus = true;
                  _this._prevAnim = view === YEAR_VIEW;
                  _this._activeMonth = +d;
                  _this._prevDocClick();
                  _this._changeView();
                  if ((s.showCalendar ? s.calendarType : s.eventRange) === 'year') {
                      var newIndex = getPageIndex(d, s);
                      _this._hook('onActiveChange', {
                          date: +d,
                          pageChange: newIndex !== _this._pageIndex,
                      });
                  }
              }
          };
          _this._onPageChange = function (args) {
              _this._isSwipeChange = true;
              _this._activeChange(args.diff);
          };
          _this._onYearPageChange = function (args) {
              _this._activeYearChange(args.diff);
          };
          _this._onYearsPageChange = function (args) {
              _this._activeYearsChange(args.diff);
          };
          _this._onAnimationEnd = function (args) {
              _this._disableHover = false;
              if (_this._isIndexChange) {
                  _this._pageLoaded();
                  _this._isIndexChange = false;
              }
          };
          _this._onStart = function () {
              clearTimeout(_this._hoverTimer);
          };
          _this._onGestureStart = function (args) {
              _this._disableHover = true;
              _this._hook('onGestureStart', args);
          };
          _this._onGestureEnd = function (args) {
              _this._prevDocClick();
          };
          _this._onPickerClose = function () {
              _this.setState({ view: MONTH_VIEW });
          };
          _this._onPickerOpen = function () {
              var pageHeight = _this._pickerCont.clientHeight;
              var pageWidth = _this._pickerCont.clientWidth;
              _this.setState({ pickerSize: _this._isVertical ? pageHeight : pageWidth });
          };
          _this._onPickerBtnClick = function (ev) {
              if (_this._view === MONTH_VIEW) {
                  _this._pickerBtn = ev.currentTarget;
              }
              _this._prevDocClick();
              _this._changeView();
          };
          _this._onDocClick = function () {
              var view = _this.s.selectView;
              if (!_this._prevClick && !_this._hasPicker && _this._view !== view) {
                  _this._changeView(view);
              }
          };
          _this._onViewAnimationEnd = function () {
              if (_this.state.viewClosing) {
                  _this.setState({ viewClosing: UNDEFINED });
              }
              if (_this.state.viewOpening) {
                  _this.setState({ viewOpening: UNDEFINED });
              }
          };
          _this._onResize = function () {
              if (!_this._body || !isBrowser) {
                  return;
              }
              var s = _this.s;
              var state = _this.state;
              var showCalendar = s.showCalendar;
              // In Chrome, if _body has a size in subpixels, the inner element will still have rounded pixel values,
              // so we calculate with the size of the inner element.
              var body = showCalendar /* TRIALCOND */ ? _this._body.querySelector('.mbsc-calendar-body-inner') : _this._body;
              // We need to use getBoundingClientRect to get the subpixel values if that's the case,
              // otherwise after navigating multiple times the transform will be off
              // const rect = body.getBoundingClientRect();
              // const pageHeight = rect.height; // this._body.clientHeight;
              // const pageWidth = rect.width; // this._body.clientWidth;
              var totalWidth = _this._el.offsetWidth;
              var totalHeight = _this._el.offsetHeight;
              var pageHeight = body.clientHeight;
              var pageWidth = body.clientWidth;
              var pageSize = _this._isVertical ? pageHeight : pageWidth;
              var pickerSize = _this._hasPicker ? state.pickerSize : pageSize;
              var ready = showCalendar !== UNDEFINED;
              var width = 'sm';
              var height = 'sm';
              var maxLabels = 1;
              var hasScrollY = false;
              var cellTextHeight = 0;
              var labelHeight = 0;
              if (s.responsiveStyle && !_this._isGrid) {
                  if (pageHeight > 300) {
                      height = 'md';
                  }
                  if (pageWidth > 767) {
                      width = 'md';
                  }
              }
              if (width !== state.width || height !== state.height) {
                  // Switch between mobile and desktop styling.
                  // After the new classes are applied, labels and page sizes needs re-calculation
                  _this._shouldCheckSize = true;
                  _this.setState({ width: width, height: height });
              }
              else {
                  if (_this._labels && showCalendar /* TRIALCOND */) {
                      // Check how many labels can we display on a day
                      // TODO: this must be refactored for React Native
                      var placeholder = body.querySelector('.mbsc-calendar-text');
                      var cell = body.querySelector('.mbsc-calendar-day-inner');
                      var labelsCont = cell.querySelector('.mbsc-calendar-labels');
                      var txtMargin = placeholder ? getDimension(placeholder, 'marginBottom') : 2;
                      var txtHeight = placeholder ? placeholder.offsetHeight : 18;
                      cellTextHeight = labelsCont.offsetTop;
                      hasScrollY = body.scrollHeight > body.clientHeight;
                      labelHeight = txtHeight + txtMargin;
                      maxLabels = Math.max(1, floor((cell.clientHeight - cellTextHeight) / labelHeight));
                  }
                  _this._hook('onResize', {
                      height: totalHeight,
                      target: _this._el,
                      width: totalWidth,
                  });
                  s.navigationService.pageSize = pageSize;
                  // Force update if page loaded needs to be triggered
                  var update = _this._shouldPageLoad ? (state.update || 0) + 1 : state.update;
                  _this.setState({ cellTextHeight: cellTextHeight, hasScrollY: hasScrollY, labelHeight: labelHeight, maxLabels: maxLabels, pageSize: pageSize, pickerSize: pickerSize, ready: ready, update: update });
              }
          };
          _this._onKeyDown = function (ev) {
              var s = _this.s;
              var view = _this._view;
              var active = view === MONTH_VIEW ? _this._active : _this._activeMonth;
              var activeDate = new Date(active);
              var year = s.getYear(activeDate);
              var month = s.getMonth(activeDate);
              var day = s.getDay(activeDate);
              var getDate = s.getDate;
              var weeks = s.weeks;
              var isMonthView = s.calendarType === 'month';
              var newDate;
              if (view === MULTI_YEAR_VIEW) {
                  var newYear = void 0;
                  switch (ev.keyCode) {
                      case LEFT_ARROW:
                          newYear = year - 1 * _this._rtlNr;
                          break;
                      case RIGHT_ARROW:
                          newYear = year + 1 * _this._rtlNr;
                          break;
                      case UP_ARROW:
                          newYear = year - 3;
                          break;
                      case DOWN_ARROW:
                          newYear = year + 3;
                          break;
                      case HOME:
                          newYear = _this._getPageYears(_this._yearsIndex);
                          break;
                      case END:
                          newYear = _this._getPageYears(_this._yearsIndex) + 11;
                          break;
                      case PAGE_UP:
                          newYear = year - 12;
                          break;
                      case PAGE_DOWN:
                          newYear = year + 12;
                          break;
                  }
                  if (newYear && _this._minYears <= newYear && _this._maxYears >= newYear) {
                      ev.preventDefault();
                      _this._shouldFocus = true;
                      _this._prevAnim = false;
                      _this._activeMonth = +getDate(newYear, 0, 1);
                      _this.forceUpdate();
                  }
              }
              else if (view === YEAR_VIEW) {
                  switch (ev.keyCode) {
                      case LEFT_ARROW:
                          newDate = getDate(year, month - 1 * _this._rtlNr, 1);
                          break;
                      case RIGHT_ARROW:
                          newDate = getDate(year, month + 1 * _this._rtlNr, 1);
                          break;
                      case UP_ARROW:
                          newDate = getDate(year, month - 3, 1);
                          break;
                      case DOWN_ARROW:
                          newDate = getDate(year, month + 3, 1);
                          break;
                      case HOME:
                          newDate = getDate(year, 0, 1);
                          break;
                      case END:
                          newDate = getDate(year, 11, 1);
                          break;
                      case PAGE_UP:
                          newDate = getDate(year - 1, month, 1);
                          break;
                      case PAGE_DOWN:
                          newDate = getDate(year + 1, month, 1);
                          break;
                  }
                  if (newDate && _this._minYear <= newDate && _this._maxYear >= newDate) {
                      ev.preventDefault();
                      _this._shouldFocus = true;
                      _this._prevAnim = false;
                      _this._activeMonth = +newDate;
                      _this.forceUpdate();
                  }
              }
              else if (view === MONTH_VIEW) {
                  switch (ev.keyCode) {
                      case LEFT_ARROW:
                          newDate = getDate(year, month, day - 1 * _this._rtlNr);
                          break;
                      case RIGHT_ARROW:
                          newDate = getDate(year, month, day + 1 * _this._rtlNr);
                          break;
                      case UP_ARROW:
                          newDate = getDate(year, month, day - 7);
                          break;
                      case DOWN_ARROW:
                          newDate = getDate(year, month, day + 7);
                          break;
                      case HOME:
                          newDate = getDate(year, month, 1);
                          break;
                      case END:
                          newDate = getDate(year, month + 1, 0);
                          break;
                      case PAGE_UP:
                          newDate = ev.altKey
                              ? getDate(year - 1, month, day)
                              : isMonthView
                                  ? getDate(year, month - 1, day)
                                  : getDate(year, month, day - weeks * 7);
                          break;
                      case PAGE_DOWN:
                          newDate = ev.altKey
                              ? getDate(year + 1, month, day)
                              : isMonthView
                                  ? getDate(year, month + 1, day)
                                  : getDate(year, month, day + weeks * 7);
                          break;
                  }
                  if (newDate && _this._minDate <= newDate && _this._maxDate >= newDate) {
                      ev.preventDefault();
                      var newIndex = getPageIndex(newDate, s);
                      _this._shouldFocus = true;
                      _this._prevAnim = false;
                      _this._pageChange = s.noOuterChange && newIndex !== _this._pageIndex;
                      _this._hook('onActiveChange', {
                          date: +newDate,
                          pageChange: _this._pageChange,
                      });
                  }
              }
          };
          _this._setHeader = function (el) {
              _this._headerElement = el;
          };
          _this._setBody = function (el) {
              _this._body = el;
          };
          _this._setPickerCont = function (el) {
              _this._pickerCont = el;
          };
          return _this;
      }
      CalendarViewBase.prototype._getPageDay = function (pageIndex) {
          return +getFirstPageDay(pageIndex, this.s);
      };
      CalendarViewBase.prototype._getPageStyle = function (index, offset, pageNr) {
          var _a;
          return _a = {},
              _a[(jsPrefix ? jsPrefix + 'T' : 't') + 'ransform'] = 'translate' + this._axis + '(' + (index - offset) * 100 * this._rtlNr + '%)',
              _a.width = 100 / (pageNr || 1) + '%',
              _a;
      };
      CalendarViewBase.prototype._getPageYear = function (pageIndex) {
          var s = this.s;
          var refDate = s.refDate ? makeDate(s.refDate) : REF_DATE;
          var year = s.getYear(refDate);
          return year + pageIndex;
      };
      CalendarViewBase.prototype._getPageYears = function (pageIndex) {
          var s = this.s;
          var refDate = s.refDate ? makeDate(s.refDate) : REF_DATE;
          var year = s.getYear(refDate);
          return year + pageIndex * 12;
      };
      CalendarViewBase.prototype._getPickerClass = function (view) {
          var animName;
          var pickerName = view === this.s.selectView ? ' mbsc-calendar-picker-main' : '';
          var baseName = 'mbsc-calendar-picker';
          var hasPicker = this._hasPicker;
          var _a = this.state, viewClosing = _a.viewClosing, viewOpening = _a.viewOpening;
          switch (view) {
              case MONTH_VIEW:
                  animName = hasPicker ? '' : (viewOpening === MONTH_VIEW ? 'in-down' : '') + (viewClosing === MONTH_VIEW ? 'out-down' : '');
                  break;
              case MULTI_YEAR_VIEW:
                  animName =
                      hasPicker && viewClosing === MONTH_VIEW
                          ? ''
                          : (viewOpening === MULTI_YEAR_VIEW ? 'in-up' : '') + (viewClosing === MULTI_YEAR_VIEW ? 'out-up' : '');
                  break;
              default:
                  animName =
                      hasPicker && viewOpening === MONTH_VIEW
                          ? ''
                          : (viewOpening === YEAR_VIEW ? (viewClosing === MULTI_YEAR_VIEW ? 'in-down' : 'in-up') : '') +
                              (viewClosing === YEAR_VIEW ? (viewOpening === MULTI_YEAR_VIEW ? 'out-down' : 'out-up') : '');
          }
          return baseName + pickerName + (hasAnimation && animName ? ' ' + baseName + '-' + animName : '');
      };
      CalendarViewBase.prototype._isNextDisabled = function (isModalPicker) {
          if (!this._hasPicker || isModalPicker) {
              var view = this._view;
              if (view === MULTI_YEAR_VIEW) {
                  return this._yearsIndex + 1 > this._maxYearsIndex;
              }
              if (view === YEAR_VIEW) {
                  return this._yearIndex + 1 > this._maxYearIndex;
              }
          }
          return this._pageIndex + 1 > this._maxIndex;
      };
      CalendarViewBase.prototype._isPrevDisabled = function (isModalPicker) {
          if (!this._hasPicker || isModalPicker) {
              var view = this._view;
              if (view === MULTI_YEAR_VIEW) {
                  return this._yearsIndex - 1 < this._minYearsIndex;
              }
              if (view === YEAR_VIEW) {
                  return this._yearIndex - 1 < this._minYearIndex;
              }
          }
          return this._pageIndex - 1 < this._minIndex;
      };
      // tslint:enable variable-name
      // ---
      CalendarViewBase.prototype._render = function (s, state) {
          var getDate = s.getDate;
          var getYear = s.getYear;
          var getMonth = s.getMonth;
          var showCalendar = s.showCalendar;
          var calendarType = s.calendarType;
          var eventRange = s.eventRange;
          var eventRangeSize = s.eventRangeSize || 1;
          var firstWeekDay = s.firstDay;
          var isWeekView = calendarType === 'week';
          var isMonthView = calendarType === 'month';
          var isYearView = calendarType === 'year';
          var size = isYearView ? 12 : +(s.size || 1);
          var isGrid = size > 1 && !isWeekView;
          var weeks = showCalendar ? (isWeekView ? s.weeks : 6) : 0;
          var active = s.activeDate || this._active || +new Date();
          var activeChanged = active !== this._active;
          var d = new Date(active);
          var prevProps = this._prevS;
          var dateFormat = s.dateFormat;
          var monthNames = s.monthNames;
          var yearSuffix = s.yearSuffix;
          var variableRow = isNumeric(s.labelList) ? +s.labelList + 1 : s.labelList === 'all' ? -1 : 0;
          var labelListingChanged = s.labelList !== prevProps.labelList;
          var navService = s.navigationService;
          var pageIndex = navService.pageIndex;
          var firstDay = navService.firstDay;
          var lastDay = navService.lastDay;
          var start = navService.viewStart;
          var end = navService.viewEnd;
          this._minDate = navService.minDate;
          this._maxDate = navService.maxDate;
          if (!isEmpty(s.min)) {
              var min = getDateOnly(this._minDate);
              this._minDate = getDateOnly(min);
              this._minYear = getDate(getYear(min), getMonth(min), 1);
              this._minYears = getYear(min);
              this._minIndex = getPageIndex(min, s);
              this._minYearIndex = getYearIndex(min, s);
              this._minYearsIndex = getYearsIndex(min, s);
          }
          else {
              this._minIndex = -Infinity;
              this._minYears = -Infinity;
              this._minYearsIndex = -Infinity;
              this._minYear = -Infinity;
              this._minYearIndex = -Infinity;
          }
          if (!isEmpty(s.max)) {
              var max = this._maxDate;
              this._maxYear = getDate(getYear(max), getMonth(max) + 1, 1);
              this._maxYears = getYear(max);
              this._maxIndex = getPageIndex(max, s);
              this._maxYearIndex = getYearIndex(max, s);
              this._maxYearsIndex = getYearsIndex(max, s);
          }
          else {
              this._maxIndex = Infinity;
              this._maxYears = Infinity;
              this._maxYearsIndex = Infinity;
              this._maxYear = Infinity;
              this._maxYearIndex = Infinity;
          }
          // We only recalculate the page index if the new active date is outside of the current view limits,
          // or page change is forced (swipe, or prev/next arrows), or the view is changed
          var viewChanged = calendarType !== prevProps.calendarType ||
              eventRange !== prevProps.eventRange ||
              firstWeekDay !== prevProps.firstDay ||
              s.eventRangeSize !== prevProps.eventRangeSize ||
              s.refDate !== prevProps.refDate ||
              s.showCalendar !== prevProps.showCalendar ||
              s.weeks !== prevProps.weeks;
          if (viewChanged && this._pageIndex !== UNDEFINED) {
              this._prevAnim = true;
          }
          if (activeChanged) {
              this._activeMonth = active;
          }
          this._view = state.view || s.selectView;
          this._yearsIndex = getYearsIndex(new Date(this._activeMonth), s);
          this._yearIndex = getYearIndex(new Date(this._activeMonth), s);
          if (this._view === YEAR_VIEW) {
              this._viewTitle = this._getPageYear(this._yearIndex) + '';
          }
          else if (this._view === MULTI_YEAR_VIEW) {
              var startYear = this._getPageYears(this._yearsIndex);
              this._viewTitle = startYear + ' - ' + (startYear + 11);
          }
          var pageNr = isGrid ? 1 : getPageNr(s.pages, state.pageSize);
          var isVertical = s.calendarScroll === 'vertical' && s.pages !== 'auto' && (s.pages === UNDEFINED || s.pages === 1);
          var showOuter = s.showOuterDays !== UNDEFINED ? s.showOuterDays : !isVertical && pageNr < 2 && (isWeekView || !size || size < 2);
          var monthIndex = dateFormat.search(/m/i);
          var yearIndex = dateFormat.search(/y/i);
          // Grid view
          if (isGrid) {
              this._monthsMulti = [];
              if (pageIndex !== UNDEFINED) {
                  // Multiplying with 0.96 and 1.1 needed, because margins and paddings are used on the month grid
                  var columns = floor((state.pageSize * 0.96) / (PAGE_WIDTH * 1.1)) || 1;
                  while (size % columns) {
                      columns--;
                  }
                  for (var i = 0; i < size / columns; ++i) {
                      var rowItems = [];
                      for (var j = 0; j < columns; ++j) {
                          rowItems.push(+getDate(getYear(firstDay), getMonth(firstDay) + i * columns + j, 1));
                      }
                      this._monthsMulti.push(rowItems);
                  }
              }
          }
          if (calendarType !== prevProps.calendarType ||
              s.theme !== prevProps.theme ||
              s.calendarScroll !== prevProps.calendarScroll ||
              s.hasContent !== prevProps.hasContent ||
              s.showCalendar !== prevProps.showCalendar ||
              s.showSchedule !== prevProps.showSchedule ||
              s.showWeekNumbers !== prevProps.showWeekNumbers ||
              s.weeks !== prevProps.weeks ||
              labelListingChanged) {
              this._shouldCheckSize = true;
          }
          if (prevProps.width !== s.width || prevProps.height !== s.height) {
              this._dim = {
                  height: addPixel(s.height),
                  width: addPixel(s.width),
              };
          }
          this._cssClass =
              'mbsc-calendar mbsc-font mbsc-flex-col' +
                  this._theme +
                  this._rtl +
                  (state.ready ? '' : ' mbsc-hidden') +
                  (isGrid ? ' mbsc-calendar-grid-view' : ' mbsc-calendar-height-' + state.height + ' mbsc-calendar-width-' + state.width) +
                  ' ' +
                  s.cssClass;
          this._dayNames = state.width === 'sm' || isGrid ? s.dayNamesMin : s.dayNamesShort;
          this._isSwipeChange = false;
          this._yearFirst = yearIndex < monthIndex;
          this._pageNr = pageNr;
          this._variableRow = variableRow;
          // Only calculate labels/marks/colors when needed
          var forcePageLoad = s.pageLoad !== prevProps.pageLoad;
          var pageChanged = +start !== +this._viewStart || +end !== +this._viewEnd;
          if (this._pageIndex !== UNDEFINED && pageChanged) {
              this._isIndexChange = !this._isSwipeChange && !viewChanged;
          }
          if (pageIndex !== UNDEFINED) {
              this._pageIndex = pageIndex;
          }
          if (pageIndex !== UNDEFINED &&
              (s.marked !== prevProps.marked ||
                  s.colors !== prevProps.colors ||
                  s.labels !== prevProps.labels ||
                  s.invalid !== prevProps.invalid ||
                  s.valid !== prevProps.valid ||
                  state.maxLabels !== this._maxLabels ||
                  pageChanged ||
                  labelListingChanged ||
                  forcePageLoad)) {
              this._maxLabels = state.maxLabels;
              this._viewStart = start;
              this._viewEnd = end;
              var labelsMap = s.labelsMap || getEventMap(s.labels, start, end, s);
              var labels = labelsMap &&
                  getLabels(s, labelsMap, start, end, this._variableRow || this._maxLabels || 1, 7, false, firstWeekDay, true, s.eventOrder, !showOuter, s.showLabelCount, s.moreEventsText, s.moreEventsPluralText);
              // If labels were not displayed previously, need to calculate how many labels can be placed
              if (labels && !this._labels) {
                  this._shouldCheckSize = true;
              }
              if ((labels && state.maxLabels) || !labels) {
                  this._shouldPageLoad = !this._isIndexChange || this._prevAnim || !showCalendar || forcePageLoad;
              }
              this._labelsLayout = labels;
              this._labels = labelsMap;
              this._marked = labelsMap ? UNDEFINED : s.marksMap || getEventMap(s.marked, start, end, s);
              this._colors = getEventMap(s.colors, start, end, s);
              this._valid = getEventMap(s.valid, start, end, s, true);
              this._invalid = getEventMap(s.invalid, start, end, s, true);
          }
          // Generate the header title
          if (pageChanged ||
              activeChanged ||
              eventRange !== prevProps.eventRange ||
              eventRangeSize !== prevProps.eventRangeSize ||
              s.monthNames !== prevProps.monthNames) {
              this._title = [];
              var lDay = addDays(lastDay, -1);
              var titleDate = pageIndex === UNDEFINED ? d : firstDay;
              // Check if a selected day is in the current view,
              // the title will be generated based on the selected day
              if (isWeekView) {
                  titleDate = d;
                  for (var _i = 0, _a = Object.keys(s.selectedDates); _i < _a.length; _i++) {
                      var key = _a[_i];
                      if (+key >= +firstDay && +key < +lastDay) {
                          titleDate = new Date(+key);
                          break;
                      }
                  }
              }
              if (this._pageNr > 1) {
                  for (var i = 0; i < pageNr; i++) {
                      var dt = getDate(getYear(firstDay), getMonth(firstDay) + i, 1);
                      var yt = getYear(dt) + yearSuffix;
                      var mt = monthNames[getMonth(dt)];
                      this._title.push({ yearTitle: yt, monthTitle: mt });
                  }
              }
              else {
                  var titleObj = { yearTitle: getYear(titleDate) + yearSuffix, monthTitle: monthNames[getMonth(titleDate)] };
                  var titleType = s.showSchedule && eventRangeSize === 1 ? eventRange : showCalendar ? calendarType : eventRange;
                  var agendaOnly = eventRange && !showCalendar && (!s.showSchedule || eventRangeSize > 1);
                  switch (titleType) {
                      case 'year': {
                          titleObj.title = getYear(firstDay) + yearSuffix;
                          if (eventRangeSize > 1) {
                              titleObj.title += ' - ' + (getYear(lDay) + yearSuffix);
                          }
                          break;
                      }
                      case 'month': {
                          if (eventRangeSize > 1 && !showCalendar) {
                              var monthStart = monthNames[getMonth(firstDay)];
                              var yearStart = getYear(firstDay) + yearSuffix;
                              var titleStart = this._yearFirst ? yearStart + ' ' + monthStart : monthStart + ' ' + yearStart;
                              var monthEnd = monthNames[getMonth(lDay)];
                              var yearEnd = getYear(lDay) + yearSuffix;
                              var titleEnd = this._yearFirst ? yearEnd + ' ' + monthEnd : monthEnd + ' ' + yearEnd;
                              titleObj.title = titleStart + ' - ' + titleEnd;
                          }
                          else if (isGrid) {
                              titleObj.title = getYear(firstDay) + yearSuffix;
                          }
                          break;
                      }
                      case 'day':
                      case 'week': {
                          if (agendaOnly) {
                              var dayIndex = dateFormat.search(/d/i);
                              var shortDateFormat = dayIndex < monthIndex ? 'D MMM, YYYY' : 'MMM D, YYYY';
                              titleObj.title = formatDate(shortDateFormat, firstDay, s);
                              if (titleType === 'week' || eventRangeSize > 1) {
                                  titleObj.title += ' - ' + formatDate(shortDateFormat, lDay, s);
                              }
                          }
                          break;
                      }
                      // case 'day': {
                      //   if (agendaOnly) {
                      //     titleObj.title = formatDate(dateFormat, firstDay, s);
                      //     if (eventRangeSize > 1) {
                      //       titleObj.title += ' - ' + formatDate(dateFormat, lDay, s);
                      //     }
                      //   }
                      // }
                  }
                  this._title.push(titleObj);
              }
          }
          this._active = active;
          this._hasPicker = s.hasPicker || isGrid || !isMonthView || !showCalendar || (state.width === 'md' && s.hasPicker !== false);
          this._axis = isVertical ? 'Y' : 'X';
          this._rtlNr = !isVertical && s.rtl ? -1 : 1;
          this._weeks = weeks;
          this._nextIcon = isVertical ? s.nextIconV : s.rtl ? s.prevIconH : s.nextIconH;
          this._prevIcon = isVertical ? s.prevIconV : s.rtl ? s.nextIconH : s.prevIconH;
          this._mousewheel = s.mousewheel === UNDEFINED ? isVertical : s.mousewheel;
          this._isGrid = isGrid;
          this._isVertical = isVertical;
          this._showOuter = showOuter;
          this._showDaysTop = isVertical || (!!variableRow && size === 1);
      };
      CalendarViewBase.prototype._mounted = function () {
          this._observer = resizeObserver(this._el, this._onResize, this._zone);
          this._doc = getDocument(this._el);
          listen(this._doc, CLICK, this._onDocClick);
      };
      CalendarViewBase.prototype._updated = function () {
          var _this = this;
          if (this._shouldCheckSize) {
              setTimeout(function () {
                  _this._onResize();
              });
              this._shouldCheckSize = false;
          }
          else if (this._shouldPageLoad) {
              // Trigger initial onPageLoaded if needed
              this._pageLoaded();
              this._shouldPageLoad = false;
          }
          if (this._shouldFocus) {
              // Angular needs setTimeout to wait for the next tick
              setTimeout(function () {
                  _this._focusActive();
                  _this._shouldFocus = false;
              });
          }
          if (this.s.instanceService) {
              this.s.instanceService.onComponentChange.next({});
          }
          this._pageChange = false;
          // TODO: why is this needed???
          if (this._variableRow && this.s.showCalendar) {
              var body = this._body.querySelector('.mbsc-calendar-body-inner');
              var hasScrollY = body.scrollHeight > body.clientHeight;
              if (hasScrollY !== this.state.hasScrollY) {
                  this._shouldCheckSize = true;
                  this.setState({ hasScrollY: hasScrollY });
              }
          }
      };
      CalendarViewBase.prototype._destroy = function () {
          if (this._observer) {
              this._observer.detach();
          }
          unlisten(this._doc, CLICK, this._onDocClick);
          clearTimeout(this._hoverTimer);
      };
      // ---
      CalendarViewBase.prototype._getActiveCell = function () {
          // TODO: get rid of direct DOM function
          var view = this._view;
          var cont = view === MONTH_VIEW ? this._body : this._pickerCont;
          var cell = view === MULTI_YEAR_VIEW ? 'year' : view === YEAR_VIEW ? 'month' : 'cell';
          return cont && cont.querySelector('.mbsc-calendar-' + cell + '[tabindex="0"]');
      };
      CalendarViewBase.prototype._focusActive = function () {
          var cell = this._getActiveCell();
          if (cell) {
              cell.focus();
          }
      };
      CalendarViewBase.prototype._pageLoaded = function () {
          var navService = this.s.navigationService;
          this._hook('onPageLoaded', {
              activeElm: this._getActiveCell(),
              firstDay: navService.firstPageDay,
              lastDay: navService.lastPageDay,
              month: this.s.calendarType === 'month' ? navService.firstDay : UNDEFINED,
              viewEnd: navService.viewEnd,
              viewStart: navService.viewStart,
          });
      };
      CalendarViewBase.prototype._activeChange = function (diff) {
          var nextIndex = this._pageIndex + diff;
          if (this._minIndex <= nextIndex && this._maxIndex >= nextIndex /* TRIALCOND */) {
              this._prevAnim = false;
              this._pageChange = true;
              this._hook('onActiveChange', {
                  date: this._getPageDay(nextIndex),
                  dir: diff,
                  pageChange: true,
              });
          }
      };
      CalendarViewBase.prototype._activeYearsChange = function (diff) {
          var nextIndex = this._yearsIndex + diff;
          if (this._minYearsIndex <= nextIndex && this._maxYearsIndex >= nextIndex) {
              var newYear = this._getPageYears(nextIndex);
              this._prevAnim = false;
              this._activeMonth = +this.s.getDate(newYear, 0, 1);
              this.forceUpdate();
          }
      };
      CalendarViewBase.prototype._activeYearChange = function (diff) {
          var nextIndex = this._yearIndex + diff;
          if (this._minYearIndex <= nextIndex && this._maxYearIndex >= nextIndex) {
              var newYear = this._getPageYear(nextIndex);
              this._prevAnim = false;
              this._activeMonth = +this.s.getDate(newYear, 0, 1);
              this.forceUpdate();
          }
      };
      CalendarViewBase.prototype._prevDocClick = function () {
          var _this = this;
          this._prevClick = true;
          setTimeout(function () {
              _this._prevClick = false;
          });
      };
      return CalendarViewBase;
  }(BaseComponent));

  var stateObservables$1 = {};
  /** @hidden */
  var CalendarLabelBase = /*#__PURE__*/ (function (_super) {
      __extends(CalendarLabelBase, _super);
      function CalendarLabelBase() {
          var _this = _super !== null && _super.apply(this, arguments) || this;
          // tslint:enable variable-name
          // tslint:disable-next-line: variable-name
          _this._onClick = function (ev) {
              if (_this._isDrag) {
                  ev.stopPropagation();
              }
              else {
                  _this._triggerEvent('onClick', ev);
                  var s = _this.s;
                  var observable = stateObservables$1[s.id];
                  if (observable && s.selected) {
                      observable.next({ hasFocus: false });
                  }
              }
          };
          // tslint:disable-next-line: variable-name
          _this._onRightClick = function (ev) {
              _this._triggerEvent('onRightClick', ev);
          };
          // tslint:disable-next-line: variable-name
          _this._onDocTouch = function (ev) {
              unlisten(_this._doc, TOUCH_START, _this._onDocTouch);
              unlisten(_this._doc, MOUSE_DOWN, _this._onDocTouch);
              _this._isDrag = false;
              _this._hook('onDragModeOff', {
                  data: _this.s.event,
                  domEvent: ev,
              });
          };
          // tslint:disable-next-line: variable-name
          _this._updateState = function (args) {
              if (_this.s.showText) {
                  _this.setState(args);
              }
          };
          // tslint:disable-next-line: variable-name
          _this._triggerEvent = function (name, ev) {
              _this._hook(name, {
                  domEvent: ev,
                  label: _this.s.event,
                  target: _this._el,
              });
          };
          return _this;
      }
      CalendarLabelBase.prototype._mounted = function () {
          var _this = this;
          var opt = this.s;
          var id = opt.id;
          var isPicker = opt.isPicker;
          var resizeDir;
          var observable = stateObservables$1[id];
          if (!observable) {
              observable = new Observable();
              stateObservables$1[id] = observable;
          }
          this._unsubscribe = observable.subscribe(this._updateState);
          this._doc = getDocument(this._el);
          this._unlisten = gestureListener(this._el, {
              keepFocus: true,
              onBlur: function () {
                  if (!isPicker) {
                      observable.next({ hasFocus: false });
                  }
              },
              onDoubleClick: function (ev) {
                  // Prevent event creation on label double click
                  ev.domEvent.stopPropagation();
                  _this._hook('onDoubleClick', {
                      domEvent: ev.domEvent,
                      label: _this.s.event,
                      target: _this._el,
                  });
              },
              onEnd: function (ev) {
                  if (_this._isDrag) {
                      var s = _this.s;
                      var args = __assign({}, ev);
                      // Will prevent mousedown event on doc
                      args.domEvent.preventDefault();
                      args.data = s.event;
                      // args.target = this._el;
                      if (s.resize && resizeDir) {
                          args.resize = true;
                          args.direction = resizeDir;
                      }
                      else if (s.drag) {
                          args.drag = true;
                      }
                      _this._hook('onDragEnd', args);
                      // Turn off update, unless we're in touch update mode
                      if (!s.isUpdate) {
                          _this._isDrag = false;
                      }
                  }
                  clearTimeout(_this._touchTimer);
                  resizeDir = UNDEFINED;
              },
              onFocus: function () {
                  if (!isPicker) {
                      observable.next({ hasFocus: true });
                  }
              },
              onHoverIn: function (ev) {
                  if (_this._isDrag || isPicker) {
                      return;
                  }
                  observable.next({ hasHover: true });
                  _this._triggerEvent('onHoverIn', ev);
              },
              onHoverOut: function (ev) {
                  observable.next({ hasHover: false });
                  _this._triggerEvent('onHoverOut', ev);
              },
              onKeyDown: function (ev) {
                  var event = _this.s.event;
                  switch (ev.keyCode) {
                      case ENTER:
                      case SPACE:
                          _this._el.click();
                          ev.preventDefault();
                          break;
                      case BACKSPACE:
                      case DELETE:
                          if (event && event.editable !== false) {
                              _this._hook('onDelete', {
                                  domEvent: ev,
                                  event: event,
                                  source: 'calendar',
                              });
                          }
                          break;
                  }
              },
              onMove: function (ev) {
                  var s = _this.s;
                  var args = __assign({}, ev);
                  args.data = s.event;
                  if (resizeDir) {
                      args.resize = true;
                      args.direction = resizeDir;
                  }
                  else if (s.drag) {
                      args.drag = true;
                  }
                  else {
                      return;
                  }
                  if (!s.event || s.event.editable === false) {
                      return;
                  }
                  if (_this._isDrag) {
                      // Prevent page scroll
                      args.domEvent.preventDefault();
                      _this._hook('onDragMove', args);
                  }
                  else if (Math.abs(args.deltaX) > 7 || Math.abs(args.deltaY) > 7) {
                      clearTimeout(_this._touchTimer);
                      if (!args.isTouch) {
                          _this._isDrag = true;
                          _this._hook('onDragStart', args);
                      }
                  }
              },
              onStart: function (ev) {
                  var s = _this.s;
                  var args = __assign({}, ev);
                  var target = args.domEvent.target;
                  args.data = s.event;
                  if (s.resize && target.classList.contains('mbsc-calendar-label-resize')) {
                      resizeDir = target.classList.contains('mbsc-calendar-label-resize-start') ? 'start' : 'end';
                      args.resize = true;
                      args.direction = resizeDir;
                  }
                  else if (s.drag) {
                      args.drag = true;
                  }
                  else {
                      return;
                  }
                  if (!s.event || s.event.editable === false) {
                      return;
                  }
                  if (_this._isDrag || !args.isTouch) {
                      // Prevent exiting drag mode in case of touch,
                      // prevent calendar swipe in case of mouse drag
                      args.domEvent.stopPropagation();
                  }
                  if (_this._isDrag) {
                      _this._hook('onDragStart', args);
                  }
                  else if (args.isTouch) {
                      _this._touchTimer = setTimeout(function () {
                          _this._hook('onDragModeOn', args);
                          _this._hook('onDragStart', args);
                          _this._isDrag = true;
                      }, 350);
                  }
              },
          });
          if (this._isDrag) {
              listen(this._doc, TOUCH_START, this._onDocTouch);
              listen(this._doc, MOUSE_DOWN, this._onDocTouch);
          }
      };
      CalendarLabelBase.prototype._destroy = function () {
          if (this._unsubscribe) {
              var id = this.s.id;
              var observable = stateObservables$1[id];
              if (observable) {
                  observable.unsubscribe(this._unsubscribe);
                  if (!observable.nr) {
                      delete stateObservables$1[id];
                  }
              }
          }
          if (this._unlisten) {
              this._unlisten();
          }
          unlisten(this._doc, TOUCH_START, this._onDocTouch);
          unlisten(this._doc, MOUSE_DOWN, this._onDocTouch);
      };
      CalendarLabelBase.prototype._render = function (s, state) {
          var event = s.event;
          var d = new Date(s.date);
          var render = s.render || s.renderContent;
          var start;
          var end;
          var isMultiDay = false;
          var isStart;
          var isEnd;
          var isEndStyle;
          var text;
          this._isDrag = this._isDrag || s.isUpdate;
          this._content = UNDEFINED;
          this._title = s.more || s.count || !s.showEventTooltip ? UNDEFINED : htmlToText(event.tooltip || event.title || event.text);
          this._tabIndex = s.isActiveMonth && s.showText && !s.count && !s.isPicker ? 0 : -1;
          if (event) {
              var allDay = event.allDay;
              var tzOpt = allDay ? UNDEFINED : s;
              start = event.start ? makeDate(event.start, tzOpt) : null;
              end = event.end ? makeDate(event.end, tzOpt) : null;
              var endTime = start && end && getEndDate(s, allDay, start, end, true);
              var firstDayOfWeek = getFirstDayOfWeek(d, s);
              var lastDayOfWeek = addDays(firstDayOfWeek, 7);
              var lastDay = s.lastDay && s.lastDay < lastDayOfWeek ? s.lastDay : lastDayOfWeek;
              isMultiDay = start && endTime && !isSameDay(start, endTime);
              isStart = !isMultiDay || (start && isSameDay(start, d));
              isEnd = !isMultiDay || (endTime && isSameDay(endTime, d));
              isEndStyle = !isMultiDay || (s.showText ? endTime < lastDay : isEnd);
              this._hasResizeStart = s.resize && isStart;
              this._hasResizeEnd = s.resize && isEndStyle;
              var color = event.color;
              if (!color && event.resource && s.resourcesMap) {
                  var resource = s.resourcesMap[isArray(event.resource) ? event.resource[0] : event.resource];
                  color = resource && resource.color;
              }
              if (s.showText) {
                  this._textColor = color ? getTextColor(color) : UNDEFINED;
              }
              this._color = s.render || s.template ? UNDEFINED : event.textColor && !color ? 'transparent' : color;
          }
          if (event && s.showText && (render || s.contentTemplate || s.template)) {
              var fillsAllDay = event.allDay || !start || (isMultiDay && !isStart && !isEnd);
              this._data = {
                  end: !fillsAllDay && isEnd && end ? formatDate(s.timeFormat, end, s) : '',
                  id: event.id,
                  isMultiDay: isMultiDay,
                  original: event,
                  start: !fillsAllDay && isStart && start ? formatDate(s.timeFormat, start, s) : '',
                  title: this._title,
              };
              if (render) {
                  var content = render(this._data);
                  if (isString(content)) {
                      text = content;
                  }
                  else {
                      this._content = content;
                  }
              }
          }
          else {
              text = s.more || s.count || (s.showText ? event.title || event.text || '' : '');
          }
          if (text !== this._text) {
              this._text = text;
              this._html = text ? this._safeHtml(text) : UNDEFINED;
              this._shouldEnhance = text && event && s.showText && !!render;
          }
          this._cssClass =
              'mbsc-calendar-text' +
                  this._theme +
                  this._rtl +
                  ((state.hasFocus && !s.inactive && !s.selected) || (s.selected && s.showText) ? ' mbsc-calendar-label-active ' : '') +
                  (state.hasHover && !s.inactive && !this._isDrag ? ' mbsc-calendar-label-hover' : '') +
                  (s.more ? ' mbsc-calendar-text-more' : s.render || s.template ? ' mbsc-calendar-custom-label' : ' mbsc-calendar-label') +
                  (s.inactive ? ' mbsc-calendar-label-inactive' : '') +
                  (s.isUpdate ? ' mbsc-calendar-label-dragging' : '') +
                  (s.hidden ? ' mbsc-calendar-label-hidden' : '') +
                  (isStart ? ' mbsc-calendar-label-start' : '') +
                  (isEndStyle ? ' mbsc-calendar-label-end' : '') +
                  (event && event.editable === false ? ' mbsc-readonly-event' : '') +
                  (event && event.cssClass ? ' ' + event.cssClass : '');
      };
      return CalendarLabelBase;
  }(BaseComponent));

  function template$b(s, inst) {
      var _a;
      var editable = s.event && s.event.editable !== false;
      var rightClick = (_a = {}, _a[ON_CONTEXT_MENU] = inst._onRightClick, _a);
      return (createElement("div", __assign({ "aria-hidden": s.showText ? UNDEFINED : 'true', className: inst._cssClass, "data-id": s.showText && s.event ? s.event.id : null, onClick: inst._onClick, ref: inst._setEl, role: s.showText ? 'button' : UNDEFINED, style: { color: inst._color }, tabIndex: inst._tabIndex, title: inst._title }, rightClick),
          inst._hasResizeStart && editable && (createElement("div", { className: 'mbsc-calendar-label-resize mbsc-calendar-label-resize-start' +
                  inst._rtl +
                  (s.isUpdate ? ' mbsc-calendar-label-resize-start-touch' : '') })),
          inst._hasResizeEnd && editable && (createElement("div", { className: 'mbsc-calendar-label-resize mbsc-calendar-label-resize-end' +
                  inst._rtl +
                  (s.isUpdate ? ' mbsc-calendar-label-resize-end-touch' : '') })),
          s.showText && !s.more && !s.render && createElement("div", { className: 'mbsc-calendar-label-background' + inst._theme }),
          s.showText && !s.more && s.render ? (inst._html ? (
          // eslint-disable-next-line react/no-danger-with-children
          createElement("div", { dangerouslySetInnerHTML: inst._html }, UNDEFINED)) : (inst._content)) : (createElement("div", { className: 'mbsc-calendar-label-inner' + inst._theme, style: { color: inst._textColor } },
              createElement("div", { "aria-hidden": "true", className: 'mbsc-calendar-label-text' + inst._theme, dangerouslySetInnerHTML: inst._html, style: { color: s.event && s.event.textColor } }, inst._content),
              s.label && createElement("div", { className: "mbsc-hidden-content" }, s.label)))));
  }
  var CalendarLabel = /*#__PURE__*/ (function (_super) {
      __extends(CalendarLabel, _super);
      function CalendarLabel() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      CalendarLabel.prototype._template = function (s) {
          return template$b(s, this);
      };
      return CalendarLabel;
  }(CalendarLabelBase));

  /** @hidden */
  var CalendarDayBase = /*#__PURE__*/ (function (_super) {
      __extends(CalendarDayBase, _super);
      function CalendarDayBase() {
          var _this = _super !== null && _super.apply(this, arguments) || this;
          // tslint:enable variable-name
          // tslint:disable-next-line variable-name
          _this._onClick = function (ev) {
              _this._cellClick('onDayClick', ev);
          };
          // tslint:disable-next-line variable-name
          _this._onRightClick = function (ev) {
              _this._cellClick('onDayRightClick', ev);
          };
          // tslint:disable-next-line variable-name
          _this._onLabelClick = function (args) {
              _this._labelClick('onLabelClick', args);
          };
          // tslint:disable-next-line variable-name
          _this._onLabelDoubleClick = function (args) {
              _this._labelClick('onLabelDoubleClick', args);
          };
          // tslint:disable-next-line variable-name
          _this._onLabelRightClick = function (args) {
              _this._labelClick('onLabelRightClick', args);
          };
          // tslint:disable-next-line variable-name
          _this._onLabelHoverIn = function (args) {
              _this._labelClick('onLabelHoverIn', args);
          };
          // tslint:disable-next-line variable-name
          _this._onLabelHoverOut = function (args) {
              _this._labelClick('onLabelHoverOut', args);
          };
          return _this;
      }
      CalendarDayBase.prototype._mounted = function () {
          var _this = this;
          var allowCreate;
          var allowStart;
          var touchTimer;
          this._unlisten = gestureListener(this._el, {
              click: true,
              onBlur: function () {
                  _this.setState({ hasFocus: false });
              },
              onDoubleClick: function (args) {
                  var s = _this.s;
                  if (s.clickToCreate && s.clickToCreate !== 'single' && s.labels && !s.disabled && s.display) {
                      _this._hook('onLabelUpdateStart', args);
                      _this._hook('onLabelUpdateEnd', args);
                  }
                  _this._cellClick('onDayDoubleClick', args.domEvent);
              },
              onEnd: function (args) {
                  if (allowCreate) {
                      // Will prevent mousedown event on doc, which would exit drag mode
                      args.domEvent.preventDefault();
                      // args.target = this._el;
                      _this._hook('onLabelUpdateEnd', args);
                      allowCreate = false;
                  }
                  clearTimeout(touchTimer);
                  allowCreate = false;
                  allowStart = false;
              },
              onFocus: function () {
                  _this.setState({ hasFocus: true });
              },
              onHoverIn: function (ev) {
                  var s = _this.s;
                  if (!s.disabled) {
                      _this.setState({ hasHover: true });
                  }
                  _this._hook('onHoverIn', {
                      date: new Date(s.date),
                      domEvent: ev,
                      hidden: !s.display,
                      outer: s.outer,
                      target: _this._el,
                  });
              },
              onHoverOut: function (ev) {
                  var s = _this.s;
                  _this.setState({ hasHover: false });
                  _this._hook('onHoverOut', {
                      date: new Date(s.date),
                      domEvent: ev,
                      hidden: !s.display,
                      outer: s.outer,
                      target: _this._el,
                  });
              },
              onKeyDown: function (ev) {
                  switch (ev.keyCode) {
                      case ENTER:
                      case SPACE:
                          ev.preventDefault();
                          _this._onClick(ev);
                          break;
                  }
              },
              onMove: function (args) {
                  if (allowCreate && _this.s.dragToCreate) {
                      args.domEvent.preventDefault();
                      _this._hook('onLabelUpdateMove', args);
                  }
                  else if (allowStart && _this.s.dragToCreate && (Math.abs(args.deltaX) > 7 || Math.abs(args.deltaY) > 7)) {
                      allowCreate = !args.isTouch;
                      _this._hook('onLabelUpdateStart', args);
                  }
                  else {
                      clearTimeout(touchTimer);
                  }
              },
              onStart: function (args) {
                  var s = _this.s;
                  args.create = true;
                  if (!s.disabled && (s.dragToCreate || s.clickToCreate) && s.labels && !allowCreate) {
                      // Check if we started on a label or not
                      var label = closest(args.domEvent.target, '.mbsc-calendar-text', _this._el);
                      if (!label) {
                          if (args.isTouch && s.dragToCreate) {
                              touchTimer = setTimeout(function () {
                                  _this._hook('onLabelUpdateStart', args);
                                  _this._hook('onLabelUpdateModeOn', args);
                                  allowCreate = true;
                              }, 350);
                          }
                          else if (s.clickToCreate === 'single') {
                              _this._hook('onLabelUpdateStart', args);
                              allowCreate = true;
                          }
                          else {
                              allowStart = !args.isTouch;
                          }
                      }
                  }
              },
          });
      };
      CalendarDayBase.prototype._render = function (s, state) {
          var now = createDate(s);
          var d = s.date;
          var colors = s.colors, display = s.display, dragData = s.dragData, hoverEnd = s.hoverEnd, hoverStart = s.hoverStart, labels = s.labels, rangeEnd = s.rangeEnd, rangeStart = s.rangeStart;
          var date = new Date(d);
          var dateKey = getDateStr(date);
          var isToday = isSameDay(now, date);
          var events = labels && labels.events;
          var color = colors && colors[0];
          var background = color && color.background;
          var highlight = color && color.highlight;
          var cellClass = '';
          var highlightClass = '';
          this._draggedLabel = dragData && dragData.draggedDates && dragData.draggedDates[dateKey];
          this._draggedLabelOrig = dragData && dragData.originDates && dragData.originDates[dateKey];
          this._todayClass = isToday ? ' mbsc-calendar-today' : '';
          this._cellStyles = background && display ? { backgroundColor: background, color: getTextColor(background) } : UNDEFINED;
          this._circleStyles = highlight ? { backgroundColor: highlight, color: getTextColor(color.highlight) } : UNDEFINED;
          this._ariaLabel =
              s.type === 'day'
                  ? (isToday ? s.todayText + ', ' : '') + s.day + ', ' + s.month + ' ' + s.text + ', ' + s.year
                  : s.type === 'month'
                      ? s.month
                      : '';
          // Only add highlight classes if the cell is actually displayed
          if (display) {
              // range selection can start with a rangeStart or with a rangeEnd without the other
              // the same classes are needed in both cases
              if ((rangeStart && d >= rangeStart && d <= (rangeEnd || rangeStart)) ||
                  (rangeEnd && d <= rangeEnd && d >= (rangeStart || rangeEnd))) {
                  highlightClass =
                      ' mbsc-range-day' +
                          (d === (rangeStart || rangeEnd) ? ' mbsc-range-day-start' : '') +
                          (d === (rangeEnd || rangeStart) ? ' mbsc-range-day-end' : '');
              }
              if (hoverStart && hoverEnd && d >= hoverStart && d <= hoverEnd) {
                  highlightClass +=
                      ' mbsc-range-hover' +
                          (d === hoverStart ? ' mbsc-range-hover-start mbsc-hover' : '') +
                          (d === hoverEnd ? ' mbsc-range-hover-end mbsc-hover' : '');
              }
          }
          if (s.marks) {
              s.marks.forEach(function (e) {
                  cellClass += e.cellCssClass ? ' ' + e.cellCssClass : '';
              });
          }
          if (colors) {
              colors.forEach(function (e) {
                  cellClass += e.cellCssClass ? ' ' + e.cellCssClass : '';
              });
          }
          if (events) {
              events.forEach(function (e) {
                  cellClass += e.cellCssClass ? ' ' + e.cellCssClass : '';
              });
          }
          this._cssClass =
              'mbsc-calendar-cell mbsc-flex-1-0-0 mbsc-calendar-' +
                  s.type +
                  this._theme +
                  this._rtl +
                  this._hb +
                  cellClass +
                  (labels ? ' mbsc-calendar-day-labels' : '') +
                  (colors ? ' mbsc-calendar-day-colors' : '') +
                  (s.outer ? ' mbsc-calendar-day-outer' : '') +
                  (s.hasMarks ? ' mbsc-calendar-day-marked' : '') +
                  (s.disabled ? ' mbsc-disabled' : '') +
                  (display ? '' : ' mbsc-calendar-day-empty') +
                  (s.selected ? ' mbsc-selected' : '') +
                  (state.hasFocus ? ' mbsc-focus' : '') +
                  // hover styling needed only on hoverStart and hoverEnd dates in the case of range hover
                  // we can tell if no range hover is in place when neither hoverStart nor hoverEnd is there
                  (state.hasHover && (d === hoverStart || d === hoverEnd || (!hoverStart && !hoverEnd)) ? ' mbsc-hover' : '') +
                  (this._draggedLabel ? ' mbsc-calendar-day-highlight' : '') +
                  highlightClass;
          this._data = {
              date: date,
              events: s.events || [],
              selected: s.selected,
          };
      };
      CalendarDayBase.prototype._destroy = function () {
          if (this._unlisten) {
              this._unlisten();
          }
      };
      CalendarDayBase.prototype._cellClick = function (name, domEvent) {
          var s = this.s;
          if (s.display) {
              this._hook(name, {
                  date: new Date(s.date),
                  disabled: s.disabled,
                  domEvent: domEvent,
                  outer: s.outer,
                  selected: s.selected,
                  source: 'calendar',
                  target: this._el,
              });
          }
      };
      CalendarDayBase.prototype._labelClick = function (name, args) {
          var s = this.s;
          args.date = new Date(s.date);
          args.labels = s.labels.events;
          this._hook(name, args);
      };
      return CalendarDayBase;
  }(BaseComponent));

  function renderEvent(inst, s, label, showText, hidden, isUpdate, key) {
      return (createElement(CalendarLabel, { key: key, amText: s.amText, count: label.count ? label.count + ' ' + (label.count > 1 ? s.eventsText : s.eventText) : UNDEFINED, date: s.date, dataTimezone: s.dataTimezone, displayTimezone: s.displayTimezone, drag: s.dragToMove, resize: computeEventResize(label.event && label.event.resize, s.dragToResize), event: label.event, exclusiveEndDates: s.exclusiveEndDates, firstDay: s.firstDay, hidden: hidden, id: label.id, inactive: !isUpdate && label.event && s.dragData && s.dragData.draggedEvent && label.event.id === s.dragData.draggedEvent.id, isActiveMonth: s.isActiveMonth, isPicker: s.isPicker, isUpdate: isUpdate, label: label.label, lastDay: label.lastDay, more: label.more, pmText: s.pmText, resourcesMap: s.resourcesMap, rtl: s.rtl, selected: label.event && s.selectedEventsMap && !!(s.selectedEventsMap[label.id] || s.selectedEventsMap[label.event.id]), showEventTooltip: s.showEventTooltip, showText: showText, theme: s.theme, timeFormat: s.timeFormat, timezonePlugin: s.timezonePlugin, render: s.renderLabel, renderContent: s.renderLabelContent, onClick: inst._onLabelClick, onDoubleClick: inst._onLabelDoubleClick, onRightClick: inst._onLabelRightClick, onHoverIn: inst._onLabelHoverIn, onHoverOut: inst._onLabelHoverOut, onDelete: s.onLabelDelete, onDragStart: s.onLabelUpdateStart, onDragMove: s.onLabelUpdateMove, onDragEnd: s.onLabelUpdateEnd, onDragModeOn: s.onLabelUpdateModeOn, onDragModeOff: s.onLabelUpdateModeOff }));
  }
  function renderLabel(inst, s, label) {
      var key = label.id;
      if (label.placeholder) {
          return createElement("div", { className: "mbsc-calendar-text mbsc-calendar-text-placeholder", key: key });
      }
      if (label.more || label.count) {
          return renderEvent(inst, s, label, true, false, false, key);
      }
      return label.multiDay
          ? [
              createElement("div", { className: "mbsc-calendar-label-wrapper", style: { width: label.width + '%' }, key: key }, renderEvent(inst, s, label, true)),
              renderEvent(inst, s, label, false, false, false, '-' + key),
          ]
          : renderEvent(inst, s, label, label.showText, false, false, key);
  }
  function template$c(s, inst) {
      var _a;
      var draggedLabel = inst._draggedLabel;
      var draggedLabelOrig = inst._draggedLabelOrig;
      var theme = inst._theme;
      var rightClick = (_a = {}, _a[ON_CONTEXT_MENU] = inst._onRightClick, _a);
      var content;
      if (s.renderDay) {
          content = s.renderDay(inst._data);
      }
      if (s.renderDayContent) {
          content = s.renderDayContent(inst._data);
      }
      if (isString(content)) {
          content = createElement("div", { dangerouslySetInnerHTML: inst._safeHtml(content) });
          inst._shouldEnhance = true;
      }
      return (createElement("div", __assign({ ref: inst._setEl, className: inst._cssClass, onClick: inst._onClick, style: inst._cellStyles, tabIndex: s.disabled ? UNDEFINED : s.active ? 0 : -1 }, rightClick),
          createElement("div", { className: 'mbsc-calendar-cell-inner mbsc-calendar-' +
                  s.type +
                  '-inner' +
                  theme +
                  (s.type === 'day' ? '' : inst._hb) +
                  (s.display ? '' : ' mbsc-calendar-day-hidden') },
              s.renderDay ? (content) : (createElement(Fragment, null,
                  s.text === 1 && (createElement("div", { "aria-hidden": "true", className: 'mbsc-calendar-month-name' + theme + inst._rtl }, s.monthShort)),
                  createElement("div", { "aria-label": inst._ariaLabel, role: "button", "aria-pressed": s.selected, className: 'mbsc-calendar-cell-text mbsc-calendar-' + s.type + '-text' + theme + inst._todayClass, style: inst._circleStyles }, s.text),
                  s.marks && ( // Extra div is needed in RTL, otherwise position is wrong in Chrome
                  createElement("div", null,
                      createElement("div", { className: 'mbsc-calendar-marks' + theme + inst._rtl }, s.marks.map(function (mark, k) { return (createElement("div", { className: 'mbsc-calendar-mark ' + (mark.markCssClass || '') + theme, key: k, style: { background: mark.color } })); })))),
                  s.renderDayContent && content)),
              s.labels && ( // Extra div is needed in RTL, otherwise position is wrong in Chrome
              createElement("div", null,
                  draggedLabelOrig && draggedLabelOrig.event && (createElement("div", { className: "mbsc-calendar-labels mbsc-calendar-labels-dragging" },
                      createElement("div", { style: { width: draggedLabelOrig.width + '%' || 100 + '%' } }, renderEvent(inst, s, { id: 0, event: draggedLabelOrig.event }, true, !!s.dragData.draggedDates, true)))),
                  draggedLabel && draggedLabel.event && (createElement("div", { className: "mbsc-calendar-labels mbsc-calendar-labels-dragging" },
                      createElement("div", { className: "mbsc-calendar-label-wrapper", style: { width: draggedLabel.width + '%' || 100 + '%' } }, renderEvent(inst, s, { id: 0, event: draggedLabel.event }, true, false, true)))),
                  createElement("div", { className: "mbsc-calendar-labels" }, s.labels.data.map(function (label) {
                      return renderLabel(inst, s, label);
                  })),
                  createElement("div", { className: "mbsc-calendar-text mbsc-calendar-text-placeholder" }))))));
  }
  var CalendarDay = /*#__PURE__*/ (function (_super) {
      __extends(CalendarDay, _super);
      function CalendarDay() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      CalendarDay.prototype._template = function (s) {
          return template$c(s, this);
      };
      return CalendarDay;
  }(CalendarDayBase));

  /** @jsxRuntime classic */
  /** @hidden */
  var CalendarWeekDays = function (props) {
      var firstDay = props.firstDay, hidden = props.hidden, rtl = props.rtl, theme = props.theme, dayNamesShort = props.dayNamesShort, showWeekNumbers = props.showWeekNumbers, hasScroll = props.hasScroll;
      return (createElement("div", { "aria-hidden": "true", className: 'mbsc-calendar-week-days mbsc-flex' + (hidden ? ' mbsc-hidden' : '') },
          showWeekNumbers && createElement("div", { className: 'mbsc-calendar-week-day mbsc-flex-none mbsc-calendar-week-nr' + theme + rtl }),
          ARRAY7.map(function (x, i) { return (createElement("div", { className: 'mbsc-calendar-week-day mbsc-flex-1-0-0' + theme + rtl, key: i }, dayNamesShort[(i + firstDay) % 7])); }),
          hasScroll && createElement("div", { className: "mbsc-schedule-fake-scroll-y" })));
  };

  /** @hidden */
  var MonthViewBase = /*#__PURE__*/ (function (_super) {
      __extends(MonthViewBase, _super);
      function MonthViewBase() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      MonthViewBase.prototype._isActive = function (d) {
          return this.s.isActive && d === this.s.activeDate;
      };
      MonthViewBase.prototype._isInvalid = function (d) {
          var s = this.s;
          var localDate = new Date(d);
          var timezoneDate = addTimezone(s, localDate);
          return isInvalid(s, timezoneDate, s.invalid, s.valid, +s.min, +s.max);
      };
      MonthViewBase.prototype._isSelected = function (d) {
          var localDate = new Date(d);
          var timezoneDate = addTimezone(this.s, localDate);
          return !!this.s.selectedDates[+timezoneDate];
      };
      MonthViewBase.prototype._getWeekNr = function (s, date) {
          var d = new Date(date);
          return '' + s.getWeekNumber(s.getDate(d.getFullYear(), d.getMonth(), d.getDate() + ((7 - s.firstDay + 1) % 7)));
      };
      // tslint:enable variable-name
      MonthViewBase.prototype._render = function (s) {
          // TODO: optimize what to calculate on render
          var weeks = s.weeks;
          var firstWeekDay = s.firstDay;
          var firstDay = new Date(s.firstPageDay);
          var year = s.getYear(firstDay);
          var month = s.getMonth(firstDay);
          var day = s.getDay(firstDay);
          var weekDay = s.getDate(year, month, day).getDay();
          var offset = firstWeekDay - weekDay > 0 ? 7 : 0;
          var row = [];
          var maxLabels = 0;
          this._rowHeights = [];
          this._rows = [];
          this._days = ARRAY7;
          for (var i = 0; i < 7 * weeks; i++) {
              var curr = s.getDate(year, month, i + firstWeekDay - offset - weekDay + day);
              var key = getDateStr(curr);
              var displayMonth = s.getMonth(curr);
              // let y = curr.getFullYear();
              // let m = curr.getMonth();
              // let d = curr.getDate();
              var outer = displayMonth !== month && s.calendarType !== 'week';
              var marked = s.marked && s.marked[key];
              var marks = marked ? (s.showSingleMark ? [{}] : marked) : null;
              var labels = s.labels && s.labels[key];
              var labelCount = labels ? labels.data.length : 0;
              var isWeekStart = i % 7 === 0;
              if (s.variableRow) {
                  // Don't render rows containing fully outer days
                  if (isWeekStart && outer && i) {
                      break;
                  }
                  if (labelCount > maxLabels) {
                      maxLabels = labelCount;
                  }
                  // Row end
                  if (i % 7 === 6) {
                      this._rowHeights.push(maxLabels * (s.labelHeight || 20) + (s.cellTextHeight || 0) + 3);
                      maxLabels = 0;
                  }
              }
              if (isWeekStart) {
                  row = [];
                  this._rows.push(row);
              }
              row.push({
                  colors: s.colors && s.colors[key],
                  date: +curr,
                  day: s.dayNames[curr.getDay()],
                  display: outer ? s.showOuter : true,
                  events: s.events && s.events[key],
                  labels: labels,
                  marks: marks,
                  month: s.monthNames[displayMonth],
                  monthShort: s.monthNamesShort[displayMonth],
                  outer: outer,
                  text: s.getDay(curr),
                  year: s.getYear(curr),
              });
          }
      };
      return MonthViewBase;
  }(BaseComponent));

  function template$d(s, inst) {
      var showWeekNumbers = s.showWeekNumbers;
      var calWeekDays = s.showWeekDays ? (createElement(CalendarWeekDays, { dayNamesShort: s.dayNamesShort, firstDay: s.firstDay, rtl: inst._rtl, showWeekNumbers: showWeekNumbers, theme: inst._theme })) : null;
      return (createElement("div", { "aria-hidden": s.isActive ? UNDEFINED : 'true', className: 'mbsc-calendar-table mbsc-flex-col mbsc-flex-1-1' + (s.isActive ? ' mbsc-calendar-table-active' : '') },
          calWeekDays,
          inst._rows.map(function (row, i) {
              var weekNr = showWeekNumbers ? inst._getWeekNr(s, row[0].date) : '';
              return (createElement("div", { className: 'mbsc-calendar-row mbsc-flex mbsc-flex-1-0', key: i, style: { minHeight: inst._rowHeights[i] } },
                  showWeekNumbers && (createElement("div", { className: 'mbsc-calendar-cell mbsc-flex-none mbsc-calendar-day mbsc-calendar-week-nr' + inst._theme },
                      createElement("div", { "aria-hidden": "true" }, weekNr),
                      createElement("div", { className: "mbsc-hidden-content" }, s.weekText.replace('{count}', weekNr)))),
                  row.map(function (cell, j) { return (createElement(CalendarDay, { active: cell.display && inst._isActive(cell.date), amText: s.amText, clickToCreate: s.clickToCreate, colors: cell.colors, date: cell.date, day: cell.day, disabled: inst._isInvalid(cell.date), display: cell.display, dataTimezone: s.dataTimezone, displayTimezone: s.displayTimezone, dragData: s.dragData, dragToCreate: s.dragToCreate, dragToResize: s.dragToResize, dragToMove: s.dragToMove, eventText: s.eventText, events: cell.events, eventsText: s.eventsText, exclusiveEndDates: s.exclusiveEndDates, firstDay: s.firstDay, hasMarks: s.hasMarks, hoverEnd: s.hoverEnd, hoverStart: s.hoverStart, isActiveMonth: s.isActive, isPicker: s.isPicker, key: cell.date, labels: cell.labels, pmText: s.pmText, marks: cell.marks, month: cell.month, monthShort: cell.monthShort, onDayClick: s.onDayClick, onDayDoubleClick: s.onDayDoubleClick, onDayRightClick: s.onDayRightClick, onLabelClick: s.onLabelClick, onLabelDoubleClick: s.onLabelDoubleClick, onLabelRightClick: s.onLabelRightClick, onLabelHoverIn: s.onLabelHoverIn, onLabelHoverOut: s.onLabelHoverOut, onLabelDelete: s.onLabelDelete, onLabelUpdateStart: s.onLabelUpdateStart, onLabelUpdateMove: s.onLabelUpdateMove, onLabelUpdateEnd: s.onLabelUpdateEnd, onLabelUpdateModeOn: s.onLabelUpdateModeOn, onLabelUpdateModeOff: s.onLabelUpdateModeOff, outer: cell.outer, renderDay: s.renderDay, renderDayContent: s.renderDayContent, renderLabel: s.renderLabel, renderLabelContent: s.renderLabelContent, rangeEnd: s.rangeEnd, rangeStart: s.rangeStart, resourcesMap: s.resourcesMap, selectedEventsMap: s.selectedEventsMap, rtl: s.rtl, showEventTooltip: s.showEventTooltip, selected: inst._isSelected(cell.date), text: cell.text, theme: s.theme, timeFormat: s.timeFormat, timezonePlugin: s.timezonePlugin, todayText: s.todayText, type: "day", year: cell.year, 
                      // In case of Preact we need to force update by always passing a new object,
                      // otherwise sometimes DOM elements will mix up
                      // update={isPreact ? {} : 0}
                      onHoverIn: s.onDayHoverIn, onHoverOut: s.onDayHoverOut })); })));
          })));
  }
  /** @hidden */
  var MonthView = /*#__PURE__*/ (function (_super) {
      __extends(MonthView, _super);
      function MonthView() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      MonthView.prototype._template = function (s) {
          return template$d(s, this);
      };
      return MonthView;
  }(MonthViewBase));

  // TODO: snap points
  function getItem(items, i, min, max) {
      var item;
      if (i < min || i > max) {
          return;
      }
      if (isArray(items)) {
          var len = items.length;
          var index = i % len;
          item = items[index >= 0 ? index : index + len];
      }
      else {
          item = items(i);
      }
      return item;
  }
  /** @hidden */
  var ScrollviewBase = /*#__PURE__*/ (function (_super) {
      __extends(ScrollviewBase, _super);
      function ScrollviewBase() {
          var _this = _super !== null && _super.apply(this, arguments) || this;
          _this._currPos = 0;
          _this._delta = 0;
          _this._endPos = 0;
          _this._lastRaf = 0;
          _this._maxSnapScroll = 0;
          _this._margin = 0;
          _this._scrollEnd = debounce(function () {
              rafc(_this._raf);
              _this._raf = false;
              _this._onEnd();
              _this._hasScrolled = false;
          }, 200);
          // tslint:enable variable-name
          // tslint:disable-next-line: variable-name
          _this._setInnerEl = function (el) {
              _this._innerEl = el;
          };
          // tslint:disable-next-line: variable-name
          _this._setScrollEl = function (el) {
              _this._scrollEl = el;
          };
          // tslint:disable-next-line: variable-name
          _this._setScrollEl3d = function (el) {
              _this._scrollEl3d = el;
          };
          // tslint:disable-next-line: variable-name
          _this._setScrollbarEl = function (el) {
              _this._scrollbarEl = el;
          };
          // tslint:disable-next-line: variable-name
          _this._setScrollbarContEl = function (el) {
              _this._scrollbarContEl = el;
          };
          // tslint:disable-next-line: variable-name
          _this._onStart = function (args) {
              // const ev = args.domEvent;
              var s = _this.s;
              _this._hook('onStart', {});
              // Don't allow new gesture if new items are only generated on animation end OR
              // mouse swipe is not enabled OR
              // swipe is completely disabled
              if ((s.changeOnEnd && _this._isScrolling) || (!s.mouseSwipe && !args.isTouch) || !s.swipe) {
                  return;
              }
              // Better performance if there are tap events on document
              // if (s.stopProp) {
              //   ev.stopPropagation();
              // }
              // TODO: check this, will prevent click on touch device
              // if (s.prevDef) {
              //   // Prevent touch highlight and focus
              //   ev.preventDefault();
              // }
              _this._started = true;
              _this._hasScrolled = _this._isScrolling;
              _this._currX = args.startX;
              _this._currY = args.startY;
              _this._delta = 0;
              _this._velocityX = 0;
              _this._velocityY = 0;
              _this._startPos = getPosition(_this._scrollEl, _this._isVertical);
              _this._timestamp = +new Date();
              if (_this._isScrolling) {
                  // Stop running movement
                  rafc(_this._raf);
                  _this._raf = false;
                  _this._scroll(_this._startPos);
              }
          };
          // tslint:disable-next-line: variable-name
          _this._onMove = function (args) {
              var ev = args.domEvent;
              var s = _this.s;
              if (_this._isVertical || s.scrollLock) {
                  // Always prevent native scroll, if vertical
                  if (ev.cancelable) {
                      ev.preventDefault();
                  }
              }
              else {
                  if (_this._hasScrolled) {
                      // Prevent native scroll
                      if (ev.cancelable) {
                          ev.preventDefault();
                      }
                  }
                  else if (ev.type === TOUCH_MOVE && (Math.abs(args.deltaY) > 7 || !s.swipe)) {
                      // It's a native scroll, stop listening
                      _this._started = false;
                  }
              }
              if (!_this._started) {
                  return;
              }
              _this._delta = _this._isVertical ? args.deltaY : args.deltaX;
              if (_this._hasScrolled || Math.abs(_this._delta) > _this._threshold) {
                  if (!_this._hasScrolled) {
                      _this._hook('onGestureStart', {});
                  }
                  _this._hasScrolled = true;
                  _this._isScrolling = true;
                  if (!_this._raf) {
                      _this._raf = raf(function () { return _this._move(args); });
                  }
              }
          };
          // tslint:disable-next-line: variable-name
          _this._onEnd = function () {
              _this._started = false;
              if (_this._hasScrolled) {
                  var s = _this.s;
                  var v = (_this._isVertical ? _this._velocityY : _this._velocityX) * 17;
                  var maxSnapScroll = _this._maxSnapScroll;
                  var delta = _this._delta;
                  var time = 0;
                  // Calculate stopping distance
                  // TODO: speedUnit
                  delta += v * v * 0.5 * (v < 0 ? -1 : 1);
                  // Allow only max snap
                  if (maxSnapScroll) {
                      delta = constrain(delta, -_this._round * maxSnapScroll, _this._round * maxSnapScroll);
                  }
                  // Round and limit between min/max
                  var pos = constrain(round((_this._startPos + delta) / _this._round) * _this._round, _this._min, _this._max);
                  var index = round((-pos * _this._rtlNr) / s.itemSize) + _this._offset;
                  var direction = delta > 0 ? (_this._isVertical ? 270 : 360) : _this._isVertical ? 90 : 180;
                  var diff = index - s.selectedIndex;
                  // Calculate animation time
                  // TODO: timeUnit
                  time = s.time || Math.max(1000, Math.abs(pos - _this._currPos) * 3);
                  _this._hook('onGestureEnd', { direction: direction, index: index });
                  // needed for the infinite scrollbar to be cleared at each end
                  _this._delta = 0;
                  // Set new position
                  _this._scroll(pos, time);
                  if (diff && !s.changeOnEnd) {
                      _this._hook('onIndexChange', { index: index, diff: diff });
                      // In case if the onIndexChange handler leaves the index at the previous position,
                      // we need a force update to move the wheel back to the correct position
                      if (s.selectedIndex === _this._prevIndex && s.selectedIndex !== index) {
                          _this.forceUpdate();
                      }
                  }
              }
          };
          // tslint:disable-next-line: variable-name
          _this._onClick = function (ev) {
              if (_this._hasScrolled) {
                  _this._hasScrolled = false;
                  ev.stopPropagation();
                  ev.preventDefault();
              }
          };
          // tslint:disable-next-line: variable-name
          _this._onScroll = function (ev) {
              ev.target.scrollTop = 0;
              ev.target.scrollLeft = 0;
          };
          // tslint:disable-next-line: variable-name
          _this._onMouseWheel = function (ev) {
              var delta = _this._isVertical ? (ev.deltaY === UNDEFINED ? ev.wheelDelta || ev.detail : ev.deltaY) : ev.deltaX;
              if (delta && _this.s.mousewheel) {
                  ev.preventDefault();
                  _this._hook('onStart', {});
                  if (!_this._started) {
                      _this._delta = 0;
                      _this._velocityX = 0;
                      _this._velocityY = 0;
                      _this._startPos = _this._currPos;
                      _this._hook('onGestureStart', {});
                  }
                  if (ev.deltaMode && ev.deltaMode === 1) {
                      delta *= 15;
                  }
                  delta = constrain(-delta, -_this._scrollSnap, _this._scrollSnap);
                  _this._delta += delta;
                  if (_this._maxSnapScroll && Math.abs(_this._delta) > _this._round * _this._maxSnapScroll) {
                      delta = 0;
                  }
                  if (_this._startPos + _this._delta < _this._min) {
                      _this._startPos = _this._min;
                      _this._delta = 0;
                      delta = 0;
                  }
                  if (_this._startPos + _this._delta > _this._max) {
                      _this._startPos = _this._max;
                      _this._delta = 0;
                      delta = 0;
                  }
                  if (!_this._raf) {
                      _this._raf = raf(function () { return _this._move(); });
                  }
                  if (!delta && _this._started) {
                      return;
                  }
                  _this._hasScrolled = true;
                  _this._isScrolling = true;
                  _this._started = true;
                  _this._scrollEnd();
              }
          };
          // tslint:disable-next-line: variable-name
          _this._onTrackStart = function (ev) {
              ev.stopPropagation();
              var args = {
                  domEvent: ev,
                  startX: getCoord(ev, 'X', true),
                  startY: getCoord(ev, 'Y', true),
              };
              _this._onStart(args);
              _this._trackStartX = args.startX;
              _this._trackStartY = args.startY;
              if (ev.target === _this._scrollbarEl) {
                  listen(_this._doc, MOUSE_UP, _this._onTrackEnd);
                  listen(_this._doc, MOUSE_MOVE, _this._onTrackMove);
              }
              else {
                  // this._trackStartY = getOffset(this._scrollbarEl).top;
                  var top_1 = getOffset(_this._scrollbarContEl).top;
                  var percent = (args.startY - top_1) / _this._barContSize;
                  _this._startPos = _this._currPos = _this._max + (_this._min - _this._max) * percent;
                  _this._hasScrolled = true;
                  _this._onEnd();
              }
          };
          // tslint:disable-next-line: variable-name
          _this._onTrackMove = function (ev) {
              var barContSize = _this._barContSize;
              var endX = getCoord(ev, 'X', true);
              var endY = getCoord(ev, 'Y', true);
              var trackDelta = _this._isVertical ? endY - _this._trackStartY : endX - _this._trackStartX;
              var percent = trackDelta / barContSize;
              if (_this._isInfinite) {
                  _this._delta = -(_this._maxSnapScroll * _this._round * 2 + barContSize) * percent;
              }
              else {
                  _this._delta = (_this._min - _this._max - barContSize) * percent;
              }
              if (_this._hasScrolled || Math.abs(_this._delta) > _this._threshold) {
                  if (!_this._hasScrolled) {
                      _this._hook('onGestureStart', {});
                  }
                  _this._hasScrolled = true;
                  _this._isScrolling = true;
                  if (!_this._raf) {
                      _this._raf = raf(function () { return _this._move({ endX: endX, endY: endY }, !_this._isInfinite); });
                  }
              }
          };
          // tslint:disable-next-line: variable-name
          _this._onTrackEnd = function () {
              _this._delta = 0;
              _this._startPos = _this._currPos;
              _this._onEnd();
              unlisten(_this._doc, MOUSE_UP, _this._onTrackEnd);
              unlisten(_this._doc, MOUSE_MOVE, _this._onTrackMove);
          };
          // tslint:disable-next-line: variable-name
          _this._onTrackClick = function (ev) {
              ev.stopPropagation();
          };
          return _this;
      }
      ScrollviewBase.prototype._render = function (s, state) {
          // console.log('scrollview render', s.selectedIndex);
          var prevS = this._prevS;
          var batchSize = s.batchSize;
          var batchSize3d = s.batchSize3d;
          var itemNr = s.itemNr || 1;
          var itemSize = s.itemSize;
          // Index of the selected item
          var selectedIndex = s.selectedIndex;
          // Index of the previously selected item;
          var prevIndex = prevS.selectedIndex;
          // Index of the actual middle item during animation
          var currIndex = state.index === UNDEFINED ? selectedIndex : state.index;
          var visibleItems = [];
          var visible3dItems = [];
          var diff = selectedIndex - prevIndex;
          var diff2 = currIndex - this._currIndex;
          var minIndex = s.minIndex;
          var maxIndex = s.maxIndex;
          var items = s.items;
          var offset = s.offset;
          this._currIndex = currIndex;
          this._isVertical = s.axis === 'Y';
          this._threshold = this._isVertical ? s.thresholdY : s.thresholdX;
          this._rtlNr = !this._isVertical && s.rtl ? -1 : 1;
          this._round = s.snap ? itemSize : 1;
          var scrollSnap = this._round;
          while (scrollSnap > 44) {
              scrollSnap /= 2;
          }
          this._scrollSnap = round(44 / scrollSnap) * scrollSnap;
          if (items) {
              for (var i = currIndex - batchSize; i < currIndex + itemNr + batchSize; i++) {
                  visibleItems.push({ key: i, data: getItem(items, i, minIndex, maxIndex) });
              }
              if (s.scroll3d) {
                  for (var i = currIndex - batchSize3d; i < currIndex + itemNr + batchSize3d; i++) {
                      visible3dItems.push({ key: i, data: getItem(items, i, minIndex, maxIndex) });
                  }
              }
              this.visibleItems = visibleItems;
              this.visible3dItems = visible3dItems;
              this._maxSnapScroll = batchSize;
              this._isInfinite = typeof items === 'function';
          }
          if (this._offset === UNDEFINED) {
              this._offset = selectedIndex;
          }
          var nextPos = -(selectedIndex - this._offset) * itemSize * this._rtlNr;
          if (Math.abs(diff) > batchSize && nextPos !== this._endPos) {
              var off = diff + batchSize * (diff > 0 ? -1 : 1);
              this._offset += off;
              this._margin -= off;
          }
          if (offset && offset !== prevS.offset) {
              this._offset += offset;
              this._margin -= offset;
          }
          if (diff2) {
              this._margin += diff2;
          }
          if (minIndex !== UNDEFINED) {
              this._max = -(minIndex - this._offset) * itemSize * this._rtlNr;
          }
          else {
              this._max = Infinity;
          }
          if (maxIndex !== UNDEFINED) {
              this._min = -(maxIndex - this._offset - (s.spaceAround ? 0 : itemNr - 1)) * itemSize * this._rtlNr;
          }
          else {
              this._min = -Infinity;
          }
          if (this._rtlNr === -1) {
              var temp = this._min;
              this._min = this._max;
              this._max = temp;
          }
          if (this._min > this._max) {
              this._min = this._max;
          }
          var visibleSize = s.visibleSize;
          var barContSize = visibleSize * itemSize;
          this._barContSize = barContSize;
          this._barSize = Math.max(20, (barContSize * barContSize) / (this._max - this._min + barContSize));
          this._cssClass = this._className + ' mbsc-ltr';
          // TODO: get rid of this:
          // (!s.scrollBar || this._barSize === this._barContSize ? ' mbsc-scroller-bar-none' : '');
      };
      ScrollviewBase.prototype._mounted = function () {
          // TODO: calculate scroll sizes, if not infinite
          // const s = this.s;
          // this.size = this.isVertical ? this.cont.clientHeight : this.cont.clientWidth;
          // this.max = 0;
          // this.min = Math.min(this.max, Math.min(0, this.size - (this.isVertical ? this.el.offsetHeight : this.el.offsetWidth)));
          // this.max = Infinity;
          // this.min = -Infinity;
          this._doc = getDocument(this._el);
          listen(this.s.scroll3d ? this._innerEl : this._el, SCROLL, this._onScroll);
          listen(this._el, CLICK, this._onClick, true);
          listen(this._el, MOUSE_WHEEL, this._onMouseWheel, { passive: false });
          listen(this._el, WHEEL, this._onMouseWheel, { passive: false });
          listen(this._scrollbarContEl, MOUSE_DOWN, this._onTrackStart);
          listen(this._scrollbarContEl, CLICK, this._onTrackClick);
          this._unlisten = gestureListener(this._el, {
              onEnd: this._onEnd,
              onMove: this._onMove,
              onStart: this._onStart,
              prevDef: true,
          });
      };
      ScrollviewBase.prototype._updated = function () {
          var s = this.s;
          var batchSize = s.batchSize;
          var itemSize = s.itemSize;
          // const selectedIndex = s.selectedIndex! < s.minIndex! ? s.minIndex! : s.selectedIndex!;
          var selectedIndex = s.selectedIndex;
          var prevIndex = this._prevIndex;
          var shouldAnimate = !s.prevAnim && ((prevIndex !== UNDEFINED && prevIndex !== selectedIndex) || this._isAnimating);
          var newPos = -(selectedIndex - this._offset) * itemSize * this._rtlNr;
          if (s.margin) {
              this._scrollEl.style.marginTop = this._isVertical ? (this._margin - batchSize) * itemSize + 'px' : '';
          }
          // Scroll to the new position, but only if the view is not being moved currently
          // The _scroll function will call _infinite, so if the index is changed from outside
          // compared to the index stored in the state, this will ensure to update the index in the state,
          // to regenerate the visible items
          if (!this._started) {
              this._scroll(newPos, shouldAnimate ? this._isAnimating || s.time || 1000 : 0);
          }
          this._prevIndex = selectedIndex;
      };
      ScrollviewBase.prototype._destroy = function () {
          unlisten(this.s.scroll3d ? this._innerEl : this._el, SCROLL, this._onScroll);
          unlisten(this._el, CLICK, this._onClick, true);
          unlisten(this._el, MOUSE_WHEEL, this._onMouseWheel, { passive: false });
          unlisten(this._el, WHEEL, this._onMouseWheel, { passive: false });
          unlisten(this._scrollbarContEl, MOUSE_DOWN, this._onTrackStart);
          unlisten(this._scrollbarContEl, CLICK, this._onTrackClick);
          rafc(this._raf);
          this._raf = false;
          // Need to reset scroll because Preact recycles the DOM element
          this._scroll(0);
          this._unlisten();
      };
      /**
       * Maintains the current position during animation
       */
      ScrollviewBase.prototype._anim = function (dir) {
          var _this = this;
          return (this._raf = raf(function () {
              var s = _this.s;
              var now = +new Date();
              // Component was destroyed
              if (!_this._raf) {
                  return;
              }
              if ((_this._currPos - _this._endPos) * -dir < 4) {
                  _this._currPos = _this._endPos;
                  _this._raf = false;
                  _this._isAnimating = 0;
                  _this._isScrolling = false;
                  _this._infinite(_this._currPos);
                  _this._hook('onAnimationEnd', {});
                  _this._scrollbarContEl.classList.remove('mbsc-scroller-bar-started'); // hide scrollbar after animation finished
                  return;
              }
              if (now - _this._lastRaf > 100) {
                  _this._lastRaf = now;
                  _this._currPos = getPosition(_this._scrollEl, _this._isVertical);
                  if (!s.changeOnEnd) {
                      _this._infinite(_this._currPos);
                  }
              }
              _this._raf = _this._anim(dir);
          }));
      };
      ScrollviewBase.prototype._infinite = function (pos) {
          var s = this.s;
          if (s.itemSize) {
              var index = round((-pos * this._rtlNr) / s.itemSize) + this._offset;
              var diff = index - this._currIndex;
              if (diff) {
                  // this._margin += diff;
                  if (s.changeOnEnd) {
                      this._hook('onIndexChange', { index: index, diff: diff });
                  }
                  else {
                      this.setState({ index: index });
                  }
              }
          }
      };
      ScrollviewBase.prototype._scroll = function (pos, time) {
          var s = this.s;
          var itemSize = s.itemSize;
          var isVertical = this._isVertical;
          var style = this._scrollEl.style;
          var prefix = jsPrefix ? jsPrefix + 'T' : 't';
          var timing = time ? cssPrefix + 'transform ' + round(time) + 'ms ' + s.easing : '';
          style[prefix + 'ransform'] = 'translate3d(' + (isVertical ? '0,' + pos + 'px,' : pos + 'px,0,') + '0)';
          style[prefix + 'ransition'] = timing;
          this._endPos = pos;
          if (s.scroll3d) {
              var style3d = this._scrollEl3d.style;
              var angle = 360 / (s.batchSize3d * 2);
              style3d[prefix + 'ransform'] = 'translateY(-50%) rotateX(' + (-pos / itemSize) * angle + 'deg)';
              style3d[prefix + 'ransition'] = timing;
          }
          if (this._scrollbarEl) {
              var sbStyle = this._scrollbarEl.style;
              var percent = this._isInfinite
                  ? (this._maxSnapScroll * this._round - this._delta) / (this._maxSnapScroll * this._round * 2)
                  : (pos - this._max) / (this._min - this._max);
              var barPos = constrain((this._barContSize - this._barSize) * percent, 0, this._barContSize - this._barSize);
              sbStyle[prefix + 'ransform'] = 'translate3d(' + (isVertical ? '0,' + barPos + 'px,' : barPos + 'px,0,') + '0)';
              sbStyle[prefix + 'ransition'] = timing;
          }
          if (time) {
              rafc(this._raf);
              // Maintain position during animation
              this._isAnimating = time;
              this._scrollbarContEl.classList.add('mbsc-scroller-bar-started'); // show the scrollbar during animation
              this._raf = this._anim(pos > this._currPos ? 1 : -1);
          }
          else {
              this._currPos = pos;
              // Infinite
              if (!s.changeOnEnd) {
                  this._infinite(pos);
              }
          }
      };
      ScrollviewBase.prototype._move = function (args, preventMaxSnap) {
          var prevX = this._currX;
          var prevY = this._currY;
          var prevT = this._timestamp;
          var maxSnapScroll = this._maxSnapScroll;
          if (args) {
              this._currX = args.endX;
              this._currY = args.endY;
              this._timestamp = +new Date();
              var timeDelta = this._timestamp - prevT;
              if (timeDelta > 0 && timeDelta < 100) {
                  var velocityX = (this._currX - prevX) / timeDelta;
                  var velocityY = (this._currY - prevY) / timeDelta;
                  this._velocityX = velocityX * 0.7 + this._velocityX * 0.3;
                  this._velocityY = velocityY * 0.7 + this._velocityY * 0.3;
              }
          }
          if (maxSnapScroll && !preventMaxSnap) {
              this._delta = constrain(this._delta, -this._round * maxSnapScroll, this._round * maxSnapScroll);
          }
          this._scroll(constrain(this._startPos + this._delta, this._min - this.s.itemSize, this._max + this.s.itemSize));
          this._raf = false;
      };
      ScrollviewBase.defaults = {
          axis: 'Y',
          batchSize: 40,
          easing: 'cubic-bezier(0.190, 1.000, 0.220, 1.000)',
          mouseSwipe: true,
          mousewheel: true,
          prevDef: true,
          selectedIndex: 0,
          spaceAround: true,
          stopProp: true,
          swipe: true,
          thresholdX: 10,
          thresholdY: 5,
      };
      return ScrollviewBase;
  }(BaseComponent));

  function template$e(s, inst, content) {
      var content3d;
      if (s.itemRenderer) {
          content = inst.visibleItems.map(function (item) { return s.itemRenderer(item, inst._offset); });
          if (s.scroll3d) {
              content3d = inst.visible3dItems.map(function (item) { return s.itemRenderer(item, inst._offset, true); });
          }
      }
      // TODO: forward other props as well
      return (createElement("div", { ref: inst._setEl, className: inst._cssClass, style: s.styles },
          createElement("div", { ref: inst._setInnerEl, className: s.innerClass, style: s.innerStyles },
              createElement("div", { ref: inst._setScrollEl, className: 'mbsc-scrollview-scroll' + inst._rtl }, content)),
          s.scroll3d && (createElement("div", { ref: inst._setScrollEl3d, style: { height: s.itemSize + 'px' }, className: 'mbsc-scroller-items-3d' }, content3d)),
          createElement("div", { ref: inst._setScrollbarContEl, className: 'mbsc-scroller-bar-cont ' +
                  inst._rtl +
                  (!s.scrollBar || inst._barSize === inst._barContSize ? ' mbsc-scroller-bar-hidden' : '') +
                  (inst._started ? ' mbsc-scroller-bar-started' : '') },
              createElement("div", { className: 'mbsc-scroller-bar' + inst._theme, ref: inst._setScrollbarEl, style: { height: inst._barSize + 'px' } }))));
  }
  var Scrollview = /*#__PURE__*/ (function (_super) {
      __extends(Scrollview, _super);
      function Scrollview() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      Scrollview.prototype._template = function (s) {
          return template$e(s, this, s.children);
      };
      return Scrollview;
  }(ScrollviewBase));

  var update = 0;
  function template$f(s, state, inst, content) {
      var _a, _b;
      update++;
      var variableRow = inst._variableRow;
      var monthOrYearSelection = inst._view !== MONTH_VIEW;
      var animationEnd = (_a = {}, _a[ON_ANIMATION_END] = inst._onViewAnimationEnd, _a);
      var keydown = (_b = {}, _b[ON_KEY_DOWN] = inst._onKeyDown, _b);
      var renderMonthView = function (timestamp, props) {
          return (createElement(MonthView, __assign({}, props, { activeDate: inst._active, amText: s.amText, calendarType: s.calendarType, cellTextHeight: state.cellTextHeight, clickToCreate: s.clickToCreate, colors: inst._colors, dayNames: s.dayNames, dayNamesShort: inst._dayNames, dataTimezone: s.dataTimezone, displayTimezone: s.displayTimezone, eventText: s.eventText, events: s.eventMap, eventsText: s.eventsText, exclusiveEndDates: s.exclusiveEndDates, firstDay: s.firstDay, firstPageDay: timestamp, getDate: s.getDate, getDay: s.getDay, getMonth: s.getMonth, getWeekNumber: s.getWeekNumber, getYear: s.getYear, hasMarks: !!inst._marked, hoverEnd: s.hoverEnd, hoverStart: s.hoverStart, isPicker: s.isPicker, invalid: inst._invalid, labels: inst._labelsLayout, labelHeight: state.labelHeight, marked: inst._marked, max: inst._maxDate, min: inst._minDate, monthNames: s.monthNames, monthNamesShort: s.monthNamesShort, onDayClick: inst._onDayClick, onDayDoubleClick: s.onDayDoubleClick, onDayRightClick: s.onDayRightClick, onDayHoverIn: inst._onDayHoverIn, onDayHoverOut: inst._onDayHoverOut, onLabelClick: inst._onLabelClick, onLabelDoubleClick: s.onLabelDoubleClick, onLabelRightClick: s.onLabelRightClick, onLabelHoverIn: s.onLabelHoverIn, onLabelHoverOut: s.onLabelHoverOut, onLabelDelete: s.onLabelDelete, pmText: s.pmText, rangeEnd: s.rangeEnd, rangeStart: s.rangeStart, resourcesMap: s.resourcesMap, rtl: s.rtl, selectedDates: s.selectedDates, selectedEventsMap: s.selectedEventsMap, showEventTooltip: s.showEventTooltip, showOuter: inst._showOuter, showWeekDays: !inst._showDaysTop, showWeekNumbers: s.showWeekNumbers, showSingleMark: !!s.marksMap, todayText: s.todayText, theme: s.theme, timeFormat: s.timeFormat, timezonePlugin: s.timezonePlugin, valid: inst._valid, weeks: inst._weeks, weekText: s.weekText, renderDay: s.renderDay, renderDayContent: s.renderDayContent, renderLabel: s.renderLabel, renderLabelContent: s.renderLabelContent, variableRow: inst._variableRow })));
      };
      var renderMonth = function (item, offset) {
          var key = item.key;
          var isActive = key >= inst._pageIndex && key < inst._pageIndex + inst._pageNr && inst._view === MONTH_VIEW;
          var ownProps = {
              dragData: s.dragData,
              dragToCreate: s.dragToCreate,
              dragToMove: s.dragToMove,
              dragToResize: s.dragToResize,
              isActive: isActive,
              onLabelUpdateEnd: s.onLabelUpdateEnd,
              onLabelUpdateModeOff: s.onLabelUpdateModeOff,
              onLabelUpdateModeOn: s.onLabelUpdateModeOn,
              onLabelUpdateMove: s.onLabelUpdateMove,
              onLabelUpdateStart: s.onLabelUpdateStart,
          };
          return (createElement("div", { className: 'mbsc-calendar-slide' + (isActive ? ' mbsc-calendar-slide-active' : '') + inst._theme + inst._rtl, key: key, style: inst._getPageStyle(key, offset, inst._pageNr) }, renderMonthView(inst._getPageDay(key), ownProps)));
      };
      var renderYears = function (item, offset) {
          var index = item.key;
          var first = inst._getPageYears(index);
          var selectedYear = s.getYear(new Date(inst._active));
          var activeYear = s.getYear(new Date(inst._activeMonth));
          return (createElement("div", { "aria-hidden": inst._yearsIndex === index ? UNDEFINED : 'true', className: 'mbsc-calendar-picker-slide mbsc-calendar-slide' + inst._theme + inst._rtl, key: index, style: inst._getPageStyle(index, offset) },
              createElement("div", { className: "mbsc-calendar-table mbsc-flex-col" }, ARRAY4.map(function (x, i) { return (createElement("div", { className: "mbsc-calendar-row mbsc-flex mbsc-flex-1-0", key: i }, ARRAY3.map(function (y, j) {
                  var year = first + i * 3 + j;
                  var d = +s.getDate(year, 0, 1);
                  return (createElement(CalendarDay, { active: year === activeYear, date: d, display: true, selected: year === selectedYear, disabled: year < inst._minYears || year > inst._maxYears, rtl: s.rtl, text: year + s.yearSuffix, theme: s.theme, type: "year", onDayClick: inst._onYearClick, key: year }));
              }))); }))));
      };
      var renderYear = function (item, offset) {
          var index = item.key;
          var year = inst._getPageYear(index);
          var active = new Date(inst._activeMonth);
          var activeYear = s.getYear(active);
          var activeMonth = s.getMonth(active);
          var selected = new Date(inst._active);
          var selectedYear = s.getYear(selected);
          var selectedMonth = s.getMonth(selected);
          return (createElement("div", { "aria-hidden": inst._yearIndex === index ? UNDEFINED : 'true', className: 'mbsc-calendar-picker-slide mbsc-calendar-slide' + inst._theme + inst._rtl, key: index, style: inst._getPageStyle(index, offset) },
              createElement("div", { className: "mbsc-calendar-table mbsc-flex-col" }, ARRAY4.map(function (a, i) { return (createElement("div", { className: "mbsc-calendar-row mbsc-flex mbsc-flex-1-0", key: i }, ARRAY3.map(function (b, j) {
                  var d = s.getDate(year, i * 3 + j, 1);
                  var y = s.getYear(d);
                  var m = s.getMonth(d);
                  return (createElement(CalendarDay, { active: y === activeYear && m === activeMonth, date: +d, display: true, selected: y === selectedYear && m === selectedMonth, disabled: d < inst._minYear || d >= inst._maxYear, month: s.monthNames[m], rtl: s.rtl, text: s.monthNamesShort[m], theme: s.theme, type: "month", onDayClick: inst._onMonthClick, key: +d }));
              }))); }))));
      };
      var renderHeader = function () {
          var headerContent;
          var html;
          if (s.renderHeader) {
              headerContent = s.renderHeader();
              if (isString(headerContent)) {
                  if (headerContent !== inst._headerHTML) {
                      inst._headerHTML = headerContent;
                      inst._shouldEnhanceHeader = true;
                  }
                  html = inst._safeHtml(headerContent);
              }
          }
          else {
              var isMultiPage = inst._pageNr > 1;
              headerContent = (createElement(Fragment, null,
                  createElement(CalendarNav, { className: "mbsc-flex mbsc-flex-1-1 mbsc-calendar-title-wrapper" }),
                  createElement(CalendarPrev, { className: 'mbsc-calendar-button-prev' + (isMultiPage ? ' mbsc-calendar-button-prev-multi' : '') }),
                  s.showToday && createElement(CalendarToday, { className: "mbsc-calendar-header-today" }),
                  createElement(CalendarNext, { className: 'mbsc-calendar-button-next' + (isMultiPage ? ' mbsc-calendar-button-next-multi' : '') })));
          }
          var header = (createElement("div", { className: 'mbsc-calendar-controls mbsc-flex' + inst._theme, dangerouslySetInnerHTML: html }, headerContent));
          // We need to use the createElement for preact to work with context
          return createElement(CalendarContext.Provider, { children: header, value: { instance: inst } });
      };
      var calWeekDays = inst._showDaysTop && s.showCalendar ? (createElement(CalendarWeekDays, { dayNamesShort: inst._dayNames, rtl: inst._rtl, theme: inst._theme, firstDay: s.firstDay, hasScroll: state.hasScrollY, hidden: inst._view !== MONTH_VIEW && !inst._hasPicker, showWeekNumbers: s.showWeekNumbers })) : null;
      var pickerProps = {
          axis: inst._axis,
          batchSize: 1,
          changeOnEnd: true,
          className: 'mbsc-calendar-scroll-wrapper' + inst._theme,
          // Need to pass some random data to render month views inside the scrollview if something changed (other than scrollview props)
          data: update,
          easing: 'ease-out',
          itemSize: state.pickerSize,
          items: inst._months,
          mousewheel: inst._mousewheel,
          prevAnim: inst._prevAnim,
          rtl: s.rtl,
          snap: true,
          time: 200,
      };
      var monthYearPicker = (createElement("div", { ref: inst._setPickerCont, className: inst._hasPicker ? 'mbsc-calendar-picker-wrapper' : '' },
          (state.view === MULTI_YEAR_VIEW || state.viewClosing === MULTI_YEAR_VIEW || s.selectView === MULTI_YEAR_VIEW) && (createElement("div", __assign({ className: inst._getPickerClass(MULTI_YEAR_VIEW) }, animationEnd),
              createElement(Scrollview, __assign({ key: "years", itemRenderer: renderYears, maxIndex: inst._maxYearsIndex, minIndex: inst._minYearsIndex, onGestureEnd: inst._onGestureEnd, onIndexChange: inst._onYearsPageChange, selectedIndex: inst._yearsIndex }, pickerProps)))),
          (state.view === YEAR_VIEW || state.viewClosing === YEAR_VIEW || s.selectView === YEAR_VIEW) && (createElement("div", __assign({ className: inst._getPickerClass(YEAR_VIEW) }, animationEnd),
              createElement(Scrollview, __assign({ key: "year", itemRenderer: renderYear, maxIndex: inst._maxYearIndex, minIndex: inst._minYearIndex, onGestureEnd: inst._onGestureEnd, onIndexChange: inst._onYearPageChange, selectedIndex: inst._yearIndex }, pickerProps))))));
      return (createElement("div", { className: inst._cssClass, ref: inst._setEl, style: inst._dim, onClick: noop },
          createElement("div", { className: 'mbsc-calendar-wrapper mbsc-flex-col' +
                  inst._theme +
                  inst._hb +
                  (s.hasContent || !s.showCalendar ? ' mbsc-calendar-wrapper-fixed mbsc-flex-none' : ' mbsc-flex-1-1') },
              createElement("div", { className: 'mbsc-calendar-header' + inst._theme + inst._hb + (inst._showDaysTop ? ' mbsc-calendar-header-vertical' : ''), ref: inst._setHeader },
                  s.showControls && renderHeader(),
                  calWeekDays),
              createElement("div", __assign({ className: 'mbsc-calendar-body mbsc-flex-col mbsc-flex-1-1' + inst._theme, ref: inst._setBody }, keydown), s.showCalendar && (createElement("div", { className: 'mbsc-calendar-body-inner mbsc-flex-col mbsc-flex-1-1' + (variableRow ? ' mbsc-calendar-body-inner-variable' : '') },
                  inst._isGrid ? (createElement("div", { "aria-hidden": monthOrYearSelection ? 'true' : UNDEFINED, className: 'mbsc-calendar-grid mbsc-flex-1-1 mbsc-flex-col' + inst._theme + inst._hb }, inst._monthsMulti.map(function (row, i) {
                      return (createElement("div", { key: i, className: "mbsc-calendar-grid-row mbsc-flex mbsc-flex-1-1" }, row.map(function (item, j) {
                          return (createElement("div", { key: j, className: 'mbsc-calendar-grid-item mbsc-flex-col mbsc-flex-1-1' + inst._theme },
                              createElement("div", { className: 'mbsc-calendar-month-title' + inst._theme }, s.monthNames[new Date(item).getMonth()]),
                              renderMonthView(item, { isActive: true })));
                      })));
                  }))) : variableRow ? (createElement("div", { "aria-hidden": monthOrYearSelection ? 'true' : UNDEFINED, className: 'mbsc-calendar-slide mbsc-calendar-slide-active ' + inst._getPickerClass(MONTH_VIEW) }, renderMonthView(+s.navigationService.firstDay, {
                      dragData: s.dragData,
                      dragToCreate: s.dragToCreate,
                      dragToMove: s.dragToMove,
                      dragToResize: s.dragToResize,
                      isActive: true,
                      onLabelUpdateEnd: s.onLabelUpdateEnd,
                      onLabelUpdateModeOff: s.onLabelUpdateModeOff,
                      onLabelUpdateModeOn: s.onLabelUpdateModeOn,
                      onLabelUpdateMove: s.onLabelUpdateMove,
                      onLabelUpdateStart: s.onLabelUpdateStart,
                  }))) : (s.selectView === MONTH_VIEW && (createElement("div", __assign({ "aria-hidden": monthOrYearSelection ? 'true' : UNDEFINED, className: inst._getPickerClass(MONTH_VIEW) }, animationEnd),
                      createElement(Scrollview, __assign({}, pickerProps, { itemNr: inst._pageNr, itemSize: state.pageSize / inst._pageNr, itemRenderer: renderMonth, maxIndex: inst._maxIndex, minIndex: inst._minIndex, mouseSwipe: s.mouseSwipe, onAnimationEnd: inst._onAnimationEnd, onGestureStart: inst._onGestureStart, onIndexChange: inst._onPageChange, onStart: inst._onStart, selectedIndex: inst._pageIndex, swipe: s.swipe }))))),
                  !inst._hasPicker && monthYearPicker)))),
          content,
          inst._hasPicker && (createElement(Popup, { anchor: inst._pickerBtn, closeOnScroll: true, contentPadding: false, context: s.context, cssClass: "mbsc-calendar-popup", display: "anchored", isOpen: inst._view !== MONTH_VIEW, locale: s.locale, onClose: inst._onPickerClose, onOpen: inst._onPickerOpen, rtl: s.rtl, scrollLock: false, showOverlay: false, theme: s.theme, themeVariant: s.themeVariant },
              createElement("div", __assign({}, keydown),
                  createElement("div", { className: 'mbsc-calendar-controls mbsc-flex' + inst._theme },
                      createElement("div", { "aria-live": "polite", className: 'mbsc-calendar-picker-button-wrapper mbsc-calendar-title-wrapper mbsc-flex mbsc-flex-1-1' + inst._theme },
                          createElement(Button, { className: "mbsc-calendar-button", onClick: inst._onPickerBtnClick, theme: s.theme, themeVariant: s.themeVariant, type: "button", variant: "flat" },
                              inst._viewTitle,
                              s.downIcon && createElement(Icon, { svg: state.view === MULTI_YEAR_VIEW ? s.downIcon : s.upIcon, theme: s.theme }))),
                      createElement(Button, { className: "mbsc-calendar-button", ariaLabel: s.prevPageText, disabled: inst._isPrevDisabled(true), iconSvg: inst._prevIcon, onClick: inst.prevPage, theme: s.theme, themeVariant: s.themeVariant, type: "button", variant: "flat" }),
                      createElement(Button, { className: "mbsc-calendar-button", ariaLabel: s.nextPageText, disabled: inst._isNextDisabled(true), iconSvg: inst._nextIcon, onClick: inst.nextPage, theme: s.theme, themeVariant: s.themeVariant, type: "button", variant: "flat" })),
                  monthYearPicker)))));
  }
  var CalendarView = /*#__PURE__*/ (function (_super) {
      __extends(CalendarView, _super);
      function CalendarView() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      CalendarView.prototype._template = function (s, state) {
          return template$f(s, state, this, s.children);
      };
      CalendarView.prototype._updated = function () {
          _super.prototype._updated.call(this);
          if (this._shouldEnhanceHeader) {
              enhance(this._headerElement, { view: this });
              this._shouldEnhanceHeader = false;
          }
      };
      return CalendarView;
  }(CalendarViewBase));

  function renderAgenda(s, inst, slots) {
      var theme = s.theme;
      var dayRefs = inst._listDays;
      var events = inst.state.eventList || [];
      var renderCustomAgenda = slots ? slots.agenda : s.renderAgenda;
      var renderCustomAgendaEmpty = slots ? slots.agendaEmpty : s.renderAgendaEmpty;
      if (renderCustomAgenda) {
          if (inst._eventListHTML === UNDEFINED) {
              return renderCustomAgenda(events, s, dayRefs);
          }
      }
      var emptyAgendaContent;
      if (!events.length) {
          var customEmptyAgendaContent = renderCustomAgendaEmpty && renderCustomAgendaEmpty();
          var emptyAgendaContentHTML = isString(customEmptyAgendaContent) && { __html: customEmptyAgendaContent };
          if (emptyAgendaContentHTML) {
              emptyAgendaContent = createElement("div", { dangerouslySetInnerHTML: emptyAgendaContentHTML });
              inst._shouldEnhance = inst._list;
          }
          else {
              emptyAgendaContent = (createElement("div", { className: !customEmptyAgendaContent ? 'mbsc-event-list-empty' + inst._theme : '' }, customEmptyAgendaContent || s.noEventsText));
          }
      }
      return (createElement(List, { theme: theme, themeVariant: s.themeVariant, rtl: s.rtl },
          !events.length && emptyAgendaContent,
          events.map(function (day, i) { return (createElement("div", { className: 'mbsc-event-group' + inst._theme, key: i, ref: function (el) { return (dayRefs[day.timestamp] = el); } },
              (inst._eventListType !== 'day' || inst._eventListSize > 1) && (createElement(ListHeader, { theme: theme, themeVariant: s.themeVariant, className: "mbsc-event-day" }, day.date)),
              day.events.map(function (event, j) { return renderEvent$1(inst, event, j, day.timestamp, s, UNDEFINED, slots); }))); })));
  }
  function renderEvent$1(inst, data, key, date, s, isPopup, slots) {
      var showColor = !inst._colorEventList;
      var source = isPopup ? 'popover' : 'agenda';
      var isVisible = !isPopup || inst.state.showPopover;
      var theme = inst._theme;
      var renderEventContent = slots ? slots.eventContent : s.renderEventContent;
      var renderCustomEvent = slots ? slots.event : s.renderEvent;
      var eventHTML;
      var eventContent = renderEventContent ? (renderEventContent(data)) : (createElement("div", { className: 'mbsc-event-text ' + theme, title: data.tooltip, dangerouslySetInnerHTML: data.html }, UNDEFINED));
      // The extra wrapper div is needed for being consistent with other frameworks.
      // We need it in the case of jQuery and JavaScript because we need an element (div) to set the inner html to.
      // At this point the Fragment component does not support the dangerouslySetInnerHTML prop.
      if (isString(eventContent)) {
          var eventContentHTML = { __html: eventContent };
          eventContent = (createElement("div", { className: 'mbsc-event-content mbsc-flex-1-1 ' + theme, dangerouslySetInnerHTML: eventContentHTML }));
          inst._shouldEnhance = isVisible && source;
      }
      else {
          eventContent = createElement("div", { className: 'mbsc-event-content mbsc-flex-1-1 ' + theme }, eventContent);
      }
      var eventInner = renderCustomEvent ? (renderCustomEvent(data)) : (createElement(Fragment, null,
          createElement("div", { className: 'mbsc-event-color' + theme + inst._rtl, style: data.style }),
          eventContent,
          createElement("div", { className: 'mbsc-event-time' + theme + inst._rtl },
              data.allDayText && createElement("div", { className: 'mbsc-event-all-day' + theme }, data.allDayText),
              data.lastDay && createElement("div", { className: 'mbsc-event-until' + theme }, data.lastDay),
              data.start && createElement("div", { className: 'mbsc-event-start' + theme }, data.start),
              data.start && data.end && createElement("div", { className: 'mbsc-event-sep' + theme }, "-"),
              data.end && createElement("div", { className: 'mbsc-event-end' + theme }, data.end))));
      // In case of custom event listing, the renderer function might return string (for jQuery and plain JS)
      // In this case we will set the string as innerHTML of the list container
      if (isString(eventInner)) {
          eventHTML = { __html: eventInner };
          eventInner = UNDEFINED;
          inst._shouldEnhance = isVisible && source;
      }
      return (createElement(ListItem, { className: 'mbsc-event' + (showColor ? '' : ' mbsc-colored-event') + (data.original.cssClass ? ' ' + data.original.cssClass : ''), "data-id": data.original.id, key: key, actionable: s.actionableEvents, dangerouslySetInnerHTML: eventHTML, data: data.original, drag: isPopup && inst._showEventLabels && (s.dragToMove || s.externalDrag), rtl: s.rtl, selected: !!(inst._selectedEventsMap[data.uid] || inst._selectedEventsMap[data.id]), style: showColor ? UNDEFINED : data.style, theme: s.theme, themeVariant: s.themeVariant, 
          // tslint:disable jsx-no-lambda
          onClick: function (ev) { return inst._onEventClick({ date: date, domEvent: ev.domEvent, event: data.original, source: source }); }, onDoubleClick: function (domEvent) { return inst._onEventDoubleClick({ date: date, domEvent: domEvent, event: data.original, source: source }); }, onContextMenu: function (domEvent) { return inst._onEventRightClick({ date: date, domEvent: domEvent, event: data.original, source: source }); }, onHoverIn: function (_a) {
              var domEvent = _a.domEvent;
              return inst._onEventHoverIn({ date: date, domEvent: domEvent, event: data.original, source: source });
          }, onHoverOut: function (_a) {
              var domEvent = _a.domEvent;
              return inst._onEventHoverOut({ date: date, domEvent: domEvent, event: data.original, source: source });
          }, 
          // tslint:enable jsx-no-lambda
          onDelete: inst._onEventDelete, onDragEnd: inst._onLabelUpdateEnd, onDragModeOff: inst._onLabelUpdateModeOff, onDragModeOn: inst._onLabelUpdateModeOn, onDragMove: inst._onLabelUpdateMove, onDragStart: inst._onLabelUpdateStart }, eventInner));
  }
  function template$g(s, state, inst, slots) {
      var eventList;
      if (!inst._listDays) {
          inst._listDays = {};
      }
      if (inst._showEventList) {
          eventList = renderAgenda(s, inst, slots);
          // In case of custom event listing, the renderer function might return string (for jQuery and plain JS)
          // In inst case we will set the string as innerHTML of the list container
          if (isString(eventList)) {
              inst._eventListHTML = { __html: eventList };
              // After the DOM is updated we should load the day wrapper based on the mbsc-timestamp attribute (if any)
              // It's needed for scrolling the list to the selected date
              inst._shouldLoadDays = true;
              inst._shouldEnhance = true;
              eventList = UNDEFINED;
          }
      }
      var commonProps = {
          amText: s.amText,
          clickToCreate: s.clickToCreate,
          dataTimezone: s.dataTimezone,
          dateFormat: s.dateFormat,
          dayNames: s.dayNames,
          dayNamesMin: s.dayNamesMin,
          dayNamesShort: s.dayNamesShort,
          displayTimezone: s.displayTimezone,
          dragBetweenResources: s.dragBetweenResources,
          dragInTime: s.dragInTime,
          dragToCreate: s.dragToCreate,
          dragToResize: s.dragToResize,
          eventMap: inst._eventMap,
          eventOrder: s.eventOrder,
          exclusiveEndDates: s.exclusiveEndDates,
          firstDay: s.firstDay,
          fromText: s.fromText,
          getDate: s.getDate,
          getDay: s.getDay,
          getMaxDayOfMonth: s.getMaxDayOfMonth,
          getMonth: s.getMonth,
          getWeekNumber: s.getWeekNumber,
          getYear: s.getYear,
          monthNames: s.monthNames,
          monthNamesShort: s.monthNamesShort,
          onActiveChange: inst._onActiveChange,
          onEventDragEnter: inst._onEventDragEnter,
          onEventDragLeave: inst._onEventDragLeave,
          pmText: s.pmText,
          refDate: inst._refDate,
          renderDay: slots ? slots.day : s.renderDay,
          rtl: s.rtl,
          selectedEventsMap: inst._selectedEventsMap,
          showEventTooltip: s.showEventTooltip,
          theme: s.theme,
          themeVariant: s.themeVariant,
          timeFormat: s.timeFormat,
          timezonePlugin: s.timezonePlugin,
          toText: s.toText,
      };
      var scheduleTimelineProps = __assign({}, commonProps, { allDayText: s.allDayText, checkSize: inst._checkSize, colorsMap: inst._colorsMap, currentTimeIndicator: inst._currentTimeIndicator, dateFormatLong: s.dateFormatLong, dragTimeStep: inst._dragTimeStep, dragToMove: s.dragToMove, eventDragEnd: inst._onEventDragStop, extendDefaultEvent: s.extendDefaultEvent, externalDrag: s.externalDrag, externalDrop: s.externalDrop, groupBy: s.groupBy, height: state.height, invalidateEvent: s.invalidateEvent, invalidsMap: inst._invalidsMap, maxDate: inst._maxDate, minDate: inst._minDate, navigateToEvent: inst._navigateToEvent, navigationService: inst._navService, newEventText: s.newEventText, onCellClick: inst._onCellClick, onCellDoubleClick: inst._onCellDoubleClick, onCellRightClick: inst._onCellRightClick, onEventClick: inst._onEventClick, onEventDelete: inst._onEventDelete, onEventDoubleClick: inst._onEventDoubleClick, onEventDragEnd: inst._onEventDragEnd, onEventDragStart: inst._onEventDragStart, onEventHoverIn: inst._onEventHoverIn, onEventHoverOut: inst._onEventHoverOut, onEventRightClick: inst._onEventRightClick, renderEvent: slots ? slots.scheduleEvent : s.renderScheduleEvent, renderEventContent: slots ? slots.scheduleEventContent : s.renderScheduleEventContent, renderResource: slots ? slots.resource : s.renderResource, resources: s.resources, scroll: inst._shouldScrollSchedule, selected: inst._selectedDateTime, width: state.width });
      return (createElement(CalendarView, __assign({ ref: inst._setEl }, commonProps, { activeDate: inst._active, calendarScroll: inst._calendarScroll, calendarType: inst._calendarType, colors: s.colors, context: s.context, cssClass: inst._cssClass, downIcon: s.downIcon, dragData: state.labelDragData, dragToMove: s.dragToMove || s.externalDrag, endDay: inst._rangeEndDay, eventRange: inst._rangeType, eventRangeSize: inst._showSchedule ? inst._scheduleSize : inst._showTimeline ? inst._timelineSize : inst._eventListSize, hasContent: inst._showEventList || inst._showSchedule || inst._showTimeline, hasPicker: true, height: s.height, invalid: s.invalid, instanceService: inst._instanceService, labels: s.labels, labelList: inst._calendarLabelList, labelsMap: inst._labelsMap, marked: s.marked, marksMap: inst._marksMap, max: s.max, min: s.min, mouseSwipe: (!s.dragToCreate && s.clickToCreate !== 'single') || (!inst._showEventLabels && !inst._showEventCount), mousewheel: s.mousewheel, navigationService: inst._navService, nextIconH: s.nextIconH, nextIconV: s.nextIconV, nextPageText: s.nextPageText, noOuterChange: !inst._showEventList, onCellHoverIn: inst._onCellHoverIn, onCellHoverOut: inst._onCellHoverOut, onDayClick: inst._onDayClick, onDayDoubleClick: inst._onDayDoubleClick, onDayRightClick: inst._onDayRightClick, onGestureStart: inst._onGestureStart, onLabelClick: inst._onLabelClick, onLabelDoubleClick: inst._onLabelDoubleClick, onLabelRightClick: inst._onLabelRightClick, onLabelHoverIn: inst._onLabelHoverIn, onLabelHoverOut: inst._onLabelHoverOut, onLabelDelete: inst._onEventDelete, onLabelUpdateStart: inst._onLabelUpdateStart, onLabelUpdateMove: inst._onLabelUpdateMove, onLabelUpdateEnd: inst._onLabelUpdateEnd, onLabelUpdateModeOn: inst._onLabelUpdateModeOn, onLabelUpdateModeOff: inst._onLabelUpdateModeOff, onPageChange: inst._onPageChange, onPageLoaded: inst._onPageLoaded, onPageLoading: inst._onPageLoading, onResize: inst._onResize, pageLoad: inst._pageLoad, prevIconH: s.prevIconH, prevIconV: s.prevIconV, prevPageText: s.prevPageText, resourcesMap: inst._resourcesMap, responsiveStyle: true, renderHeader: slots ? slots.header : s.renderHeader, renderDayContent: slots ? slots.dayContent : s.renderDayContent, renderLabel: slots ? slots.label : s.renderLabel, renderLabelContent: slots ? slots.labelContent : s.renderLabelContent, selectedDates: inst._selectedDates, selectView: MONTH_VIEW, showCalendar: inst._showCalendar, showControls: s.showControls, showLabelCount: inst._showEventCount, showOuterDays: inst._showOuterDays, showSchedule: inst._showSchedule || inst._showTimeline, showToday: s.showToday, showWeekNumbers: inst._showWeekNumbers, size: inst._calendarSize, startDay: inst._rangeStartDay, swipe: !state.isTouchDrag, upIcon: s.upIcon, valid: s.valid, weeks: inst._calendarSize, width: s.width, 
          // Localization
          eventText: s.eventText, eventsText: s.eventsText, fromText: s.fromText, moreEventsPluralText: s.moreEventsPluralText, moreEventsText: s.moreEventsText, todayText: s.todayText, toText: s.toText, weekText: s.weekText, yearSuffix: s.yearSuffix }),
          inst._showDate && (createElement("div", { className: 'mbsc-schedule-date-header mbsc-flex' + inst._theme + inst._hb },
              inst._showSchedule && !inst._showCalendar && s.resources && createElement("div", { className: "mbsc-schedule-time-col" }),
              createElement("div", { className: 'mbsc-schedule-date-header-text mbsc-flex-1-1' + inst._theme }, inst._selectedDateHeader),
              inst._showSchedule && !inst._showCalendar && s.resources && createElement("div", { className: "mbsc-schedule-fake-scroll-y" }))),
          inst._showEventList && (createElement("div", { className: 'mbsc-flex-1-1 mbsc-event-list' + (state.isListScrollable ? ' mbsc-event-list-scroll' : ''), dangerouslySetInnerHTML: inst._eventListHTML, onScroll: inst._onScroll, ref: inst._setList }, eventList)),
          inst._showSchedule && (createElement(Scheduler, __assign({}, scheduleTimelineProps, { endDay: inst._scheduleEndDay, endTime: inst._scheduleEndTime, renderDayContent: slots ? slots.dayContent : s.renderDayContent, showAllDay: inst._showScheduleAllDay, showDays: inst._showScheduleDays, size: inst._scheduleSize, startDay: inst._scheduleStartDay, startTime: inst._scheduleStartTime, timeCellStep: inst._scheduleTimeCellStep, timeLabelStep: inst._scheduleTimeLabelStep, timezones: inst._scheduleTimezones, type: inst._scheduleType, onWeekDayClick: inst._onWeekDayClick }))),
          inst._showTimeline && (createElement(Timeline, __assign({}, scheduleTimelineProps, { dragToCreate: s.slots ? false : s.dragToCreate, dragToResize: s.slots ? false : s.dragToResize, downIcon: s.chevronIconDown, connections: s.connections, endDay: inst._timelineEndDay, endTime: inst._timelineEndTime, eventList: inst._timelineListing, nextIcon: s.nextIconH, nextIconRtl: s.prevIconH, onResourceCollapse: inst._proxy, onResourceExpand: inst._proxy, renderDayFooter: slots ? slots.dayFooter : s.renderDayFooter, renderHour: slots ? slots.hour : s.renderHour, renderHourFooter: slots ? slots.hourFooter : s.renderHourFooter, renderMonth: slots ? slots.month : s.renderMonth, renderMonthFooter: slots ? slots.monthFooter : s.renderMonthFooter, renderWeek: slots ? slots.week : s.renderWeek, renderWeekFooter: slots ? slots.weekFooter : s.renderWeekFooter, renderYear: slots ? slots.year : s.renderYear, renderYearFooter: slots ? slots.yearFooter : s.renderYearFooter, renderResourceFooter: slots ? slots.resourceFooter : s.renderResourceFooter, renderResourceHeader: slots ? slots.resourceHeader : s.renderResourceHeader, renderSidebar: slots ? slots.sidebar : s.renderSidebar, renderSidebarFooter: slots ? slots.sidebarFooter : s.renderSidebarFooter, renderSidebarHeader: slots ? slots.sidebarHeader : s.renderSidebarHeader, renderSlot: slots ? slots.slot : s.renderSlot, resolution: inst._timelineResolution, resolutionVertical: inst._timelineResolutionVertical, rowHeight: inst._timelineRowHeight, weekNumbers: inst._showTimelineWeekNumbers, weekText: s.weekText, size: inst._timelineSize, slots: s.slots, startDay: inst._timelineStartDay, startTime: inst._timelineStartTime, timeCellStep: inst._timelineTimeCellStep, timeLabelStep: inst._timelineTimeLabelStep, type: inst._timelineType, virtualScroll: !inst._print }))),
          createElement(Popup, { anchor: inst._anchor, closeOnScroll: true, contentPadding: false, context: s.context, cssClass: 'mbsc-calendar-popup ' + inst._popoverClass, display: "anchored", isOpen: state.showPopover, locale: s.locale, maxHeight: "24em", onClose: inst._onPopoverClose, rtl: s.rtl, scrollLock: false, showOverlay: false, theme: s.theme, themeVariant: s.themeVariant }, state.popoverList && (createElement(List, { ref: inst._setPopoverList, theme: s.theme, themeVariant: s.themeVariant, rtl: s.rtl, className: "mbsc-popover-list" }, state.popoverList.map(function (event, i) { return renderEvent$1(inst, event, i, state.popoverDate, s, true, slots); })))),
          state.labelDragData && state.labelDragData.draggedEvent && !state.isTouchDrag && createElement("div", { className: "mbsc-calendar-dragging" })));
  }
  /**
   * The Eventcalendar component.
   *
   * Usage:
   *
   * ```
   * <Eventcalendar />
   * ```
   */
  var Eventcalendar = /*#__PURE__*/ (function (_super) {
      __extends(Eventcalendar, _super);
      function Eventcalendar() {
          var _this = _super !== null && _super.apply(this, arguments) || this;
          /** @hidden */
          // tslint:disable-next-line: variable-name
          _this._instanceService = new InstanceServiceBase();
          return _this;
      }
      Eventcalendar.prototype._template = function (s, state) {
          return template$g(s, state, this);
      };
      return Eventcalendar;
  }(EventcalendarBase));

  var renderOptions = {
      before: function (elm, options) {
          if (options.selectedDate) {
              options.defaultSelectedDate = options.selectedDate;
              delete options.selectedDate;
          }
      },
  };

  // tslint:disable no-non-null-assertion
  var id = 0;
  function jsonp(url, callback) {
      // Check if we're in browser env
      if (win) {
          var script_1 = doc.createElement('script');
          var unique_1 = 'mbscjsonp' + ++id;
          win[unique_1] = function (data) {
              script_1.parentNode.removeChild(script_1);
              delete win[unique_1];
              if (!data) {
                  return;
              }
              callback(data);
          };
          script_1.src = url + (url.indexOf('?') >= 0 ? '&' : '?') + 'callback=' + unique_1;
          doc.body.appendChild(script_1);
      }
  }
  function ajaxGet(url, callback) {
      var request = new XMLHttpRequest();
      request.open('GET', url, true);
      request.onload = function () {
          if (request.status >= 200 && request.status < 400) {
              // Success!
              callback(JSON.parse(request.response));
          } // else {
          // We reached our target server, but it returned an error
          // }
      };
      request.onerror = function () {
          // There was a connection error of some sort
      };
      request.send();
  }
  /**
   * Load JSON-encoded data from a server using a GET HTTP request.
   * @param url URL to which the request is sent.
   * @param callback A function that is executed if the request succeeds.
   * @param type Type of the JSON request (json or jsonp)
   */
  function getJson(url, callback, type) {
      if (type === 'jsonp') {
          jsonp(url, callback);
      }
      else {
          ajaxGet(url, callback);
      }
  }
  var http = {
      getJson: getJson,
  };
  util.http = http;

  // tslint:disable: no-use-before-declare
  var localTimezone;
  function normTimezone(timezone) {
      if (!localTimezone) {
          localTimezone = luxonTimezone.luxon.DateTime.local().zoneName;
      }
      return !timezone || timezone === 'local' ? localTimezone : timezone;
  }
  /**
   * Checks which version of luxon library is used, version 1 or 2+
   * @param DT
   * @returns 1 for version 1.x and 2 for versions above 2.0, depending on the DT.fromObject function
   */
  function getVersion(DT) {
      var fn = DT.fromObject.toString().trim();
      return /^(function )?\w*\(\w+\)/.test(fn) ? 1 : 2;
  }
  var LDate = /*#__PURE__*/ (function () {
      function LDate(value, timezone) {
          if (timezone === void 0) { timezone = 'utc'; }
          // tslint:disable-next-line
          this._mbsc = true;
          timezone = normTimezone(timezone);
          var DT = luxonTimezone.luxon.DateTime;
          var zoneOpt = { zone: timezone };
          this.zone = timezone;
          if (isUndefined(value)) {
              this.dt = DT.utc().setZone(timezone);
          }
          else if (isDate(value) || isNumber(value)) {
              this.dt = DT.fromMillis(+value, zoneOpt);
          }
          else if (isString(value)) {
              this.dt = DT.fromISO(value, zoneOpt);
          }
          else if (isArray(value)) {
              var keys = ['year', 'month', 'day', 'hour', 'minute', 'second', 'millisecond'];
              var valueObj = {};
              for (var i = 0; i < value.length && i < 7; i++) {
                  valueObj[keys[i]] = value[i] + (i === 1 ? 1 : 0);
              }
              // In version 2+ of luxon, the options (the zone) should go into a second parameter.
              // To work with both version 1 and 2 we need to determin the version of luxon if not provided explicitly.
              luxonTimezone.version = luxonTimezone.version || getVersion(DT);
              if (luxonTimezone.version === 1) {
                  // v1.x
                  this.dt = DT.fromObject(__assign({}, valueObj, zoneOpt));
              }
              else {
                  // v2+
                  this.dt = DT.fromObject(valueObj, zoneOpt);
              }
          }
      }
      LDate.prototype.clone = function () {
          return new LDate(this, this.zone);
      };
      LDate.prototype.createDate = function (year, month, date, h, min, sec, ms) {
          return luxonTimezone.createDate({ displayTimezone: this.zone }, year, month, date, h, min, sec, ms);
      };
      LDate.prototype[Symbol.toPrimitive] = function (hint) {
          return this.dt.toJSDate()[Symbol.toPrimitive](hint);
      };
      LDate.prototype.toDateString = function () {
          return this.dt.toFormat('ccc MMM dd yyyy');
      };
      LDate.prototype.toISOString = function () {
          return this.dt.toISO();
      };
      LDate.prototype.toJSON = function () {
          return this.dt.toISO();
      };
      LDate.prototype.valueOf = function () {
          return this.dt.valueOf();
      };
      // Getters
      LDate.prototype.getDate = function () {
          return this.dt.day;
      };
      LDate.prototype.getDay = function () {
          return this.dt.weekday % 7;
      };
      LDate.prototype.getFullYear = function () {
          return this.dt.year;
      };
      LDate.prototype.getHours = function () {
          return this.dt.hour;
      };
      LDate.prototype.getMilliseconds = function () {
          return this.dt.millisecond;
      };
      LDate.prototype.getMinutes = function () {
          return this.dt.minute;
      };
      LDate.prototype.getMonth = function () {
          return this.dt.month - 1;
      };
      LDate.prototype.getSeconds = function () {
          return this.dt.second;
      };
      LDate.prototype.getTime = function () {
          return this.valueOf();
      };
      LDate.prototype.getTimezoneOffset = function () {
          return -this.dt.offset;
      };
      LDate.prototype.getUTCDate = function () {
          return this.dt.toUTC().day;
      };
      LDate.prototype.getUTCDay = function () {
          return this.dt.toUTC().weekday % 7;
      };
      LDate.prototype.getUTCFullYear = function () {
          return this.dt.toUTC().year;
      };
      LDate.prototype.getUTCHours = function () {
          return this.dt.toUTC().hour;
      };
      LDate.prototype.getUTCMilliseconds = function () {
          return this.dt.toUTC().millisecond;
      };
      LDate.prototype.getUTCMinutes = function () {
          return this.dt.toUTC().minute;
      };
      LDate.prototype.getUTCMonth = function () {
          return this.dt.toUTC().month - 1;
      };
      LDate.prototype.getUTCSeconds = function () {
          return this.dt.toUTC().second;
      };
      // Setters
      LDate.prototype.setMilliseconds = function (millisecond) {
          return this.setter({ millisecond: millisecond });
      };
      LDate.prototype.setSeconds = function (second, millisecond) {
          return this.setter({ second: second, millisecond: millisecond });
      };
      LDate.prototype.setMinutes = function (minute, second, millisecond) {
          return this.setter({ minute: minute, second: second, millisecond: millisecond });
      };
      LDate.prototype.setHours = function (hour, minute, second, millisecond) {
          return this.setter({ hour: hour, minute: minute, second: second, millisecond: millisecond });
      };
      LDate.prototype.setDate = function (day) {
          return this.setter({ day: day });
      };
      LDate.prototype.setMonth = function (month, day) {
          month++;
          return this.setter({ month: month, day: day });
      };
      LDate.prototype.setFullYear = function (year, month, day) {
          return this.setter({ year: year, month: month, day: day });
      };
      LDate.prototype.setTime = function (time) {
          this.dt = luxonTimezone.luxon.DateTime.fromMillis(time);
          return this.dt.valueOf();
      };
      LDate.prototype.setTimezone = function (timezone) {
          timezone = normTimezone(timezone);
          this.zone = timezone;
          this.dt = this.dt.setZone(timezone);
      };
      // The correct implementations of the UTC methods are omitted for not using them currently
      // but because of the Date interface they must be present
      LDate.prototype.setUTCMilliseconds = function (ms) {
          return 0;
      };
      LDate.prototype.setUTCSeconds = function (sec, ms) {
          return 0;
      };
      LDate.prototype.setUTCMinutes = function (min, sec, ms) {
          return 0;
      };
      LDate.prototype.setUTCHours = function (hours, min, sec, ms) {
          return 0;
      };
      LDate.prototype.setUTCDate = function (date) {
          return 0;
      };
      LDate.prototype.setUTCMonth = function (month, date) {
          return 0;
      };
      LDate.prototype.setUTCFullYear = function (year, month, date) {
          return 0;
      };
      LDate.prototype.toUTCString = function () {
          return '';
      };
      LDate.prototype.toTimeString = function () {
          return '';
      };
      LDate.prototype.toLocaleDateString = function () {
          return '';
      };
      LDate.prototype.toLocaleTimeString = function () {
          return '';
      };
      LDate.prototype.setter = function (obj) {
          this.dt = this.dt.set(obj);
          return this.dt.valueOf();
      };
      return LDate;
  }());
  /** @hidden */
  var luxonTimezone = {
      luxon: UNDEFINED,
      version: UNDEFINED,
      parse: function (date, s) {
          return new LDate(date, s.dataTimezone || s.displayTimezone);
      },
      /**
       * Supports two call signatures:
       * createDate(settings, dateObj|timestamp)
       * createDate(settings, year, month, date, hour, min, sec)
       * @returns IDate object
       */
      createDate: function (s, year, month, day, hour, minute, second, millisecond) {
          var displayTimezone = s.displayTimezone;
          if (isObject(year) || isString(year) || isUndefined(month)) {
              return new LDate(year, displayTimezone);
          }
          return new LDate([year || 1970, month || 0, day || 1, hour || 0, minute || 0, second || 0, millisecond || 0], displayTimezone);
      },
  };

  // tslint:disable: no-use-before-declare
  function normTimezone$1(timezone) {
      return !timezone || timezone === 'local' ? momentTimezone.moment.tz.guess() : timezone;
  }
  var MDate = /*#__PURE__*/ (function () {
      function MDate(value, timezone) {
          // tslint:disable-next-line
          this._mbsc = true;
          this.timezone = normTimezone$1(timezone);
          this.init(value);
      }
      MDate.prototype.clone = function () {
          return new MDate(this, this.timezone);
      };
      MDate.prototype.createDate = function (year, month, date, h, min, sec, ms) {
          return momentTimezone.createDate({ displayTimezone: this.timezone }, year, month, date, h, min, sec, ms);
      };
      MDate.prototype[Symbol.toPrimitive] = function (hint) {
          return this.m.toDate()[Symbol.toPrimitive](hint);
      };
      MDate.prototype.toDateString = function () {
          return this.m.format('ddd MMM DD YYYY');
      };
      MDate.prototype.toISOString = function () {
          return this.m.toISOString(true);
      };
      MDate.prototype.toJSON = function () {
          return this.m.toISOString();
      };
      MDate.prototype.valueOf = function () {
          return this.m.valueOf();
      };
      // Getters
      MDate.prototype.getDate = function () {
          return this.m.date();
      };
      MDate.prototype.getDay = function () {
          return this.m.day();
      };
      MDate.prototype.getFullYear = function () {
          return this.m.year();
      };
      MDate.prototype.getHours = function () {
          return this.m.hours();
      };
      MDate.prototype.getMilliseconds = function () {
          return this.m.milliseconds();
      };
      MDate.prototype.getMinutes = function () {
          return this.m.minutes();
      };
      MDate.prototype.getMonth = function () {
          return this.m.month();
      };
      MDate.prototype.getSeconds = function () {
          return this.m.seconds();
      };
      MDate.prototype.getTime = function () {
          return this.m.valueOf();
      };
      MDate.prototype.getTimezoneOffset = function () {
          return -this.m.utcOffset();
      };
      MDate.prototype.getUTCDate = function () {
          return this.utc().date();
      };
      MDate.prototype.getUTCDay = function () {
          return this.utc().day();
      };
      MDate.prototype.getUTCFullYear = function () {
          return this.utc().year();
      };
      MDate.prototype.getUTCHours = function () {
          return this.utc().hours();
      };
      MDate.prototype.getUTCMilliseconds = function () {
          return this.utc().milliseconds();
      };
      MDate.prototype.getUTCMinutes = function () {
          return this.utc().minutes();
      };
      MDate.prototype.getUTCMonth = function () {
          return this.utc().month();
      };
      MDate.prototype.getUTCSeconds = function () {
          return this.utc().seconds();
      };
      // Setters
      MDate.prototype.setMilliseconds = function (ms) {
          return +this.m.set({ millisecond: ms });
      };
      MDate.prototype.setSeconds = function (sec, ms) {
          return +this.m.set({ seconds: sec, milliseconds: ms });
      };
      MDate.prototype.setMinutes = function (min, sec, ms) {
          return +this.m.set({ minutes: min, seconds: sec, milliseconds: ms });
      };
      MDate.prototype.setHours = function (hours, min, sec, ms) {
          return +this.m.set({ hours: hours, minutes: min, seconds: sec, milliseconds: ms });
      };
      MDate.prototype.setDate = function (date) {
          return +this.m.set({ date: date });
      };
      MDate.prototype.setMonth = function (month, date) {
          return +this.m.set({ month: month, date: date });
      };
      MDate.prototype.setFullYear = function (year, month, date) {
          return +this.m.set({ year: year, month: month, date: date });
      };
      MDate.prototype.setTime = function (time) {
          this.init(time);
          return this.m.valueOf();
      };
      MDate.prototype.setTimezone = function (timezone) {
          this.timezone = normTimezone$1(timezone);
          this.m.tz(this.timezone);
      };
      // The original implementations of the UTC methods are commented out below for not using them currently
      // but because of the Date interface they must be present
      MDate.prototype.setUTCMilliseconds = function (ms) {
          return 0;
      };
      MDate.prototype.setUTCSeconds = function (sec, ms) {
          return 0;
      };
      MDate.prototype.setUTCMinutes = function (min, sec, ms) {
          return 0;
      };
      MDate.prototype.setUTCHours = function (hours, min, sec, ms) {
          return 0;
      };
      MDate.prototype.setUTCDate = function (date) {
          return 0;
      };
      MDate.prototype.setUTCMonth = function (month, date) {
          return 0;
      };
      MDate.prototype.setUTCFullYear = function (year, month, date) {
          return 0;
      };
      MDate.prototype.toUTCString = function () {
          return '';
      };
      MDate.prototype.toTimeString = function () {
          return '';
      };
      MDate.prototype.toLocaleDateString = function () {
          return '';
      };
      MDate.prototype.toLocaleTimeString = function () {
          return '';
      };
      // public setUTCMilliseconds(ms: number) { return this.setter({ millisecond: ms }, true).milliseconds(); }
      // public setUTCSeconds(sec: number, ms?: number) { return this.setter({ seconds: sec, milliseconds: ms }, true).seconds(); }
      // public setUTCMinutes(min: number, sec?: number, ms?: number) {
      //   return this.setter({ minutes: min, seconds: sec, milliseconds: ms }, true).minutes();
      // }
      // public setUTCHours(hours: number, min?: number, sec?: number, ms?: number) {
      //   return this.setter({ hours, minutes: min, seconds: sec, milliseconds: ms }, true).hours();
      // }
      // public setUTCDate(date: number) { return this.setter({ date }, true).date(); }
      // public setUTCMonth(month: number, date?: number) { return this.setter({ month, date }, true).month(); }
      // public setUTCFullYear(year: number, month?: number, date?: number) { return this.setter({ year, month, date }, true).year(); }
      // public toUTCString() { throw new Error('not implemented'); return ''; }
      // public toTimeString() { throw new Error('not implemented'); return ''; }
      // public toLocaleDateString() { throw new Error('not implemented'); return ''; }
      // public toLocaleTimeString() { throw new Error('not implemented'); return ''; }
      MDate.prototype.init = function (input) {
          var tz = momentTimezone.moment.tz;
          var normInput = isUndefined(input) || isString(input) || isNumber(input) || isArray(input) ? input : +input;
          var isTime = isString(input) && ISO_8601_TIME.test(input);
          this.m = isTime ? tz(normInput, 'HH:mm:ss', this.timezone) : tz(normInput, this.timezone);
      };
      MDate.prototype.utc = function () {
          return this.m.clone().utc();
      };
      return MDate;
  }());
  /** @hidden */
  var momentTimezone = {
      // ...timezonePluginBase,
      moment: UNDEFINED,
      parse: function (date, s) {
          return new MDate(date, s.dataTimezone || s.displayTimezone);
      },
      /**
       * Supports two call signatures:
       * createDate(settings, dateObj|timestamp)
       * createDate(settings, year, month, date, hour, min, sec)
       * @returns IDate object
       */
      createDate: function (s, year, month, date, h, min, sec, ms) {
          var displayTimezone = s.displayTimezone;
          if (isObject(year) || isString(year) || isUndefined(month)) {
              return new MDate(year, displayTimezone);
          }
          return new MDate([year || 1970, month || 0, date || 1, h || 0, min || 0, sec || 0, ms || 0], displayTimezone);
      },
  };

  // tslint:disable only-arrow-functions
  var CalendarNav$1 = (function () {
      CalendarNav._fname = 'calendarNav';
      CalendarNav._selector = '[mbsc-calendar-nav]';
      return CalendarNav;
  })();
  var CalendarNext$1 = (function () {
      CalendarNext._fname = 'calendarNext';
      CalendarNext._selector = '[mbsc-calendar-next]';
      return CalendarNext;
  })();
  var CalendarPrev$1 = (function () {
      CalendarPrev._fname = 'calendarPrev';
      CalendarPrev._selector = '[mbsc-calendar-prev]';
      return CalendarPrev;
  })();
  var CalendarToday$1 = (function () {
      CalendarToday._fname = 'calendarToday';
      CalendarToday._selector = '[mbsc-calendar-today]';
      return CalendarToday;
  })();

  function template$h(content) {
      return content || ''; // this is needed because otherwise if the Draggable component is empty, React throws an error
  }
  var Draggable = /*#__PURE__*/ (function (_super) {
      __extends(Draggable, _super);
      function Draggable() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      Draggable.prototype._template = function (s) {
          return template$h(s.children);
      };
      return Draggable;
  }(DraggableBase));

  var renderOptions$1 = {
      before: function (elm, options) {
          options.element = elm;
      },
  };

  var Draggable$1 = /*#__PURE__*/ (function (_super) {
      __extends(Draggable, _super);
      function Draggable() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      // tslint:disable variable-name
      Draggable._fname = 'draggable';
      Draggable._selector = '[mbsc-draggable]';
      Draggable._renderOpt = renderOptions$1;
      return Draggable;
  }(Draggable));

  // tslint:disable no-non-null-assertion
  // tslint:disable no-inferrable-types
  // tslint:disable directive-class-suffix
  // tslint:disable directive-selector
  /** @hidden */
  var DropcontainerBase = /*#__PURE__*/ (function (_super) {
      __extends(DropcontainerBase, _super);
      function DropcontainerBase() {
          var _this = _super !== null && _super.apply(this, arguments) || this;
          // tslint:enable variable-name
          // tslint:disable-next-line: variable-name
          _this._onExternalDrag = function (args) {
              var element = _this.s.element || _this._el;
              var isIn = function () {
                  return args.endY < _this._elBottom && args.endY > _this._elTop && args.endX > _this._elLeft && args.endX < _this._elRight;
              };
              var isInArea;
              switch (args.eventName) {
                  case 'onDragStart':
                      if (element) {
                          var rect = element.getBoundingClientRect();
                          _this._elTop = rect.top;
                          _this._elBottom = rect.bottom;
                          _this._elLeft = rect.left;
                          _this._elRight = rect.right;
                          _this._isItemIn = _this._isOwner = isIn();
                      }
                      break;
                  case 'onDragMove':
                      isInArea = isIn();
                      if (isInArea && !_this._isItemIn) {
                          _this._hook('onItemDragEnter', {
                              clone: args.clone,
                              data: args.event,
                              domEvent: args.domEvent,
                          });
                      }
                      else if (!isInArea && _this._isItemIn) {
                          _this._hook('onItemDragLeave', {
                              clone: args.clone,
                              data: args.event,
                              domEvent: args.domEvent,
                          });
                      }
                      _this._isItemIn = isInArea;
                      break;
                  case 'onDragEnd':
                      if (_this._isItemIn && !_this._isOwner) {
                          if (args.from) {
                              // If the external item comes from a calendar, notify the instance from the drop
                              args.from._eventDropped = true;
                          }
                          _this._hook('onItemDrop', {
                              clone: args.clone,
                              data: args.event,
                              domEvent: args.domEvent,
                          });
                      }
                      _this._isItemIn = false;
                      break;
              }
          };
          return _this;
      }
      DropcontainerBase.prototype._mounted = function () {
          this._unsubscribe = subscribeExternalDrag(this._onExternalDrag);
      };
      DropcontainerBase.prototype._destroy = function () {
          if (this._unsubscribe) {
              unsubscribeExternalDrag(this._unsubscribe);
          }
      };
      // tslint:disable variable-name
      DropcontainerBase._name = 'Dropcontainer';
      return DropcontainerBase;
  }(BaseComponent));

  function template$i(content) {
      return content || '';
  }
  var Dropcontainer = /*#__PURE__*/ (function (_super) {
      __extends(Dropcontainer, _super);
      function Dropcontainer() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      Dropcontainer.prototype._template = function (s) {
          return template$i(s.children);
      };
      return Dropcontainer;
  }(DropcontainerBase));

  var renderOptions$2 = {
      before: function (elm, options) {
          options.element = elm;
      },
  };

  var Dropcontainer$1 = /*#__PURE__*/ (function (_super) {
      __extends(Dropcontainer, _super);
      function Dropcontainer() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      // tslint:disable variable-name
      Dropcontainer._fname = 'dropcontainer';
      Dropcontainer._selector = '[mbsc-dropcontainer]';
      Dropcontainer._renderOpt = renderOptions$2;
      return Dropcontainer;
  }(Dropcontainer));

  var Eventcalendar$1 = /*#__PURE__*/ (function (_super) {
      __extends(Eventcalendar, _super);
      function Eventcalendar() {
          return _super !== null && _super.apply(this, arguments) || this;
      }
      // tslint:disable variable-name
      Eventcalendar._fname = 'eventcalendar';
      Eventcalendar._renderOpt = renderOptions;
      return Eventcalendar;
  }(Eventcalendar));

  registerComponent$1(Eventcalendar$1);
  registerComponent$1(CalendarNav$1);
  registerComponent$1(CalendarNext$1);
  registerComponent$1(CalendarPrev$1);
  registerComponent$1(CalendarToday$1);
  registerComponent$1(Draggable$1);

  exports.CalendarNav = CalendarNav$1;
  exports.CalendarNext = CalendarNext$1;
  exports.CalendarPrev = CalendarPrev$1;
  exports.CalendarToday = CalendarToday$1;
  exports.Draggable = Draggable$1;
  exports.Dropcontainer = Dropcontainer$1;
  exports.Eventcalendar = Eventcalendar$1;
  exports.autoDetect = autoDetect;
  exports.createCustomTheme = createCustomTheme;
  exports.enhance = enhance;
  exports.formatDate = formatDatePublic;
  exports.getAutoTheme = getAutoTheme;
  exports.getInst = getInst;
  exports.getJson = getJson;
  exports.globalChanges = globalChanges;
  exports.hijriCalendar = hijriCalendar;
  exports.jalaliCalendar = jalaliCalendar;
  exports.locale = locale;
  exports.localeEn = localeEn;
  exports.luxonTimezone = luxonTimezone;
  exports.momentTimezone = momentTimezone;
  exports.options = options;
  exports.parseDate = parseDate;
  exports.platform = platform$1;
  exports.print = print;
  exports.registerComponent = registerComponent$1;
  exports.setOptions = setOptions;
  exports.themes = themes;
  exports.updateRecurringEvent = updateRecurringEvent;
  exports.util = util;

  Object.defineProperty(exports, '__esModule', { value: true });

})));
