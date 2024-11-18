//Show all files in Solution explorer.
//./obj/Debug/net7.0/AppMvc.GlobalUsings.g.cs show all implicit "using"
using Services;
using DbRepos;
using DbContext;
using Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();

#region adding support for several secret sources and database sources
//to use either user secrets or azure key vault depending on UseAzureKeyVault tag in appsettings.json
builder.Configuration.AddApplicationSecrets("../Configuration/Configuration.csproj");

//use multiple Database connections and their respective DbContexts
builder.Services.AddDatabaseConnections(builder.Configuration);
builder.Services.AddDatabaseConnectionsDbContext();
#endregion

//read in various options from appsettings.json, or ApplicationSecrets (usersecrets or azure)
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.Position));
builder.Services.Configure<PasswordOptions>(
    builder.Configuration.GetSection(PasswordOptions.Position));


#region Injecting a dependency service to read MusicWebApi
builder.Services.AddHttpClient(name: "MusicWebApi", configureClient: options =>
{
    options.BaseAddress = new Uri(builder.Configuration["DataService:WebApiBaseUri"]);
    options.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(
            mediaType: "application/json",
            quality: 1.0));
});

builder.Services.AddSingleton<IMusicServiceActive, MusicServiceActive>();
builder.Services.AddScoped<MusicDbRepos>();
builder.Services.AddScoped<MusicServiceDb>();
builder.Services.AddScoped<MusicServiceWapi>();

builder.Services.AddScoped<IMusicService> (sp => 
    {
        var dataSource = sp.GetService<IMusicServiceActive> ().ActiveDataSource;
        if (dataSource == MusicDataSource.WebApi)
        {
            return sp.GetService<MusicServiceWapi> ();
        }
        return sp.GetService<MusicServiceDb> ();
    });

#endregion

var app = builder.Build();

//using Hsts and https to secure the site
if (!app.Environment.IsDevelopment())
{
    //https://en.wikipedia.org/wiki/HTTP_Strict_Transport_Security
    //https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl
    app.UseHsts();
    app.UseDeveloperExceptionPage();
}
app.UseHttpsRedirection();

//Use static files css, html, and js
app.UseStaticFiles();

//Use endpoint routing
app.UseRouting();

//Use Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

//This is the endpoint, Map Razorpages into Pages folder
app.MapRazorPages();

//Mapped Get response example
app.MapGet("/hello", () =>
{
    //read the environment variable ASPNETCORE_ENVIRONMENT
    //Change in launchSettings.json, (not VS2022 Debug/Release)
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    var envMyOwn = Environment.GetEnvironmentVariable("MyOwn");

    return $"Hello World!\nASPNETCORE_ENVIRONMENT: {env}\nMyOwn: {envMyOwn}";
});

app.Run();

#region App in Kestrel without VS2022 environment:
//Here I add to be shown in console as final after application stopped
//open terminal in AppMvc and type
//dotnet run --launch-profile https

//stopp server in Kesterl by ctrl-C
Console.WriteLine("The AppMvc webserver has stopped");
#endregion



