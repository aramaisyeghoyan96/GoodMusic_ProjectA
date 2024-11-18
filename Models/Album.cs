using Seido.Utilities.SeedGenerator;

namespace Models;

public class Album : IAlbum, ISeed<Album>
{
    public virtual Guid AlbumId { get; set; }

    public virtual string Name { get; set; }
    public virtual int ReleaseYear { get; set; }
    public virtual long CopiesSold { get; set; }

    //Navigation properties
    public virtual IMusicGroup MusicGroup { get; set; } = null;

    #region Constructors
    public Album(){}
    public Album(Album org)
    {
        this.Seeded = org.Seeded;

        this.AlbumId = org.AlbumId;
        this.Name = org.Name;
        this.ReleaseYear = org.ReleaseYear;
        this.CopiesSold = org.CopiesSold;
    }
    #endregion

    #region randomly seed this instance
    public virtual bool Seeded { get; set; } = false;
    public virtual Album Seed(SeedGenerator seedGenerator)
    {
        Seeded = true;
        AlbumId = Guid.NewGuid();

        Name = seedGenerator.MusicAlbumName;
        CopiesSold = seedGenerator.Next(1_000, 1_000_000);
        ReleaseYear = seedGenerator.Next(1970, 2024);

        return this;
    }
    #endregion
}


