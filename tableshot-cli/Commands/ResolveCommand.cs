﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace tableshot.Commands
{
    internal sealed class ResolveCommand : TableCommand
    {
        private static readonly ILogger _logger = ApplicationLogging.CreateLogger<ResolveCommand>();
        private CommandArgument _tableArgument;

        public override string Name => "resolve";
        public override string Description => "Resolves all dependent tables";

        public override void Configure(CommandLineApplication application)
        {
            _tableArgument = application.Argument("table", "table to resolve");
        }

        protected override async Task Execute(DatabaseManager databaseManager)
        {
            var shallowTable = await ParseTable(_tableArgument.Value);
            var referencedTables = await databaseManager.QueryTablesReferencedByAsync(shallowTable);

            _logger.LogInformation("All tables depending on (in order):");
            _logger.LogInformation(referencedTables.Aggregate($"{shallowTable}", (a, b) => $"{a}\r\n {b}"));
        }
    }
}