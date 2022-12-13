using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Contracts.Http
{
    public class GetBirthDatesRequest
    {
        [Required]
        public bool WentHome { get; init; }
    }

    public class GetBirthDatesResponse
    {
        public ICollection<DateTime> BirthDates { get; init; }
    }
}