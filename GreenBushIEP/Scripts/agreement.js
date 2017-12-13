$(function () {
    $("#agree").click(function (e) {
        e.preventDefault();
        window.location = "/Home/YesAgreement";
    });
    $("#cancel").click(function (e) {
        e.preventDefault();
        window.location = "/Home/Index";
    });
});