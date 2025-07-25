(function ($) {
    app.modals.RejectLeaseHaulerRequestModal = function () {
        const _leaseHaulerRequestsService = abp.services.app.leaseHaulerRequestEdit;

        let _modal;
        let _modalManager;
        let _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _modal = _modalManager.getModal();
            _$form = _modal.find('form');
            _$form.validate({
                ignore: []
            });

            const commentsInput = _$form.find('#Comments');
            const commentsLengthLabel = _$form.find('#CommentsLength');
            const clearCommentsBtn = _$form.find('#ClearCommentsBtn');
            commentsInput.on('input', function () {
                commentsLengthLabel.text(app.localize('{0}chars', (commentsInput.val() || "").length));
            });
            clearCommentsBtn.on('click', function (e) {
                commentsInput.val('').trigger('input');
            });
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            const formData = _$form.serializeFormToObject();

            _modalManager.setBusy(true);
            _leaseHaulerRequestsService.rejectJob({
                id: formData.Id,
                comments: formData.Comments
            }).done(function () {
                abp.notify.info(app.localize('LeaseHaulerRequestHasBeenRejected'));
                _modalManager.close();
                abp.event.trigger('app.leaseHaulerRequestRejectedModal');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);
