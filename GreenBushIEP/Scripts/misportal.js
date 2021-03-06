﻿$(function () {

    function init() {
        $(".studentIEPDetails").each(function (index, element) {
            var contentElementId = $(element).data().target;
            var contentHtml = $(contentElementId).html();
            $(element).popover({
                content: contentHtml,
                html: true,
                trigger: 'hover'

            });
        });

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

            if ($("#evalsDueCount").length > 0) {
                if (message != "") {
                    message += " and ";
                }
                message += "(" + $("#evalsDueCount").val() + ") 3 Year Evals Coming Due"
            }

            if (message != "") {
                message = "<button type='button' class='close' aria-label='Close'><span aria-hidden='true'>&times;</span></button><a href='#' class='notification' onclick='_showNotifications();' /><i class='fa fa-bell' title='You have pending actions' data-toggle='tooltip'></i> You have " + message + "</a>";
                _showAlert(message, false);
            }
        }

        $("#notificationBtn").on("click", function () {
            _showNotifications();
            return false;
        });

        $("#iepsEvalListBtn").on("click", function () {
            $("#iepsEvalList").fadeToggle("fast", "linear");
        });

        $("#iepsDueListBtn").on("click", function () {
            $("#iepsDueList").fadeToggle("fast", "linear");
        });

        $("#iepsDraftListBtn").on("click", function () {
            $("#iepsDraftList").fadeToggle("fast", "linear");
        });

        $(".startIEP, .dashboardIEP").on("click", function () {
            //open iep like this to prevent true false button switches from not working right in firefox
            var stid = $(this).attr("data-id");
            window.location.href = '/Home/StudentProcedures?stid=' + stid;
            return false;
        });

        $(".chosen-select").chosen({
            width: "95%",
            disable_search_threshold: 10
        });

        //$('[data-toggle="popover"]').popover({
        //    html: true,
        //    title: "Student Notes <i class='pull-right glyphicon glyphicon-remove-circle'></i>",
        //    trigger: "click",
        //    content: "<p>this is someone's note here<br>-MJ 03-25-2020</p><hr/><textarea rows='4' cols='30' placeholder='Have something to say about the student? Add a note.'></textarea>",
        //    template: "<div class='popover' role='tooltip'><div class='arrow'></div><h3 class='popover-title'></h3><div class='popover-content'></div><center><a href='#' class='btn btn-class'>Add Note <i class='glyphicon glyphicon-plus'></i></a></center></div>"
        //});

        $(".tip-auto").tooltip({
            placement: 'auto',
            html: true
        });

        // filter to only active students
        var filterCollection = $(".list-group-root").find(".list-group-item");

        $(".btn-filter").on("click", function () {
            $(".showFilters").toggleClass("hidden");
        });

        // attach event
        // fires when an delete button is pressed on a MIS role.
        $("#deleteUser").on("show.bs.modal", function (e) {
            var user = $(e.relatedTarget).data("id");
            $(e.currentTarget).find("input[name='id']").val(user);
            $("#confirmDeletion").val("");
        });

        // attach event
        // fires when you click yes on the module deleteForm.
        $("#deleteUser button[type=submit]").on("click", function (e) {
            if ($("#confirmDeletion").val() === "DELETE") {
                var userId = $(e.currentTarget).parent().parent().find("input[name='id']").val();

                $.ajax({
                    type: "POST",
                    url: "/Manage/Delete",
                    data: {
                        id: userId
                    },
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
                        $("#alertMessage .moreinfo").html("Unable to connect to the server or another related problem.");
                        $("#alertMessage").fadeTo(2000, 500).slideUp(500, function () {
                            $("#alertMessage").slideUp(500);
                        });
                    }
                });
            }
        });

        // attach event
        // fires when an start button in pressed on a student IEP
        $("#initIEP").on("show.bs.modal", function (e) {
            var user = $(e.relatedTarget).data("id");
            $(e.currentTarget).find("input[name='id']").val(user);
            $("#confirmStart").val("");
        });

        // attach event
        // fires when you click start on the module startInit form.
        $("#initIEP button[type=submit]").on("click", function (e) {
            if ($("#confirmStart").val() === "START") {
                var userId = $(e.currentTarget).parent().parent().find("input[name='id']").val();

                $(".ajax-loader").show();

                $.ajax({
                    type: "GET",
                    url: "/Home/UnlockStudentIEP",
                    data: {
                        stid: userId
                    },
                    async: false,
                    success: function (data) {
                        if (data.Result === "success") {
                            var container = document.querySelector(".list-group-root");
                            var matchFound = container.querySelectorAll("div[data-id='" + userId + "']");
                            if (matchFound[0] != null) {
                                var button = matchFound[0].querySelector("button");
                                $(button).replaceWith("<a href='/Home/StudentProcedures?stid=" + userId + "' title='Launch the IEP for this student' role='button' data-ftrans='slide' class='btn btn-info btn-action pull-right startIEP'><span class='glyphicon glyphicon-log-out'></span></a>");
                            }
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
        // fires when the MIS chooses a user
        $("#filterName").change(function () {
            var userId = this.value;
            var selectedDistrict = $("#userDistricts option:selected").val() + "";
            var selectedBuilding = $("#userBuildings option:selected").val() + "";
            var selectedRole = $("#userRoles option:selected").val() + "";

            $(".ajax-filter").removeClass("hidden");
            $("#filterMyUserList").submit();

            //$(".ajax-loader").show();

            //$.ajax({
            //    type: "POST",
            //    url: "/Manage/FilterUserList",
            //    dataType: "json",
            //    data: {
            //        DistrictId: selectedDistrict,
            //        BuildingId: selectedBuilding,
            //        RoleId: selectedRole,
            //        userId: userId
            //    },
            //    async: false,
            //    success: function (data) {
            //        if (data.Result === "success") {

            //            var results = data.Message;
            //            if (results.members.length > 0) {
            //                filterList(results.members);
            //            }
            //            else {
            //                hideAllUsers(null);
            //            }
            //        } else {
            //            alert("doh");
            //        }
            //    },
            //    error: function (data) {
            //        alert("Not connected to the network!");

            //        console.log(data);
            //    },
            //    complete: function (data) {
            //        $(".ajax-loader").hide();
            //        //A function to be called when the request finishes 
            //        // (after success and error callbacks are executed). 
            //    }
            //});
        });

        // attach event
        // fires when the MIS chooses a district
        $("#userDistricts").on("change", function () {
            var selectedDistrict = $(this).val() + "";
            var selectedBuilding = $("#userBuildings option:selected").val() + "";
            var selectedRole = $("#userRoles option:selected").val() + "";

            $(".ajax-filter").removeClass("hidden");
            $("#filterMyUserList").submit();

           // $(".ajax-loader").show();

            //$.ajax({
            //    type: "POST",
            //    url: "/Manage/FilterUserList",
            //    dataType: "json",
            //    data: {
            //        DistrictId: selectedDistrict,
            //        BuildingId: selectedBuilding,
            //        RoleId: selectedRole
            //    },
            //    async: false,
            //    success: function (data) {
            //        if (data.Result === "success") {

            //            var results = data.Message;
            //            if (results.members.length > 0) {
            //                filterList(results.members);
            //            } else {
            //                hideAllUsers(null);
            //            }

            //            if (results.buildings != null && results.buildings.length > 0) {
            //                filterBuildingList(results.buildings);
            //            }


            //        } else {
            //            alert("doh");
            //        }
            //    },
            //    error: function (data) {
            //        alert("ERROR!!!");

            //        console.log(data);
            //    },
            //    complete: function (data) {
            //        $(".ajax-loader").hide();
            //        //A function to be called when the request finishes 
            //        // (after success and error callbacks are executed). 
            //    }
            //});
        });

        // attach event
        // fires when the MIS chooses a building
        $("#userBuildings").on("change", function () {
            var selectedDistrict = $("#userDistricts option:selected").val() + "";
            var selectedBuilding = $(this).val() + "";
            var selectedRole = $("#userRoles option:selected").val() + "";

            $(".ajax-filter").removeClass("hidden");
            $("#filterMyUserList").submit();

            //$(".ajax-loader").show();

            //$.ajax({
            //    type: "POST",
            //    url: "/Manage/FilterUserList",
            //    dataType: "json",
            //    data: {
            //        DistrictId: selectedDistrict,
            //        BuildingId: selectedBuilding,
            //        RoleId: selectedRole
            //    },
            //    async: false,
            //    success: function (data) {
            //        if (data.Result === "success") {

            //            var results = data.Message;
            //            if (results.members.length > 0) {
            //                filterList(results.members);
            //            } else {
            //                hideAllUsers(null);
            //            }
            //        } else {
            //            alert("doh");
            //        }
            //    },
            //    error: function (data) {
            //        alert("ERROR!!!");

            //        console.log(data);
            //    },
            //    complete: function (data) {
            //        $(".ajax-loader").hide();
            //        //A function to be called when the request finishes 
            //        // (after success and error callbacks are executed). 
            //    }
            //});
        });

        // attach event
        // fires when the MIS chooses a role
        $("#userRoles").on("change", function () {
            var selectedDistrict = $("#userDistricts option:selected").val() + "";
            var selectedBuilding = $("#userBuildings option:selected").val() + "";
            var selectedRole = $(this).val() + "";
            var selectedStatus = $("#statusActive option:selected").val() + "";

            $(".ajax-filter").removeClass("hidden");
            $("#filterMyUserList").submit();

/*            $(".ajax-loader").show();*/

            if (selectedRole === "5") {
                $(".activeIEPCol").removeClass("hidden");
            } else {
                $(".activeIEPCol").addClass("hidden");
            }

            //$.ajax({
            //    type: "POST",
            //    url: "/Manage/FilterUserList",
            //    dataType: "json",
            //    data: {
            //        DistrictId: selectedDistrict,
            //        BuildingId: selectedBuilding,
            //        RoleId: selectedRole,
            //        statusActive: selectedStatus
            //    },
            //    async: false,
            //    success: function (data) {
            //        if (data.Result === "success") {

            //            var results = data.Message;
            //            if (results.members.length > 0) {
            //                filterList(results.members);
            //            } else {
            //                hideAllUsers(null);
            //            }
            //        } else {
            //            alert("doh");
            //        }
            //    },
            //    error: function (data) {
            //        alert("ERROR!!!");

            //        console.log(data);
            //    },
            //    complete: function (data) {
            //        $(".ajax-loader").hide();
            //        //A function to be called when the request finishes 
            //        // (after success and error callbacks are executed). 
            //    }
            //});
        });

        // attach event
        // fires when the MIS chooses active/inactive
        $("#statusActive").change(function () {

            var selectedDistrict = $("#userDistricts option:selected").val() + "";
            var selectedBuilding = $("#userBuildings option:selected").val() + "";
            var selectedRole = $("#userRoles option:selected").val() + "";
            var selectedActive = $("#filterActive option:selected").val() + "";
            var selectedStatus = this.value;

            $(".ajax-filter").removeClass("hidden");
            $("#filterMyUserList").submit();

            //$(".ajax-loader").show();

            //$.ajax({
            //    type: "POST",
            //    url: "/Manage/FilterUserList",
            //    dataType: "json",
            //    data: {
            //        DistrictId: selectedDistrict,
            //        BuildingId: selectedBuilding,
            //        RoleId: selectedRole,
            //        activeType: selectedActive,
            //        statusActive: selectedStatus
            //    },
            //    async: false,
            //    success: function (data) {
            //        if (data.Result === "success") {

            //            var results = data.Message;
            //            if (results.members.length > 0) {
            //                filterList(results.members);
            //            } else {
            //                hideAllUsers(null);
            //            }
            //        } else {
            //            alert("doh");
            //        }
            //    },
            //    error: function (data) {
            //        alert("Not connected to the network!");

            //        console.log(data);
            //    },
            //    complete: function (data) {
            //        $(".ajax-loader").hide();
            //        //A function to be called when the request finishes 
            //        // (after success and error callbacks are executed). 
            //    }
            //});
        });

        // attach event
        // fires when the MIS chooses active/inactive
        $("#filterActive").change(function () {

            var selectedDistrict = $("#userDistricts option:selected").val() + "";
            var selectedBuilding = $("#userBuildings option:selected").val() + "";
            var selectedRole = $("#userRoles option:selected").val() + "";
            var selectedActive = this.value;
            var selectedStatus = $("#statusActive option:selected").val() + "";

            $(".ajax-filter").removeClass("hidden");
            $("#filterMyUserList").submit();

            //$(".ajax-loader").show();

            //$.ajax({
            //    type: "POST",
            //    url: "/Manage/FilterUserList",
            //    dataType: "json",
            //    data: {
            //        DistrictId: selectedDistrict,
            //        BuildingId: selectedBuilding,
            //        RoleId: selectedRole,
            //        activeType: selectedActive,
            //        statusActive: selectedStatus
            //    },
            //    async: false,
            //    success: function (data) {
            //        if (data.Result === "success") {

            //            var results = data.Message;
            //            if (results.members.length > 0) {
            //                filterList(results.members);
            //            } else {
            //                hideAllUsers(null);
            //            }
            //        } else {
            //            alert("doh");
            //        }
            //    },
            //    error: function (data) {
            //        alert("Not connected to the network!");

            //        console.log(data);
            //    },
            //    complete: function (data) {
            //        $(".ajax-loader").hide();
            //        //A function to be called when the request finishes 
            //        // (after success and error callbacks are executed). 
            //    }
            //});
        });

        // attach event
        // fires where the button on an alert message is clicked
        $("#alertMessage button").on("click", function (e) {
            $(e.currentTarget).parent().hide();
        });

        //////////////////////////////////////////////////////
        //
        // Events for adding and removing an MIS from a building.
        //
        /////////////////////////////////////////////////////

        // attach event
        // fires when clicking the "checkbox" on the 'add a building' modal popup.
        $(".dual-list .selector").click(function () {
            var $checkBox = $(this);
            if (!$checkBox.hasClass("selected")) {
                $checkBox.addClass("selected").closest(".well").find("ul li:not(.active)").addClass("active");
                $checkBox.children("i").removeClass("glyphicon-unchecked").addClass("glyphicon-check");
            } else {
                $checkBox.removeClass("selected").closest(".well").find("ul li.active").removeClass("active");
                $checkBox.children("i").removeClass("glyphicon-check").addClass("glyphicon-unchecked");
            }
        });

        // attack event
        // fired off when edit a building is clicked
        $("#assignBuilding").on("show.bs.modal", function (e) {
            // assign the userid to the id value in the form.
            var userId = $(e.relatedTarget).data("id");
            $(e.currentTarget).find("input[name='id']").val(userId);

            $.ajax({
                type: "GET",
                url: "/Manage/GetDistricts",
                data: {
                    id: userId
                },
                dataType: "json",
                async: false,
                success: function (data) {
                    if (data.Result === "success") {
                        var buildings = data.Message;
                        $("#selectedDistrict").find("option").remove();
                        $.each(buildings, function (key, value) {
                            // throw away the key. It's simply an index counter for the returned array.
                            $("#selectedDistrict").find("option").end().append($("<option></option>").attr("value", value.USD).text(value.DistrictName));
                        });

                        // fire off the selected building event by selecting an option in the newly created list.
                        $("#selectedDistrict option:first-child").attr("selected", "selected").change();
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
        $("[name='SearchBuildingList']").keyup(function (e) {
            var code = e.keyCode || e.which;
            if (code === "9") return;
            if (code === "27") $(this).val(null);
            var $rows = $(this).closest(".dual-list").find(".list-group li");
            var val = $.trim($(this).val()).replace(/ +/g, ' ').toLowerCase();
            $rows.show().filter(function () {
                var text = $(this).text().replace(/\s+/g, ' ').toLowerCase();
                return !~text.indexOf(val);
            }).hide();
        });

        // attach event
        // fires when we select a different district in the "add building" modal popup
        $("#selectedDistrict").on("change", function () {
            var districtId = $("#selectedDistrict option:selected").val();
            var user = $("#assignBuilding").find("input[name='id']").val();

            $.ajax({
                type: "GET",
                url: "/Manage/GetBuildings",
                dataType: "json",
                data: {
                    id: districtId,
                    userId: user
                },
                async: false,
                success: function (data) {
                    if (data.Result === "success") {

                        // blow away the list
                        $("#selectBuildings").empty();

                        // rebuild the list with the new data.
                        var buildings = data.Message;
                        $.each(buildings, function () {
                            $("#selectBuildings").append("<li class='list-group-item building-group-item' data-id='" + this.BuildingID + "'><i class='glyphicon glyphicon-home'></i>&nbsp;" + this.BuildingName + "</li>");
                        });
                    } else {
                        console.log(data.Message);
                    }
                },
                error: function (data) { }
            });
        });

        // attach event
        // fires when the save button in the 'add building' modal popup is clicked.
        $("#savetheseBuildingsToThisUser").on("click", function (e) {
            e.preventDefault();
            var districtId = $("#selectedDistrict option:selected").val();
            var userId = $("#assignBuilding").find("input[name='id']").val();
            var listOfBuildings = [];

            $.each($("#selectBuildings li.active"), function (key, value) {
                var buildingId = $(this).data("id");
                listOfBuildings.push(buildingId);
            });

            if (listOfBuildings.length > 0) {
                $.ajax({
                    type: "GET",
                    url: "/Manage/SaveBuildingsToUser",
                    dataType: "json",
                    traditional: true,
                    data: {
                        USD: districtId,
                        userId: userId,
                        buildings: listOfBuildings
                    },
                    async: false,
                    success: function (data) {
                        if (data.Result === "success") {
                            $("#alertMessage").removeClass("alert alert-danger").addClass("alert alert-info").hide();
                            $("#alertMessage").addClass("alert alert-info animated fadeInUp");
                            $("#alertMessage .moreinfo").html("The user was successfully added to the buildings");
                        } else {
                            $("#alertMessage").removeClass("alert alert-info").show();
                            $("#alertMessage").addClass("alert alert-danger animated fadeInUp");
                            $("#alertMessage .moreinfo").html(data.Message);
                        }
                    },
                    error: function (data) {
                        $("#alertMessage").removeClass("alert alert-info").show();
                        $("#alertMessage").addClass("alert alert-danger animated fadeInUp");
                        $("#alertMessage .moreinfo").html(data.Message);
                    }
                });
            }
        });

        // attach event
        // fires when an item in the building list uder modal popup is clicked.
        $("body").on("click", ".building-group-item", function () {
            $(this).toggleClass("active");
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
});

function filterList(members) {

    let studentDropDown = $("#filterName");
    studentDropDown.find('option').remove().end();
    studentDropDown.append($('<option>', { value: "-1" }).text("All Users"));

    //console.time();
   
    let container = document.querySelector(".list-group-root");
    hideAllUsers(container);

    let j = members.length;
    while (j--) {
        var matchFound = container.querySelectorAll("div[data-id='" + members[j].UserID + "']");
        if (matchFound[0] != null) {

            if (members[j].RoleID == 5) {
                const lastName = members[j].LastName == null ? "" : members[j].LastName;
                const firstName = members[j].FirstName == null ? "" : members[j].FirstName;
                const middleName = members[j].MiddleName == null ? "" : members[j].MiddleName;
                const kidsID = members[j].KidsID == null ? "" : members[j].KidsID;

                const studentName = lastName + ", " + firstName + " " + middleName + " - " + kidsID;
                studentDropDown
                    .append($('<option>', { value: members[j].UserID })
                        .text(studentName));
            }

            matchFound[0].classList.remove("hidden");
        }
    }

    //console.timeEnd();

    studentDropDown.trigger("chosen:updated");
}

function hideAllUsers(container) {
    // hides all the users in the list.

    // if null was sent then query the root
    if (container == null) {
        container = document.querySelector(".list-group-root");
    }

    const filterCollection = container.querySelectorAll(".list-group-item");
    let i = filterCollection.length;
    while (i--) {
        filterCollection[i].classList.add("hidden");
    }
}

function filterBuildingList(buildingList) {
    var buildingElement = $("#userBuildings");
    buildingElement.find('option').remove().end();

    var buildingHtml = buildingElement.html();
    buildingHtml += "<option value='-1'>All</option>";

    //district only
    $.each(buildingList, function (key, value) {
        buildingHtml += "<option data-district='" + value.BuildingUSD + "' value='" + value.BuildingID + "'>" + value.BuildingName + "</option>";
    });

    buildingElement.html(buildingHtml);
}

function _showNotifications() {
    $("#dashboardNotification").modal();

}

function _showAlert(message, positive) {

    var successFade = 9000;
    $("#alertMessage").removeClass("hidden");

    if (positive) {
        $("#alertMessage").removeClass("alert alert-danger").addClass("alert alert-info");
        $("#alertMessage").addClass("alert alert-info animated fadeInUp");
        $("#alertMessage .moreinfo").html(message);
    }
    else {
        $("#alertMessage").removeClass("alert alert-info").show();
        $("#alertMessage").addClass("alert alert-danger animated fadeInUp");
        $("#alertMessage").html(message);
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
            if ($(this).hasClass("img-circle")) {
                $(this).toggleClass("img-selection-correction");
            } else {
                $(this).next().toggleClass("img-selection-correction");
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