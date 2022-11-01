using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Contracts.Database;

[Table("tbl_dogs", Schema = "public")]
public class Dog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("breed")]
    public string Breed { get; set; }

    [Column("size")]
    public string Size { get; set; }

    [Column("age")]
    public int Age { get; set; }

    [Column("about")]
    public string About { get; set; }

    [Column("row")]
    public int Row { get; set; }
    
    [Column("enclosure")]
    public int Enclosure { get; set; }

    [Column("title_photo")]
    public string TitlePhoto { get; set; }

    [Column("last_update")]
    public DateTime LastUpdate { get; set; }
    public ICollection<Tag> Tags { get; set; }
    public ICollection<Image> Photos { get; set; } 

}