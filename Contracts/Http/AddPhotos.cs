using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;

namespace Contracts.Http;

public class AddPhotosRequest
{
    [Required]
    public int DogId { get; init; }

    [Required]
    public IFormFile Photo { get; init; }
}

public class AddPhotosResponse
{
    public string PhotoPath { get; init; }
}