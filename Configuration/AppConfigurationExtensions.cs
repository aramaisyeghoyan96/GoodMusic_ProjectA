using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Xml.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Configuration;

public static class AppConfigurationExtensions
{
    const string _appsettingfile = "appsettings.json";
    static string _projectFile;
    
    //to use either user secrets or azure key vault depending on UseAzureKeyVault tag in appsettings.json
    //Azure key vault access parameters location are set in <AzureProjectSettings> tag in the csproj file
    //User secret id is set in <UserSecretsId> and <UserSecretPath> in the csproj file
    public static IConfigurationBuilder AddApplicationSecrets(this IConfigurationBuilder configuration, string projectFile)
    {
        _projectFile = projectFile;

        var conf = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(_appsettingfile, optional: true, reloadOnChange: true)
            .Build();
        bool useAzureKeyVault = conf.GetValue<bool>("ApplicationSecrets:UseAzureKeyVault");

        configuration.SetBasePath(Directory.GetCurrentDirectory());

        //Ensure that also docker environment has Development/Production detection
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");

#if DEBUG
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        if (useAzureKeyVault)
        {
            //Environment variables are not set in Debug environment, read and set them
            SetEnvironmentVariablesToAccessAzureKeyVault();
        }
#endif

        if (useAzureKeyVault) 
        {
            //Environment variables are set as part of the deployment process
            configuration.AddAzureKeyVault();
        }
        else
        {
           //Load the user secrets file
           string secretId = CsprojElement("UserSecretsId");
           configuration.AddUserSecrets(secretId, reloadOnChange: true);
           Environment.SetEnvironmentVariable("USERSECRETID", secretId);
        }

        //override with any locally set configuration from appsettings.json
        configuration.AddJsonFile(_appsettingfile, optional: true, reloadOnChange: true);

        return configuration;
    }

    public static IServiceCollection AddDatabaseConnections(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<DbConnectionSetsOptions>(
            options => configuration.GetSection(DbConnectionSetsOptions.Position).Bind(options));
        serviceCollection.AddSingleton<DatabaseConnections>();

        return serviceCollection;
    }

    private static IConfigurationBuilder AddAzureKeyVault(this IConfigurationBuilder configuration)
    {
            //A deployed WebApp will start here by reading the environment variables
            var vaultUri = new Uri(Environment.GetEnvironmentVariable("AZURE_KeyVaultUri"));
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var secretname = Environment.GetEnvironmentVariable("AZURE_KeyVaultSecret");

            //Open the AZKV from creadentials in the environment variables
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var client = new SecretClient(vaultUri, credential);

            var secret = client.GetSecret(secretname);
            var userSecretsJson = secret.Value.Value;

            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(userSecretsJson)); 
            configuration.AddJsonStream(stream);

        return configuration;
    }

    //For Debug: Read and set them from Vault and set Environment variables
    //For Production: Will be set as Environment variables as part of the deployment process
    private static void SetEnvironmentVariablesToAccessAzureKeyVault()
    {
        string azureSettingsFile = Path.Combine(CsprojElement("AzureProjectSettings"), "az-access", "az-settings.json");
        string tenantFile = Path.Combine(CsprojElement("AzureProjectSettings"), "az-secrets", "az-app-access.json");

        var _vaultAccess = new ConfigurationBuilder()
                .AddJsonFile(azureSettingsFile, optional: true, reloadOnChange: true)
                .AddJsonFile(tenantFile, optional: true, reloadOnChange: true)
                .Build();


        //A deployed WebApp will use environment variables, so here I set them during DEBUG
        Environment.SetEnvironmentVariable("AZURE_TENANT_ID", _vaultAccess["tenantId"]);
        Environment.SetEnvironmentVariable("AZURE_KeyVaultSecret", _vaultAccess["kvSecret"]);
        Environment.SetEnvironmentVariable("AZURE_KeyVaultUri", _vaultAccess["kvUri"]);
        Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", _vaultAccess["password"]);
        Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", _vaultAccess["appId"]);
    }

    //read the tag from project file. Only during development
    private static string CsprojElement (string elemtTag) { 
    
        string csprojPath = Path.Combine(Directory.GetCurrentDirectory(), _projectFile);
        XDocument csproj = XDocument.Load(csprojPath);
        var elemValue = csproj.Descendants(elemtTag).FirstOrDefault()?.Value;
        return elemValue;        
    }
    
    private static Dictionary<string, string> JsonFlatToDictionary(string json)
    {
        IEnumerable<(string Path, JsonProperty P)> GetLeaves(string path, JsonProperty p)
            => p.Value.ValueKind != JsonValueKind.Object
                ? new[] { (Path: path == null ? p.Name : path + ":" + p.Name, p) }
                : p.Value.EnumerateObject()
                    .SelectMany(child => GetLeaves(path == null ? p.Name : path + ":" + p.Name, child));

        using (JsonDocument document = JsonDocument.Parse(json)) // Optional JsonDocumentOptions options
            return document.RootElement.EnumerateObject()
                .SelectMany(p => GetLeaves(null, p))
                //Clone so that we can use the values outside of using
                .ToDictionary(k => k.Path, v => v.P.Value.Clone().ToString()); 
    }
}