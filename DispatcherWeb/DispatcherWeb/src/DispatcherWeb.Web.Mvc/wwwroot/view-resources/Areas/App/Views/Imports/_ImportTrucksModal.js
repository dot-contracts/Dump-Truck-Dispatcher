(function ($) {
    app.modals.ImportTrucksModal = function () {

        var _modalManager;
        var _importMappingModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Imports/ImportMappingModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Imports/_ImportMappingModal.js',
            modalClass: 'ImportMappingModal',
            modalId: 'ImportMappingModal'
        });

        this.init = function (modalManager) {
            _modalManager = modalManager;
        };

        var _uploadData;

        function showAndLogImportWarning(text, caption) {
            abp.services.app.importTruxEarnings.logImportWarning({
                location: window.location.toString(),
                text: caption + ' ' + text
            });
            abp.message.warn(text, caption);
        }

        $('#ImportFile').fileupload({
            add: function add(e, data) {
                if (data.files.length > 0) {
                    var fileName = data.files[0].name;
                    var fileExt = fileName.split('.').pop().toLowerCase();
                    if (fileExt === "xlsx") {
                        showAndLogImportWarning('You can convert this xlsx file to a csv in Excel by ensuring you have selected the desired tab and "Save As" being sure to select to save as a csv.', fileName + " is not the appropriate type.");
                        return;
                    } else if (fileExt !== "csv") {
                        showAndLogImportWarning('Only csv files can be uploaded.', fileName + " is not the appropriate type.");
                        return;
                    }
                }
                data.submit();
            },
            submit: function submit(e, data) {
                _uploadData = data;
                abp.ui.block();
            },
            done: function done(e, data) {
                var result = data.result.result;
                //_cancelModal.close();
                _modalManager.close();
                abp.ui.unblock();
                if (result === null) {
                    abp.message.error('There were no rows to import.');
                    return;
                }

                _importMappingModal.open({ id: result.id, fileName: result.blobName, importType: $('#ImportType').val() });
            },
            error: function error(jqXHR, textStatus, errorThrown) {
                abp.ui.unblock();
                if (errorThrown === 'abort') {
                    abp.notify.info('File Upload has been canceled');
                }
            }
        });
        abp.event.on('app.UploadCanceled', function () {
            _uploadData.abort();
            abp.ui.unblock();
        });
    };
})(jQuery);