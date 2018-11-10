using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.Reflection;

namespace SysAudit.Controllers
{
    public sealed class ControllerAD
    {
        #region Properties

        public Type Type
        {
            get
            {
                if (_type != null) return _type;
                throw new Exception(string.Format("{0}: Domain object return Type was not specified", nameof(ControllerAD)));
            }
            set
            {
                _type = value;
                // Set the Class and get the PropertyInfos as soon as the type is available
                this.Class = value.Name;
                this.PropertyInfos = GetPropertyInfos();
            }
        }
        public string ConnectionString
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_connectionString)) return _connectionString;
                throw new Exception(string.Format("{0}: ConnectionString was not specified", nameof(ControllerAD)));
            }
            set
            {
                _connectionString = value;
            }
        }
        public string Attribute
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_attribute)) return _attribute;
                throw new Exception(string.Format("{0}: Attribute was not specified", nameof(ControllerAD)));
            }
            set
            {
                _attribute = value;
            }
        }
        public string Filter { get; set; }

        // Private Properties
        private string Class { get; set; }
        private PropertyInfo[] PropertyInfos
        {
            get
            {
                return _propertyInfos;
            }
            set
            {
                _propertyInfos = value;
                // Get the Properties array as soon as the PropertyInfos are available
                this.Properties = GetProperties();
            }
        }
        private string[] Properties { get; set; }

        // Backing Fields
        private Type _type;
        private string _connectionString;
        private string _attribute;
        private string _filter;
        private PropertyInfo[] _propertyInfos;

        #endregion

        /// <summary>
        /// Initialized a new instance of the ControllerAD class.
        /// </summary>
        /// <param name="connectionString">The property formatted LDAP connection string.</param>
        /// <param name="attribute">The name of the identification field to be used.</param>
        public ControllerAD(string connectionString = null, string @class = null, string attribute = null)
        {
            this.ConnectionString = connectionString;
            this.Class = @class;
            this.Attribute = attribute;
        }

        #region Public Methods

        public T GetRecord<T>(string attributeValue, string attribute = null, string filter = null) where T : new()
        {
            if (attribute == null) attribute = this.Attribute;
            if (filter == null) filter = this.Filter;

            try
            {
                using (DirectoryEntry directoryEntry = new DirectoryEntry(this.ConnectionString))
                {
                    directoryEntry.AuthenticationType = AuthenticationTypes.FastBind | AuthenticationTypes.Secure;
                    using (DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry))
                    {
                        directorySearcher.Filter = string.Format("(&(objectClass={0})({1}={2}))", this.Class, attribute, attributeValue);
                        directorySearcher.PropertiesToLoad.AddRange(this.Properties);

                        return GetPopulatedObject<T>(directorySearcher.FindOne());
                    }
                }
            }
            catch (Exception ex)
            {
                Global.LogEvent(string.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message));
            }

            return default(T);
        }

        #endregion

        #region Private Methods

        private PropertyInfo[] GetPropertyInfos()
        {
            // Get all non shared/static properties of the generic object
            return this.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToArray();
        }

        private string[] GetProperties()
        {
            // Create array of property names for DirectorySearcher.PropertiesToLoad.AddRange(string[])
            return PropertyInfos.Select(p => p.Name).ToArray();
        }

        private T GetPopulatedObject<T>(SearchResult searchResult) where T : new()
        {
            T @object = new T();
            foreach (PropertyInfo propertyInfo in this.PropertyInfos)
            {
                if (propertyInfo != null && propertyInfo.CanWrite)
                {
                    if (searchResult.Properties[propertyInfo.Name].Count >= 1)
                    {
                        var value = searchResult.Properties[propertyInfo.Name][0].ToString();
                        propertyInfo.SetValue(@object, value);
                    }
                }
            }

            return @object;
        }


        #endregion
    }
}
