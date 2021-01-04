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