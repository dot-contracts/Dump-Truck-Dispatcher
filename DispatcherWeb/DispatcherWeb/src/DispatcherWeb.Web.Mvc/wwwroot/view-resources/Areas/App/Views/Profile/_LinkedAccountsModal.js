(function ($) {
    app.modals.LinkedAccountsModal = function () {

        var _modalManager;
        var _modal;
        var _userLinkService = abp.services.app.userLink;
        var _linkedAccountsGrid;

        var _linkNewAccountModal = new app.ModalManager({
            viewUrl: abp.appPath + 'App/Profile/LinkAccountModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Profile/_LinkAccountModal.js',
            modalClass: 'LinkAccountModal'
        });

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _modal = _modalManager.getModal();
            var linkedAccountsTable = _modal.find('#LinkedAccountsTable');
            _linkedAccountsGrid = linkedAccountsTable.DataTableInit({
                paging: true,
                serverSide: true,
                processing: true,
                listAction: {
                    ajaxFunction: _userLinkService.getLinkedUsers
                },
                columns: [
                    {
                        data: null,
                        orderable: false,
                        defaultContent: '',
                        rowAction: {
                            element: $("<button/>")
                                .addClass("btn btn-primary btn-sm m-btn--icon")
                                .text(app.localize('LogIn'))
                                .prepend($("<i/>").addClass("fa-regular fa-right-to-bracket"))
                                .click(function () {
                                    switchToUser($(this).data());
                                })
                        }
                    },
                    {
                        data: "username",
                        orderable: false,
                        render: function (userName, type, row, meta) {
                            return $('<div/>').append($("<span/>").text(app.getShownLinkedUserName(row)))[0].outerHTML;
                        }
                    },
                    {
                        data: null,
                        orderable: false,
                        defaultContent: '',
                        rowAction: {
                            element: $("<button/>")
                                .addClass("btn btn-outline-danger m-btn m-btn--icon btn-sm m-btn--icon-only m-btn--pill m-btn--air")
                                .attr("title", app.localize('Delete'))
                                .append($("<i/>").addClass("fa fa-trash"))
                                .click(function () {
                                    deleteLinkedUser($(this).data());
                                })
                        }
                    }
                ],
                bDestroy: true
            });

            var linkNewAccountButton = _modal.find('#LinkNewAccountButton');
            linkNewAccountButton.click(async function () {
                try {
                    linkNewAccountButton.prop('disabled', true);
                    await _linkNewAccountModal.open({}, function () {
                        getLinkedUsers();
                    });
                } finally {
                    linkNewAccountButton.prop('disabled', false);
                }
            });
        };

        function switchToUser(linkedUser) {
            abp.ajax({
                url: abp.appPath + 'Account/SwitchToLinkedAccount',
                data: JSON.stringify({
                    targetUserId: linkedUser.id,
                    targetTenantId: linkedUser.tenantId
                }),
                success: function () {
                    if (!app.supportsTenancyNameInUrl) {
                        abp.multiTenancy.setTenantIdCookie(linkedUser.tenantId);
                    }
                }
            });
        }

        async function deleteLinkedUser(linkedUser) {
            if (await abp.message.confirm(
                app.localize('LinkedUserDeleteWarningMessage', linkedUser.username),
                app.localize('AreYouSure')
            )) {
                _userLinkService.unlinkUser({
                    userId: linkedUser.id,
                    tenantId: linkedUser.tenantId
                }).done(function () {
                    getLinkedUsers();
                    abp.notify.success(app.localize('SuccessfullyUnlinked'));
                });
            }
        }

        function getLinkedUsers() {
            _linkedAccountsGrid.ajax.reload();
        }
    };
})(jQuery);
