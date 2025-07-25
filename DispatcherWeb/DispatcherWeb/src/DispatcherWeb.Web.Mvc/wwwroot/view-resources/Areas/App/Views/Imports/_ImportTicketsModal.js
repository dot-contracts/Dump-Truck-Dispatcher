(function ($) {
    app.modals.ImportTicketsModal = function () {

        var _modalManager;
        var _$form;
        var _importTypeDropdown;
        var _fileInput;

        this.init = function (modalManager) {
            _modalManager = modalManager;
            
            _$form = _modalManager.getModal().find('form');

            _importTypeDropdown = _$form.find('#ImportType');
            _importTypeDropdown.select2Init({
                showAll: true,
                allowClear: false,
            });
            _importTypeDropdown.change(function () {
                _$form.find('.fileUpload').show();
            });

            _fileInput = _$form.find('#ImportFile');

            _fileInput.fileupload({
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
                done: async function done(e, data) {
                    var result = data.result.result;
                    //_cancelModal.close();
                    _modalManager.close();
                    abp.ui.unblock();
                    if (result === null) {
                        abp.message.error('There were no rows to import.');
                        return;
                    }

                    abp.ui.block();
                    try {
                        let file = result.id + '/' + result.blobName;
                        
                        switch (Number(_importTypeDropdown.val())) {
                            case abp.enums.importType.trux:
                                var validationResult = await abp.services.app.importTruxEarnings.validateFile(file);
                                if (validationResult.duplicateShiftAssignments.length) {
                                    if (!await abp.message.confirm(app.localize('{0}DuplicateRecordsWereFound_WillBeSkippedIfYouContinueImport_AreYouSure', validationResult.duplicateShiftAssignments.length))) {
                                        return;
                                    }
                                }
                                if (validationResult.duplicateShiftAssignmentsInFile.length) {
                                    if (!await abp.message.confirm(app.localize('{0}DuplicateShiftAssigmentWereFoundInFile_WillBeSkippedIfYouContinueImport_AreYouSure', validationResult.duplicateShiftAssignmentsInFile.length))) {
                                        return;
                                    }
                                }
                                break;

                            case abp.enums.importType.luckStone:
                                var validationResult = await abp.services.app.importLuckStoneEarnings.validateFile(file);
                                if (validationResult.duplicateTickets.length) {
                                    if (!await abp.message.confirm(app.localize('{0}DuplicateRecordsWereFound_WillBeSkippedIfYouContinueImport_AreYouSure', validationResult.duplicateTickets.length))) {
                                        return;
                                    }
                                }
                                break;

                            case abp.enums.importType.ironSheepdog:
                                var validationResult = await abp.services.app.importIronSheepdogEarnings.validateFile(file);
                                if (validationResult.duplicateTickets.length) {
                                    if (!await abp.message.confirm(app.localize('{0}DuplicateRecordsWereFound_WillBeSkippedIfYouContinueImport_AreYouSure', validationResult.duplicateTickets.length))) {
                                        return;
                                    }
                                }
                                break;
                        }

                        await abp.services.app.importSchedule.scheduleImport({
                            blobName: file,
                            importType: _importTypeDropdown.val(),
                        });
                        await abp.message.info("The file is scheduled for importing. You will receive a notification on completion.");
                        //abp.ui.block();
                        //location = abp.appPath + 'app/tickets';
                    } finally {
                        abp.ui.unblock();
                    }
                },
                error: function error(jqXHR, textStatus, errorThrown) {
                    abp.ui.unblock();
                    if (errorThrown === 'abort') {
                        abp.notify.info('File Upload has been canceled');
                    }
                }
            });
        };

        var _uploadData;

        function showAndLogImportWarning(text, caption) {
            abp.services.app.importTruxEarnings.logImportWarning({
                location: window.location.toString(),
                text: caption + ' ' + text
            });
            abp.message.warn(text, caption);
        }
        abp.event.on('app.UploadCanceled', function () {
            _uploadData.abort();
            abp.ui.unblock();
        });
    };
})(jQuery);
