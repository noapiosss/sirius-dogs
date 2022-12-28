const ageSlider = document.getElementById("age-slider");
const selectedAgeLabel = document.getElementById("selected-age");
const showFiltersBtn = document.getElementById("show-filters-button");
const searchFilters = document.getElementById("search-filters");

document.onload = InitiateSlider();

document.getElementById("search-request").onfocus = () => { document.getElementById("search-form").style.boxShadow = "0px 0px 5px 2px rgba(0, 122, 255, 0.5)"; }
document.getElementById("search-request").onblur = () => { document.getElementById("search-form").style.boxShadow = ""; }

showFiltersBtn.onclick = async () => 
{ 
    searchFilters.hidden = !searchFilters.hidden; 
    if (!searchFilters.hidden)
    {
        showFiltersBtn.innerHTML = "hide filters"
        return;
    }
    showFiltersBtn.innerHTML = "show filters"
}

ageSlider.oninput = function setLabelValue()
{
    var monthCount = birthDates[ageSlider.value];
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

    birthDates = await fetch(`${window.location.origin}/api/birthdates/${pageLocation}`)
        .then(res => {return res.json()})
        .then(res => res.birthDates);

    birthDates.sort().reverse();
    for (let i = 0; i<birthDates.length; i++)
    {
        birthDates[i] = monthDiff(new Date(birthDates[i]), new Date(Date.now()))
    }

    ageSlider.setAttribute("min", 0);
    ageSlider.setAttribute("max", birthDates.length-1);

    if (!window.location.href.includes("searchRequest"))
    {
        var monthCount = birthDates[ageSlider.value];
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
    else
    {
        var searchRequest = decodeURIComponent(window.location.href.split("?")[1].split("&").filter(n => n.includes("searchRequest"))[0].split("=")[1]);
        var monthCount = window.location.href.split("?")[1].split("&").filter(n => n.includes("age"))[0].split("=")[1];
        var filterRow = window.location.href.split("?")[1].split("&").filter(n => n.includes("row"))[0].split("=")[1];
        var filterEnclosure = window.location.href.split("?")[1].split("&").filter(n => n.includes("enclosure"))[0].split("=")[1];
        var gender = window.location.href.split("?")[1].split("&").filter(n => n.includes("gender"))[0].split("=")[1];
        var size = window.location.href.split("?")[1].split("&").filter(n => n.includes("size"))[0].split("=")[1];

        document.getElementById("search-request").value = searchRequest;
        document.getElementById("row-filter").value = filterRow;
        document.getElementById("enclousre-filter").value = filterEnclosure;
        document.getElementById("gender-filter").value = gender;
        document.getElementById("size-filter").value = size;

        document.getElementById("filter-age-monthnumber").value = monthCount;
        if (monthCount < 12)
        {
            selectedAgeLabel.innerHTML = `${monthCount} month(s)`;
        }
        else
        {
            selectedAgeLabel.innerHTML = `${Math.round(monthCount/12)} year(s)`;
        }

        ageSlider.value = birthDates.indexOf(Number(monthCount));
    }

}