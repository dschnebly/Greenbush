var body = document.getElementsByTagName("body")[0];
body.classList += ' ' + 'fadeInToBackground';

var el = document.getElementsByClassName("body-content")[0], c = el.style;
c.borderLeft = "1px solid #000";
c.borderRight = "1px solid #000";
c.boxShadow = "10px 10px 25px #333";
c.position = "relative";
c.top = "15px";
c.backgroundColor = "#ffffff";
c.marginBottom = "75px";

setTimeout(function () {
	var tp = document.getElementsByClassName("transition-page")[0];
	tp.style.display = "block";

	var className = 'transition-page-scaleUpCenter';
	if (el.classList) {
		el.classList.add(className);
	}
	else {
		el.className += ' ' + className;
	}
}, 200);

document.getElementsByClassName('transition-page')[0].addEventListener('click', function (e) {
	if (e && e.target && !(hasClass(e.target, 'exit') || hasClass(e.target, 'print'))) {
	}
});

function hasClass(target, className) {
	return new RegExp('(\\s|^)' + className + '(\\s|$)').test(target.className);
}

function PrintText() {
	var x = document.getElementsByClassName("form-print");
	var markup = "";
	var modPageElement = document.getElementsByClassName("module-page");
	var markup2 = "";
	var studentInfoElement = document.getElementsByClassName("studentInformationPage");

	if (modPageElement.length > 0)
		markup = modPageElement[0].innerHTML;

	if (studentInfoElement.length > 0)
		markup2 = studentInfoElement[0].innerHTML;

	$("#printText").val(markup);
	$("#studentText").val(markup2);
	$("#isArchive").val(0);	
	x[0].submit();
}

function ArchiveText() {
	var x = document.getElementsByClassName("form-print");
	var markup = "";
	var modPageElement = document.getElementsByClassName("module-page");
	var markup2 = "";
	var studentInfoElement = document.getElementsByClassName("studentInformationPage");

	if (modPageElement.length > 0)
		markup = modPageElement[0].innerHTML;

	if (studentInfoElement.length > 0)
		markup2 = studentInfoElement[0].innerHTML;

	$("#printText").val(markup);
	$("#studentText").val(markup2);
	$("#isArchive").val(1);
	x[0].submit();
}

