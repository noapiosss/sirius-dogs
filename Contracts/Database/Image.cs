using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contracts.Database;

[Table("tbl_images", Schema = "public")]
public class Image
{
    [Column("dog_id")]
    public int DogId { get; set; }
    public Dog Dog { get; set; }

    [Column("photo_path")]
    public string PhotoPath { get; set; }
}