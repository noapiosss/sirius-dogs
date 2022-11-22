const aboutTextarea = document.getElementById("about-textarea");
aboutTextarea.style.height = 0;
aboutTextarea.style.height = (aboutTextarea.scrollHeight) + "px";

aboutTextarea.oninput = () =>
{
    aboutTextarea.style.height = 0;
    aboutTextarea.style.height = (aboutTextarea.scrollHeight) + "px";
}