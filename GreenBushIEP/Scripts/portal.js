$(function () {
    function init() {

        // attach Event
        // fires when a user clicks on the main new system user button
        $("#user-toggle").on("click", function () {
            $(".ajax-loader").css("visibility", "visible");
            $(".ajax-loader img").css("visibility", "visible");
        })

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