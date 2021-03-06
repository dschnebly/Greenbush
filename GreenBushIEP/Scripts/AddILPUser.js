﻿$(document).ready(function () {

    // attach event
    // fires when an avatar is uploaded
    $("#adminpersona").on('change', function (e) {
        var oFReader = new FileReader();
        oFReader.readAsDataURL(document.getElementById('adminpersona').files[0]);

        oFReader.onload = function (oFREvent) {
            document.getElementById("avatarImage").src = oFREvent.target.result;
        };
    });

    // attach event
    // fires when the user sumbits the form
    $("#UserForm").on("submit", function (e) {

        e.preventDefault();
        var action = $(this).closest("form").attr("action");
        var districtCount = $("li.search-choice").length;

        // must have a district selected.
        if (districtCount == 0) {
            $("#alertMessage .moreinfo").html("The user must be assigned to a location. Please choose a location.");
            $("#alertMessage").fadeTo(3000, 500).slideUp(500, function () {
                $("#alertMessage").slideUp(500);
            });

            return false;
        }

        // if there are errors, don't allow them to submit.
        if ($("input.input-validation-error").length > 0) {
            console.log($("input.input-validation-error").first());
            alert("There are errors on the form. Please correct them and resubmit.");
            return false;
        }

        $(".ajax-loader").css("visibility", "visible");

        $.ajax({
            url: action,
            type: 'POST',
            data: $("#UserForm").serialize(),
            success: function (data) {
                $(".ajax-loader").css("visibility", "hidden");

                if (data.Result === "error") {
                    $("#alertMessage .moreinfo").html(data.Message);
                    $("#alertMessage").fadeTo(3000, 500).slideUp(500, function () {
                        $("#alertMessage").slideUp(500);
                    });
                } else {
                    window.location.href = "/ILP/Index";
                }
            },
            error: function (data) {
                $("#alertMessage .moreinfo").html("There was an error when attempt to connect to the server.");
                $("#alertMessage").fadeTo(3000, 500).slideUp(500, function () {
                    $("#alertMessage").slideUp(500);
                });
            }
        });

        return true;
    });

    // attach event
    // fires when the user chooses a district
    $(".chosen-select").chosen().change(function () {
        var item = $(this).val();
        if (item.length <= 0) {
            $("button[type='submit']").prop('disabled', true);

            $("#alertMessage .moreinfo").html("The user must be assigned to a location. Please choose a location.");
            $("#alertMessage").fadeTo(3000, 500).slideUp(500, function () {
                $("#alertMessage").slideUp(500);
            });
        } else {
            $("button[type='submit']").prop('disabled', false);
        }
    });
});
