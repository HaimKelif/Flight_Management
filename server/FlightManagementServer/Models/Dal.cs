using System.Data.SqlClient;
using System.Data;
using System.ComponentModel;

namespace FlightManagementServer
{
    public class Dal
    {
        public enum SQLType
        {
            Update,
            Insert,
            Delete,
            None
        };

        private SqlConnection _connection = null;
        private SqlTransaction _transaction = null;
        private string _connectionString = string.Empty;

        public Dal(string connectionString)
        {
            _connectionString=connectionString;
        }

        private SqlConnection connection
        {
            get
            {
                if (_connection == null)
                    _connection = new SqlConnection(_connectionString);
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();
                return _connection;
            }
        }

        public void closeConnection()
        {
            if (_transaction == null)
            {
                if (_connection != null && _connection.State != ConnectionState.Closed)
                {
                    _connection.Close();
                }
                _connection = null;
            }
        }

        public DataSet getDataSet(string sqlStr, SqlParameter[] parameters)
        {
            DataSet dataSet = new DataSet();
            using (SqlCommand command = new SqlCommand(sqlStr, connection, _transaction))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);
                using (SqlDataAdapter dbDataAdapter = new SqlDataAdapter(command))
                {
                    dbDataAdapter.Fill(dataSet);
                }
            }
            closeConnection();
            return dataSet;
        }

        public DataTable getDataTable(string sqlStr, SqlParameter[] parameters)
        {
            return getDataSet(sqlStr, parameters).Tables[0];
        }

        public DataView getDataView(string sqlStr, SqlParameter[] parameters) 
        {
            return new DataView(getDataTable(sqlStr, parameters));
        }

        public int exexuteQuery(string sqlStr, SqlParameter[] parameters)
        {
            int retVal;
            using (SqlCommand command = new SqlCommand(sqlStr, connection, _transaction))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);
                retVal = command.ExecuteNonQuery();
            }
            closeConnection();
            return retVal;
        }

        public int exexuteCommand(SqlCommand command)
        {
            int retVal;
            using (command)
            {
                command.Connection = connection;
                retVal = command.ExecuteNonQuery();
            }
            closeConnection();
            return retVal;
        }

        public object getScalar(string sqlStr, SqlParameter[] parameters) 
        {
            object retVal=null;
            using (SqlCommand command = new SqlCommand(sqlStr, connection, _transaction))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);
                retVal = command.ExecuteScalar();
            }
            closeConnection();
            return retVal;
        }

        public SqlTransaction startTransaction()
        {
            _transaction = connection.BeginTransaction();
            return _transaction;
        }

        public SqlTransaction startTransaction(System.Data.IsolationLevel isolationLevel)
        {
            _transaction = connection.BeginTransaction(isolationLevel);
            return _transaction;
        }

        public void commitTransaction()
        {
            if (_transaction !=null)
            {
                _transaction.Commit();
                _transaction = null;
            }
        }

        public void rollbackTransaction()
        {
            if (_transaction !=null)
            {
                _transaction.Rollback();
                _transaction = null;
            }
        }

        public int update(DataTable dataTable, string sqlStr, SQLType sqlType)
        {
            int retVal = 0;
            if (dataTable == null) return retVal;
            using (SqlCommand command = new SqlCommand(sqlStr, connection, _transaction))
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter())
                {
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        if (sqlStr.ToUpper().IndexOf("@" + column.ColumnName.ToUpper()) > -1)
                            command.Parameters.Add("@" + column.ColumnName, convertDBType(column.DataType), column.MaxLength, column.ColumnName);
                    }
                    switch (sqlType)
                    {
                        case SQLType.Insert:
                            adapter.InsertCommand = command;
                            break;
                        case SQLType.Update:
                            adapter.UpdateCommand = command;
                            break;
                        case SQLType.Delete:
                            adapter.DeleteCommand = command;
                            break;
                    }
                    retVal = adapter.Update(dataTable);
                }
            }
            closeConnection();
            return retVal;
        }

        private SqlDbType convertDBType(System.Type dataType)
        {
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.DbType = (DbType)TypeDescriptor.GetConverter(sqlParameter.DbType).ConvertFrom(dataType.Name);
            return sqlParameter.SqlDbType;
        }

        public struct Param
        {
            private readonly string _name;
            private readonly object _value;
            private readonly ParameterDirection _dir;

            public Param(string name, object value, ParameterDirection? dir = null)
                : this()
            {
                this._name = name;
                this._value = value;
                if (dir == null)
                    this._dir = ParameterDirection.Input;
                else
                    this._dir = ParameterDirection.Output;
            }

            public string Name { get { return _name; } }
            public object Value { get { return _value; } }
            public ParameterDirection Direction { get { return _dir; } }
        }
    
    }
}