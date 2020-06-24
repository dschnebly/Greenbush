$(function () {
    function init() {

        $(".name a, .printForm").not(".bound").addClass("bound").on("click", function (e) {
            var id = $("#stid").val();

            if (id) {
                //$('.ajax-loader').css("visibility", "visible");

                var pageName = this.id;
                window.location.href = '/Home/IEPFormFile?id=' + id + '&fileName=' + pageName;
            }
            else {
                $("#alertMessage .moreinfo").html('Server Error');
                $("#alertMessage").fadeTo(3000, 500).slideUp(500, function () {
                    $("#alertMessage").slideUp(500);
                });
            }
        });

        $(".name a, .deleteForm").not(".bound").addClass("bound").on("click", function (e) {
            var button = $(this);
            var id = $("#stid").val();
            var formid = $(this).data("formid");

            $('.ajax-loader').css("visibility", "visible");

            $.ajax({
                type: 'GET',
				url: '/Home/DeleteUploadForm',
                data: { studentId: id, formId: formid },
                dataType: 'json',
                success: function (data) {
                    if (data.Result === 'success') {

                        button.closest("tr").remove();

                    } else {

                        $(".ajax-loader img").css("visibility", "hidden");
                        $(".ajax-loader .failure").show().fadeOut(1000, "linear", function () {
                            $("#alertMessage .moreinfo").html('There was an error while trying to delete the data.');
                        });
                    }
                },
                error: function (data) {
                    alert("Unable to connect to the server or other related network problem. Please contact your admin.");
                },
                complete: function () {
                    $('.ajax-loader').css("visibility", "hidden");
                }
            });
        });

        $("#hideArchived").on("change", function () {
            if ($(this).val() == 2) {
                $('table tr.notActiveForm').removeClass("hidden");
                $('table tr.ActiveForm').addClass("hidden"); 
            } else {
                $('table tr.notActiveForm').addClass("hidden");
                $('table tr.ActiveForm').removeClass("hidden"); 

            }
        });        
		
        $(".closeForms").on("click", function (e) {
            window.location.href = '/Home/TeacherPortal';
        });

        $(".downloadForm").on("click", function (e) {
            var id = this.id;

            if (id) {
                window.location.href = '/Home/DownloadArchive?id=' + id;
            }
            else {
                $("#alertMessage .moreinfo").html('Server Error');
                $("#alertMessage").fadeTo(3000, 500).slideUp(500, function () {
                    $("#alertMessage").slideUp(500);
                });
            }
		});


		$(".deleteArchiveForm").on("click", function (e) {
			var button = $(this);
			var id = button.attr("data-val");
			var documentName = $(this).attr("data-name");
			var answer = confirm("Are you sure you want to delete '" + documentName + "'?");

			if (answer) {				
				$.ajax({
					type: 'GET',
					url: '/Home/DeleteArchive',
					data: { id: id},
					dataType: "json",
					async: false,
					success: function (data) {
						if (data.Result) {
							button.closest("tr").remove();
							_showAlert(data.Message, true);							
						} else {
							_showAlert(data.Message, false);
						}
					},
					error: function (data) {
						_showAlert(data.Message, false);
					}
				});
			}

			return false;
		});

        $(document).ready(function () {

            $("input[name='myFile']").fileupload({
                dataType: 'json',
                url: '/Home/UploadStudentFile',
                acceptFileTypes: /(\.|\/)(pdf)$/i,
                formData: {
                    studentId: $("#stid").val(),
                },
                done: function (e, data) {

                    if (data.result.result) {
                        _showAlert(data.result.message, true);

                        //update tblArchives
                        var json = data.result.archives;
                        var content = '';
                        for (var i = 0; i < json.length; i++) {
                            content += '<tr>';
                            content += '<td>' + json[i].fileName + '</td>';
                            content += '<td>' + json[i].fileDate + '</td>';
                            content += '<td class=\"date pull-right\"><button type=\"button\" class=\"btn btn-default btn-lg downloadForm\" id=\"' + json[i].id + '\"><i class=\"fa fa-download\"></i> <span>&nbsp;Download</span></button></td>';
                            content += '<td class=\"date\"><button type=\"button\" data-formid=\"' + json[i].id + '\" class=\"btn btn-default btn-lg deleteForm \" id=\"' + json[i].id + '\"><i class="fa fa-remove"></i><span>&nbsp;Delete</span></button></td>';
                            content += '</tr>';
                        }
                        $('#tblUploads tbody').html(content);
                        init();
                    }
                    else {
                        _showAlert(data.result.message, false);
                    }
                },
                progressall: function (e, data) {

                    var progress = parseInt(data.loaded / data.total * 100, 10);
                    $('#progress .progress-bar').css(
                        'width',
                        progress + '%'
                    );
                }

			});//end file upload

			var id = getUrlParameter('saved');
			if (id == 2) {
				$("#alertMessage .moreinfo").html('There was an error while trying to save the data.');
				$("#alertMessage").show();
			
			}

        });//end document ready
    }

	init();

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

	function _showAlert(message, positive) {
		
        if ($("#alertMessage").css('display') && $("#alertMessage").css('display') === 'none') {
			if (positive)
			{
				$("#alertMessage").removeClass('alert-danger').addClass('alert-success');
			}
			else {
				$("#alertMessage").removeClass('alert-success').addClass('alert-danger');
			}

            $("#alertMessage .moreinfo").html(message);
            $("#alertMessage").fadeTo(3000, 1000).slideUp(2000, function () {
                $("#alertMessage").slideUp(5000);
            });
        }
    }

});
