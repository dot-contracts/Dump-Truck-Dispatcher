(function () {
    $(function () {
        'use strict';

        var _workOrderService = abp.services.app.workOrder;
        var _dtHelper = abp.helper.dataTables;

        var _createOrEditWorkOrderLineModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/WorkOrders/CreateOrEditWorkOrderLineModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/WorkOrders/_CreateOrEditWorkOrderLineModal.js',
            modalClass: 'CreateOrEditWorkOrderLineModal'
        });

        $('form#WorkOrderForm').validate();

        $("#IssueDate").datepickerInit();
        $("#StartDate").datepickerInit();
        $("#CompletionDate").datepickerInit();

        var workOrderLinesTable = $('#WorkOrderLinesTable');
        var workOrderLinesGrid = workOrderLinesTable.DataTableInit({
            paging: false,
            info: false,
            ordering: false,
            language: {
                emptyTable: app.localize("NoDataInTheTableClick{0}ToAdd", app.localize("AddWorkOrderItem"))
            },
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                _workOrderService.getWorkOrderLines(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                });
            },
            order: [[1, 'asc']],
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
                    data: "vehicleServiceName",
                    title: "Service"
                },
                {
                    data: "note",
                    title: "Note"
                },
                {
                    data: "laborTime",
                    title: "Labor time"
                },
                {
                    data: "laborRate",
                    title: "Labor rate"
                },
                {
                    data: "laborCost",
                    title: "Labor cost"
                },
                {
                    data: "partsCost",
                    title: "Parts cost"
                },
                {
                    data: null,
                    orderable: false,
                    name: "Actions",
                    title: " ",
                    width: "175px",
                    className: "actions",
                    responsivePriority: 1,
                    defaultContent: '<div class="dropdown">'
                        + '<ul class="dropdown-menu">'
                        + '<li><a class="btnEditRow" title="Edit"><i class="fa fa-edit"></i> Edit</a></li>'
                        + '<li><a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>'
                        + '</ul>'
                        + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                        + '</div>'
                }
            ]
        });

        var reloadMainGrid = function () {
            workOrderLinesGrid.ajax.reload();
        };

        abp.event.on('app.createOrEditWorkOrderLineModalSaved', function (result) {
            reloadMainGrid();
            updateTotals(result);
        });

        function updateTotals(result) {
            if ($("#IsTotalLaborCostOverridden").val() !== 'True') {
                $("#TotalLaborCost").val(result.totalLaborCost);
            }
            if ($("#IsTotalPartsCostOverridden").val() !== 'True') {
                $("#TotalPartsCost").val(result.totalPartsCost);
            }
            $("#TotalCost").val(result.totalCost);
        }

        function recalculateTotals() {
            let totalLaborCost = round($("#TotalLaborCost").val()) || 0.00;
            let totalPartsCost = round($("#TotalPartsCost").val()) || 0.00;
            let taxPercent = round($("#Tax").val()) || 0.00;
            let discountPercent = round($("#Discount").val()) || 0.00;
            let subTotal = totalLaborCost + totalPartsCost;

            let taxAmount = round(subTotal * taxPercent / 100) || 0.00;
            let discountAmount = round((subTotal + taxAmount) * discountPercent / 100) || 0.00;
            let totalCost = subTotal + taxAmount - discountAmount;

            $("#TotalCost").val(totalCost.toFixed(2));
        }

        function round(num) {
            return abp.utils.round(num);
        }

        function roundTextboxValueIfNumeric(field) {
            var value = round($(field).val());
            if (value !== null) {
                $(field).val(value.toFixed(2));
            }
        }

        $('#TotalLaborCost').change(function () {
            $('#IsTotalLaborCostOverridden').val('True');
        });

        $('#TotalPartsCost').change(function () {
            $('#IsTotalPartsCostOverridden').val('True');
        });

        $('#TotalLaborCost, #TotalPartsCost, #Tax, #Discount').change(function () {
            recalculateTotals();
        });

        $("#CreateNewWorkOrderLineButton").click(function (e) {
            e.preventDefault();
            saveWorkOrder(function () {
                _createOrEditWorkOrderLineModal.open({ workOrderId: getWorkOrderId() });
            });
        });

        function getWorkOrderId() {
            return $('#Id').val();
        }

        $('#TruckId').select2Init({
            abpServiceMethod: abp.services.app.truck.getTrucksSelectList,
            abpServiceParams: {
                allOffices: true,
                inServiceOnly: true
            },
            showAll: false,
            allowClear: true
        });
        $('#AssignedToId').select2Init({
            abpServiceMethod: abp.services.app.user.getMaintenanceUsersSelectList,
            showAll: false,
            allowClear: true
        });
        $('#VehicleServiceTypeId').select2Init({
            abpServiceMethod: abp.services.app.vehicleServiceType.getSelectList,
            showAll: true,
            allowClear: false
        });
        $('#Status').select2Init({
            showAll: true,
            allowClear: false
        });

        $('#Odometer').rules('add', { mileage: true });

        workOrderLinesTable.on('click', '.btnEditRow', function () {
            var workOrderLineId = _dtHelper.getRowData(this).id;
            saveWorkOrder(function () {
                _createOrEditWorkOrderLineModal.open({
                    id: workOrderLineId,
                    workOrderId: getWorkOrderId()
                });
            });
        });

        workOrderLinesTable.on('click', '.btnDeleteRow', async function () {
            var workOrderLineId = _dtHelper.getRowData(this).id;
            if (await abp.message.confirm(
                'Are you sure you want to delete the line?'
            )) {
                saveWorkOrder(function () {
                    _workOrderService.deleteWorkOrderLine({
                        id: workOrderLineId
                    }).done(function (result) {
                        abp.notify.info('Successfully deleted.');
                        reloadMainGrid();
                        updateTotals(result);
                    });
                });
            }
        });

        $('#SaveWorkOrderButton').click(function (e) {
            e.preventDefault();
            saveWorkOrder();
        });

        function saveWorkOrder(callback, clearBusy = true) {
            var form = $("#WorkOrderForm");
            if (!form.valid()) {
                form.showValidateMessage();
                return;
            }
            if (!$('#IssueDate, #StartDate, #CompletionDate').validateDatePickersIsNotInFuture()) {
                return;
            }
            if (!abp.helper.validateStartEndDates(
                { value: $('#IssueDate').val(), title: $('label[for="IssueDate"]').text() },
                { value: $('#StartDate').val(), title: $('label[for="StartDate"]').text() },
                { value: $('#CompletionDate').val(), title: $('label[for="CompletionDate"]').text() }
            )) {
                return;
            }

            var workOrder = form.serializeFormToObject();

            abp.ui.setBusy(form);
            _workOrderService.saveWorkOrder(workOrder).done(function (data) {
                abp.notify.info('Saved successfully.');
                $("#Id").val(data.id);
                if (data.completionDate) {
                    $('#CompletionDate').val(_dtHelper.renderUtcDate(data.completionDate)).change();
                } else {
                    $('#CompletionDate').val('');
                }
                history.replaceState({}, "", abp.appPath + 'app/workorders/details/' + data.id);
                if (callback) {
                    callback(data.id);
                }
                showEditingBlocks();
            }).always(function () {
                if (clearBusy) {
                    abp.ui.clearBusy(form);
                }
            });
        }
        function showEditingBlocks() {
            $('.editing-only-block').not(":visible").slideDown();
        }

        $('#Picture, #CameraPicture').click(function (e) {
            if (app.showWarningIfFreeVersion()) {
                e.preventDefault();
                return;
            }
        });

        $('#Picture, #CameraPicture').fileupload({
            add: function (e, data) {
                var allowedExtensions = $(this).data('allowed-extensions').split(',')
                    .map(function (item) { return item.trim(); });
                var workOrderId = $('#Id').val();
                if (workOrderId && workOrderId != 0) {
                    addFile(data, workOrderId, allowedExtensions);
                } else {
                    saveWorkOrder(function (id) {
                        addFile(data, id, allowedExtensions);
                    }, false);
                }
            },
            submit: function (e, data) {
                abp.ui.setBusy('form');
                $('#Uploading').show();
            },
            done: function (e, data) {
                var id = data.result.result.id;
                $.get(abp.appPath + 'app/WorkOrders/GetPictureRow?id=' + id,
                    function (htmlRow) {
                        var $tableBody = $('#PicturesTable tbody');
                        $tableBody.append(htmlRow);
                    })
                    .always(function () {
                        abp.ui.clearBusy('form');
                        $('#Uploading').hide();
                    });

            },
            fail: function () {
                abp.ui.clearBusy('form');
            }
        });
        function addFile(data, workOrderId, allowedExtensions) {
            if (data.files.length > 0) {
                var fileName = data.files[0].name;
                var fileExt = fileName.split('.').pop().toLowerCase();
                if (!allowedExtensions.includes(fileExt)) {
                    abp.message.warn('The only types of files that can be uploaded are ' + allowedExtensions.join(", "), fileName + " is not of the appropriate type.");
                    abp.ui.clearBusy('form');
                    return;
                }
            }
            data.formData = { 'id': workOrderId };
            data.submit();
        }

        $('#PicturesTable').on('click', 'button', function (e) {
            e.preventDefault();
            var $button = $(this);
            var id = $button.data('id');
            abp.ui.setBusy($button);
            _workOrderService.deletePicture({ id: id })
                .done(function () {
                    $button.closest('tr').remove();
                })
                .always(function () {
                    abp.ui.clearBusy($button);
                }
                );
        });

        $('#Status').change(function () {
            $("#StartDate").removeAttr('required');
            $("#CompletionDate").removeAttr('required');
            $("#lblStartDate").removeClass('required-label');
            $("#lblCompletionDate").removeClass('required-label');
            if (Number($("#Status").val()) === abp.enums.workOrderStatus.complete) {

                $("#StartDate").prop('required', true);
                $("#lblStartDate").addClass('required-label');
                $("#CompletionDate").prop('required', true);
                $("#lblCompletionDate").addClass('required-label');

                if ($("#StartDate").val() == '') {
                    $("#StartDate").val(moment().format("MM/DD/YYYY"));
                }
                if ($("#CompletionDate").val() == '') {
                    $("#CompletionDate").val(moment().format("MM/DD/YYYY"));
                }
            } else {
                $("#CompletionDate").val('');
            }
        });

        $('#CompletionDate').blur(function () {
            if ($('#CompletionDate').val()) {
                if (Number($("#Status").val()) !== abp.enums.workOrderStatus.complete) {
                    $("#Status").val(abp.enums.workOrderStatus.complete).change();
                }
            }
        });

        $("#PrintWorkOrderButton").click(function (e) {
            e.preventDefault();
            var workOrderId = getWorkOrderId();
            app.openPopup(`${abp.reportCenterPath}report/VehicleMaintenanceWorkOrderReport/${workOrderId}/pdf`);
        });

    });
})();
