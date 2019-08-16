$(document).ready(function () {

    init();
    	
});

function init() {
	
	$(".delReferral").on("click", function () {
		var answer = confirm("Are you sure you want to delete this Referral?");
		if (answer) {
			var row = $(this).closest('tr');
			var referralId = $(this).attr('data-val');
			var args = { referralId: referralId };
			$.ajax({
				url: '/Manage/DeleteReferral',
				type: 'POST',
				data: args,
				success: function (data) {
					if (data.Result === "success") {
						row.hide();
					} else {
						alert(data.Message);
					}
				},
				error: function (data) {
					alert("There was an error when attempting to connect to the server.");
				}
			});
		}

		return false;
	});	 

	$("#filterList").on("change", function () {
		var selectedVal = parseInt($("#filterList").val());
		getData(selectedVal);
	});	 
	
}

function getData(searchVal) {

	var args = { searchType: searchVal};

	$.ajax({
		url: '/Manage/FilterReferrals',
		type: 'POST',
		data: args,
		success: function (data) {
			if (data.Result === "success") {

				var content = '';
				
				for (m = 0; m < data.FilterList.length; m++) {
					content += '<tr>';
					content += '<td class="name truncate">' + data.FilterList[m].submitDate + '</td>';
					content += '<td class="name truncate">';

					if (data.FilterList[m].isComplete == 1)
						content += '<text><span class="text-success">Yes</span></text>';
					else
						content += '<text><span class="text-danger">No</span></text>';

					content += '</td>';
					content += '<td class="name truncate">' + data.FilterList[m].kidsId + '</td>';
					content += '<td class="name truncate">' + data.FilterList[m].lastName + '</td>';
					content += '<td class="name truncate">' + data.FilterList[m].firstName + '</td>';
					content += '<td class="name truncate">' + data.FilterList[m].notes + '</td>';
					content += '<td  style="vertical-align:top">';
					content += '<a title="View" class="btn btn-info " role="button" href="/Manage/EditReferral/' + data.FilterList[m].referralId +'" data-toggle="tooltip" "><span class="glyphicon glyphicon-pencil"></span></a>';
					content += '</td>';
					content += '<td style="vertical-align:top;"><a title="Delete" class="btn btn-info delReferral" role="button" data-val="' + data.FilterList[m].referralId +'" style="border: none!important;" data-toggle="tooltip" ><span class="glyphicon glyphicon-trash"></span></a>';
					content += '</td>';
					content += '</tr>';
				}
				
				$('#tblRefferals tbody').html(content); 





			} else {
				alert(data.Message);
			}
		},
		error: function (data) {
			alert("There was an error when attempting to connect to the server.");
		}
	});

}



