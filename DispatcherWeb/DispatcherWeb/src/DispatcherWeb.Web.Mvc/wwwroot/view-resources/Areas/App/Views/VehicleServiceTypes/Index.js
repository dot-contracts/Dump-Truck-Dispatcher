(function () {
    'use strict';

    var _vehicleServiceTypeService = abp.services.app.vehicleServiceType;
    var _$editRow = null;
    var _$form = $('form');

    _$form.validate();

    _$form.on('submit', function (e) {
        e.preventDefault();

        if (!_$form.valid()) {
            _$form.showValidateMessage();
            return;
        }

        var serviceTypeName = $('#Name').val();
        var serviceTypeId = $('#Id').val();
        $('#AddButton').buttonBusy(true);
        _vehicleServiceTypeService.save({ id: serviceTypeId, name: serviceTypeName }).done(function (serviceType) {
            if (_$editRow === null) {
                var template = $('#serviceTypeTemplate').html();
                Mustache.parse(template);
                var htmlRow = Mustache.render(template, serviceType);
                var $tableBody = $('#ServiceTypesTable tbody');
                $tableBody.append(htmlRow);
            } else {
                _$editRow.find('td:first').text(serviceTypeName);
            }
            resetControls();
        }).always(function () {
            $('#AddButton').buttonBusy(false);
        });

    });

    $('#CancelButton').click(function (e) {
        resetControls();
    });

    function resetControls() {
        _$editRow = null;
        $('#Id').val('0');
        $('#Name').val('');
        updateControls();
    }

    $('#ServiceTypesTable').on('click', '.btnDelete', function (e) {
        e.preventDefault();

        var $tr = $(this).closest('tr');
        var serviceTypeId = $tr.data('id');
        _vehicleServiceTypeService.delete(serviceTypeId).done(function (result) {
            if (result) {
                $tr.remove();
            }
        });
    });

    $('#ServiceTypesTable').on('click', '.btnEdit', function (e) {
        e.preventDefault();

        _$editRow = $(this).closest('tr');
        var $td = _$editRow.find('td:first');
        $('#Name').val($td.text());
        $('#Id').val(_$editRow.data('id'));
        updateControls();
    });

    function updateControls() {
        var edit = _$editRow !== null;
        if (edit) {
            $('#AddButton').html('Save');
            $('#CancelButton').show();
        } else {
            $('#AddButton').html('Add');
            $('#CancelButton').hide();
        }
        $('#ServiceTypesTable').find('.btnDelete, .btnEdit').prop('disabled', edit);
        $('#Name').focus();
    }

})();