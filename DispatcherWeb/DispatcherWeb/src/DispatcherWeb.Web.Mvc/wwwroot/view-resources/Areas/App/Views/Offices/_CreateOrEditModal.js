(function ($) {
    app.modals.CreateOrEditOfficeModal = function () {

        var _modalManager;
        var _modal;
        var _officeService = abp.services.app.office;
        var _$form = null;
        var _officeId = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _modal = _modalManager.getModal();

            _$form = _modal.find('#OfficeForm');
            _$form.validate();

            _officeId = _$form.find("#Id").val();
            var truckColorContainer = _$form.find('#TruckColorContainer');

            truckColorContainer.colorpicker({
                format: 'hex',
                useAlpha: false,
                autoInputFallback: true,
                customClass: 'colorpicker-2x',
                sliders: {
                    saturation: {
                        maxLeft: 200,
                        maxTop: 200
                    },
                    hue: {
                        maxTop: 200
                    },
                    alpha: {
                        maxTop: 200
                    }
                }
            });

            var truckColorPicker = truckColorContainer.data("colorpicker");

            _$form.find('#TruckColor').click(function () {
                truckColorPicker.show();
            });

            _$form.find("#DefaultStartTime").timepickerInit({ stepping: 1 });

            //Office/ApplicationLogo
            var officeLogoUploadForm = _modal.find('#OfficeLogoUploadForm');
            officeLogoUploadForm.ajaxForm({
                beforeSubmit: function (formData, jqForm, options) {

                    if (_officeId === '') {
                        saveOfficeAsync(() => {
                            officeLogoUploadForm.trigger('submit');
                        });
                        return false;
                    }

                    var $fileInput = officeLogoUploadForm.find('input[name=ApplicationLogoImage]');
                    var files = $fileInput.get()[0].files;

                    if (!files.length) {
                        return false;
                    }

                    var file = files[0];

                    //File type check
                    var type = file.type.slice(file.type.lastIndexOf('/') + 1);
                    if (!app.allowedLogoTypes.includes(type)) {
                        abp.message.warn(app.localize('File_Invalid_Type_Error'));
                        return false;
                    }

                    //File size check
                    if (file.size > app.maxLogoSize)
                    {
                        abp.message.warn(app.localize('File_SizeLimit_Error'));
                        return false;
                    }

                    return true;
                },
                success: function (response) {
                    if (response.success) {
                        //refreshLogo(abp.appPath + 'TenantCustomization/GetLogo?id=' + response.result.id);
                        officeLogoUploadForm.find('button[type=reset]').show();
                        abp.notify.info(app.localize('SavedSuccessfully'));
                    } else {
                        abp.message.error(response.error.message);
                    }
                }
            });

            officeLogoUploadForm.find('button[type=reset]').click(function () {
                _officeService.clearLogo(_officeId).done(function () {
                    //refreshLogo(abp.appPath + 'Common/Images/app-logo-dump-truck-130x35.gif');
                    abp.notify.info(app.localize('ClearedSuccessfully'));
                    officeLogoUploadForm.find('button[type=reset]').hide();
                    updateSubmitButtonStateForFileUploadForm(officeLogoUploadForm.find('input[name=ApplicationLogoImage]'));
                });
            });

            officeLogoUploadForm.find('input[name=ApplicationLogoImage]').on("change", function () {
                var filename = $(this).val().split('\\').pop();
                officeLogoUploadForm.find("#uploadApplicationLogoFile").val(filename);
                updateSubmitButtonStateForFileUploadForm(this);
            });

            //Office/ReportsLogo
            var officeReportsLogoUploadForm = _modal.find('#OfficeReportsLogoUploadForm');
            officeReportsLogoUploadForm.ajaxForm({
                beforeSubmit: function (formData, jqForm, options) {

                    if (_officeId === '') {
                        saveOfficeAsync(() => {
                            officeReportsLogoUploadForm.trigger('submit');
                        });
                        return false;
                    }

                    var $fileInput = officeReportsLogoUploadForm.find('input[name=ReportsLogoImage]');
                    var files = $fileInput.get()[0].files;

                    if (!files.length) {
                        return false;
                    }

                    var file = files[0];

                    //File type check
                    var type = file.type.slice(file.type.lastIndexOf('/') + 1);
                    if (!app.allowedReportLogoTypes.includes(type)) {
                        abp.message.warn(app.localize('File_Invalid_Type_Error'));
                        return false;
                    }

                    //File size check
                    if (file.size > app.maxReportLogoSize)
                    {
                        abp.message.warn(app.localize('File_SizeLimit_Error'));
                        return false;
                    }

                    return true;
                },
                success: function (response) {
                    if (response.success) {
                        officeReportsLogoUploadForm.find('button[type=reset]').show();
                        abp.notify.info(app.localize('SavedSuccessfully'));
                    } else {
                        abp.message.error(response.error.message);
                    }
                }
            });

            officeReportsLogoUploadForm.find('button[type=reset]').click(function () {
                _officeService.clearReportsLogo(_officeId).done(function () {
                    abp.notify.info(app.localize('ClearedSuccessfully'));
                    officeReportsLogoUploadForm.find('button[type=reset]').hide();
                    updateSubmitButtonStateForFileUploadForm(officeReportsLogoUploadForm.find('input[name=ReportsLogoImage]'));
                });
            });

            officeReportsLogoUploadForm.find('input[name=ReportsLogoImage]').on("change", function () {
                var filename = $(this).val().split('\\').pop();
                officeReportsLogoUploadForm.find("#uploadReportsLogoFile").val(filename);
                updateSubmitButtonStateForFileUploadForm(this);
            });

            function updateSubmitButtonStateForFileUploadForm(inputFile) {
                var $form = $(inputFile).closest('form');
                if ($(inputFile).val()) {
                    $form.find('button:submit').removeAttr('disabled');
                } else {
                    $form.find('button:submit').attr('disabled', true);
                }
            }

            //function refreshLogo(url) {
            //    $('#AppLogo').attr('src', url);
            //}
        };

        this.save = function () {
            saveOfficeAsync(function (editResult) {
                _modalManager.close();
            });
        };

        var saveOfficeAsync = function (callback) {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var office = _$form.serializeFormToObject();

            abp.ui.setBusy(_$form);
            _modalManager.setBusy(true);
            _officeService.editOffice(office).done(function (editResult) {
                abp.notify.info('Saved successfully.');
                _officeId = editResult.id;
                _$form.find("#Id").val(_officeId);
                _modal.find("#OfficeLogoUploadForm_Id").val(_officeId);
                _modal.find("#OfficeReportsLogoUploadForm_Id").val(_officeId);
                if (callback)
                    callback(editResult);
                abp.event.trigger('app.createOrEditOfficeModalSaved');
            }).always(function () {
                abp.ui.clearBusy(_$form);
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);
