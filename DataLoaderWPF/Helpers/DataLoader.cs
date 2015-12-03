using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace DataLoaderWPF.Helpers
{
    public class DataLoader
    {
        private readonly string _connectionString;
        private readonly char _delimieter;
        private readonly char[] _scrubChars;
        private readonly string[] _files;

        public DataLoader(string connectionString, string[] files, char delimiter, char[] scrubChars)
        {
            _connectionString = connectionString;
            _files = files;
            _delimieter = delimiter;
            _scrubChars = scrubChars;
        }

        public event Action<int> FileProgressHappened;
        public event Action<int> TableProgressHappened;
        public event Action<int> InsertProgressHappened;

        private void ProcessFile(string filetext, string file)
        {
            var tableName = Path.GetFileNameWithoutExtension(file);
            var lines = filetext.Split('\n');
            string[] columnHeaders = null;
            var lineNumber = 0;
            lineNumber++;

            var dt = new DataTable(tableName);

            foreach (var line in lines)
            {
                if (lineNumber == 1)
                {
                    //this is the first line, contains column headers
                    columnHeaders = line.Split(_delimieter);
                    columnHeaders = columnHeaders.Select(a => a.Trim().Replace("\"", string.Empty)).ToArray();
                }
                else
                {
                    //this is a data line
                    var dataTokens = line.Split(_delimieter);
                    dataTokens = dataTokens.Select(a => a.Trim()).ToArray();

                    if (dt.Columns.Count == 0)
                    {
                        //build columns from the first data line
                        BuildColumns(ref dt, dataTokens, columnHeaders);
                    }
                    AddNewRow(ref dt, dataTokens);
                }
                lineNumber++;
                if (TableProgressHappened != null)
                {
                    TableProgressHappened((int) (lineNumber/(double) lines.Count()*100.0));
                }
            }
            dt.AcceptChanges();
            var ddlText = GenerateDDLfromTable(dt);
            var inserts = GenerateInsertsFromTable(dt, columnHeaders);

            var sqlConn = new SqlConnection();
            var sqlComm = new SqlCommand();

            sqlConn.ConnectionString = _connectionString;
            sqlComm.CommandText = ddlText;
            sqlComm.CommandType = CommandType.Text;
            sqlComm.Connection = sqlConn;
            sqlConn.Open();
            sqlComm.ExecuteNonQuery();

            for (var i = 0; i < inserts.Length; i++)
            {
                sqlComm.CommandText = inserts[i];
                try
                {
                    sqlComm.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                }
                if (InsertProgressHappened != null)
                {
                    InsertProgressHappened((int) (i/(double) inserts.Length*100.0));
                }
            }
            sqlConn.Close();
        }

        private string[] GenerateInsertsFromTable(DataTable dt, string[] columnHeaders)
        {
            var inserts = new List<string>(dt.Rows.Count);
            var cols = string.Join(", ", columnHeaders);
            foreach (DataRow dr in dt.Rows)
            {
                var values = string.Empty;
                for (var i = 0; i < dr.ItemArray.Length; i++)
                {
                    if (dr.ItemArray[i] == DBNull.Value)
                    {
                        values += "NULL";
                    }
                    else
                    {
                        if (dr.ItemArray[i].GetType().Name == typeof (DateTime).Name)
                        {
                            values += string.Format("'{0}'", ((DateTime) dr.ItemArray[i]).ToString("MM/dd/yyyy"));
                        }
                        else if (dr.ItemArray[i].GetType().Name == typeof (string).Name)
                        {
                            values += string.Format("'{0}'",
                                dr.ItemArray[i].ToString().Replace("\"", string.Empty).Replace("'", "''"));
                        }
                        else
                            values += dr.ItemArray[i].ToString();
                    }
                    if (i + 1 < dr.ItemArray.Length)
                        values += ", ";
                }
                inserts.Add(string.Format("Insert into {0} ({1}) values ({2})", dt.TableName, cols, values));
            }
            return inserts.ToArray();
        }

        private void AddNewRow(ref DataTable dt, string[] dataTokens)
        {
            var dr = dt.NewRow();
            for (var i = 0; i < dataTokens.Length; i++)
            {
                //scrub the data of provided scrub chars
                dataTokens[i] = dataTokens[i].Trim(_scrubChars);
                if (dt.Columns[i].DataType.Name == typeof (int).Name)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(dataTokens[i]))
                        {
                            dr[i] = int.Parse(dataTokens[i]);
                        }
                        continue;
                    }
                    catch (FormatException)
                    {
                        if (dataTokens[i].StartsWith("."))
                        {
                            var dc = new DataColumn(dt.Columns[i].ColumnName, typeof (double));
                            dt = CopyTableReplaceColumn(dt, dc);

                            var newrow = dt.NewRow();
                            newrow.ItemArray = dr.ItemArray.Select(a => a).ToArray();
                            dr = newrow;
                        }
                    }
                }
                if (dt.Columns[i].DataType.Name == typeof (double).Name)
                {
                    if (!string.IsNullOrEmpty(dataTokens[i]))
                    {
                        dr[i] = double.Parse(dataTokens[i]);
                    }
                    continue;
                }
                if (dt.Columns[i].DataType.Name == typeof (DateTime).Name)
                {
                    if (!string.IsNullOrEmpty(dataTokens[i]))
                    {
                        try
                        {
                            dr[i] = DateTime.Parse(dataTokens[i]);
                        }
                        catch
                        {
                            var subtokens = dataTokens[i].Split('/');
                            dr[i] = new DateTime(int.Parse(subtokens[2]), int.Parse(subtokens[1]),
                                int.Parse(subtokens[0]));
                        }
                    }
                    continue;
                }
                if (dt.Columns[i].DataType.Name == typeof (string).Name)
                {
                    if (!string.IsNullOrEmpty(dataTokens[i]))
                    {
                        dr[i] = dataTokens[i].Trim().Trim(new char[] {'~'});
                    }
                }
            }
            try
            {
                dt.Rows.Add(dr);
            }
            catch (ConstraintException ex)
            {
                var cols = dt.PrimaryKey;
                dt.PrimaryKey = null;
                cols[0].AllowDBNull = true;
                dt.Rows.Add(dr);
            }
        }

        private DataTable CopyTableReplaceColumn(DataTable dt, DataColumn dc)
        {
            var copy = new DataTable(dt.TableName);
            foreach (DataColumn col in dt.Columns)
            {
                copy.Columns.Add(col.ColumnName == dc.ColumnName ? dc : new DataColumn(col.ColumnName, col.DataType));
            }

            foreach (DataRow dr in dt.Rows)
            {
                var copyRow = copy.NewRow();
                for (var i = 0; i < dt.Columns.Count; i++)
                {
                    if (dt.Columns[i].ColumnName == dc.ColumnName)
                    {
                        if (dr[i] != DBNull.Value)
                            copyRow[i] = Convert.ToDouble(dr[i]);
                        else
                            copyRow[i] = dr[i];
                    }
                    else
                        copyRow[i] = dr[i];
                }
                copy.Rows.Add(copyRow);
            }
            copy.AcceptChanges();

            return copy;
        }

        private string GenerateDDLfromTable(DataTable dt)
        {
            var ddl = new StringBuilder();

            ddl.AppendFormat("CREATE TABLE {0}\n", dt.TableName);
            ddl.AppendLine("(");
            foreach (DataColumn col in dt.Columns)
            {
                var typeName = "nvarchar(max)";
                if (col.DataType.Name == typeof (int).Name)
                {
                    typeName = "int";
                }
                if (col.DataType.Name == typeof (DateTime).Name)
                {
                    typeName = "datetime";
                }
                if (col.DataType.Name == typeof (double).Name)
                {
                    typeName = "float";
                }
                //if (col.ColumnName.EndsWith("ID") && primaryKeyDefined && dt.PrimaryKey.Contains(col))
                //{
                //    ddl.AppendFormat("{0} {1} PRIMARY KEY,\n", col.ColumnName, typeName);                    
                //}
                //else
                ddl.AppendFormat("{0} {1},\n", col.ColumnName, typeName);
            }
            ddl.AppendLine(")\n");

            return ddl.ToString();
        }

        private void BuildColumns(ref DataTable dt, string[] dataTokens, string[] columnHeaders)
        {
            DateTime testDate;
            int testInt;
            double testDbl;

            var idx = 0;
            foreach (var columnName in columnHeaders)
            {
                Type t = null;

                if (columnName.Trim().ToLower().EndsWith("id"))
                {
                    t = typeof (int);
                }
                else
                {
                    if (dataTokens[idx].StartsWith("."))
                    {
                        t = typeof (double);
                    }
                    if (t == null && DateTime.TryParse(dataTokens[idx], out testDate))
                    {
                        t = typeof (DateTime);
                    }
                    //try int
                    if (t == null && int.TryParse(dataTokens[idx], out testInt))
                    {
                        t = typeof (int);
                    }

                    //default to string
                    if (t == null)
                    {
                        t = typeof (string);
                    }
                }

                var col = new DataColumn(columnName, t);
                dt.Columns.Add(col);
                //if (idx == 0 && columnName.Trim().ToLower().EndsWith("id"))
                //{
                //    dt.PrimaryKey = new DataColumn[] { col };
                //}
                idx++;
            }
        }

        public void LoadData()
        {
            if (_files == null || _files.Length == 0)
                return;

            for (var i = 0; i < _files.Length; i++)
            {
                if (Path.GetExtension(_files[i]) != ".txt")
                    continue;

                var fs = File.OpenRead(_files[i]);
                var sr = new StreamReader(fs);
                var filetext = sr.ReadToEnd();
                sr.Dispose();
                fs.Dispose();
                var doPrimaryKey = false;
                //TODO: chkPrimaryKeys.CheckedIndices.Contains(i);             
                ProcessFile(filetext, _files[i]);
                if (FileProgressHappened != null)
                {
                    FileProgressHappened((int) (i/(double) _files.Length*100.0));
                }
            }
        }
    }
}