var app = app || {};
(function () {
    //the session is available right away, this event trigger is left for backwards compatibility in case it is being used by existing ABP code
    //on the second thought, we'll comment it out as well, and can add it back if we find any issues running without it
    //if (abp && abp.event) {
    //    abp.event.trigger('app.appSessionLoaded');
    //}
})();
