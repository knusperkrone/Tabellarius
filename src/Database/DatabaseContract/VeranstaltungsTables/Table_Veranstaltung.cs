using SQLite;
using System;

namespace Tabellarius.Database
{
	[Table("Veranstaltung")]
	public class Table_Veranstaltung : DatabaseTable
	{
		[PrimaryKey]
		public int Id { get; set; }
		[PrimaryKey]
		public string Kürzel { get; set; }
		[NotNull]
		public string Name { get; set; }
		[NotNull]
		public string Sprache { get; set; }
		[NotNull]
		public int Jahr { get; set; }
		public int Dauer { get; set; }

		public override int Veranstaltungs_ID { get { return Id; } }

		public Table_Veranstaltung() : base("Veranstaltung") { }

		public Table_Veranstaltung(bool preInit) : base("Veranstaltung")
		{
			if (preInit) {
				this.Id = db.Curr_veranstaltungsId;
			}
		}

		public Table_Veranstaltung(int id, string krzl, string name, string sprache, int jahr) : base("Veranstaltung")
		{
			this.Id = id;
			this.Kürzel = krzl;
			this.Name = name;
			this.Sprache = sprache;
			this.Jahr = jahr;
			this.Dauer = 4; // TODO: Delete line
		}

		public override string SETString()
		{

			return String.Format("Id = '{0}', Kürzel = '{1}', Name = '{2}', Sprache = '{3}', Jahr = '{4}', Dauer = '{5}'",
								Id, Kürzel, Name, Sprache, Jahr, Dauer);
		}

		public override string IdString()
		{
			return String.Format("ID == '{0}' AND Kürzel == '{1}' AND Name == '{2}' AND Sprache == '{3}' AND Jahr == '{4}' AND Dauer == '{5}'",
								Id, Kürzel, Name, Sprache, Jahr, Dauer);
		}

		public override void Update(DatabaseTable newElem)
		{
			if (!(newElem is Table_Veranstaltung))
				throw new NotSupportedException("Wrong class! Expected:" + TableName);

			//Update all other tables
			var newVeranstaltung = (Table_Veranstaltung)newElem;
			db.MakeNonQuery(String.Format("UPDATE Instanz"
				+ " SET Id_Veranstaltung = {0}"
				+ " WHERE Id_Veranstaltung == {1}"
					, this.Id
					, newVeranstaltung.Id));
			db.MakeNonQuery(String.Format("UPDATE Termin"
				+ " SET Id_Veranstaltung = {0}"
				+ " WHERE Id_Veranstaltung == {1}"
					, this.Id
					, newVeranstaltung.Id));
			db.MakeNonQuery(String.Format("UPDATE Beschreibung"
				+ " SET Id_Veranstaltung = {0}"
				+ " WHERE Id_Veranstaltung == {1}"
					, this.Id
					, newVeranstaltung.Id));
			db.MakeNonQuery(String.Format("UPDATE Kategorie"
				+ " SET Id_Veranstaltung = {0}"
				+ " WHERE Id_Veranstaltung == {1}"
					, this.Id
					, newVeranstaltung.Id));
			db.MakeNonQuery(String.Format("UPDATE Kategorie_Tab"
				+ " SET Id_Veranstaltung = {0}"
				+ " WHERE Id_Veranstaltung == {1}"
					, this.Id
					, newVeranstaltung.Id));
			db.MakeNonQuery(String.Format("UPDATE Kategorie_Tab_Titel"
				+ " SET Id_Veranstaltung = {0}"
				+ " WHERE Id_Veranstaltung == {1}"
					, this.Id
					, newVeranstaltung.Id));
			db.MakeNonQuery(String.Format("UPDATE Kategorie_Tab_Text"
				+ " SET Id_Veranstaltung = {0}"
				+ " WHERE Id_Veranstaltung == {1}"
					, this.Id
					, newVeranstaltung.Id));

			db.UpdateEntry(this, newElem);
		}

	}
}