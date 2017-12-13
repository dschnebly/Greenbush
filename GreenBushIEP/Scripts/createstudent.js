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
});

function init() {

    //set up the bootstrap datepicker
    var date_input = $('input[name="dob"]'); //our date input has the name "dob"
    var container = $('.bootstrap-iso form').length > 0 ? $('.bootstrap-iso form').parent() : "body";
    var options = {
        format: 'mm-dd-yyyy',
        container: container,
        todayHighlight: true,
        autoclose: true,
    };
    date_input.datepicker(options);

    $(".chosen-select").chosen({
        disable_search_threshold: 10,
        no_results_text: "Oops, nothing found!",
        width: "100%"
    });

    $("#submitForm").on('click', function () {
        document.forms[0].submit();
    });

    $('#buildingIds').hide();
    var link = document.createElement("a");
    link.href = "/Scripts/bootstrap-mutliselect.js";

    // loads the building script
    $.getScript(link.protocol + "//" + link.host + link.pathname + link.search + link.hash, function () {
        //$('#buildingIds').show();
    });

    var link = document.createElement("a");
    link.href = "/Scripts/bootstrap-datepicker.min.js";

    // loads the building script
    $.getScript(link.protocol + "//" + link.host + link.pathname + link.search + link.hash, function () {
    });

    //Initialize tooltips
    $('.nav-tabs > li a[title]').tooltip();

    //Wizard
    $('a[data-toggle="tab"]').on('show.bs.tab', function (e) {
        var $target = $(e.target);
        if ($target.parent().hasClass('disabled')) {
            return false;
        }
    });

    $(".next-step").click(function (e) {
        if (tabValidates()) {
            var $active = $('.wizard .nav-tabs li.active');
            $active.next().removeClass('disabled');
            nextTab($active);
        }
    });

    $(".prev-step").click(function (e) {
        var $active = $('.wizard .nav-tabs li.active');
        prevTab($active);
    });

    function nextTab(elem) {
        $(elem).next().find('a[data-toggle="tab"]').click();
    }
    function prevTab(elem) {
        $(elem).prev().find('a[data-toggle="tab"]').click();
    }
}

function tabValidates()
{
    var validates = true;
    var stepId = $('.wizard .nav-tabs li.active a').attr('href');
    var $inputs = $(stepId + " :input[data-validate='true']");

    console.log($inputs);

    $inputs.each(function () {
        var input = $(this);
        var is_valid = input.val();
        if (is_valid == "") {
            if (input.is("select")) { $(this).next().addClass('contact-tooltip'); }
            else { input.addClass('contact-tooltip'); }
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

                // new contact id.
                newContact.html(newContact.html().replace(/\[#\]/g, '[' + ++index + ']'));
                newContact.find(".contact-button").removeClass("contact-button").addClass("add-contact").removeClass("btn-info").addClass('btn-danger');

                // rebind the recently added contact.
                initContacts();
            } else {
                $(this).unbind("click").parents(".student-contact").fadeOut(300, function () {
                    $(this).remove();
                });
            }
        });
    });
}