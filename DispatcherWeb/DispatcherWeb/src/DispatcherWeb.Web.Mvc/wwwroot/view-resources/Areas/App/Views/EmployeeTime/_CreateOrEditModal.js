(function ($) {
    app.modals.CreateOrEditEmployeeTimeModal = function () {

        var _modalManager;
        var _employeeTimeService = abp.services.app.employeeTime;
        var _timeClassificationService = abp.services.app.timeClassification;
        var _driverService = abp.services.app.driver;
        var _modal;
        var _$form = null;
        var _employeeIdDropdown = null;
        var _driverIdDropdown = null;
        var _timeClassificationDropdown = null;
        var _isManualTimeCheckbox = null;
        var _manualHourAmountElement;
        var _endDateTimeElement;
        var _driverCompanies = null;

        var getIsManualTime = () => _isManualTimeCheckbox.is(':checked');
        var setIsManualTime = (val) => _isManualTimeCheckbox.prop('checked', val).change();
        var getTimeClassificationAllowsManualTime = () => _modal.find("#TimeClassificationAllowsManualTime").val().toLowerCase() === "true";
        var setTimeClassificationAllowsManualTime = (val) => _modal.find("#TimeClassificationAllowsManualTime").val(val);

        function updateUIForTimeClassificationAllowsManualTime(onChange) {
            let timeClassificationAllowsManualTime = getTimeClassificationAllowsManualTime();
            if (onChange && !timeClassificationAllowsManualTime) {
                setIsManualTime(false);
            }
            let isManualTime = getIsManualTime();
            _isManualTimeCheckbox.parent().toggle(timeClassificationAllowsManualTime || isManualTime);
            _endDateTimeElement.parent().toggle(!isManualTime);
            _manualHourAmountElement.parent().toggle(isManualTime);
        };

        async function checkIfManualTimeIsAllowed() {
            let employeeId = _employeeIdDropdown.val();
            let timeClassificationId = _timeClassificationDropdown.val();
            if (!employeeId || !timeClassificationId) {
                return;
            }
            _modalManager.setBusy(true);
            abp.ui.setBusy(_modalManager.getModalContent());
            try {
                var employeeTimeClassification = await _driverService.getEmployeeTimeClassificationOrNull({
                    employeeId,
                    timeClassificationId
                });
                if (employeeTimeClassification) {
                    setTimeClassificationAllowsManualTime(employeeTimeClassification.allowForManualTime);
                } else {
                    setTimeClassificationAllowsManualTime(true);
                }
                updateUIForTimeClassificationAllowsManualTime(true);
            } finally {
                _modalManager.setBusy(false);
                abp.ui.clearBusy(_modalManager.getModalContent());
            }
        };

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _modal = _modalManager.getModal();

            _modal.find("#StartDateTime").datetimepickerInit();
            _endDateTimeElement = _modal.find("#EndDateTime");
            _endDateTimeElement.datetimepickerInit();
            _manualHourAmountElement = _modal.find("#ManualHourAmount");

            _employeeIdDropdown = _modal.find("#EmployeeId");
            _employeeIdDropdown.select2Init({
                abpServiceMethod: _employeeTimeService.getUsersSelectList,
                showAll: false,
                allowClear: false
            }).on("change", function (e) {
                checkIfManualTimeIsAllowed();
            });

            _driverIdDropdown = _modal.find("#DriverId");
            _driverIdDropdown.select2Init({
                showAll: true,
                allowClear: false
            });

            _timeClassificationDropdown = _modal.find("#TimeClassificationId");
            _timeClassificationDropdown.select2Init({
                abpServiceMethod: _timeClassificationService.getTimeClassificationsSelectList,
                abpServiceParamsGetter: (params) => ({
                    excludeProductionPay: false,
                    allowForManualTime: getIsManualTime() ? true : null,
                    //employeeId: _employeeIdDropdown.val()
                    //employeeId filtering is incorrect for past dates where the active driverId might have been different
                }),
                showAll: true,
                allowClear: false
            }).on("change", function (e) {
                checkIfManualTimeIsAllowed();
            });

            _employeeIdDropdown.change(function () {
                _driverCompanies = null;
                _driverIdDropdown.val(null).change();
                _driverIdDropdown.removeUnselectedOptions();
                updateDriverList();
            });
            _modal.find("#StartDateTime, #EndDateTime").on('dp.change', () => {
                updateDriverList();
            });

            _isManualTimeCheckbox = _modal.find("#IsManualTime");
            _isManualTimeCheckbox.on("change", function () {
                let isManualTime = getIsManualTime();
                _endDateTimeElement.parent().toggle(!isManualTime);
                _manualHourAmountElement.parent().toggle(isManualTime);
            });

            updateUIForTimeClassificationAllowsManualTime(false);
        };

        async function updateDriverList() {
            try {
                _modalManager.setBusy(true);
                let userId = _employeeIdDropdown.val();
                if (!userId) {
                    setDriverInputVisibility(false);
                    return;
                }
                if (!_driverCompanies) {
                    _driverCompanies = await abp.services.app.driver.getCompanyListForUserDrivers({ userId });
                }
                if (_driverCompanies.length === 0) {
                    setDriverInputVisibility(false);
                    return;
                }
                if (_driverCompanies.length === 1) {
                    setDriverInputVisibility(false);
                    abp.helper.ui.addAndSetDropdownValue(_driverIdDropdown, _driverCompanies[0].driverId, _driverCompanies[0].companyName);
                    return;
                }

                var employeeTimeStartDateTime = parseDateTimeStringToDateOrNull(_modal.find("#StartDateTime").val());
                var employeeTimeEndDateTime = parseDateTimeStringToDateOrNull(_modal.find("#EndDateTime").val());

                var dateToUse = employeeTimeEndDateTime || employeeTimeStartDateTime;
                if (!dateToUse) {
                    setDriverInputVisibility(false);
                    return;
                }

                let possibleCompanies = _driverCompanies.filter(c => {
                    let companyStartDate = parseDateTimeStringToDateOrNull(c.dateOfHire);
                    let companyEndDate = parseDateTimeStringToDateOrNull(c.terminationDate);
                    if (!companyStartDate || dateToUse > companyStartDate && (!companyEndDate || companyEndDate > dateToUse)) {
                        return true;
                    }
                    return false;
                });
                if (possibleCompanies.length === 0) {
                    setDriverInputVisibility(false);
                    return;
                }
                if (possibleCompanies.length === 1) {
                    setDriverInputVisibility(false);
                    abp.helper.ui.addAndSetDropdownValue(_driverIdDropdown, possibleCompanies[0].driverId, possibleCompanies[0].companyName);
                    return;
                }
                let currentDriverId = _driverIdDropdown.val();
                _driverIdDropdown.val(null).change();
                _driverIdDropdown.removeUnselectedOptions();
                possibleCompanies.forEach(company => {
                    _driverIdDropdown.append($('<option>').attr('value', company.driverId).text(company.companyName));
                });
                if (currentDriverId && possibleCompanies.some(c => c.driverId.toString() === currentDriverId)) {
                    _driverIdDropdown.val(currentDriverId).change();
                }
                setDriverInputVisibility(true);
            } finally {
                _modalManager.setBusy(false);
            }
        }

        function setDriverInputVisibility(visible) {
            _driverIdDropdown.closest('.form-group').toggle(!!visible);
        }

        function getDriverInputVisibility() {
            return _driverIdDropdown.closest('.form-group').is(':visible');
        }

        function parseDateTimeStringToDateOrNull(val) {
            if (!val) {
                return null;
            }
            let result = moment(val);
            if (!result.isValid()) {
                return null;
            }
            return result.startOf('day');
        }

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var employeeTime = _$form.serializeFormToObject();

            if (getDriverInputVisibility() && !employeeTime.DriverId) {
                abp.message.error('Driver Company is required');
                return;
            }

            if (getIsManualTime()) {
                employeeTime.IsManualTime = true;
                employeeTime.EndDateTime = employeeTime.StartDateTime;
            } else {
                employeeTime.manualHourAmount = null;
            }

            if (employeeTime.EndDateTime && !abp.helper.validateStartEndDates(
                { value: employeeTime.StartDateTime, title: _$form.find('label[for="StartDateTime"]').text() },
                { value: employeeTime.EndDateTime, title: _$form.find('label[for="EndDateTime"]').text() }
            )) {
                return;
            }

            try {
                _modalManager.setBusy(true);
                await _employeeTimeService.editEmployeeTime(employeeTime);
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.createOrEditEmployeeTimeModalSaved');
            } finally {
                _modalManager.setBusy(false);
            }
        };
    };
})(jQuery);
