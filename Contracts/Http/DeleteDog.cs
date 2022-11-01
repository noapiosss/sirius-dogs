using System.ComponentModel.DataAnnotations;

namespace Contracts.Http;

public class DeleteDogRequest
{
    public int DogId { get; init; }
}

public class DeleteDogResponse
{
    public bool DeletingIsSuccessful { get; set; }
}