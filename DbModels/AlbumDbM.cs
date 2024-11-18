using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

using Seido.Utilities.SeedGenerator;
using Models;

namespace DbModels;

[Table("Albums", Schema = "supusr")]
public class AlbumDbM : Album, ISeed<AlbumDbM>
{
    [Key]
    public override Guid AlbumId { get; set; }
    
    [Required]
    public override string Name { get; set; }
    
    #region correcting the Navigation properties migration error caused by using interfaces
    [NotMapped]
    public override IMusicGroup MusicGroup { get => MusicGroupDbM; set => new NotImplementedException(); }
    [JsonIgnore] 
    
    [Required]
    public virtual MusicGroupDbM MusicGroupDbM { get; set; } = null;
    #endregion

    #region Constructors
    public AlbumDbM() {}
    public AlbumDbM(AlbumCUdto dto)
    {
        AlbumId = Guid.NewGuid();
        UpdateFromDTO(dto);
    }
    #endregion

    #region Update from DTO
    public AlbumDbM UpdateFromDTO(AlbumCUdto dto)
    {
        Name = dto.Name;
        ReleaseYear = dto.ReleaseYear;
        CopiesSold = dto.CopiesSold;

        return this;
    }
    #endregion

    #region randomly seed this instance
    public override AlbumDbM Seed(SeedGenerator sgen)
    {
        base.Seed(sgen);
        return this;
    }
    #endregion
}


