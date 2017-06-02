using System;
using SQLite;
using Gtk;

namespace Tabellarius.Database
{

	[Table("Kategorie_Tab_Text")]
	public class Table_Kategorie_Tab_Text : DatabaseTable
	{

		[PrimaryKey]
		public int Id_Veranstaltung { get; set; }
		[PrimaryKey]
		public string Name_Kategorie { get; set; }
		[PrimaryKey]
		public string TabName_Kategorie_Tab { get; set; }
		[PrimaryKey]
		public string Titel_Kategorie_Tab_Titel { get; set; }
		[PrimaryKey]
		public string Text { get; set; }
		[PrimaryKey]
		public int Typ { get; set; }
		[PrimaryKey]
		public int Rang { get; set; }

		public override int Veranstaltungs_ID { get { return Id_Veranstaltung; } }

		public Table_Kategorie_Tab_Text() : base("Kategorie_Tab_Text") { }

		public Table_Kategorie_Tab_Text(bool preInit) : base("Kategorie_Tab_Text")
		{
			if (preInit) {
				this.Id_Veranstaltung = db.Curr_veranstaltungsId;
				this.Name_Kategorie = db.Curr_categorie;
			}
		}

		public Table_Kategorie_Tab_Text(string tabName, string titelName, string text, int typ, int rang) : base("Kategorie_Tab_Text")
		{
			this.Id_Veranstaltung = db.Curr_veranstaltungsId;
			this.Name_Kategorie = db.Curr_categorie;
			this.TabName_Kategorie_Tab = tabName;
			this.Titel_Kategorie_Tab_Titel = titelName;
			this.Text = text;
			this.Typ = typ;
			this.Rang = rang;
		}

		public override string SETString()
		{
			return String.Format("Id_Veranstaltung = '{0}', Name_Kategorie = '{1}', TabName_Kategorie_Tab = '{2}', Titel_Kategorie_Tab_Titel = '{3}', Text = '{4}', Typ = '{5}', Rang = '{6}'",
								Id_Veranstaltung, Name_Kategorie, TabName_Kategorie_Tab, Titel_Kategorie_Tab_Titel, Text, Typ, Rang);
		}

		public override string IdString()
		{
			return String.Format("Id_Veranstaltung == '{0}' AND Name_Kategorie == '{1}' AND TabName_Kategorie_Tab == '{2}' AND Titel_Kategorie_Tab_Titel == '{3}' AND Text == '{4}' AND Typ == '{5}' AND Rang == '{6}'",
								Id_Veranstaltung, Name_Kategorie, TabName_Kategorie_Tab, Titel_Kategorie_Tab_Titel, Text, Typ, Rang);
		}

		public override void Update(DatabaseTable newText) {
			if (!(newText is Table_Kategorie_Tab_Text))
				throw new NotSupportedException("Wrong class! Expected: " + TableName);

			db.UpdateEntry(this, newText);
		}

	}
}