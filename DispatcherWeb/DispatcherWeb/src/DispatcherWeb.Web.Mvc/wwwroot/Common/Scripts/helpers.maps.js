(function () {

    abp.maps ??= {};
    abp.maps.waitForGoogleMaps = function () {
        return new Promise((resolve, reject) => {
            var isAvailable = () => typeof (window.googleMapsAreAvailable) !== 'undefined' && window.googleMapsAreAvailable;
            if (isAvailable()) {
                resolve();
                return;
            }
            if (typeof (window.googleMapsApiKeyIsMissing) !== 'undefined') {
                let message = app.localize('GoogleMapsApiKeyIsMissing');
                console.error(message);
                //abp.message.error(message);
                reject(message);
                return;
            }
            var resolveWhenAvailable = function () {
                setTimeout(function () {
                    if (isAvailable()) {
                        resolve();
                    } else {
                        resolveWhenAvailable();
                    }
                }, 300);
            };
            resolveWhenAvailable();
        });
    };

    abp.maps.areGoogleMapsAvailable = async function () {
        try {
            await abp.maps.waitForGoogleMaps();
            return true;
        } catch {
            return false;
        }
    };



    $('body').on('click', '.map-help .map-help-collapse', function () {
        let body = $(this).closest('.map-help').find('.map-help-body');
        body.toggleClass('d-none');
        isCollapsed = body.hasClass('d-none');
        $(this).find('i').removeClass('fa-angle-double-up fa-angle-double-down').addClass('fa-angle-double-' + (isCollapsed ? 'down' : 'up'));
    });
    $('body').on('click', '.map-help .map-help-close', function () {
        $(this).closest('.map-help').addClass('d-none');
    });
})();
