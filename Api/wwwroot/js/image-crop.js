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

let cropper = "";

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

document.getElementById("submit-all-photos").onclick = () =>
{
    document.getElementById("all-photos").files = allPhotos.files;
    document.getElementById("close-all-photos-modal").click();
}

titlePhotoInput.onchange = function getImgData() 
{
    document.getElementById("image-handler").replaceChildren();

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