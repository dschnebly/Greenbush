$(function () {

    $(".deleteNote").on("click", function () {

        var answer = confirm("Are you sure you want to archive this comment?");
        if (answer) {

            var cmtId = $(this).closest(".onecomment").find("input").val();
            if (cmtId !== null) {
                var stid = $('#stid').val();

                var jqxhr  = $.post('/Home/DeleteNote', { studentId: stid, commentId: cmtId }, function() {
                    window.location.reload();
                })
                .fail(function(data){
                    $("#alertMessage .moreinfo").html('Unable to connect to the server or other related problem. Please contact your admin.');
                    $("#alertMessage").fadeTo(3000, 500).slideUp(500, function () {
                        $("#alertMessage").slideUp(500);
                    });
                })
            }
        }
    });

    $(".notes-print").on("click", function () {
        window.print();
    });

});