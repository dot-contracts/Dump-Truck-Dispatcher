(function ($) {
    app.modals.CreateOrEditVehicleServiceModal = function () {

        var _modalManager;
        var _vehicleServiceService = abp.services.app.vehicleService;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();



            _$form.find('#Document').click(function (e) {
                if (app.showWarningIfFreeVersion()) {
                    e.preventDefault();
                    return;
                }
                if (!_$form.valid()) {
                    e.preventDefault();
                    _$form.showValidateMessage();
                }
            });
            _$form.find('#Document').fileupload({
                add: function (e, data) {
                    var vehicleServiceId = _$form.find('#Id').val();
                    if (vehicleServiceId == 0) {
                        saveService(function (vehicleServiceId) {
                            addDocument(data, vehicleServiceId);
                        });
                    } else {
                        addDocument(data, vehicleServiceId);
                    }
                },
                submit: function (e, data) {
                    _modalManager.setBusy(true);
                    $('#Uploading').show();
                },
                done: function (e, data) {
                    var template = $('#documentTemplate').html();
                    Mustache.parse(template);
                    var htmlRow = Mustache.render(template, data.result.result);
                    var $tableBody = $('#DocumentsTable tbody');
                    var istableBodyBlank = ($('#DocumentsTable tbody').html() !== '' ? false : true);
                    $tableBody.append(htmlRow);

                    _modalManager.setBusy(false);
                    $('#Uploading').hide();



                }
            });
            function addDocument(data, vehicleServiceId) {
                if (data.files.length > 0) {
                    var fileName = data.files[0].name;
                    var fileExt = fileName.split('.').pop().toLowerCase();
                    if (fileExt !== "pdf") {
                        abp.message.warn('Only pdf files can be uploaded.', fileName + " is not of the appropriate type.");
                        return;
                    }
                }
                data.formData = { 'id': vehicleServiceId };
                data.submit();
            }

            _$form.find('#DocumentsTable').on('click', 'button.btnDelete', function (e) {
                e.preventDefault();
                $(this).closest('tr').remove();
            });
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var vehicleService = _$form.serializeFormToObject();


            if (vehicleService.RecommendedHourInterval && vehicleService.WarningHours) {
                if (!abp.helper.checkGreaterNumber(
                    { value: $("#WarningHours").val(), title: $('label[for="WarningHours"]').text() },
                    { value: $("#RecommendedHourInterval").val(), title: $('label[for="RecommendedHourInterval"]').text() }
                )) {
                    return;
                }
            }

            if (vehicleService.RecommendedMileageInterval && vehicleService.WarningMiles) {
                if (!abp.helper.checkGreaterNumber(
                    { value: $("#WarningMiles").val(), title: $('label[for="WarningMiles"]').text() },
                    { value: $("#RecommendedMileageInterval").val(), title: $('label[for="RecommendedMileageInterval"]').text() }
                )) {
                    return;
                }
            }

            if (vehicleService.RecommendedTimeInterval && vehicleService.WarningDays) {
                if (!abp.helper.checkGreaterNumber(
                    { value: $("#WarningDays").val(), title: $('label[for="WarningDays"]').text() },
                    { value: $("#RecommendedTimeInterval").val(), title: $('label[for="RecommendedTimeInterval"]').text() }
                )) {
                    return;
                }
            }

            saveService(function () {
                _modalManager.close();
            });
        };

        function saveService(doneAction) {
            var model = _$form.serializeFormToObject();
            model.Documents = getDocuments();
            _modalManager.setBusy(true);
            _vehicleServiceService.save(model)
                .done(function (result) {
                    $('#Id').val(result.id);
                    abp.notify.info('Saved successfully.');
                    abp.event.trigger('app.createOrEditVehicleServiceModalSaved');
                    if (doneAction) {
                        doneAction(result.id);
                    }
                }).always(function () {
                    _modalManager.setBusy(false);
                });

        }

        function getDocuments() {
            var $documentRows = _$form.find('#DocumentsTable tbody tr');
            var documents = [];
            $documentRows.each(function (i) {
                var $row = $(this);
                var doc = {
                    Id: $row.data('id'),
                    FileId: $row.data('file-id'),
                    Name: $row.find('td:nth-child(2) input').val(),
                    Description: $row.find('td:nth-child(3) input').val()
                };

                documents.push(doc);
            });
            return documents;
        }


    };
})(jQuery);
