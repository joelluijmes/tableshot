﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using tableshot.Models;

namespace tableshot.Commands
{
    internal sealed class ResolveCommand : DatabaseCommand
    {
        private static readonly ILogger _logger = ApplicationLogging.CreateLogger<ResolveCommand>();

        public override string Name => "resolve";
        public override string Description => "Resolves (all) referenced tables";

        public override void Configure(CommandLineApplication application)
        {
        }

        protected override async Task Execute(DatabaseManager databaseManager)
        {
            var tables = Program.Configuration.TableConfigurations;

            Console.WriteLine("All referenced tables on (in order of dependency):");
            foreach (var table in tables)
            {
                var referencedTables = await databaseManager.ListTablesReferencedByAsync(table.Table, table.ReferencedBy, Scope);

                Console.WriteLine(referencedTables.Aggregate($" {table}", (a, b) => $"{a}\r\n  {b}"));
                Console.WriteLine();
            }
        }
    }
}
