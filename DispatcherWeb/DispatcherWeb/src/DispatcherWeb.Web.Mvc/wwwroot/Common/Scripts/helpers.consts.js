var app = app || {};
(function () {

    app.consts ??= {};
    $.extend(true, app.consts, {
        maxProfilPictureBytesUserFriendlyValue: 5,
        userManagement: {
            defaultAdminUserName: 'admin',
        },
        contentTypes: {
            formUrlencoded: 'application/x-www-form-urlencoded; charset=UTF-8',
        },
        friendshipState: {
            accepted: 1,
            blocked: 2
        },
        maxQuantity: 1000000,
        maxDecimal: 999999999999999,
    });

    app.colors = {
        danger: '#dd4b39',
        warning: '#f39c12',
        success: '#00a65a',
        unavailable: '#999999',
        freight: '#00008b',
        material: '#4169e1',
        fuel: '#6aa0ca',
        tripToLoadSite: '#a4eaad',
        tripToDeliverySite: '#8fb2e0',
    };

    app.maxLogoSize = 30720; //30 KB
    app.maxReportLogoSize = 307200; //300 KB
    app.allowedLogoTypes = ['jpg', 'jpeg', 'png', 'gif'];
    app.allowedReportLogoTypes = ['jpg', 'jpeg', 'png', 'gif'];


    abp.helper ??= {};

    abp.helper.maxFileSizes = {
        ticketPhoto: 8388608, // 8 MB
        insuranceDocument: 5242880, // 5 MB
    };

    abp.helper.fileTypes = {
        image: 'image/jpeg,image/gif,image/png',
        pdf: 'application/pdf',
    };
    abp.helper.fileTypes.imageOrPdf = [
        abp.helper.fileTypes.image,
        abp.helper.fileTypes.pdf,
    ].join(',');


})();
