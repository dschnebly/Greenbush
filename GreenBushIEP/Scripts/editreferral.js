$(document).ready(function () {	

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

	if ($("#alertMessage").text() == "")
		$("#alertMessage").hide();

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

	$('#misDistrict').change(function () {
		$(this).trigger("chosen:updated");		
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


function tabValidates() {
	
	if ($('#alertMessage').css('display') == 'block') {
		$('#alertMessage').html("");		
		$('#alertMessage').hide();		
	}

    var validates = true;
    var stepId = $('.wizard .nav-tabs li.active a').attr('href');
    var $inputs = $(stepId + " :input[data-validate='true']");

    $inputs.each(function () {
        var input = $(this);
		var is_valid = input.val();

		if (input.is("select") && input[0] != undefined && input[0].id =="misDistrict" && is_valid.length == 0) {
			is_valid = "";
		}

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

function checkPrimarySelected() {

	//check if primary selected	
	var primaryChecked = $('input.primaryContactCheckbox:checkbox:checked').length;
	if (primaryChecked == 0) {
		return false;
	}

	return true;
}

function createStudentContacts() {
	
	$.ajax({
		url: '/Manage/EditReferralContacts',
		type: 'POST',
		data: $("#editStudentContacts").serialize(),
		success: function (data) {
			if (data.Result === "success") {

				var $active = $('.wizard .nav-tabs li.active');
				$active.next().removeClass('disabled');


				$($active).next().find('a[data-toggle="tab"]').click();

				$('*[name*=studentId]').each(function () {
					$(this).val(data.Message);
				});


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

				$('#alertMessage').html(data.Message);
				$('#alertMessage').show();
			}
		},
		error: function (data) {
			alert("There was an error when attempt to connect to the server.");
		}
	});
}

$("#btnPrimaryContactContinue").on("click", function () {
	$('#primaryContactErrorModal').modal('toggle');
	createStudentContacts();
});

$("#next7").on("click", function () {
	if (tabValidates()) {
		
		var primary = checkPrimarySelected();
		if (!primary) {
			$('#primaryContactErrorModal').modal();			
		}
		else {
			createStudentContacts();
		}
	}
	else {
		$('#alertMessage').html("Please verify that all required fields are filled out.");
		$('#alertMessage').show();
	}	
});

$("#next5").on("click", function () {

	var theForm = document.getElementById("editStudent");
	if (tabValidates()) {

		$.ajax({
			url: '/Manage/EditReferral',
			type: 'POST',
			data: $("#editStudent").serialize(),
			success: function (data) {
				if (data.Result === "success") {
					var $active = $('.wizard .nav-tabs li.active');
					$active.next().removeClass('disabled');
					$($active).next().find('a[data-toggle="tab"]').click();

					$('*[name*=studentId]').each(function () {
						$(this).val(data.Message);
					});
					
				} else {

					$('#alertMessage').html(data.Message);
					$('#alertMessage').show();
				}
			},
			error: function (data) {
				alert("There was an error when attempt to connect to the server.");
			}
		});

	}
	else {
		$('#alertMessage').html("Please verify that all required fields are filled out.");
		$('#alertMessage').show();
	}
});

$("#next6").on("click", function () {

	var theForm = document.getElementById("editStudentOptions");

	if (tabValidates()) {

		$.ajax({
			url: '/Manage/EditReferralOptions',
			type: 'POST',
			data: $("#editStudentOptions").serialize(),
			success: function (data) {
				if (data.Result === "success") {

					var $active = $('.wizard .nav-tabs li.active');
					$active.next().removeClass('disabled');
					$($active).next().find('a[data-toggle="tab"]').click();
					
					$('*[name*=studentId]').each(function () {
						$(this).val(data.Message);
					});

				} else {

					$('#alertMessage').html(data.Message);
					$('#alertMessage').show();
				}
			},
			error: function (data) {
				alert("There was an error when attempt to connect to the server.");
			}
		});

	}
	else {
		$('#alertMessage').html("Please verify that all required fields are filled out.");
		$('#alertMessage').show();
	}

	
});

$("#next8").on("click", function () {

	if (tabValidates()) {
		$("#IsDraftSubmit").val(1);
		$("#avatarForm").submit();
	}
	else {
		$('#alertMessage').html("Please verify that all required fields are filled out.");
		$('#alertMessage').show();
	}
});

$("#next9").on("click", function () {


	var activeTeachers = $(".listrap").listrap().getSelection();
	var teacherIds = [];
	var teacherList = $("#selectedTeachers");

	if (activeTeachers.length > 0) {
		$.each(activeTeachers, function (index, value) {
			teacherIds.push($(activeTeachers[index]).find('.ourTeacher').data("id"));
		});					

		teacherList.val(teacherIds);
	}

	var $active = $('.wizard .nav-tabs li.active');
	$active.next().removeClass('disabled');
	$($active).next().find('a[data-toggle="tab"]').click();

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