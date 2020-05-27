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

        $(".btn-filter").on("click", function () {
            $(".showFilters").toggleClass("hidden");
        });

        // attach event
        // fires when an delete button is pressed next to listed user.
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
        // fires when user-x is pressed.
        $('#confirmRemoval').on('show.bs.modal', function (e) {
            var user = $(e.relatedTarget).data('id');
            $(e.currentTarget).find('input[name="id"]').val(user);
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
				url: '/Manage/FilterUserList',
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
		// fires when the admin chooses a user
		$('#filterName').change(function () {
			var userId = this.value;
			var selectedDistrict = $("#userDistricts option:selected").val() + "";
			var selectedBuilding = $("#userBuildings option:selected").val() + "";
			var selectedRole = $("#userRoles option:selected").val() + "";

			$(".ajax-loader").show();

			$.ajax({
			    type: 'POST',
			    url: '/Manage/FilterUserList',
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
	    // fires when the admin chooses a district
        $("#userDistricts").on('change', function () {
            var selectedDistrict = $(this).val() + "";
            var selectedBuilding = $("#userBuildings option:selected").val() + "";
            var selectedRole = $("#userRoles option:selected").val() + "";

            $(".ajax-loader").show();

            $.ajax({
                type: 'POST',
                url: '/Manage/FilterUserList',
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
	    // fires when the admin chooses a building
        $("#userBuildings").on('change', function () {
            var selectedDistrict = $("#userDistricts option:selected").val() + "";
            var selectedBuilding = $(this).val() + "";
            var selectedRole = $("#userRoles option:selected").val() + "";

            $(".ajax-loader").show();

            $.ajax({
                type: 'POST',
                url: '/Manage/FilterUserList',
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
        // fires when the MIS chooses a role
        $("#userRoles").on("change", function () {
            var selectedDistrict = $("#userDistricts option:selected").val() + "";
            var selectedBuilding = $("#userBuildings option:selected").val() + "";
            var selectedRole = $(this).val() + "";

            $(".ajax-loader").show();

            if (selectedRole === "5") {
                $(".activeIEPCol").removeClass("hidden");
            } else {
                $(".activeIEPCol").addClass("hidden");
            }

            $.ajax({
                type: "POST",
                url: "/Manage/FilterUserList",
                dataType: "json",
                data: {
                    DistrictId: selectedDistrict,
                    BuildingId: selectedBuilding,
                    RoleId: selectedRole
                },
                async: false,
                success: function (data) {
                    if (data.Result === "success") {

                        var results = data.Message;
                        if (results.members.length > 0) {
                            filterList(results.members);
                        }
                    } else {
                        alert("doh");
                    }
                },
                error: function (data) {
                    alert("ERROR!!!");

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
        // fires when the MIS chooses active/inactive
        $("#statusActive").change(function () {

            var selectedDistrict = $("#userDistricts option:selected").val() + "";
            var selectedBuilding = $("#userBuildings option:selected").val() + "";
            var selectedRole = $("#userRoles option:selected").val() + "";
            var selectedActive = $("#filterActive option:selected").val() + "";
            var selectedStatus = this.value;

            $(".ajax-loader").show();

            $.ajax({
                type: "POST",
                url: "/Manage/FilterUserList",
                dataType: "json",
                data: {
                    DistrictId: selectedDistrict,
                    BuildingId: selectedBuilding,
                    RoleId: selectedRole,
                    activeType: selectedActive,
                    statusActive: selectedStatus
                },
                async: false,
                success: function (data) {
                    if (data.Result === "success") {

                        var results = data.Message;
                        if (results.members.length > 0) {
                            filterList(results.members);
                        }
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
        // fires when the MIS chooses active/inactive
        $("#filterActive").change(function () {

            var selectedDistrict = $("#userDistricts option:selected").val() + "";
            var selectedBuilding = $("#userBuildings option:selected").val() + "";
            var selectedRole = $("#userRoles option:selected").val() + "";
            var selectedActive = this.value;
            var selectedStatus = $("#statusActive option:selected").val() + "";

            $(".ajax-loader").show();

            $.ajax({
                type: "POST",
                url: "/Manage/FilterUserList",
                dataType: "json",
                data: {
                    DistrictId: selectedDistrict,
                    BuildingId: selectedBuilding,
                    RoleId: selectedRole,
                    activeType: selectedActive,
                    statusActive: selectedStatus
                },
                async: false,
                success: function (data) {
                    if (data.Result === "success") {

                        var results = data.Message;
                        if (results.members.length > 0) {
                            filterList(results.members);
                        }
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
        // fires where the button on an alert message is clicked
        $("#alertMessage button").on("click", function (e) {
            $(e.currentTarget).parent().hide();
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
            $(".launchListOfStudents").on('click', function (e) {
                e.preventDefault();

                $(".ajax-loader").show();
                $(".ajax-loader img").show();
            });

            $('div.list-group-item').each(function (index) { // one sweet bit of code.

                $(this).not('.bound').addClass('bound').on("click", function (e) {

                    if (!($(e.target).is('i') || $(e.target).is('text'))) { return; } // only fire if the name or the icon was clicked.

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

function filterList(members) {
    var container = document.querySelector(".list-group-root");

    // hide all the users in the list.
    var filterCollection = container.querySelectorAll(".list-group-item");
    var i = filterCollection.length - 1;
    while (i >= 0) {
        filterCollection[i].classList.add("hidden");
        i--;
    }

    var j = members.length - 1;
    while (j >= 0) {
        var matchFound = container.querySelectorAll("div[data-id='" + members[j].UserID + "']");
        if (matchFound[0] != null) {
            matchFound[0].classList.remove("hidden");
        }
        j--;
    }
}

// once the page is fully loaded, hide the ajax loading icon.
document.addEventListener('readystatechange', event => {
    if (event.target.readyState === "complete") {
        $(".ajax-loader").hide();
    }
});