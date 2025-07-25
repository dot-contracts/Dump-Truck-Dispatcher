/* Here, there are some custom plug-ins.
 * Developed for ASP.NET Iteration Zero (http://aspnetzero.com). */
(function ($) {
    if (!$) {
        return;
    }

    /* A simple jQuery plug-in to make a button busy. */
    $.fn.buttonBusy = function (isBusy) {
        return $(this).each(function () {
            var $button = $(this);
            var $icon = $button.find('i');
            var $buttonInnerSpan = $button.find('span');

            if (isBusy) {
                if ($button.hasClass('button-busy')) {
                    return;
                }

                $button.attr('disabled', 'disabled');

                //change icon
                if ($icon.length) {
                    $button.data('iconOriginalClasses', $icon.attr('class'));
                    $icon.removeClass();
                    $icon.addClass('fa fa-spin fa-spinner');
                }

                //change text
                if ($buttonInnerSpan.length && $button.attr('busy-text')) {
                    $button.data('buttonOriginalText', $buttonInnerSpan.html());
                    $buttonInnerSpan.html($button.attr('busy-text'));
                }

                $button.addClass('button-busy');
            } else {
                if (!$button.hasClass('button-busy')) {
                    return;
                }

                //enable button
                $button.removeAttr('disabled');

                //restore icon
                if ($icon.length && $button.data('iconOriginalClasses')) {
                    $icon.removeClass();
                    $icon.addClass($button.data('iconOriginalClasses'));
                }

                //restore text
                if ($buttonInnerSpan.length && $button.data('buttonOriginalText')) {
                    $buttonInnerSpan.html($button.data('buttonOriginalText'));
                }

                $button.removeClass('button-busy');
            }
        });
    };

    $.fn.serializeFormToObject = function () {
        var $form = $(this);
        var fields = $form.find('[disabled]');
        fields.prop('disabled', false);
        var json = $form.serializeJSON();
        fields.prop('disabled', true);
        return json;
    };

    $.fn.serializeFormWithMultipleToObject = function () {
        //serialize to array
        var data = $(this).serializeArray();

        //add also disabled items
        $(':disabled[name]', this)
            .each(function () {
                data.push({ name: this.name, value: $(this).val() });
            });

        //map to object
        var obj = {};
        data.map(function (x) {
            if (obj[x.name] === undefined) {
                obj[x.name] = x.value;
            } else if ($.isArray(obj[x.name])) {
                obj[x.name].push(x.value);
            } else {
                obj[x.name] = [obj[x.name], x.value];
            }
        });

        return obj;
    };

})(jQuery);
