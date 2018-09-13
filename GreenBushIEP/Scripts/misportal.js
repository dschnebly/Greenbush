$(function () {
    function init() {

        // attach event
        // fires when an delete button is pressed on a MIS role.
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
                        $("#alertMessage .moreinfo").html("Unable to connect to the server or another related problem.");
                        $("#alertMessage").fadeTo(2000, 500).slideUp(500, function () {
                            $("#alertMessage").slideUp(500);
                        });
                    }
                });
            }
        });

        // attach event
        // fires when the user chooses a district
        $("#userDistricts").on('change', function () {
            var selectedDistrict = $(this).val();
            
            $("#alertMessage").removeClass('alert alert-info').hide();
            if (selectedDistrict != -1) {
                $("#userBuildings option").remove();

                $("#AllUserBuildings > option").each(function () {
                    if ($(this).data('district') == selectedDistrict) {
                        $("#userBuildings").append('<option value="' + $(this).val() + '">' + $(this).text() + '</option>');
                    }
                });

                if ($("#userBuildings > option").length == 0)
                {
                    $("#userBuildings").append('<option value="-1">All Buildings</option>');
                    $("#userBuildings").prop("disabled", true);

                    $("#alertMessage").removeClass('alert alert-info').show();
                    $("#alertMessage").addClass("alert alert-danger animated fadeInUp");
                    $("#alertMessage .moreinfo").html('There are no buildings assigned to you in this district.');

                    return;
                }

                $('.list-group-item').each(function () {
                    var districts = $(this).data('districts') + "";
                    var hasDistricts = districts.split(",").indexOf(selectedDistrict) != -1;
                    
                    if (hasDistricts) {
                        $(this).show();
                    } else {
                        $(this).hide();
                    }
                });

                $("#userBuildings").prop("disabled", false);
            }
            else {
                $("#userBuildings").append('<option value="-1">All Buildings</option>');
                $("#userBuildings").prop("disabled", true);

                $('.list-group-item').each(function () {
                    $(this).show();
                });
            }
        });

        // attach event
        // fires when the user chooses a building
        $("#userBuildings").on('change', function () {
            var selectedBuilding = $(this).val();
            var selectedDistrict = $("#userDistricts").val();

            $('.list-group-item').each(function () {
                var districts = $(this).data('districts') + "";
                var buildings = $(this).data('buildings') + "";
                var hasDistricts = districts.split(",").indexOf(selectedDistrict) != -1;
                var hasBuildings = buildings.split(",").indexOf(selectedBuilding) != -1;

                if(hasDistricts && hasBuildings) {
                    $(this).show();
                }
                else {
                    $(this).hide();
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
                        $(div).find("i:first-child").toggleClass("fa-minus-square-o fa-plus-square-o");
                        div.next().toggle();
                    }
                    else {
                        if ($(e.target).hasClass("clickEventDisabled")) { return; }

                        $(e.target).addClass("clickEventDisabled");

                        //$.ajax({
                        //    type: 'GET',
                        //    url: '/Home/GetOrganization',
                        //    data: { id: userId },
                        //    dataType: 'html',
                        //    success: function (data) {
                        //        if ($.trim(data).length !== 0) {
                        //            $(div).find("i:first-child").toggleClass("fa-minus-square-o fa-plus-square-o");
                        //            $(data).insertAfter(div);
                        //            initHref();

                        //            $(e.target).removeClass("clickEventDisabled");

                        //        }
                        //        else {
                        //            $(div).find("i:first-child").removeClass().addClass("empty-icon");
                        //        }
                        //    },
                        //    error: function (data) {

                        //    }
                        //});
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