// document.header.onload = () =>
// {
//     if (window.location.href.toLowerCase().includes("dogs/create"))
//     {
//         document.getElementById("add-new-dog-li").style.textDecorationColor = "blue";
//     }
//     if (window.location.href.toLowerCase().includes("dogs/shelter"))
//     {
//         document.getElementById("shelter-li").style.background = "rgba(0,0,255,0.1)";
//     }
//     if (window.location.href.toLowerCase().includes("dogs/home"))
//     {
//         document.getElementById("home-li").style.background = "rgba(0,0,255,0.1)";
//     }
//     // if (window.location.href.toLowerCase().includes("dogs/create"))
//     // {
//     //     document.getElementById("add-new-dog-li").style.background = "rgba(0,0,255,0.1)";
//     // }
// }

window.onload = () =>
{
    const bgC = "rgb(118, 164, 255)"
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
}