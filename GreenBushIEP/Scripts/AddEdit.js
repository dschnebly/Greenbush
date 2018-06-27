$(document).ready(function () {

    $(".chosen-select").chosen({
        disable_search_threshold: 10,
        no_results_text: "Oops, nothing found!",
        width: "100%"
    });

    $('#buildingIds').hide();
    var link = document.createElement("a");
    link.href = "/Scripts/bootstrap-mutliselect.js";

    // loads the building script
    $.getScript(link.protocol + "//" + link.host + link.pathname + link.search + link.hash, function () {
        $('#buildingIds').show();
    });

    $('#confirmPassword button[type=submit]').on('click', function (e) {
        var userId = $("#hidden-userid").val();

        $.ajax({
            type: 'POST',
            url: '/Account/ResetPassword',
            dataType: 'json',
            data: { id: userId },
            async: false,
            success: function (data) {
                if (data.Result === "Success") {
                    $("#alertMessage .moreinfo").html('An email was sent to the user with their new password.');
                    $("#alertMessage").fadeTo(3000, 500).slideUp(500, function () {
                        $("#alertMessage").slideUp(500);
                    });
                }
                else {
                    $("#alertMessage .moreinfo").html(data.Message);
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
            }
        });
    });

    //// attach event
    //// fires when an avatar is uploaded
    $("#adminpersona").on('change', function (e) {
        var oFReader = new FileReader();
        oFReader.readAsDataURL(document.getElementById('adminpersona').files[0]);

        oFReader.onload = function (oFREvent) {
            document.getElementById("avatarImage").src = oFREvent.target.result;
        };
    });

    $("#ActionButton").on('click', function (e) {
        e.preventDefault();

        var action = $(this).closest(form).arrt('action');
        $.ajax({
            url: action,
            type: 'POST',
            data: $("#UserForm").serialize(),
            success: function (data) {
                if (data.Result === "success") {
                    alert("The user was successfully created.");
                } else {

                    $("#alertMessage .moreinfo").html(data.Message);
                    $("#alertMessage").fadeTo(3000, 500).slideUp(500, function () {
                        $("#alertMessage").slideUp(500);
                    });

                }
            },
            error: function (data) {
                alert("There was an error when attempt to connect to the server.");
            }
        });
        return false;
    });


});

