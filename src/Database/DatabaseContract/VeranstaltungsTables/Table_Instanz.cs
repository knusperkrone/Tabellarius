using System;
using SQLite;

namespace Tabellarius.Database
{
	[Table("Instanz")]
	public class Table_Instanz : DatabaseTable
	{

		[PrimaryKey]
		public int Id_Veranstaltung { get; set; }
		[PrimaryKey]
		public int Id { get; set; }
		[NotNull]
		public string StartDatum { get; set; }

		public override int Veranstaltungs_ID { get { return Id_Veranstaltung; } }

		public Table_Instanz() : base("Instanz") { }

		public Table_Instanz(bool preInit) : base("Instanz")
		{
			if (preInit) {
				this.Id_Veranstaltung = db.Curr_veranstaltungsId;
				this.Id = db.Curr_instanzId;
			}
		}

		public Table_Instanz(int id_Veranstaltung, int id, string startDatum) : base("Instanz")
		{
			this.Id_Veranstaltung = id_Veranstaltung;
			this.Id = id;
			this.StartDatum = startDatum;
		}

		public override string SETString()
		{
			return String.Format("Id_Veranstaltung = '{0}', Id == '{1}', StartDatum = '{2}'",
								Id_Veranstaltung, Id, StartDatum);
		}

		public override string IdString()
		{
			return String.Format("Id_Veranstaltung == '{0}' AND Id == '{1}' AND StartDatum == '{2}'",
								Id_Veranstaltung, Id, StartDatum);
		}

		public override void Update(DatabaseTable newElem)
		{
			if (!(newElem is Table_Instanz))
				throw new NotSupportedException("Wrong class! Expected:" + TableName);

			db.UpdateEntry(this, newElem);
		}

	}
}