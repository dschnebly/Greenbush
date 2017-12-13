$(function () {

    var body = document.getElementsByTagName("body")[0];
    body.classList += ' ' + 'fadeInToBackground';

    var el = document.getElementsByClassName("body-content")[0], c = el.style;
    c.borderLeft = "1px solid #000";
    c.borderRight = "1px solid #000";
    c.boxShadow = "10px 10px 25px #333";
    c.position = "relative";
    c.top = "15px";
    c.backgroundColor = "#ffffff";
    c.marginBottom = "75px";

    setTimeout(function () {
        var tp = document.getElementsByClassName("transition-page")[0];
        tp.style.display = "block";

        var className = 'transition-page-scaleUpCenter';
        if (el.classList) {
            el.classList.add(className);
        } else {
            el.className += ' ' + className;
        }
    }, 200);

    var returnUrl = '/Home/StudentProcedures?under=false';

    // attach event to the close button
    // return user to the teacherportal page if they chose NOT to print the consent form. WE CANNOT GO FORWARD WITH THIS STUDENT
    $("#backtoportal").on('click', function (event) {
        event.stopPropagation();

        $('.body-content').addClass('transition-page-scaleDownCenter');
        $('body').removeClass("fadeInToBackground").addClass("fadeOutToBackground");

        setTimeout(function () {
            window.location = $("#backtoportal").attr("href");
        }, 325);
    });


    $("#dobtimepicker").datepicker({
        onSelect: function (dateText, inst) {
            var dobDate = $(this).datepicker('getDate');
            var dobYear = dobDate.getFullYear();
            var currentYear = (new Date).getFullYear();

            if (currentYear - dobYear < 13) {
                returnUrl = '/Home/StudentProcedures?under=true';
            }
        }
    }).on("change", function () {
        var dobDate = $(this).datepicker('getDate');
        var dobYear = dobDate.getFullYear();
        var currentYear = (new Date).getFullYear();

        if (currentYear - dobYear < 13) {
            // showForm('form12Icon');
        }
        else {
            // hideForm('form12Icon');
        }
    });
});