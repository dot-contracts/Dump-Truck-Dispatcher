
(function ($) {
    app.modals.AddPayStatementModal = function () {

        var _modalManager;
        var _payStatementService = abp.services.app.payStatement;
        var _dtHelper = abp.helper.dataTables;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            var modal = _modalManager.getModal();
            _$form = modal.find('form');
            _$form.validate();

            _$form.find("#DateRange").daterangepicker({
                locale: {
                    cancelLabel: 'Clear'
                },
                showDropDown: true,
                autoUpdateInput: false
            }).on('apply.daterangepicker', function (ev, picker) {
                $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
            }).on('cancel.daterangepicker', function (ev, picker) {
                $(this).val('');
            });

        };


        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var model = _$form.serializeFormToObject();
            $.extend(model, _dtHelper.getDateRangeObject(model.DateRange, 'startDate', 'endDate'));
            delete model.dateRange;
            model.includeProductionPay = _$form.find("#IncludeProductionPay").is(':checked');
            model.includeHourly = _$form.find("#IncludeHourly").is(':checked');
            //model.includeSalary = _$form.find("#IncludeSalary").is(':checked');

            if (!model.includeHourly && !model.includeProductionPay) {
                abp.message.warn(app.localize('YouMustSelectPayStatementCheckboxes'));
                return;
            }
            model.LocalEmployeesOnly = true;

            try {
                if (model.includeHourly) {
                    _modalManager.setBusy(true);
                    var haveTimeWithNoEnd = await _payStatementService.haveEmployeeTimeWithNoEnd(model);
                    if (haveTimeWithNoEnd) {
                        _modalManager.setBusy(false);
                        if (!await abp.message.confirm(app.localize('TheyWillNotBeIncludedInStatementPrompt'), app.localize('IncompleteTimeEntriesInRange'))) {
                            return;
                        }
                    }
                }

                if (model.includeProductionPay) {
                    _modalManager.setBusy(true);
                    var pastProductionPayTicketCount = await _payStatementService.getPastProductionPayTicketCount(model);
                    if (pastProductionPayTicketCount) {
                        if (await abp.message.confirmWithOptions({
                            text: app.localize('YouHave{0}TicketsFromPriorPayPeriodPrompt', pastProductionPayTicketCount),
                            title: ' ',
                            buttons: ['No', 'Yes']
                        })) {
                            model.includePastTickets = true;
                        }
                    }
                }

                _modalManager.setBusy(true);

                var hasProductionPayTickets = await _payStatementService.hasProductionPayTickets({
                    ...model,
                    includeUnverified: true,
                    excludeVerified: true,
                });
                if (hasProductionPayTickets) {
                    if (!await abp.message.confirmWithDangerYesNo(app.localize('ThereAreUnverifiedProductionPayTicketsPrompt'))) {
                        return;
                    }
                }

                var driversWithMissingRates = await _payStatementService.getDriversWithMissingRates(model);
                if (driversWithMissingRates.length > 0) {
                    var driverNames = driversWithMissingRates.join('; \n');
                    if (!await abp.message.confirm(app.localize('DriversDontHaveRates_KeepPayStatement{0}', driverNames))) {
                        return;
                    }
                }

                await _payStatementService.addPayStatement(model);
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.addDriverPayStatementModalSaved');
            } finally {
                _modalManager.setBusy(false);
            }
        };
    };
})(jQuery);
