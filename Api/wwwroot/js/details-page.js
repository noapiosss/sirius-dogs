const statusBtn = document.getElementById("status-button");
const editBtn = document.getElementById("edit-button");
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

statusBtn.onclick = async () =>
{
    if (statusBtn.className === "btn btn-danger")
    {
        await fetch(`${window.location.origin}/api/${statusBtn.getAttribute("dogId")}/backtoshelter`, {
            method: 'POST'
        }).then(() => {
            statusBtn.className = "btn btn-success";
            statusBtn.innerHTML = "Went home";
        });
    }
    else
    {
        await fetch(`${window.location.origin}/api/${statusBtn.getAttribute("dogId")}/wenthome`, {
            method: 'POST'
        }).then(() =>
        {
            statusBtn.className = "btn btn-danger";
            statusBtn.innerHTML = "Back to shelter";
        })
    }
}

editBtn.onclick = () => { window.location.href = `${window.location.origin}/Dogs/Edit/${editBtn.getAttribute("dogId")}` }