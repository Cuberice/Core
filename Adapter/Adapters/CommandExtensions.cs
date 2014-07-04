using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Core.Data;

namespace Core.Data
{
	#region Interface

	public interface ICommand
	{
		List<ICommandDetail> ParameterList { get; set; }
		string GetCommandString(IDbAdapter adapter);
	}

	public interface ICommandDetail
	{
		Column Column { get; set; }
		string ParameterName { get; set; }
		object ParameterValue { get; set; }
		ExpressionType Logic { get; set; }
		string GetParameterExpression(IDbAdapter adapter);
	}

	#endregion

	#region Command

	public class Command : ICommand
	{
		public List<ICommandDetail> ParameterList { get; set; }

		public void AddExpression(Column column, ExpressionType logic, object value)
		{
			if (ParameterList.IsNull())
				ParameterList = new List<ICommandDetail>();

			ParameterList.Add(CommandDetail.Create(column, logic, value));
		}

		public string GetCommandString(IDbAdapter adapter)
		{
			return ParameterList.Aggregate(string.Empty, (current, d) => current + d.GetParameterExpression(adapter));
		}
	}

	public struct CommandDetail : ICommandDetail
	{
		public Column Column { get; set; }
		public string ParameterName { get; set; }
		public object ParameterValue { get; set; }
		public ExpressionType Logic { get; set; }

		public static CommandDetail Create(Column column, ExpressionType logic, object value)
		{
			CommandDetail d = new CommandDetail()
			{
				Column = column,
				Logic = logic,
				ParameterValue = value
			};

			d.ParameterName = "@" + d.Column.Name.ToLower();
			return d;
		}

		public string GetParameterExpression(IDbAdapter adapter)
		{
			return string.Format("{0} {2} {1}", Column.Name, ParameterName, adapter.OPERATOR(Logic));
		}
	}

	#endregion

	#region Expression Command

	public class ExpressionCommand : ICommand
	{
		protected string ExpressionString { get; set; }
		public List<ICommandDetail> ParameterList { get; set; }

		public void Create<T>(Expression<Func<T, bool>> ex)
		{
			ExpressionString = GetExpressionString(ex);
		}

		public void AddExpression(Expression ex, PropertyInfo pi, object value)
		{
			if (ParameterList.IsNull())
				ParameterList = new List<ICommandDetail>();

			ExpressionCommandDetail d = ExpressionCommandDetail.Create(ex, pi, value);
			ParameterList.Add(d);
		}

		protected string GetExpressionString(Expression ex)
		{
			const string f = "=> ";
			string s = ex.ToString();
			return s.Substring(s.IndexOf(f, StringComparison.Ordinal) + f.Length);
		}

		public string GetCommandString(IDbAdapter adapter)
		{
			return ParameterList.Cast<ExpressionCommandDetail>()
				.Aggregate(ExpressionString, (current, d) => current.Replace(d.ExpressionString, d.GetParameterExpression(adapter)));
		}
	}

	public struct ExpressionCommandDetail : ICommandDetail
	{
		public Column Column { get; set; }
		public string ExpressionString { get; set; }
		public string ParameterName { get; set; }
		public object ParameterValue { get; set; }
		public ExpressionType Logic { get; set; }

		public static ExpressionCommandDetail Create(Expression ex, PropertyInfo pi, object value)
		{
			ExpressionCommandDetail d = new ExpressionCommandDetail
			{
				ExpressionString = ex.ToString(),
				ParameterValue = value,
				Logic = ex.NodeType,
				Column = Column.Get(pi)
			};

			d.ParameterName = "@" + d.Column.Name.ToLower();
			return d;
		}

		public string GetParameterExpression(IDbAdapter adapter)
		{
			return string.Format("{0} {2} {1}", Column.Name, ParameterName, adapter.OPERATOR(Logic));
		}
	}

	#endregion
}
