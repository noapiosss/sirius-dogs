const ageSlider = document.getElementById("age-slider");
const selectedAgeLabel = document.getElementById("selected-age");
const showFiltersBtn = document.getElementById("show-filters-button");
const searchFilters = document.getElementById("search-filters");

document.onload = InitiateSlider();

document.getElementById("search-request").onfocus = () => { document.getElementById("search-form").style.boxShadow = "0px 0px 0px 5px rgba(0, 122, 255, 0.5)"; }
document.getElementById("search-request").onblur = () => { document.getElementById("search-form").style.boxShadow = ""; }

showFiltersBtn.onclick = async () => { searchFilters.hidden = !searchFilters.hidden; }

ageSlider.oninput = function setLabelValue()
{
    var monthCount = monthDiff(new Date(birthDates[ageSlider.value]), new Date(Date.now()));
    document.getElementById("filter-age-monthnumber").value = monthCount;
    if (monthCount < 12)
    {
        selectedAgeLabel.innerHTML = `${monthCount} month(s)`;
    }
    else
    {
        selectedAgeLabel.innerHTML = `${Math.round(monthCount/12)} year(s)`;
    }
}

function monthDiff(d1, d2) {
    var months;
    months = (d2.getFullYear() - d1.getFullYear()) * 12;
    months -= d1.getMonth();
    months += d2.getMonth();
    return months <= 0 ? 0 : months;
}

async function InitiateSlider()
{
    const pageLocation = wentHome ? "home" : "shelter";
    console.log(wentHome);

    birthDates = await fetch(`${window.location.origin}/api/birthdates/${pageLocation}`)
        .then(res => {return res.json()})
        .then(res => res.birthDates);

    birthDates.sort().reverse();
    console.log(birthDates);

    ageSlider.setAttribute("min", 0);
    ageSlider.setAttribute("max", birthDates.length-1);

    var monthCount = monthDiff(new Date(birthDates[ageSlider.value]), new Date(Date.now()));
    document.getElementById("filter-age-monthnumber").value = monthCount;
    if (monthCount < 12)
    {
        selectedAgeLabel.innerHTML = `${monthCount} month(s)`;
    }
    else
    {
        selectedAgeLabel.innerHTML = `${Math.round(monthCount/12)} year(s)`;
    }
}