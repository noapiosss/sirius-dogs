using System.ComponentModel.DataAnnotations;

namespace Contracts.Http
{
    public class DeletePhotoRequest
    {
        [Required]
        public int DogId { get; init; }

        [Required]
        public string PhotoUrl { get; init; }
    }

    public class DeletePhotoResponse
    {
        public bool DeleteIsSuccessful { get; init; }
    }
}