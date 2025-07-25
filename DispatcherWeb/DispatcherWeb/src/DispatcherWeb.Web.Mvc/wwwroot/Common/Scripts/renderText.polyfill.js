//abp.toastr.js from the same folder depends on abp.helper.dataTables.renderText(message), which sometimes is not loaded depending on a specific bundle.
//It's included in datatables-helper package, but that is dependent on datatables.
//To avoid adding more packages and increasing the size where it's not needed, we added the polyfill for the below function here instead. It's a bit of code duplication, but it reduces the size of
//some bundles significantly

abp = abp || {};
abp.helper = abp.helper || {};
abp.helper.dataTables = abp.helper.dataTables || {};
abp.helper.dataTables.renderText = function renderText(d) {
    //code is taken from DataTable's __htmlEscapeEntities
    return typeof d === 'string' ?
        d.replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#39;') :
        d;
};