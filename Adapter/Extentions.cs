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

		public static bool IsNull(this Attribute attr)
		{
			return attr == null;
		}		
		public static bool IsNull(this Expression expression)
		{
			return expression == null;
		}
	}
}
