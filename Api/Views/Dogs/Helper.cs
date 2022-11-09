using System;

using Microsoft.AspNetCore.Mvc.Razor;

public abstract class Helper<TModel> : RazorPage<TModel>
{
    public string GetAge(DateTime birthDate)
    {
        birthDate = birthDate.ToLocalTime();
        if ((DateTime.Now - birthDate).Days > 365)
        {
            int years;
            if (DateTime.Now.DayOfYear > birthDate.DayOfYear)
            {
                years = DateTime.Now.Year - birthDate.Year;
                return IsPlural(years) ? $"{years} years" : $"{years} year";
            }
            years = DateTime.Now.Year - birthDate.Year - 1;
            return IsPlural(years) ? $"{years} years" : $"{years} year";
        }

        int months = (DateTime.Now.Year - birthDate.Year) * 12 + DateTime.Now.Month - birthDate.Month;
        return IsPlural(months) ? $"{months} months" : $"{months} month";
    }

    private bool IsPlural(int number)
    {
        if (number % 100 == 11) return true;
        if (number % 10 == 1) return false;
        return true; 
    }
}