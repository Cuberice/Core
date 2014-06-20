using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Core.Adapters;
using Core.Data;
using Core.Models;
using Core.Service;
using Models;

namespace Core
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
		protected IDataService DataService { get; private set; }
		protected SQLBuilder Builder { get; private set; }

		private void TestForm_Shown(object sender, EventArgs e)
		{
			IDbAdapter adapter = new SQLiteDataAdapter();
			DataService = new DataService {Adapter = adapter};
			Builder = new SQLBuilder(DataService.Adapter);
			Builder.CreateStructure();

			List<Equipment> users = DataService.GetAllForModel(ModelExtensions.CreateInstance<Equipment>);
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
