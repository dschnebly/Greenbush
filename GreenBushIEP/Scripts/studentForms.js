$(function () {
	function init() {

		$(".name a, .printForm").not(".bound").addClass("bound").on("click", function (e) {
			var id = $("#stid").val();

			if (id) {
				$('.ajax-loader').css("visibility", "visible");

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



		$(document).ready(function () {
			
			$('#files').fileupload({
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
								content += '<td class=\"date\"><button type=\"button\" onclick=\"window.open(\'/Home/DownloadArchive?id=' + json[i].id  + '\');\" class=\"btn btn-default btn-lg downloadForm\" id=\"' + json[i].id + '\"><i class=\"fa fa-print\"></i> <span>&nbsp;Download</span></button></td>';								
								content += '</tr>';
							}
							$('#tblArchives tbody').html(content); 
						
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

		});//end document ready
	

	}
		
	init();
	function _showAlert(message, positive) {

		if ($("#alertMessage").css('display') && $("#alertMessage").css('display') === 'none') {
			if (positive) { $("#alertMessage").removeClass('alert-danger').addClass('alert-success'); }
			else { $("#alertMessage").removeClass('alert-success').addClass('alert-danger'); }
			$("#alertMessage .moreinfo").html(message);
			$("#alertMessage").fadeTo(3000, 1000).slideUp(2000, function () {
				$("#alertMessage").slideUp(5000);
			});
		}
	}

});
