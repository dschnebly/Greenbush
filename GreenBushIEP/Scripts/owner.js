﻿$(function () {
    function init() {

        // attach Event
        // fires when a user clicks on the main new system user button
        $("#user-toggle .user-toggle-item button").on("click", function () {
            $(this).parent().find("ul").toggleClass("show-buttons hide-buttons");
        });

        // attach event
        // fires when an delete button is pressed.
        $('#deleteUser').on('show.bs.modal', function (e) {

            var user = $(e.relatedTarget).data('id');
            $(e.currentTarget).find('input[name="id"]').val(user);
            $('#confirmDeletion').val('');
        });

        // attach event
        // fires when the user chooses a district
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

                        // blow away the building list 
                        $('#userBuildings').empty();
                        // blow away the user list
                        document.getElementsByClassName('list-group-root')[0].innerHTML = "";

                        var results = data.Message;
                        $('#userBuildings').append('<option value="-1">All Buildings</option>');
                        if (results.buildings.length > 0) {
                            $.each(results.buildings, function (index, value) {
                                console.log(value);
                                $('#userBuildings').append('<option value="' + value.BuildingID + '">' + value.BuildingName + '</option>');
                            });
                        }

                        if (results.members.length > 0) {
                            $.each(results.members, function (index, value) {
                                switch (value.RoleID) {
                                    case "2":
                                        $('.list-group-root').append('<div class="list-group-item" data-id="' + value.UserID + '"><i class="fa fa-cog" aria-hidden="true"></i>&nbsp;<text>' + value.FirstName + ' ' + value.LastName + '</text><button type="button" class="btn btn-info btn-action pull-right" data-id="' + value.UserID + '" data-toggle="modal" data-target="#deleteUser"><span class="glyphicon glyphicon glyphicon-trash"></span></button><button type="button" class="btn btn-info btn-action pull-right" data-id="' + value.UserID + '" data-toggle="modal" data-target="#assignBuilding"><span class="fa fa-building-o" aria-hidden="true"></span></button><a href="/Manage/Edit/' + value.UserID + '" title="Edit an existing user" role="button" data-toggle="tooltip" class="btn btn-info pull-right edit-btn" data-ftrans="slide"><span class="glyphicon glyphicon-pencil"></span></a></div>')
                                        break;
                                    case "3":
                                        $('.list-group-root').append('<div class="list-group-item" data-id="' + value.UserID + '"><i class="fa fa-user-o" aria-hidden="true"></i> <text>' + value.FirstName + ' ' + value.LastName + '</text><button type="button" class="btn btn-info btn-action pull-right" data-id="' + value.UserID + '" data-toggle="modal" data-target="#deleteUser"><span class="glyphicon glyphicon glyphicon-trash"></span></button><button type="button" class="btn btn-info btn-action pull-right" data-id="' + value.UserID + '" data-toggle="modal" data-target="#assignBuilding"><span class="fa fa-building-o" aria-hidden="true"></span></button><a href="/Manage/Edit/' + value.UserID + '" title="Edit an existing user" role="button" data-toggle="tooltip" class="btn btn-info pull-right edit-btn" data-ftrans="slide"><span class="glyphicon glyphicon-pencil"></span></a></div>');
                                        break;
                                    case "4":
                                        $('.list-group-root').append('<div class="list-group-item" data-id="' + value.UserID + '"><i class="fa fa-graduation-cap" aria-hidden="true"></i><a href="/Home/TeacherStudentsRole/' + value.UserID + '" class="launchListOfStudents" data-ftrans="slide"> <text>' + value.FirstName + ' ' + value.LastName + '</text></a><button type="button" class="btn btn-info btn-action pull-right" data-id="' + value.UserID + '" data-toggle="modal" data-target="#deleteUser"><span class="glyphicon glyphicon glyphicon-trash"></span></button><button type="button" class="btn btn-info btn-action pull-right" data-id="' + value.UserID + '" data-toggle="modal" data-target="#assignBuilding"><span class="fa fa-building-o" aria-hidden="true"></span></button><a href="/Manage/Edit/' + value.UserID + '" title="Edit an existing user" role="button" data-toggle="tooltip" class="btn btn-info pull-right edit-btn" data-ftrans="slide"><span class="glyphicon glyphicon-pencil"></span></a></div>');
                                        break;
                                    case "5":
                                        $('.list-group-root').append('<div class="list-group-item bound" data-id="' + value.UserID + '"><i class="fa fa-child" aria-hidden="true"></i> <text>' + value.FirstName + ' ' + value.LastName + '</text><a href="/Home/StudentProcedures?stid=' + value.UserID + '" title="Lauch the IEP for this student" role="button" data-ftrans="slide" class="btn btn-info btn-action pull-right startIEP"><span class="glyphicon glyphicon-log-out"></span></a><button type="button" class="btn btn-info btn-action pull-right" data-id="' + value.UserID + '" data-toggle="modal" data-target="#deleteUser"><span class="glyphicon glyphicon glyphicon-trash"></span></button><a href="/Manage/EditStudent/' + value.UserID + '" title="Edit an existing student" role="button" data-toggle="tooltip" class="btn btn-info pull-right edit-btn" data-ftrans="slide"><span class="glyphicon glyphicon-pencil"></span></a></div>');
                                        break;
                                }
                            });
                        }

                        initHref();
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
        // fires when the user chooses a building
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

                        // blow away the user list
                        document.getElementsByClassName('list-group-root')[0].innerHTML = "";

                        var results = data.Message;
                        if (results.members.length > 0) {
                            $.each(results.members, function (index, value) {
                                switch (value.RoleID) {
                                    case "2":
                                        $('.list-group-root').append('<div class="list-group-item" data-id="' + value.UserID + '"><i class="fa fa-cog" aria-hidden="true"></i>&nbsp;<text>' + value.FirstName + ' ' + value.LastName + '</text><button type="button" class="btn btn-info btn-action pull-right" data-id="' + value.UserID + '" data-toggle="modal" data-target="#deleteUser"><span class="glyphicon glyphicon glyphicon-trash"></span></button><button type="button" class="btn btn-info btn-action pull-right" data-id="' + value.UserID + '" data-toggle="modal" data-target="#assignBuilding"><span class="fa fa-building-o" aria-hidden="true"></span></button><a href="/Manage/Edit/' + value.UserID + '" title="Edit an existing user" role="button" data-toggle="tooltip" class="btn btn-info pull-right edit-btn" data-ftrans="slide"><span class="glyphicon glyphicon-pencil"></span></a></div>')
                                        break;
                                    case "3":
                                        $('.list-group-root').append('<div class="list-group-item" data-id="' + value.UserID + '"><i class="fa fa-user-o" aria-hidden="true"></i> <text>' + value.FirstName + ' ' + value.LastName + '</text><button type="button" class="btn btn-info btn-action pull-right" data-id="' + value.UserID + '" data-toggle="modal" data-target="#deleteUser"><span class="glyphicon glyphicon glyphicon-trash"></span></button><button type="button" class="btn btn-info btn-action pull-right" data-id="' + value.UserID + '" data-toggle="modal" data-target="#assignBuilding"><span class="fa fa-building-o" aria-hidden="true"></span></button><a href="/Manage/Edit/' + value.UserID + '" title="Edit an existing user" role="button" data-toggle="tooltip" class="btn btn-info pull-right edit-btn" data-ftrans="slide"><span class="glyphicon glyphicon-pencil"></span></a></div>');
                                        break;
                                    case "4":
                                        $('.list-group-root').append('<div class="list-group-item" data-id="' + value.UserID + '"><i class="fa fa-graduation-cap" aria-hidden="true"></i><a href="/Home/TeacherStudentsRole/' + value.UserID + '" class="launchListOfStudents" data-ftrans="slide"> <text>' + value.FirstName + ' ' + value.LastName + '</text></a><button type="button" class="btn btn-info btn-action pull-right" data-id="' + value.UserID + '" data-toggle="modal" data-target="#deleteUser"><span class="glyphicon glyphicon glyphicon-trash"></span></button><button type="button" class="btn btn-info btn-action pull-right" data-id="' + value.UserID + '" data-toggle="modal" data-target="#assignBuilding"><span class="fa fa-building-o" aria-hidden="true"></span></button><a href="/Manage/Edit/' + value.UserID + '" title="Edit an existing user" role="button" data-toggle="tooltip" class="btn btn-info pull-right edit-btn" data-ftrans="slide"><span class="glyphicon glyphicon-pencil"></span></a></div>');
                                        break;
                                    case "5":
                                        $('.list-group-root').append('<div class="list-group-item bound" data-id="' + value.UserID + '"><i class="fa fa-child" aria-hidden="true"></i> <text>' + value.FirstName + ' ' + value.LastName + '</text><a href="/Home/StudentProcedures?stid=' + value.UserID + '" title="Lauch the IEP for this student" role="button" data-ftrans="slide" class="btn btn-info btn-action pull-right startIEP"><span class="glyphicon glyphicon-log-out"></span></a><button type="button" class="btn btn-info btn-action pull-right" data-id="' + value.UserID + '" data-toggle="modal" data-target="#deleteUser"><span class="glyphicon glyphicon glyphicon-trash"></span></button><a href="/Manage/EditStudent/' + value.UserID + '" title="Edit an existing student" role="button" data-toggle="tooltip" class="btn btn-info pull-right edit-btn" data-ftrans="slide"><span class="glyphicon glyphicon-pencil"></span></a></div>');
                                        break;
                                }
                            });
                        }

                        initHref();
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
        // fires when the user chooses a role
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

                        // blow away the user list
                        document.getElementsByClassName('list-group-root')[0].innerHTML = "";

                        var results = data.Message;
                        if (results.members.length > 0) {
                            $.each(results.members, function (index, value) {
                                switch (value.RoleID) {
                                    case "2":
                                        $('.list-group-root').append('<div class="list-group-item" data-id="' + value.UserID + '"><i class="fa fa-cog" aria-hidden="true"></i>&nbsp;<text>' + value.FirstName + ' ' + value.LastName + '</text><button type="button" class="btn btn-info btn-action pull-right" data-id="' + value.UserID + '" data-toggle="modal" data-target="#deleteUser"><span class="glyphicon glyphicon glyphicon-trash"></span></button><button type="button" class="btn btn-info btn-action pull-right" data-id="' + value.UserID + '" data-toggle="modal" data-target="#assignBuilding"><span class="fa fa-building-o" aria-hidden="true"></span></button><a href="/Manage/Edit/' + value.UserID + '" title="Edit an existing user" role="button" data-toggle="tooltip" class="btn btn-info pull-right edit-btn" data-ftrans="slide"><span class="glyphicon glyphicon-pencil"></span></a></div>')
                                        break;
                                    case "3":
                                        $('.list-group-root').append('<div class="list-group-item" data-id="' + value.UserID + '"><i class="fa fa-user-o" aria-hidden="true"></i> <text>' + value.FirstName + ' ' + value.LastName + '</text><button type="button" class="btn btn-info btn-action pull-right" data-id="' + value.UserID + '" data-toggle="modal" data-target="#deleteUser"><span class="glyphicon glyphicon glyphicon-trash"></span></button><button type="button" class="btn btn-info btn-action pull-right" data-id="' + value.UserID + '" data-toggle="modal" data-target="#assignBuilding"><span class="fa fa-building-o" aria-hidden="true"></span></button><a href="/Manage/Edit/' + value.UserID + '" title="Edit an existing user" role="button" data-toggle="tooltip" class="btn btn-info pull-right edit-btn" data-ftrans="slide"><span class="glyphicon glyphicon-pencil"></span></a></div>');
                                        break;
                                    case "4":
                                        $('.list-group-root').append('<div class="list-group-item" data-id="' + value.UserID + '"><i class="fa fa-graduation-cap" aria-hidden="true"></i><a href="/Home/TeacherStudentsRole/' + value.UserID + '" class="launchListOfStudents" data-ftrans="slide"> <text>' + value.FirstName + ' ' + value.LastName + '</text></a><button type="button" class="btn btn-info btn-action pull-right" data-id="' + value.UserID + '" data-toggle="modal" data-target="#deleteUser"><span class="glyphicon glyphicon glyphicon-trash"></span></button><button type="button" class="btn btn-info btn-action pull-right" data-id="' + value.UserID + '" data-toggle="modal" data-target="#assignBuilding"><span class="fa fa-building-o" aria-hidden="true"></span></button><a href="/Manage/Edit/' + value.UserID + '" title="Edit an existing user" role="button" data-toggle="tooltip" class="btn btn-info pull-right edit-btn" data-ftrans="slide"><span class="glyphicon glyphicon-pencil"></span></a></div>');
                                        break;
                                    case "5":
                                        $('.list-group-root').append('<div class="list-group-item bound" data-id="' + value.UserID + '"><i class="fa fa-child" aria-hidden="true"></i> <text>' + value.FirstName + ' ' + value.LastName + '</text><a href="/Home/StudentProcedures?stid=' + value.UserID + '" title="Lauch the IEP for this student" role="button" data-ftrans="slide" class="btn btn-info btn-action pull-right startIEP"><span class="glyphicon glyphicon-log-out"></span></a><button type="button" class="btn btn-info btn-action pull-right" data-id="' + value.UserID + '" data-toggle="modal" data-target="#deleteUser"><span class="glyphicon glyphicon glyphicon-trash"></span></button><a href="/Manage/EditStudent/' + value.UserID + '" title="Edit an existing student" role="button" data-toggle="tooltip" class="btn btn-info pull-right edit-btn" data-ftrans="slide"><span class="glyphicon glyphicon-pencil"></span></a></div>');
                                        break;
                                }
                            });
                        }

                        initHref();
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
        // Events for Filtering the Search Results
        //
        /////////////////////////////////////////////////////

        //// attach event
        //// fires when clicking the search icon.
        //$('[data-command="toggle-my-admin-search"]').on('click', function (event) {
        //    event.preventDefault;

        //    if ($(this).hasClass('hide-search')) {
        //        $(this).removeClass('hide-search');
        //        $('.c-my-search').closest('.row').slideUp(100);
        //    }
        //    else {
        //        $('.c-my-search').closest('.row').slideDown(100);
        //        $(this).addClass('hide-search');
        //    }
        //});

        //// attach event
        //// fires when the user is searching
        //$('[name="contact-list-search"]').keyup(function (e) {
        //    var val = $(e.currentTarget).val();
        //    var code = e.keyCode || e.which;
        //    if (code === '9') return;
        //    if (code === '27') $(this).val(null);

        //    var teachers = $('div.list-group-root').find('div.list-group-item');
        //    teachers.show().filter(function () {
        //        var text = $(this).text().replace(/\s+/g, ' ').toLowerCase();
        //        return !~text.indexOf(val.toLowerCase());
        //    }).hide();
        //});

        //// attach event
        //// fires when user select a filter option
        //$(".filteroptions > li").on('click', function () {
        //    $(".filteroptions >li").removeClass("selected-filter");
        //    $(this).addClass("selected-filter");
        //    var filter = $(this).find(".filteroption").text();

        //    $('.filteredby').removeClass("filteredby");
        //    switch (filter) {
        //        case "Show Teachers Only":
        //            $.each($('div.list-group-item > i.fa-child'), function () {
        //                $(this).parent().addClass("filteredby");
        //            });
        //            break;
        //        case "Show Students Only":
        //            $.each($('.list-group-item > i.fa-graduation-cap'), function () {
        //                $(this).parent().addClass("filteredby");
        //                $(this).parent().nextAll('.list-group').addClass("filteredby");
        //            });
        //            break;
        //        default:
        //            break;
        //    }
        //});

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