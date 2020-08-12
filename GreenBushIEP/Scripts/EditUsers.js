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
    //// fires when the a user is editing themselves and the click update
    $('#updateMe').on('click', function (e) {
        var userId = $("#hidden-userid").val();
        var myPassword = $("#password").val();

        if (!/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*(\W|_)).{10,}$/.test(myPassword)) {
            $("#invalidPassword").modal();
        } else {
            $('#UserForm').submit();
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

    //// attach event
    //// fires off a new password when the reset password button is clicked
    $('#confirmNewPassword button[type=submit]').on('click', function (e) {
        var userId = $("#hidden-userid").val();
        var newPassword = $("input[type='password']").eq(1).val();

        $.ajax({
            type: 'POST',
            url: '/Account/ResetMyPassword',
            dataType: 'json',
            data: { id: userId, password: newPassword, sendEmail: false },
            async: false,
            success: function (data) {
                if (data.Result === "Success") {
                    $("#alertMessage .moreinfo").html('You have successfully reset the user\'s password');
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


    // attach event
    // fires when the user chooses a district
    $(".chosen-select").chosen().change(function () {

        // must have a district selected.
        var item = $(this).val();
        if (item.length <= 0) {
            $("#alertMessage .moreinfo").html("The user must be assigned to a district. Please choose a district.");
            $("#alertMessage").fadeTo(3000, 500).slideUp(500, function () {
                $("#alertMessage").slideUp(500);
            });

            $('#AttendanceBuildingId').find('option').remove().end();
            $('#AttendanceBuildingId').multiselect('rebuild');
            $("button[type='submit']").prop('disabled', true);
        }
        else {
            // add our buildings.

            $(".ajax-loader").show();

            var selectedDistricts = $(this).val() + "";
            $.ajax({
                type: "GET",
                url: "/Manage/GetAllBuilingsByDistrictIds",
                dataType: "json",
                data: {
                    districtIds: selectedDistricts,
                },
                async: false,
                success: function (data) {
                    if (data.Result === "success") {

                        // get the current selected building Ids
                        var building = [];
                        var $el = $("#buildingIds");
                        $el.find('option:selected').each(function () {
                            building.push({ value: $(this).val(), text: $(this).text() });
                        });

                        // clear the select
                        var responsibleBuildingElement = $('#buildingIds');
                        var listOfValues = $("#buildingIds option:selected").val();
                        $("#buildingIds").find("option").remove().end();

                        // add the new options to the select
                        var responsibleBuilding = responsibleBuildingElement.html();
                        $.each(data.DistrictBuildings, function (key, value) {
                            var checked = building.find(b => b.value === value.BuildingID) !== undefined;
                            var showChecked = checked ? "selected='selected'" : '';
                            responsibleBuilding += "<option value='" + value.BuildingID + "' data-icon='glyphicon-home' " + showChecked + ">" + value.BuildingName + "</option>";
                        });

                        // trigger chosen select to update.
                        responsibleBuildingElement.html(responsibleBuilding);
                        $('#buildingIds').multiselect('rebuild');

                        // enable the submit button
                        $("button[type='submit']").prop('disabled', false);
                    } else {
                        alert("Oops, something happened on the server side. Please contact our organization.");
                    }
                },
                error: function (data) {
                    alert("ERROR!!!");

                    console.log(data);
                },
                complete: function (data) {
                    $(".ajax-loader").hide();
                }
            });
        }
    });

    $("#UserForm").on("submit", function (e) {

        $(".ajax-loader").css("visibility", "visible");

        return true;
    });

    $(".chosen-select").trigger("chosen:updated").change();
});