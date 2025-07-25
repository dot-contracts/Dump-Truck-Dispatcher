var app = app || {};
(function () {
    app.consts ??= {};
    $.extend(true, app.consts, {
        grid: {
            defaultPageSize: 10,
            defaultPageSizes: [10, 20, 50, 100],
        },
    });

    abp.helperConfiguration ??= {};
    abp.helperConfiguration.getCurrentLanguageLocale = function () {
        return abp.localization.currentLanguage.name
    };

    abp.helperConfiguration.getIanaTimezoneId = function () {
        return abp.timing.timeZoneInfo.iana.timeZoneId;
    };

    abp.helperConfiguration.getDefaultCurrencySymbol = function () {
        return abp.setting.get('App.General.CurrencySymbol');
    };

    abp.helperConfiguration.dataTables ??= {};
    abp.helperConfiguration.dataTables.beforeInit ??= [];
    abp.helperConfiguration.dataTables.afterInit ??= [];

    if (abp.helperConfiguration.dataTables.defaultOptions) {
        abp.helperConfiguration.dataTables.defaultOptions.dom =
            `<'row bottom'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6 dataTables_pager'p>>
             <'row'<'col-sm-12'tr>>
             <'row bottom'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7 dataTables_pager'p>>`;

        abp.helperConfiguration.dataTables.defaultOptions.lengthMenu = app.consts.grid.defaultPageSizes;
        abp.helperConfiguration.dataTables.defaultOptions.pageLength = app.consts.grid.defaultPageSize;
    }
})();
