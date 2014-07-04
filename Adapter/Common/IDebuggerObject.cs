namespace Core
{
	public interface IDebuggerObject
	{
		string DebugString();
	}
	public interface ITestObject
	{
		object CreateTestObject(params object[] parameters);
	}
}
