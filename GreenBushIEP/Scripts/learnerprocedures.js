$(function () {

    // attach event
    // fires when user clicks a button
    $(".module-section").on("click", function (e) {
        var stId = $("#studentIEPId").val();
        var ilpId = $("#ILPid").val();
        var ModuleView = $(e.currentTarget).data("view");

        $('.ajax-loader').css("visibility", "visible");

        $.ajax({
            type: 'GET',
            url: '/ILP/LoadModuleSection',
            data: {
                studentId: stId, //stId,
                ilpId: ilpId,
                view: ModuleView
            },
            dataType: 'html',
            success: function (data) {
                if (data.length !== 0) {
                    $("#module-form-section").html(data);
                    $('#moduleSection').modal('show');
                } else {
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