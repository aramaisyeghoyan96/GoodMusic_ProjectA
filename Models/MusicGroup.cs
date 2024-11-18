using Seido.Utilities.SeedGenerator;

namespace Models;

public class MusicGroup : IMusicGroup, ISeed<MusicGroup>
{
    public virtual Guid MusicGroupId { get; set; }
    public virtual string Name { get; set; }
    public virtual int EstablishedYear { get; set; }

    public virtual string strGenre { get => Genre.ToString(); set { } }
    public virtual MusicGenre Genre { get; set; }

    //Navigation properties
    public virtual List<IAlbum> Albums { get; set; } = new List<IAlbum>();
    public virtual List<IArtist> Artists { get; set; } = new List<IArtist>();

    public override string ToString() =>
            $"{Name} with {Artists.Count} members was esblished {EstablishedYear} and made {Albums.Count} great albums.";

    #region Constructors
    public MusicGroup(){}
    public MusicGroup(MusicGroup org)
    {
        Seeded = org.Seeded;

        MusicGroupId = org.MusicGroupId;
        Name = org.Name;
        EstablishedYear = org.EstablishedYear;
        Genre = org.Genre;
    }
    #endregion

    #region randomly seed this instance
    public virtual bool Seeded { get; set; } = false;
    public virtual MusicGroup Seed(SeedGenerator seedGenerator)
    {
        Seeded = true;
        MusicGroupId = Guid.NewGuid();

        Name = seedGenerator.MusicGroupName;
        EstablishedYear = seedGenerator.Next(1970, 2024);
        Genre = seedGenerator.FromEnum<MusicGenre>();
        return this;
    }
    #endregion
}


