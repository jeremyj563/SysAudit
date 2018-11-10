using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Reflection;

namespace SysAudit.Controllers
{
    public class ControllerWMI
    {
        #region Properties

        // Public Properties
        public Type Type
        {
            get
            {
                if (_type != null) return _type;
                throw new Exception(string.Format("{0}: Domain object return Type was not specified", nameof(ControllerWMI)));
            }
            set
            {
                _type = value;
                // Set the Class and get the PropertyInfos as soon as the type is available
                this.Class = value.Name;
                this.PropertyInfos = GetPropertyInfos();
            }
        }
        public string Namespace
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_nameSpace)) return _nameSpace;
                throw new Exception(string.Format("{0}: Namespace was not specified", nameof(ControllerWMI)));
            }
            set
            {
                _nameSpace = value;
            }
        }
        public string IdField
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_idField)) return _idField;
                throw new Exception(string.Format("{0}: IdField was not specified", nameof(ControllerWMI)));
            }
            set
            {
                _idField = value;
            }
        }
        public string ComputerName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_computerName)) return _computerName;
                throw new Exception(string.Format("{0}: ComputerName was not specified", nameof(ControllerWMI)));
            }
            set
            {
                _computerName = value;
                // Get the ProviderArchitecture as soon as the ComputerName is available
                this.ProviderArchitecture = GetProviderArchitecture(value);
            }
        }

        // Private Properties
        private string Class { get; set; }
        private ProviderArchitectures ProviderArchitecture { get; set; }
        private ConnectionOptions ConnectionOptions { get; set; }
        private string Fields { get; set; }
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
                this.Fields = GetFields();
            }
        }

        // Backing Fields
        private Type _type;
        private string _nameSpace;
        private string _idField;
        private string _computerName;
        private PropertyInfo[] _propertyInfos;

        #endregion

        /// <summary>
        /// Initialized a new instance of the ControllerWMI class.
        /// </summary>
        /// <param name="computerName">The name of the computer to connect to.</param>
        /// <param name="namespace">The WMI Namespace containing the needed Class.</param>
        /// <param name="class">The WMI Class to control.</param>
        /// <param name="idField">The name of the identification field to be used.</param>
        public ControllerWMI(string computerName = null, string nameSpace = null, string idField = null)
        {
            this.ConnectionOptions = new ConnectionOptions
            {
                Impersonation = ImpersonationLevel.Impersonate,
                Authentication = AuthenticationLevel.PacketPrivacy,
                EnablePrivileges = true,
                Timeout = new TimeSpan(0, 0, 0, 5, 0)
            };
            this.Namespace = nameSpace;
            this.IdField = idField;
            this.ComputerName = computerName;
        }

        #region Public Methods

        /// <summary>
        /// Gets a list containing all records of the specified data object type.
        /// </summary>
        /// <returns>A list containing all records of the specified type.</returns>
        public List<T> GetAllRecords<T>() where T : new()
        {
            try
            {
                ManagementScope managementScope = GetManagementScope();

                string commandText = string.Format("SELECT {0} FROM {1}", this.Fields, this.Class);
                ManagementObjectCollection results = GetCommandResults(commandText, managementScope);

                List<T> returnList = new List<T>();
                if (results != null)
                {
                    foreach (ManagementBaseObject result in results)
                        returnList.Add(GetPopulatedObject<T>(result));
                }

                return returnList;
            }
            catch (Exception ex)
            {
                Global.LogEvent(string.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message));
            }

            return null;
        }

        #endregion

        #region Private Methods

        private ManagementScope GetManagementScope(string computerName = null, string nameSpace = null, bool specifyArchitecture = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(computerName)) computerName = this.ComputerName;
                if (string.IsNullOrWhiteSpace(nameSpace)) nameSpace = this.Namespace;

                ManagementScope managementScope = new ManagementScope();
                managementScope.Path.Server = computerName;
                managementScope.Path.NamespacePath = nameSpace;
                managementScope.Options = this.ConnectionOptions;

                if (specifyArchitecture)
                    managementScope.Options.Context.Add("__ProviderArchitecture", this.ProviderArchitecture);

                managementScope.Connect();

                return managementScope;
            }
            catch (Exception ex)
            {
                Global.LogEvent(string.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message));
            }

            return null;
        }

        private ManagementObjectCollection GetCommandResults(string commandText, ManagementScope managementScope)
        {
            try
            {
                ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(managementScope.Path.ToString(), commandText);
                return managementObjectSearcher.Get();
            }
            catch (Exception ex)
            {
                Global.LogEvent(string.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message));
            }

            return null;
        }

        private PropertyInfo[] GetPropertyInfos()
        {
            // Get all non shared/static properties of the generic object
            return this.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToArray();
        }

        private string GetFields()
        {
            // Concatenated fields for SELECT command
            return string.Join(", ", PropertyInfos.Select(p => string.Format("{0}", p.Name)).ToArray());
        }

        private T GetPopulatedObject<T>(ManagementBaseObject managementBaseObject) where T : new()
        {
            T @object = new T();
            foreach (PropertyInfo propertyInfo in this.PropertyInfos)
            {
                if (propertyInfo != null && propertyInfo.CanWrite)
                {
                    var value = managementBaseObject.GetPropertyValue(propertyInfo.Name);
                    propertyInfo.SetValue(@object, value);
                }
            }

            return @object;
        }

        private ProviderArchitectures GetProviderArchitecture(string computerName)
        {
            try
            {
                ManagementScope managementScope = GetManagementScope(computerName: computerName, nameSpace: Namespaces.cimv2, specifyArchitecture: false);
                string commandText = string.Format("SELECT {0} FROM {1}", nameof(Win32_Processor.AddressWidth), nameof(Win32_Processor));
                ManagementBaseObject result = GetCommandResults(commandText, managementScope).Cast<ManagementBaseObject>().ToArray()[0];

                string addressWidth = result.GetPropertyValue(nameof(Win32_Processor.AddressWidth)).ToString();
                if (! string.IsNullOrWhiteSpace(addressWidth))
                {
                    if (addressWidth == ProviderArchitectures.x64.ToString("D"))
                        return ProviderArchitectures.x64;
                    if (addressWidth == ProviderArchitectures.x86.ToString("D"))
                        return ProviderArchitectures.x86;
                }
            }
            catch (Exception ex)
            {
                Global.LogEvent(string.Format("EXCEPTION in {0}: {1}", MethodBase.GetCurrentMethod(), ex.Message));
            }

            return default(ProviderArchitectures);
        }

        private enum ProviderArchitectures
        {
            x86 = 32,
            x64 = 64
        }

        private enum Win32_Processor
        {
            AddressWidth
        }

        private struct Namespaces
        {
            public static readonly string cimv2 = @"\ROOT\cimv2";
            public static readonly string power = @"\ROOT\cimv2\power";
        }

        #endregion
    }
}