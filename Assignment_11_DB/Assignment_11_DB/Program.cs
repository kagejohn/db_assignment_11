﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Assignment_11_DB
{
    class Program
    {
        private static readonly string JsonTest = @"
{""schemaName"": ""MicroShop"",
  ""entities"": [
    {""Customer"": {
        ""name"": ""String"",
        ""orders"" :""*Order""}},
    {""Order"" :{
        ""date"": ""String"",
        ""total"": ""Number"",
        ""customer"": ""Customer"",
        ""lines"": ""*OrderLine"" }},
    {""OrderLine"" : {
        ""order"": ""Order"",
        ""product"": ""Product"",
        ""count"": ""Number"",
        ""total"": ""Number"" }},
    {""Product"" : {
        ""name"": ""String"",
        ""price"" :""Number""}}
  ]
}";
        private static readonly DatabaseObject DatabaseObject = new DatabaseObject();

        static void Main(string[] args)
        {
            JsonObject jsonObject = JsonConvert.DeserializeObject<JsonObject>(JsonTest);
            
            DatabaseObject.Name = jsonObject.schemaName;

            ReadJson(jsonObject);
            Console.WriteLine("Done reading JSON.");

            CreateMySql();
            Console.WriteLine("Done creating MySQL.");

            CreateClass();
            Console.WriteLine("Done creating the class file.");
        }

        static void ReadJson(JsonObject jsonObject)
        {
            foreach (dynamic outerLayerTable in jsonObject.entities)
            {//useless outher layer
                foreach (dynamic innerLayerTable in outerLayerTable)
                {
                    Table table = new Table();
                    table.Name = innerLayerTable.Name;
                    DatabaseObject.TableNames.Add(table.Name);
                    foreach (dynamic columns in innerLayerTable)
                    {//useless layer
                        foreach (dynamic column in columns)
                        {
                            Column column2 = new Column();
                            column2.Name = column.Name;
                            column2.Type = column.Value;
                            table.Columns.Add(column2);
                        }
                    }
                    DatabaseObject.Tables.Add(table);
                }
            }
        }

        static void CreateMySql()
        {
            List<TableSql> tableSqlList = new List<TableSql>();

            string sql = "drop database if exists " + DatabaseObject.Name + ";\n";
            sql += "create database `" + DatabaseObject.Name + "` /*!40100 default character set latin1 */;\n";
            sql += "\n\nuse `" + DatabaseObject.Name + "`;";
            foreach (Table table in DatabaseObject.Tables)
            {
                List<string> foreignKeyList = new List<string>();

                string tableSql = "\n\ndrop table if exists `" + table.Name + "`;\n\n";

                tableSql += "create table `" + table.Name + "` (\n";
                tableSql += "id int not null AUTO_INCREMENT primary key,\n";

                foreach (Column column in table.Columns)
                {
                    string columnTypeToLower = column.Type.ToLower();

                    if (column.Type.StartsWith("*"))
                    {
                        //then do nothing
                    }
                    else if (columnTypeToLower == "string")
                    {
                        tableSql += column.Name + " text not null,\n";
                    }
                    else if (columnTypeToLower == "number")
                    {
                        tableSql += column.Name + " decimal(18,4) not null,\n";
                    }
                    else if (DatabaseObject.TableNames.Contains(column.Type))
                    {
                        foreignKeyList.Add(column.Type);
                        tableSql += column.Name + "Id int not null,\n";
                    }
                }

                foreach (string foreignKey in foreignKeyList)
                {
                    tableSql += "key `" + foreignKey + "` (`" + foreignKey + "Id`),\n";
                    tableSql += "constraint " + table.Name + "ForeignKey_" + foreignKeyList.IndexOf(foreignKey) +
                           " foreign key (" + foreignKey + "Id) references `" + foreignKey + "` (id),\n";
                }

                if (tableSql.EndsWith(",\n"))
                {
                    tableSql = tableSql.Substring(0, tableSql.Length - 2) + "\n";
                }

                tableSql += ");";
                tableSqlList.Add(new TableSql{Name = table.Name, Sql = tableSql, ForeignKeys = new HashSet<string>(foreignKeyList)});
            }

            while (tableSqlList.Count != 0)
            {
                //if they don't refer to any other table then just add them
                tableSqlList.Sort((t, t2) => t.ForeignKeys.Count.CompareTo(t2.ForeignKeys.Count));
                List<TableSql> temp = tableSqlList.TakeWhile(t => t.ForeignKeys.Count == 0).ToList();
                foreach (TableSql tableSql in temp)
                {
                    sql += tableSql.Sql;
                    tableSqlList.Remove(tableSql);
                }

                foreach (TableSql tableSql in tableSqlList)
                {
                    if (!tableSqlList.Any(t => t.ForeignKeys.Contains(tableSql.Name)))
                    {
                        tableSql.Order += 1;
                    }
                }

                tableSqlList.Sort((t, t2) => t.Order.CompareTo(t2.Order));

                List<TableSql> temp2 = new List<TableSql>(tableSqlList);
                foreach (TableSql tableSql in temp2)
                {
                    sql += tableSql.Sql;
                    tableSqlList.Remove(tableSql);
                }
            }

            CreateFile(DatabaseObject.Name + ".sql", sql);
        }

        static void CreateFile(string fileNameAndExtension, string textInput)
        {
            try
            {
                string filePath = "..\\..\\..\\..\\..\\" + fileNameAndExtension;
                // Check if file already exists. If yes, delete it.     
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Create a new file     
                using (FileStream fs = File.Create(filePath))
                {
                    // Add some text to file    
                    Byte[] text = new UTF8Encoding(true).GetBytes(textInput);
                    fs.Write(text, 0, text.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static void CreateClass()
        {
            string classText = @"using System;
using System.Collections.Generic;
using System.Text;

namespace ";
            classText += DatabaseObject.Name + "\n{\n\tclass " + DatabaseObject.Name + "\n\t{\n\t\tpublic string schemaName { get; set; }\n\t\t";
            classText += "public Entity[] entities { get; set; }\n\t}\n\n";
            classText += "\tpublic class Entity\n\t{\n";

            foreach (string tableName in DatabaseObject.TableNames)
            {
                classText += "\t\tpublic " + tableName + " " + tableName + " { get; set; }\n";
            }

            classText += "\t}\n\n";

            foreach (Table table in DatabaseObject.Tables)
            {
                classText += "\tpublic class " + table.Name + "\n\t{\n";

                foreach (Column column in table.Columns)
                {
                    string columnTypeToLower = column.Type.ToLower();
                    classText += "\t\t";

                    if (DatabaseObject.TableNames.Contains(column.Type))
                    {
                        classText += "public " + column.Type + " " + column.Name + " { get; set; }\n";
                    }
                    else if (columnTypeToLower == "string")
                    {
                        classText += "public string " + column.Name + " { get; set; }\n";
                    }
                    else if (columnTypeToLower == "number")
                    {
                        classText += "public decimal " + column.Name + " { get; set; }\n";
                    }
                    else if (column.Type.StartsWith("*"))
                    {
                        classText += "public List<" + column.Type.Substring(1) + "> " + column.Name + " { get; set; }\n";
                    }
                }

                classText += "\t}\n\n";
            }

            classText = classText.Substring(0, classText.Length - 1) + "}";

            CreateFile(DatabaseObject.Name + ".cs", classText);
        }
    }
}
