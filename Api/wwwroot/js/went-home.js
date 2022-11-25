const statusBtn = document.getElementById("status-button");
const editBtn = document.getElementById("edit-button");
const backToListBtn = document.getElementById("back-to-list-button");

editBtn.onclick = () => { window.location.href = `${window.location.origin}/Dogs/Edit/${statusBtn.getAttribute("dogId")}` }
backToListBtn.onclick = () => { window.location.href = `${window.location.origin}` }

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