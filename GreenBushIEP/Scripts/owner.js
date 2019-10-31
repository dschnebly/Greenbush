$(function () {
    function init() {
		$(".chosen-select").chosen({ width: "100%" });

		// filter to only active students
		var filterCollection = $('.list-group-root').find('.list-group-item');

		$.each(filterCollection, function (index, value) {
			if ($(value).attr("data-isActive") == 2) {
				$(value).addClass('hidden');
			}
		});


		//$('.chosen-search input').autocomplete({
		//	minLength: 3,
		//	source: function (request, response) {
		//		$.ajax({
		//			type: 'POST',
		//			url: "/Home/SearchUserName?username=" + request.term,
		//			dataType: "json",
		//			beforeSend: function () { $('ul.chzn-results').empty(); },
		//			success: function (data) {

		//				if (data.result) {

		//					var options = $("#filterName");
		//					options.empty(); //remove all child nodes
		//					options.append($("<option />").val(-1).text("All Users"));
		//					options.trigger("chosen:updated");

		//					if (data.filterUsers.length > 0) {
		//						response($.map(data.filterUsers, function (item) {
		//							var username = item.LastName + ", " + item.FirstName;
		//							if (item.MiddleName != null)
		//								username += " " + item.MiddleName;
		//							options.append($("<option />").val(item.UserID).text(username));
		//						}));
		//					}

		//					options.trigger("chosen:updated");
		//				}
		//			}
		//		});
		//	}
		//});

        // attach Event
        // fires when a user clicks on the main new system user button
        $("#user-toggle .user-toggle-item button").on("click", function () {
            $(this).parent().find("ul").toggleClass("show-buttons hide-buttons");
        });

        // attach event
        // fires when a delete button is pressed.
        $('#deleteUser').on('show.bs.modal', function (e) {

            var user = $(e.relatedTarget).data('id');
            $(e.currentTarget).find('input[name="id"]').val(user);
            $('#confirmDeletion').val('');
        });

		// attach event
		// fires when the MIS chooses active/inactive
		$('#filterActive').change(function () {

			var selectedDistrict = $("#userDistricts option:selected").val() + "";
			var selectedBuilding = $("#userBuildings option:selected").val() + "";
			var selectedRole = $("#userRoles option:selected").val() + "";
			var selectedActive = this.value;

			$(".ajax-loader").show();

			$.ajax({
				type: 'POST',
				url: '/Manage/FilterOwnerUserList',
				dataType: 'json',
				data: { DistrictId: selectedDistrict, BuildingId: selectedBuilding, RoleId: selectedRole, activeType: selectedActive },
				async: false,
				success: function (data) {
					if (data.Result === "success") {
						var results = data.Message;

						// blow away the building list 
						$('#userBuildings').empty();

						// hide all the users in the list.
						var filterCollection = $('.list-group-root').find('.list-group-item');

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
		// fires when the owner chooses a user
		$('#filterName').change(function () {
			var userId = this.value;
			var selectedDistrict = $("#userDistricts option:selected").val() + "";
			var selectedBuilding = $("#userBuildings option:selected").val() + "";
			var selectedRole = $("#userRoles option:selected").val() + "";

			$(".ajax-loader").show();

			$.ajax({
			    type: 'POST',
				url: '/Manage/FilterOwnerUserList',
			    dataType: 'json',
			    data: { DistrictId: selectedDistrict, BuildingId: selectedBuilding, RoleId: selectedRole, userId: userId },
			    async: false,
			    success: function (data) {
			        if (data.Result === "success") {
			            var results = data.Message;

			            // blow away the building list 
			            $('#userBuildings').empty();

			            // hide all the users in the list.
			            var filterCollection = $('.list-group-root').find('.list-group-item');

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
        // fires when the owner chooses a district
        $("#userDistricts").on('change', function () {
            var selectedDistrict = $(this).val() + "";
            var selectedBuilding = $("#userBuildings option:selected").val() + "";
            var selectedRole = $("#userRoles option:selected").val() + "";

            $(".ajax-loader").show();

            $.ajax({
                type: 'POST',
                url: '/Manage/FilterOwnerUserList',
                dataType: 'json',
                data: { DistrictId: selectedDistrict, BuildingId: selectedBuilding, RoleId: selectedRole },
                async: false,
                success: function (data) {
                    if (data.Result === "success") {

                        // hide all the users in the list.
                        var filterCollection = $('.list-group-root').find('.list-group-item');

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
        // fires when the owner chooses a building
        $("#userBuildings").on('change', function () {
            var selectedDistrict = $("#userDistricts option:selected").val() + "";
            var selectedBuilding = $(this).val() + "";
            var selectedRole = $("#userRoles option:selected").val() + "";

            $(".ajax-loader").show();

            $.ajax({
                type: 'POST',
                url: '/Manage/FilterOwnerUserList',
                dataType: 'json',
                data: { DistrictId: selectedDistrict, BuildingId: selectedBuilding, RoleId: selectedRole },
                async: false,
                success: function (data) {
                    if (data.Result === "success") {

                        // hide all the users in the list.
                        var filterCollection = $('.list-group-root').find('.list-group-item');

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
        // fires when the owner chooses a role
        $("#userRoles").on('change', function () {
            var selectedDistrict = $("#userDistricts option:selected").val() + "";
            var selectedBuilding = $("#userBuildings option:selected").val() + "";
            var selectedRole = $(this).val() + "";

            $(".ajax-loader").show();

            $.ajax({
                type: 'POST',
                url: '/Manage/FilterOwnerUserList',
                dataType: 'json',
                data: { DistrictId: selectedDistrict, BuildingId: selectedBuilding, RoleId: selectedRole },
                async: false,
                success: function (data) {
                    if (data.Result === "success") {

                        // hide all the users in the list.
                        var filterCollection = $('.list-group-root').find('.list-group-item');

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
        })

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
                        var currentUser = $("div.list-group-item.bound[data-id='" + userId + "']");
                        $(currentUser).remove();

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
        // fires where the button on an alert message is clicked
        $("#alertMessage button").on("click", function (e) {
            $(e.currentTarget).parent().hide();
        });

        // attach event
        // fires when the modal is closed. removes the checkbox option if still checked.
        $("#assignBuilding").on('hidden.bs.modal', function (e) {
            var checkBox = $(this).find("a.selector");
            if (checkBox.hasClass('selected')) {
                checkBox.removeClass('selected').closest('.well').find('ul li.active').removeClass('active');
                checkBox.children('i').removeClass('glyphicon-check').addClass('glyphicon-unchecked');
            }
        });

        //////////////////////////////////////////////////////
        //
        // Events for adding and removing an MIS from a building.
        //
        /////////////////////////////////////////////////////

        // attach event
        // fires when clicking the "checkbox" on the 'add a building' modal popup.
        $('.dual-list .selector').click(function () {
            var $checkBox = $(this);
            if (!$checkBox.hasClass('selected')) {
                $checkBox.addClass('selected').closest('.well').find('ul li:not(.active)').addClass('active');
                $checkBox.children('i').removeClass('glyphicon-unchecked').addClass('glyphicon-check');
            } else {
                $checkBox.removeClass('selected').closest('.well').find('ul li.active').removeClass('active');
                $checkBox.children('i').removeClass('glyphicon-check').addClass('glyphicon-unchecked');
            }
        });

        // attack event
        // fired off when edit a building is clicked
        $('#assignBuilding').on('show.bs.modal', function (e) {
            // assign the userid to the id value in the form.
            var userId = $(e.relatedTarget).data('id');
            $(e.currentTarget).find('input[name="id"]').val(userId);

            $.ajax({
                type: 'GET',
                url: '/Manage/GetDistricts',
                data: { id: userId },
                dataType: "json",
                success: function (data) {
                    if (data.Result === "success") {
                        var buildings = data.Message;

                        $("#selectedDistrict").find('option').remove();
                        $.each(buildings, function (key, value) {
                            // throw away the key. It's simply an index counter for the returned array.
                            $("#selectedDistrict").find('option').end().append($("<option></option>").attr("value", value.USD).text(value.DistrictName));
                        });

                        // fire off the selected building event by selecting an option in the newly created list.
                        $('#selectedDistrict option:first-child').attr("selected", "selected").change();
                    }
                },
                error: function (data) {
                    alert("Unable to connect to the server!");
                    console.log(data);
                }
            });

        });

        // attach event
        // fires when searching the building list in the 'add building' modal popup
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

        // attach event
        // fires when we select a different district in the 'add building' modal popup
        $('#selectedDistrict').on('change', function () {
            var districtId = $('#selectedDistrict option:selected').val();
            var user = $("#assignBuilding").find('input[name="id"]').val();

            $.ajax({
                type: 'GET',
                url: '/Manage/GetBuildings',
                dataType: 'json',
                data: { id: districtId, userId: user },
                async: false,
                success: function (data) {
                    if (data.Result === "success") {
                        // blow away the list
                        $('#selectBuildings').empty();

                        // rebuild the list with the new data.
                        var buildings = data.Message;
                        $.each(buildings, function () {
                            $('#selectBuildings').append('<li class="list-group-item building-group-item" data-id="' + this.BuildingID + '"><i class="glyphicon glyphicon-home"></i>&nbsp;' + this.BuildingName + '</li>');
                        });
                    }
                    else {
                        console.log(data.Message);
                    }
                },
                error: function (data) {

                }
            });
        });

        // attach event
        // fires when the save button in the 'add building' modal popup is clicked.
        $('#savetheseBuildingsToThisUser').on('click', function (e) {
            e.preventDefault();
            var districtId = $('#selectedDistrict option:selected').val();
            var userId = $('#assignBuilding').find('input[name="id"]').val();
            var listOfBuildings = [];

            $.each($('#selectBuildings li.active'), function (key, value) {
                var buildingId = $(this).data("id");
                listOfBuildings.push(buildingId);
            });

            if (listOfBuildings.length > 0) {
                $.ajax({
                    type: 'GET',
                    url: '/Manage/SaveBuildingsToUser',
                    dataType: 'json',
                    traditional: true,
                    data: { USD: districtId, userId: userId, buildings: listOfBuildings },
                    async: false,
                    success: function (data) {
                        if (data.Result === "success") {
                            $("#alertMessage").removeClass('alert alert-danger').addClass("alert alert-info").hide();
                            $("#alertMessage").addClass("alert alert-info animated fadeInUp");
                            $("#alertMessage .moreinfo").html('The user was successfully added to the buildings');
                        }
                        else {
                            $("#alertMessage").removeClass('alert alert-info').show();
                            $("#alertMessage").addClass("alert alert-danger animated fadeInUp");
                            $("#alertMessage .moreinfo").html(data.Message);
                        }
                    },
                    error: function (data) {
                        $("#alertMessage").removeClass('alert alert-info').show();
                        $("#alertMessage").addClass("alert alert-danger animated fadeInUp");
                        $("#alertMessage .moreinfo").html(data.Message);
                    }
                });
            }
        });

        // attach event
        // fires when an start button in pressed on a student IEP
        $('#initIEP').on('show.bs.modal', function (e) {
            var user = $(e.relatedTarget).data('id');
            $(e.currentTarget).find('input[name="id"]').val(user);
            $('#confirmStart').val('');
        });

        // attach event
        // fires when you click start on the module startInit form.
        $('#initIEP button[type=submit]').on('click', function (e) {
            if ($('#confirmStart').val() === 'START') {
                var userId = $(e.currentTarget).parent().parent().find('input[name="id"]').val();

                $(".ajax-loader").show();
                $.ajax({
                    type: 'GET',
                    url: '/Home/UnlockStudentIEP',
                    data: { stid: userId },
                    async: false,
                    success: function (data) {
                        if (data.Result === "success") {
                            window.location.href = window.location.href;
                        } else {
                            alert(data.Message);
                        }
                    },
                    error: function (data) {
                        alert("Unknown error occurred. Please contact your administrator or a Greenbush official.");
                        console.log(data);
                    },
                    complete: function (data) {
                        $(".ajax-loader").hide();
                        //A function to be called when the request finishes 
                        // (after success and error callbacks are executed). 
                    }
                });
            }
        });

        // attach event
        // fires when an item in the building list uder modal popup is clicked.
        $('body').on('click', '.building-group-item', function () {
            $(this).toggleClass('active');
        });

        //////////////////////////////////////////////////////
        //
        // Events for adding the hieracial classes
        //
        /////////////////////////////////////////////////////

        initHref();

        // attach event
        // event is fired when hiearachy view is clicked
        function initHref() {
            $('div.list-group-item').each(function (index) { 

                $(this).not('.bound').addClass('bound').on("click", function (e) { // one sweet bit of code.

                    //if (!($(e.target).is('i') || $(e.target).is('text'))) { return; } // only fire if the name or the icon was clicked.
                    if (!$(e.target).is('i')) { return; }  // only fire if the icon was clicked.

                    var div = $(this);
                    var userId = div.data("id");

                    if (div.next().hasClass('list-group')) {
                        div.toggleClass('subactivated');
                        div.next().toggle();
                    }
                    else {
                        if ($(e.target).hasClass("clickEventDisabled")) { return; }

                        $(e.target).addClass("clickEventDisabled");
                    }
                });
            });
        }
    }
    init();

    //SET UP FOR TRANSITIONS
    var params =	//All params are optional, you can just assign {} 
    {
        "navB": "slide",	//Effect for navigation button, leave it empty to disable it
        "but": true,		//Flag to enable transitions on button, false by default
        "cBa": function () { init(); }	//callback function
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
            if ($(this).hasClass('img-circle')) {
                $(this).toggleClass('img-selection-correction');
            }
            else {
                $(this).next().toggleClass('img-selection-correction');
            }

            $(this).parent().parent().toggleClass("active");
            listrap.trigger("selection-changed", [listrap.getSelection()]);
        };

        $(listrap).find(toggle + "img").on("click", selectionChanged);
        $(listrap).find(toggle + "span").on("click", selectionChanged);
        return listrap;
    }
});