import Cropper from "./cropper/cropper.esm.js";

const aboutTextarea = document.getElementById("about-textarea");
aboutTextarea.style.height = 0;
aboutTextarea.style.height = (aboutTextarea.scrollHeight) + "px";

aboutTextarea.oninput = () =>
{
    aboutTextarea.style.height = 0;
    aboutTextarea.style.height = (aboutTextarea.scrollHeight) + "px";
}

const titlePhotoInput = document.getElementById("title-photo-input");
const allPhotos = document.getElementById("all-photos-input");

let croppedPhotoInput = document.getElementById("cropped-image");

allPhotos.onchange = () =>
{
    const allPhotosLabels = document.getElementById("all-photos-labels");
    allPhotosLabels.replaceChildren();
    for (const photo of allPhotos.files)
    {        
        const photoData = document.createElement("label");
        photoData.className = `about-${photo.name}`;
        photoData.innerHTML = `${photo.name} ${photo.size/1000}b`;
        allPhotosLabels.appendChild(photoData);
        allPhotosLabels.appendChild(document.createElement("br"));
    };
}

document.getElementById("submit-all-photos").onclick = async () =>
{
    document.getElementById("all-photos").files = allPhotos.files;
    
    for (const photo of allPhotos.files)
    {
        const dogId = window.location.href.split('/')[window.location.href.split('/').length - 1];
        let dataToSend = new FormData();
        dataToSend.append("dogId", dogId);
        dataToSend.append("photo", photo);

        const newPhotoPath = await fetch(`${window.location.origin}/api/photos`,{
            method: 'POST',
            body: dataToSend
        }).then(response => response.json()).then(result => result.photoPath);

        console.log(newPhotoPath);

        const newPhotoContainer = document.createElement("div");
        newPhotoContainer.className = "photo-container";

        const newPhoto = document.createElement("img");
        newPhoto.className = "dog-photo-img";
        newPhoto.src = newPhotoPath;
        newPhotoContainer.appendChild(newPhoto);

        const newDeleteBtn = document.createElement("div");
        newDeleteBtn.className = "delete-photo-btn";
        newDeleteBtn.innerHTML = "[ delete ]";
        newDeleteBtn.onclick = async () => 
        {
            const delIsSubmited = confirm("Are you sure, that you wanna delete this photo?");
            if (!delIsSubmited) return;

            const photoForDelete = 
            {
                dogId: dogId,
                photoPath: newPhotoPath
            };
            await fetch(`${window.location.origin}/api/photos`,{
                method: 'DELETE',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(photoForDelete)
            });
            newDeleteBtn.parentElement.remove();
        }

        const newPhotoControlContainer = document.createElement("div");
        newPhotoControlContainer.className = "photo-control-container";

        newPhotoControlContainer.appendChild(newPhotoContainer);
        newPhotoControlContainer.appendChild(newDeleteBtn);

        const addPhotoControlContainer = document.getElementById("add-photo-control-container");
        addPhotoControlContainer.parentNode.insertBefore(newPhotoControlContainer, addPhotoControlContainer);
    };

    document.getElementById("close-all-photos-modal").click();
}

titlePhotoInput.onchange = function getImgData() 
{
    document.getElementById("image-handler").replaceChildren();
    let cropper = "";

    const imgPreview = document.createElement("img");
    imgPreview.className = "input-image";
    const imgCropped = document.createElement("img");
    imgCropped.className = "cropped-image";

    document.getElementById("image-handler").appendChild(imgPreview);
    document.getElementById("image-handler").appendChild(imgCropped);

    const file = titlePhotoInput.files[0];
    const fileReader = new FileReader();
    fileReader.readAsDataURL(file);
    fileReader.addEventListener("load", function () {
        imgPreview.src = this.result;
        
        setTimeout(() => {
            cropper = new Cropper(imgPreview, {
                aspectRatio: 2 / 3,
                minContainerWidth: imgPreview.width,
                minContainerHeight: imgPreview.height,
                viewModer: 1,
                center: false,
                zoomOnTouch: false,
                scalable: false,
                crop: function(event) {
                    let imgSrc = this.cropper.getCroppedCanvas().toDataURL("image/jpeg");
                    imgCropped.src = imgSrc;
                }
            });
        }, 500);
        
        const applyBtn = document.getElementById("submit-cropped-image");

        applyBtn.onclick = async () =>
        {            
            let croppedPhotoFile = await fetch(imgCropped.src)
                .then(res => res.blob())
                .then(blob => {
                    return new File([blob], "Title.jpg", blob);
                });
            const dataTransfer = new DataTransfer();
            dataTransfer.items.add(croppedPhotoFile);
            croppedPhotoInput.files = dataTransfer.files;
            document.getElementById("close-cropped-image-modal").click();
        }
    });      
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