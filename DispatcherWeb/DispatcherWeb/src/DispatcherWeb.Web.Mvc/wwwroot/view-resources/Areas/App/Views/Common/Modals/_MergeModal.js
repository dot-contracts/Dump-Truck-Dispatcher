(function () {
    app.modals.MergeModal = function () {

        var _modalManager;

        var _options = {
            dropdownServiceMethod: null,
            mergeServiceMethod: null
        };

        var _$form = null;
        var _dropdown = null;
        var _idsToMerge = [];

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _options = $.extend(_options, _modalManager.getOptions().lookupOptions);

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _dropdown = _$form.find('#MainRecordId');
            _idsToMerge = _$form.find('#IdsToMerge').val().split(',');

            _options.dropdownServiceMethod({
                ids: _idsToMerge
            }).done(function (data) {
                $.each(data.items, function (ind, val) {
                    $('<option></option>').text(val.name).attr('value', val.id).appendTo(_dropdown);
                });
            });

            _dropdown.select2Init({
                showAll: true
            });
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var mainRecordId = _dropdown.val();

            _modalManager.setBusy(true);
            _options.mergeServiceMethod({
                idsToMerge: _idsToMerge,
                mainRecordId: mainRecordId
            }).done(function (data) {
                abp.notify.info('Merged successfully.');
                _modalManager.close();
                abp.event.trigger('app.mergeModalFinished');
            }).always(function () {
                _modalManager.setBusy(false);
            });

        };
    };

    app.modals.MergeModal.create = function (lookupOptions) {
        return new app.ModalManager({
            viewUrl: abp.appPath + 'app/Common/MergeModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Common/Modals/_MergeModal.js',
            modalClass: 'MergeModal',
            lookupOptions: lookupOptions
        });
    };
})();