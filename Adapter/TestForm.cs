using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
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

			List<CoreEquipment> users = DataService.GetAllForModel(ModelExtensions.CreateInstance<CoreEquipment>);
			Grid.DataSource = users;
		}

		private void GenerateTestData_Click(object sender, EventArgs e)
		{
			List<CoreEquipmentMake> equipment = CoreEquipment.CreateTestInstances(5);
			string cmd = DataService.Adapter.CreateSelectCommand<CoreEquipment>(eq => eq.Name == "SomeName" || eq.Make.ID == equipment.First().ID);
		}

		private void TestInsert_Click(object sender, EventArgs e)
		{
			CoreEquipment.CreateTestInstances(3).ForEach(u => DataService.InsertModel(u));
			CoreEquipment.CreateTestInstances(5).ForEach(u => u.InsertTestObject(DataService));
		}

		private void TestUpdate_Click(object sender, EventArgs e)
		{
			CoreEquipment eq = DataService.GetAllForModel(ModelExtensions.CreateInstance<CoreEquipment>).First();
			eq.SerialNumber = "123-456-789";

			DataService.UpdateModel(eq);

			List<CoreEquipment> equipment = DataService.GetAllForModel(ModelExtensions.CreateInstance<CoreEquipment>);
			Grid.DataSource = equipment;
		}
	}
}
