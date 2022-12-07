const ageSlider = document.getElementById("age-slider");
const showFiltersBtn = document.getElementById("show-filters-button");
const searchFilters = document.getElementById("search-filters");

showFiltersBtn.onclick = async () =>
{
    console.log("test")
    searchFilters.hidden = !searchFilters.hidden;

    if (!searchFilters.hidden)
    {
        const ageMinMax = await fetch(`${window.location.origin}/api/age`, {
            method: 'GET'
        }).then(response => response.json());

        ageSlider.setAttribute("min", monthDiff(new Date(ageMinMax.maxBirthDate), new Date(Date.now())));
        ageSlider.setAttribute("max", monthDiff(new Date(ageMinMax.minBirthDate), new Date(Date.now())));
        ageSlider.value = ageSlider.getAttribute("max");
        ageSlider.setAttribute("step", 1);

        setLabelValue(document.getElementById("selected-age"));
        ageSlider.oninput = () => 
        {
            setLabelValue(document.getElementById("selected-age"));
        };

        document.getElementById("search-request").onfocus = () =>
        {
            document.getElementById("search-form").style.boxShadow = "0px 0px 0px 5px rgba(0, 122, 255, 0.5)";
        }

        document.getElementById("search-request").onblur = () => 
        {
            document.getElementById("search-form").style.boxShadow = "";
        }
    }
}

function setLabelValue(label)
{
    if (ageSlider.value < 12)
    {
        label.innerHTML = `${ageSlider.value} month(s)`;
    }
    else
    {
        label.innerHTML = `${Math.round(ageSlider.value/12)} year(s)`;
    }
}

function monthDiff(d1, d2) {
    var months;
    months = (d2.getFullYear() - d1.getFullYear()) * 12;
    months -= d1.getMonth();
    months += d2.getMonth();
    return months <= 0 ? 0 : months;
}