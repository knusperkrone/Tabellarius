namespace Tabellarius.Database
{

	public class ProgramQuery
	{
		public string termin_Uhrzeit { set; get; }
		public string termin_Titel { set; get; }
		public string beschreibung_Text { set; get; }
		public int beschreibung_Typ { set; get; }
		public int termin_Tag { set; get; }
		public int termin_Typ { set; get; }
	}

	public class CategorieQuery
	{
		public string KategorieName { set; get; }
		public string TabName { set; get; }
		public int TabRang { set; get; }
		public string Titel { set; get; }
		public int TitelRang { set; get; }
		public int TitelTyp { set; get; }
		public string Text { set; get; }
		public int TextRang { set; get; }
		public int TextTyp { set; get; }
	}

	public class EventQuery
	{
		public int VeranstaltungID { set; get; }
		public string VeranstaltungsKürzel { set; get; }
		public string VeranstaltungName { set; get; }
		public string VeranstaltungSprache { set; get; }
		public int VeranstaltungJahr { set; get; }
		public int VeranstaltungDauer { set; get; }
		public int InstanzID { set; get; }
		public string InstanzStartDatum { set; get; }
	}

}