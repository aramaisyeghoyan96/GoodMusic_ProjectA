using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

using Seido.Utilities.SeedGenerator;
using Models;

namespace DbModels
{
    [Table("Artists", Schema = "supusr")]
    public class ArtistDbM: Artist, ISeed<ArtistDbM>
    {
        [Key]   
        public override Guid ArtistId { get; set; }
        
        [Required]
        public override string FirstName { get; set; }
        
        [Required]
        public override string LastName { get; set; }

        #region correcting the Navigation properties migration error caused by using interfaces
        [NotMapped]
        public override List<IMusicGroup> MusicGroups { get => MusicGroupsDbM?.ToList<IMusicGroup>(); set => new NotImplementedException(); }
        [JsonIgnore]
        public virtual List<MusicGroupDbM> MusicGroupsDbM { get; set; } = null;        
        #endregion
        
        #region Constructors
        public ArtistDbM()
        {
        }
        public ArtistDbM(ArtistCUdto dto)
        {
            ArtistId = Guid.NewGuid();
            UpdateFromDTO(dto);
        }
        #endregion

        #region Update from DTO
        public ArtistDbM UpdateFromDTO(ArtistCUdto dto)
        {
            this.FirstName = dto.FirstName;
            this.LastName = dto.LastName;
            this.BirthDay = dto.BirthDay;

            return this;
        }
        #endregion

        #region randomly seed this instance
        public override ArtistDbM Seed(SeedGenerator seedGenerator)
        {
            base.Seed(seedGenerator);
            return this;
        }
        #endregion
    }
}

