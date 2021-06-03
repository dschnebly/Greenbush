$(function () {
    function init() {

		$(".chosen-select").chosen({ width: "100%", });

        // attach Event
        // fires when a user clicks on the new system user button
        $("#user-toggle").on("click", function () {
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

						//clear user list
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

						//clear user list
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

	//SET UP FOR TRANSITIONS
	var params = //All params are optional, you can just assign {} 
	{
		"navB": "slide", //Effect for navigation button, leave it empty to disable it
		"but": true, //Flag to enable transitions on button, false by default
		"cBa": function () {
			init();

			//callback function
			var ajax = document.querySelector(".ajax-loader");
			if (ajax != null) {
				ajax.style.display = 'none';
			}
		}
	};
	new ft(params);

	function _getStudentName(value) {
		var lastName = value.LastName == null ? "" : value.LastName;
		var firstName = value.FirstName == null ? "" : value.FirstName;
		var middleName = value.MiddleName == null ? "" : value.MiddleName;
		return lastName + ", " + firstName + " " + middleName;;
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

window.addEventListener('DOMContentLoaded', function () { $(".ajax-loader").hide(); });