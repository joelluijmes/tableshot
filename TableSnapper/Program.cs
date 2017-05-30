﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using TableSnapper.Models;

namespace TableSnapper
{
    internal class Program
    {
        public static ILoggerFactory LoggerFactory { get; } = new LoggerFactory();
        public static ILogger CreateLogger<T>() => LoggerFactory.CreateLogger<T>();

        private static readonly ILogger _logger = CreateLogger<Program>();

        private static void Main(string[] args)
        {
            LoggerFactory.AddConsole(LogLevel.Debug, true);
            
            AsyncContext.Run(MainImpl);
        }

        private static async Task MainImpl()
        {
            _logger.LogInformation("Started");

            var connectionA = await DatabaseConnection.CreateConnectionAsync("localhost", "TestA");
            var databaseA = new DatabaseManager(connectionA);

            var connectionB = await DatabaseConnection.CreateConnectionAsync("localhost", "TestB");
            var databaseB = new DatabaseManager(connectionB);

            var baseDirectory = Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"), "TableSnapper");

            var tables = await databaseA.ListTablesDependentOnAsync("Orders");
            await databaseA.BackupToDirectoryAsync(baseDirectory, tables);
            
            _logger.LogInformation("Completed");
        }
    }
}
