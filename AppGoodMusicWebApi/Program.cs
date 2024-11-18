using Configuration;
using DbContext;
using DbRepos;
using Services;

var builder = WebApplication.CreateBuilder(args);

//using jwt find out the user requesting the endpoint
//builder.Services.AddHttpContextAccessor();

#region Insert standard WebApi services
// NOTE: global cors policy needed for JS and React frontends
builder.Services.AddCors();

// Add services to the container.
builder.Services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
#endregion

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
    
#region configure swagger
builder.Services.AddSwaggerGen();
#endregion

#region Dependency Inject Custom logger
builder.Services.AddSingleton<ILoggerProvider, InMemoryLoggerProvider>();
#endregion

#region Dependency Inject
//Services are typically added as Scoped as one scope is a Web client request
//- Transient objects are always different in the IndexModel and in the middleware.
//- Scoped objects are the same for a given request but differ across each new request.
//- Singleton objects are the same for every request.

//DI injects the DbRepos 
builder.Services.AddScoped<MusicDbRepos>();
builder.Services.AddScoped<IMusicService, MusicServiceDb>();
#endregion

#region Dependency Inject LoginService
#endregion

var app = builder.Build();

#region Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// global cors policy - the call to UseCors() must be done here
app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true) // allow any origin
    .AllowCredentials()); // allow credentials

app.UseAuthorization();
app.MapControllers();

app.Run();
#endregion

