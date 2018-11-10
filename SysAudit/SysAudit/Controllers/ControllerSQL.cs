using System;
using System.Linq;
using System.Data.SqlClient;
using System.Reflection;

namespace SysAudit.Controllers
{
    public sealed class ControllerSQL
    {
        #region Properties

        // Public Properties
        public Type Type
        {
            get
            {
                if (_type != null) return _type;
                throw new Exception(string.Format("{0}: Domain object return Type was not specified", nameof(ControllerSQL)));
            }
            set
            {
                _type = value;
                // Get the PropertyInfos as soon as the type is available
                this.PropertyInfos = GetPropertyInfos();
            }
        }
        public string ConnectionString
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_connectionString)) return _connectionString;
                throw new Exception(string.Format("{0}: ConnectionString was not specified", nameof(ControllerSQL)));
            }
            set
            {
                _connectionString = value;
            }
        }
        public string Schema
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_schema)) return _schema;
                throw new Exception(string.Format("{0}: Database schmea was not specified", nameof(ControllerSQL)));
            }
            set
            {
                _schema = value;
            }
        }
        public string TableName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_tableName)) return _tableName;
                throw new Exception(string.Format("{0}: Database TableName was not specified", nameof(ControllerSQL)));
            }
            set
            {
                _tableName = value;
            }
        }
        public string IdField
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_idField)) return _idField;
                throw new Exception(string.Format("{0}: IdField was not specified", nameof(ControllerSQL)));
            }
            set
            {
                _idField = value;
            }
        }

        // Private Properties
        private PropertyInfo[] PropertyInfos
        {
            get
            {
                return _propertyInfos;
            }
            set
            {
                _propertyInfos = value;
                // Get the Fields string as soon as the PropertyInfos are available
                this.Fields = GetFields(FieldTypes.Fields);
            }
        }
        private string Fields { get; set; }

        // Backing Fields
        private Type _type;
        private string _connectionString;
        private string _schema;
        private string _tableName;
        private string _idField;
        private PropertyInfo[] _propertyInfos;

        #endregion

        /// <summary>
        /// Initialized a new instance of the ControllerSQL class.
        /// </summary>
        /// <param name="connectionString">The connection string property formatted for the System.Data.SqlClient provider.</param>
        /// <param name="schema">The database schema holding the needed table(s).</param>
        /// <param name="tableName">The name of the needed database table.</param>
        /// <param name="idField">The name of the identification field to be used.</param>
        public ControllerSQL(string connectionString = null, string schema = "dbo", string tableName = null, string idField = null)
        {
            this.ConnectionString = connectionString;
            this.Schema = schema;
            this.TableName = tableName;
            this.IdField = idField;
        }

        #region Public Methods

        /// <summary>
        /// Posts a record of the specified type. Performs an update if supplied with the Id.
        /// </summary>
        /// <param name="record">The record of the specified type to post.</param>
        /// <param name="id">The optional id provided when performing an update.</param>
        /// <returns>The number of rows affected.</returns>
        public int PostRecord<T>(T record, object id = null)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(this.ConnectionString))
                {
                    sqlConnection.Open();

                    string commandText = null;
                    if (id != null)
                        commandText = string.Format("UPDATE [{0}] SET {1} WHERE [{2}] = @id", this.TableName, GetFields(FieldTypes.Updates), this.IdField);
                    else
                        commandText = string.Format("INSERT INTO [{0}] ({1}) VALUES ({2})", this.TableName, Fields, GetFields(FieldTypes.Values));

                    using (SqlCommand sqlCommand = new SqlCommand(commandText, sqlConnection))
                    {
                        if (id != null) sqlCommand.Parameters.AddWithValue("@id", id);
                        foreach (PropertyInfo property in this.PropertyInfos)
                            sqlCommand.Parameters.AddWithValue(string.Format("@{0}", property.Name), property.GetValue(record));

                        return sqlCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Global.LogEvent(string.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message));
            }

            return default(int);
        }

        #endregion

        #region Private Methods

        private PropertyInfo[] GetPropertyInfos()
        {
            // Get all non shared/static properties of the generic object
            return this.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToArray();
        }

        private string GetFields(FieldTypes fieldType)
        {
            string returnValue = null;

            switch (fieldType)
            {
                case FieldTypes.Fields:
                    // Concatenated fields for SELECT command
                    returnValue = string.Join(", ", PropertyInfos.Select(p => string.Format("[{0}]", p.Name)).ToArray()); break;
                case FieldTypes.Values:
                    // Concatenated fields for INSERT command
                    returnValue = string.Join(", ", PropertyInfos.Select(p => string.Format("@{0}", p.Name)).ToArray()); break;
                case FieldTypes.Updates:
                    // Concatenated fields for UPDATE command
                    returnValue = string.Join(", ", PropertyInfos.Where(p => p.Name != this.IdField).Select(p => string.Format("[{0}] = @{1}", p.Name, p.Name)).ToArray()); break;
            }

            return returnValue;
        }

        private enum FieldTypes
        {
            Fields,
            Values,
            Updates
        }

        #endregion

    }
}