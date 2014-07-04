using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Core.Data
{
	public static class ExpressionExtensions
	{
		public static ExpressionCommand FromExpression<T>(Expression<Func<T, bool>> ex)
		{
			Expression body = ex.Body;
			ExpressionCommand command = new ExpressionCommand();
			command.Create(ex);

			if (body is BinaryExpression)
				FromBinaryExpression<T>(body as BinaryExpression, command);

			if (body is MemberExpression)
				FromMemberExpression<T>(body as MemberExpression, command);

			if (body is UnaryExpression)
				FromUnaryExpression<T>(body as UnaryExpression, command);

			if (body is LambdaExpression)
				PropertyFromLambdaExpression<T>(body);

			return command;
		}

		public static void FromBinaryExpression<T>(BinaryExpression binEx, ExpressionCommand command)
		{
			if (IsComplexExpression(binEx))
			{
				FromBinaryExpression<T>(binEx.Left as BinaryExpression, command);
				FromBinaryExpression<T>(binEx.Right as BinaryExpression, command);
			}
			else
			{
				PropertyInfo pi = PropertyFromBinaryExpression<T>(binEx);
				object value = GetValueFromExpression<T>(binEx.Right);

				command.AddExpression(binEx, pi, value);
			}
		}
		private static void FromMemberExpression<T>(MemberExpression memEx, ExpressionCommand command)
		{
			PropertyInfo pi = PropertyFromMemberExpression<T>(memEx);

			if (memEx.Type == typeof(bool))
			{

			}
		}
		private static void FromUnaryExpression<T>(UnaryExpression ex, ExpressionCommand command)
		{
			PropertyInfo pi = PropertyFromUnaryExpression<T>(ex);
		}

		private static PropertyInfo PropertyFromBinaryExpression<T>(BinaryExpression ex)
		{
			Expression lex = ex.Left;

			if (lex is MemberExpression)
			{
				MemberExpression member = lex as MemberExpression;
				while (member.NotNull() && member.Expression.NotNull() && member.Expression.Type != typeof(T))
				{
					member = member.Expression as MemberExpression;
				}
				return PropertyFromMemberExpression<T>(member);
			}

			if (lex is UnaryExpression)
			{
				UnaryExpression unary = lex as UnaryExpression;
				return PropertyFromMemberExpression<T>(unary.Operand as MemberExpression);
			}

			if (lex is LambdaExpression)
			{
				MessageBox.Show("Lambda Expression...");
			}

			throw new Exception("PropertyInfo Not Found from Binary Expression");
		}
		private static PropertyInfo PropertyFromMemberExpression<T>(MemberExpression ex)
		{
			return ex.Member as PropertyInfo;
		}
		private static PropertyInfo PropertyFromUnaryExpression<T>(UnaryExpression ex)
		{
			return PropertyFromMemberExpression<T>(ex.Operand as MemberExpression);
		}
		public static PropertyInfo PropertyFromLambdaExpression<T>(Expression ex)
		{
			if (ex is LambdaExpression)
			{
				LambdaExpression l = ex as LambdaExpression;

				if (l.Body is UnaryExpression)
				{
					return PropertyFromUnaryExpression<T>(l.Body as UnaryExpression);
				}
			}

			throw new Exception("PropertyInfo Not Found from Lambda Expression");
		}

		private static object GetValueFromExpression<T>(Expression ex)
		{
			if (ex.NodeType == ExpressionType.Constant)
			{
				ConstantExpression c = ex as ConstantExpression;
				return c.Value;
			}
			if (ex.NodeType == ExpressionType.MemberAccess || ex.NodeType == ExpressionType.Call)
			{
				LambdaExpression l = Expression.Lambda(ex, null);
				return l.Compile().DynamicInvoke();
			}
			if (ex is LambdaExpression)
			{
				MessageBox.Show("Lambda Expression...");
			}

			throw new Exception("Value Not Found from Expression");
		}
		public static bool IsComplexExpression(BinaryExpression binEx)
		{
			List<ExpressionType> logic = new List<ExpressionType>() { ExpressionType.Equal, ExpressionType.GreaterThan, ExpressionType.GreaterThanOrEqual, ExpressionType.LessThan, ExpressionType.LessThanOrEqual };
			List<ExpressionType> combine = new List<ExpressionType>() { ExpressionType.AndAlso, ExpressionType.OrElse };

			return combine.Contains(binEx.NodeType);
		}

		public static string LogicStringFromExpression<T>(this Expression ex, IDbAdapter adapter)
		{
			List<ExpressionType> compareNode = new List<ExpressionType>() { ExpressionType.Equal, ExpressionType.GreaterThan, ExpressionType.GreaterThanOrEqual, ExpressionType.LessThan, ExpressionType.LessThanOrEqual };

			if (ex is BinaryExpression)
			{
				BinaryExpression bin = ex as BinaryExpression;

				if (compareNode.Contains(bin.NodeType))
				{
					MemberExpression member = null;

					if (bin.Left is MemberExpression)
					{
						member = bin.Left as MemberExpression;
						while (member.Expression != null && member.Expression.Type != typeof(T))
						{
							member = member.Expression as MemberExpression;
						}
					}
					else if (bin.Left is UnaryExpression)
					{
						UnaryExpression unary = bin.Left as UnaryExpression;
						member = unary.Operand as MemberExpression;
					}

					var value = GetValueFromExpression<T>(bin.Right);

					PropertyInfo property = member.Member as PropertyInfo;
					Table table = Table.Get(property.DeclaringType);
					Column column = Column.Get(property);

					if (column.ColumnSystemType != bin.Right.Type)
						throw new Exception(string.Format("{0}.{1} - [{2}] and Value [{3}] DataTypes does not match for query ", table.TableName, column.Name, column.ColumnSystemType.Name, bin.Right.Type.Name));

					return string.Format("{0} {2} {1}", column.Name, value, adapter.OPERATOR(bin.NodeType));
				}

				return string.Format("{0} {2} {1}", bin.Left.LogicStringFromExpression<T>(adapter), bin.Right.LogicStringFromExpression<T>(adapter), adapter.OPERATOR(bin.NodeType));
			}

			return "";
		}
		public static string SetStringFromExpression<T>(this Expression ex, IDbAdapter adapter, object obj, out List<Parameter> parameters)
		{
			parameters = new List<Parameter>();
			PropertyInfo pi = PropertyFromLambdaExpression<T>(ex);
			Column column = Column.Get(pi);

			if (column.IsNull())
				return string.Empty;

			object value = column.GetColumnValue(obj);
			parameters.Add(new Parameter() { Name = column.Name, Value = value });
			return string.Format("{0} = @param", column.Name); //NOt sure about this

			return string.Format("{0} = {1}", column.Name, value);
		}
	}
}
