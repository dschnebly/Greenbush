$(document).ready(function ()
{
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
            $("#alertMessage .moreinfo").html("The user must be assigned to a district. Please choose a district.");
            $("#alertMessage").fadeTo(3000, 500).slideUp(500, function () {
                $("#alertMessage").slideUp(500);
            });

            return false;
        }

        // if there are errors, don't allow them to submit.
        if ($("input.input-validation-error").length > 0) {
            alert("errors");
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
                    window.location.href = "/Home/Portal";
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

        // must have a district selected.
        var item = $(this).val();
        if (item.length <= 0)
        {
            $("#alertMessage .moreinfo").html("The user must be assigned to a district. Please choose a district.");
            $("#alertMessage").fadeTo(3000, 500).slideUp(500, function () {
                $("#alertMessage").slideUp(500);
            });

            $('#AttendanceBuildingId').find('option').remove().end();
            $('#AttendanceBuildingId').multiselect('rebuild');
            $("button[type='submit']").prop('disabled', true);
        }
        else
        {
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

                        // clear the select
                        var responsibleBuildingElement = $('#AttendanceBuildingId');
                        $('#AttendanceBuildingId').find('option').remove().end();

                        // add the new options to the select
                        var responsibleBuilding = responsibleBuildingElement.html();
                        $.each(data.DistrictBuildings, function (key, value) {
                            responsibleBuilding += "<option value='" + value.BuildingID + "'>" + value.BuildingName + "</option>";
                        });

                        // trigger chosen select to update.
                        responsibleBuildingElement.html(responsibleBuilding);
                        $('#AttendanceBuildingId').multiselect('rebuild');

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
});

