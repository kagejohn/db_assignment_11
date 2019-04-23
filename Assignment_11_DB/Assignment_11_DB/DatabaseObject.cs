using System;
using System.Collections.Generic;
using System.Text;

namespace Assignment_11_DB
{
    class DatabaseObject
    {
        public string Name { get; set; }
        public List<Table> Tables { get; } = new List<Table>();
        public HashSet<string> TableNames { get; } = new HashSet<string>();
    }

    class Table
    {
        public string Name { get; set; }
        public List<Column> Columns { get; } = new List<Column>();
    }

    class Column
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
