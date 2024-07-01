using log4net;
using System.Data;
using System.Data.SqlClient;


namespace FlightManagementServer.Models
{
    
    public abstract class BaseRepository
    {

        private readonly SqlConnection _connection;
        private SqlTransaction _transaction;
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        
        protected SqlConnection Connection
        {
            get { return _connection; }
        }

        
        protected SqlTransaction Transaction
        {
            get { return _transaction; }
        }

        
        protected BaseRepository(SqlConnection connection)
        {
            this._connection = connection;
        }

        
        public void OpenConnection()
        {
            try
            {
                if (_connection != null)
                {
                    if (_connection.State == ConnectionState.Closed)
                        _connection.Open();
                }
                else
                {
                    throw new CustomException("DB Connection is null");
                }
            }
            catch (Exception ex)
            {
                throw new CustomException("Failed to open connection to data base", ex);
            }

        }

        
        private bool IsConnectionClosed()
        {
            if ((_connection == null) || (_connection.State == ConnectionState.Closed))
                return true;

            return false;
        }

        
        public void CloseConnection()
        {
            if (_connection == null) return;
            if (_connection.State == ConnectionState.Open)
                _connection.Close();
        }

        
        public void BeginTransaction()
        {
            if (IsConnectionClosed())
                OpenConnection();

            _transaction = _connection.BeginTransaction();

        }

        
        public void CommitTransaction()
        {
            if (_transaction != null)
                _transaction.Commit();

            CloseConnection();
        }

        
        public void RollbackTransaction()
        {
            if (_transaction != null)
                _transaction.Rollback();

            CloseConnection();
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


        protected SqlDataReader GetData(string spName, List<Param> paramsList)
        {
            try
            {
                //defines new sql command to call stored procedure
                var cmd = new SqlCommand(spName, Connection) { CommandType = CommandType.StoredProcedure };

                //builds sql command parameters list
                if (paramsList.Count > 0)
                    foreach (var param in paramsList)
                    {
                        if (param.Value != null)
                        {
                            cmd.Parameters.Add("@" + param.Name, GetParamSqlDbType(param));
                            cmd.Parameters["@" + param.Name].Value = param.Value;
                        }
                        else
                        {
                            cmd.Parameters.Add(new SqlParameter("@" + param.Name, DBNull.Value));
                        }

                    }

                //opens connections and gets the data

                this.OpenConnection();

                SqlDataReader rdr = cmd.ExecuteReader();

                return rdr;

            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                //throw ex;
                this.CloseConnection();
                //remider
                throw new CustomException("Failed to GetData with params in stp: " + spName, ex);
            }

        }


        protected string SaveData<TEntity>(TEntity entity, string spName) where TEntity : BaseEntity
        {
            try
            {
                var cmd = new SqlCommand(spName, Connection) { CommandType = CommandType.StoredProcedure };
                DataTable dtToSave = entity.PrepareDataForSave();
                cmd.Parameters.Add("@OUT", SqlDbType.VarChar, 10);
                cmd.Parameters["@OUT"].Direction = ParameterDirection.Output;
                foreach (DataColumn column in dtToSave.Columns)
                {
                    cmd.Parameters.AddWithValue("@" + column.ColumnName, dtToSave.Rows[0][column.ColumnName]);
                }
                this.OpenConnection();
                cmd.ExecuteNonQuery();
                string id = (string)cmd.Parameters["@OUT"].Value;
                this.CloseConnection();
                return id;
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                this.CloseConnection();
                throw new CustomException("Failed to SaveData in sp: " + spName, ex);
            }
        }


        private static SqlDbType GetParamSqlDbType(Param param)
        {
            Type paramType = param.Value.GetType();

            var returnedType = new SqlDbType();

            if (paramType == typeof(System.Int32))
                returnedType = SqlDbType.Int;

            else if (paramType == typeof(System.String))
                returnedType = SqlDbType.NVarChar;

            else if (paramType == typeof(System.DateTime))
                returnedType = SqlDbType.DateTime;

            else if (paramType == typeof(System.Boolean))
                returnedType = SqlDbType.Bit;

            else if (paramType == typeof(System.Decimal))
                returnedType = SqlDbType.Decimal;

            else throw new Exception("No type found for the requested param.");

            return returnedType;
        }
    }
}

