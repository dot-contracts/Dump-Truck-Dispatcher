(function () {
    $(function () {

        var _$editionsTable = $('#EditionsTable');
        var _editionService = abp.services.app.edition;

        var _permissions = {
            create: abp.auth.hasPermission('Pages.Editions.Create'),
            edit: abp.auth.hasPermission('Pages.Editions.Edit'),
            'delete': abp.auth.hasPermission('Pages.Editions.Delete')
        };

        var _createOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'App/Editions/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Editions/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditEditionModal'
        });

        async function deleteEdition(edition) {
            if (await abp.message.confirm(app.localize('EditionDeleteWarningMessage', edition.displayName))) {
                _editionService.deleteEdition({
                    id: edition.id
                }).done(function () {
                    getEditions();
                    abp.notify.success(app.localize('SuccessfullyDeleted'));
                });
            }
        };

        $('#CreateNewEditionButton').click(function () {
            _createOrEditModal.open();
        });

        abp.event.on('app.createOrEditEditionModalSaved', function () {
            getEditions();
        });

        var dataTable = _$editionsTable.DataTableInit({
            paging: false,
            listAction: {
                ajaxFunction: _editionService.getEditions
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
                                text: '<i class="fa fa-edit"></i> ' + app.localize('Edit'),
                                visible: function () {
                                    return _permissions.edit;
                                },
                                action: function (data) {
                                    _createOrEditModal.open({ id: data.record.id });
                                }
                            }, {
                                text: '<i class="fa fa-trash"></i> ' + app.localize('Delete'),
                                visible: function () {
                                    return _permissions.delete;
                                },
                                action: function (data) {
                                    deleteEdition(data.record);
                                }
                            }
                        ]
                    }
                },
                {
                    data: "displayName"
                },
                {
                    data: "creationTime",
                    render: function (creationTime) {
                        return moment(creationTime).format('L');
                    }
                }
            ]
        });

        function getEditions() {
            dataTable.ajax.reload();
        }
    });
})();