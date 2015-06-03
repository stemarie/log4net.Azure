using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace log4net.Appender
{
    /// <summary>
    /// copied from here:
    /// http://pascallaurin42.blogspot.de/2013/03/using-azure-table-storage-with-dynamic.html
    /// </summary>
    public class ElasticTableEntity : DynamicObject, ITableEntity//, ICustomMemberProvider // For LinqPad's Dump
    {
        public ElasticTableEntity()
        {
            Properties = new Dictionary<string, EntityProperty>();
        }

        public IDictionary<string, EntityProperty> Properties { get; private set; }

        public object this[string key]
        {
            get
            {
                if (!Properties.ContainsKey(key))
                    Properties.Add(key, GetEntityProperty(key, null));

                return Properties[key];
            }
            set
            {
                var property = GetEntityProperty(key, value);

                if (Properties.ContainsKey(key))
                    Properties[key] = property;
                else
                    Properties.Add(key, property);
            }
        }

        #region DynamicObject overrides

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this[binder.Name];
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            this[binder.Name] = value;
            return true;
        }

        #endregion

        #region ITableEntity implementation

        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public string ETag { get; set; }

        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            Properties = properties;
        }

        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            return Properties;
        }

        #endregion

        private EntityProperty GetEntityProperty(string key, object value)
        {
            if (value == null) return new EntityProperty((string)null);
            if (value.GetType() == typeof(byte[])) return new EntityProperty((byte[])value);
            if (value is bool) return new EntityProperty((bool)value);
            if (value is DateTimeOffset) return new EntityProperty((DateTimeOffset)value);
            if (value is DateTime) return new EntityProperty((DateTime)value);
            if (value is double) return new EntityProperty((double)value);
            if (value is Guid) return new EntityProperty((Guid)value);
            if (value is int) return new EntityProperty((int)value);
            if (value is long) return new EntityProperty((long)value);
            if (value is string) return new EntityProperty((string)value);
            throw new Exception("not supported " + value.GetType() + " for " + key);
        }

        private Type GetType(EdmType edmType)
        {
            switch (edmType)
            {
                case EdmType.Binary: return typeof(byte[]);
                case EdmType.Boolean: return typeof(bool);
                case EdmType.DateTime: return typeof(DateTime);
                case EdmType.Double: return typeof(double);
                case EdmType.Guid: return typeof(Guid);
                case EdmType.Int32: return typeof(int);
                case EdmType.Int64: return typeof(long);
                case EdmType.String: return typeof(string);
                default: throw new Exception("not supported " + edmType);
            }
        }

        private object GetValue(EntityProperty property)
        {
            switch (property.PropertyType)
            {
                case EdmType.Binary: return property.BinaryValue;
                case EdmType.Boolean: return property.BooleanValue;
                case EdmType.DateTime: return property.DateTimeOffsetValue;
                case EdmType.Double: return property.DoubleValue;
                case EdmType.Guid: return property.GuidValue;
                case EdmType.Int32: return property.Int32Value;
                case EdmType.Int64: return property.Int64Value;
                case EdmType.String: return property.StringValue;
                default: throw new Exception("not supported " + property.PropertyType);
            }
        }
    }
}