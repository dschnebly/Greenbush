$(function () {

    function init() {
        $(".chosen-select").chosen({ width: "95%", disable_search_threshold: 10 });

        // filter to only active students
        var filterCollection = $('.list-group-root').find('.list-group-item');

        $(".btn-filter").on("click", function () {
            $(".showFilters").toggleClass("hidden");
        });

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
        // fires when the MIS chooses a user
        $("#filterName").change(function () {
            var userId = this.value;
            var selectedDistrict = $("#userDistricts option:selected").val() + "";
            var selectedBuilding = $("#userBuildings option:selected").val() + "";
            var selectedRole = $("#userRoles option:selected").val() + "";

            $(".ajax-loader").show();

            $.ajax({
                type: "POST",
                url: "/Manage/FilterUserList",
                dataType: "json",
                data: {
                    DistrictId: selectedDistrict,
                    BuildingId: selectedBuilding,
                    RoleId: selectedRole,
                    userId: userId
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
        // fires when the MIS chooses a district
        $("#userDistricts").on("change", function () {
            var selectedDistrict = $(this).val() + "";
            var selectedBuilding = $("#userBuildings option:selected").val() + "";
            var selectedRole = $("#userRoles option:selected").val() + "";

            $(".ajax-loader").show();

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
        // fires when the MIS chooses a building
        $("#userBuildings").on("change", function () {
            var selectedDistrict = $("#userDistricts option:selected").val() + "";
            var selectedBuilding = $(this).val() + "";
            var selectedRole = $("#userRoles option:selected").val() + "";

            $(".ajax-loader").show();

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
        // fires when the MIS chooses a role
        $("#userRoles").on("change", function () {
            var selectedDistrict = $("#userDistricts option:selected").val() + "";
            var selectedBuilding = $("#userBuildings option:selected").val() + "";
            var selectedRole = $(this).val() + "";
            var selectedStatus = $("#statusActive option:selected").val() + "";

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
                    RoleId: selectedRole,
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
        // fires when an item in the building list under modal popup is clicked.
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
            $(".launchListOfStudents").on("click", function (e) {
                e.preventDefault();

                $(".ajax-loader").show();
                $(".ajax-loader img").show();
            });

            $("div.list-group-item").each(function (index) { // one sweet bit of code.

                $(this).not(".bound").addClass("bound").on("click", function (e) {

                    if (!($(e.target).is("i") || $(e.target).is("text"))) {
                        return;
                    } // only fire if the name or the icon was clicked.

                    var div = $(this);
                    var userId = div.data("id");

                    if (div.next().hasClass("list-group")) {
                        div.toggleClass("subactivated");
                        div.next().toggle();
                    } else {
                        if ($(e.target).hasClass("clickEventDisabled")) {
                            return;
                        }

                        $(e.target).addClass("clickEventDisabled");
                    }
                });
            });
        }

        function filterListInactive(selectedDistrict, selectedBuilding, selectedRole) {

            $.ajax({
                type: "POST",
                url: "/Manage/FilterUserListInactive",
                data: {
                    DistrictId: selectedDistrict,
                    BuildingId: selectedBuilding,
                    RoleId: selectedRole
                },
                dataType: "html",
                success: function (response) {

                    $('.inactive-list').html(response);
                    var container = document.querySelector(".list-group-root");
                    $(".inactive-list").removeClass("hidden");
                    $(container).addClass("hidden");
                },
                failure: function (response) {
                    alert("There was a problem loading the users.");
                },
                error: function (response) {
                    alert("There was a problem loading the users.");
                },
                complete: function (data) {
                    $(".ajax-loader").hide();
                }
            });
        }
    }
    init();

    //SET UP FOR TRANSITIONS
    var params =	//All params are optional, you can just assign {} 
    {
        "navB": "slide",	//Effect for navigation button, leave it empty to disable it
        "but": true,		//Flag to enable transitions on button, false by default
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

    var studentDropDown = $("#filterName");
    studentDropDown.find('option').remove().end();
    studentDropDown
        .append($('<option>', {
            value: "-1"
        })
            .text("All Users"));

    var j = members.length - 1;
    while (j >= 0) {
        var roleId = members[j].RoleID;
        console.log(members[j].UserID);
        var matchFound = document.querySelectorAll("div [data-id='" + members[j].UserID + "']");
        console.log(matchFound);
        if (matchFound[0] != null) {
            matchFound[0].classList.remove("hidden");
        }

        if (roleId == 5) {
            var lastName = members[j].LastName == null ? "" : members[j].LastName;
            var firstName = members[j].FirstName == null ? "" : members[j].FirstName;
            var middleName = members[j].MiddleName == null ? "" : members[j].MiddleName;
            var kidsID = members[j].KidsID == null ? "" : members[j].KidsID;

            var studentName = lastName + ", " + firstName + " " + middleName + " - " + kidsID;
            studentDropDown
                .append($('<option>', { value: members[j].UserID })
                    .text(studentName));
        }

        j--;
    }

    studentDropDown.trigger("chosen:updated");
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

window.addEventListener('DOMContentLoaded', function () { $(".ajax-loader").hide(); });