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

	}
	init();

	////SET UP 
	//var params =	//All params are optional, you can just assign {} 
	//{
	//	"navB": "slide",	//Effect for navigation button, leave it empty to disable it
	//	"but": false,		//Flag to enable transitions on button, false by default
	//	"cBa": function () { init(); } //callback function
	//};
	//new ft(params);
});

//jQuery.fn.extend({
//	listrap: function () {
//		var listrap = this;
//		listrap.getSelection = function () {
//			var selection = new Array();
//			listrap.children("li.active").each(function (ix, el) {
//				selection.push($(el)[0]);
//			});
//			return selection;
//		};
//		var toggle = "li .listrap-toggle ";
//		var selectionChanged = function () {
//			$(this).parent().parent().toggleClass("active");
//			listrap.trigger("selection-changed", [listrap.getSelection()]);
//		};
//		$(listrap).find(toggle + "img").on("click", selectionChanged);
//		$(listrap).find(toggle + "span").on("click", selectionChanged);
//		return listrap;
//	}
//});