using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Newtonsoft.Json;
using PPCT.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace PPCT.Services
{
    public class DataverseConnectionService : IDataverseConnectionService
    {
        private readonly ILogger<DataverseConnectionService> _log;
        private ServiceClient _client = null;
        private readonly string ConnectionsFilePath;
        private readonly string TokenPath;

        //private string ConnectionString { get; set; }

        private List<StoredConnection> _listedConnections = [];

        public ServiceClient Client
        {
            get
            {
                if (_client == null)
                {
                    Connect();
                    return _client;
                }

                return _client;
            }
        }
        public DataverseConnectionService(ILogger<DataverseConnectionService> log)
        {
            _log = log;

            ConnectionsFilePath = Path.Combine(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ppct"),
                "Connections.json");
            TokenPath = Path.Combine(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ppct"),
                "TokenCache");
            LoadExistingConnections();
        }

        private void Connect()
        {
            var connectionStringRoot = $@"AuthType=OAuth;
                    AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;
                    RedirectUri=http://localhost;
                    TokenCacheStorePath={TokenPath}";

            var connectionIndex = -1;

            if (_listedConnections.Count != 0)
            {
                var builder = new StringBuilder();
                builder.AppendLine("(0) Add New Dataverse Connection");
                var i = 1;
                foreach (var connection in _listedConnections)
                {
                    builder.AppendLine($"({i}) {connection.EnvironmentUrl},  {connection.DisplayName},  {connection.UserName}");
                    i++;
                }
                builder.AppendLine($"\nSpecify the saved connection number (0-{_listedConnections.Count}) [{_listedConnections.Count}] : ");

                Console.WriteLine(builder.ToString());
                var selection = Console.ReadLine();

                if (selection == string.Empty)
                {
                    selection = _listedConnections.Count.ToString();
                }

                if (!int.TryParse(selection, out connectionIndex))
                {
                    connectionIndex = -1;
                }
            }

            StoredConnection selectedConnection;
            var loginPrompt = "Auto";
            var newConnection = connectionIndex <= 0;
            if (newConnection)
            {
                selectedConnection = new StoredConnection()
                {
                    EnvironmentUrl = ReadLine("Environment/Organization Url (e.g. org123.crm.dynamics.com)", regex: @"^(?<!http)([^\s:\/]+)(\.crm[0-9]*\.dynamics\.com[\/]?)$").TrimEnd('/')
                };
                // If new connection - set LoginPrompt=Always
                loginPrompt = "Always";
            }
            else
            {
                selectedConnection = _listedConnections[connectionIndex - 1];
                // Move the saved connection to the end so it's default next time
                _listedConnections.Remove(selectedConnection);
                _listedConnections.Add(selectedConnection);
                SaveConnections();
                connectionStringRoot += $";UserName={selectedConnection.UserName}";
            }
            var connectionString = $@"{connectionStringRoot};Url=https://{selectedConnection.EnvironmentUrl};LoginPrompt={loginPrompt}";

            _log.LogInformation("Connecting to Dataverse Environment {env}...", selectedConnection.EnvironmentUrl);

            try
            {
                _client = new ServiceClient(connectionString);
            }
            catch (Exception)
            {
                _log.LogError("Failed to connect to Dataverse.");
                _log.LogError("Cannot connect: {lastError}\n{lastException}", _client.LastError, _client.LastException);
                throw new Exception("Failed to connect to Dataverse.");
            }

            if (_client.IsReady)
            {
                _log.LogInformation("Connected to {envFriendly} successfully", _client.ConnectedOrgFriendlyName);
                if (newConnection)
                {
                    selectedConnection.EnvironmentUrl = _client.ConnectedOrgUriActual.Host;
                    selectedConnection.UserName = _client.OAuthUserId;
                    selectedConnection.DisplayName = _client.ConnectedOrgFriendlyName;
                    connectionString += $";UserName={selectedConnection.UserName}";
                    _listedConnections.RemoveAll(c => c.EnvironmentUrl.Equals(selectedConnection.EnvironmentUrl, StringComparison.InvariantCultureIgnoreCase) &&
                        c.UserName.Equals(selectedConnection.UserName, StringComparison.InvariantCultureIgnoreCase));
                    _listedConnections.Add(selectedConnection);
                    SaveConnections();
                }
            }
            else
            {
                throw new Exception($"Cannot connect: {_client.LastError}", _client.LastException);
            }
        }

        private void LoadExistingConnections()
        {
            if (File.Exists(ConnectionsFilePath))
            {
                string configJson = File.ReadAllText(ConnectionsFilePath);

                if (string.IsNullOrEmpty(configJson))
                {
                    _listedConnections = [];
                    return;
                }

                List<StoredConnection> parsedConnections = [];

                try
                {
                    parsedConnections = JsonConvert.DeserializeObject<List<StoredConnection>>(configJson);
                }
                catch (Exception)
                {
                    _log.LogWarning("Failed to parse connections file. Resetting connections....");
                    parsedConnections = [];
                }

                _listedConnections = parsedConnections;
            }
        }

        private void SaveConnections()
        {
            string configJson = JsonConvert.SerializeObject(_listedConnections);
            File.WriteAllText(ConnectionsFilePath, configJson);
        }

        private static string ReadLine(string prompt, string regex = null, string defaultValue = null)
        {
            if (defaultValue != null)
            {
                prompt += $"[{defaultValue}]";
            }

            bool isValid = true;
            string returnedValue = defaultValue;
            do
            {
                Console.Write(prompt + ": ");
                string input = Console.ReadLine();
                if (input?.Length > 0)
                {
                    if (regex != null && Regex.IsMatch(input, regex))
                    {
                        returnedValue = input;
                        isValid = true;
                    }
                    else
                    {
                        Console.WriteLine("\nInput invalid");
                        isValid = false;
                    }
                }
                else if (defaultValue == null)
                {
                    Console.WriteLine("\nInput Required");
                    isValid = false;
                }
            } while (!isValid);
            Console.Write("\n");
            return returnedValue;
        }
    }
}
