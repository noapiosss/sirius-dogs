const bgC = "rgb(118, 164, 255)";

if (window.location.href.toLowerCase().includes("dogs/create"))
{
    document.getElementById("nav-link-add-dog").style.backgroundColor = bgC;
}

if (window.location.href.toLowerCase().includes("dogs/shelter"))
{
    document.getElementById("nav-link-shelter").style.backgroundColor = bgC;
}

if (window.location.href.toLowerCase().includes("dogs/home"))
{
    document.getElementById("nav-link-home").style.backgroundColor = bgC;
}

if (window.location.href.toLowerCase() === window.location.origin+"/")
{
    document.getElementById("navbar-brand").style.backgroundColor = bgC;
}