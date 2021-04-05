// jquery onload equivalent
$(function () {

    function init() {

        // If needsPlan is on the planning module than we need to pop that up before doing ANYTHING else.
        if ($("#modal-studentPlanning").hasClass('needsPlan')) {
            $("#modal-studentPlanning").modal('show');
        }

        /* tooltips */
        $('[data-toggle="tooltip"]').tooltip({
            trigger: 'manual'
        });

        $('.tooltip-help').on('click', function () {
            $('[data-toggle="tooltip"]').tooltip('toggle');
        });


        if ($('ul#iepStatusList li').length === 0) {
            $('ul#iepStatusList').hide();
        }

        $('#modal-studentTeamGoals').on('hidden.bs.modal', function () {
            $('[data-toggle="tooltip"]').tooltip('hide');
        });

        $('#modal-studentGoals').on('hidden.bs.modal', function () {
            $('[data-toggle="tooltip"]').tooltip('hide');
        });

        $('#modal-studentStrategies').on('hidden.bs.modal', function () {
            $('[data-toggle="tooltip"]').tooltip('hide');
        });

        $('#modal-studentAccommodations').on('hidden.bs.modal', function () {
            $('[data-toggle="tooltip"]').tooltip('hide');
        });
        /* end tooltips */

        // Attach Event
        // fires when the "form" evaluation consent button was clicked. *if the teacher wants to reprint the consent form
        $('#EvaluationConsent').not('.bound').addClass('bound').on("click", function (e) {
            var id = getUrlParameter('stid');

            $('.ajax-loader').css("visibility", "visible");
            window.location.href = '/Home/RequestConsent/' + id;
        });

        // Attach Event
        // fires when the "form" notice of meeting button was clicked.
        $('#NoticeOfMeeting').not('.bound').addClass('bound').on("click", function (e) {
            var id = getUrlParameter('stid');
            window.location.href = '/Home/NoticeOfMeeting/' + id;
        });

        // Attach Event
        // fires when the user prints the IEP
        $('#printIEP').not('.bound').addClass('bound').on("click", function (e) {
            var stid = getUrlParameter('stid');
            var iepId = $("#studentIEPId").val();
            window.location.href = '/Home/PrintIEP/?stid=' + stid + '&iepId=' + iepId;
        });

        // Attach Event
        // fires when the user clicks the button to view student info
        $('#printStudentInfo').not('.bound').addClass('bound').on("click", function (e) {
            var stid = getUrlParameter('stid');
            var iepId = $("#studentIEPId").val();
            window.location.href = '/Home/PrintStudentInfo/?stid=' + stid + '&iepId=' + iepId;
        });

        // Attach Event
        // fires when the user clicks the button to see the student forms
        $('#viewForms').not('.bound').addClass('bound').on("click", function (e) {
            $('.ajax-loader').css("visibility", "visible");
            var stid = getUrlParameter('stid');
            location.href = "/Home/IEPFormModule?studentId=" + stid + "&home=true";
        });

        $('.navbar-toggle').click(function () {
            $('.navbar-nav').toggleClass('slide-in');
            $('.side-body').toggleClass('body-slide-in');
            $('#search').removeClass('in').addClass('collapse').slideUp(200);
            $('.absolute-wrapper').toggleClass('slide-in');
        });

        // Attach Event
        // when the user clicks the Save Dates button
        $("#IEPDates").on("click", function () {
            var stId = $("#stid").val();
            var iepId = $("#studentIEPId").val();
            var startDate = $("#IEPBeginDate").val().toString('MM/dd/yyyy');
            var meetingDate = $("#IEPMeetingDate").val().toString('MM/dd/yyyy');
            var birthDate = $("#studentBirthDate").val();

            var a = new Date(startDate);
            var b = new Date(a);
            var c = new Date(birthDate);
            b.setDate(a.getDate() + 365); // the endDate is 365 days from the startDate
            var daysToStart = (a - c) / 1000 / 60 / 60 / 24;
            var daysToEnd = (b - c) / 1000 / 60 / 60 / 24;
            var startDiff = parseInt(daysToStart / 365);
            var endDiff = parseInt(daysToEnd / 365);

            if (Number.isNaN(startDiff) || Number.isNaN(endDiff)) {
                $("#IEPBeginDate").addClass("date-error");

                return alert("The Annual Initiation Date was not in the correct format or some other related problem. Please try again.");
            }

            //If the student is over 21 or under 3, notify the teacher but let them save regardless.
            if (startDiff < 3) {
                $("#IEPBeginDate").addClass("date-error");

                if (!confirm('The student will be younger than 3 when this IEP starts. Please be aware that this could be an issue.')) {
                    return;
                }
            }

            if (endDiff > 21) {
                $("#IEPBeginDate").addClass("date-error");

                if (!confirm('The student will be older than 21 when this IEP ends. Please be aware that this could be an issue.')) {
                    return;
                }
            }

            var dayStart = a.getUTCDay(); //start date
            var dayEnd = b.getUTCDay(); //end date
            // Days in JS range from 0-6 where 0 is Sunday and 6 is Saturday
            if (dayStart === 0 || dayStart === 6) {
                $("#IEPBeginDate").addClass("date-error");
                return alert("Please select a Weekday for the Annual Initiation Date.");
            } else {
                $("#IEPBeginDate").removeClass("date-error");
            }

            $("#IEPBeginDate").attr('disabled', true);
            $('.ajax-loader').css("visibility", "visible");
            $(".ajax-loader img").css("visibility", "visible");

            $.ajax({
                type: 'GET',
                url: '/Home/UpdateIEPDates',
                data: {
                    Stid: stId,
                    IepId: iepId,
                    IEPStartDate: startDate,
                    IEPMeetingDate: meetingDate
                },
                dataType: 'json',
                success: function (data) {
                    if (data.Result === 'success') {

                        location.reload();

                    } else {

                        $(".ajax-loader img").css("visibility", "hidden");
                        $(".ajax-loader .failure").show().fadeOut(1000, "linear", function () {
                            $("#alertMessage .moreinfo").html('There was an error while trying to save the data.');
                        });
                    }
                },
                error: function (data) {
                    $("#IEPBeginDate").addClass("date-error");

                    alert("Unable to connect to the server or other related network problem. Please contact your admin.");
                },
                complete: function () {
                    $("#IEPBeginDate").attr('disabled', false);
                }
            });
        });

        //$("#IEPBeginDate").datepicker({
        //    dateFormat: "mm/dd/yy",
        //    changeYear: true,
        //    changeMonth: true,
        //    yearRange: "-1:+2",
        //    beforeShowDay: function (date) {
        //        var day = date.getDay();
        //        var string = jQuery.datepicker.formatDate('d-m-yy', date);
        //        return [day !== 0 && day !== 6]; //day != 0/6 disables all Sundays and Saturdays
        //    }
        //});

        //$("#IEPMeetingDate").datepicker({
        //    dateFormat: "mm/dd/yy",
        //    changeYear: true,
        //    changeMonth: true,
        //    yearRange: "-1:+2",
        //    beforeShowDay: function (date) {
        //        var day = date.getDay();
        //        var string = jQuery.datepicker.formatDate('d-m-yy', date);
        //        return [day !== 0 && day !== 6]; //day != 0/6 disables all Sundays and Saturdays
        //    }
        //});

        $("#HealthHearingDate").datepicker({
            dateFormat: "mm/dd/yy",
            changeYear: true,
            changeMonth: true,
            yearRange: "-4:+2",
            beforeShowDay: function (date) {
                var day = date.getDay();
                var string = jQuery.datepicker.formatDate('d-m-yy', date);
                return [day !== 0 && day !== 6]; //day != 0/6 disables all Sundays and Saturdays
            }
        });

        $("#HealthVisionDate").datepicker({
            dateFormat: "mm/dd/yy",
            changeYear: true,
            changeMonth: true,
            yearRange: "-4:+2",
            beforeShowDay: function (date) {
                var day = date.getDay();
                var string = jQuery.datepicker.formatDate('d-m-yy', date);
                return [day !== 0 && day !== 6]; //day != 0/6 disables all Sundays and Saturdays
            }
        });

        // Attach Event
        // if the user is an MIS or ADMIN the Initiation Date is today or later AND the iep status is "draft"; then when they click the url button start the questions..
        $("#makeIEPActive").on("click", function () {
            var answer = confirm("Are you sure you want to make this IEP Active?");
            if (answer) {

                $("#betterCheckYourWork").modal('show');

                $('#carousel-questions').on('slide.bs.carousel', function onSlide(ev) {
                    var id = ev.relatedTarget.id;
                    var StId = $("#stid").val();
                    var IEPid = $("#iepId").val();
                    switch (id) {
                        case "0":
                            $("#betterCheckYourWorkPrevious").addClass("hidden");
                            break;
                        case "1":
                            $("#betterCheckYourWorkContinue").addClass("hidden");
                            $("#betterCheckYourWorkComplete").removeClass("hidden");
                            break;
                        case "savegrade":
                            var Grade = $("#studentCarouselGrade").val();

                            $.ajax({
                                type: 'POST',
                                url: '/Home/UpdateStudentIEPGrade',
                                async: true,
                                data: {
                                    grade: Grade,
                                    stid: StId,
                                    iepId: IEPid
                                },
                                dataType: 'json',
                                success: function (data) {
                                    console.log("grade done");
                                },
                                error: function (data) {
                                    console.log("grade error");
                                }
                            });
                            break;
                        case "savecode":
                            var Code = $("#studentCarouselCode").val();
                            var data = {
                                code: Code,
                                stid: StId,
                                iepId: IEPid
                            };
                            console.log(data);

                            $.ajax({
                                type: 'POST',
                                url: '/Home/UpdateStudentCode',
                                async: true,
                                data: {
                                    code: Code,
                                    stid: StId,
                                    iepId: IEPid
                                },
                                dataType: 'json',
                                success: function (data) {
                                    console.log("code done");
                                },
                                error: function (data) {
                                    console.log("code error");
                                }
                            });
                            break;
                        default:
                            $("#betterCheckYourWorkContinue").removeClass("hidden");
                            $("#betterCheckYourWorkPrevious").removeClass("hidden");
                            $("#betterCheckYourWorkComplete").addClass("hidden");
                    }
                });
            };
        });

        // Attach Event
        // if the user is an MIS or ADMIN the Initiation Date is today or later AND the iep status is "draft"; then when they click the url button and have answered all the "Check Yo Self" questions..
        $("#betterCheckYourWorkComplete").on("click", function () {
            if ($("#makeIEPActive").hasClass("disabled")) {
                return false;
            } // the link is disabled.

            var iepType = $(this).data("iep");

            $("#betterCheckYourWork").modal('hide');
            $('.ajax-loader').css("visibility", "visible");
            $(".ajax-loader img").css("visibility", "visible");

            var stId = $("#stid").val();
            var iepId = $("#studentIEPId").val();
            var theUrl;
            switch (iepType) {
                case "admendment":
                    theUrl = "/Home/UpdateIEPAmendmentToActive";
                    break;
                case "annual":
                    theUrl = "/Home/UpdateIEPAnnualToActive";
                    break;
                default:
                    theUrl = "/Home/UpdateIEPStatusToActive";
                    break;
            }

            $.ajax({
                type: 'GET',
                url: theUrl,
                data: {
                    Stid: stId,
                    IEPid: iepId
                },
                dataType: 'json',
                success: function (data) {
                    if (data.Result === 'success') {
                        location.reload(true);
                    }
                },
                error: function (data) {
                    alert(data.Message);

                    $('.ajax-loader').css("visibility", "hidden");
                    $(".ajax-loader img").css("visibility", "hidden");
                }
            });
        });

        // Attach Event
        // if the user is an MIS or ADMIN the Initiation Date is today or later AND the iep status is "draft"; then when they click the url button..
        $("#makeIEPAmendment").on("click", function () {
            if ($("#makeIEPAmendment").hasClass("disabled")) {
                return false;
            } // the link is disabled

            var answer = confirm("Are you sure you want to make an Amendment to this IEP?");
            if (answer) {
                $('.ajax-loader').css("visibility", "visible");
                $(".ajax-loader img").css("visibility", "visible");

                var stId = $("#stid").val();
                var iepId = $("#studentIEPId").val();

                $.ajax({
                    type: 'GET',
                    url: '/Manage/CreateIEPAmendment',
                    data: {
                        Stid: stId,
                        IepId: iepId,
                        amend: true
                    },
                    dataType: 'json',
                    success: function (data) {
                        if (data.Result === 'success') {
                            window.location.href = '/Home/StudentProcedures/?stid=' + stId + '&iepID=' + data.Message;
                        } else {
                            alert(data.Message);
                            location.reload(true);
                        }
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        alert(xhr.status);
                        alert(thrownError);
                    },
                    complete: function () {
                        $('.ajax-loader').css("visibility", "hidden");
                        $(".ajax-loader img").css("visibility", "hidden");
                    }
                });
            }
        });

        // Attach Event
        // if the user is an MIS or ADMIN the Initiation Date is today or later AND then when they click the url button..
        $("#makeIEPAnnual").on("click", function () {
            if ($("#makeIEPAnnual").hasClass("disabled")) {
                return false;
            } // the link is disabled

            var answer = confirm("Are you sure you want to make an Annual IEP?");
            if (answer) {
                $('.ajax-loader').css("visibility", "visible");
                $(".ajax-loader img").css("visibility", "visible");

                var stId = $("#stid").val();
                var iepId = $("#studentIEPId").val();

                $.ajax({
                    type: 'GET',
                    url: '/Manage/CreateIEPAnnual',
                    data: {
                        Stid: stId,
                        IepId: iepId
                    },
                    dataType: 'json',
                    success: function (data) {
                        if (data.Result === 'success') {
                            window.location.href = '/Home/StudentProcedures/?stid=' + stId + '&iepID=' + data.Message;
                        } else {
                            alert(data.Message);
                            location.reload(true);
                        }
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        alert(xhr.status);
                        alert(thrownError);
                    },
                    complete: function () {
                        $('.ajax-loader').css("visibility", "hidden");
                        $(".ajax-loader img").css("visibility", "hidden");
                    }
                });
            }
        });

        // Attach Event
        // if the user is the OWNER or MIS and they choose to make the iep Inactive
        $("#makeIEPInActive").on("click", function () {
            if ($("#makeIEPInActive").hasClass("disabled")) {
                return false;
            } // the link is disabled

            var answer = confirm("Are you sure you want to set this DRAFT to inactive?");
            if (answer) {
                $('.ajax-loader').css("visibility", "visible");
                $(".ajax-loader img").css("visibility", "visible");

                var stId = $("#stid").val();
                var iepId = $("#studentIEPId").val();

                $.ajax({
                    type: 'GET',
                    url: '/Home/UpdateIEPStatusToInActive',
                    data: {
                        Stid: stId,
                        IepId: iepId
                    },
                    dataType: 'json',
                    success: function (data) {
                        if (data.Result === 'success') {

                            if (data.ActiveIEP != 0) {
                                window.location.href = '/Home/StudentProcedures/?stid=' + stId + '&iepID=' + data.ActiveIEP;
                            }
                            else {
                                location.href = "/Home/Portal";
                            }
                        } else {
                            alert(data.Message);
                            location.reload(true);
                        }
                    },
                    error: function (data) {
                        alert("Unable to connect to the server or other related network problem. Please contact your admin.");
                    },
                    complete: function () {
                        $('.ajax-loader').css("visibility", "hidden");
                        $(".ajax-loader img").css("visibility", "hidden");
                    }
                });
            }
        });

        // Attach Event
        // if the iep is an amendment and they make it active
        $("#makeIEPAmendmentActive").on("click", function () {
            if ($("#makeIEPAmendmentActive").hasClass("disabled")) {
                return false;
            } // the link is disabled

            var answer = confirm("Are you sure you want to set this AMENDMENT to active?");
            if (answer) {

                $("#betterCheckYourWork").modal('show');

                $('#carousel-questions').on('slide.bs.carousel', function onSlide(ev) {
                    var id = ev.relatedTarget.id;
                    switch (id) {
                        case "0":
                            $("#betterCheckYourWorkPrevious").addClass("hidden");
                            break;
                        case "1":
                            $("#betterCheckYourWorkContinue").addClass("hidden");
                            $("#betterCheckYourWorkComplete").removeClass("hidden");
                            break;
                        case "savegrade":
                            var Grade = $("#studentCarouselGrade").val();
                            var StId = $("#stid").val();
                            var IEPid = $("#studentIEPId").val();

                            $.ajax({
                                type: 'POST',
                                url: '/Home/UpdateStudentIEPGrade',
                                async: true,
                                data: {
                                    grade: Grade,
                                    stid: StId,
                                    iepId: IEPid
                                },
                                dataType: 'json',
                                success: function (data) {
                                    console.log("grade done");
                                },
                                error: function (data) {
                                    console.log("grade error");
                                }
                            });
                            break;
                        case "savecode":
                            var Code = $("#studentCarouselCode").val();
                            var StId = $("#stid").val();
                            var IEPid = $("#studentIEPId").val();

                            $.ajax({
                                type: 'POST',
                                url: '/Home/UpdateStudentCode',
                                async: true,
                                data: {
                                    code: Code,
                                    stid: StId,
                                    iepId: IEPid
                                },
                                dataType: 'json',
                                success: function (data) {
                                    console.log("code done");
                                },
                                error: function (data) {
                                    console.log("code error");
                                }
                            });
                            break;
                        default:
                            $("#betterCheckYourWorkContinue").removeClass("hidden");
                            $("#betterCheckYourWorkPrevious").removeClass("hidden");
                            $("#betterCheckYourWorkComplete").addClass("hidden");
                    }
                });
            };
        });

        $("#makeIEPAnnualActive").on("click", function () {
            if ($("#makeIEPAnnualActive").hasClass("disabled")) {
                return false;
            } // the link is disabled

            var answer = confirm("Are you sure you want to set this ANNUAL to active?");
            if (answer) {

                $("#betterCheckYourWork").modal('show');

                $('#carousel-questions').on('slide.bs.carousel', function onSlide(ev) {
                    var id = ev.relatedTarget.id;
                    console.log(id);
                    switch (id) {
                        case "0":
                            $("#betterCheckYourWorkPrevious").addClass("hidden");
                            break;
                        case "1":
                            $("#betterCheckYourWorkContinue").addClass("hidden");
                            $("#betterCheckYourWorkComplete").removeClass("hidden");
                            break;
                        case "savegrade":
                            var Grade = $("#studentCarouselGrade").val();
                            var StId = $("#stid").val();
                            var IEPid = $("#studentIEPId").val();

                            $.ajax({
                                type: 'POST',
                                url: '/Home/UpdateStudentIEPGrade',
                                async: true,
                                data: {
                                    grade: Grade,
                                    stid: StId,
                                    iepId: IEPid
                                },
                                dataType: 'json',
                                success: function (data) {
                                    console.log("grade done");
                                },
                                error: function (data) {
                                    console.log("grade error");
                                }
                            });
                            break;
                        case "savecode":
                            var Code = $("#studentCarouselCode").val();
                            var StId = $("#stid").val();

                            $.ajax({
                                type: 'POST',
                                url: '/Home/UpdateStudentCode',
                                async: true,
                                data: {
                                    code: Code,
                                    stid: StId
                                },
                                dataType: 'json',
                                success: function (data) {
                                    console.log("code done");
                                },
                                error: function (data) {
                                    console.log("code error");
                                }
                            });
                            break;
                        default:
                            $("#betterCheckYourWorkContinue").removeClass("hidden");
                            $("#betterCheckYourWorkPrevious").removeClass("hidden");
                            $("#betterCheckYourWorkComplete").addClass("hidden");
                    }
                });
            };
        });

        // Attach Event
        // if the owner or mis reverts the draft back to active
        $("#revertToDraft").on("click", function () {
            if ($("#makeIEPInActive").hasClass("disabled")) {
                return false;
            } // the link is disabled

            var answer = confirm("Are you sure you want to revert this IEP from ACTIVE to DRAFT?");
            if (answer) {

                $('#reasonsRevertDraft').modal('show');
            }
        });

        $("#saveRevertDraft").on('click', function () {
            if ($("#yourReasoning").val().length > 0) {

                $('.ajax-loader').css("visibility", "visible");
                $(".ajax-loader img").css("visibility", "visible");

                var stId = $("#stid").val();
                var iepId = $("#studentIEPId").val();
                var reasons = $("#yourReasoning").val();

                $.ajax({
                    type: 'GET',
                    url: '/Home/UpdateRevertIEPtoDraft',
                    data: {
                        Stid: stId,
                        IepId: iepId,
                        MyReason: reasons
                    },
                    dataType: 'json',
                    success: function (data) {
                        if (data.Result === 'success') {
                            location.reload(true);
                        } else {
                            alert(data.Message);
                            location.reload(true);
                        }
                    },
                    error: function (data) {
                        alert("Unable to connect to the server or other related network problem. Please contact your admin.");
                    },
                    complete: function () {
                        $('.ajax-loader').css("visibility", "hidden");
                        $(".ajax-loader img").css("visibility", "hidden");
                    }
                });
            }
            else {
                alert("This field cannot be blank.");
            }
        });
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
                ajax.classList.add("hidden");
            }
        }
    };
    new ft(params);
});

// $(document).ready equivalent
document.addEventListener("DOMContentLoaded", function (event) {
    $(".ajax-loader").css("visibility", "hidden");

    var unavailableDates = [];
    var studentUSD = $("#studentUSD").val();
    var studentYear = $("#studentYear").val();

    $.ajax({
        type: 'GET',
        url: '/Home/GetCalendarViewByBuilding',
        data: {
            SchoolYear: studentYear,
            usd: studentUSD,
            bId: 0,
        },
        dataType: 'json',
        success: function (data) {
            if (data.Result === 'success') {
                var i = data.calendarEvents.length - 1
                while (i >= 0) {
                    if (data.calendarEvents[i].calendarDate != null) {
                        var currentTime = new Date(parseInt(data.calendarEvents[i].calendarDate.substr(6)));
                        var month = ("0" + (currentTime.getMonth() + 1)).slice(-2);
                        var day = ("0" + currentTime.getDate()).slice(-2);
                        var year = currentTime.getFullYear();
                        var date = month + '/' + day + '/' + year;
                        unavailableDates.push(date);
                    }
                    i--;
                }
            }
        }
    }).done(function () {

        $("#IEPBeginDate").datepicker({
            dateFormat: 'mm/dd/yy',
            changeYear: true,
            changeMonth: true,
            yearRange: "-1:+2",
            beforeShowDay: function (date) {
                var month = ("0" + (date.getMonth() + 1)).slice(-2);
                var day = ("0" + date.getDate()).slice(-2);
                var year = date.getFullYear();
                var dmy = month + '/' + day + '/' + year;
                if ($.inArray(dmy + "", unavailableDates) > -1) {
                    return [false, "", "unAvailable"];
                } else {
                    var day = date.getDay();
                    var string = jQuery.datepicker.formatDate('dd/mm/yy', date);
                    return (day !== 0 && day !== 6) ? [true, "", "Available"] : [false, "", "unAvailable"];
                }
            }
        });

        console.log(unavailableDates);
        console.log("----------------");

        $("#IEPMeetingDate").datepicker({
            dateFormat: 'mm/dd/yy',
            changeYear: true,
            changeMonth: true,
            yearRange: "-1:+2",
            beforeShowDay: function (date) {
                var month = ("0" + (date.getMonth() + 1)).slice(-2);
                var day = ("0" + date.getDate()).slice(-2);
                var year = date.getFullYear();
                var dmy = month + '/' + day + '/' + year;
                if ($.inArray(dmy + "", unavailableDates) > -1) {
                    return [false, "", "unAvailable"];
                } else {
                    var day = date.getDay();
                    var string = jQuery.datepicker.formatDate('dd/mm/yy', date);
                    return (day !== 0 && day !== 6) ? [true, "", "Available"] : [false, "", "unAvailable"];
                }
            }
        });

    }).fail(function () {
        alert("An error occured either when our server or your connection.");
    });
});

var moduleFormSerialize = '';

// Prevents the user from using the back button
window.history.pushState({
    page: 1
}, "", "");

window.onpopstate = function (event) {

    // "event" object seems to contain value only when the back button is clicked
    // and if the pop state event fires due to clicks on a button
    // or a link it comes up as "undefined" 

    if (event) {
        // Code to handle back button or prevent from navigation
        event.preventDefault();
        history.go(1);
    }
};

function getParameterByName(name, url) {
    if (!url) {
        url = window.location.href;
    }
    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, " "));
}



$('input[type=radio][name=optionsExtendedSchoolYear]').change(function () {
    if (this.value === 'option1') {
        $('#form').css('display', 'block');
    } else {
        $('#form').css('display', 'none');
    }
});

function getUrlParameter(sParam) {
    var sPageURL = decodeURIComponent(window.location.search.substring(1)),
        sURLVariables = sPageURL.split('&'),
        sParameterName,
        i;

    for (i = 0; i < sURLVariables.length; i++) {
        sParameterName = sURLVariables[i].split('=');

        if (sParameterName[0] === sParam) {
            return sParameterName[1] === undefined ? true : sParameterName[1];
        }
    }
}

function printModule(divOverride) {
    var stid = getUrlParameter('stid');
    var iepId = $("#studentIEPId").val();
    var getGoalsToPrint = "";

    if (divOverride.toLowerCase() === "progress") {
        var idList = [];
        $.each($("input[name='printGoal']:checked"), function () {
            idList.push($(this).attr('data-val'));
        });
        getGoalsToPrint = idList; //JSON.stringify(idList);
    }

    window.location.href = '/Home/PrintIEPSection/?stid=' + stid + '&iepId=' + iepId + "&section=" + divOverride + "&goalsToPrint=" + getGoalsToPrint;
}

function createDateString(newDate) {
    var dateArr = newDate.split('-');
    var month = 0;
    var day = 0;
    var year = 0;

    if (dateArr.length > 0 && dateArr.length === 3) {
        year = dateArr[0];
        month = dateArr[1];
        day = dateArr[2];
    }

    var returnDate = new Date(year, month - 1, day);
    var output = returnDate.getMonth() + 1 + '/' + returnDate.getDate() + '/' + returnDate.getFullYear();
    return output;
}

function formatDate(date) {
    var d = new Date(date),
        month = '' + (d.getMonth() + 1),
        day = '' + d.getDate(),
        year = d.getFullYear();

    if (month.length < 2) month = '0' + month;
    if (day.length < 2) day = '0' + day;

    return ([year, month, day].join('-')).toString();
}

/** fixing the bootstrap modal overlay bug and setting up events. **/
$(window).on('shown.bs.modal', function (e) {
    var moduleId = e.target.id;
    var modals = $(".modal").get(),
        element = null;

    for (var i = 0, length = modals.length; i < length; i++) {
        $(modals[i]).css("height", "0");
    }

    $("#" + moduleId).css("height", "auto");

    // EVENT LISTERNERS
    // get the form serialized so if we make changes we can compare.
    moduleFormSerialize = $("#moduleSection form").serialize();

    // Create a save listener for when saving content in a module
    $(window).on('saving.bs.modal', function (e) {
        moduleFormSerialize = $("#moduleSection form").serialize();

        setTimeout(function () {
            if (!$('#moduleSection').is(':visible')) {
                window.location.reload();
            }
        }, 10);
    });

    // Create a delete listener for when deleteing content in a module
    $(window).on('deleting.bs.modal', function (e) {
        moduleFormSerialize = $("#moduleSection form").serialize();
    });
});

// Checks for changes that are not yet saved and send the user a message
$('#moduleSection').on('hide.bs.modal', function (e) {
    if ($("#moduleSection form").serialize() !== moduleFormSerialize) {
        if (!confirm("If your leave this module now the changes you made will NOT be saved.")) {
            e.preventDefault();
        }
    }

    // this checks if all of the modules are completed.
    var allCompleted = true;

    var modules = $(".ui-state-default.iep");
    var otherIEP = $(".ui-state-default.otherIep");
    var elementsToCheck = $.merge(modules, otherIEP);

    $.each(elementsToCheck, function () {
        if (!$(this).find(".hero-widget").hasClass("module-completed")) {
            allCompleted = false;
            return false; // breaks us out of the $.each loop.
        }
    });

    allCompleted ? $("#makeIEPActive").removeClass("disabled") : $("#makeIEPActive").addClass("disabled");
    allCompleted ? $("#makeIEPAmendmentActive").removeClass("disabled") : $("#makeIEPAmendmentActive").addClass("disabled");
    allCompleted ? $("#makeIEPAnnualActive").removeClass("disabled") : $("#makeIEPAnnualActive").addClass("disabled");

});

///////////
// Module Scripts
///////////
//
// Student Planning

// Attach Event
// Save Plan button clicked.
$('#saveplan').on('click', function () {
    $('.ajax-loader').css("visibility", "visible");
    var stId = $("#stid").val();
    var iepId = $("#studentIEPId").val();
    var form = $('#thePlan').serialize();

    $.ajax({
        type: 'POST',
        url: '/Home/StudentPlanning',
        data: form,
        dataType: 'json',
        async: false,
        success: function (data) {
            if (data.result === "success") {
                window.location.href = "/Home/StudentProcedures?stid=" + stId + "&iepID=" + iepId;
            } else {
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
        },
        complete: function () {
            $('.ajax-loader').css("visibility", "hidden");
        }
    });
});

// Attach Event
// Dismissing a Student Plan
$("#dismissIEPPlan").on("click", function () {
    var stId = $("#stid").val();
    var iepId = $("#studentIEPId").val();

    $.ajax({
        type: 'GET',
        url: '/Home/dismissPlanning',
        data: {
            studentId: stId,
            iepId: iepId
        },
        dataType: 'json',
        async: false,
        success: function (data) {
            if (data.result === "success") {
                window.location.href = "/Home/StudentProcedures?stid=" + data.message + "&iepID=" + iepId;
            } else {
                $("#alertMessage .moreinfo").html('Server Error');
                $("#alertMessage").fadeTo(3000, 500).slideUp(500, function () {
                    $("#alertMessage").slideUp(500);
                });
            }
        },
        error: function (data) {
            alert('errors?');
            console.log(data);
            $("#alertMessage .moreinfo").html('Unable to connect to the server or other related problem. Please contact your admin.');
            $("#alertMessage").fadeTo(3000, 500).slideUp(500, function () {
                $("#alertMessage").slideUp(500);
            });
        },
        complete: function () {
            $('.ajax-loader').css("visibility", "hidden");
        }
    });
});

// Attach Event
// Health
$("input[name='HealthNoConcern']").on("click", function () {
    if ($("input[name='HealthNoConcern']").is(':checked')) {
        $("input[name='HealthProgressTowardGenEd']").prop('checked', false);
        $("input[name='HealthDiagnosis']").prop('checked', false);
    }
});

// Attach Event
// Motor
$("input[name='MotorNoConcern']").on('click', function (event) {
    if ($("input[name='MotorNoConcern']").is(':checked')) {
        $("input[name='MotorProgress']").prop('checked', false);
        $("input[name='MotorNeeds']").prop('checked', false);
        $("input[name='MotorParticipation']").prop('checked', false);
    }
});

// Attach Event
// Communication
$("input[name='CommunicationNoConcern']").on('click', function (event) {
    if ($("input[name='CommunicationNoConcern'").is(':checked')) {
        $("input[name='CommunicationSpeech']").prop('checked', false);
        $("input[name='CommunicationDeaf']").prop('checked', false);
        $("input[name='CommunicationEnglish']").prop('checked', false);
        $("input[name='CommunicationProgressTowardGenEd']").prop('checked', false);
    }
});

// Attach Event
// Social-Emotional
$("input[name='SocialNoConcern']").on('click', function (event) {
    if ($("input[name='SocialNoConcern'").is(':checked')) {
        $("input[name='SocialProgressTowardGenEd']").prop('checked', false);
        $("input[name='SocialAreaOfNeed']").prop('checked', false);
        $("input[name='SocialMental']").prop('checked', false);
        $("input[name='SocialBehaviorSignificant']").prop('checked', false);
        $("input[name='SocialBehaviorImpede']").prop('checked', false);
        $("input[name='SocialInterventionPlan']").prop('checked', false);
        $("input[name='SocialSkillsDeficit']").prop('checked', false);
    }
});

// Attach Event
// Intelligence Concerns
$("input[name='IntelligenceNoConcern']").on('click', function (event) {
    if (!$("input[name='IntelligenceNoConcern']").is(':checked')) {
        $("input[name='IntelligenceProgressTowardGenEd']").prop('checked', false);
        $("input[name='IntelligenceAreaOfNeed']").prop('checked', false);
    }
});

// Attach Event
// Academic Performance
$("input[name='AcademicModuleNoConcern']").on('click', function (event) {
    if ($("input[name='AcademicModuleNoConcern']").is(':checked')) {
        $("input[name='AcademicProgressTowardGenEd']").prop('checked', false);
        $("input[name='AcademicNeeds']").prop('checked', false);
        $("input[name='AcademicNoConcern']").prop('checked', true);

        $("input[name='ReadingNoConcern']").prop('checked', false);
        $("input[name='ReadingNoConcern']").trigger('click');
        $("input[name='ReadingNoConcern']").prop('checked', true);

        $("input[name='MathNoConcern']").prop('checked', false);
        $("input[name='MathNoConcern']").trigger('click');
        $("input[name='MathNoConcern']").prop('checked', true);

        $("input[name='WrittenNoConcern']").prop('checked', false);
        $("input[name='WrittenNoConcern']").trigger('click');
        $("input[name='WrittenNoConcern']").prop('checked', true);
    }
});

// Attach Event
// Reading
$("input[name='ReadingNoConcern']").on('click', function (event) {

    if ($("input[name='ReadingNoConcern']").is(':checked')) {
        $("input[name='ReadingProgress']").prop('checked', false);
        $("input[name='ReadingTier1']").prop('checked', false);
        $("input[name='ReadingTier2']").prop('checked', false);
        $("input[name='ReadingTier3']").prop('checked', false);
        $("input[name='ReadingNeed']").prop('checked', false);
    }
});

$("input[name='ReadingProgress']").on('click', function (event) {
    if ($("input[name='ReadingProgress']").is(':checked')) {
        $("input[name='ReadingNoConcern']").prop('checked', false);
    }
});

$("input[name='ReadingTier1']").on('click', function (event) {
    if ($("input[name='ReadingTier1']").is(':checked')) {
        $("input[name='ReadingNoConcern']").prop('checked', false);
    }
});

$("input[name='ReadingTier2']").on('click', function (event) {
    if ($("input[name='ReadingTier2']").is(':checked')) {
        $("input[name='ReadingNoConcern']").prop('checked', false);
    }
});

$("input[name='ReadingTier3']").on('click', function (event) {
    if ($("input[name='ReadingTier3']").is(':checked')) {
        $("input[name='ReadingNoConcern']").prop('checked', false);
    }
});

$("input[name='ReadingNeed']").on('click', function (event) {
    if ($("input[name='ReadingNeed']").is(':checked')) {
        $("input[name='ReadingNoConcern']").prop('checked', false);
    }
});

// Attach Event
// Math
$("input[name='MathNoConcern']").on('click', function (event) {
    if ($("input[name='MathNoConcern']").is(':checked')) {
        $("input[name='MathProgress']").prop('checked', false);
        $("input[name='MathTier1']").prop('checked', false);
        $("input[name='MathTier2']").prop('checked', false);
        $("input[name='MathTier3']").prop('checked', false);
        $("input[name='MathNeed']").prop('checked', false);
    }
});

$("input[name='MathProgress']").on('click', function (event) {
    if ($("input[name='MathProgress']").is(':checked')) {
        $("input[name='MathNoConcern']").prop('checked', false);
    }
});

$("input[name='MathTier1']").on('click', function (event) {
    if ($("input[name='MathTier1']").is(':checked')) {
        $("input[name='MathNoConcern']").prop('checked', false);
    }
});

$("input[name='MathTier2']").on('click', function (event) {
    if ($("input[name='MathTier2']").is(':checked')) {
        $("input[name='MathNoConcern']").prop('checked', false);
    }
});

$("input[name='MathTier3']").on('click', function (event) {
    if ($("input[name='MathTier3']").is(':checked')) {
        $("input[name='MathNoConcern']").prop('checked', false);
    }
});

$("input[name='MathNeed']").on('click', function (event) {
    if ($("input[name='MathNeed']").is(':checked')) {
        $("input[name='MathNoConcern']").prop('checked', false);
    }
});

//Attach Event
// Written Language
$("input[name='WrittenNoConcern']").on('click', function (event) {
    if ($("input[name='WrittenNoConcern']").is(':checked')) {
        $("input[name='WrittenProgress']").prop('checked', false);
        $("input[name='WrittenTier1']").prop('checked', false);
        $("input[name='WrittenTier2']").prop('checked', false);
        $("input[name='WrittenTier3']").prop('checked', false);
        $("input[name='WrittenNeed']").prop('checked', false);
    }
});

$("input[name='WrittenProgress']").on('click', function (event) {
    if ($("input[name='WrittenProgress']").is(':checked')) {
        $("input[name='WrittenNoConcern']").prop('checked', false);
    }
});

$("input[name='WrittenTier1']").on('click', function (event) {
    if ($("input[name='WrittenTier1']").is(':checked')) {
        $("input[name='WrittenNoConcern']").prop('checked', false);
    }
});

$("input[name='WrittenTier2']").on('click', function (event) {
    if ($("input[name='WrittenTier2']").is(':checked')) {
        $("input[name='WrittenNoConcern']").prop('checked', false);
    }
});

$("input[name='WrittenTier3']").on('click', function (event) {
    if ($("input[name='WrittenTier3']").is(':checked')) {
        $("input[name='WrittenNoConcern']").prop('checked', false);
    }
});

$("input[name='WrittenNeed']").on('click', function (event) {
    if ($("input[name='WrittenNeed']").is(':checked')) {
        $("input[name='WrittenNoConcern']").prop('checked', false);
    }
});

//Attach Event
// Functional Academic
$("input[name='AcademicNoConcern']").on('click', function (event) {

    if ($("input[name='AcademicNoConcern']").is(':checked')) {
        $("input[name='AcademicProgressTowardGenEd']").prop('checked', false);
        $("input[name='AcademicNeeds']").prop('checked', false);
    }
});

$("input[name='AcademicProgressTowardGenEd']").on('click', function (event) {
    if ($("input[name='AcademicProgressTowardGenEd']").is(':checked')) {
        $("input[name='AcademicNoConcern']").prop('checked', false);
    }
});

$("input[name='AcademicNeeds']").on('click', function (event) {
    if ($("input[name='AcademicNeeds']").is(':checked')) {
        $("input[name='AcademicNoConcern']").prop('checked', false);
    }
});

$(".module-section").on("click", function (e) {
    var tId = $("#tid").val();
    var stId = $("#stid").val();
    var iepId = $("#studentIEPId").val();
    var ModuleView = $(e.currentTarget).data("view");

    $('.ajax-loader').css("visibility", "visible");

    $.ajax({
        type: 'GET',
        url: '/Home/LoadModuleSection',
        data: {
            studentId: stId,
            iepId: iepId,
            view: ModuleView
        },
        dataType: 'html',
        success: function (data) {
            if (data.length !== 0) {
                $("#module-form-section").html(data);
                $('#moduleSection').modal('show');
            } else {
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
        },
        complete: function () {
            $('.ajax-loader').css("visibility", "hidden");
        }
    });
});

$(".goal-section").on('click', function (e) {
    var stId = $("#stid").val();
    var iepId = $("#studentIEPId").val();

    $('.ajax-loader').css("visibility", "visible");

    $.ajax({
        type: 'GET',
        url: '/Home/StudentGoals',
        data: {
            studentId: stId,
            IEPid: iepId
        },
        dataType: 'html',
        success: function (data) {
            if (data.length !== 0) {
                $("#module-form-section").html(data);
                $('#moduleSection').modal('show');
            } else {
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
        },
        complete: function () {
            $('.ajax-loader').css("visibility", "hidden");
        }
    });
});

$(".service-section").on('click', function (e) {
    var stId = $("#stid").val();
    var iepId = $("#studentIEPId").val();

    $('.ajax-loader').css("visibility", "visible");

    $.ajax({
        type: 'GET',
        url: '/Home/StudentServices',
        data: {
            studentId: stId,
            IEPid: iepId
        },
        dataType: 'html',
        success: function (data) {
            if (data.length !== 0) {
                $("#module-form-section").html(data);
                $('#moduleSection').modal('show');
            } else {
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
        },
        complete: function () {
            $('.ajax-loader').css("visibility", "hidden");
        }
    });
});

$(".accom-mod-section").on('click', function (e) {
    var stId = $("#stid").val();
    var iepId = $("#studentIEPId").val();

    $('.ajax-loader').css("visibility", "visible");

    $.ajax({
        type: 'GET',
        url: '/Home/Accommodations',
        data: {
            studentId: stId,
            IEPid: iepId
        },
        dataType: 'html',
        success: function (data) {
            if (data.length !== 0) {
                $("#module-form-section").html(data);
                $('#moduleSection').modal('show');
            } else {
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
        },
        complete: function () {
            $('.ajax-loader').css("visibility", "hidden");
        }
    });
});

$(".other-considerations-section").on('click', function (e) {
    var stId = $("#stid").val();
    var iepId = $("#studentIEPId").val();

    $('.ajax-loader').css("visibility", "visible");

    $.ajax({
        type: 'GET',
        url: '/Home/OtherConsiderations',
        data: {
            studentId: stId,
            IEPid: iepId
        },
        dataType: 'html',
        success: function (data) {
            if (data.length !== 0) {
                $("#module-form-section").html(data);
                $('#moduleSection').modal('show');
            } else {
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
        },
        complete: function () {
            $('.ajax-loader').css("visibility", "hidden");
        }
    });
});

$(".behavior-plan-section").on('click', function (e) {
    var stId = $("#stid").val();
    var iepId = $("#studentIEPId").val();

    $('.ajax-loader').css("visibility", "visible");

    $.ajax({
        type: 'GET',
        url: '/Home/BehaviorPlan',
        data: {
            studentId: stId,
            iepID: iepId
        },
        dataType: 'html',
        success: function (data) {
            if (data.length !== 0) {
                $("#module-form-section").html(data);
                $('#moduleSection').modal('show');
            } else {
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
        },
        complete: function () {
            $('.ajax-loader').css("visibility", "hidden");
        }
    });
});

$(".transition-section").on('click', function (e) {
    var stId = $("#stid").val();
    var iepId = $("#studentIEPId").val();

    $('.ajax-loader').css("visibility", "visible");

    $.ajax({
        type: 'GET',
        url: '/Home/StudentTransition',
        data: {
            studentId: stId,
            IEPid: iepId
        },
        dataType: 'html',
        success: function (data) {
            if (data.length !== 0) {
                $("#module-form-section").html(data);
                $('#moduleSection').modal('show');
            } else {
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
        },
        complete: function () {
            $('.ajax-loader').css("visibility", "hidden");
        }
    });
});

$(".contingency-section").on('click', function (e) {
    var stId = $("#stid").val();
    var iepId = $("#studentIEPId").val();

    $('.ajax-loader').css("visibility", "visible");

    $.ajax({
        type: 'GET',
        url: '/Home/StudentContingency',
        data: {
            studentId: stId,
            IEPid: iepId
        },
        dataType: 'html',
        success: function (data) {
            if (data.length !== 0) {
                $("#module-form-section").html(data);
                $('#moduleSection').modal('show');
            } else {
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
        },
        complete: function () {
            $('.ajax-loader').css("visibility", "hidden");
        }
    });
});

///////////
// Module Scripts
///////////
//
// Student Health

// Attach Event
// YesMedication
$("#myonoffswitch3").click(function (event) {
    if ($(event.target).is(':checked')) {
        $("#AdditionalHealthInfo").fadeIn();
    } else {
        $("#AdditionalHealthInfo").fadeOut();
    }
});

// Attach Event
// MedicationID
$("#myonoffswitch4").click(function (event) {
    if ($(event.target).is(':checked')) {
        $("#AdditionalMedicaidID").fadeIn();
    } else {
        $("#AdditionalMedicaidID").fadeOut();
    }
});

$("#truefalseSwitchHealthConcerns").click(function (event) {
    if ($(event.target).is(':checked')) {
        $('.isHealthConcern').addClass("noConcerns").fadeOut();
      } else {
      $('.isHealthConcern').removeClass("noConcerns").fadeIn();
    }
});

$("#truefalseSwitchMotorConcerns").click(function (event) {
    if ($(event.target).is(':checked')) {
        $('.isMotorConcern').addClass("noConcerns").fadeOut();
    } else {
        $('.isMotorConcern').removeClass("noConcerns").fadeIn();
    }
});

$("#truefalseSwitchCommunicationConcerns").click(function (event) {
    if ($(event.target).is(':checked')) {
        $('.isCommunicationConcern').addClass("noConcerns").fadeOut();
    } else {
        $('.isCommunicationConcern').removeClass("noConcerns").fadeIn();
    }
});

$("#truefalseSwitchSocialConcerns").click(function (event) {
    if ($(event.target).is(':checked')) {
        $('.isSocialConcern').addClass("noConcerns").fadeOut();
    } else {
        $('.isSocialConcern').removeClass("noConcerns").fadeIn();
    }
});

$("#truefalseSwitchAcademicConcerns").click(function (event) {
    if ($(event.target).is(':checked')) {
        $('.isAcademicPerformanceConcern').addClass("noConcerns").fadeOut();
    } else {
        $('.isAcademicPerformanceConcern').removeClass("noConcerns").fadeIn();
    }
});

$("#truefalseSwitchIntelligenceNoConcern").click(function (event) {
    if ($(event.target).is(':checked')) {
        $('.isIntelligenceConcern').addClass("noConcerns").fadeOut();
    } else {
        $('.isIntelligenceConcern').removeClass("noConcerns").fadeIn();
    }
});