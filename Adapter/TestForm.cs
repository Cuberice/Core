using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Core.Data;
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
		protected IDbAdapter Adapter { get { return DataService.Adapter; } }
		protected SQLBuilder Builder { get; private set; }

		private void TestForm_Shown(object sender, EventArgs e)
		{
			DataService = new DataService();
			Builder = new SQLBuilder(DataService.Adapter);
			Builder.CreateStructure();

			List<CoreEquipment> users = DataService.SelectForModel<CoreEquipment>();
			Grid.DataSource = users;
		}

		private void GenerateTestData_Click(object sender, EventArgs e)
		{
			List<CoreEquipment> equipment = ModelExtensions.CreateTestInstances<CoreEquipment>(5);
		}

		private void TestInsert_Click(object sender, EventArgs e)
		{
			ModelExtensions.CreateTestInstances<CoreEquipment>(3).ForEach(u => DataService.InsertModel(u));
		}

		private void TestUpdate_Click(object sender, EventArgs e)
		{
			CoreEquipment eq = DataService.SelectForModel<CoreEquipment>().First();
			eq.SerialNumber = "123-456-789";

			DataService.UpdateModel(eq);

			List<CoreEquipment> equipment = DataService.SelectForModel<CoreEquipment>();
			Grid.DataSource = equipment;
		}
	}
}
