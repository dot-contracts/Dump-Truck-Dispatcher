(function () {
    $(function () {

        var _$rolesTable = $('#RolesTable');
        var _roleService = abp.services.app.role;
        var _entityTypeFullName = 'DispatcherWeb.Authorization.Roles.Role';

        var _permissions = {
            create: abp.auth.hasPermission('Pages.Administration.Roles.Create'),
            edit: abp.auth.hasPermission('Pages.Administration.Roles.Edit'),
            'delete': abp.auth.hasPermission('Pages.Administration.Roles.Delete')
        };

        var _createOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'App/Roles/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Roles/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditRoleModal'
        });

        var _entityTypeHistoryModal = app.modals.EntityTypeHistoryModal.create();

        $('#PermissionSelectionCombo').select2Init({
            showAll: true,
            allowClear: true,
        });

        function entityHistoryIsEnabled() {
            return abp.custom.EntityHistory
                && abp.custom.EntityHistory.IsEnabled
                && _.filter(abp.custom.EntityHistory.EnabledEntities, entityType => entityType === _entityTypeFullName).length === 1;
        }

        var dataTable = _$rolesTable.DataTableInit({
            paging: true,
            serverSide: true,
            processing: true,
            drawCallback: function (settings) {
                $('[data-toggle=m-tooltip]').tooltip();
            },
            listAction: {
                ajaxFunction: _roleService.getRoles,
                inputFilter: function () {
                    return {
                        permission: $('#PermissionSelectionCombo').val()
                    };
                }
            },
            columns: [
                {
                    width: '20px',
                    className: 'control responsive',
                    orderable: false,
                    render: function () {
                        return '';
                    }
                },
                {
                    data: "displayName",
                    render: function (displayName, type, row, meta) {
                        var $span = $('<span/>');
                        $span.append(displayName + " &nbsp;");

                        if (row.isStatic) {
                            $span.append(
                                $("<span/>")
                                    .addClass("m-badge m-badge--brand m-badge--wide mr-1")
                                    .attr("data-toggle", "m-tooltip")
                                    .attr("title", app.localize('StaticRole_Tooltip'))
                                    .attr("data-placement", "top")
                                    .text(app.localize('Static'))
                            );
                        }

                        if (row.isDefault) {
                            $span.append(
                                $("<span/>")
                                    .addClass("m-badge m-badge--metal m-badge--wide mr-1")
                                    .attr("data-toggle", "m-tooltip")
                                    .attr("title", app.localize('DefaultRole_Description'))
                                    .attr("data-placement", "top")
                                    .text(app.localize('Default'))
                            );
                        }

                        return $span[0].outerHTML;
                    }
                },
                {
                    data: "creationTime",
                    render: function (creationTime) {
                        return moment(creationTime).format('L');
                    }
                },
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    defaultContent: '',
                    width: '10px',
                    responsivePriority: 1,
                    rowAction: {
                        items: [{
                            text: '<i class="fa fa-edit"></i> ' + app.localize('Edit'),
                            visible: function () {
                                return _permissions.edit;
                            },
                            action: function (data) {
                                _createOrEditModal.open({ id: data.record.id });
                            }
                        }, {
                            text: '<i class="fa fa-trash"></i> ' + app.localize('Delete'),
                            visible: function (data) {
                                return !data.record.isStatic && _permissions.delete;
                            },
                            action: function (data) {
                                deleteRole(data.record);
                            }
                        },
                        {
                            text: '<i class="fa fa-edit"></i> ' + app.localize('History'),
                            visible: function () {
                                return entityHistoryIsEnabled();
                            },
                            action: function (data) {
                                _entityTypeHistoryModal.open({
                                    entityTypeFullName: _entityTypeFullName,
                                    entityId: data.record.id,
                                    entityTypeDescription: data.record.displayName
                                });
                            }
                        }]
                    }
                }
            ]
        });

        async function deleteRole(role) {
            let hasUsers = await _roleService.isRoleAssignedToUsers({
                id: role.id
            });

            if (hasUsers) {
                if (!await abp.message.confirm(
                    app.localize('RoleDeleteWarningMessageforHasUsers', role.displayName),
                    app.localize('AreYouSure')
                )) {
                    return;
                }
            } else {
                if (!await abp.message.confirm(
                    "",
                    app.localize('AreYouSure')
                )) {
                    return;
                }
            }

            _roleService.deleteRole({
                id: role.id
            }).done(function () {
                getRoles();
                abp.notify.success(app.localize('SuccessfullyDeleted'));
            });
        }

        $('#CreateNewRoleButton').click(function () {
            _createOrEditModal.open();
        });

        $('#RefreshRolesButton').click(function (e) {
            e.preventDefault();
            getRoles();
        });

        function getRoles() {
            dataTable.ajax.reload();
        }

        abp.event.on('app.createOrEditRoleModalSaved', function () {
            getRoles();
        });

    });
})();
