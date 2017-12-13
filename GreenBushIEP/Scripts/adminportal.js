﻿$(function () {
    function init() {
        // attach Event
        // fires when a user clicks on the main new system user button
        $("#user-toggle .user-toggle-item button").on("click", function () {
            $(this).parent().find("ul").toggleClass("show-buttons hide-buttons");
        })

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
        // fires when you click yes on the module confirmRemoval.
        $('#confirmRemoval button[type=submit]').on('click', function (e) {
            var userId = $(e.currentTarget).parent().parent().find('input[name="id"]').val();

            $.ajax({
                type: 'POST',
                url: '/Manage/RemoveFromList',
                data: { id: userId },
                dataType: "json",
                success: function (data) {
                    var currentUser = $("div.list-group-item[data-id='" + userId + "']");
                    $(currentUser).remove();

                    if ($("#alertMessage").css('display') && $("#alertMessage").css('display') === 'none') {
                        $("#alertMessage .moreinfo").html(data.Message);
                        $("#alertMessage").fadeTo(3000, 500).slideUp(500, function () {
                            $("#alertMessage").slideUp(500);
                        });
                    }
                },
                error: function (data) {
                    console.log("Unable to connect to the server or another related problem.");
                }
            });
        });

        // attach event
        // fires when you click the "avaliable teachers" button
        $('#addExistingTeacher').on('click', function (e) {
            var userId = $(e.currentTarget).data("id");

            $.ajax({
                type: 'GET',
                url: '/Home/AvailableTeachers',
                data: { id: userId },
                dataType: 'html',
                success: function (data) {
                    if (data.length !== 0) {
                        $('ul.listrap').html(data);
                        $(".listrap").listrap().getSelection();
                        $('#addExistingTeacherModal').modal('show');
                    }
                    else {
                        $("#alertMessage .moreinfo").html('Server Error');
                        $("#alertMessage").fadeTo(3000, 500).slideUp(500, function () {
                            $("#alertMessage").slideUp(500);
                        });
                    }
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
        // fires when the "add" button is clicked on the Available Users module.
        $('#addAvailableTeachers').on('click', function (e) {
            var activeTeachers = $(".listrap").listrap().getSelection();
            var teacherIds = [];

            if (activeTeachers.length > 0) {
                $.each(activeTeachers, function (index, value) {
                    teacherIds.push($(activeTeachers[index]).find('.ourTeacher').data("id"));
                });
                $.ajax({
                    type: 'POST',
                    url: '/Home/AddTeachers',
                    data: JSON.stringify(teacherIds),
                    dataType: "json",
                    contentType: 'application/json; charset=utf-8',
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

            $('#addExistingTeacherModal').modal('hide');
        });

        //////////////////////////////////////////////////////
        //
        // Events for Filtering the Search Results
        //
        /////////////////////////////////////////////////////

        // attach event
        // fires when clicking the search icon.
        $('[data-command="toggle-my-admin-search"]').on('click', function (event) {
            event.preventDefault;

            if ($(this).hasClass('hide-search')) {
                $(this).removeClass('hide-search');
                $('.c-my-search').closest('.row').slideUp(100);
            }
            else {
                $('.c-my-search').closest('.row').slideDown(100);
                $(this).addClass('hide-search');
            }
        });

        // attach event
        // fires when the user is searching
        $('[name="contact-list-search"]').keyup(function (e) {
            var val = $(e.currentTarget).val();
            var code = e.keyCode || e.which;
            if (code === '9') return;
            if (code === '27') $(this).val(null);

            var teachers = $('div.list-group-root').find('div.list-group-item');
            teachers.show().filter(function () {
                var text = $(this).text().replace(/\s+/g, ' ').toLowerCase();
                return !~text.indexOf(val.toLowerCase());
            }).hide();
        });

        // attach event
        // fires when user select a filter option
        $(".filteroptions > li").on('click', function () {
            $(".filteroptions >li").removeClass("selected-filter");
            $(this).addClass("selected-filter");
            var filter = $(this).find(".filteroption").text();

            $('.filteredby').removeClass("filteredby");
            switch (filter) {
                case "Show Teachers Only":
                    $.each($('div.list-group-item > i.fa-child'), function () {
                        $(this).parent().addClass("filteredby");
                    });
                    break;
                case "Show Students Only":
                    $.each($('.list-group-item > i.fa-graduation-cap'), function () {
                        $(this).parent().addClass("filteredby");
                        $(this).parent().nextAll('.list-group').addClass("filteredby");
                    });
                    break;
                default:
                    break;
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
                async: false,
                success: function (data) {
                    if (data.Result === "success") {
                        var buildings = data.Message;

                        $.each(buildings, function (key, value) {
                            // throw away the key. It's simply an index counter for the returned array.
                            $("#selectedDistrict").find('option').remove().end().append($("<option></option>").attr("value", value.USD).text(value.DistrictName));
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
                    if (!($(e.target).is('i'))) { return; }  // only fire if the icon was clicked.

                    var div = $(this);
                    var userId = div.data("id");

                    if (div.next().hasClass('list-group')) {
                        div.toggleClass('subactivated');
                        $(div).find("i:first-child").toggleClass("fa-minus-square-o fa-plus-square-o");
                        div.next().toggle();
                    }
                    else {
                        $.ajax({
                            type: 'GET',
                            url: '/Home/GetOrganization',
                            data: { id: userId },
                            dataType: 'html',
                            success: function (data) {
                                if ($.trim(data).length !== 0) {
                                    $(div).find("i:first-child").toggleClass("fa-minus-square-o fa-plus-square-o");
                                    $(div).toggleClass('subactivated');
                                    $(data).insertAfter(div);
                                    initHref();
                                }
                                else {
                                    $(div).find("i.fa-plus-square-o").removeClass().addClass("empty-icon");
                                }
                            },
                            error: function (data) {
                                console.log('error: ' + data);
                            }
                        });
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
            // -- begin modification --
            if ($(this).hasClass('img-circle')) {
                $(this).toggleClass('img-selection-correction');
            }
            else {
                $(this).next().toggleClass('img-selection-correction');
            }
            // -- end modification --
            $(this).parent().parent().toggleClass("active");
            listrap.trigger("selection-changed", [listrap.getSelection()]);
        };
        $(listrap).find(toggle + "img").on("click", selectionChanged);
        $(listrap).find(toggle + "span").on("click", selectionChanged);
        return listrap;
    }
});