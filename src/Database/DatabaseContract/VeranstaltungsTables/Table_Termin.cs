using System;
using SQLite;

namespace Tabellarius.Database
{

	[Table("Termin")]
	public class Table_Termin : DatabaseTable
	{
		[PrimaryKey]
		public int Id_Veranstaltung { get; set; }
		[PrimaryKey]
		public int Tag { get; set; }
		[PrimaryKey]
		public string Uhrzeit { get; set; }
		[NotNull]
		public string Titel { get; set; }
		[NotNull]
		public int Typ { get; set; }

		public override int Veranstaltungs_ID { get { return Id_Veranstaltung; } }

		public Table_Termin() : base("Termin") { }

		public Table_Termin(bool preInit) : base("Termin")
		{
			if (preInit) {
				this.Id_Veranstaltung = db.Curr_veranstaltungsId;
			}
		}

		public Table_Termin(int tag, string uhrzeit, string titel, int typ) : base("Termin")
		{
			this.Id_Veranstaltung = db.Curr_veranstaltungsId;
			this.Tag = tag;
			this.Uhrzeit = uhrzeit;
			this.Titel = titel;
			this.Typ = typ;
		}

		public override string SETString()
		{
			return String.Format("Id_Veranstaltung = '{0}', Tag = '{1}', Uhrzeit = '{2}', Titel = '{3}', TYP = '{4}'",
								Id_Veranstaltung, Tag, Uhrzeit, Titel, Typ);
		}

		public override string IdString()
		{
			return String.Format("Id_Veranstaltung == '{0}' AND Tag == '{1}' AND Uhrzeit == '{2}' AND Titel == '{3}' AND TYP == '{4}'",
								Id_Veranstaltung, Tag, Uhrzeit, Titel, Typ);
		}

		public override void Update(DatabaseTable newElem)
		{
			if (!(newElem is Table_Termin))
				throw new NotSupportedException("Wrong class! Expected:" + TableName);

			var newTermin = (Table_Termin)newElem;
			int Id_Instanz = db.Curr_instanzId;
			db.MakeNonQuery(String.Format("UPDATE Beschreibung"
					+ " SET Id_Veranstaltung = {0}, Id_Instanz = {1}, Termin_Tag = {2}, Termin_Uhrzeit = '{3}'"
					+ " WHERE Id_Veranstaltung == {4} AND Id_Instanz = {5} AND Termin_Tag == {6} AND Termin_Uhrzeit == '{7}'"
					, newTermin.Id_Veranstaltung, Id_Instanz, newTermin.Tag, newTermin.Uhrzeit
					, this.Id_Veranstaltung, Id_Instanz, this.Tag, this.Uhrzeit));
			db.UpdateEntry(this, newTermin);
		}

	}
}