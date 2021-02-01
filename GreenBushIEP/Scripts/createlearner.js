$(document).ready(function () {

    init();

    // Custom jQuery to hide popover on click of the close button
    $(document).on("click", ".popover-header .popover-close", function () {
        $(this).parents(".popover").popover('hide');
    });

    $('[data-toggle="popover"]').popover({
        html: true,
        template: '<div class="popover"><div class="arrow"></div><h3 class="popover-header">Edit Dates <i class="glyphicon glyphicon-remove-circle pull-right popover-close"></i></h3><div class="popover-content"></div></div>'
    });

    $(".toggle-reEvaluationPopover").click(function () {

        //studentid, will have to get the iep
        var student = $("#studentId").val();
        var folder = $(this);

        //ajax call to get the past dates
        $.ajax({
            type: 'GET',
            url: '/Manage/GetPastSignedReEvaluationDates',
            data: { studentId: student },
            dataType: "json",
            async: false,
            success: function (data) {
                if (data.Result === "success") {
                    if (data.Dates && data.Dates.length > 0) {
                        var dataContentString = "<ul class='popupcontainer'>";
                        $.each(data.Dates, function (key, value) {
                            dataContentString += "<li data-signedId='" + value.archiveEvaluationDateSignedID + "'><input type='date' value='" + new Date(parseInt(value.evaluationDateSigned.replace('/Date(', ''))).toISOString().split('T')[0] + "'><a href='#' class='btn-sm btn-success' onclick='editSignedDate(" + value.archiveEvaluationDateSignedID + ")'><i class='glyphicon glyphicon-ok'></i></a><a href='#' class='btn-sm btn-danger deletedate' onclick='deleteDate(" + value.archiveEvaluationDateSignedID + ", false)'><i class='glyphicon glyphicon-trash'></i></a></li>";
                        });
                        dataContentString += "</ul>";

                        $("#reEvaluationSignature").attr("data-content", dataContentString);
                        $("#reEvaluationSignature").popover('show');
                    }
                    else {
                        folder.attr("disabled", "disabled");
                    }
                }
            },
            error: function (data) {
                alert("There was an error while retrieving past dates.");
            }
        });
    });

    $(".toggle-reCompletedPopover").click(function () {

        //studentid, will have to get the iep
        var student = $("#studentId").val();
        var folder = $(this);

        //ajax call to get the past dates
        $.ajax({
            type: 'GET',
            url: '/Manage/GetPastCompletedReEvaluationDates',
            data: { studentId: student },
            dataType: "json",
            async: false,
            success: function (data) {
                if (data.Result === "success") {
                    if (data.Dates && data.Dates.length > 0) {
                        var dataContentString = "<ul class='popupcontainer'>";
                        $.each(data.Dates, function (key, value) {
                            dataContentString += "<li data-completeId='" + value.archiveEvaluationDateID + "'><input type='date' value='" + new Date(parseInt(value.evalutationDate.replace('/Date(', ''))).toISOString().split('T')[0] + "'><a href='#' class='btn-sm btn-success editComplete' onclick='editCompleteDate(" + value.archiveEvaluationDateID + ")'><i class='glyphicon glyphicon-ok'></i></a><a href='#' class='btn-sm btn-danger deletedate' onclick='deleteDate(" + value.archiveEvaluationDateID + ", true)'><i class='glyphicon glyphicon-trash'></i></a></li>";
                        });
                        dataContentString += "</ul>";

                        $("#reEvalCompleted").attr("data-content", dataContentString);
                        $("#reEvalCompleted").popover('show');
                    }
                    else {
                        folder.attr("disabled", "disabled");
                    }
                }
            },
            error: function (data) {
                alert("There was an error while retrieving past dates.");
                console.log(data);
            }
        });
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

    // hooks up the dates on safari and explorer with a datepicker.
    $("#IEPBeginDate").datepicker({
        dateFormat: "mm/dd/yy"
    }).datepicker("setDate", "0");
    $("#IEPEndDate").datepicker({
        dateFormat: "mm/dd/yy"
    }).datepicker("setDate", "0");

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

    $("#assignChildCount").on('change', function (e) {
        var optionSelected = $("option:selected", this);
        var valueSelected = this.value;

        $.ajax({
            type: 'GET',
            url: '/Manage/GetBuildingsByDistrictId',
            data: { districtId: valueSelected },
            dataType: "json",
            async: false,
            success: function (data) {
                if (data.Result === "success") {
                    // clear the select
                    var responsibleBuildingElement = $('.districtOnly');
                    $('#AttendanceBuildingId').find('option').remove().end();

                    // add the new options to the select
                    var responsibleBuilding = responsibleBuildingElement.html();
                    $.each(data.DistrictBuildings, function (key, value) {
                        responsibleBuilding += "<option value='" + value.BuildingID + "'>" + value.BuildingName + "</option>";
                    });

                    // trigger chosen select to update.
                    responsibleBuildingElement.html(responsibleBuilding);
                    responsibleBuildingElement.trigger("change");
                    responsibleBuildingElement.trigger("chosen:updated");
                } else {
                    alert(data.Message);
                }
            },
            error: function (data) {
                alert("There was an error retrieving the building information.");
            },
            complete: function (data) {
                $(".info").hide();
            }
        });
    });

    $(".add-contact").on("click", function () {
        // clone and unhide the contact template.
        var newContact = $("#contact-template").clone().removeAttr("id").removeAttr("style").addClass("student-contact").appendTo("#student-contacts");
        var index = $(".student-contact").length;

        newContact.html(newContact.html().replace(/\[#\]/g, '[' + index + ']'));
        newContact.find(".contact-button").removeClass("contact-button btn-info").addClass('btn-danger');

        initContacts();

        return false;
    });

    $("#nokidsId").on('click', function () {
        if ($("#kidsid").attr("disabled") !== undefined) {
            $("#kidsid").removeAttr("disabled");
            $("#kidsid").val("");
        } else {
            $("#kidsid").attr("disabled", "disabled");
            $("#kidsid").val("0000000000");
        }
    });
});

function init() {

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
}

$("#submitLearner").on("click", function () {

    var theForm = document.getElementById("createNewStudent");

    if ($("#misDistrict").val() === "" || $("#misDistrict").val() === null) {
        $("#misDistrict_chosen").addClass('contact-tooltip');

        return alert("Attending Location is required.");
    } else {
        $("#misDistrict_chosen").removeClass('contact-tooltip');
    }

    if (tabValidates()) {
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
            }
        });
    }
});


function tabValidates() {
    var validates = true;
    //var stepId = $('.wizard .nav-tabs li.active a').attr('href');
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
