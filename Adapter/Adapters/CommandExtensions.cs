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
		List<ICommandDetail> DetailList { get; set; }
		string GetCommandString(IDbAdapter adapter);
	}

	public interface ICommandDetail
	{
		Column Column { get; set; }
		CommandParameter Parameter { get; set; }
		List<CommandParameter> Parameters { get; set; }
		OperatorType Logic { get; set; }
		string GetParameterExpression(IDbAdapter adapter);
	}

	#endregion

	#region Command

	public class Command : ICommand
	{
		public List<ICommandDetail> DetailList { get; set; }

		public void AddExpression(Column column, OperatorType logic, object value)
		{
			if (DetailList.IsNull())
				DetailList = new List<ICommandDetail>();

			DetailList.Add(CommandDetail.Create(column, logic, value)); //ID >= 123 --> ID >= @id
		}

		public string GetCommandString(IDbAdapter adapter)
		{
			return DetailList.Aggregate(string.Empty, (current, d) => current + d.GetParameterExpression(adapter));
		}

		public void SetParameters(IAdapterCommand aCmd)
		{
			foreach (ICommandDetail d in DetailList)
			{
				if (d.Parameters.Any())
				{
					d.Parameters.ForEach(p => aCmd.AddParameter(p.ParameterName, p.ParameterValue));
				}
			}
		}
	}

	public struct CommandDetail : ICommandDetail
	{
		public Column Column { get; set; }
		public CommandParameter Parameter { get { return Parameters.First(); } set{} }
		public List<CommandParameter> Parameters { get; set; }
		public OperatorType Logic { get; set; }

		public static CommandDetail Create(Column column, OperatorType logic, object value)
		{
			CommandDetail d = new CommandDetail()
			{
				Column = column,
				Logic = logic,
				Parameters = new List<CommandParameter>()
			};

			if (logic == OperatorType.In)
			{
				List<object> values =(List<object>)value;
				values.ForEach(v => d.Parameters.Add(CommandParameter.Create(d.Column, v)));
			}
			else
			{
				d.Parameters.Add(CommandParameter.Create(d.Column, value));
			}
			
			return d;
		}

		public string GetParameterExpression(IDbAdapter adapter)
		{
			if (!Parameters.Any())
				throw new ArgumentNullException("No Command Expressions Added");
				
				//{
				string paramNames = Parameters.Aggregate(string.Empty, (current, p) => current + string.Format("{0}, ", p.ParameterName)).TrimEnd(' ').TrimEnd(',');
				return string.Format("{0} {2} ({1})", Column.Name, paramNames , adapter.OPERATOR(Logic));
			//}
			//else
			//	return string.Format("{0} {2} {1}", Column.Name, Parameter.ParameterName, adapter.OPERATOR(Logic));
		}
	}

	public struct CommandParameter
	{
		public string ParameterName { get; set; }
		public object ParameterValue { get; set; }
		
		public static CommandParameter Create(Column column, object value)
		{
			CommandParameter p = new CommandParameter()
			{
				ParameterName = "@" + column.Name.ToLower() + new Random().Next(),
				ParameterValue = value,
			};

			return p;
		}
	}

	#endregion

	#region Expression Command

	public class ExpressionCommand : ICommand
	{
		protected string ExpressionString { get; set; }
		public List<ICommandDetail> DetailList { get; set; }

		public void Create<T>(Expression<Func<T, bool>> ex)
		{
			ExpressionString = GetExpressionString(ex);
		}

		public void AddExpression(Expression ex, PropertyInfo pi, object value)
		{
			if (DetailList.IsNull())
				DetailList = new List<ICommandDetail>();

			ExpressionCommandDetail d = ExpressionCommandDetail.Create(ex, pi, value);
			DetailList.Add(d);
		}

		protected string GetExpressionString(Expression ex)
		{
			const string f = "=> ";
			string s = ex.ToString();
			return s.Substring(s.IndexOf(f, StringComparison.Ordinal) + f.Length);
		}

		public string GetCommandString(IDbAdapter adapter)
		{
			return DetailList.Cast<ExpressionCommandDetail>()
				.Aggregate(ExpressionString, (current, d) => current.Replace(d.ExpressionString, d.GetParameterExpression(adapter)));
		}
	}

	public struct ExpressionCommandDetail : ICommandDetail
	{
		public Column Column { get; set; }
		public string ExpressionString { get; set; }
		public CommandParameter Parameter { get; set; }
		public List<CommandParameter> Parameters { get; set; }
		public OperatorType Logic { get; set; }

		public static ExpressionCommandDetail Create(Expression ex, PropertyInfo pi, object value)
		{

			ExpressionCommandDetail d = new ExpressionCommandDetail
			{
				ExpressionString = ex.ToString(),
				Logic = ex.NodeType.OperatorType(),
				Column = Column.Get(pi),
				Parameter = CommandParameter.Create(Column.Get(pi), value)
			};

			return d;
		}

		public string GetParameterExpression(IDbAdapter adapter)
		{
			return string.Format("{0} {2} {1}", Column.Name, Parameter.ParameterName, adapter.OPERATOR(Logic));
		}
	}

	#endregion
}
