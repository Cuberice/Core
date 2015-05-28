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
	public static class AdapterExtensions
	{
		public static string CreateSelectCommandString<T>(this IDbAdapter adapter)
		{
			Table t = Table.Get(typeof(T));

			string scol = t.GetColumnString();
			return string.Format(adapter.SELECT(), scol, t.TableName, string.Empty);
		}

		public static IAdapterCommand CreateSelectCommand<T>(this IDbAdapter adapter)
		{
			Table t = Table.Get(typeof(T));
			string scol = t.GetColumnString();
			return adapter.CreateCommand(string.Format(adapter.SELECT(), scol, t.TableName, string.Empty));
		} 
		public static IAdapterCommand CreateSelectCommand<T>(this IDbAdapter adapter, Expression<Func<T, bool>> f)
		{
			Table t = Table.Get(typeof(T));

			ExpressionCommand exCmd = ExpressionExtensions.FromExpression(f);
			string swhere = "WHERE " + exCmd.GetCommandString(adapter);
			string scolumns = t.GetColumnString();

			IAdapterCommand ACmd = adapter.CreateCommand(string.Format(adapter.SELECT(), scolumns, t.TableName, swhere));
			exCmd.DetailList.ForEach(p => ACmd.AddParameter(p.Parameter.ParameterName, p.Parameter.ParameterValue));

			return ACmd;
		}		
		public static IAdapterCommand CreateSelectCommand<T>(this IDbAdapter adapter, Command cmd)
		{
			Table t = Table.Get(typeof(T));
			string swhere = "WHERE " + cmd.GetCommandString(adapter);
			string scolumns = t.GetColumnString();

			IAdapterCommand ACmd = adapter.CreateCommand(string.Format(adapter.SELECT(), scolumns, t.TableName, swhere));
			cmd.SetParameters(ACmd);

			//cmd.DetailList.ForEach(p => ACmd.AddParameter(p.Parameter.ParameterName, p.Parameter.ParameterValue));

			return ACmd;
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
				Column pk = t.PrimaryKey;

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
				string sset = fset.SetStringFromExpression<T>(adapter, obj, out parameters);
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

		#region Data Type Conversion

		public static string GetString(object value, string column)
		{
			return value.ToString();
		}

		public static bool GetBoolean(object value, string column)
		{
			if (value == null || value == DBNull.Value)
				return false;

			return Boolean.Parse(value.ToString());
		}

		public static int GetInt32(object value, string column)
		{
			if (value == null || value == DBNull.Value)
				return 0;
			
			return Int32.Parse(value.ToString());
		}

		public static int? GetNullableInt32(object value, string column)
		{
			if (value == null || value == DBNull.Value)
				return null;
			
			return Int32.Parse(value.ToString());
		}

		public static DateTime? GetDateTime(object value)
		{
			try
			{
				string s =value.ToString();
				if (s == string.Empty)
					return null;

				return DateTime.Parse(s);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return DateTime.MinValue;
			}
		}

		#endregion
	}
}
