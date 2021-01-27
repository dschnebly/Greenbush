$(function () {

    $(".chosen-select").chosen({
        width: "95%"
    });

    $(".btn-filter").on("click", function () {
        $(".showFilters").toggleClass("hidden");
    });

    $("#filterName").on("change", function () {
        var userId = $(this).val();

        if (userId == -1) {

            container = document.querySelector(".list-group-root");
            var filterCollection = container.querySelectorAll(".list-group-item");
            var i = filterCollection.length;
            while (i--) {
                filterCollection[i].classList.remove("hidden");
            }
        } else {
            var data = [];
            data.push({ UserID: userId });

            filterList(data);
        }
    });

    $("#userRoles").on("change", function () {

        var selectedLocation = $("#userDistricts option:selected").val();
        var selectedRole = $(this).val();
        var selectedProgram = $("#userProgram option:selected").val();
        var selectedArchive = $("#statusActive option:selected").val();

        $.ajax({
            type: "POST",
            url: "/ILP/FilterUserList",
            dataType: "json",
            data: {
                LocationId: selectedLocation,
                RoleId: selectedRole,
                ProgramId: selectedProgram,
                Archived: selectedArchive
            },
            async: false,
            success: function (data) {
                if (data.Result === "success") {

                    var results = data.Message;
                    if (results.length > 0) {
                        filterList(results);
                    } else {
                        hideAllUsers(null);
                    }
                }
            },
            error: function (data) {
                alert("Not connected to the network!");
                console.log(data);
            },
            complete: function (data) {

            }
        });
    });

    $("#userDistricts").on("change", function () {

        var selectedLocation = $(this).val();
        var selectedRole = $("#userRoles option:selected").val();
        var selectedProgram = $("#userProgram option:selected").val();
        var selectedArchive = $("#statusActive option:selected").val();

        $.ajax({
            type: "POST",
            url: "/ILP/FilterUserList",
            dataType: "json",
            data: {
                LocationId: selectedLocation,
                RoleId: selectedRole,
                ProgramId: selectedProgram,
                Archived: selectedArchive
            },
            async: false,
            success: function (data) {
                if (data.Result === "success") {

                    var results = data.Message;
                    if (results.length > 0) {
                        filterList(results);
                    } else {
                        hideAllUsers(null);
                    }
                }
            },
            error: function (data) {
                alert("Not connected to the network!");
                console.log(data);
            },
            complete: function (data) {

            }
        });
    });

    $("#userProgram").on("change", function () {

        var selectedLocation = $("#userDistricts option:selected").val();
        var selectedRole = $("#userRoles option:selected").val();
        var selectedProgram = $(this).val();
        var selectedArchive = $("#statusActive option:selected").val();

        $.ajax({
            type: "POST",
            url: "/ILP/FilterUserList",
            dataType: "json",
            data: {
                LocationId: selectedLocation,
                RoleId: selectedRole,
                ProgramId: selectedProgram,
                Archived: selectedArchive
            },
            async: false,
            success: function (data) {
                if (data.Result === "success") {

                    var results = data.Message;
                    if (results.length > 0) {
                        filterList(results);
                    } else {
                        hideAllUsers(null);
                    }
                }
            },
            error: function (data) {
                alert("Not connected to the network!");
                console.log(data);
            },
            complete: function (data) {

            }
        });
    });

    $("#statusArchived").on("change", function () {

        var selectedLocation = $("#userDistricts option:selected").val();
        var selectedRole = $("#userRoles option:selected").val();
        var selectedProgram = $("#userProgram option:selected").val();
        var selectedArchive = $(this).val();

        $.ajax({
            type: "POST",
            url: "/ILP/FilterUserList",
            dataType: "json",
            data: {
                LocationId: selectedLocation,
                RoleId: selectedRole,
                ProgramId: selectedProgram,
                Archived: selectedArchive
            },
            async: false,
            success: function (data) {
                if (data.Result === "success") {

                    var results = data.Message;
                    if (results.length > 0) {
                        filterList(results);
                    } else {
                        hideAllUsers(null);
                    }
                }
            },
            error: function (data) {
                alert("Not connected to the network!");
                console.log(data);
            },
            complete: function (data) {

            }
        });
    });

    $(".startILP").on("click", function () {
        //open iep like this to prevent true false button switches from not working right in firefox
        var stid = $(this).attr("data-id");
        window.location.href = '/ILP/LearnerProcedures?stid=' + stid;
        return false;
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
                url: "/Manage/DeleteILPUser",
                data: {
                    id: userId
                },
                dataType: "json",
                success: function (data) {
                    debugger;
                    var currentUser = $("div.list-group-item[data-id='" + userId + "']");
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

    function filterList(members) {
        var container = document.querySelector(".list-group-root");

        hideAllUsers(container);

        var j = members.length;
        while (j--) {
            var matchFound = container.querySelectorAll("div[data-id='" + members[j].UserID + "']");
            if (matchFound[0] != null) {
                matchFound[0].classList.remove("hidden");
            }
        }
    }

    function hideAllUsers(container) {
        // hide all the users in the list.

        if (container == null) {
            container = document.querySelector(".list-group-root");
        }

        var filterCollection = container.querySelectorAll(".list-group-item");
        var i = filterCollection.length;
        while (i--) {
            filterCollection[i].classList.add("hidden");
        }
    }
});