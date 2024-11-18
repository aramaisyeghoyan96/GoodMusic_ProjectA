using System;
using Models;
using Models.DTO;

namespace Services;

public interface IMusicService
{
    //Full set of async methods
    public Task<GstUsrInfoAllDto> InfoAsync();
    public Task<GstUsrInfoAllDto> SeedAsync(int nrOfItems);
    public Task<GstUsrInfoAllDto> RemoveSeedAsync(bool seeded);

    public Task<RespPageDto<IMusicGroup>> ReadMusicGroupsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize);
    public Task<IMusicGroup> ReadMusicGroupAsync(Guid id, bool flat);
    public Task<IMusicGroup> DeleteMusicGroupAsync(Guid id);
    public Task<IMusicGroup> UpdateMusicGroupAsync(MusicGroupCUdto item);
    public Task<IMusicGroup> CreateMusicGroupAsync(MusicGroupCUdto item);

    public Task<RespPageDto<IAlbum>> ReadAlbumsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize);
    public Task<IAlbum> ReadAlbumAsync(Guid id, bool flat);
    public Task<IAlbum> DeleteAlbumAsync(Guid id);
    public Task<IAlbum> UpdateAlbumAsync(AlbumCUdto item);
    public Task<IAlbum> CreateAlbumAsync(AlbumCUdto item);

    public Task<RespPageDto<IArtist>> ReadArtistsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize);
    public Task<IArtist> ReadArtistAsync(Guid id, bool flat);
    public Task<IArtist> DeleteArtistAsync(Guid id);
    public Task<IArtist> UpdateArtistAsync(ArtistCUdto item);
    public Task<IArtist> CreateArtistAsync(ArtistCUdto item);
    public Task<IArtist> UpsertArtistAsync(ArtistCUdto item);
}


