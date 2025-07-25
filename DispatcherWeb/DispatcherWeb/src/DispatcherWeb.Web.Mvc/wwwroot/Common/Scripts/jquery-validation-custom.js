(function ($) {
    $.validator.setDefaults({
        errorElement: 'div',
        errorClass: 'form-control-feedback',
        focusInvalid: false,
        submitOnKeyPress: true,
        ignore:':hidden',
        highlight: function (element) {
            $(element).closest('.form-group').addClass('has-danger');
        },

        unhighlight: function (element) {
            $(element).closest('.form-group').removeClass('has-danger');
        },

        errorPlacement: function (error, element) {
            if (element.hasClass('select2-hidden-accessible')) {
                element = element.parent().children('span.select2-container');
            }
            if (element.closest('.input-icon').length === 1) {
                error.insertAfter(element.closest('.input-icon'));
            } else {
                error.insertAfter(element);
            }
        },

        success: function (label) {
            label.closest('.form-group').removeClass('has-danger');
            label.remove();
        },

        submitHandler: function (form) {
            $(form).find('.alert-danger').hide();
        }
    });

    $.validator.addMethod("email", function (value, element) {
        return /^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/.test(value);
    }, "Please enter a valid Email.");


    // Source: https://stackoverflow.com/questions/995183/how-to-allow-only-numeric-0-9-in-html-inputbox-using-jquery
    // Usage:
    // Restrict input to digits by using a regular expression filter.
    // $("#myTextBox").inputFilter(function (value) {
    //	 return /^\d*$/.test(value);
    // });
    $.fn.inputFilter = function (inputFilter) {
        return this.on("input keydown keyup mousedown mouseup select contextmenu drop", function () {
            if (inputFilter(this.value)) {
                this.oldValue = this.value;
                this.oldSelectionStart = this.selectionStart;
                this.oldSelectionEnd = this.selectionEnd;
            } else if (this.hasOwnProperty("oldValue")) {
                this.value = this.oldValue;
                this.setSelectionRange(this.oldSelectionStart, this.oldSelectionEnd);
            }
        });
    };
})(jQuery);
