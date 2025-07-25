(function ($) {
    $(function () {
        $(document).on('click', function (e) {
            var $target = $(e.target);
            if (!$target.hasClass('fa-regular') && !$target.hasClass('fa-bell')) {
                if ($("#header_notification_bar.m-dropdown--open").length > 0) {
                    $('#header_notification_bar').removeClass('m-dropdown--open');
                }
            }
        });
    });
})(jQuery);
