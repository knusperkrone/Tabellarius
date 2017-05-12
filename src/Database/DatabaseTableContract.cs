using System;
using SQLite;

namespace Tabellarius.Database
{
	//XXX: Allow multiple PrimaryKeys in the SQLite API

	public abstract class DatabaseTable
	{
		protected static DatabaseAdapter db = DatabaseAdapter.GetInstance();

		public readonly string TableName;

		public abstract int Veranstaltungs_ID { get; }

		public DatabaseTable() { }

		public DatabaseTable(string tableName)
		{
			this.TableName = tableName;
		}

		public abstract string SETString();

		public abstract string UPDATEString();
	}


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

		public override string UPDATEString()
		{
			return String.Format("ID == '{0}' AND Kürzel == '{1}' AND Name == '{2}' AND Sprache == '{3}' AND Jahr == '{4}' AND Dauer == '{5}'",
								Id, Kürzel, Name, Sprache, Jahr, Dauer);
		}

	}

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

		public override string UPDATEString()
		{
			return String.Format("Id_Veranstaltung == '{0}' AND Id == '{1}' AND StartDatum == '{2}'",
								Id_Veranstaltung, Id, StartDatum);
		}
	}

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

		public Table_Termin(int tag, string uhrzeit, string titel, int typ) : base("Termin")
		{
			this.Id_Veranstaltung = db.Curr_veranstaltungsId;
			this.Tag = tag;
			this.Uhrzeit = uhrzeit;
			this.Titel = titel;
			this.Typ = typ;
		}

		public override string SETString() {
			return String.Format("Id_Veranstaltung = '{0}', Tag = '{1}', Uhrzeit = '{2}', Titel = '{3}', TYP = '{4}'",
								Id_Veranstaltung, Tag, Uhrzeit, Titel, Typ);
		}

		public override string UPDATEString()
		{
			return String.Format("Id_Veranstaltung == '{0}' AND Tag == '{1}' AND Uhrzeit == '{2}' AND Titel == '{3}' AND TYP == '{4}'",
								Id_Veranstaltung, Tag, Uhrzeit, Titel, Typ);
		}
	}

	[Table("Beschreibung")]
	public class Table_Beschreibung : DatabaseTable
	{
		[PrimaryKey]
		public int Id_Veranstaltung { get; set; }
		[PrimaryKey]
		public int Id_Instanz { get; set; }
		[PrimaryKey]
		public int Termin_Tag { get; set; }
		[NotNull]
		public string Termin_Uhrzeit { get; set; }
		[NotNull]
		public string Text { get; set; }
		[NotNull]
		public int Typ { get; set; }

		public override int Veranstaltungs_ID { get { return Id_Veranstaltung; } }

		public Table_Beschreibung() : base("Beschreibung") { }

		public Table_Beschreibung(int termin_Tag, string termin_Uhrzeit, string text, int typ) : base("Beschreibung")
		{
			this.Id_Veranstaltung = db.Curr_veranstaltungsId;
			this.Id_Instanz = db.Curr_instanzId;
			this.Termin_Tag = termin_Tag;
			this.Termin_Uhrzeit = termin_Uhrzeit;
			this.Text = text;
			this.Typ = typ;
		}

		public override string SETString()
		{
			return String.Format("Id_Veranstaltung = '{0}', Id_Instanz = '{1}', Termin_Tag = '{2}', Termin_Uhrzeit = '{3}', Text = '{4}', Typ = '{5}'",
								Id_Veranstaltung, Id_Instanz, Termin_Tag, Termin_Uhrzeit, Text, Typ);
		}

		public override string UPDATEString()
		{
			return String.Format("Id_Veranstaltung == '{0}' AND Id_Instanz == '{1}' AND Termin_Tag == '{2}' AND Termin_Uhrzeit == '{3}' AND Text == '{4}' AND Typ == '{5}'",
								Id_Veranstaltung, Id_Instanz, Termin_Tag, Termin_Uhrzeit, Text, Typ);
		}

	}

	[Table("Kategorie")]
	public class Table_Kategorie : DatabaseTable
	{
		[PrimaryKey]
		public int Id_Veranstaltung { get; set; }
		[NotNull]
		public string Titel { get; set; } //TODO: Change to Name

		public Table_Kategorie() : base("Kategorie") { }

		public override int Veranstaltungs_ID { get { return Id_Veranstaltung; } }

		public Table_Kategorie(string titel) : base("Kategorie")
		{
			this.Id_Veranstaltung = db.Curr_veranstaltungsId;
			this.Titel = titel;
		}

		public override string SETString()
		{
			return String.Format("Id_Veranstaltung = '{0}', Titel = '{1}'",
								Id_Veranstaltung, Titel);
		}

		public override string UPDATEString()
		{
			return String.Format("Id_Veranstaltung == '{0}' AND Titel == '{1}'",
								Id_Veranstaltung, Titel);
		}
	}

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

		public override string UPDATEString()
		{
			return String.Format("Id_Veranstaltung == '{0}' AND Name_Kategorie == '{1}' AND TabName == '{2}' AND Rang == '{3}'",
								Id_Veranstaltung, Name_Kategorie, TabName, Rang);
		}
	}

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

		public override string UPDATEString()
		{
			return String.Format("Id_Veranstaltung == '{0}' AND Name_Kategorie == '{1}' AND TabName_Kategorie_Tab == '{2}' AND Titel == '{3}' AND Typ == '{4}' AND Rang == '{5}'",
									Id_Veranstaltung, Name_Kategorie, TabName_Kategorie_Tab, Titel, Typ, Rang);
		}
	}

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

		public override string UPDATEString()
		{
			return String.Format("Id_Veranstaltung == '{0}' AND Name_Kategorie == '{1}' AND TabName_Kategorie_Tab == '{2}' AND Titel_Kategorie_Tab_Titel == '{3}' AND Text == '{4}' AND Typ == '{5}' AND Rang == '{6}'",
								Id_Veranstaltung, Name_Kategorie, TabName_Kategorie_Tab, Titel_Kategorie_Tab_Titel, Text, Typ, Rang);
		}
	}

}