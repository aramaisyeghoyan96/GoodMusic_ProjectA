namespace Models;

public interface IAlbum
{
    public Guid AlbumId { get; set; }

    public string Name { get; set; }
    public int ReleaseYear { get; set; }
    public long CopiesSold { get; set; }

    public IMusicGroup MusicGroup { get; set;} 
}

public interface IArtist
{
    public Guid ArtistId { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }

    public DateTime? BirthDay { get; set; }

    public List<IMusicGroup> MusicGroups { get; set; }
}

public enum MusicGenre {Rock, Blues, Jazz, Metal }
public interface IMusicGroup
{
    public Guid MusicGroupId { get; set; }
    public string Name { get; set; }
    public int EstablishedYear { get; set; }

    public string strGenre { get; set; }
    public MusicGenre Genre { get; set; }

    public List<IAlbum> Albums { get; set; }
    public List<IArtist> Artists { get; set; }
}
