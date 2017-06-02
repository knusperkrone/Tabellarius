using System;
using SQLite;
using Gtk;

namespace Tabellarius.Database
{
	[Table("Kategorie_Tab_Titel")]
	public class Table_Kategorie_Tab_Titel : DatabaseTable
	{

		[PrimaryKey]
		public int Id_Veranstaltung { get; set; }
		[PrimaryKey]
		public string Name_Kategorie { get; set; }
		[PrimaryKey]
		public string TabName_Kategorie_Tab { get; set; }
		[PrimaryKey]
		public string Titel { get; set; }
		[PrimaryKey]
		public int Typ { get; set; }
		[PrimaryKey]
		public int Rang { get; set; }

		public override int Veranstaltungs_ID { get { return Id_Veranstaltung; } }

		public Table_Kategorie_Tab_Titel(bool preInit) : base("Kategorie_Tab_Titel")
		{
			if (preInit) {
				this.Id_Veranstaltung = db.Curr_veranstaltungsId;
				this.Name_Kategorie = db.Curr_categorie;
			}
		}

		public Table_Kategorie_Tab_Titel() : base("Kategorie_Tab_Titel") { }

		public Table_Kategorie_Tab_Titel(string tabName, string titel, int typ, int rang) : base("Kategorie_Tab_Titel")
		{
			this.Id_Veranstaltung = db.Curr_veranstaltungsId;
			this.Name_Kategorie = db.Curr_categorie;
			this.TabName_Kategorie_Tab = tabName;
			this.Titel = titel;
			this.Typ = typ;
			this.Rang = rang;
		}

		public override string SETString()
		{
			return String.Format("Id_Veranstaltung = '{0}', Name_Kategorie = '{1}', TabName_Kategorie_Tab = '{2}', Titel = '{3}', Typ = '{4}', Rang = '{5}'",
									Id_Veranstaltung, Name_Kategorie, TabName_Kategorie_Tab, Titel, Typ, Rang);
		}

		public override string IdString()
		{
			return String.Format("Id_Veranstaltung == '{0}' AND Name_Kategorie == '{1}' AND TabName_Kategorie_Tab == '{2}' AND Titel == '{3}' AND Typ == '{4}' AND Rang == '{5}'",
									Id_Veranstaltung, Name_Kategorie, TabName_Kategorie_Tab, Titel, Typ, Rang);
		}

		public override void Update(DatabaseTable newElem)
		{
			if (!(newElem is Table_Kategorie_Tab_Titel))
				throw new NotSupportedException("Wrong class! Expected: " + TableName);

			var newTitel = (Table_Kategorie_Tab_Titel)newElem;
			db.MakeNonQuery(String.Format("UPDATE Kategorie_Tab_Text"
					+ " SET Id_Veranstaltung = {0}, Name_Kategorie = '{1}', TabName_Kategorie_Tab = '{2}', Titel_Kategorie_Tab_Titel = '{3}'"
					+ " WHERE Id_Veranstaltung == {4} AND Name_Kategorie == '{5}' AND TabName_Kategorie_Tab == '{6}' AND Titel_Kategorie_Tab_Titel = '{7}'"
					, newTitel.Id_Veranstaltung, newTitel.Name_Kategorie, newTitel.TabName_Kategorie_Tab, newTitel.Titel
					, this.Id_Veranstaltung, this.Name_Kategorie, this.TabName_Kategorie_Tab, this.Titel));

			db.UpdateEntry(this, newTitel);
		}

	}
}