using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Core.Data;

namespace Core
{
	public static class Extentions
	{
		public static string GetRandomString(int length)
		{
			string chars = Guid.NewGuid().ToString();
			var random = new Random();
			return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
		}
		public static DateTime GetRandomDate()
		{
			return DateTime.Now.AddDays(new Random().Next(100));
		} 	
		public static int GetRandomInt()
		{
			return new Random().Next();
		}   	
		public static bool GetRandomBool()
		{
			return Convert.ToBoolean(new Random().Next(0, 2));
		}  

		public static bool IsNull(this Attribute attr)
		{
			return attr == null;
		}		
		public static bool IsNull(this Expression expression)
		{
			return expression == null;
		}
		public static bool IsNull(this Object obj)
		{
			return obj == null;
		}
		public static bool IsNullOrDbNull(this Object obj)
		{
			return obj == null || obj == DBNull.Value;
		}		 
		
		public static bool NotNull(this Object obj)
		{
			return obj != null;
		}

		public static bool IsNullableType(this Type t)
		{
			return t.IsGenericType && t.GetGenericTypeDefinition() == typeof (Nullable<>);
		}
		public static object GetNullableValue(this Type t, object value)
		{
			return Convert.ChangeType(value, Nullable.GetUnderlyingType(t));
		}
	}
}
