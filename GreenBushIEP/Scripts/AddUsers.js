$(document).ready(function () {

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

    $("#ActionButton").on('click', function (e) {
        e.preventDefault();
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

        $.ajax({
            url: action,
            type: 'POST',
            data: $("#UserForm").serialize(),
            success: function (data) {
                if (data.Result === "success") {
                    alert("The user was successfully created.");
                }
                else
                {
                    $("#alertMessage .moreinfo").html(data.Message);
                    $("#alertMessage").fadeTo(3000, 500).slideUp(500, function () {
                        $("#alertMessage").slideUp(500);
                    });
                }
            },
            error: function (data)
            {
                $("#alertMessage .moreinfo").html("There was an error when attempt to connect to the server.");
                $("#alertMessage").fadeTo(3000, 500).slideUp(500, function () {
                    $("#alertMessage").slideUp(500);
                });
            }
        });

        return false;
    });
});

