$(document).ready(function () {

    init();
    initContacts();

    // attach event
    // fires when an avatar is uploaded
    $("#adminpersona").on('change', function (e) {
        var oFReader = new FileReader();
        oFReader.readAsDataURL(document.getElementById('adminpersona').files[0]);

        oFReader.onload = function (oFREvent) {
            document.getElementById("avatarImage").src = oFREvent.target.result;
        };
    });

    // hooks up the dates on safari and explorer with a datepicker.
    $("#IEPBeginDate").datepicker({
        dateFormat: "mm/dd/yy"
    }).datepicker("setDate", "0");
    $("#IEPEndDate").datepicker({
        dateFormat: "mm/dd/yy"
    }).datepicker("setDate", "0");

    $("#assignChildCount").on('change', function (e) {
        var optionSelected = $("option:selected", this);
        var valueSelected = this.value;

        var optionExists = $("#misDistrict option[value=" + valueSelected + "]").length > 0;
        if (optionExists) {
            var currentValues = $("#misDistrict").val();
            currentValues.push(valueSelected);
            $("#misDistrict").val(currentValues);
            $("#misDistrict").trigger("change");
            $("#misDistrict").trigger("chosen:updated");
        }
    });
});

function init() {

    $(".studentDOB").datepicker({
        dateFormat: "mm/dd/yy",
        changeMonth: true,
        changeYear: true
    });

    $(".chosen-select").chosen({
        disable_search_threshold: 10,
        no_results_text: "Oops, nothing found!",
        width: "100%"
    });

    $("#submitForm").on('click', function () {
        document.forms[3].submit();
    });

    $('#buildingIds').hide();
    var link = document.createElement("a");
    link.href = "/Scripts/bootstrap-mutliselect.js";

    // loads the building script
    $.getScript(link.protocol + "//" + link.host + link.pathname + link.search + link.hash, function () { });

    //Initialize tooltips
    $('.nav-tabs > li a[title]').tooltip();

    //Wizard
    $('a[data-toggle="tab"]').on('show.bs.tab', function (e) {
        var $target = $(e.target);
        if ($target.parent().hasClass('disabled')) {
            return false;
        }
    });

    $(".prev-step").click(function (e) {
        var $active = $('.wizard .nav-tabs li.active');
        prevTab($active);
    });

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

    $('#misDistrict').change(function (e) {
        var districtIds = '';
        var districtNums = new Array();
        var districtArr = $("#misDistrict").val();

        if (districtArr.length > 0) {
            for (i = 0; i < districtArr.length; i++) {
                var districtAdd = districtArr[i];
                districtNums.push(districtAdd);
            }
            districtIds = districtNums.join(',');
        }

        var args = { ids: districtIds };

        $(".info").show();
        // current options html
        var responsibleBuildingElement = $('.districtOnly');
        var neighborhoodBuildingElement = $('.allActive');

        $.ajax({
            type: 'GET',
            url: '/Manage/GetBuildingsByDistrictId',
            data: args,
            dataType: "json",
            async: false,
            success: function (data) {
                if (data.Result === "success") {
                    var buildings = data.DistrictBuildings;
                    var activeBuildings = data.ActiveBuildings;
                    $(".studentBuilding").find('option').remove().end();

                    var responsibleBuilding = responsibleBuildingElement.html();
                    var neighborhoodBuilding = neighborhoodBuildingElement.html();

                    //district only
                    $.each(buildings, function (key, value) {
                        responsibleBuilding += "<option value='" + value.BuildingID + "'>" + value.BuildingName + " (" + value.BuildingID + ")" + "</option>";
                        neighborhoodBuilding += "<option value='" + value.BuildingID + "'>" + value.BuildingName + " (" + value.BuildingID + ")" + "</option>";
                    });

                    //now add all active 
                    $.each(activeBuildings, function (key, value) {
                        neighborhoodBuilding += "<option value='" + value.BuildingID + "'>" + value.BuildingName + " (" + value.BuildingID + ")" + "</option>";
                    });

                    responsibleBuildingElement.html(responsibleBuilding);
                    neighborhoodBuildingElement.html(neighborhoodBuilding);

                    responsibleBuildingElement.trigger("change");
                    responsibleBuildingElement.trigger("chosen:updated");

                    neighborhoodBuildingElement.trigger("change");
                    neighborhoodBuildingElement.trigger("chosen:updated");
                }
            },
            error: function (data) {
                alert("There was an error retrieving the building information.");
                console.log(data);
            },
            complete: function (data) {
                $(".info").hide();
            }
        });
    });

    $('#Is_Gifted').click(function () {
        if ($(this).is(':checked')) {
            $('#claimingCode').prop('checked', false);
        }
        else {
            $('#claimingCode').prop('checked', true);
        }
    });

    function nextTab(elem) {
        $(elem).next().find('a[data-toggle="tab"]').click();
    }
    function prevTab(elem) {
        $(elem).prev().find('a[data-toggle="tab"]').click();
    }
}


function tabValidates() {
    var validates = true;
    var stepId = $('.wizard .nav-tabs li.active a').attr('href');
    var $inputs = $(stepId + " :input[data-validate='true']");

    $inputs.each(function () {
        var input = $(this);
        var is_valid = input.val();
        console.log(input);
        console.log(input.val());
        if (is_valid === "" || is_valid === null) {
            if (input.is("select")) {
                $(this).next().addClass('contact-tooltip');
                input.addClass('contact-tooltip');
            }
            else {
                input.addClass('contact-tooltip');
            }
            validates = false;
        }
        else {
            input.removeClass('contact-tooltip');
        }
    });

    return validates;
}

function initContacts() {

    $('.add-contact').each(function (index) {

        $(this).not('.bound').addClass('bound').on("click", function (e) {
            if ($(this).find('i').hasClass("glyphicon-plus")) {

                // clone and unhide the contact template.
                var newContact = $("#contact-template").clone().removeAttr("id").removeAttr("style").addClass("student-contact").appendTo("#student-contacts");
                //var count = $("#student-contacts .student-contact").length; //$("#numberOfContacts").val();

                // new contact id.
                //newContact.find("#relationshipId").val(count).attr("name", "contacts[#].relationshipId").attr("id", "contacts[#].relationshipId");
                newContact.html(newContact.html().replace(/\[#\]/g, '[' + ++index + ']'));
                newContact.find(".contact-button").removeClass("contact-button").addClass("add-contact").removeClass("btn-info").addClass('btn-danger');

                // rebind the recently added contact.
                initContacts();

                return false;
            } else {
                $(this).unbind("click").parents(".student-contact").fadeOut(300, function () {
                    $(this).remove();
                });
            }
        });

    });
}

$("#next2").on("click", function () {

    var theForm = document.getElementById("createNewStudent");

    if (tabValidates()) {

        $.ajax({
            url: '/Manage/CreateStudent',
            type: 'POST',
            data: $("#createNewStudent").serialize(),
            success: function (data) {
                if (data.Result === "success") {

                    var $active = $('.wizard .nav-tabs li.active');
                    $active.addClass('disabled');
                    $active.next().removeClass('disabled');
                    $($active).next().find('a[data-toggle="tab"]').click();

                    // create a new student id and add it to the contacts form here.
                    $("form:eq(1)").find("input[name='studentId']").val(data.Message);
                    $("form:eq(3)").find("input[name='studentId']").val(data.Message);
                } else {

                    alert(data.Message);
                }
            },
            error: function (data) {
                alert("There was an error when attempt to connect to the server.");
            }
        });

    }
});

$("#next3").on("click", function () {

    var theForm = document.getElementById("createStudentContacts");

    if (tabValidates()) {

        $.ajax({
            url: '/Manage/CreateStudentOptions',
            type: 'POST',
            data: $("#createStudentOptions").serialize(),
            success: function (data) {
                if (data.Result === "success") {

                    var $active = $('.wizard .nav-tabs li.active');
                    $active.next().removeClass('disabled');
                    $($active).next().find('a[data-toggle="tab"]').click();

                    // add student id to the avatar form here.
                    $("form:eq(2)").find("input[name='studentId']").val(data.Message);
                } else {

                    alert(data.Message);
                }
            },
            error: function (data) {
                alert("There was an error when attempt to connect to the server.");
            }
        });

    }
});

$("#next4").on("click", function () {

    var theForm = document.getElementById("editStudentContacts");

    if (tabValidates()) {
        $.ajax({
            url: '/Manage/CreateStudentContacts',
            type: 'POST',
            data: $("#createStudentContacts").serialize(),
            success: function (data) {
                if (data.Result === "success") {

                    var $active = $('.wizard .nav-tabs li.active');
                    $active.next().removeClass('disabled');
                    $($active).next().find('a[data-toggle="tab"]').click();
                } else {

                    alert(data.Message);
                }
            },
            error: function (data) {
                alert("There was an error when attempt to connect to the server.");
            }
        });

    }
});

$("#next5").on("click", function () {

    var theForm = document.getElementById("editStudent");
    if (tabValidates()) {

        $.ajax({
            url: '/Manage/EditStudent',
            type: 'POST',
            data: $("#editStudent").serialize(),
            success: function (data) {
                if (data.Result === "success") {
                    var $active = $('.wizard .nav-tabs li.active');
                    $active.next().removeClass('disabled');
                    $($active).next().find('a[data-toggle="tab"]').click();

                } else {

                    alert(data.Message);
                }
            },
            error: function (data) {
                alert("There was an error when attempt to connect to the server.");
            }
        });

    }
});

$("#next6").on("click", function () {

    var theForm = document.getElementById("editStudentOptions");

    if (tabValidates()) {

        $.ajax({
            url: '/Manage/EditStudentOptions',
            type: 'POST',
            data: $("#editStudentOptions").serialize(),
            success: function (data) {
                if (data.Result === "success") {

                    var $active = $('.wizard .nav-tabs li.active');
                    $active.next().removeClass('disabled');
                    $($active).next().find('a[data-toggle="tab"]').click();
                } else {

                    alert(data.Message);
                }
            },
            error: function (data) {
                alert("There was an error when attempt to connect to the server.");
            }
        });

    }
});

$("#next7").on("click", function () {

    var theForm = document.getElementById("editStudentContacts");

    if (tabValidates()) {

        $.ajax({
            url: '/Manage/EditStudentContacts',
            type: 'POST',
            data: $("#editStudentContacts").serialize(),
            success: function (data) {
                if (data.Result === "success") {

                    var $active = $('.wizard .nav-tabs li.active');
                    $active.next().removeClass('disabled');
                    $($active).next().find('a[data-toggle="tab"]').click();
                } else {

                    alert(data.Message);
                }
            },
            error: function (data) {
                alert("There was an error when attempt to connect to the server.");
            }
        });

    }
});