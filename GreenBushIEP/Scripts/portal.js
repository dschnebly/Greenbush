$(function () {
    function init() {

        // attach Event
        // fires when a user clicks on the new system user button
        $("#user-toggle").on("click", function () {
            $(".ajax-loader").css("visibility", "visible");
            $(".ajax-loader img").css("visibility", "visible");
        });

        // attach Event
        // fires when a user clicks on the edit user button
        $("#editStudent").on("click", function () {
            $(".ajax-loader").css("visibility", "visible");
            $(".ajax-loader img").css("visibility", "visible");
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
                console.log(userId);
                $.ajax({
                    type: 'POST',
                    url: '/Manage/Delete',
                    data: { id: userId },
                    dataType: "json",
                    success: function (data) {
                        var currentUser = $("div.list-group-item.bound[data-id='" + userId + "']");
                        $(currentUser).remove();

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

        $(".listrap").listrap().on("selection-changed", function (event, selection) {
            console.log(selection);
        });

        $('[data-command="toggle-search"]').on('click', function (event) {
            event.preventDefault();
            $(this).toggleClass('hide-search');

            if ($(this).hasClass('hide-search')) {
                $('.c-search').closest('.row').slideUp(100);
            } else {
                $('.c-search').closest('.row').slideDown(100);
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