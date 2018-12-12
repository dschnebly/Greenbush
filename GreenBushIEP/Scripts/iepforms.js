$(".printForm").on("click", function (e) {

	var id = getUrlParameter('stid');

	if (id) {
		$('.ajax-loader').css("visibility", "visible");
		var pageName = this.id;
		window.location.href = '/Home/' + pageName + '/' + id;
	}
	else {
		$("#alertMessage .moreinfo").html('Server Error');
		$("#alertMessage").fadeTo(3000, 500).slideUp(500, function () {
			$("#alertMessage").slideUp(500);
		});
	}
});