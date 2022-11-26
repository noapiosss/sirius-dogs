const backToListBtn = document.getElementById("back-to-list-button");

backToListBtn.onclick = () => 
{
    if (backToListBtn.getAttribute("wentHome"))
    {
        window.location.href = `${window.location.origin}/Dogs/Home`;
    }
    else
    {
        window.location.href = `${window.location.origin}/Dogs/Shelter`;
    }
}