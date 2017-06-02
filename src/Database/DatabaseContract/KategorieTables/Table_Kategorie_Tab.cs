using System;
using SQLite;
using Gtk;

namespace Tabellarius.Database
{

	[Table("Kategorie_Tab")]
	public class Table_Kategorie_Tab : DatabaseTable
	{

		[PrimaryKey]
		public int Id_Veranstaltung { get; set; }
		[PrimaryKey]
		public string Name_Kategorie { get; set; }
		[PrimaryKey]
		public string TabName { get; set; }
		[PrimaryKey]
		public int Rang { get; set; }

		public override int Veranstaltungs_ID { get { return Id_Veranstaltung; } }

		public Table_Kategorie_Tab() : base("Kategorie_Tab") { }

		public Table_Kategorie_Tab(bool preInit) : base("Kategorie_Tab")
		{
			if (preInit) {
				this.Id_Veranstaltung = db.Curr_veranstaltungsId;
				this.Name_Kategorie = db.Curr_categorie;
			}
		}

		public Table_Kategorie_Tab(string tabName, int rang) : base("Kategorie_Tab")
		{
			this.Id_Veranstaltung = db.Curr_veranstaltungsId;
			this.Name_Kategorie = db.Curr_categorie;
			this.TabName = tabName;
			this.Rang = rang;
		}

		public override string SETString()
		{
			return String.Format("Id_Veranstaltung = '{0}', Name_Kategorie = '{1}', TabName = '{2}', Rang = '{3}'",
								Id_Veranstaltung, Name_Kategorie, TabName, Rang);
		}

		public override string IdString()
		{
			return String.Format("Id_Veranstaltung == '{0}' AND Name_Kategorie == '{1}' AND TabName == '{2}' AND Rang == '{3}'",
								Id_Veranstaltung, Name_Kategorie, TabName, Rang);
		}

		public override void Update(DatabaseTable newElem)
		{
			if (!(newElem is Table_Kategorie_Tab))
				throw new NotSupportedException("Wrong class! Expected: " + TableName);

			var newTab = (Table_Kategorie_Tab)newElem;
			db.MakeNonQuery(String.Format("UPDATE Kategorie_Tab_Titel"
					+ " SET Id_Veranstaltung = {0}, Name_Kategorie = '{1}', TabName_Kategorie_Tab = '{2}'"
					+ " WHERE Id_Veranstaltung == {3} AND Name_Kategorie == '{4}' AND TabName_Kategorie_Tab == '{5}'"
					, newTab.Id_Veranstaltung, newTab.Name_Kategorie, newTab.TabName
					, this.Id_Veranstaltung, this.Name_Kategorie, this.TabName));
			db.MakeNonQuery(String.Format("UPDATE Kategorie_Tab_Text"
					+ " SET Id_Veranstaltung = {0}, Name_Kategorie = '{1}', TabName_Kategorie_Tab = '{2}'"
					+ " WHERE Id_Veranstaltung == {3} AND Name_Kategorie == '{4}' AND TabName_Kategorie_Tab == '{5}'"
					, newTab.Id_Veranstaltung, newTab.Name_Kategorie, newTab.TabName
					, this.Id_Veranstaltung, this.Name_Kategorie, this.TabName));
			db.UpdateEntry(this, newTab);
		}

	}
}