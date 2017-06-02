using System;
using SQLite;

namespace Tabellarius.Database
{
	[Table("Kategorie")]
	public class Table_Kategorie : DatabaseTable
	{
		[PrimaryKey]
		public int Id_Veranstaltung { get; set; }
		[NotNull]
		public string Titel { get; set; } //TODO: Change to Name

		public override int Veranstaltungs_ID { get { return Id_Veranstaltung; } }

		public Table_Kategorie() : base("Kategorie") { }

		public Table_Kategorie(bool preInit) : base("Kategorie")
		{
			if (preInit) {
				this.Id_Veranstaltung = db.Curr_veranstaltungsId;
			}
		}

		public Table_Kategorie(string CategorieName) : base("Kategorie")
		{
			this.Titel = CategorieName;
			this.Id_Veranstaltung = db.Curr_veranstaltungsId;
		}

		public override string SETString()
		{
			return String.Format("Id_Veranstaltung = '{0}', Titel = '{1}'",
								Id_Veranstaltung, Titel);
		}

		public override string IdString()
		{
			return String.Format("Id_Veranstaltung == '{0}' AND Titel == '{1}'",
								Id_Veranstaltung, Titel);
		}

		public override void Update(DatabaseTable newElem)
		{
			if (!(newElem is Table_Kategorie))
				throw new NotSupportedException("Wrong class! Expected: " + TableName);

			var newKategorie = (Table_Kategorie)newElem;
			db.MakeNonQuery(String.Format("UPDATE Kategorie_Tab"
					+ " SET Id_Veranstaltung = {0}, Name_Kategorie = '{1}'"
					+ " WHERE Id_Veranstaltung == {2} AND Name_Kategorie == '{3}'"
					, newKategorie.Id_Veranstaltung, newKategorie.Titel
					, this.Id_Veranstaltung, this.Titel));
			db.MakeNonQuery(String.Format("UPDATE Kategorie_Tab_Titel"
					+ " SET Id_Veranstaltung = {0}, Name_Kategorie = '{1}'"
					+ " WHERE Id_Veranstaltung == {2} AND Name_Kategorie == '{3}'"
					, newKategorie.Id_Veranstaltung, newKategorie.Titel
					, this.Id_Veranstaltung, this.Titel));
			db.MakeNonQuery(String.Format("UPDATE Kategorie_Tab_Text"
					+ " SET Id_Veranstaltung = {0}, Name_Kategorie = '{1}'"
					+ " WHERE Id_Veranstaltung == {2} AND Name_Kategorie == '{3}'"
					, newKategorie.Id_Veranstaltung, newKategorie.Titel
					, this.Id_Veranstaltung, this.Titel));
			db.UpdateEntry(this, newElem);
		}

	}
}