using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Core;
using Core.Common;
using Core.Data;


namespace Models
{
	[DebuggerDisplay("{DebugString()}")]
	[Table("TBL_CORE_EQUIPMENT")]
	public struct CoreEquipment : ITestObject, IDebuggerObject
	{
		[Column("ID", Column.DataType.Guid, PrimaryKey = true)]
		public Guid ID { get; private set; }

		[Column("NAME", Column.DataType.String, NotNull = true)]
		public string Name { get; set; }

		[Column("SERIALNUMBER", Column.DataType.String)]
		public string SerialNumber { get; set; }

		[Column("COST", Column.DataType.Double)]
		public double Cost { get; set; }

		[Column("ISBROKEN", Column.DataType.Boolean)]
		public bool IsBroken { get; set; }

		[Column("PURCHASEDATE", Column.DataType.DateTime)]
		public DateTime PurchaseDate { get; set; }

		[Column("MAKE", Column.DataType.Guid)]
		public CoreEquipmentMake Make { get; set; }

		[Column("DIR", Column.DataType.String, Conversion = Column.ConversionType.DirectoryInfo)]
		public DirectoryInfo Directory { get; set; }

		public object CreateTestObject()
		{
			ID = Guid.NewGuid();
			Name = Extentions.GetRandomString(8);
			Cost = new Random().NextDouble();
			PurchaseDate = DateTime.Now;
			Make = (CoreEquipmentMake)new CoreEquipmentMake().CreateTestObject();
			Directory = new DirectoryInfo(@"C:\Stuff\SyncTest\Destination");
			return this;
		}

		public string DebugString()
		{
			return ToString();
		}

		public override string ToString()
		{
			return string.Format("Equipment Name: {0}, Make: {1}", Name, Make);
		}
	}

	[Table("TBL_CORE_EQUIPMENT_MAKE")]
	public struct CoreEquipmentMake : ITestObject
	{
		[Column("ID", Column.DataType.Guid, PrimaryKey = true)]
		public Guid ID { get; private set; }
	
		[Column("NAME", Column.DataType.String, NotNull = true)]
		public string Name { get; set; }
	
		public object CreateTestObject()
		{
			ID = Guid.NewGuid();
			Name = "Make-" + Extentions.GetRandomString(8);
	
			return this;
		}
		public override string ToString()
		{
			return Name;
		}
	}
}
