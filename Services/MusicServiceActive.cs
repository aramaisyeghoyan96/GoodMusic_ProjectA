using Microsoft.Extensions.Configuration;

namespace Services;

//to shift datasource between WebApi and Database
public enum MusicDataSource { SQLDatabase, WebApi }

public interface IMusicServiceActive
{
    public MusicDataSource ActiveDataSource {get; set;}
}

public class MusicServiceActive : IMusicServiceActive
{
    private static readonly object s_instanceLock = new();

    //allow datasource shift att application level, through singleton
    private MusicDataSource _datasource;
    public MusicDataSource ActiveDataSource 
    {
        get 
        {
            lock (s_instanceLock)
            { 
                return _datasource;
            }
        }
        set 
        {
            lock (s_instanceLock)
            { 
                _datasource = value;
            }
        }
    }

    public MusicServiceActive(IConfiguration configuration)
    {
        _datasource = configuration["DataService:DataSource"] switch {
            "WebApi" => MusicDataSource.WebApi,
            _ => MusicDataSource.SQLDatabase
        };
    }
}

