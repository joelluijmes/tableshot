﻿using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using tableshot.Models;

namespace tableshot.Commands
{
    internal sealed class CloneCommand : ICommand
    {
        private CommandArgument _configArgument;
        public string Name => "clone";
        public string Description => "clones tables and data between schemas and databases";

        public void Configure(CommandLineApplication application)
        {
            _configArgument = application.Argument("config", "configuration file for cloning");
        }

        public async Task Execute()
        {
            // parse the config json file
            var reader = File.OpenText(_configArgument.Value);
            var jsonReader = new JsonTextReader(reader);
            var json = await JObject.LoadAsync(jsonReader);

            // read the config
            var source = json["source"].ToObject<ServerCredentials>().ToConnectionStringBuilder();
            var target = json["target"].ToObject<ServerCredentials>().ToConnectionStringBuilder();
            var tables = json["tables"].ToObject<CloningTables[]>();

            // do the cloning
            using (var sourceConnection =await DatabaseConnection.CreateConnectionAsync(source))
            using (var targeteConnection = await DatabaseConnection.CreateConnectionAsync(target))
            {
                var cloner = new DatabaseCloner(sourceConnection, targeteConnection);
                var options = new DatabaseCloner.DatabaseCloneOptions(tables.Select(t => t.Table));

                await cloner.CloneAsync(options);
            }
        }

        private class CloningTables
        {
            [JsonProperty("name")]
            [JsonConverter(typeof(TableNameConverter))]
            public ShallowTable Table { get; set; }

            [JsonProperty("referenced")]
            public bool DescendReferenced { get; set; }

            private class TableNameConverter : JsonConverter
            {
                public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
                {
                    throw new NotImplementedException();
                }

                public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
                {
                    return Util.ParseTableName(reader.Value.ToString());
                }

                public override bool CanConvert(Type objectType) => false;

                public override bool CanWrite => false;
            }
        }

        private class ServerCredentials
        {
            [JsonProperty("server")]
            public string Server { get; set; }

            [JsonProperty("database")]
            public string Database { get; set; }
            
            [JsonProperty("username")]
            public string Username { get; set; }

            [JsonProperty("password")]
            public string Password { get; set; }

            public SqlConnectionStringBuilder ToConnectionStringBuilder()
            {
                var builder = new SqlConnectionStringBuilder()
                {
                    InitialCatalog = Database,
                    DataSource = Server
                };

                if (Username == null && Password == null)
                {
                    builder.IntegratedSecurity = true;
                }
                else
                {
                    builder.UserID = Username;
                    builder.Password = Password;
                }

                return builder;
            }
        }
    }
}
