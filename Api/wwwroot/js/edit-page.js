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

const allDeleteBtns = document.querySelectorAll("[id='delete-photo-btn']");

allDeleteBtns.forEach(btn => {
    btn.onclick = async () =>
    {
        const delIsSubmited = confirm("Are you sure, that you wanna delete this photo?");
        if (!delIsSubmited) return;

        const photoForDelete = 
        {
            dogId: `${btn.dataset.dogId}`,
            photoPath: `${btn.dataset.photoPath}`
        };
        await fetch(`${window.location.origin}/api/photos`,{
            method: 'DELETE',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(photoForDelete)
        });
        btn.parentElement.remove();
    }
});

const addPhotoButton = document.getElementById("add-photo-container");

addPhotoButton.addEventListener("mouseenter", () => {
    document.getElementById("add-photo-svg").setAttribute("fill", "black");
    document.getElementById("add-photo-container").style.borderColor = "black";
});

addPhotoButton.addEventListener("mouseleave", () => {
    document.getElementById("add-photo-svg").setAttribute("fill", "gray");
    document.getElementById("add-photo-container").style.borderColor = "rgb(194, 194, 194)";
});