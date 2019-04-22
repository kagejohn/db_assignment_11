using System;
using System.Collections.Generic;
using System.Text;

namespace Assignment_11_DB
{
    class TableSql
    {
        public string Name { get; set; }
        public string Sql { get; set; }
        public HashSet<string> ForeignKeys { get; set; }
        public int Order { get; set; }
    }
}
