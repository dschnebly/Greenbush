﻿$(document).ready(function () {

    init();

});

function init() {

	$(".launchIEP").on("click", function () {
		var stid = $(this).attr("data-id");
		var teacherId = $('#addExistingStudentModal').data('id');
		window.location.href = '/Home/StudentProcedures?stid=' + stid + "&rtnId=" + teacherId;
		return false;
	});

    // attach event
    // fires when user-x is pressed.
    $('#confirmStudentRemoval').on('show.bs.modal', function (e) {
        var user = $(e.relatedTarget).data('id');
        $(e.currentTarget).find('input[name="id"]').val(user);
    });

    // attach event
    // fires when you click yes on the module confirmRemoval.
    $('#confirmStudentRemoval button[type=submit]').on('click', function (e) {
        var userId = $(e.currentTarget).parent().parent().find('input[name="id"]').val();
        var teacherId = $('#addExistingStudentModal').data('id');

        $.ajax({
            type: 'POST',
            url: '/Manage/RemoveFromTeacherList',
            data: { id: userId, teacherid: teacherId },
            dataType: "json",
            success: function (data) {
                location.reload(true); // force non-cache reload.
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
    // fires off when you open up the Avaliable Students Modal
	$('#addExistingStudentModal').on('show.bs.modal', function () {
		resetSearch();
		getAllStudents();		
    });

    // attach event
    // fires when the "add" button is clicked on the Avaliable Students Modal.
    $('#addAvailableStudents').on('click', function (e) {
		var teacherId = $('#addExistingStudentModal').data('id');
		var activeStudents = $(".listrap").listrap().getSelection();
		var studentIds = [];

		if (activeStudents.length > 0) {
			$.each(activeStudents, function (index, value) {
				studentIds.push($(activeStudents[index]).find('.ourStudent').data("id"));
			});

			$.ajax({
				type: 'POST',
				url: '/Manage/AddStudentsToTeacher',
				dataType: 'json',
				data: { id: teacherId, students: studentIds },
				async: false,
				success: function (data) {
					location.reload(true); // force non-cache reload.
				},
				error: function (data) {
					$("#alertMessage .moreinfo").html('Unable to connect to the server or other related problem. Please contact your admin.');
					$("#alertMessage").fadeTo(3000, 500).slideUp(500, function () {
						$("#alertMessage").slideUp(500);
					});
				}
			});
		}
        $('#addExistingStudentModal').modal('hide');
	});

	
	$('#searchStudents').on('click', function () {
		$("#searchAvailableStudents").show();
		$("#searchStudents").hide();		
	});

	$('#searchClearBtn').on('click', function () {
		resetSearch();
		getAllStudents();
	});
	    
	$('#search_muted').on('click', function () {
		getStudentBySearch();
    });

    // attach event
    // fires when the user is searching
    $('[name="contact-list-search"]').keyup(function (e) {
        var val = $(e.currentTarget).val();
        var code = e.keyCode || e.which;
        if (code === '9') return;
        if (code === '27') $(this).val(null);

        var teachers = $('table.my-table-root').find('tr.my-table-item');
        teachers.show().filter(function () {
            var text = $(this).text().replace(/\s+/g, ' ').toLowerCase();
            return !~text.indexOf(val);
        }).hide();
	});


	$('#searchStudent').keyup(function (e) {
		var keycode = (event.keyCode ? event.keyCode : event.which);
		if (keycode == '13') {
			var filterStudentName = $('#searchStudent').val();
			if (filterStudentName != "") {
				getStudentBySearch();
			}
		}
	});
}

function getAllStudents() {
	var teacherId = $('#addExistingStudentModal').data('id');

	$.ajax({
		type: 'GET',
		url: '/Manage/GetAllStudentsInBuildings',
		dataType: 'json',
		data: { id: teacherId },
		async: false,
		success: function (data) {
			if (data.Result === "success") {				
				loadStudents(data);
			}

			$("#loadingIcon").hide();
		},
		error: function (data) {
			console.log('error: ' + data);
		}
	});
}

function getStudentBySearch() {
	var teacherId = $('#addExistingStudentModal').data('id');
	var filterStudentName = $('#searchStudent').val();
	$.ajax({
		type: 'GET',
		url: '/Manage/GetAllStudentsInBuildings',
		dataType: 'json',
		data: { id: teacherId, filterStudentName: filterStudentName },
		async: false,
		success: function (data) {
			if (data.Result === "success") {
				loadStudents(data);
			}

			$("#loadingIcon").hide();
		},
		error: function (data) {
			console.log('error: ' + data);
		}
	});
}

function loadStudents(data)
{
	// clear out the ul list
	$("ul#studentList").empty();

	// our array to user for appending
	var items = [];

	if (data.Message.length > 0) {

		// rebuild the list with the new data.
		$.each(data.Message, function (i, item) {
			var userImage = item.ImageURL !== null ? '/Avatar/' + item.ImageURL : '/Content/Images/newUser.png';
			items.push("<li><div class='listrap-toggle pull-left'><span class='ourStudent' data-id='" + this.UserID + "'></span><img src='" + userImage + "' class='img-circle pull-left img-responsive' style='height:60px;width:60px;' /></div><div class='teacher-search-additional-information'><strong>" + item.FirstName.toProperCase() + " " + item.LastName.toProperCase() + "</strong><div class='school-name'>" + item.BuildingName.toProperCase() + "</div></div></li>");
		});

		$("ul#studentList").append.apply($("ul#studentList"), items);
		$(".listrap").listrap().getSelection();
	}
	else {
		$("ul#studentList").append("<li style='padding-left: 100px;'><b>No Students Found. Try Creating Some.</b></li>");
	}
}

function resetSearch() {
	$("#searchStudents").show();
	$("#searchAvailableStudents").hide();	
	$('#searchStudent').val("");
}


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
        $("#studentList li").on("click", selectionChanged);
        return listrap;
    }
});

String.prototype.toProperCase = function () {
    return this.replace(/\w\S*/g, function (txt) { return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase(); });
};
