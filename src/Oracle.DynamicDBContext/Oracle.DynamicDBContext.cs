using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace AlphaNet.Oracle;

public class DynamicDBContext : IDisposable
{
    public DbConnection _conn { get; set; }
    public bool isConnected { get; private set; }


    public DynamicDBContext(DbConnection connection)
    {
        _conn = connection;
        //_connType = connection.GetType();
        isConnected = false;
    }

    /// <summary>
    /// Try to open the connection. If it is already open, nothing will be done.
    /// </summary>
    public void OpenConnection()
    {
        if (isConnected)
        {
            return;
        }
        _conn.Open();
        isConnected = true;
    }

    /// <summary>
    /// Force to close connection to db
    /// </summary>
    public void CloseConnection()
    {
        if (!isConnected)
        {
            return;
        }
        _conn.Close();
        isConnected = false;
    }


    private T GetItem<T>(DataRow dr)
    {
        Type temp = typeof(T);
        T obj = Activator.CreateInstance<T>();
        foreach (DataColumn column in dr.Table.Columns)
        {
            foreach (PropertyInfo pro in temp.GetProperties())
            {
                if (pro.Name.ToLower() == column.ColumnName.ToLower())
                {

                    var valor = dr[column.ColumnName].ToString();
                    var tipoBanco = dr[column.ColumnName].GetType().Name;
                    var tipoClasse = pro.GetType().Name;
                    try
                    {
                        switch (tipoClasse)
                        {
                            case "Int16": pro.SetValue(obj, Int16.Parse(valor), null); break;

                            case "Int32": pro.SetValue(obj, Int32.Parse(valor), null); break;

                            case "Int64": pro.SetValue(obj, Int64.Parse(valor), null); break;

                            case "DateTime": pro.SetValue(obj, DateTime.ParseExact(valor, "dd/MM/yyyy HH:mm:ss", null), null); break;

                            case "Decimal": pro.SetValue(obj, decimal.Parse(valor.ToString()), null); break;

                            case "DBNull": pro.SetValue(obj, null, null); break;

                            default: pro.SetValue(obj, valor, null); break;
                        }
                    }
                    catch (Exception error)
                    {
                        throw new Exception($"Convert your property {column.ColumnName} of class {obj.GetType().Name} to type {tipoBanco}. It's value is '{valor}'", error);
                    }

                }
                else
                    continue;
            }
        }
        return obj;
    }



    /// <summary>
    /// Execute a query on db and return just a one entry parsed to yout T class
    /// </summary>
    /// <typeparam name="T">Class to parse the return</typeparam>
    /// <param name="sql">String with your query</param>
    /// <param name="parameters">A Dictionary of parameters to prepared statement like NpgsqlParameter or OracleParameter</param>
    /// <returns>A object of your T class or null</returns>
    public T queryOne<T>(string sql, Dictionary<string, dynamic> parameters = null)
    {
        var list = this.query<T>(sql, parameters);
        if (list.Count >= 1)
        {
            return list[0];
        }
        return default(T);
    }


    /// <sumpublicmary>
    /// Execute a query on db and return a list of entry parsed to yout T class
    /// </summary>
    /// <typeparam name="T">Class to parse the return</typeparam>
    /// <param name="sql">String with your query</param>
    /// <param name="parameters">A Dictionary of parameters to prepared statement like NpgsqlParameter or OracleParameter</param>
    /// <returns>A list of objects of your T class or a empty list</returns>
    public List<T> query<T>(string sql, Dictionary<string, dynamic> parameters = null)
    {
        using (var cmd = _conn.CreateCommand())
        {
            OpenConnection();
            cmd.CommandText = sql;

            if (parameters is not null && parameters.Count >= 1)
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.Add(param);
                }
            }

            var reader = cmd.ExecuteReader();

            DataTable datatable = new DataTable();
            datatable.Load(reader);

            var json = JsonConvert.SerializeObject(datatable);

            return JsonConvert.DeserializeObject<List<T>>(json);


            List<T> list = new List<T>();

            for (int i = 0; i < datatable.Rows.Count; i++)
            {
                var objeto = datatable.Rows[i];

                T add = GetItem<T>(objeto);
                list.Add(add);

            }
            return list;
        };
    }



    /// <summary>
    /// Execute a non query (INSERT, DELETE, UPDATE, DROP, CREATE, etc) on db and return the number of tuples affecteds
    /// </summary>
    /// <param name="parameters">A Dictionary of parameters to prepared statement like NpgsqlParameter or OracleParameter</param>
    /// <param name="parameters"></param>
    /// <returns>The number of tuples affecteds</returns>
    public int execute(string sql, Dictionary<string, dynamic> parameters = null)
    {
        using (var cmd = _conn.CreateCommand())
        {
            OpenConnection();
            cmd.Connection = _conn;
            cmd.CommandText = sql;

            if (parameters is not null && parameters.Count >= 1)
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.Add(param);
                }
            }

            int rowsUpdated = cmd.ExecuteNonQuery();
            return rowsUpdated;
        }
    }




    // To detect redundant calls
    private bool _disposedValue;

    // Public implementation of Dispose pattern callable by consumers.
    public void Dispose() => Dispose(true);

    // Protected implementation of Dispose pattern.
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _conn.Close();
                _conn.Dispose();
            }

            _disposedValue = true;
        }
    }
}
