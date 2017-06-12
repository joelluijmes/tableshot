using System;
using System.Threading.Tasks;
using tableshot.Models;

namespace tableshot.Commands {
    internal abstract class TableCommand : DatabaseCommand
    {
        protected async Task<ShallowTable> ParseTable(string tableName)
        {
            ShallowTable shallowTable;
            var splitted = tableName.Split('.');
            switch (splitted.Length)
            {
            case 0:
                var schema = Program.Configuration["schema"] ?? await DatabaseManager.GetDefaultSchema(Connection);
                shallowTable = new ShallowTable(schema, tableName);
                break;
            case 2:
                shallowTable = new ShallowTable(splitted[0], splitted[1]);
                break;
            default:
                throw new InvalidOperationException();
            }

            return shallowTable;
        }
    }
}