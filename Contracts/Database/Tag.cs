using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contracts.Database;

[Table("tbl_tags", Schema = "public")]
public class Tag
{
    [Column("dog_id")]
    public int DogId { get; set; }
    public Dog Dog { get; set; }

    [Column("tag")]
    public string TagName { get; set; }
}