using System;
using SQLite;

namespace Tabellarius.Database
{
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

		public Table_Beschreibung(bool preInit) : base("Beschreibung")
		{
			if (preInit) {
				this.Id_Veranstaltung = db.Curr_veranstaltungsId;
				this.Id_Instanz = db.Curr_instanzId;
			}
		}

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

		public override string IdString()
		{
			return String.Format("Id_Veranstaltung == '{0}' AND Id_Instanz == '{1}' AND Termin_Tag == '{2}' AND Termin_Uhrzeit == '{3}' AND Text == '{4}' AND Typ == '{5}'",
								Id_Veranstaltung, Id_Instanz, Termin_Tag, Termin_Uhrzeit, Text, Typ);
		}

		public override void Update(DatabaseTable newElem)
		{
			if (!(newElem is Table_Beschreibung))
				throw new NotSupportedException("Wrong class! Expected:" + TableName);

			db.UpdateEntry(this, newElem);
		}

	}
}