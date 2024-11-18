using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;

using Seido.Utilities.SeedGenerator;
using Configuration;
using Models;
using Models.DTO;


namespace DbModels;

[Table("MusicGroups", Schema = "supusr")]
public class MusicGroupDbM : MusicGroup, ISeed<MusicGroupDbM>
{
    [Key]       
    public override Guid MusicGroupId { get; set; }

    [Required]
    public override string Name { get; set; }

    #region correcting the Navigation properties migration error caused by using interfaces
    [NotMapped]
    public override List<IAlbum> Albums { get => AlbumsDbM?.ToList<IAlbum>(); set => new NotImplementedException(); }
    [JsonIgnore] 
    public virtual List<AlbumDbM> AlbumsDbM { get; set; } = null;
    [NotMapped]
    public override List<IArtist> Artists { get => ArtistsDbM?.ToList<IArtist>(); set => new NotImplementedException(); }
    [JsonIgnore] 
    public virtual List<ArtistDbM> ArtistsDbM { get; set; } = null;
    #endregion
    
    #region Constructors
    public MusicGroupDbM(){}
    public MusicGroupDbM(MusicGroupCUdto dto)
    {
        MusicGroupId = Guid.NewGuid();
        UpdateFromDTO(dto);
    }
    #endregion

    #region Update from DTO
    public MusicGroupDbM UpdateFromDTO(MusicGroupCUdto dto)
    {
        Name = dto.Name;
        EstablishedYear = dto.EstablishedYear;
        Genre = dto.Genre;

        return this;
    }
    #endregion

    #region randomly seed this instance
    public override MusicGroupDbM Seed(SeedGenerator seedGenerator)
    {
        base.Seed(seedGenerator);
        return this;
    }
    #endregion
}


