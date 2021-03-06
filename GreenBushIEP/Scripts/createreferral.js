﻿$(document).ready(function () {

    init();
    initContacts();
   
    if ($("#nokidsId").prop("checked") == true) {
        $("#kidsid").attr("disabled", "disabled");
        $("#kidsid").val("0000000000");
    }

    $("#nokidsId").on('click', function () {
        if ($("#kidsid").attr("disabled") !== undefined) {
            $("#kidsid").removeAttr("disabled");
            $("#kidsid").val("");
        } else {
            $("#kidsid").attr("disabled", "disabled");
            $("#kidsid").val("0000000000");
        }
    });

    $("#referralType").on('change', function (e) {

        var valueSelected = this.value;
        if (valueSelected == "Incoming") {
            //show enrollment date and disabilities
            $(".incomingRefferal").show();
        }
        else {
            //clear fields
            $("#enrollmentDate").val("");
            $("#primaryDisability").val("");
            $("#secondaryDisability").val("");
            $(".incomingRefferal").hide();
        }
    });

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
				console.log(data);
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
   
});

function init() {

    $(".studentDOB").datepicker({
        dateFormat: "mm/dd/yy",
        changeMonth: true,
        changeYear: true
    });

    $(".chosen-select").chosen({
        disable_search_threshold: 10,
        no_results_text: "Oops, nothing found!",
        width: "100%"
    });

    var link = document.createElement("a");
    link.href = "/Scripts/bootstrap-mutliselect.js";

    // loads the building script
    $.getScript(link.protocol + "//" + link.host + link.pathname + link.search + link.hash, function () { });

    //Initialize tooltips
    $('.nav-tabs > li a[title]').tooltip();

    //Wizard
    $('a[data-toggle="tab"]').on('show.bs.tab', function (e) {
        var $target = $(e.target);
        if ($target.parent().hasClass('disabled')) {
            return false;
        }
    });

    $(".prev-step").click(function (e) {
        var $active = $('.wizard .nav-tabs li.active');
        prevTab($active);
    });

    $('[name="SearchBuildingList"]').keyup(function (e) {
        var code = e.keyCode || e.which;
        if (code === '9') return;
        if (code === '27') $(this).val(null);
        var $rows = $(this).closest('.dual-list').find('.list-group li');
        var val = $.trim($(this).val()).replace(/ +/g, ' ').toLowerCase();
        $rows.show().filter(function () {
            var text = $(this).text().replace(/\s+/g, ' ').toLowerCase();
            return !~text.indexOf(val);
        }).hide();
	});

	

    function nextTab(elem) {
        $(elem).next().find('a[data-toggle="tab"]').click();
    }
    function prevTab(elem) {
        $(elem).prev().find('a[data-toggle="tab"]').click();
    }
}

function tabValidates() {
    var validates = true;
    var stepId = $('.wizard .nav-tabs li.active a').attr('href');
    var $inputs = $(stepId + " :input[data-validate='true']");

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

function initContacts() {

    $('.student-contact:not(.bound)').addClass('bound').each(function (index) {
        $(this).find('.remove-contact').on("click", function (e) {
            $(this).parents(".student-contact").fadeOut(300, function () {
                $(this).remove();
            });
        });
    });
}

$("#next2").on("click", function () {

	var theForm = document.getElementById("createNewReferral");

    if (tabValidates()) {

        $.ajax({
            url: '/Manage/CreateReferral',
            type: 'POST',
			data: $("#createNewReferral").serialize(),
            success: function (data) {
                if (data.Result === "success") {

                    var $active = $('.wizard .nav-tabs li.active');
					$active.removeClass('disabled');
                    $active.next().removeClass('disabled');
                    $($active).next().find('a[data-toggle="tab"]').click();
					
					// create a new student id and add it to the contacts form here.
					$("form:eq(0)").find("input[name='studentId']").val(data.Message);
					$("form:eq(1)").find("input[name='studentId']").val(data.Message);
					$("form:eq(2)").find("input[name='studentId']").val(data.Message);
					$("form:eq(3)").find("input[name='studentId']").val(data.Message);
					//$("form:eq(4)").find("input[name='studentId']").val(data.Message);
                } else {

                    alert(data.Message);
                }
            },
            error: function (data) {
                alert("There was an error when attempting to connect to the server.");
            }
        });

    }
});

$("#next3").on("click", function () {

	var theForm = document.getElementById("createReferralData");

	if (tabValidates()) {

		$.ajax({
			url: '/Manage/CreateReferralSchoolData',
			type: 'POST',
			data: $("#createReferralData").serialize(),
			success: function (data) {
				if (data.Result === "success") {

					var $active = $('.wizard .nav-tabs li.active');
					$active.next().removeClass('disabled');
					$($active).next().find('a[data-toggle="tab"]').click();
					
					$("form:eq(1)").find("input[name='studentId']").val(data.Message);
					$("form:eq(2)").find("input[name='studentId']").val(data.Message);
					$("form:eq(3)").find("input[name='studentId']").val(data.Message);
					$("form:eq(4)").find("input[name='studentId']").val(data.Message);

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

//function checkPrimarySelected() {

//    //check if primary selected
//    var numberOfContacts = $("#student-contacts").find('input.primaryContactCheckbox:checkbox').length;
//    var primaryChecked = $('input.primaryContactCheckbox:checkbox:checked').length;
//    if (numberOfContacts > 0 && primaryChecked == 0) {
//        return false;
//    }

//    return true;
//}

//function createStudentContacts() {
//    $.ajax({
//        url: '/Manage/CreateReferralContacts',
//        type: 'POST',
//        data: $("#createReferralContacts").serialize(),
//        success: function (data) {
//            if (data.Result === "success") {

//                var $active = $('.wizard .nav-tabs li.active');
//                $active.next().removeClass('disabled');
//                $($active).next().find('a[data-toggle="tab"]').click();

//                $("form:eq(1)").find("input[name='studentId']").val(data.Message);
//                $("form:eq(2)").find("input[name='studentId']").val(data.Message);
//                $("form:eq(3)").find("input[name='studentId']").val(data.Message);
//                $("form:eq(4)").find("input[name='studentId']").val(data.Message);

//                $("#pSummary").html(data.Summary);

//            } else {

//                alert(data.Message);
//            }
//        },
//        error: function (data) {
//            alert("There was an error when attempt to connect to the server.");
//        }
//    });
//}


$("#next4").on("click", function () {

    var theForm = document.getElementById("createReferralContacts");

    if (tabValidates()) {
        $.ajax({
            url: '/Manage/CreateReferralContacts',
            type: 'POST',
            data: $("#createReferralContacts").serialize(),
            success: function (data) {
                if (data.Result === "success") {

                    var $active = $('.wizard .nav-tabs li.active');
                    $active.next().removeClass('disabled');
                    $($active).next().find('a[data-toggle="tab"]').click();

                    $("form:eq(1)").find("input[name='studentId']").val(data.Message);
                    $("form:eq(2)").find("input[name='studentId']").val(data.Message);
                    $("form:eq(3)").find("input[name='studentId']").val(data.Message);
                    $("form:eq(4)").find("input[name='studentId']").val(data.Message);

                    $("#pSummary").html(data.Summary);

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

$(".submitForm").on("click", function (e) {

	e.preventDefault();
	$(this).attr("disabled", true);

	var theForm = document.getElementById("submitReferral");	
	$.ajax({
			url: '/Manage/SubmitReferral',
			type: 'POST',
			data: $("#submitReferral").serialize(),
			success: function (data) {
				if (data.Result === "success") {

					var $active = $('.wizard .nav-tabs li.active');
					$active.next().removeClass('disabled');
					$($active).next().find('a[data-toggle="tab"]').click();
					
					var returnUrl = '/Manage/Referrals';
					window.location = returnUrl;

				} else {
					$(this).attr("disabled", false);
					alert(data.Message);

				}
			},
		error: function (data) {
			$(this).attr("disabled", false);
				alert("There was an error when attempt to connect to the server.");
				
			}
	});	

	return false;
});


