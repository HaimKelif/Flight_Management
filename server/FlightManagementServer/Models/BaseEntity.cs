using log4net;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace FlightManagementServer.Models
{
    public class DBAttribute : System.Attribute
    {
        public readonly string ParamName;

        public DBAttribute(string paramName)
        {
            this.ParamName = paramName;
        }
    }
    /// <summary>
    ///Must be inherited
    ///Allows functionality of generic load of data from DB
    /// </summary>
    public abstract class BaseEntity
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Fills the properties of the object with the given values in the sqlReader.
        /// IMPORTANT: 
        /// In order that the function will work the database fields names and the object properties names must be the same. 
        /// If at some point it will be impossible to use identical names, attributes may be added to the object properties and the
        /// mapping should rely on them.
        /// </summary>
        /// <param name="sqlReader">SqlDataReader filled with data.</param>
        public void FillAll(SqlDataReader sqlReader)
        {
            if (sqlReader == null) throw new ArgumentNullException("sqlReader");
            //Gets all the properties of the entity
            PropertyInfo[] objectProperties = this.GetType().GetProperties();
            //Loop to fill each property
            foreach (PropertyInfo propertyInfo in objectProperties)
            {
                FillProperty(propertyInfo, sqlReader, "FillAll");
            }
        }

        /// <summary>
        /// Like the "FillAll" function with addition of permissions check. 
        /// The function fills the property only if the user permitted to see this value.
        /// </summary>
        /// <param name="sqlReader">SqlDataReader filled with data.</param>
        public void Fill(SqlDataReader sqlReader)
        {
            if (sqlReader == null) throw new ArgumentNullException("sqlReader");
            //UserData userData = (UserData)HttpContext.Current.Session["UserData"];

            //Gets all the properties of the entity
            PropertyInfo[] objectProperties = this.GetType().GetProperties();
            //DBAttribute dbClassAttr = (DBAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(DBAttribute));
            //Loop to fill each property
            foreach (PropertyInfo propertyInfo in objectProperties)
            {
                //    DBAttribute dbAttr = (DBAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(DBAttribute));
                //    List<PermissionDetails> permissionDetails = null;
                //    if (dbClassAttr != null && dbAttr != null)
                //        permissionDetails = Globals.PermissionDetails.Where(rd => rd.prd_entity_name == dbClassAttr.ParamName && rd.prd_field_name == dbAttr.ParamName && rd.prd_visibility == false).ToList();
                //    if (permissionDetails != null && permissionDetails.Count > 0 && userData.user_permissions.Where(ur => permissionDetails.Select(rd => rd.prd_prm_id).Contains(ur.prm_id)).Count() == 0)
                //        propertyInfo.SetValue(this, null, null);
                //    else
                FillProperty(propertyInfo, sqlReader, "Fill");
            }
        }

        /// <summary>
        /// Recursive function that sets the value of specific property.
        /// </summary>
        /// <param name="propertyInfo">property</param>
        /// <param name="sqlReader">SqlDataReader filled with data</param>
        private void FillProperty(PropertyInfo propertyInfo, IDataRecord sqlReader, string InvokeMethod)
        {
            try
            {
                Type propType = propertyInfo.PropertyType;

                //Check if the current property is a collection - if so just sets property value to null
                if (propType.IsGenericType && typeof(ICollection<>).IsAssignableFrom(propType.GetGenericTypeDefinition()) ||
                    propType.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>)))
                {
                    propertyInfo.SetValue(this, null);
                }
                //Check if the current property type inherits from the custom  BaseEntity type
                else if (propType.IsSubclassOf(typeof(BaseEntity)))
                {
                    //If so, create instance of the property and invoke the "Fill" method
                    var paramsArray = new object[] { sqlReader };
                    var classInstance = Activator.CreateInstance(propertyInfo.PropertyType, null);
                    propertyInfo.SetValue(this, classInstance, null);
                    MethodInfo methodInfo = propertyInfo.PropertyType.GetMethod(InvokeMethod);
                    methodInfo.Invoke(classInstance, paramsArray);
                }
                else
                {
                    //else the property type is built-in type and the property can be filled with the data
                    var propertyValue = GetPropertyValue(propertyInfo, sqlReader);

                    if (propertyValue != null)
                        propertyInfo.SetValue(this, propertyValue, null);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException("Failed to set value of property " + propertyInfo.Name, ex);
            }
        }


        /// <summary>
        /// Intended for save function
        /// </summary>
        /// <returns></returns>
        public string GetParamTableName()
        {
            DBAttribute dbClassAttr = (DBAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(DBAttribute));

            if (dbClassAttr != null)
                return dbClassAttr.ParamName;
            else
                return "";
        }

        public DataTable PrepareDataForSave()
        {
            DataTable dataToSave;

            try
            {
                //UserData userData = (UserData)HttpContext.Current.Session["UserData"];
                //Gets all the properties of the entity
                PropertyInfo[] objectProperties = this.GetType().GetProperties();

                DBAttribute dbClassAttr = (DBAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(DBAttribute));

                dataToSave = new DataTable(dbClassAttr.ParamName);
                DataRow dr = dataToSave.NewRow();

                //Loop to fill each property
                foreach (PropertyInfo propertyInfo in objectProperties)
                {
                    if (Attribute.IsDefined(propertyInfo, typeof(DBAttribute)))
                    {
                        DBAttribute dbAttr = (DBAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(DBAttribute));
                        dataToSave.Columns.Add(dbAttr.ParamName, Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType);
                        //dr[dbAttr.ParamName] = CheckValue(propertyInfo, dataToSave, orgEntity, dbClassAttr.ParamName, dbAttr.ParamName);
                        dr[dbAttr.ParamName] = propertyInfo.GetValue(this) ?? DBNull.Value;
                    }
                }

                dataToSave.Rows.Add(dr);

                return dataToSave;
            }
            catch (Exception ex)
            {
                dataToSave = null;
                throw new CustomException("Failed to PrepareDataForSave ", ex);
            }
        }


        


        /// <summary>
        /// Gets the property value from sqlReader
        /// </summary>
        /// <param name="propertyInfo">property</param>
        /// <param name="sqlReader">SqlDataReader filled with data</param>
        /// <returns></returns>
        private static object GetPropertyValue(PropertyInfo propertyInfo, IDataRecord sqlReader)
        {
            var propertyType = propertyInfo.PropertyType;
            var propertyName = propertyInfo.Name;

            try
            {
                object propertyValue;

                //check that the wanted column exists in the sqlReader and its value is not null
                if (ReaderContainsColumn(sqlReader, propertyName) && !sqlReader.IsDBNull(sqlReader.GetOrdinal(propertyName)))
                {
                    propertyValue = new object();

                    if (propertyType == typeof(System.Int16) || propertyType == typeof(System.Int16?))
                        propertyValue = sqlReader.GetInt16(sqlReader.GetOrdinal(propertyName));

                    if (propertyType == typeof(System.Int32) || propertyType == typeof(System.Int32?))
                        propertyValue = sqlReader.GetInt32(sqlReader.GetOrdinal(propertyName));

                    else if (propertyType == typeof(System.Int64) || propertyType == typeof(System.Int64?))
                        propertyValue = sqlReader.GetInt64(sqlReader.GetOrdinal(propertyName));

                    else if (propertyType == typeof(System.String))
                        propertyValue = sqlReader.GetString(sqlReader.GetOrdinal(propertyName));

                    else if (propertyType == typeof(System.DateTime) || propertyType == typeof(System.DateTime?))
                        propertyValue = sqlReader.GetDateTime(sqlReader.GetOrdinal(propertyName));

                    else if (propertyType == typeof(System.Boolean) || propertyType == typeof(System.Boolean?))
                        propertyValue = sqlReader.GetBoolean(sqlReader.GetOrdinal(propertyName));

                    else if (propertyType == typeof(System.Decimal) || propertyType == typeof(System.Decimal?))
                        propertyValue = sqlReader.GetDecimal(sqlReader.GetOrdinal(propertyName));

                    else if (propertyType == typeof(System.Double) || propertyType == typeof(System.Double?))
                        propertyValue = sqlReader.GetDouble(sqlReader.GetOrdinal(propertyName));

                    else if (propertyType == typeof(System.Single) || propertyType == typeof(System.Single?))
                        propertyValue = sqlReader.GetDouble(sqlReader.GetOrdinal(propertyName));


                }
                else
                {
                    propertyValue = null;
                }

                return propertyValue;

            }
            catch (Exception ex)
            {
                throw new CustomException("Failed to get value of property from sqlReader " + propertyInfo.Name, ex);
            }

        }

        /// <summary>
        /// The function checks whether the column exists in the sqlReader.The check done by column name. 
        /// </summary>
        /// <param name="reader">SqlDataReader filled with data</param>
        /// <param name="colName">column name to check</param>
        /// <returns></returns>
        private static bool ReaderContainsColumn(IDataRecord reader, string colName)
        {
            for (var i = 0; i < reader.FieldCount; i++)
                if (reader.GetName(i).Equals(colName, StringComparison.CurrentCultureIgnoreCase)) return true;
            return false;
        }

        
    }
}
