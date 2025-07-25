(function ($) {
    app.modals.ImportMappingModal = function () {

        var _modalManager;
        var _$form;
        var _$importButton;
        var _$selectControls;
        var _importService = abp.services.app.importSchedule;
        var _fields = [];

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$importButton = _modalManager.getModal().find('.save-button');
            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _modalManager.getModal().find('.close-button').click(function (e) {
                e.preventDefault();
                var model = _$form.serializeFormToObject();
                abp.ajax({
                    url: $(this).data('delete-url'),
                    data: { blobName: model.BlobName },
                    contentType: 'application/x-www-form-urlencoded; charset=UTF-8'
                });
                _modalManager.close();
            });

            _$selectControls = _$form.find('select').each(function () {
                $(this).select2({ dropdownParent: $('#ImportMappingModal'), width: '100%' });
            });


            abp.ajax({
                url: _$form.data('getfields-url'),
                data: { importType: _$form.find('#ImportType').val() },
                contentType: 'application/x-www-form-urlencoded; charset=UTF-8'
            }).done(function (data) {
                _fields = data.fields1; // ignore fields with option group 
                populateSelectControlsWithData(data);

                _$selectControls.each(function () {
                    var ctrl = $(this);
                    selectValue(ctrl, fixField(ctrl.attr('data-field')));
                });
                validateRequiredFields();

                _$selectControls.on('select2:selecting', function (e) {
                    if ($(e.params.args.data.element).data('allowmulti')) {
                        return true;
                    }
                    var ctrlWithValue = getSelectWithValue($(this), e.params.args.data.id);
                    if (ctrlWithValue !== null && ctrlWithValue !== undefined) {
                        abp.message.warn("Cannot select element", 'This data element is already mapped to the "' + ctrlWithValue.data('field') + '" column in your spreadsheet. Please choose a different data element or remove the element from the other column.');
                        return false;
                    }
                });

                _$selectControls.on('select2:select', function () {
                    validateRequiredFields();
                });
            });

        };

        function selectValue($selectCtrl, idValue) {
            if (canSelectValue($selectCtrl, idValue)) {
                $selectCtrl.val(idValue).trigger('change');
            } else {
                // Try AllowMulti values without trailing numbers
                idValue = idValue.replace(/\d+$/, "").trim();
                if (canSelectValue($selectCtrl, idValue, true)) {
                    $selectCtrl.val(idValue).trigger('change');
                }
            }
        }

        function fixField(field) {
            field = field.toLowerCase().trim();
            if (field === "truck number") {
                field = "trucknumber";
            } else if (field === "fuel date time" || field === "fuel date") {
                field = "fueldatetime";
            } else if (field === "full rate") {
                field = "fullrate";
            }

            return field;
        }

        function canSelectValue($selectCtrl, idValue, onlyMulti) {
            var $option = $selectCtrl.find('option[value="' + idValue + '"]');
            if ($option.length == 0) {
                return false;
            } else if ($option.data('allowmulti')) {
                return true;
            } else if (onlyMulti !== true) {
                return getSelectWithValue($selectCtrl, idValue) === null;
            }
            return false;
        }

        function getSelectWithValue($selectCtrl, idValue) {
            if (!idValue) {
                return null;
            }
            var selectWithValue = null;
            _$selectControls.each(function () {
                var ctrl = $(this);
                if (ctrl === $selectCtrl) {
                    return true; // continue
                }
                if (ctrl.val() == idValue) {
                    selectWithValue = ctrl;
                    return false; // break
                }
            });
            return selectWithValue;
        }

        function populateSelectControlsWithData(data) {
            _$selectControls.each(function () {
                var ctrl = $(this);
                addOptions(ctrl, data.fields1);
                for (var i = 0; i < data.fields2.length; i++) {
                    var $optgroup = $('<optgroup>');
                    $optgroup.attr('label', data.fields2[i].text);
                    addOptions($optgroup, data.fields2[i].children);
                    ctrl.append($optgroup);
                }
            });
        }

        function addOptions($ctrl, itemArray) {
            for (var i = 0; i < itemArray.length; i++) {
                $ctrl.append($('<option>', { value: itemArray[i].id, text: itemArray[i].text, 'data-allowmulti': itemArray[i].allowMulti }));
            }
        }

        function validateRequiredFields() {
            var requiredFields = _fields.filter(function (field) {
                return field.isRequired;
            });
            _$selectControls.each(function () {
                if (requiredFields.length === 0) {
                    return false; // break
                }
                var selectedValue = $(this).val();
                var mapedFields = requiredFields.filter(function (field) {
                    return field.id === selectedValue
                        || field.requireOnlyOneOf !== null && field.requireOnlyOneOf.includes(selectedValue);
                });
                mapedFields.forEach(function (field) {
                    var index = requiredFields.indexOf(field);
                    requiredFields.splice(index, 1);
                });
            });
            _$importButton.prop('disabled', requiredFields.length > 0);
            var $div = _modalManager.getModal().find('div#RequiredFields');
            var $span = $div.find('span');
            $span.text(requiredFields.map(function (item) { return item.text; }).join(", "));
            if (requiredFields.length > 0) {
                $div.show();
            } else {
                $div.hide();
            }
        }

        this.save = function () {
            if (!_$form.valid()) {
                return;
            }
            var model = _$form.serializeFormToObject();
            var fieldMap = [];
            _$selectControls.each(function () {
                var ctrl = $(this);
                fieldMap.push({ UserField: ctrl.data('field'), StandardField: ctrl.val() });
            });
            model.FieldMap = fieldMap;
            _modalManager.setBusy(true);
            _importService.scheduleImport(model).done(function () {
                abp.notify.info("The file is scheduled for importing. You will receive a notification on completion.");
                _modalManager.close();
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };

    };

})(jQuery);
