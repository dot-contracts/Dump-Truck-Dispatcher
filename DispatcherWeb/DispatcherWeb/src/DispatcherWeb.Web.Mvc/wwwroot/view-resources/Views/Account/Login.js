var CurrentPage = function () {

    var handleLogin = function () {
        var $loginForm = $('.login-form');
        var $loginButton = $loginForm.find('#login-button');

        $loginForm.validate({
            rules: {
                username: {
                    required: true
                },
                password: {
                    required: true
                }
            }
        });

        $loginForm.find('input').keypress(function (e) {
            if (e.which === 13) {
                if ($loginForm.valid()) {
                    $loginForm.submit();
                }
                return false;
            }
        });

        $loginForm.submit(function (e) {
            e.preventDefault();

            let usernameInput = $loginForm.find('[name=usernameOrEmailAddress]');
            usernameInput.val((usernameInput.val() || '').trim());

            if (!$loginForm.valid()) {
                return;
            }

            if ($loginButton.is(':disabled')) {
                return;
            }
            $loginButton.prop('disabled', true);

            app.localStorage.clear();
            abp.ui.setBusy(
                null,
                abp.ajax({
                    contentType: app.consts.contentTypes.formUrlencoded,
                    url: $loginForm.attr('action'),
                    data: $loginForm.serialize()
                }).fail(() => { //only unlock on fail, do not unlock on success and instead wait for the redirect to complete
                    $loginButton.prop('disabled', false);
                })
            );
        });

        $loginButton.prop('disabled', false);
        $loginForm.find('[name=usernameOrEmailAddress]').prop('disabled', false);
        $loginForm.find('[name=password]').prop('disabled', false);

        $('a.social-login-icon').click(function () {
            var $a = $(this);
            var $form = $a.closest('form');
            $form.find('input[name=provider]').val($a.attr('data-provider'));
            $form.submit();
        });

        $loginForm.find('input[name=returnUrlHash]').val(location.hash);

        $('input[type=text]').first().focus();
    };

    return {
        init: function () {
            handleLogin();
        }
    };

}();
