using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common;
using RegalWorxData.Service;

namespace RegalWorxData
{
	public partial class TestForm : Form
	{
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new TestForm());
		}
		public TestForm()
		{
			InitializeComponent();
		}

		protected DataGridView Grid { get { return grid; } }
		protected IRWDataService DataService { get; private set; }
		protected SQLBuilder Builder { get; private set; }

		private void TestForm_Shown(object sender, EventArgs e)
		{
			IDbAdapter adapter = new SQLiteDataAdapter();
			DataService = new RWDataService {Adapter = adapter};
			Builder = new SQLBuilder(DataService.Adapter);
			Builder.CreateStructure();

			List<User> users = DataService.GetAllUsers();
			Grid.DataSource = users;
		}

		private void GenerateTestData_Click(object sender, EventArgs e)
		{
			List<User> users = User.CreateTestInstances(3);
			List<Equipment> equipment = Equipment.CreateTestInstances(5);

			string cmd = DataService.Adapter.CreateSelectCommand<Equipment>(eq => eq.Name == "SomeName" || eq.Type.ID == equipment.First().Type.ID);
		}

		private void TestInsert_Click(object sender, EventArgs e)
		{
			User.CreateTestInstances(3).ForEach(u => DataService.InsertModel(u));
			Equipment.CreateTestInstances(5).ForEach(u => u.InsertTestObject(DataService));
		}

		private void TestUpdate_Click(object sender, EventArgs e)
		{
			Equipment eq = DataService.GetAllEquipment().First();
			eq.SerialNumber = "123-456-789";

			DataService.UpdateModel(eq);

			List<Equipment> equipment = DataService.GetAllEquipment();
			Grid.DataSource = equipment;
		}
	}
}
