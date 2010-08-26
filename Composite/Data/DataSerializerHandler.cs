﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Composite.Core.Serialization;
using Composite.Core.Types;


namespace Composite.Data
{
    internal sealed class DataSerilizationException : Exception
    {
        public DataSerilizationException(string message)
            : base(message)
        {
        }
    }



    internal sealed class DataSerializerHandler : ISerializerHandler
    {
        public string Serialize(object objectToSerialize)
        {
            if (objectToSerialize == null) throw new ArgumentNullException("objectToSerialize");

            IData data = objectToSerialize as IData;
            if (data == null) throw new ArgumentException("data");

            StringBuilder sb = new StringBuilder();

            if (data.DataSourceId.ExistsInStore == false)
            {
                StringConversionServices.SerializeKeyValuePair(sb, "_IsNew_", true);
                StringConversionServices.SerializeKeyValuePair(sb, "_Type_", TypeManager.SerializeType(data.DataSourceId.InterfaceType));
            }
            else
            {
                StringConversionServices.SerializeKeyValuePair(sb, "_IsNew_", false);
                StringConversionServices.SerializeKeyValuePair(sb, "_DataSourceId_", data.DataSourceId.Serialize());
            }

            IEnumerable<PropertyInfo> propertyInfos =
                from prop in data.DataSourceId.InterfaceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                where prop.CanRead == true && prop.CanWrite == true
                select prop;

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                MethodInfo methodInfo =
                        (from mi in typeof(StringConversionServices).GetMethods(BindingFlags.Public | BindingFlags.Static)
                         where mi.Name == "SerializeKeyValuePair" &&
                               mi.IsGenericMethodDefinition == true &&
                               mi.GetParameters().Length == 3 &&
                               mi.GetParameters()[2].ParameterType.IsGenericParameter == true
                         select mi).SingleOrDefault();
                
                methodInfo = methodInfo.MakeGenericMethod(new Type[] { propertyInfo.PropertyType });

                object propertyValue = propertyInfo.GetValue(data, null);

                methodInfo.Invoke(null, new object[] { sb, propertyInfo.Name, propertyValue });
            }

            return sb.ToString();
        }



        public object Deserialize(string serializedObject)
        {
            Dictionary<string, string> dic = StringConversionServices.ParseKeyValueCollection(serializedObject);

            if (dic.ContainsKey("_IsNew_") == false) throw new ArgumentException("serializedObject is of wrong format");

            IData data = null;

            bool isNew = StringConversionServices.DeserializeValueBool(dic["_IsNew_"]);
            if (isNew == true)
            {
                if (dic.ContainsKey("_Type_") == false) throw new ArgumentException("serializedObject is of wrong format");

                string typeString = StringConversionServices.DeserializeValueString(dic["_Type_"]);
                Type interfaceType = TypeManager.GetType(typeString);

                data = DataFacade.BuildNew(interfaceType);
            }
            else
            {
                if (dic.ContainsKey("_DataSourceId_") == false) throw new ArgumentException("serializedObject is of wrong format");

                string dataSourceIdString = StringConversionServices.DeserializeValueString(dic["_DataSourceId_"]);
                DataSourceId dataSourceId = DataSourceId.Deserialize(dataSourceIdString);

                data = DataFacade.GetDataFromDataSourceId(dataSourceId);

                if (data == null) throw new DataSerilizationException(string.Format("Failed to get the '{0}' with the given data source '{1}', data might have been deleted sinse this serialized data was created", dataSourceId.InterfaceType, dataSourceId));
            }
            

            IEnumerable<PropertyInfo> propertyInfos =
                from prop in data.DataSourceId.InterfaceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                where prop.CanRead == true && prop.CanWrite == true
                select prop;

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                if (dic.ContainsKey(propertyInfo.Name) == false) throw new DataSerilizationException(string.Format("The data type '{0}' does not contain a property named '{1}', type might have changed sinse this serialized data was created", data.DataSourceId.InterfaceType, propertyInfo.Name));

                MethodInfo methodInfo =
                        (from mi in typeof(StringConversionServices).GetMethods(BindingFlags.Public | BindingFlags.Static)
                         where mi.Name == "DeserializeValue" &&
                               mi.IsGenericMethodDefinition == true &&
                               mi.GetParameters().Length == 2 &&
                               mi.GetParameters()[1].ParameterType.IsGenericParameter == true
                         select mi).SingleOrDefault();

                object defaultValue;
                if (propertyInfo.PropertyType == typeof(Guid)) defaultValue = default(Guid);
                else if (propertyInfo.PropertyType == typeof(string)) defaultValue = default(string);
                else if (propertyInfo.PropertyType == typeof(int)) defaultValue = default(int);
                else if (propertyInfo.PropertyType == typeof(DateTime)) defaultValue = default(DateTime);
                else if (propertyInfo.PropertyType == typeof(bool)) defaultValue = default(bool);
                else if (propertyInfo.PropertyType == typeof(decimal)) defaultValue = default(decimal);
                else if (propertyInfo.PropertyType == typeof(long)) defaultValue = default(long);
                else defaultValue = null;

                methodInfo = methodInfo.MakeGenericMethod(new Type[] { propertyInfo.PropertyType });


              /*  string methodName;

                if (propertyInfo.PropertyType == typeof(Guid)) methodName = "DeserializeValueGuid";
                else if (propertyInfo.PropertyType == typeof(string)) methodName = "DeserializeValueString";
                else if (propertyInfo.PropertyType == typeof(int)) methodName = "DeserializeValueInt";
                else if (propertyInfo.PropertyType == typeof(DateTime)) methodName = "DeserializeValueDateTime";
                else if (propertyInfo.PropertyType == typeof(DateTime?)) methodName = "DeserializeValueDateTimeNullable";                    
                else if (propertyInfo.PropertyType == typeof(bool)) methodName = "DeserializeValueBool";
                else if (propertyInfo.PropertyType == typeof(decimal)) methodName = "DeserializeValueDecimal";
                else if (propertyInfo.PropertyType == typeof(long)) methodName = "DeserializeValueLong";
                else methodName = null;

                if (methodName == null) throw new InvalidOperationException(string.Format("StringConversionServices does not support the type '{0}'", propertyInfo.PropertyType));
                */

                object propertyValue = methodInfo.Invoke(null, new object[] { dic[propertyInfo.Name], defaultValue });

                propertyInfo.SetValue(data, propertyValue, null);
            }

            return data;
        }
    }
}
