$(function () {
    function init() {

		$(".chosen-select").chosen({ width: "100%", });

		if ($("#dashboardNotification").length > 0 && ($("#showBannerNotification").val() == "1")) {  
			var message = "";

			if ($("#dueIepsCount").length > 0) {
				message = "(" + $("#dueIepsCount").val() + ") IEPs Coming Due"
			}

			if ($("#draftIepsCount").length > 0) {
				if (message != "") {
					message += " and ";
				}
				message += "(" + $("#draftIepsCount").val() + ") Draft IEPs"
			}

			if (message != "") {
				message = "<a href='#' class='notification' onclick = '_showNotifications();' > <i class='fa fa-bell' title='You have pending actions' data-toggle='tooltip'></i> You have " + message + "</a>";
				_showAlert(message, false);
			}
		}

		$("#notificationBtn").on("click", function () {
			_showNotifications();
			return false;
		});

		$("#iepsDueListBtn").on("click", function () {
			$("#iepsDueList").fadeToggle("fast", "linear");
		});

		$("#iepsDraftListBtn").on("click", function () {
			$("#iepsDraftList").fadeToggle("fast", "linear");
		});

		$(".dashboardIEP").on("click", function () {
			//open iep like this to prevent true false button switches from not working right in firefox
			var stid = $(this).attr("data-id");
			window.location.href = '/Home/StudentProcedures?stid=' + stid;
			return false;
		});

		/* Event */
		/* When the user clicks the close button on the alert  */
		$("#alertMessage").on("click", function (e) {
			$(e.currentTarget).hide();
		});
				

		// attach Event
        // fires when a user clicks on the new system user button
        $("#user-toggle").on("click", function () {
            $(".ajax-loader").show();
            $(".ajax-loader img").show();
        });

        // attach Event
        // fires when a user clicks on the edit user button
        $("#editStudent").on("click", function () {
            $(".ajax-loader").show();
            $(".ajax-loader img").show();
        });

        // attach Event
        // fires when a user clicks on the edit user button
        $(".viewForms").on("click", function () {
            $(".ajax-loader").show();
            $(".ajax-loader img").show();
        });

        // attach event
        // finds all launchIEP on the page and fires when the link is clicked.
        $('.launchIEP').each(function (index) {
            $(this).not('.bound').addClass('bound').on("click", function (e) {
                e.preventDefault();
                $(".ajax-loader").show();
                $(".ajax-loader img").show();
            });
		});

        // attach event
        // fires when an delete button is pressed.
        $('#deleteUser').on('show.bs.modal', function (e) {
            var user = $(e.relatedTarget).data('id');
            $(e.currentTarget).find('input[name="id"]').val(user);
            $('#confirmDeletion').val('');
        });

        // attach event
        // fires when you click yes on the module deleteForm.
        $('#deleteUser button[type=submit]').on('click', function (e) {
            if ($('#confirmDeletion').val() === 'DELETE') {
                var userId = $(e.currentTarget).parent().parent().find('input[name="id"]').val();
                $.ajax({
                    type: 'POST',
                    url: '/Manage/Delete',
                    data: { id: userId },
                    dataType: "json",
                    success: function (data) {
                        var userRow = $("tr[data-id=" + userId + "]")
                        userRow.remove();

                        $("#alertMessage .moreinfo").html(data.Message);
                        $("#alertMessage").fadeTo(2000, 500).slideUp(500, function () {
                            $("#alertMessage").slideUp(500);
                        });
                    },
                    error: function (data) {
                        console.log("Unable to connect to the server or another related problem.");
                    }
                });
            }
		});

		// attach event
		// fires when the MIS chooses active/inactive
		$('#filterActive').change(function () {

			var statusActive = this.value;
			var selectedDistrict = $("#userDistricts option:selected").val() + "";
			var selectedBuilding = $("#userBuildings option:selected").val() + "";

			$(".ajax-loader").show();

			$.ajax({
				type: "POST",
				url: "/Manage/FilterUserList",
				dataType: "json",
				data: {
					DistrictId: selectedDistrict,
					BuildingId: selectedBuilding,
					RoleId: 5,
					statusActive: statusActive
				},
				async: false,
				success: function (data) {
					if (data.Result === "success") {

						var results = data.Message;

						// blow away the building list 
						$('#userBuildings').empty();

						var studentDropDown = $("#filterName");
						studentDropDown.find('option').remove().end();
						studentDropDown
							.append($('<option>', {
								value: "-1"
							})
								.text("All Users"));

						// hide all the users in the list.
						var filterCollection = $('#studentTable > tbody > tr');

						$.each(filterCollection, function (index, value) {
							$(value).addClass('hidden');
						});

						$('#userBuildings').append('<option value="-1">All Buildings</option>');
						if (results.buildings.length > 0) {
							$.each(results.buildings, function (index, value) {
								console.log(value);
								$('#userBuildings').append('<option value="' + value.BuildingID + '">' + value.BuildingName + '</option>');
							});
						}

						if (results.members.length > 0) {
							$.each(filterCollection, function (filterIndex, filterValue) {
								$.each(results.members, function (index, value) {								

									if ($(filterValue).data('id') === value.UserID) {
										$(filterValue).removeClass('hidden');
										return false;
									}
								});
							});

							$.each(results.members, function (index, value) {
								var roleId = value.RoleID;
								if (roleId == 5) {									
									var studentName = _getStudentName(value);
									studentDropDown
										.append($('<option>', { value: value.UserID })
											.text(studentName));
								}
							});
							
						}

						studentDropDown.trigger("chosen:updated");

					} else {
						alert("doh");
					}
				},
				error: function (data) {
					alert("Not connected to the network!");

					console.log(data);
				},
				complete: function (data) {
					$(".ajax-loader").hide();
					//A function to be called when the request finishes 
					// (after success and error callbacks are executed). 
				}
			});	
		
		});

		// attach event
		// fires when the MIS chooses a user
		$('#filterName').change(function () {
			var userId = this.value;
			var selectedDistrict = $("#userDistricts option:selected").val() + "";
			var selectedBuilding = $("#userBuildings option:selected").val() + "";
			

			$(".ajax-loader").show();

			$.ajax({
				type: 'POST',
				url: '/Manage/FilterStudentList',
				dataType: 'json',
				data: { DistrictId: selectedDistrict, BuildingId: selectedBuilding, userId: userId },
				async: false,
				success: function (data) {
					if (data.Result === "success") {
						var results = data.Message;

						// blow away the building list 
						$('#userBuildings').empty();

						// hide all the users in the list.
						var filterCollection = $('#studentTable > tbody > tr');

						$.each(filterCollection, function (index, value) {
							$(value).addClass('hidden');
						});

						$('#userBuildings').append('<option value="-1">All Buildings</option>');
						if (results.buildings.length > 0) {
							$.each(results.buildings, function (index, value) {
								console.log(value);
								$('#userBuildings').append('<option value="' + value.BuildingID + '">' + value.BuildingName + '</option>');
							});
						}

						if (results.members.length > 0) {
							$.each(filterCollection, function (filterIndex, filterValue) {
								$.each(results.members, function (index, value) {
									if ($(filterValue).data('id') === value.UserID) {
										$(filterValue).removeClass('hidden');
										return false;
									}
								});
							});
						}
					}
					else {
						alert('doh');
					}
				},
				error: function (data) {
					alert('Not connected to the network!');

					console.log(data);
				},
				complete: function (data) {
					$(".ajax-loader").hide();
					//A function to be called when the request finishes 
					// (after success and error callbacks are executed). 
				}
			});
		});

		// attach event
		// fires when the MIS chooses a district
		$("#userDistricts").on('change', function () {
			var selectedDistrict = $(this).val() + "";
			var selectedBuilding = $("#userBuildings option:selected").val() + "";
			

			$(".ajax-loader").show();

			$.ajax({
				type: 'POST',
				url: '/Manage/FilterStudentList',
				dataType: 'json',
				data: { DistrictId: selectedDistrict, BuildingId: selectedBuilding },
				async: false,
				success: function (data) {
					if (data.Result === "success") {

						var studentDropDown = $("#filterName");
						studentDropDown.find('option').remove().end();
						studentDropDown
							.append($('<option>', {
								value: "-1"
							})
								.text("All Users"));

						// hide all the users in the list.
						var filterCollection = $('#studentTable > tbody > tr');

						var i = filterCollection.length;
						while (i >= 0) {
							$(filterCollection[i]).addClass('hidden');
							i--;
						}

						var results = data.Message;
						if (results.members.length > 0) {

							var j = results.members.length - 1;
							while (j >= 0) {
								var roleId = results.members[j].RoleID;
								if (roleId == 5) {
									var studentName = _getStudentName(results.members[j]);
									studentDropDown
										.append($('<option>', { value: results.members[j].UserID })
											.text(studentName));
								}


								var foundIndex = Object.keys(filterCollection).map(function (x) { return $(filterCollection[x]).data('id'); }).indexOf(results.members[j].UserID);
								$(filterCollection[foundIndex]).removeClass('hidden');
								j--;
							}
							
						}

						studentDropDown.trigger("chosen:updated");
					}
					else {
						alert('doh');
					}
				},
				error: function (data) {
					alert('ERROR!!!');

					console.log(data);
				},
				complete: function (data) {
					$(".ajax-loader").hide();
					//A function to be called when the request finishes 
					// (after success and error callbacks are executed). 
				}
			});
		});

		// attach event
		// fires when the MIS chooses a building
		$("#userBuildings").on('change', function () {
			var selectedDistrict = $("#userDistricts option:selected").val() + "";
			var selectedBuilding = $(this).val() + "";
			
			$(".ajax-loader").show();

			$.ajax({
				type: 'POST',
				url: '/Manage/FilterStudentList',
				dataType: 'json',
				data: { DistrictId: selectedDistrict, BuildingId: selectedBuilding },
				async: false,
				success: function (data) {
					if (data.Result === "success") {

						var studentDropDown = $("#filterName");
						studentDropDown.find('option').remove().end();
						studentDropDown
							.append($('<option>', {
								value: "-1"
							})
								.text("All Users"));


						// hide all the users in the list.
						var filterCollection = $('#studentTable > tbody > tr');
												
						var i = filterCollection.length;
						while (i >= 0) {
							$(filterCollection[i]).addClass('hidden');
							i--;
						}

						var results = data.Message;
						if (results.members.length > 0) {

							var j = results.members.length - 1;
							while (j >= 0) {

								var roleId = results.members[j].RoleID;
								if (roleId == 5) {									
									var studentName = _getStudentName(results.members[j]);

									studentDropDown
										.append($('<option>', { value: results.members[j].UserID })
											.text(studentName));
								}

								var foundIndex = Object.keys(filterCollection).map(function (x) { return $(filterCollection[x]).data('id'); }).indexOf(results.members[j].UserID);
								$(filterCollection[foundIndex]).removeClass('hidden');
								j--;
							}
							
						}

						studentDropDown.trigger("chosen:updated");
					}
					else {
						alert('doh');
					}
				},
				error: function (data) {
					alert('ERROR!!!');

					console.log(data);
				},
				complete: function (data) {
					$(".ajax-loader").hide();
					//A function to be called when the request finishes 
					// (after success and error callbacks are executed). 
				}
			});
		});



    }
    init();

    //SET UP 
    var params =	//All params are optional, you can just assign {} 
    {
        "navB": "slide",	//Effect for navigation button, leave it empty to disable it
        "but": false,		//Flag to enable transitions on button, false by default
        "cBa": function () { init(); } //callback function
    };
	new ft(params);

	function _getStudentName(value) {
		var lastName = value.LastName == null ? "" : value.LastName;
		var firstName = value.FirstName == null ? "" : value.FirstName;
		var middleName = value.MiddleName == null ? "" : value.MiddleName;
		return lastName + ", " + firstName + " " + middleName;;
	}
});


function _showNotifications() {
	$("#dashboardNotification").modal();

}

function _showAlert(message, positive) {

	var successFade = 9000;
	if ($("#alertMessage").css('display') && $("#alertMessage").css('display') === 'none') {
		if (positive) {
			$("#alertMessage").removeClass('alert-danger').addClass('alert-success'); successFade = 3000;
		}
		else {
			$("#alertMessage").removeClass('alert-success').addClass('alert-danger');
		}
		
		if ($("#alertMessage").css('display') && $("#alertMessage").css('display') === 'none') {
			$("#alertMessage .moreinfo").html(message);
			$("#alertMessage").fadeTo(successFade, 500);
		}
	}
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
        var toggle = "li .listrap-toggle ";
        var selectionChanged = function () {
            $(this).parent().parent().toggleClass("active");
            listrap.trigger("selection-changed", [listrap.getSelection()]);
        };
        $(listrap).find(toggle + "img").on("click", selectionChanged);
        $(listrap).find(toggle + "span").on("click", selectionChanged);
        return listrap;
    }
});