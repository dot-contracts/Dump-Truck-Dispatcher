(function () {
    $(function () {
        var _tenantService = abp.services.app.tenant;
        var _demoDataService = abp.services.app.demoData;
        var _$tenantsTable = $("#TenantsTable");
        var _$tenantsTableFilter = $('#TenantsTableFilter');
        var _$tenantsFormFilter = $('#TenantsFormFilter');
        var _$tenantsStatusFilter = $('#StatusFilter');
        var _$subscriptionEndDateRangeActive = $("#TenantsTable_SubscriptionEndDateRangeActive");
        var _$subscriptionEndDateRange = _$tenantsFormFilter.find("input[name='SubscriptionEndDateRange']");
        var _$creationDateRangeActive = $("#TenantsTable_CreationDateRangeActive");
        var _$creationDateRange = _$tenantsFormFilter.find("input[name='CreationDateRange']");
        var _$refreshButton = _$tenantsFormFilter.find("button[name='RefreshButton']");
        var _$editionDropdown = _$tenantsFormFilter.find("#EditionDropdown");
        var _entityTypeFullName = 'DispatcherWeb.MultiTenancy.Tenant';

        var _permissions = {
            create: abp.auth.hasPermission('Pages.Tenants.Create'),
            edit: abp.auth.hasPermission('Pages.Tenants.Edit'),
            changeFeatures: abp.auth.hasPermission('Pages.Tenants.ChangeFeatures'),
            impersonation: abp.auth.hasPermission('Pages.Tenants.Impersonation'),
            addMonthToDriverDOTRequirements: abp.auth.hasPermission('Pages.Tenants.AddMonthToDriver'),
            addDemoData: abp.auth.hasPermission('Pages.Tenants.AddDemoData'),
            'delete': abp.auth.hasPermission('Pages.Tenants.Delete'),
            deleteDispatchData: abp.auth.hasPermission('Pages.Tenants.DeleteDispatchData')
        };

        var _urlParams = {
            creationDateStart: url('?creationDateStart'),
            creationDateEnd: url('?creationDateEnd'),
            subscriptionEndDateStart: url('?subscriptionEndDateStart'),
            subscriptionEndDateEnd: url('?subscriptionEndDateEnd')
        };

        var _selectedSubscriptionEndDateRange = {
            startDate: _urlParams.subscriptionEndDateStart ? moment(_urlParams.subscriptionEndDateStart) : moment().startOf('day'),
            endDate: _urlParams.subscriptionEndDateEnd ? moment(_urlParams.subscriptionEndDateEnd) : moment().add(30, 'days').endOf('day')
        };

        var _selectedCreationDateRange = {
            startDate: _urlParams.creationDateStart ? moment(_urlParams.creationDateStart) : moment().add(-7, 'days').startOf('day'),
            endDate: _urlParams.creationDateEnd ? moment(_urlParams.creationDateEnd) : moment().endOf('day')
        };

        $('#TenantsTableFilter').on("keypress", function (e) {
            if (e.key === "Enter")
                _$tenantsFormFilter.find("button[name='RefreshButton']").click();
        });

        _$subscriptionEndDateRange.daterangepicker(
            $.extend(true, app.createDateRangePickerOptions({
                allowFutureDate: true
            }), _selectedSubscriptionEndDateRange),
            function (start, end, label) {
                _selectedSubscriptionEndDateRange.startDate = start;
                _selectedSubscriptionEndDateRange.endDate = end;
            });

        _$creationDateRange.daterangepicker(
            $.extend(true, app.createDateRangePickerOptions(), _selectedCreationDateRange),
            function (start, end, label) {
                _selectedCreationDateRange.startDate = start;
                _selectedCreationDateRange.endDate = end;
            });

        var _createModal = new app.ModalManager({
            viewUrl: abp.appPath + "App/Tenants/CreateModal",
            scriptUrl: abp.appPath + "view-resources/Areas/App/Views/Tenants/_CreateModal.js",
            modalClass: "CreateTenantModal"
        });

        var _editModal = new app.ModalManager({
            viewUrl: abp.appPath + "App/Tenants/EditModal",
            scriptUrl: abp.appPath + "view-resources/Areas/App/Views/Tenants/_EditModal.js",
            modalClass: "EditTenantModal"
        });

        var _featuresModal = new app.ModalManager({
            viewUrl: abp.appPath + "App/Tenants/FeaturesModal",
            scriptUrl: abp.appPath + "view-resources/Areas/App/Views/Tenants/_FeaturesModal.js",
            modalClass: "TenantFeaturesModal"
        });

        var _userLookupModal = app.modals.LookupModal.create({
            title: app.localize("SelectAUser"),
            serviceMethod: abp.services.app.commonLookup.findUsers
        });

        var _entityTypeHistoryModal = app.modals.EntityTypeHistoryModal.create();

        var getFilter = function () {
            var editionId = _$editionDropdown.find(":selected").val();

            var filter = {
                filter: _$tenantsTableFilter.val(),
                editionId: editionId,
                editionIdSpecified: editionId !== "-1",
                status: _$tenantsStatusFilter.val()
            };

            if (_$creationDateRangeActive.prop("checked")) {
                filter.creationDateStart = _selectedCreationDateRange.startDate;
                filter.creationDateEnd = _selectedCreationDateRange.endDate;
            }

            if (_$subscriptionEndDateRangeActive.prop("checked")) {
                filter.subscriptionEndDateStart = _selectedSubscriptionEndDateRange.startDate;
                filter.subscriptionEndDateEnd = _selectedSubscriptionEndDateRange.endDate;
            }

            return filter;
        };

        function entityHistoryIsEnabled() {
            return abp.custom.EntityHistory
                && abp.custom.EntityHistory.IsEnabled
                && _.filter(abp.custom.EntityHistory.EnabledEntities, entityType => entityType === _entityTypeFullName).length === 1;
        }

        var dataTable = _$tenantsTable.DataTableInit({
            paging: true,
            serverSide: true,
            processing: true,
            listAction: {
                ajaxFunction: _tenantService.getTenants,
                inputFilter: function () {
                    return getFilter();
                }
            },
            columns: [
                {
                    className: 'control responsive',
                    orderable: false,
                    render: function () {
                        return '';
                    },
                },
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    defaultContent: '',
                    width: "10px",
                    rowAction: {
                        items: [
                            {
                                text: app.localize('LoginAsThisTenant'),
                                visible: function () {
                                    return _permissions.impersonation;
                                },
                                action: function (data) {
                                    _userLookupModal.open({
                                        extraFilters: {
                                            tenantId: data.record.id
                                        },
                                        title: app.localize('SelectAUser')
                                    },
                                        function (selectedItem) {
                                            abp.ajax({
                                                url: abp.appPath + 'Account/ImpersonateTenant',
                                                data: JSON.stringify({
                                                    tenantId: data.record.id,
                                                    userId: parseInt(selectedItem.value)
                                                }),
                                                success: function () {
                                                    if (!app.supportsTenancyNameInUrl) {
                                                        abp.multiTenancy.setTenantIdCookie(data.record.id);
                                                    }
                                                }
                                            });
                                        });
                                }
                            },
                            {
                                text: app.localize('Edit'),
                                visible: function () {
                                    return _permissions.edit;
                                },
                                action: function (data) {
                                    _editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: app.localize('Features'),
                                visible: function () {
                                    return _permissions.changeFeatures;
                                },
                                action: function (data) {
                                    _featuresModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: app.localize('Delete'),
                                visible: function () {
                                    return _permissions.delete;
                                },
                                action: function (data) {
                                    deleteTenant(data.record);
                                }
                            },
                            {
                                text: app.localize('DeleteDispatchData'),
                                visible: function () {
                                    return _permissions.deleteDispatchData;
                                },
                                action: function (data) {
                                    deleteDispatchDataForTenant(data.record);
                                }
                            },
                            {
                                text: app.localize('Unlock'),
                                action: function (data) {
                                    _tenantService.unlockTenantAdmin({
                                        id: data.record.id
                                    }).done(function () {
                                        abp.notify.success(app.localize('UnlockedTenandAdmin', data.record.name));
                                    });
                                }
                            },
                            {
                                text: app.localize('History'),
                                visible: function () {
                                    return entityHistoryIsEnabled();
                                },
                                action: function (data) {
                                    _entityTypeHistoryModal.open({
                                        entityTypeFullName: _entityTypeFullName,
                                        entityId: data.record.id,
                                        entityTypeDescription: data.record.name
                                    });
                                }
                            },
                            {
                                text: app.localize('AddMonthToDriverDOTRequirements'),
                                visible: function () {
                                    return _permissions.addMonthToDriverDOTRequirements;
                                },
                                action: function (data) {
                                    abp.ui.setBusy();
                                    _tenantService.addMonthToDriverDOTRequirements({
                                        id: data.record.id
                                    }).done(function () {
                                        abp.notify.success(app.localize('SavedSuccessfully'));
                                    }).always(function () {
                                        abp.ui.clearBusy();
                                    });
                                }
                            },
                            {
                                text: app.localize('AddDemoData'),
                                visible: function () {
                                    return _permissions.addDemoData;
                                },
                                action: function (data) {
                                    abp.ui.setBusy();
                                    _demoDataService.createDemoData({
                                        id: data.record.id
                                    }).done(function () {
                                        abp.notify.success(app.localize('SavedSuccessfully'));
                                    }).always(function () {
                                        abp.ui.clearBusy();
                                    });
                                }
                            }
                        ]
                    }
                },
                {
                    data: "tenancyName"
                },
                {
                    data: "name"
                },
                {
                    data: "editionDisplayName"
                },
                {
                    data: "subscriptionEndDateUtc",
                    render: function (subscriptionEndDateUtc) {
                        if (subscriptionEndDateUtc) {
                            return moment(subscriptionEndDateUtc).format('L');
                        }

                        return "";
                    }
                },
                {
                    data: "isActive",
                    render: function (isActive) {
                        if (isActive) {
                            return '<span class="label label-success">' + app.localize('Yes') + '</span>';
                        } else {
                            return '<span class="label label-default">' + app.localize('No') + '</span>';
                        }
                    }
                },
                {
                    data: "creationTime",
                    render: function (creationTime) {
                        return moment(creationTime).format('L');
                    }
                }
            ]
        });

        function getTenants() {
            dataTable.ajax.reload();
        }

        async function deleteTenant(tenant) {
            if (await abp.message.confirm(
                app.localize("TenantDeleteWarningMessage", tenant.tenancyName)
            )) {
                _tenantService.deleteTenant({
                    id: tenant.id
                }).done(function () {
                    getTenants();
                    abp.notify.success(app.localize("SuccessfullyDeleted"));
                });
            }
        }

        async function deleteDispatchDataForTenant(tenant) {
            if (await abp.message.confirm(
                app.localize("DeleteDispatchDataWarningMessage", tenant.tenancyName)
            )) {
                abp.ui.setBusy();
                _tenantService.deleteDispatchDataForTenant({
                    id: tenant.id
                }).done(function () {
                    getTenants();
                    abp.notify.success(app.localize("SuccessfullyDeleted"));
                }).always(function () {
                    abp.ui.clearBusy();
                });
            }
        }

        $('#CreateNewTenantButton').click(function () {
            _createModal.open();
        });

        $('#GetTenantsButton').click(function (e) {
            e.preventDefault();
            getTenants();
        });

        abp.event.on("app.editTenantModalSaved",
            function () {
                getTenants(true);
            });

        abp.event.on("app.createTenantModalSaved",
            function () {
                getTenants(true);
            });

        _$subscriptionEndDateRangeActive.change(function () {
            _$subscriptionEndDateRange.prop("disabled", !$(this).prop("checked"));
        });

        if (_urlParams.subscriptionEndDateStart || _urlParams.subscriptionEndDateEnd) {
            _$subscriptionEndDateRangeActive.prop("checked", true);
        } else {
            _$subscriptionEndDateRange.prop("disabled", true);
        }

        _$creationDateRangeActive.change(function () {
            _$creationDateRange.prop("disabled", !$(this).prop("checked"));
        });

        if (_urlParams.creationDateStart || _urlParams.creationDateEnd) {
            _$creationDateRangeActive.prop("checked", true);
        } else {
            _$creationDateRange.prop("disabled", true);
        }

        _$refreshButton.click(function (e) {
            e.preventDefault();
            getTenants();
        });

        _$tenantsTableFilter.focus();
    });
})();
