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

        int months = ((DateTime.Now.Year - birthDate.Year) * 12) + DateTime.Now.Month - birthDate.Month;
        return IsPlural(months) ? $"{months} months" : $"{months} month";
    }

    private static bool IsPlural(int number)
    {
        return number % 100 == 11 || number % 10 != 1;
    }

    public string PaginationHref(string query, int page)
    {
        if (string.IsNullOrEmpty(query))
        {
            return $"?page={page}";
        }

        while (query[^1] != '=')
        {
            _ = query.Remove(query.Length - 1);
        }

        return query + page;
    }

    public int GetMonthDifference(DateTime birthDate)
    {
        return ((DateTime.Now.Year - birthDate.Year) * 12) + DateTime.Now.Month - birthDate.Month;
    }
}