(function () {
    'use strict';

    $(function () {

        var $form = $('form#CreateReportForm');

        $form.validate();

        $('#CreateReportPdf, #CreateReportCsv').click(function (e) {
            e.preventDefault();
            var reportService = abp.helper.reports.getReportService();
            var $clickedButton = $(this);
            var $otherButton;
            var create;
            var noDataMessage;
            if (this.id === "CreateReportCsv") {
                create = reportService.createCsv;
                $otherButton = $('#CreateReportPdf');
                noDataMessage = "There is no data to export.";
            } else {
                create = reportService.createPdf;
                $otherButton = $('#CreateReportCsv');
                noDataMessage = "There is no data to report.";
            }
            if ($form.valid()) {
                $clickedButton.buttonBusy(true);
                $otherButton.attr('disabled', 'disabled');

                var formData = $('form').serializeFormWithMultipleToObject();
                abp.helper.reports.executeFormDataHandler(formData);
                create(formData).done(function (data) {
                    if (data.fileName) {
                        app.downloadReportFile(data);
                    } else {
                        abp.message.info(noDataMessage);
                    }
                }).always(function () {
                    $clickedButton.buttonBusy(false);
                    $otherButton.removeAttr('disabled');
                });

            }
        });
    });

})();