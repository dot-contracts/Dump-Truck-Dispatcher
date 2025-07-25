(function () {
    $(function () {

        var _$languagesTable = $('#LanguagesTable');
        var _languageService = abp.services.app.language;
        var _defaultLanguageName = null;

        var _permissions = {
            create: abp.auth.hasPermission('Pages.Administration.Languages.Create'),
            edit: abp.auth.hasPermission('Pages.Administration.Languages.Edit'),
            'delete': abp.auth.hasPermission('Pages.Administration.Languages.Delete')
        };

        var _createOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'App/Languages/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/Languages/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditLanguageModal'
        });

        var dataTable = _$languagesTable.DataTableInit({
            paging: false,
            serverSide: false,
            processing: false,
            listAction: {
                ajaxFunction: _languageService.getLanguages
            },
            columns: [
                {
                    className: 'control responsive',
                    orderable: false,
                    render: function () {
                        return '';
                    }
                },
                {
                    data: "displayName",
                    render: function (displayName, type, row, meta) {
                        var $span = $('<span/>')
                            .append($("<i/>").addClass(row.icon).addClass("mr-2"))
                            .append($("<span/>").attr("data-language-name", row.name).text(row.displayName));

                        if (meta.settings.rawServerResponse.defaultLanguageName === row.name) {
                            $span.addClass("text-bold").append(" (" + app.localize("Default") + ")");
                        }

                        return $span[0].outerHTML;
                    }
                },
                {
                    data: "name"
                },
                {
                    data: "creationTime",
                    render: function (creationTime) {
                        return moment(creationTime).format('L');
                    }
                },
                {
                    data: "isDisabled",
                    render: function (isDisabled) {
                        var isEnabled = !isDisabled;
                        var $span = $("<span/>").addClass("label");
                        if (isEnabled) {
                            $span.addClass("m-badge m-badge--success m-badge--wide").text(app.localize('Yes'));
                        } else {
                            $span.addClass("m-badge m-badge--metal m-badge--wide").text(app.localize('No'));
                        }

                        return $span[0].outerHTML;
                    }
                },
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    defaultContent: '',
                    width: "10px",
                    rowAction: {
                        items: [{
                            text: '<i class="fa fa-edit"></i> ' + app.localize('Edit'),
                            visible: function (data) {
                                return _permissions.edit;
                            },
                            action: function (data) {
                                _createOrEditModal.open({ id: data.record.id });
                            }
                        }, {
                            text: '<i class="fa fa-edit"></i> ' + app.localize('SetAsDefaultLanguage'),
                            visible: function () {
                                return _permissions.edit;
                            },
                            action: function (data) {
                                setAsDefaultLanguage(data.record);
                            }
                        }, {
                            text: '<i class="fa fa-trash"></i> ' + app.localize('Delete'),
                            visible: function (data) {
                                return _permissions.delete;
                            },
                            action: function (data) {
                                deleteLanguage(data.record);
                            }
                        }]
                    }
                }
            ]
        });


        function setAsDefaultLanguage(language) {
            _languageService.setDefaultLanguage({
                name: language.name
            }).done(function () {
                getLanguages();
                abp.notify.success(app.localize('SuccessfullySaved'));
            });
        };

        async function deleteLanguage(language) {
            if (await abp.message.confirm(
                app.localize('LanguageDeleteWarningMessage', language.displayName)
            )) {
                _languageService.deleteLanguage({
                    id: language.id
                }).done(function () {
                    getLanguages();
                    abp.notify.success(app.localize('SuccessfullyDeleted'));
                });
            }
        };

        $('#CreateNewLanguageButton').click(function () {
            _createOrEditModal.open();
        });

        function getLanguages() {
            dataTable.ajax.reload();
        }

        abp.event.on('app.createOrEditLanguageModalSaved', function () {
            getLanguages();
        });

    });
})();
