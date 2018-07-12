EBRD.PYS.Loader = {
    show: function () {
        App.blockUI({
            boxed: true
        });
    },
    hide: function () {
        App.unblockUI();
    },
    showInPopup: function () {

        App.blockUI({
            boxed: true
        });
        $('.blockUI.blockMsg.blockPage').css('opacity', 1);
        $('.blockUI.blockMsg.blockPage').css('z-index', 111111);
    }
};