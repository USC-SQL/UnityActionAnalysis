using System;
using System.Globalization;
using System.Reflection;

namespace Tiny {

	public static class StringExtensions {
		public static string SnakeCaseToCamelCase(string snakeCaseName) {
			var x = snakeCaseName.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
			string r = "";
			foreach (var s in x)
            {
				r += char.ToUpperInvariant(s[0]) + s.Substring(1, s.Length - 1);
			}
			return r;
		}

		public static string CamelCaseToSnakeCase(string camelCaseName) {
			string s = "";
			int i = 0;
			foreach (var x in camelCaseName)
            {
				if (i > 0 && char.IsUpper(x))
                {
					s += "_" + x.ToString();
                } else
                {
					s += x.ToString();
                }
				++i;
            }
			return s.ToLower(CultureInfo.InvariantCulture);
		}
	}

	public static class TypeExtensions {
		public static bool IsInstanceOfGenericType(Type type, Type genericType) {
			while (type != null) {
				if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType) return true;
				type = type.BaseType;
			}
			return false;
		}

		public static bool HasGenericInterface(Type type, Type genericInterface) {
			if (genericInterface == null) throw new ArgumentNullException();
			var interfaceTest = new Predicate<Type>(i => i.IsGenericType && i.GetGenericTypeDefinition().IsAssignableFrom(genericInterface));
			if (interfaceTest(type))
            {
				return true;
            }

			foreach (var i in type.GetInterfaces())
            {
				if (interfaceTest(i))
                {
					return true;
                }
            }

			return false;
		}

		static string UnwrapFieldName(string name) {
			if (name.StartsWith("<", StringComparison.Ordinal) && name.Contains(">")) {
				return name.Substring(name.IndexOf("<", StringComparison.Ordinal) + 1, name.IndexOf(">", StringComparison.Ordinal) - 1);
			}
			return name;
		}

		public static string UnwrappedFieldName(FieldInfo field, Type type, bool convertSnakeCase) {
			string name = UnwrapFieldName(field.Name);

			/*if (field.GetCustomAttributes(typeof(JsonPropertyAttribute), true).Length == 1) {
				var jsonProperty = field.GetCustomAttributes(typeof(JsonPropertyAttribute), true)[0] as JsonPropertyAttribute;
				name = jsonProperty.Name;
			} else {
				foreach (var property in type.GetProperties()) {
					if (UnwrapFieldName(property.Name).Equals(name, StringComparison.OrdinalIgnoreCase)) {
						name = UnwrappedPropertyName(property);
						break;
					}
				}
			}

			return convertSnakeCase ? StringExtensions.SnakeCaseToCamelCase(name) : name;*/
			return name;
		}

		public static string UnwrappedPropertyName(PropertyInfo property) {
			string name = UnwrapFieldName(property.Name);

			if (property.GetCustomAttributes(typeof(JsonPropertyAttribute), true).Length == 1) {
				var jsonProperty = property.GetCustomAttributes(typeof(JsonPropertyAttribute), true)[0] as JsonPropertyAttribute;
				name = jsonProperty.Name;
			}

			return name;
		}

		public static bool MatchFieldName(FieldInfo field, String name, Type type, bool matchSnakeCase) {
			string fieldName = UnwrappedFieldName(field, type, matchSnakeCase);
			if (matchSnakeCase) {
				name = StringExtensions.SnakeCaseToCamelCase(name);
			}

			return name.Equals(fieldName, StringComparison.CurrentCultureIgnoreCase);
		}
	}

	public static class JsonExtensions {
		public static bool IsNullable(Type type) {
			// Nullable not supported by Temple Runner's Mono
			return false; // Nullable.GetUnderlyingType(type) != null || !type.IsPrimitive;
		}

		public static bool IsNumeric(Type type) {
			if (type.IsEnum) return false;
			switch (Type.GetTypeCode(type)) {
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.Single:
					return true;
				case TypeCode.Object:
					// Nullable not supported by Temple Runner's Mono
					//Type underlyingType = Nullable.GetUnderlyingType(type);
					//return underlyingType != null && IsNumeric(underlyingType);
					return false;
				default:
					return false;
			}
		}

		public static bool IsFloatingPoint(Type type) {
			if (type.IsEnum) return false;
			switch (Type.GetTypeCode(type)) {
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.Single:
					return true;
				case TypeCode.Object:
					// Nullable not supported by Temple Runner's Mono
					//Type underlyingType = Nullable.GetUnderlyingType(type);
					//return underlyingType != null && IsFloatingPoint(underlyingType);
					return false;
				default:
					return false;
			}
		}
	}

	public static class StringBuilderExtensions {
		public static void Clear(System.Text.StringBuilder sb) {
			sb.Length = 0;
		}
	}
}
