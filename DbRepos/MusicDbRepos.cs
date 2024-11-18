using Seido.Utilities.SeedGenerator;
using Configuration;
using Models;
using Models.DTO;
using DbModels;
using DbContext;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Reflection.Metadata;

//DbRepos namespace is a layer to abstract the detailed plumming of
//retrieveing and modifying and data in the database using EFC.

//DbRepos implements database CRUD functionality using the DbContext
namespace DbRepos;

public class MusicDbRepos
{
    const string _seedSource = "./master-seeds.json";
    private ILogger<MusicDbRepos> _logger;
    private readonly MainDbContext _dbContext;

    #region contructors
    public MusicDbRepos(ILogger<MusicDbRepos> logger, MainDbContext context)
    {
        _logger = logger;
        _dbContext = context;
    }
    #endregion

    #region Admin repo methods
    public async Task<GstUsrInfoAllDto> InfoAsync()
    {
        return await DbInfo(_dbContext);
    }

    private static async Task<GstUsrInfoAllDto> DbInfo(MainDbContext db)
    {
        var info = new GstUsrInfoAllDto();
        info.Db = await db.vwInfoDb.FirstAsync();

        return info;
    }

    public async Task<GstUsrInfoAllDto> SeedAsync(int nrOfItems)
    {
        //Create a seeder
        var fn = Path.GetFullPath(_seedSource);
        var seeder = new SeedGenerator(fn);

        //get a list of music groups
        var musicGroups = seeder.ItemsToList<MusicGroupDbM>(nrOfItems);

        //Set between 5 and 50 albums for each music groups
        musicGroups.ForEach(mg => mg.AlbumsDbM = seeder.ItemsToList<AlbumDbM>(seeder.Next(2, 5)));

        //get a list of artists
        var artists = seeder.ItemsToList<ArtistDbM>(100);

        //Assign artists to Music groups
        musicGroups.ForEach(mg => mg.ArtistsDbM = seeder.UniqueIndexPickedFromList<ArtistDbM>(seeder.Next(2, 5), artists));

        //Note that all other tables are automatically set through csFriendDbM Navigation properties
        _dbContext.MusicGroups.AddRange(musicGroups);

        await _dbContext.SaveChangesAsync();
        return  await DbInfo(_dbContext);
    }
    
    public async Task<GstUsrInfoAllDto> RemoveSeedAsync(bool seeded)
    {
        var parameters = new List<SqlParameter>();

        var retValue = new SqlParameter("retval", SqlDbType.Int) { Direction = ParameterDirection.Output };
        var retSeeded = new SqlParameter("seeded", seeded);
        var nrM = new SqlParameter("nrM", SqlDbType.Int) { Direction = ParameterDirection.Output };
        var nrAl = new SqlParameter("nrAl", SqlDbType.Int) { Direction = ParameterDirection.Output };
        var nrAr = new SqlParameter("nrAr", SqlDbType.Int) { Direction = ParameterDirection.Output };

        parameters.Add(retValue);
        parameters.Add(retSeeded);
        parameters.Add(nrM);
        parameters.Add(nrAl);
        parameters.Add(nrAr);

        //there is no FromSqlRawAsync to I make one here
        var query = await Task.Run(() =>
            _dbContext.vwInfoDb.FromSqlRaw($"EXEC @retval = supusr.spDeleteAll @seeded," +
                $"@nrM OUTPUT, @nrAl OUTPUT, " +
                $"@nrAr OUTPUT", parameters.ToArray()).AsEnumerable());


        //Not using result in this code, just to show how to get the result set
        var _ = query.FirstOrDefault();

        //Check the return code
        int _retcode = (int)retValue.Value;
        if (_retcode != 0) throw new DataException("supusr.spDeleteAll return code error");

        return  await DbInfo(_dbContext);
    }
    #endregion
    
    #region MusicGroup repo methods
    public async Task<RespPageDto<IMusicGroup>> ReadMusicGroupsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize)
    {
        filter ??= "";
        IQueryable<MusicGroupDbM> query;
        if (flat)
        {
            query = _dbContext.MusicGroups.AsNoTracking();
        }
        else
        {
            query = _dbContext.MusicGroups.AsNoTracking()
                .Include(i => i.ArtistsDbM)
                .Include(i => i.AlbumsDbM);
        }

        var ret = new RespPageDto<IMusicGroup>()
        {
            DbItemsCount = await query

            //Adding filter functionality
            .Where(i => (i.Seeded == seeded) && 
                        (i.Name.ToLower().Contains(filter) ||
                            i.strGenre.ToLower().Contains(filter) ||
                            i.EstablishedYear.ToString().Contains(filter))).CountAsync(),

            PageItems = await query

            //Adding filter functionality
            .Where(i => (i.Seeded == seeded) && 
                        (i.Name.ToLower().Contains(filter) ||
                            i.strGenre.ToLower().Contains(filter) ||
                            i.EstablishedYear.ToString().Contains(filter)))

            //Adding paging
            .Skip(pageNumber * pageSize)
            .Take(pageSize)

            .ToListAsync<IMusicGroup>(),

            PageNr = pageNumber,
            PageSize = pageSize
        };
        return ret;
    }

    public async Task<IMusicGroup> ReadMusicGroupAsync(Guid id, bool flat)
    {
        if (!flat)
        {
            //make sure the model is fully populated, try without include.
            //remove tracking for all read operations for performance and to avoid recursion/circular access
            var query = _dbContext.MusicGroups.AsNoTracking()
                .Include(i => i.ArtistsDbM)
                .Include(i => i.AlbumsDbM)
                .Where(i => i.MusicGroupId == id);

            return await query.FirstOrDefaultAsync<IMusicGroup>();
        }
        else
        {
            //Not fully populated, compare the SQL Statements generated
            //remove tracking for all read operations for performance and to avoid recursion/circular access
            var query = _dbContext.MusicGroups.AsNoTracking()
                .Where(i => i.MusicGroupId == id);

            return await query.FirstOrDefaultAsync<IMusicGroup>();
        }   
    }

    public async Task<IMusicGroup> DeleteMusicGroupAsync(Guid id)
    {
        //Find the instance with matching id
        var query1 = _dbContext.MusicGroups
            .Where(i => i.MusicGroupId == id);
        var item = await query1.FirstOrDefaultAsync<MusicGroupDbM>();

        //If the item does not exists
        if (item == null) throw new ArgumentException($"Item {id} is not existing");

        //delete in the database model
        _dbContext.MusicGroups.Remove(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();
        return item;  
    }

    public async Task<IMusicGroup> UpdateMusicGroupAsync(MusicGroupCUdto itemDto)
    {
        //Find the instance with matching id and read the navigation properties.
        var query1 = _dbContext.MusicGroups
            .Where(i => i.MusicGroupId == itemDto.MusicGroupId);
        var item = await query1
            .Include(i => i.ArtistsDbM)
            .Include(i => i.AlbumsDbM)
            .FirstOrDefaultAsync<MusicGroupDbM>();

        //If the item does not exists
        if (item == null) throw new ArgumentException($"Item {itemDto.MusicGroupId} is not existing");

        //transfer any changes from DTO to database objects
        //Update individual properties
        item.UpdateFromDTO(itemDto);

        //Update navigation properties
        await navProp_csMusicGroupCUdto_To_csMusicGroup(_dbContext, itemDto, item);

        //write to database model
        _dbContext.MusicGroups.Update(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();

        //return the updated item in non-flat mode
        return await ReadMusicGroupAsync(item.MusicGroupId, false);    
    }

    public async Task<IMusicGroup> CreateMusicGroupAsync(MusicGroupCUdto itemDto)
    {
        if (itemDto.MusicGroupId != null)
            throw new ArgumentException($"{nameof(itemDto.MusicGroupId)} must be null when creating a new object");

        //transfer any changes from DTO to database objects
        //Update individual properties. Seeded always false on created items
        itemDto.Seeded = false; 
        var item = new MusicGroupDbM(itemDto);

        //Update navigation properties
        await navProp_csMusicGroupCUdto_To_csMusicGroup(_dbContext, itemDto, item);

        //write to database model
        _dbContext.MusicGroups.Add(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();
        
        //return the updated item in non-flat mode
        return await ReadMusicGroupAsync(item.MusicGroupId, false);   
    }

    //from all Guid relationships in _itemDtoSrc finds the corresponding object in the database and assigns it to _itemDst 
    //as navigation properties. Error is thrown if no object is found corresponing to an id.
    private static async Task navProp_csMusicGroupCUdto_To_csMusicGroup(MainDbContext db, MusicGroupCUdto itemDtoSrc, MusicGroupDbM itemDst)
    {
        //Navigation prop Albums
        List<AlbumDbM> albums = new List<AlbumDbM>();
        foreach (var id in itemDtoSrc.AlbumsId)
        {
            var album = await db.Albums.FirstOrDefaultAsync(a => a.AlbumId == id);

            if (album == null)
                throw new ArgumentException($"Item id {id} not existing");

            albums.Add(album);
        }
        itemDst.AlbumsDbM = albums;

        //Navigation prop Artist
        List<ArtistDbM> artists = new List<ArtistDbM>();
        foreach (var id in itemDtoSrc.ArtistsId)
        {
            var artist = await db.Artists.FirstOrDefaultAsync(a => a.ArtistId == id);

            if (artist == null)
                throw new ArgumentException($"Item id {id} not existing");

            artists.Add(artist);
        }

        itemDst.ArtistsDbM = artists;
    }
    #endregion

    #region Albums repo methods
    public async Task<IAlbum> ReadAlbumAsync(Guid id, bool flat)
    {
        if (!flat)
        {
            //make sure the model is fully populated, try without include.
            //remove tracking for all read operations for performance and to avoid recursion/circular access
            var query = _dbContext.Albums.AsNoTracking()
                .Include(i => i.MusicGroupDbM)
                .ThenInclude(i => i.ArtistsDbM)
                .Where(i => i.AlbumId == id);

            return await query.FirstOrDefaultAsync<IAlbum>();
        }
        else
        {
            //Not fully populated, compare the SQL Statements generated
            //remove tracking for all read operations for performance and to avoid recursion/circular access
            var query = _dbContext.Albums.AsNoTracking()
                .Where(i => i.AlbumId == id);

            return await query.FirstOrDefaultAsync<IAlbum>();
        }  
    }

    public async Task<RespPageDto<IAlbum>> ReadAlbumsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize)
      {
        filter ??= "";
        IQueryable<AlbumDbM> query;
        if (flat)
        {
            query = _dbContext.Albums.AsNoTracking();
        }
        else
        {
            query = _dbContext.Albums.AsNoTracking()
                .Include(i => i.MusicGroupDbM)
                .ThenInclude(i => i.ArtistsDbM);
        }

        var ret = new RespPageDto<IAlbum>()
        {
            DbItemsCount = await query

            //Adding filter functionality
            .Where(i => (i.Seeded == seeded) && 
                        (i.Name.ToLower().Contains(filter) ||
                            i.ReleaseYear.ToString().Contains(filter))).CountAsync(),

            PageItems = await query

            //Adding filter functionality
            .Where(i => (i.Seeded == seeded) && 
                        (i.Name.ToLower().Contains(filter) ||
                            i.ReleaseYear.ToString().Contains(filter)))

            //Adding paging
            .Skip(pageNumber * pageSize)
            .Take(pageSize)

            .ToListAsync<IAlbum>(),

            PageNr = pageNumber,
            PageSize = pageSize
        };
        return ret;
    }

    public async Task<IAlbum> DeleteAlbumAsync(Guid id)
    {
        var query1 = _dbContext.Albums
            .Where(i => i.AlbumId == id);

        var item = await query1.FirstOrDefaultAsync<AlbumDbM>();

        //If the item does not exists
        if (item == null) throw new ArgumentException($"Item {id} is not existing");

        //delete in the database model
        _dbContext.Albums.Remove(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();
        return item;    
    }

    public async Task<IAlbum> UpdateAlbumAsync(AlbumCUdto itemDto)
    {
        var query1 = _dbContext.Albums
            .Where(i => i.AlbumId == itemDto.AlbumId);
        var item = await query1
                .Include(i => i.MusicGroupDbM)
                .FirstOrDefaultAsync<AlbumDbM>();

        //If the item does not exists
        if (item == null) throw new ArgumentException($"Item {itemDto.AlbumId} is not existing");

        //transfer any changes from DTO to database objects
        //Update individual properties 
        item.UpdateFromDTO(itemDto);

        //Update navigation properties
        await navProp_csAlbumCUdto_to_csAlbumDbM(_dbContext, itemDto, item);

        //write to database model
        _dbContext.Albums.Update(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();
        
        //return the updated item in non-flat mode
        return await ReadAlbumAsync(item.AlbumId, false);    
    }

    public async Task<IAlbum> CreateAlbumAsync(AlbumCUdto itemDto)
    { 
        if (itemDto.AlbumId != null)
            throw new ArgumentException($"{nameof(itemDto.AlbumId)} must be null when creating a new object");

        //transfer any changes from DTO to database objects
        //Update individual properties. Seeded always false on created items
        itemDto.Seeded = false; 
        var item = new AlbumDbM(itemDto);

        //Update navigation properties
        await navProp_csAlbumCUdto_to_csAlbumDbM(_dbContext, itemDto, item);

        //write to database model
        _dbContext.Albums.Add(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();
        
        //return the updated item in non-flat mode
        return await ReadAlbumAsync(item.AlbumId, false);    
    }

    //from all id's in _itemDtoSrc finds the corresponding object in the database and assigns it to _itemDst
    //Error is thrown if no object is found correspodning to an id.
    private static async Task navProp_csAlbumCUdto_to_csAlbumDbM(MainDbContext db, AlbumCUdto itemDtoSrc, AlbumDbM itemDst)
    {
        //Navigation prop Albums
        var musicGroup = await db.MusicGroups.FirstOrDefaultAsync(a => a.MusicGroupId == itemDtoSrc.MusicGroupId);
        if (musicGroup == null)
            throw new ArgumentException($"Item id {itemDtoSrc.MusicGroupId} not existing");
        
        itemDst.MusicGroupDbM = musicGroup;
    }
    #endregion

    #region Artist repo methods
    public async Task<IArtist> ReadArtistAsync(Guid id, bool flat)
    {
        if (!flat)
        {
            //make sure the model is fully populated, try without include.
            //remove tracking for all read operations for performance and to avoid recursion/circular access
            var query = _dbContext.Artists.AsNoTracking()
                .Include(a => a.MusicGroupsDbM)
                .ThenInclude(a => a.AlbumsDbM)
                .Where(i => i.ArtistId == id);

            return await query.FirstOrDefaultAsync<IArtist>();
        }
        else
        {
            //Not fully populated, compare the SQL Statements generated
            //remove tracking for all read operations for performance and to avoid recursion/circular access
            var query = _dbContext.Artists.AsNoTracking()
                .Where(i => i.ArtistId == id);

            return await query.FirstOrDefaultAsync<IArtist>();
        }
    }

    public async Task<RespPageDto<IArtist>> ReadArtistsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize)
    {
        filter ??= "";
        IQueryable<ArtistDbM> query;
        if (flat)
        {
            query = _dbContext.Artists.AsNoTracking();
        }
        else
        {
            query = _dbContext.Artists.AsNoTracking()
                .Include(a => a.MusicGroupsDbM)
                .ThenInclude(a => a.AlbumsDbM);
        }

        var ret = new RespPageDto<IArtist>()
        {
            DbItemsCount = await query

            //Adding filter functionality
            .Where(i => (i.Seeded == seeded) && 
                        (i.FirstName.ToLower().Contains(filter) ||
                            i.LastName.ToLower().Contains(filter))).CountAsync(),

            PageItems = await query

            //Adding filter functionality
            .Where(i => (i.Seeded == seeded) && 
                        (i.FirstName.ToLower().Contains(filter) ||
                            i.LastName.ToLower().Contains(filter)))

            //Adding paging
            .Skip(pageNumber * pageSize)
            .Take(pageSize)

            .ToListAsync<IArtist>(),

            PageNr = pageNumber,
            PageSize = pageSize
        };
        return ret; 
    }

    public async Task<IArtist> DeleteArtistAsync(Guid id)
    {
        var query1 = _dbContext.Artists
            .Where(i => i.ArtistId == id);

        var item = await query1.FirstOrDefaultAsync<ArtistDbM>();

        //If the item does not exists
        if (item == null) throw new ArgumentException($"Item {id} is not existing");

        //delete in the database model
        _dbContext.Artists.Remove(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();
        return item;  
    }

    public async Task<IArtist> UpdateArtistAsync(ArtistCUdto itemDto)
    {
        var query1 = _dbContext.Artists
            .Where(i => i.ArtistId == itemDto.ArtistId);
        var item = await query1
                .Include(i => i.MusicGroupsDbM)
                .FirstOrDefaultAsync();

        //If the item does not exists
        if (item == null) throw new ArgumentException($"Item {itemDto.ArtistId} is not existing");

        //transfer any changes from DTO to database objects
        //Update individual properties 
        item.UpdateFromDTO(itemDto);

        //Update navigation properties
        await navProp_csArtistCUdto_to_csArtistDbM(_dbContext, itemDto, item);

        //write to database model
        _dbContext.Artists.Update(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();

        //return the updated item in non-flat mode
        return await ReadArtistAsync(item.ArtistId, false);    
    }

    public async Task<IArtist> CreateArtistAsync(ArtistCUdto itemDto)
    {
        if (itemDto.ArtistId != null)
            throw new ArgumentException($"{nameof(itemDto.ArtistId)} must be null when creating a new object");

        //transfer any changes from DTO to database objects
        //Update individual properties. Seeded always false on created items
        itemDto.Seeded = false; 
        var item = new ArtistDbM(itemDto);

        //Update navigation properties
        await navProp_csArtistCUdto_to_csArtistDbM(_dbContext, itemDto, item);


        //write to database model
        _dbContext.Artists.Add(item);

        //write to database in a UoW
        await _dbContext.SaveChangesAsync();

        //return the updated item in non-flat mode
        return await ReadArtistAsync(item.ArtistId, false);    
    }


    //Upsert = Update or Insert
    public async Task<IArtist> UpsertArtistAsync(ArtistCUdto itemDto)
    {
        //Check if a Artist Exists
        var query1 = _dbContext.Artists.Where(a =>  (a.Seeded == itemDto.Seeded) && 
                                                (a.FirstName == itemDto.FirstName) && (a.LastName == itemDto.LastName) &&
                                                (a.BirthDay == itemDto.BirthDay));
        var item = await query1.Include(a => a.MusicGroupsDbM).FirstOrDefaultAsync();

        if (item != null)
        {
            //Instead of Create the Artist should be updated
            item.UpdateFromDTO(itemDto);

            await navProp_csArtistCUdto_to_csArtistDbM(_dbContext, itemDto, item);
            _dbContext.Artists.Update(item);
        }
        else
        {

            //Create and insert a new Artist
            itemDto.Seeded = false; 
            item = new ArtistDbM(itemDto);
            
            await navProp_csArtistCUdto_to_csArtistDbM(_dbContext, itemDto, item);
            _dbContext.Artists.Add(item);
        }

        await _dbContext.SaveChangesAsync();
        return (item);
    }

    //from all id's in _itemDtoSrc finds the corresponding object in the database and assigns it to _itemDst
    //Error is thrown if no object is found correspodning to an id.
    private static async Task navProp_csArtistCUdto_to_csArtistDbM(MainDbContext db, ArtistCUdto itemDtoSrc, ArtistDbM itemDst)
    {
        //Navigation prop MusicGroups
        List<MusicGroupDbM> mgs = new List<MusicGroupDbM>();
        foreach (var id in itemDtoSrc.MusicGroupsId)
        {
            var musicGroup = await db.MusicGroups.FirstOrDefaultAsync(a => a.MusicGroupId == id);

            if (musicGroup == null)
                throw new ArgumentException($"Item id {id} not existing");

            mgs.Add(musicGroup);
        }

        itemDst.MusicGroupsDbM = mgs;
    }
    #endregion
}
