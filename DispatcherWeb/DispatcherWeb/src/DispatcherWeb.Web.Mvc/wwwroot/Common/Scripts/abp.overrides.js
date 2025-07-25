(function () {
    abp ??= {};
    abp.multiTenancy ??= {};
    abp.multiTenancy.setTenantIdCookie = function (tenantId) {
        if (tenantId) {
            abp.utils.setCookieValue(
                abp.multiTenancy.tenantIdCookieName,
                tenantId.toString(),
                new Date(new Date().getTime() + 5 * 365 * 86400000), //5 years
                abp.appPath,
                abp.domain,
                {
                    samesite: 'None',
                    secure: true,
                },
            );
        } else {
            //added `domain` to correctly delete a cookie that was created with a domain
            abp.utils.deleteCookie(
                abp.multiTenancy.tenantIdCookieName,
                abp.appPath,
                abp.domain,
                {
                    samesite: 'None',
                    secure: true,
                },
            );
        }
    };

    abp.utils = abp.utils || {};
    abp.utils.deleteCookie = function (key, path, domain, attributes) {
        var cookieValue = encodeURIComponent(key) + '=';

        cookieValue = cookieValue + "; expires=" + (new Date(new Date().getTime() - 86400000)).toUTCString();

        if (path) {
            cookieValue = cookieValue + "; path=" + path;
        }

        //added `domain` to correctly delete a cookie that was created with a domain
        if (domain) {
            cookieValue = cookieValue + "; domain=" + domain;
        }

        for (var name in attributes) {
            if (!attributes[name]) {
                continue;
            }

            cookieValue += '; ' + name;
            if (attributes[name] === true) {
                continue;
            }

            cookieValue += '=' + attributes[name].split(';')[0];
        }

        document.cookie = cookieValue;
    };

    abp.log.log = function (logObject, logLevel) {
        if (!window.console || !window.console.log) {
            return;
        }

        if (logLevel != undefined && logLevel < abp.log.level) {
            return;
        }

        switch (logLevel) {
            case abp.log.levels.DEBUG:
                console.debug(logObject);
                break;
            case abp.log.levels.INFO:
                console.info(logObject);
                break;
            case abp.log.levels.WARN:
                console.warn(logObject);
                break;
            case abp.log.levels.ERROR:
            case abp.log.levels.FATAL:
                console.error(logObject);
                break;
            default:
                console.log(logObject);
                break;
        }
    };
})();
