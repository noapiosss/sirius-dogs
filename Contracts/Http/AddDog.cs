using System.ComponentModel.DataAnnotations;

namespace Contracts.Http;

public class AddDogRequest
{
    public string Name { get; init; }
    public string Breed { get; init; }
    public string Size { get; init; }
    public int Age { get; init; }
    public string About { get; init; }    
    public int Row { get; set; }
    public int Enclosure { get; set; }
}

public class AddDogResponse
{
    public bool AddingIsSuccessful { get; set; }
}