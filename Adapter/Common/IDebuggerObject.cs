namespace Core.Common
{
	public interface IDebuggerObject
	{
		string DebugString();
	}
	public interface ITestObject
	{
		object CreateTestObject();
	}
}
