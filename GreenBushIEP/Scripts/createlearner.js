$(document).ready(function () {

    $(".sdob").datepicker({
        dateFormat: "mm/dd/yy",
        changeMonth: true,
        changeYear: true
    });

    $(".chosen-select").chosen({
        disable_search_threshold: 10,
        no_results_text: "Oops, nothing found!",
        width: "100%"
    });

    $("#submitForm").on('click', function () {
        var numForms = document.forms.length;
        document.forms[numForms - 1].submit();
    });

    // attach event
    // fires when an avatar is uploaded
    $("#adminpersona").on('change', function (e) {
        var oFReader = new FileReader();
        oFReader.readAsDataURL(document.getElementById('adminpersona').files[0]);

        oFReader.onload = function (oFREvent) {
            document.getElementById("avatarImage").src = oFREvent.target.result;
        };
    });

    JSON.dateParser = function (key, value) {
        if (typeof value === 'string') {
            var a = reISO.exec(value);
            if (a)
                return new Date(value);
            a = reMsAjax.exec(value);
            if (a) {
                var b = a[1].split(/[-+,.]/);
                return new Date(b[0] ? +b[0] : 0 - +b[1]);
            }
        }
        return value;
    };

    $("#nokidsId").on('click', function () {
        if ($("#kidsid").attr("disabled") !== undefined) {
            $("#kidsid").removeAttr("disabled");
            $("#kidsid").val("");
        } else {
            $("#kidsid").attr("disabled", "disabled");
            $("#kidsid").val("0000000000");
        }
    });

    $("#submitEditLearner").on("click", function () {

        if ($("#misDistrict").val() === "" || $("#misDistrict").val() === null) {
            $("#misDistrict_chosen").addClass('contact-tooltip');

            return alert("Attending Location is required.");
        } else {
            $("#misDistrict_chosen").removeClass('contact-tooltip');
        }

        if (tabValidates()) {
            $(".ajax-loader").show();

            $.ajax({
                url: '/Manage/EditLEarner',
                type: 'POST',
                data: $("#editLearner").serialize(),
                success: function (data) {
                    if (data.Result === "success") {
                        window.location.href = "/ILP/Index";
                    } else {
                        alert(data.Message);
                    }
                },
                error: function (data) {
                    alert("There was an error when attempt to connect to the server.");
                },
                complete: function () {
                    $(".ajax-loader").hide();
                }
            });
        }
    });

    $("#submitLearner").on("click", function () {

        var theForm = document.getElementById("createNewStudent");

        if ($("#misDistrict").val() === "" || $("#misDistrict").val() === null) {
            $("#misDistrict_chosen").addClass('contact-tooltip');

            return alert("Attending Location is required.");
        } else {
            $("#misDistrict_chosen").removeClass('contact-tooltip');
        }

        if (tabValidates()) {
            $(".ajax-loader").show();

            $.ajax({
                url: '/Manage/CreateLearner',
                type: 'POST',
                data: $("#createNewStudent").serialize(),
                success: function (data) {
                    if (data.Result === "success") {
                        window.location.href = "/ILP/Index";
                    } else {
                        alert(data.Message);
                    }
                },
                error: function (data) {
                    alert("There was an error when attempt to connect to the server.");
                },
                complete: function () {
                    $(".ajax-loader").hide();
                }
            });
        }
    });
});

// once the page is fully loaded, hide the ajax loading icon.
document.addEventListener('readystatechange', event => {
    if (event.target.readyState === "complete") {
        $(".ajax-loader").hide();
    }
});

function tabValidates() {
    var validates = true;
    var $inputs = $("#step1 :input[data-validate='true']");

    $inputs.each(function () {
        var input = $(this);
        var is_valid = input.val();

        if (is_valid === "" || is_valid === null) {
            if (input.is("select")) {
                $(this).next().addClass('contact-tooltip');
                input.addClass('contact-tooltip');
            }
            else {
                input.addClass('contact-tooltip');
            }
            validates = false;
        }
        else {
            input.removeClass('contact-tooltip');
        }
    });

    //copy student name
    var firstName = $("#firstname").val();
    var middleName = $("#middlename").val();
    var lastname = $("#lastname").val();
    var studentName = "";

    if (middleName === "") {
        studentName = firstName + " " + lastname;
    }
    else {
        studentName = firstName + " " + middleName + " " + lastname;
    }

    $(".studentNameLabel").html(studentName);

    return validates;
}