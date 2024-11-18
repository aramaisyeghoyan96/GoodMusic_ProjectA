using Seido.Utilities.SeedGenerator;

namespace Models;
public class Artist : IArtist, ISeed<Artist>
{
    public virtual Guid ArtistId { get; set; }

    public virtual string FirstName { get; set; }
    public virtual string LastName { get; set; }

    public virtual DateTime? BirthDay { get; set; }

    //Navigation properties
    public virtual List<IMusicGroup> MusicGroups { get; set; } = new List<IMusicGroup>();

    public override string ToString() => $"{FirstName} {LastName}";

    #region Constructors
    public Artist(){}
    public Artist(Artist org)
    {
        this.Seeded = org.Seeded;

        this.ArtistId = org.ArtistId;
        this.FirstName = org.FirstName;
        this.LastName = org.LastName;
        this.BirthDay = org.BirthDay;
    }
    #endregion

    #region randomly seed this instance
    public virtual bool Seeded { get; set; } = false;
    public virtual Artist Seed(SeedGenerator seedGenerator)
    {
        Seeded = true;  
        ArtistId = Guid.NewGuid();

        FirstName = seedGenerator.FirstName;
        LastName = seedGenerator.LastName;
        BirthDay = (seedGenerator.Bool) ? seedGenerator.DateAndTime(1940, 1990) : null;

        return this;
    }
    #endregion
}


