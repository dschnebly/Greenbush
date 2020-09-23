$(document).ready(function () {

    init();
    initContacts();

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
        if($("#kidsid").attr("disabled") !== undefined) {
            $("#kidsid").removeAttr("disabled");
            $("#kidsid").val("");
        } else {
            $("#kidsid").attr("disabled", "disabled");
            $("#kidsid").val("0000000000");
        }
    });
});

function editSignedDate(Id) {
    var userId = parseInt($("#studentId").val());
    var date = $("li[data-signedId='" + Id + "']").find("input").val();

    $.ajax({
        type: 'GET',
        url: '/Manage/editReevalSignedDates',
        data: { studentId: userId, dateId: Id, newDateValue: date },
        dataType: 'json',
        success: function (data) {
            if (data.Result === 'success') {
                $("#reEvaluationSignature").popover('hide');
            } else {
                alert(data.Message);
            }
        },
        error: function (data) {
            alert("Unable to connect to the server or other related network problem. Please contact your admin.");
        }
    });
}

function editCompleteDate(Id) {
    var userId = parseInt($("#studentId").val());
    var date = $("li[data-completeId='" + Id + "']").find("input").val();

    $.ajax({
        type: 'GET',
        url: '/Manage/editReevalCompletDates',
        data: { studentId: userId, dateId: Id, newDateValue: date },
        dataType: 'json',
        success: function (data) {
            if (data.Result === 'success') {
                $("#reEvalCompleted").popover('hide');
            } else {
                alert(data.Message);
            }
        },
        error: function (data) {
            alert("Unable to connect to the server or other related network problem. Please contact your admin.");
        }
    });
}

function deleteDate(Id, isComplete) {

    var answer = confirm("Are you sure you want to delete this date?");

    if (answer) {
        //ajax call to get the past dates
        $.ajax({
            type: 'GET',
            url: '/Manage/deleteReEvaluationDates',
            data: { dateId: Id, Completed: isComplete },
            dataType: "json",
            async: false,
            success: function (data) {
                if (data.Result === "success") {
                    if (isComplete) {
                        if ($("li[data-completeId='" + Id + "']").closest("ul").children().length > 1) {
                            $("li[data-completeId='" + Id + "']").remove();
                        } else { // hide the popup because there are none left;
                            $("#reEvalCompleted").popover('hide');
                        }
                    } else {
                        if ($("li[data-signedId='" + Id + "']").closest("ul").children().length > 1) {
                            $("li[data-signedId='" + Id + "']").remove();
                        } else { // hide the popup because there are none left;
                            $("#reEvaluationSignature").popover('hide');
                        }
                    }
                } else {
                    alert("There was a problem while trying to delete the date.");
                }
            },
            error: function (data) {
                alert("There was an error while retrieving past dates.");
            }
        });

        return true;
    }

    return false;
}

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

    $('#buildingIds').hide();
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

    $('#Is_Gifted').click(function () {
        if ($(this).is(':checked')) {
            $('#claimingCode').prop('checked', false);
        }
        else {
            $('#claimingCode').prop('checked', true);
        }
    });

    function nextTab(elem) {
        $(elem).next().find('a[data-toggle="tab"]').click();
    }
    function prevTab(elem) {
        $(elem).prev().find('a[data-toggle="tab"]').click();
    }
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

function checkPrimarySelected() {

	//check if primary selected
	var numberOfContacts = $("#student-contacts").find('input.primaryContactCheckbox:checkbox').length;
	var primaryChecked = $('input.primaryContactCheckbox:checkbox:checked').length;
	if (numberOfContacts > 0 && primaryChecked == 0) {
		return false;
	}

	return true;
}

function createStudentContacts() {	
		$.ajax({
			url: '/Manage/CreateStudentContacts',
			type: 'POST',
			data: $("#createStudentContacts").serialize(),
			success: function (data) {
				if (data.Result === "success") {

					var $active = $('.wizard .nav-tabs li.active');
					$active.next().removeClass('disabled');
					$($active).next().find('a[data-toggle="tab"]').click();


					// clear out the ul list
					$("ul#teacherList").empty();

					// our array to user for appending
					var items = [];

					if (!$.isEmptyObject(data.teacherList)) {

						$.each(data.teacherList, function (key, value) {
							var userImage = '/Content/Images/newUser.png';
							items.push("<li><div class='listrap-toggle pull-left'><span class='ourTeacher' data-id='" + value.UserID + "'></span><img src='" + userImage + "' class='img-circle pull-left img-responsive' style='height:60px;width:60px;' /></div><div class='teacher-search-additional-information'><strong>" + value.Name + "</strong></div></li>");
						});

						$("ul#teacherList").append.apply($("ul#teacherList"), items);
						$(".listrap").listrap().getSelection();
					}
				} else {
					alert(data.Message);
				}
			},
			error: function (data) {
				alert("There was an error when attempt to connect to the server.");
			}
		});
	
}


function editStudentContacts() {	

		$.ajax({
			url: '/Manage/EditStudentContacts',
			type: 'POST',
			data: $("#editStudentContacts").serialize(),
			success: function (data) {
				if (data.Result === "success") {

					var $active = $('.wizard .nav-tabs li.active');
					$active.next().removeClass('disabled');
					$($active).next().find('a[data-toggle="tab"]').click();

					// clear out the ul list
					$("ul#teacherList").empty();

					// our array to user for appending
					var items = [];
					var assignments = null;

					if (!$.isEmptyObject(data.assignments)) {
						assignments = data.assignments;
					}

					if (!$.isEmptyObject(data.teacherList)) {

						$.each(data.teacherList, function (key, value) {
							var userImage = '/Content/Images/newUser.png';
							var isActive = "";
							var isActiveImage = "";
							if (assignments !== null) {
								var isAssigned = assignments.indexOf(value.UserID);
								if (isAssigned >= 0) {
									isActive = "active";
									isActiveImage = "img-selection-correction";
								}
							}

							items.push("<li class='" + isActive + "'><div class='listrap-toggle pull-left'><span class='ourTeacher' data-id='" + value.UserID + "'></span><img src='" + userImage + "' class='img-circle pull-left img-responsive " + isActiveImage + "' style='height:60px;width:60px;' /></div><div class='teacher-search-additional-information'><strong>" + value.Name + "</strong></div></li>");
						});

						$("ul#teacherList").append.apply($("ul#teacherList"), items);
						$(".listrap").listrap().getSelection();
					}
				} else {

					alert(data.Message);
				}
			},
			error: function (data) {
				alert("There was an error when attempt to connect to the server.");
			}
		});

	
}

$("#next2").on("click", function () {

    var theForm = document.getElementById("createNewStudent");

    if ($("#misDistrict").val() === "" || $("#misDistrict").val() === null) {
        $("#misDistrict_chosen").addClass('contact-tooltip');

        return alert("Attending District is required.");
    } else {
        $("#misDistrict_chosen").removeClass('contact-tooltip');
    }


    if (tabValidates()) {
        $.ajax({
            url: '/Manage/CreateStudent',
            type: 'POST',
            data: $("#createNewStudent").serialize(),
            success: function (data) {
                if (data.Result === "success") {

                    var $active = $('.wizard .nav-tabs li.active');
                    $active.addClass('disabled');
                    $active.next().removeClass('disabled');
                    $($active).next().find('a[data-toggle="tab"]').click();

                    // create a new student id and add it to the contacts form here.

                    $('*[name*=studentId]').each(function () {
                        $(this).val(data.Message);
                    });
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
$("#next3").on("click", function () {

    var theForm = document.getElementById("createStudentContacts");

	

    if (tabValidates()) {

        $.ajax({
            url: '/Manage/CreateStudentOptions',
            type: 'POST',
            data: $("#createStudentOptions").serialize(),
            success: function (data) {
                if (data.Result === "success") {

                    var $active = $('.wizard .nav-tabs li.active');
                    $active.next().removeClass('disabled');
                    $($active).next().find('a[data-toggle="tab"]').click();

                    $('*[name*=studentId]').each(function () {
                        $(this).val(data.Message);
                    });
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
$("#next4").on("click", function () {
	if (tabValidates()) {
		//var theForm = document.getElementById("editStudentContacts");
		var primary = checkPrimarySelected();
		if (!primary) {
			var answer = confirm("You do not have a primary contact specified.  Without specifying a primary contact no information will appear on the IEP. Do you wish to continue?");

			if (answer) {
				createStudentContacts();
			}
		}
		else {
			createStudentContacts();
		}
	}

    
});
$("#next5").on("click", function () {

    var theForm = document.getElementById("editStudent");
    if (tabValidates()) {

        $.ajax({
            url: '/Manage/EditStudent',
            type: 'POST',
            data: $("#editStudent").serialize(),
            success: function (data) {
                if (data.Result === "success") {
                    var $active = $('.wizard .nav-tabs li.active');
                    $active.next().removeClass('disabled');
                    $($active).next().find('a[data-toggle="tab"]').click();

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
$("#next6").on("click", function () {

    var theForm = document.getElementById("editStudentOptions");

    if (tabValidates()) {

        $.ajax({
            url: '/Manage/EditStudentOptions',
            type: 'POST',
            data: $("#editStudentOptions").serialize(),
            success: function (data) {
                if (data.Result === "success") {

                    var $active = $('.wizard .nav-tabs li.active');
                    $active.next().removeClass('disabled');
                    $($active).next().find('a[data-toggle="tab"]').click();
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

$("#next7").on("click", function () {

	if (tabValidates()) {
		var primary = checkPrimarySelected();
		if (!primary) {
			var answer = confirm("You do not have a primary contact specified.  Without specifying a primary contact no information will appear on the IEP. Do you wish to continue?");

			if (answer) {
				editStudentContacts();
			}
		}
		else {
			editStudentContacts();
		}
	}
    
});

$("#next8").on("click", function () {


    var activeTeachers = $(".listrap").listrap().getSelection();
    var teacherIds = [];
    var studentId = $("#createStudentAssignments").find("input[name='studentId']").val();

	if (activeTeachers.length > 0) {
		$.each(activeTeachers, function (index, value) {
			teacherIds.push($(activeTeachers[index]).find('.ourTeacher').data("id"));
		});

		$.ajax({
			type: 'POST',
			url: '/Manage/CreateStudentAssignments',
			dataType: 'json',
			data: { studentId: studentId, teachers: teacherIds },
			async: false,
			success: function (data) {
				if (data.Result === "success") {
					var $active = $('.wizard .nav-tabs li.active');
					$active.next().removeClass('disabled');
					$($active).next().find('a[data-toggle="tab"]').click();
				} else {
					alert(data.Message);
				}
			},
			error: function (data) {
				alert("There was an error when attempt to connect to the server.");
			}
		});
	}
	else {
		var $active = $('.wizard .nav-tabs li.active');
		$active.next().removeClass('disabled');
		$($active).next().find('a[data-toggle="tab"]').click();

	}

});

jQuery.fn.extend({
    listrap: function () {
        var listrap = this;
        listrap.getSelection = function () {
            var selection = new Array();
            listrap.children("li.active").each(function (ix, el) {
                selection.push($(el)[0]);
            });
            return selection;
        };
        var toggle = "li.listrap-toggle";
        var selectionChanged = function () {
            $(this).closest("li").toggleClass("active");
            $(this).find("img").toggleClass('img-selection-correction');
            listrap.trigger("selection-changed", [listrap.getSelection()]);
        };
        $("#teacherList li").on("click", selectionChanged);
        return listrap;
    }
});

