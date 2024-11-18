using Microsoft.Extensions.Logging;

using DbRepos;
using Models;
using Models.DTO;

namespace Services;

public class MusicServiceDb : IMusicService
{
    private readonly MusicDbRepos _repo;
    private readonly ILogger<MusicServiceDb> _logger;

    #region constructors
    public MusicServiceDb(MusicDbRepos repo)
    {
        _repo = repo;
    }
    public MusicServiceDb(MusicDbRepos repo, ILogger<MusicServiceDb> logger):this(repo)
    {
        _logger = logger;
    }
    #endregion

    #region Simple 1:1 calls in this case, but as Services expands, this will no longer be the case
    public Task<GstUsrInfoAllDto> InfoAsync() => _repo.InfoAsync();
    public Task<GstUsrInfoAllDto> SeedAsync(int nrOfItems) => _repo.SeedAsync(nrOfItems);
    public Task<GstUsrInfoAllDto> RemoveSeedAsync(bool seeded) => _repo.RemoveSeedAsync(seeded);

    public Task<RespPageDto<IMusicGroup>> ReadMusicGroupsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize) => _repo.ReadMusicGroupsAsync(seeded, flat, filter, pageNumber, pageSize);
    public Task<IMusicGroup> ReadMusicGroupAsync(Guid id, bool flat) => _repo.ReadMusicGroupAsync(id, flat);
    public Task<IMusicGroup> DeleteMusicGroupAsync(Guid id) => _repo.DeleteMusicGroupAsync(id);
    public Task<IMusicGroup> UpdateMusicGroupAsync(MusicGroupCUdto item) => _repo.UpdateMusicGroupAsync(item);
    public Task<IMusicGroup> CreateMusicGroupAsync(MusicGroupCUdto item) => _repo.CreateMusicGroupAsync(item);

    public Task<RespPageDto<IAlbum>> ReadAlbumsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize) => _repo.ReadAlbumsAsync(seeded, flat, filter, pageNumber, pageSize);
    public Task<IAlbum> ReadAlbumAsync(Guid id, bool flat) => _repo.ReadAlbumAsync(id, flat);
    public Task<IAlbum> DeleteAlbumAsync(Guid id) => _repo.DeleteAlbumAsync(id);
    public Task<IAlbum> UpdateAlbumAsync(AlbumCUdto item) => _repo.UpdateAlbumAsync(item);
    public Task<IAlbum> CreateAlbumAsync(AlbumCUdto item) => _repo.CreateAlbumAsync(item);

    public Task<RespPageDto<IArtist>> ReadArtistsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize) => _repo.ReadArtistsAsync(seeded, flat, filter, pageNumber, pageSize);
    public Task<IArtist> ReadArtistAsync(Guid id, bool flat) => _repo.ReadArtistAsync(id, flat);
    public Task<IArtist> DeleteArtistAsync(Guid id) => _repo.DeleteArtistAsync(id);
    public Task<IArtist> UpdateArtistAsync(ArtistCUdto item) => _repo.UpdateArtistAsync(item);
    public Task<IArtist> CreateArtistAsync(ArtistCUdto item) => _repo.CreateArtistAsync(item);
    public Task<IArtist> UpsertArtistAsync(ArtistCUdto item) => _repo.UpsertArtistAsync(item);
    #endregion
}

