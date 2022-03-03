using System;
using System.Collections;
using System.Collections.Generic;

namespace Tiny {
	public static class JsonMapper
	{

		internal static Encoder genericEncoder;
		internal static IDictionary<Type, Encoder> encoders = new Dictionary<Type, Encoder>();

		static JsonMapper()
		{
			// Register default encoder
			RegisterEncoder(typeof(object), DefaultEncoder.GenericEncoder());
			RegisterEncoder(typeof(IDictionary), DefaultEncoder.DictionaryEncoder());
			RegisterEncoder(typeof(IEnumerable), DefaultEncoder.EnumerableEncoder());
			RegisterEncoder(typeof(DateTime), DefaultEncoder.ZuluDateEncoder());
		}

		public static void RegisterEncoder(Type type, Encoder encoder)
		{
			if (type == typeof(object))
			{
				genericEncoder = encoder;
			}
			else
			{
				encoders.Add(type, encoder);
			}
		}

		public static Encoder GetEncoder(Type type)
		{
			if (encoders.ContainsKey(type))
			{
				return encoders[type];
			}
			foreach (var entry in encoders)
			{
				Type baseType = entry.Key;
				if (baseType.IsAssignableFrom(type))
				{
					return entry.Value;
				}
				if (TypeExtensions.HasGenericInterface(baseType, type))
				{
					return entry.Value;
				}
			}
			return genericEncoder;
		}

		public static void EncodeValue(object value, JsonBuilder builder)
		{
			if (JsonBuilder.IsSupported(value))
			{
				builder.AppendValue(value);
			}
			else
			{
				Encoder encoder = GetEncoder(value.GetType());
				if (encoder != null)
				{
					encoder(value, builder);
				}
				else
				{
					Console.WriteLine("Encoder for " + value.GetType() + " not found");
				}
			}
		}

		public static void EncodeNameValue(string name, object value, JsonBuilder builder)
		{
			builder.AppendName(name);
			EncodeValue(value, builder);
		}

		static object ConvertValue(object value, Type type)
		{
			if (value != null)
			{
				// Nullable not supported by Temple Runner's Mono
				Type safeType = type; // Nullable.GetUnderlyingType(type) ?? type;
				if (!type.IsEnum)
				{
					return Convert.ChangeType(value, safeType);
				}
				else
				{
					if (value is string)
					{
						return Enum.Parse(type, (string)value);
					}
					else
					{
						return Enum.ToObject(type, value);
					}
				}
			}
			return value;
		}
	}
}

