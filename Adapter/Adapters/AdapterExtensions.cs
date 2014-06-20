using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Core.Data;

namespace Core.Adapters
{
	public static class AdapterExtensions
	{
		public static string CreateSelectCommand<T>(this IDbAdapter adapter)
		{
			Table t = Table.Get(typeof(T));
			List<Column> columns = Column.GetAll(typeof(T));

			string scol = t.Columns.Aggregate(string.Empty, (current, c) => current + string.Format("{0},", c.Name)).TrimEnd(',');
			return string.Format(adapter.SELECT(), scol, t.TableName, string.Empty);
		}
		public static string CreateSelectCommand<T>(this IDbAdapter adapter, Expression<Func<T, bool>> f)
		{
			Table t = Table.Get(typeof(T));
			List<Column> columns = Column.GetAll(typeof(T));

			string swhere = "WHERE " + f.Body.LogicStringFromExpression<T>(adapter);
			string scolumns = t.Columns.Aggregate(string.Empty, (current, c) => current + string.Format("{0},", c.Name)).TrimEnd(',');
			return string.Format(adapter.SELECT(), scolumns, t.TableName, swhere);
		}

		public static IAdapterCommand CreateInsertCommand<T>(this IDbAdapter adapter, T obj)
		{
			try
			{
				Table t = Table.Get(typeof(T));
				List<Column> columns = Column.GetAll(typeof(T));

				string scol = t.Columns.Aggregate(string.Empty, (current, c) => current + string.Format("{0},", c.Name)).TrimEnd(',');
				string sval = t.Columns.Aggregate(string.Empty, (current, c) => current + string.Format("@{0},", c.Name.ToLower())).TrimEnd(',');
				IAdapterCommand cmd = adapter.CreateCommand(string.Format(adapter.INSERTINTO(), t.TableName, scol, sval));

				foreach (Column c in columns)
				{
					cmd.AddParameter(string.Format("@{0}", c.Name.ToLower()), c.GetColumnValue(obj));
				}

				return cmd;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}

		public static IAdapterCommand CreateUpdateCommand<T>(this IDbAdapter adapter, T obj)
		{
			try
			{
				Table t = Table.Get(typeof(T));
				Column pk = t.GetPrimaryKey();

				string swhere = string.Format("{0} = @pk_val", pk.Name);
				string sset = t.Columns.Where(c => !c.PrimaryKey).Aggregate(string.Empty, (current, c) => current + string.Format("{0} = @p_{1},", c.Name, c.Name.ToLower())).TrimEnd(',');
				IAdapterCommand cmd = adapter.CreateCommand(string.Format(adapter.UPDATE(), t.TableName, sset, swhere));

				cmd.AddParameter("@pk_val", pk.GetColumnValue(obj));
				foreach (Column c in t.Columns)
				{
					cmd.AddParameter(string.Format("@p_{0}", c.Name.ToLower()), c.GetColumnValue(obj));
				}

				return cmd;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}
		public static IAdapterCommand CreateUpdateCommand<T>(this IDbAdapter adapter, T obj, Expression<Func<T, object>> fset, Expression<Func<T, bool>> fwhere)
		{
			try
			{
				List<Parameter> parameters;
				Table t = Table.Get(typeof(T));

				string swhere = "WHERE " + fwhere.Body.LogicStringFromExpression<T>(adapter);
				string sset = fset.SetStringFromExpression(adapter, obj, out parameters);
				IAdapterCommand cmd = adapter.CreateCommand(string.Format(adapter.UPDATE(), t.TableName, sset, swhere));

				cmd.AddParameter(parameters);

				return cmd;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
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
		public static string SetStringFromExpression(this Expression ex, IDbAdapter adapter, object obj, out List<Parameter> parameters)
		{
			parameters = new List<Parameter>();
			PropertyInfo pi = PropertyFromExpression(ex);
			Column column = Column.Get(pi);

			if (column.IsNull())
				return string.Empty;

			object value = column.GetColumnValue(obj);
			parameters.Add(new Parameter() { Name = "@" + column.Name, Value = value });
			return string.Format("{0} = @param", column.Name);

			return string.Format("{0} = {1}", column.Name, value);
		}
		public static PropertyInfo PropertyFromExpression(Expression ex)
		{
			if (ex is LambdaExpression)
			{
				LambdaExpression l = ex as LambdaExpression;

				if (l.Body is UnaryExpression)
				{
					UnaryExpression unary = l.Body as UnaryExpression;
					MemberExpression member = unary.Operand as MemberExpression;

					if (member.IsNull())
						throw new ArgumentException();

					return member.Member as PropertyInfo;
				}
			}

			return null;
		}
		private static object GetValueFromExpression<T>(Expression ex)
		{
			object value = null;

			if (ex.NodeType == ExpressionType.Constant)
			{
				return ex;
			}
			if (ex.NodeType == ExpressionType.MemberAccess)
			{
				LambdaExpression l = Expression.Lambda(ex, null);
				return l.Compile().DynamicInvoke();

				//						var objectMember = Expression.Convert(valueEx, typeof(object));
				//						var getterLambda = Expression.Lambda<Func<object>>(objectMember);
				//						var getter = getterLambda.Compile();
				//						value = getter();
			}
			if (ex is LambdaExpression)
			{

			}

			return value;
		}
	}
}
