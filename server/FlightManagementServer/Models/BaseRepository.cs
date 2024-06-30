using log4net;
using System.Data;
using System.Data.SqlClient;


namespace FlightManagementServer.Models
{
    /// <summary>
    /// Contains common base repository functions. Can't be instantiated.
    /// Should be inherited by every repository class.
    /// /// </summary>
    public abstract class BaseRepository
    {
        #region ==========BASICS=====================

        private readonly SqlConnection _connection;
        private SqlTransaction _transaction;
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Connection getter
        /// </summary>
        protected SqlConnection Connection
        {
            get { return _connection; }
        }

        /// <summary>
        /// Transaction getter
        /// </summary>
        protected SqlTransaction Transaction
        {
            get { return _transaction; }
        }

        /// <summary>
        /// Initializes the connection using ninject
        /// </summary>
        /// <param name="connection"></param>
        protected BaseRepository(SqlConnection connection)
        {
            this._connection = connection;
        }

        ///  <summary>
        /// Open the connection.
        /// </summary>
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

        /// <summary>
        /// Check whether the connection is closed
        /// </summary>
        /// <returns></returns>
        private bool IsConnectionClosed()
        {
            if ((_connection == null) || (_connection.State == ConnectionState.Closed))
                return true;

            return false;
        }

        ///  <summary>
        /// Close the connection.
        /// </summary>
        public void CloseConnection()
        {
            if (_connection == null) return;
            if (_connection.State == ConnectionState.Open)
                _connection.Close();
        }

        /// <summary>
        /// Begins a new transaction. To begin a transaction the connection should be opened?
        /// </summary>
        public void BeginTransaction()
        {
            if (IsConnectionClosed())
                OpenConnection();

            _transaction = _connection.BeginTransaction();

        }

        /// <summary>
        /// Commits the current trunsaction
        /// </summary>
        public void CommitTransaction()
        {
            if (_transaction != null)
                _transaction.Commit();

            CloseConnection();
        }

        /// <summary>
        /// Rollbacks the current trunsaction
        /// </summary>
        public void RollbackTransaction()
        {
            if (_transaction != null)
                _transaction.Rollback();

            CloseConnection();
        }

        /// <summary>
        /// struct for passing params from specific entity repository function to base repository function.
        /// Base repository functions biulds command params according the Param data
        /// </summary>
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

        #endregion

        #region ==========GET FUNCTIONS==============

        /// <summary>
        /// Gets data from db by entity id
        /// IMPORTANT: Param name in the stored procedure must be "@ID"
        /// </summary>
        /// <param name="spName">stored procedure name</param>
        /// <param name="id">entity id to return</param>
        /// <returns></returns>
        protected SqlDataReader GetDataById(string spName, int id)
        {
            //defines new sql command to call stored procedure
            var cmd = new SqlCommand(spName, Connection) { CommandType = CommandType.StoredProcedure };

            //initializes id param
            cmd.Parameters.Add("@ID", SqlDbType.Int);
            cmd.Parameters["@ID"].Value = id;

            //opens connections and gets the data
            try
            {
                this.OpenConnection();

                SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.SingleRow);

                return rdr;
            }
            catch (CustomException ex)
            {
                //just "this.OpenConnection()" can return CustomException therefore there nothing to close
                throw ex;
            }
            catch (Exception ex)
            {
                this.CloseConnection();
                throw new CustomException("Failed to GetDataById in sp: " + spName, ex);
            }
        }

        /// <summary>
        /// Gets data from db
        /// </summary>
        /// <param name="spName">stored procedure name</param>
        /// <returns></returns>
        protected SqlDataReader GetData(string spName)
        {

            //opens connections and gets the data
            try
            {
                //defines new sql command to call stored procedure
                var cmd = new SqlCommand(spName, Connection) { CommandType = CommandType.StoredProcedure };

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
                this.CloseConnection();
                throw new CustomException("Failed to GetData in sp: " + spName, ex);
            }
        }

        /// <summary>
        /// Gets data from the database according to different parameters
        /// </summary>
        /// <param name="spName">stored procedure name</param>
        /// <param name="paramsList">parameters list for the query</param>
        /// <returns></returns>
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

        protected DataTable GetDataForCsv(string spName)
        {
            return GetDataForCsv(spName, null);
        }

        /// <summary>
        /// Dedicated function to getting data for export to excel
        /// </summary>
        /// <param name="spName"></param>
        /// <returns>Data table with the requested data</returns>
        protected DataTable GetDataForCsv(string spName, List<Param> paramsList)
        {
            try
            {
                using (var cmd = new SqlCommand(spName, Connection) { CommandType = CommandType.StoredProcedure })
                {
                    //builds sql command parameters list
                    if (paramsList != null && paramsList.Count > 0)
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

                    //this.OpenConnection();
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dataTable = new DataTable();
                        dataAdapter.SelectCommand.CommandTimeout = 180;  // seconds
                        dataAdapter.Fill(dataTable);
                        return dataTable;
                    }
                }

            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CustomException("Failed to GetData in sp: " + spName, ex);
            }
        }

        protected SqlDataReader GetDataWithTrans(string spName, List<Param> paramsList)
        {
            try
            {
                //defines new sql command to call stored procedure
                var cmd = new SqlCommand(spName, Connection, Transaction) { CommandType = CommandType.StoredProcedure };

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

                SqlDataReader rdr = cmd.ExecuteReader();

                return rdr;

            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                this.RollbackTransaction();
                throw new CustomException("Failed to GetData with params in stp: " + spName, ex);
            }
        }

        #endregion

        #region ==========SAVE FUNCTIONS==============

        protected string SaveDataWithOutParamStr(List<Param> paramsList, string spName)
        {
            try
            {
                //defines new sql command to call stored procedure
                var cmd = new SqlCommand(spName, Connection) { CommandType = CommandType.StoredProcedure };

                //builds sql command parameters list
                if (paramsList.Count > 0)
                    foreach (var param in paramsList)
                    {
                        if (param.Direction == ParameterDirection.Output)
                        {
                            //cmd.Parameters.Add("@JobNumbers", SqlDbType.NVarChar, -1);
                            cmd.Parameters.Add("@OUT_PARAM", SqlDbType.NVarChar, -1);
                            cmd.Parameters["@OUT_PARAM"].Direction = ParameterDirection.Output;
                        }
                        else if (param.Value != null)
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

                //executes the save stored procedure
                cmd.ExecuteNonQuery();

                string output = (string)cmd.Parameters["@OUT_PARAM"].Value;

                this.CloseConnection();

                return output;

            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                this.CloseConnection();
                throw new CustomException("Failed to GetData with params in stp: " + spName, ex);
            }
        }

        protected int SaveDataWithOutParam(List<Param> paramsList, string spName)
        {
            try
            {
                //defines new sql command to call stored procedure
                var cmd = new SqlCommand(spName, Connection) { CommandType = CommandType.StoredProcedure };

                //builds sql command parameters list
                if (paramsList.Count > 0)
                    foreach (var param in paramsList)
                    {
                        if (param.Direction == ParameterDirection.Output)
                        {
                            cmd.Parameters.Add("@OUT_PARAM", SqlDbType.Int);
                            cmd.Parameters["@OUT_PARAM"].Direction = ParameterDirection.Output;
                        }
                        else if (param.Value != null)
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

                //executes the save stored procedure
                cmd.ExecuteNonQuery();

                int output = (int)cmd.Parameters["@OUT_PARAM"].Value;

                this.CloseConnection();

                return output;

            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                this.CloseConnection();
                throw new CustomException("Failed to GetData with params in stp: " + spName, ex);
            }
        }

        protected void SaveData(string spName)
        {
            SaveData(null, spName);
        }

        protected void SaveData(List<Param> paramsList, string spName)
        {
            try
            {
                //defines new sql command to call stored procedure
                var cmd = new SqlCommand(spName, Connection) { CommandType = CommandType.StoredProcedure };

                //builds sql command parameters list
                if (paramsList != null && paramsList.Count > 0)
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

                //executes the save stored procedure
                cmd.ExecuteNonQuery();

                this.CloseConnection();

            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                this.CloseConnection();
                throw new CustomException("Failed to GetData with params in stp: " + spName, ex);
            }
        }

        /// <summary>
        /// Inserts number of rows for specific entity
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity">the new entity data for insert</param>
        /// <param name="spName">stored procedure name</param>
        /// <returns>the id of the inserted/updated entity</returns>
        protected void SaveData<TEntity>(IList<TEntity> entityColl, string spName) where TEntity : BaseEntity
        {
            DataTable temp, dtToSave;
            bool tableCloned = false;

            try
            {
                //defines new sql command to call stored procedure
                var cmd = new SqlCommand(spName, Connection) { CommandType = CommandType.StoredProcedure };

                dtToSave = new DataTable(entityColl[0].GetParamTableName());

                foreach (var entity in entityColl)
                {
                    temp = entity.PrepareDataForSave();
                    if (!tableCloned)
                    {
                        dtToSave = temp.Clone();
                        tableCloned = true;
                    }
                    dtToSave.ImportRow(temp.Rows[0]);
                }

                cmd.Parameters.AddWithValue("@" + dtToSave.TableName, dtToSave);

                //opens connections 
                this.OpenConnection();

                //executes the save stored procedure
                cmd.ExecuteNonQuery();

                this.CloseConnection();
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


        protected int SaveData<TEntity>(TEntity entity, string spName) where TEntity : BaseEntity
        {
            return SaveData(entity, 0, spName);
        }

        /// <summary>
        /// Save function. In the stored procedure there is a check for the entityId - if it is 0 then it's a new
        /// entity and insert should be done. If the value is different then 0 then update action should be executed.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity">The entity for saving</param>
        /// <param name="entityId">entity id - if it is insert then 0 is passed to the sp</param>
        /// <param name="spName">stored procedure name</param>
        /// <returns>the id of the inserted/updated entity</returns>
        protected int SaveData<TEntity>(TEntity entity, int entityId, string spName) where TEntity : BaseEntity
        {
            try
            {
                //defines new sql command to call stored procedure
                var cmd = new SqlCommand(spName, Connection) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@NEW_ID", SqlDbType.Int);
                cmd.Parameters["@NEW_ID"].Direction = ParameterDirection.Output;

                DataTable dtToSave = entity.PrepareDataForSave();

                foreach (DataColumn column in dtToSave.Columns)
                {
                    cmd.Parameters.AddWithValue("@" + column.ColumnName, dtToSave.Rows[0][column.ColumnName]);
                }

                //cmd.Parameters.AddWithValue("@" + dtToSave.TableName, dtToSave);

                //opens connections 
                this.OpenConnection();

                //executes the save stored procedure
                cmd.ExecuteNonQuery();

                int id = (int)cmd.Parameters["@NEW_ID"].Value;

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

        protected int SaveData<TEntity>(TEntity entity, bool IsNew, string spName) where TEntity : BaseEntity
        {
            try
            {
                //defines new sql command to call stored procedure
                var cmd = new SqlCommand(spName, Connection) { CommandType = CommandType.StoredProcedure };

                //initializes id param
                cmd.Parameters.Add("@ISNEW", SqlDbType.Bit);
                cmd.Parameters["@ISNEW"].Value = IsNew;

                DataTable dtToSave = entity.PrepareDataForSave();

                foreach (DataColumn column in dtToSave.Columns)
                {
                    cmd.Parameters.AddWithValue("@" + column.ColumnName, dtToSave.Rows[0][column.ColumnName]);
                }

                //cmd.Parameters.AddWithValue("@" + dtToSave.TableName, dtToSave);

                //opens connections 
                this.OpenConnection();

                //executes the save stored procedure
                int resCnt = cmd.ExecuteNonQuery();

                this.CloseConnection();

                return resCnt;

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

        protected int SaveDataWithTrans<TEntity>(TEntity entity, string spName) where TEntity : BaseEntity
        {
            return SaveDataWithTrans(entity, 0, spName);
        }

        /// <summary>
        ///  Save function. In the stored procedure there is a check for the entityId - if it is 0 then it's a new
        /// entity and insert should be done. If the value is different then 0 then update action should be executed.
        /// IMPORTANT: Before this function is called, transaction must be started with "BeginTransaction();". This 
        /// function opens connection to db and begins transaction. After all the required actions are executed the 
        /// "CommitTransaction();" function should be called.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity">The entity for saving</param>
        /// <param name="entityId">Entity id. If it is insert then 0 is passed to the sp</param>
        /// <param name="spName">Stored procedure name</param>
        /// <returns>The id of the inserted/updated entity</returns>
        protected int SaveDataWithTrans<TEntity>(TEntity entity, int entityId, string spName, List<Param> paramsList = null) where TEntity : BaseEntity
        {
            try
            {
                //defines new sql command to call stored procedure
                var cmd = new SqlCommand(spName, Connection, Transaction) { CommandType = CommandType.StoredProcedure };

                //initializes id param
                //cmd.Parameters.Add("@ID", SqlDbType.Int);
                //cmd.Parameters["@ID"].Value = entityId;

                //Initialize output ID parameter
                cmd.Parameters.Add("@NEW_ID", SqlDbType.Int);
                cmd.Parameters["@NEW_ID"].Direction = ParameterDirection.Output;

                DataTable dtToSave = entity.PrepareDataForSave();
                foreach (DataColumn column in dtToSave.Columns)
                {
                    cmd.Parameters.AddWithValue("@" + column.ColumnName, dtToSave.Rows[0][column.ColumnName]);
                }

                //cmd.Parameters.AddWithValue("@" + dtToSave.TableName, dtToSave);

                //builds sql command parameters list
                if (paramsList != null)
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


                //executes the save stored procedure
                cmd.ExecuteNonQuery();

                int id = (int)cmd.Parameters["@NEW_ID"].Value;

                return id;

            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                Transaction.Rollback();
                throw new CustomException("Failed to SaveData in sp: " + spName, ex);
            }
        }

        protected void SaveDataWithTrans(string spName)
        {
            SaveDataWithTrans(null, spName);
        }

        protected void SaveDataWithTrans(List<Param> paramsList, string spName)
        {
            try
            {
                //defines new sql command to call stored procedure
                var cmd = new SqlCommand(spName, Connection, Transaction) { CommandType = CommandType.StoredProcedure };

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

                cmd.ExecuteNonQuery();

            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                Transaction.Rollback();
                throw new CustomException("Failed to SaveDataWithTrans with params in stp: " + spName, ex);
            }
        }

        protected int SaveDataWithTrans<TEntity>(IList<TEntity> entityColl, string spName, List<Param> paramsList = null) where TEntity : BaseEntity
        {
            DataTable temp, dtToSave;
            bool tableCloned = false;

            try
            {
                //defines new sql command to call stored procedure
                var cmd = new SqlCommand(spName, Connection, Transaction) { CommandType = CommandType.StoredProcedure };

                //Initialize output ID parameter
                cmd.Parameters.Add("@NEW_ID", SqlDbType.Int);
                cmd.Parameters["@NEW_ID"].Direction = ParameterDirection.Output;

                //builds sql command parameters list
                if (paramsList != null)
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

                //prepare data for save
                if (entityColl.Count > 0)
                {
                    dtToSave = new DataTable(entityColl[0].GetParamTableName());
                    foreach (var entity in entityColl)
                    {
                        temp = entity.PrepareDataForSave();
                        if (!tableCloned)
                        {
                            dtToSave = temp.Clone();
                            tableCloned = true;
                        }
                        dtToSave.ImportRow(temp.Rows[0]);
                    }
                    cmd.Parameters.AddWithValue("@" + dtToSave.TableName, dtToSave);
                }

                //executes the save stored procedure
                cmd.ExecuteNonQuery();

                int id = (int)cmd.Parameters["@NEW_ID"].Value;

                return id;
            }
            catch (CustomException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                Transaction.Rollback();
                throw new CustomException("Failed to SaveDataWithTrans in sp: " + spName, ex);
            }
        }

        #endregion

        #region ==========GENERAL FUNCTIONS==========

        /// <summary>
        /// Returns the SqlDbType of param accordingly to its value.
        /// </summary>
        /// <param name="param">Parameter whose value should be checked</param>
        /// <returns>SqlDbType of the param</returns>
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

        #endregion

        #region ==========CHECK PERMISSIONS FUNCTIONS==========

        /// <summary>
        /// Function checks whether the current user has write permission to an object of typeToCheck type.
        /// Used in SAVE functions.
        /// </summary>
        /// <param name="typeToCheck"></param>
        /// <returns>true - if the user has the permissions, otherwise false</returns>
        /// <history>Olga - 18/11/2015</history>
        

        /// <summary>
        /// Function checks whether the current user has read permission to an object of typeToCheck type.
        /// Used in GET functions.
        /// </summary>
        /// <param name="typeToCheck"></param>
        /// <returns>true - if the user has the permissions, otherwise false</returns>
        /// <history>Olga - 18/11/2015</history>
        

        #endregion
    }
}

