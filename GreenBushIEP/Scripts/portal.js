$(function () {
    function init() {

        // attach Event
        // fires when a user clicks on the new system user button
        $("#user-toggle").on("click", function () {
            $(".ajax-loader").show();
            $(".ajax-loader img").show();
        });

        // attach Event
        // fires when a user clicks on the edit user button
		$("#editStudent").on("click", function () {
            $(".ajax-loader").show();
            $(".ajax-loader img").show();
        });

		// attach Event
		// fires when a user clicks on the edit user button
		$(".viewForms").on("click", function () {
			$(".ajax-loader").show();
			$(".ajax-loader img").show();
		});

        // attach event
        // finds all launchIEP on the page and fires when the link is clicked.
        $('.launchIEP').each(function (index) {
            $(this).not('.bound').addClass('bound').on("click", function (e) {
                e.preventDefault();
                $(".ajax-loader").show();
                $(".ajax-loader img").show();
            });
        });

        // attach event
        // fires when an delete button is pressed.
        $('#deleteUser').on('show.bs.modal', function (e) {
            var user = $(e.relatedTarget).data('id');
            $(e.currentTarget).find('input[name="id"]').val(user);
            $('#confirmDeletion').val('');
        });

        // attach event
        // fires when you click yes on the module deleteForm.
        $('#deleteUser button[type=submit]').on('click', function (e) {
            if ($('#confirmDeletion').val() === 'DELETE') {
                var userId = $(e.currentTarget).parent().parent().find('input[name="id"]').val();
                $.ajax({
                    type: 'POST',
                    url: '/Manage/Delete',
                    data: { id: userId },
                    dataType: "json",
                    success: function (data) {
                        var userRow = $("tr[data-id=" + userId + "]")
                        userRow.remove();

                        $("#alertMessage .moreinfo").html(data.Message);
                        $("#alertMessage").fadeTo(2000, 500).slideUp(500, function () {
                            $("#alertMessage").slideUp(500);
                        });
                    },
                    error: function (data) {
                        console.log("Unable to connect to the server or another related problem.");
                    }
                });
            }
		});

		
		
    }
    init();

    //SET UP 
    var params =	//All params are optional, you can just assign {} 
    {
        "navB": "slide",	//Effect for navigation button, leave it empty to disable it
        "but": false,		//Flag to enable transitions on button, false by default
        "cBa": function () { init(); } //callback function
    };
    new ft(params);
});

jQuery.fn.extend({
    listrap: function () {
        var listrap = this;
        listrap.getSelection = function () {
            var selection = new Array();
            listrap.children("li.active").each(function (ix, el) {
                selection.push($(el)[0]);
            });
            return selection;
        };
        var toggle = "li .listrap-toggle ";
        var selectionChanged = function () {
            $(this).parent().parent().toggleClass("active");
            listrap.trigger("selection-changed", [listrap.getSelection()]);
        };
        $(listrap).find(toggle + "img").on("click", selectionChanged);
        $(listrap).find(toggle + "span").on("click", selectionChanged);
        return listrap;
    }
});