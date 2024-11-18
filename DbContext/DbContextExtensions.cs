using System.Data;
using Microsoft.EntityFrameworkCore;

using Configuration;
using Models;
using Models.DTO;
using DbModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace DbContext;
public static class DbContextExtensions
{
    public static IServiceCollection AddDatabaseConnectionsDbContext(this IServiceCollection serviceCollection, string user=null)
    {
        serviceCollection.AddDbContext<MainDbContext>((serviceProvider, options) => 
        { 
            var configuration = serviceProvider.GetRequiredService<IConfiguration>(); 
            var databaseConnections = serviceProvider.GetRequiredService<DatabaseConnections>(); 
            
            user ??= configuration["DatabaseConnections:DefaultDataUser"];
            /*
            //using jwt find out the user requesting the endpoint
            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>(); 
            var httpContext = httpContextAccessor.HttpContext; 
            if (httpContext != null) 
            { 
                var token = httpContext.GetTokenAsync("access_token").Result;
                user = jwtService.DecodeToken(_token);
            } 
            */

            var conn = databaseConnections.GetDataConnectionDetails(user);
            if (databaseConnections.SetupInfo.DataConnectionServer == DatabaseServer.SQLServer)
            {
                options.UseSqlServer(conn.DbConnectionString, options => options.EnableRetryOnFailure());
            }
            else if (databaseConnections.SetupInfo.DataConnectionServer == DatabaseServer.MySql)
            {
                options.UseMySql(conn.DbConnectionString, ServerVersion.AutoDetect(conn.DbConnectionString));
            }
            else if (databaseConnections.SetupInfo.DataConnectionServer == DatabaseServer.PostgreSql)
            {
                options.UseNpgsql(conn.DbConnectionString);
            }
            else if (databaseConnections.SetupInfo.DataConnectionServer == DatabaseServer.SQLite)
            {
                options.UseSqlite(conn.DbConnectionString);
            }
            else
            {
                //unknown database type
                throw new InvalidDataException($"DbContext for {databaseConnections.SetupInfo.DataConnectionServer} not existing");
            }
        });
        
        return serviceCollection;
    }
}