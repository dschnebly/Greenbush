﻿$(function () {
    function init() {

		$(".chosen-select").chosen({ width: "100%", });

		if ($("#dashboardNotification").length > 0) {
			$("#dashboardNotification").modal();
		}

		$("#iepsDueListBtn").on("click", function () {
			$("#iepsDueList").fadeToggle("fast", "linear");

			if ($(this).find("span.glyphicon").hasClass("glyphicon-plus")) {
				$(this).find("span.glyphicon").removeClass("glyphicon-plus");
				$(this).find("span.glyphicon").addClass("glyphicon-minus");
			}
			else {
				$(this).find("span.glyphicon").removeClass("glyphicon-minus")
				$(this).find("span.glyphicon").addClass("glyphicon-plus")
			}
		});

		$("#iepsDraftListBtn").on("click", function () {
			$("#iepsDraftList").fadeToggle("fast", "linear");

			if ($(this).find("span.glyphicon").hasClass("glyphicon-plus")) {
				$(this).find("span.glyphicon").removeClass("glyphicon-plus");
				$(this).find("span.glyphicon").addClass("glyphicon-minus");
			}
			else {
				$(this).find("span.glyphicon").removeClass("glyphicon-minus")
				$(this).find("span.glyphicon").addClass("glyphicon-plus")
			}
		});

		$(".dashboardIEP").on("click", function () {
			//open iep like this to prevent true false button switches from not working right in firefox
			var stid = $(this).attr("data-id");
			window.location.href = '/Home/StudentProcedures?stid=' + stid;
			return false;
		});

		// filter to only active students		
		var filterCollection = $('#studentTable > tbody > tr');

		$.each(filterCollection, function (index, value) {
			if ($(value).attr("data-isActive") == 2) {
				$(value).addClass('hidden');
			}
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

			var selectedDistrict = $("#userDistricts option:selected").val() + "";
			var selectedBuilding = $("#userBuildings option:selected").val() + "";
			var selectedActive = this.value;

			$(".ajax-loader").show();

			$.ajax({
				type: 'POST',
				url: '/Manage/FilterStudentList',
				dataType: 'json',
				data: { DistrictId: selectedDistrict, BuildingId: selectedBuilding, activeType: selectedActive },
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
								var foundIndex = Object.keys(filterCollection).map(function (x) { return $(filterCollection[x]).data('id'); }).indexOf(results.members[j].UserID);
								$(filterCollection[foundIndex]).removeClass('hidden');
								j--;
							}
						}
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
								var foundIndex = Object.keys(filterCollection).map(function (x) { return $(filterCollection[x]).data('id'); }).indexOf(results.members[j].UserID);
								$(filterCollection[foundIndex]).removeClass('hidden');
								j--;
							}
						}
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