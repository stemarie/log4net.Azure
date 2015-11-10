using System;
using System.Collections.Generic;
using System.Dynamic;
using log4net.Appender.Language;
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

		private static EntityProperty GetEntityProperty(string key, object value)
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
			// ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
			if (value is string) return new EntityProperty((string)value);
			throw new Exception(string.Format(Resources.ElasticTableEntity_GetEntityProperty_not_supported__0__for__1_,
				value.GetType(), key));
		}
	}
}