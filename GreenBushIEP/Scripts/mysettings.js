$(function () {

    $(".module-section").on("click", function (e) {
        var mis = $("#misId").val();
        var ModuleView = $(e.currentTarget).data("view");

        $('.ajax-loader').css("visibility", "visible");

        $.ajax({
            type: 'GET',
            url: '/Home/LoadMISSection',
            data: { view: ModuleView },
            dataType: 'html',
            success: function (data) {
                if (data.length !== 0) {
                    $("#module-form-section").html(data);
                    $('#moduleSection').modal('show');
                }
                else {
                    $("#alertMessage .moreinfo").html('Server Error');
                    $("#alertMessage").fadeTo(3000, 500).slideUp(500, function () {
                        $("#alertMessage").slideUp(500);
                    });
                }
            },
            error: function (data) {
                $("#alertMessage .moreinfo").html('Unable to connect to the server or other related problem. Please contact your admin.');
                $("#alertMessage").fadeTo(3000, 500).slideUp(500, function () {
                    $("#alertMessage").slideUp(500);
                });
            },
            complete: function () {
                $('.ajax-loader').css("visibility", "hidden");
            }
        });
    });

});

/** fixing the bootstrap modal overlay bug **/
$(window).on('shown.bs.modal', function (e) {
    var moduleId = e.target.id;
    var modals = $(".modal").get(), element = null;
	$(document).off('focusin.modal');
    for (var i = 0, length = modals.length; i < length; i++) {
        $(modals[i]).css("height", "0");
    }

    $("#" + moduleId).css("height", "auto");
});