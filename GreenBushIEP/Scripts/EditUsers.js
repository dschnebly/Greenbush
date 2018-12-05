$(document).ready(function () {

    //// attach event
    //// sets up drop down list of buildings a user is in.
    $('#buildingIds').hide();
    var link = document.createElement("a");
    link.href = "/Scripts/bootstrap-mutliselect.js";

    //// Loads the building scripts
    $.getScript(link.protocol + "//" + link.host + link.pathname + link.search + link.hash, function () {
        $('#buildingIds').hide();
    });

    //// attach event
    //// fires when the doucment is loaded, adds the districts to the drop down.
    $(".chosen-select").chosen({
        disable_search_threshold: 10,
        no_results_text: "Oops, nothing found!",
        width: "100%"
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

    //// attach event
    //// fires off a new password when the reset password button is clicked
    $('#myPassword button[type=submit]').on('click', function (e) {
        var userId = $("#hidden-userid").val();
        var myPassword = $("#password").val();

        if (/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*(\W|_)).{7,}$/.test(myPassword)) {
            // valid password
            $.ajax({
                type: 'POST',
                url: '/Account/ResetMyPassword',
                dataType: 'json',
                data: { id: userId, password: myPassword },
                async: false,
                success: function (data) {
                    if (data.Result === "Success") {
                        window.location.href = "/Home/Portal";
                    } else {
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
        } else {
            $("#invalidPassword").modal();
        }
    });

    //// attach event
    //// fires off a new password when the reset password button is clicked
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

    $("#UserForm").on("submit", function (e) {

        var action = $(this).closest("form").attr("action");
        var districtCount = $("li.search-choice").length;

        // must have a district selected.
        if (districtCount == 0) {
            $("#alertMessage .moreinfo").html("The user must be assigned to a district. Please choose a district.");
            $("#alertMessage").fadeTo(3000, 500).slideUp(500, function () {
                $("#alertMessage").slideUp(500);
            });

            return false;;
        }

        if ($("input.input-validation-error").length > 0) {
            return false;
        }

        $(".ajax-loader").css("visibility", "visible");

        return true;
    });
});

