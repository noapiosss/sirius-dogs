const backToListBtn = document.getElementById("back-to-list-button");
const backToDetailsBtn = document.getElementById("back-to-details-button");

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

backToDetailsBtn.onclick = () => 
{
    window.location.href = `${window.location.origin}/Dogs/Details/${backToDetailsBtn.getAttribute("dogId")}`;
}