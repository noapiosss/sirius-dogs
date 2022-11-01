import Cropper from "./cropper/cropper.esm.js";

const myModal = document.getElementById('myModal')
const myInput = document.getElementById('myInput')

myModal.addEventListener('shown.bs.modal', () => {
  myInput.focus()
})


const fileInput = document.getElementById("file-input");

const imgPreview = document.createElement("img");
imgPreview.className = "input-image";
const imgCropped = document.createElement("img");
imgCropped.className = "cropped-image";

document.getElementById("image-handler").appendChild(imgPreview);
document.getElementById("image-handler").appendChild(imgCropped);

let croppedPhotoInput = document.getElementById("cropped-image");

let cropper = "";

fileInput.onchange = () =>
{    
    getImgData();
}

function getImgData() 
{
    const file = fileInput.files[0];
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
                zoomOnWheel: false,
                zoomOnTouch: false,
                rotatable: false,
                scalable: false,
                zoomable: false,
                crop: async function(event) {
                    let imgSrc = this.cropper.getCroppedCanvas().toDataURL("image/jpeg");
                    imgCropped.src = imgSrc;
                }
            });
        }, 500);
        
        const applyBtn = document.createElement("button");
        applyBtn.innerHTML = "Apply";
        applyBtn.type = "button";
        document.getElementById("image-handler").appendChild(applyBtn);
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
            document.querySelector("#image-handler > div").remove();
            applyBtn.remove();
        }

    });
      
}

