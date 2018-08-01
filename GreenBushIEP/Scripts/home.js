$(function () {
    var emailfield = $("input[name=Email]");
    var passwordfield = $("input[name=Password]");

    $('input[type="submit"]').click(function (e) {
        e.preventDefault();

        if ($('input[type="submit"]').val() === "continue") {
            window.location = $('input[type="submit"]').attr("action");
        }
        else if (emailfield.val() !== "" && passwordfield.val() !== "") {
            var requestData = {
                Email: emailfield.val(),
                Password: passwordfield.val()
            };

            $.ajax({
                url: '/Account/Login',
                crossDomain: true,
                type: 'POST',
                data: JSON.stringify(requestData),
                dataType: 'json',
                contentType: "application/json; charset=utf-8",
                cache: false,
                success: function (data) {
                    if (data.success) {
                        $("#output").removeClass(' alert-danger');

                        $(".panel-default").addClass('slideOutLeft');
                        $(".jumbotron").fadeOut();
                        setTimeout(function () { window.location = data.portal; }, 500);
                    }
                    else {
                        $("#output").removeClass(' alert alert-success');
                        $("#output").addClass("alert alert-danger animated fadeInUp").html("Incorrect username or password.");
                    }
                },
                error: function (data) {
                    $("#output").removeClass(' alert alert-success');
                    $("#output").addClass("alert alert-danger animated fadeInUp").html("Incorrect username or password.");
                }
            });
        }
        else {
            $("#output").removeClass(' alert alert-success');
            $("#output").addClass("alert alert-danger animated fadeInUp").html("Incorrect username or password.");
        }
    });
});