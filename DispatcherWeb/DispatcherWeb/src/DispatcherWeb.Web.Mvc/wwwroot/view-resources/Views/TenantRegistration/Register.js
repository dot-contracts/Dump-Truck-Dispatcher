var CurrentPage = function () {

    function setPayment() {
        var $periodType = $('input[name=PaymentPeriodType]:checked');
        $('input[name=DayCount]').val($periodType.data('day-count') ? $periodType.data('day-count') : 0);
    }

    var _passwordComplexityHelper = new app.PasswordComplexityHelper();

    var handleRegister = function () {

        $('input[name=PaymentPeriodType]').change(function () {
            setPayment();
        });

        $('input[name=PaymentPeriodType]:first').prop('checked', true);

        setPayment();

        $('.register-form').validate({
            errorElement: 'div',
            errorClass: 'form-control-feedback',
            focusInvalid: false, // do not focus the last invalid input
            ignore: ':hidden',
            rules: {
                AdminPasswordRepeat: {
                    equalTo: "#AdminPassword"
                }
            },
            messages: {

            },
            invalidHandler: function (event, validator) {

            },
            highlight: function (element) {
                $(element).closest('.form-group').addClass('has-danger');
            },
            success: function (label) {
                label.closest('.form-group').removeClass('has-danger');
                label.remove();
            },
            errorPlacement: function (error, element) {
                if (element.closest('.input-icon').length === 1) {
                    error.insertAfter(element.closest('.input-icon'));
                } else {
                    error.insertAfter(element);
                }
            },
            submitHandler: function (form) {
                form.submit();
            }
        });

        $('.register-form input').keypress(function (e) {
            if (e.which === 13) {
                if ($('.register-form').valid()) {
                    $('.register-form').submit();
                }
                return false;
            }
        });

        _passwordComplexityHelper.setPasswordComplexityRules($("input[name=AdminPassword],input[name=AdminPasswordRepeat]"));
    }

    function init() {
        handleRegister();
    }

    return {
        init: init
    };
}();