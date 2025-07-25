(function () {
    $(function () {

        var _$usersTable = $('#UsersTable');
        var _userService = abp.services.app.user;
        var _dtHelper = abp.helper.dataTables;
        var _showOfficeColumn = abp.session.tenantId && abp.features.isEnabled('App.AllowMultiOfficeFeature');

        var _permissions = {
            create: abp.auth.hasPermission('Pages.Administration.Users.Create'),
            edit: abp.auth.hasPermission('Pages.Administration.Users.Edit'),
            changePermissions: abp.auth.hasPermission('Pages.Administration.Users.ChangePermissions'),
            impersonation: abp.auth.hasPermission('Pages.Administration.Users.Impersonation'),
            'delete': abp.auth.hasPermission('Pages.Administration.Users.Delete')
        };

        var _createOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'App/Users/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Users/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditUserModal'
        });

        var _userPermissionsModal = new app.ModalManager({
            viewUrl: abp.appPath + 'App/Users/PermissionsModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Users/_PermissionsModal.js',
            modalClass: 'UserPermissionsModal'
        });

        if (_showOfficeColumn) {
            $("#OfficeIdFilter").select2Init({
                abpServiceMethod: listCacheSelectLists.office(),
                showAll: true
            });
        }

        $("#RoleSelectionCombo").select2Init({
            showAll: true,
            allowClear: true
        });

        var dataTable = _$usersTable.DataTableInit({
            serverSide: true,
            processing: true,
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, {
                    filter: $('#UsersTableFilter').val(),
                    permission: $("#PermissionSelectionCombo").val(),
                    role: $("#RoleSelectionCombo").val(),
                    onlyLockedUsers: $("#UsersTable_OnlyLockedUsers").is(':checked'),
                    officeId: $('#OfficeIdFilter').val()
                    //name: $('#NameFilter').val()
                });
                _userService.getUsers(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                });
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
                    title: "User Name",
                    data: "userName",
                    width: "20px",
                    render: function (userName, type, row, meta) {
                        var $container = $("<span/>");
                        if (row.profilePictureId) {
                            var profilePictureUrl = "/Profile/GetProfilePictureById?id=" + row.profilePictureId;
                            var $link = $("<a/>").addClass("m--margin-right-10").attr("href", profilePictureUrl).attr("target", "_blank");
                            var $img = $("<img/>")
                                .addClass("img-circle profileimage")
                                .attr("src", profilePictureUrl);

                            $link.append($img);
                            $container.append($link);
                        }

                        $container.append($('<span>').text(userName));
                        return $container[0].outerHTML;
                    }
                },
                {
                    title: "First Name",
                    data: "name"
                },
                {
                    title: "Last Name",
                    data: "surname"
                },
                {
                    title: "Office",
                    data: "officeName",
                    visible: _showOfficeColumn,
                },
                {
                    title: "Roles",
                    data: "roles",
                    orderable: false,
                    render: function (roles) {
                        var roleNames = $.map(roles, function (value, index) { return value.roleName; });
                        return _dtHelper.renderText(roleNames.join(', '));
                    }
                },
                {
                    title: "Email Address",
                    data: "emailAddress"
                },
                {
                    title: "Email Confirm",
                    data: "isEmailConfirmed",
                    render: function (isEmailConfirmed) {
                        if (isEmailConfirmed) {
                            return '<span class="label label-success">' + app.localize('Yes') + '</span>';
                        } else {
                            return '<span class="label label-default">' + app.localize('No') + '</span>';
                        }
                    }
                },
                {
                    title: "Active",
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
                    title: '<i class="fa fa-lock"></i>',
                    responsivePriority: 1,

                    data: "isLocked",
                    orderable: false,
                    render: function (isLocked) {
                        if (isLocked) {
                            return '<i class="fa fa-lock"></i>';
                        } else {
                            return '';
                        }
                    }
                },
                {
                    title: "Last Login Time",
                    data: "lastLoginTime",
                    render: function (lastLoginTime) {
                        if (lastLoginTime) {
                            return moment(lastLoginTime).format('L LTS');
                        }

                        return "";
                    }
                },
                {
                    title: "Creation Time",
                    data: "creationTime",
                    render: function (creationTime) {
                        return moment(creationTime).format('L');
                    }
                },
                {
                    responsivePriority: 1,
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    width: "10px",
                    render: function (data, type, full, meta) {
                        return '<div class="dropdown">'
                            + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                            + '<ul class="dropdown-menu dropdown-menu-right">'
                            + (_permissions.impersonation && full.id !== abp.session.userId
                                ? '<li><a class="btnLoginAsThisUser"> Login as this user</a></li>'
                                : '')
                            + (_permissions.edit ? '<li><a class="btnEditRow"><i class="fa fa-edit"></i> Edit</a></li>' : '')
                            + (_permissions.changePermissions ? '<li><a class="btnChangePermissionsRow"><i class="fa fa-edit"></i> Permissions</a></li>' : '')
                            + (_permissions.changePermissions && full.isLocked ? '<li><a class="btnUnlockRow"><i class="fa fa-unlock"></i> Unlock</a></li>' : '')
                            + (_permissions.delete ? '<li><a class="btnDeleteRow"><i class="fa fa-trash"></i> Delete</a></li>' : '')
                            + '</ul>'
                            + '</div>';
                    }
                }
            ]
        });


        function getUsers() {
            dataTable.ajax.reload();
        }

        async function deleteUser(user) {
            if (user.userName === app.consts.userManagement.defaultAdminUserName) {
                abp.message.warn(app.localize("{0}UserCannotBeDeleted", app.consts.userManagement.defaultAdminUserName));
                return;
            }

            if (await abp.message.confirm(
                app.localize('UserDeleteWarningMessage', user.userName),
                app.localize('AreYouSure')
            )) {
                _userService.deleteUser({
                    id: user.id
                }).done(function () {
                    getUsers(true);
                    abp.notify.success(app.localize('SuccessfullyDeleted'));
                });
            }
        }

        _$usersTable.on('click', '.btnLoginAsThisUser', function (e) {
            e.preventDefault();
            var record = _dtHelper.getRowData(this);
            abp.ajax({
                url: abp.appPath + 'Account/ImpersonateUser',
                data: JSON.stringify({
                    tenantId: abp.session.tenantId,
                    userId: record.id
                })
            });
        });

        _$usersTable.on('click', '.btnEditRow', function (e) {
            e.preventDefault();
            var record = _dtHelper.getRowData(this);
            _createOrEditModal.open({ id: record.id });
        });

        _$usersTable.on('click', '.btnChangePermissionsRow', function (e) {
            e.preventDefault();
            var record = _dtHelper.getRowData(this);
            _userPermissionsModal.open({ id: record.id });
        });

        _$usersTable.on('click', '.btnUnlockRow', function (e) {
            e.preventDefault();
            var record = _dtHelper.getRowData(this);
            _userService.unlockUser({
                id: record.id
            }).done(function () {
                abp.notify.success(app.localize('UnlockedTheUser', record.userName));
                getUsers();
            });
        });

        _$usersTable.on('click', '.btnDeleteRow', function (e) {
            e.preventDefault();
            var record = _dtHelper.getRowData(this);
            deleteUser(record);
        });

        $('#ShowAdvancedFiltersSpan').click(function () {
            $('#ShowAdvancedFiltersSpan').hide();
            $('#HideAdvancedFiltersSpan').show();
            $('#AdvacedAuditFiltersArea').slideDown();
        });

        $('#HideAdvancedFiltersSpan').click(function () {
            $('#HideAdvancedFiltersSpan').hide();
            $('#ShowAdvancedFiltersSpan').show();
            $('#AdvacedAuditFiltersArea').slideUp();
        });

        $('#CreateNewUserButton').click(function () {
            _createOrEditModal.open();
        });

        $('#GetUsersButton, #RefreshUserListButton').click(function (e) {
            e.preventDefault();
            getUsers();
        });

        $("#SearchButton").closest('form').submit(function (e) {
            e.preventDefault();
            getUsers();
        });

        $("#ClearSearchButton").click(function () {
            $(this).closest('form')[0].reset();
            $(".filter").change();
            getUsers();
        });

        $('#UsersTableFilter').on('keydown', function (e) {
            if (e.keyCode !== 13) {
                return;
            }

            e.preventDefault();
            getUsers();
        });

        abp.event.on('app.createOrEditUserModalSaved', function () {
            getUsers();
        });

        $('#UsersTableFilter').focus();

        $('#ExportUsersToCsvButton').click(function () {
            var $button = $(this);
            var abpData = {};
            $.extend(abpData, {
                filter: $('#UsersTableFilter').val(),
                permission: $("#PermissionSelectionCombo").val(),
                role: $("#RoleSelectionCombo").val(),
                onlyLockedUsers: $("#UsersTable_OnlyLockedUsers").is(':checked'),
                officeId: $('#OfficeIdFilter').val()
            });
            abp.ui.setBusy($button);
            _userService
                .getUsersToCsv(abpData)
                .done(function (result) {
                    app.downloadTempFile(result);
                }).always(function () {
                    abp.ui.clearBusy($button);
                });
        });

    });
})();
