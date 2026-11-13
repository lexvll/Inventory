using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory
{
    public static class Database
    {
        public static readonly String DBpATH =
         Path.Combine(
             @"C:\Users\Admin\Documents",
             "InventoryDB.accdb"
         );

        private static readonly string connectionString =
            @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + DBpATH + ";Persist Security Info=False;";

        public static OleDbConnection GetConnection()
        {
            return new OleDbConnection(connectionString);
        }
    }
}
