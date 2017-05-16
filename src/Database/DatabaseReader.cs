using Gtk;
using SQLite;
using System;
using System.Collections.Generic;

namespace Tabellarius.Database
{

	public class DatabaseReader
	{
		private readonly string fullPath;
		private readonly SQLiteConnection db;

		private ProgramQuery[/*day*/][/*text*/] cached_ProgrammQuery;
		private CategorieQuery[/*tab*/][/*title*/][/*text*/] cached_CategorieQuery;
		private string cached_categorie;
		private string cached_sprache;
		private int cached_instanz_id;
		private int cached_veranstaltungs_id;


		public DatabaseReader(string fullPath)
		{
			this.fullPath = fullPath;
			db = new SQLiteConnection(fullPath);
		}

		private void CacheProgrammData(int veranstaltungs_id, int instanz_id, string sprache)
		{
			this.cached_veranstaltungs_id = veranstaltungs_id;
			this.cached_instanz_id = instanz_id;
			this.cached_sprache = sprache;

			//Get everything from the Database
			ProgramQuery[] query = db.Query<ProgramQuery>(@"
				SELECT
					Termin.Uhrzeit AS Termin_Uhrzeit,
					Termin.Titel AS termin_Titel,
					Beschreibung.Text AS beschreibung_Text,
					Beschreibung.Typ AS beschreibung_Typ,
					Termin.Tag AS termin_Tag,
					Termin.Typ AS termin_Typ

				FROM Veranstaltung
				JOIN Instanz
					ON Veranstaltung.Id == Instanz.Id_Veranstaltung
				JOIN Termin
					ON Veranstaltung.Id == Termin.Id_Veranstaltung
				LEFT JOIN Beschreibung
					ON Instanz.id = Beschreibung.Id_Instanz
					AND Termin.Tag == Beschreibung.Termin_Tag
					AND Termin.Uhrzeit == Beschreibung.Termin_Uhrzeit

				WHERE Veranstaltung.Id == ?
				AND Instanz.Id == ?
				AND Veranstaltung.Jahr = ?
				AND Veranstaltung.Sprache = ?

				ORDER BY Termin.Tag, Termin.Uhrzeit, Beschreibung.Typ, Beschreibung.Text",
				veranstaltungs_id, instanz_id, 2017, sprache).ToArray();

			// Split data in days and add every termin/descr
			var programmList = new List<List<ProgramQuery>>();
			uint i = 0;
			while (i < query.Length) {

				var dayList = new List<ProgramQuery>();
				ProgramQuery firstElem = query[i];
				while (i < query.Length && firstElem.termin_Tag == query[i].termin_Tag) {
					dayList.Add(query[i]);
					i++;
				}

				programmList.Add(dayList);
			}

			// Convert data as array
			i = 0;
			cached_ProgrammQuery = new ProgramQuery[programmList.Count][];
			foreach (var dayList in programmList)
				cached_ProgrammQuery[i++] = dayList.ToArray();
		}

		public TreeStore GetListFrameDataFor(int day, int veranstaltungs_id, int instanz_id, string sprache)
		{   // Populates a treeStore with the data from @programm[day]
			// Check if data is already cached
			if (cached_ProgrammQuery == null
					|| (veranstaltungs_id != cached_veranstaltungs_id
					|| instanz_id != cached_instanz_id
					|| sprache != null || !sprache.Equals(cached_sprache))) {
				CacheProgrammData(veranstaltungs_id, instanz_id, sprache);
			}

			// No more data will be needed
			if (day >= cached_ProgrammQuery.Length) {
				cached_ProgrammQuery = null; // Free cached_programm
				return null;
			}

			TreeStore treeStore = new TreeStore(typeof(string), typeof(string), typeof(string));
			ProgramQuery[] dayList = cached_ProgrammQuery[day];

			for (uint i = 0; i < dayList.Length;) {

				var currItem = dayList[i];
				var iter = treeStore.AppendValues(currItem.termin_Uhrzeit,
										currItem.termin_Titel,
										API_Contract.ProgrammTerminTypHR[currItem.termin_Typ]);

				if (currItem.beschreibung_Text != null) {
					// While has childs
					do {
						treeStore.AppendValues(iter,
								"└──",
								API_Contract.ConvertDatabaseToTreeChild(dayList[i].beschreibung_Text),
								API_Contract.ProgrammDescrTypHR[currItem.beschreibung_Typ]);
						i++;
					} while (i < dayList.Length && currItem.termin_Uhrzeit.Equals(dayList[i].termin_Uhrzeit) && currItem.termin_Tag == dayList[i].termin_Tag);
					continue; // No need to count up
				}
				i++;
			}

			return treeStore;
		}

		public TableQuery<Table_Veranstaltung> GetEvents()
		{
			return db.Table<Table_Veranstaltung>();
		}

		public TableQuery<Table_Instanz> GetInstanzFor(int veranstaltungs_id)
		{
			return db.Table<Table_Instanz>()
					.Where(v => v.Id_Veranstaltung == veranstaltungs_id);
		}

		private void CacheCategorieContent(int veranstaltungs_id, string categorie)
		{
			this.cached_veranstaltungs_id = veranstaltungs_id;
			this.cached_categorie = categorie;

			CategorieQuery[] query = db.Query<CategorieQuery>(@"
				SELECT
					Kategorie.Titel AS KategorieName,
					Kategorie_Tab.TabName AS TabName,
					Kategorie_Tab.Rang AS TabRang,
					Kategorie_Tab_Titel.Titel AS Titel,
					Kategorie_Tab_Titel.Rang AS TitelRang,
					Kategorie_Tab_Titel.Typ As TitelTyp,
					Kategorie_Tab_Text.Text AS Text,
					Kategorie_Tab_Text.Rang AS TextRang,
					Kategorie_Tab_Text.Typ AS TextTyp

				FROM Kategorie
				LEFT JOIN Kategorie_Tab
					ON Kategorie_Tab.Name_Kategorie == Kategorie.Titel
				LEFT JOIN Kategorie_Tab_Titel
					ON Kategorie_Tab_Titel.Name_Kategorie == Kategorie.Titel
					AND Kategorie_Tab_Titel.TabName_Kategorie_Tab  == Kategorie_Tab.TabName
				LEFT JOIN Kategorie_Tab_Text
					ON Kategorie_Tab_Text.Name_Kategorie == Kategorie.Titel
					AND Kategorie_Tab_Text.TabName_Kategorie_Tab  == Kategorie_Tab.TabName
					AND Kategorie_Tab_Text.Titel_Kategorie_Tab_Titel == Kategorie_Tab_Titel.Titel

				WHERE Kategorie.Id_Veranstaltung == ?
				AND Kategorie.Titel ==  ?

				ORDER BY Kategorie.Titel, Kategorie_Tab.Rang, Kategorie_Tab_Titel.Rang, Kategorie_Tab_Text.Rang"
				, veranstaltungs_id, categorie).ToArray();

			var tabList = new List<List<List<CategorieQuery>>>();

			// Make that query on the database and try to visualize it
			int i = 0;
			while (i < query.Length) {
				int currTabR = query[i].TabRang;
				var titelList = new List<List<CategorieQuery>>();
				// While tabId is equal
				while (i < query.Length && currTabR == query[i].TabRang) { // Has childs
					var textList = new List<CategorieQuery>();
					if (query[i].Titel != null) { // Has childs
						int currTitelR = query[i].TitelRang;
						// While tabId and titelId is equal
						while (i < query.Length && currTabR == query[i].TabRang && currTitelR == query[i].TitelRang) {
							textList.Add(query[i]);
							i++;
						}
					} else {
						i++; // No childs -> next entry
					}
					titelList.Add(textList);
				}
				tabList.Add(titelList);

			}

			//Save in array
			cached_CategorieQuery = new CategorieQuery[tabList.Count][][];
			for (i = 0; i < tabList.Count; i++) {
				cached_CategorieQuery[i] = new CategorieQuery[tabList[i].Count][];
				for (int j = 0; j < tabList[i].Count; j++)
					cached_CategorieQuery[i][j] = tabList[i][j].ToArray();
			}
		}

		public Gtk.TreeStore GetTextContentFor(int veranstaltungs_id, string categorie, int tabIndex)
		{
			if (cached_CategorieQuery == null
					|| cached_veranstaltungs_id != veranstaltungs_id
					|| cached_categorie == null || !cached_categorie.Equals(categorie)) {
				CacheCategorieContent(veranstaltungs_id, categorie);
			}

			CategorieQuery[][] titleList = cached_CategorieQuery[tabIndex];

			// Rang, Text, Typ
			TreeStore treeStore = new Gtk.TreeStore(typeof(string), typeof(string), typeof(string));
			TreeIter currIter = TreeIter.Zero;

			foreach (CategorieQuery[] titles in titleList) {
				if (titles.Length == 0) // No childs
					continue;

				CategorieQuery currElem = titles[0];
				currIter = treeStore.AppendValues(currElem.TitelRang,
								API_Contract.CategorieTextParentTypHR[currElem.TitelTyp]
								, currElem.Titel);
				try {

					if (currElem.Text != null) {
						foreach (var text in titles) {
							treeStore.AppendValues(currIter,
								text.TextRang,
								API_Contract.CategorieTextChildTypHR[currElem.TextTyp],
								text.Text);
						}
					}
				} catch (Exception e) {
					Console.WriteLine(e.ToString());
					return new Gtk.TreeStore(typeof(int), typeof(string), typeof(string));
				}
			}
			return treeStore;
		}

		public List<Table_Veranstaltung> GetAllKrzl()
		{
			return db.Query<Table_Veranstaltung>(@"
					SELECT * FROM Veranstaltung
					GROUP BY Kürzel", "");
		}

		public TableQuery<Table_Kategorie> GetCategoriesFor(int veranstaltungs_id)
		{
			return db.Table<Table_Kategorie>()
					.Where(v => v.Id_Veranstaltung == veranstaltungs_id);
		}

		public TableQuery<Table_Kategorie_Tab> GetKategorieTabNamesFor(int veranstaltungs_id, string categorie)
		{
			return db.Table<Table_Kategorie_Tab>()
					.Where(v => v.Id_Veranstaltung == veranstaltungs_id && v.Name_Kategorie.Equals(categorie))
					.OrderBy(v => v.Rang);
		}

		public TreeStore GetEventContent()
		{
			// Id, VeranstaltungName, VeranstaltungsKürzel, VeranstaltungSprache, VeranstaltungZeit
			TreeStore treeStore = new TreeStore(typeof(int), typeof(string), typeof(string), typeof(string), typeof(string));
			EventQuery[] query = db.Query<EventQuery>(@"
			SELECT
				Veranstaltung.Id  AS VeranstaltungID,
				Veranstaltung.Kürzel AS VeranstaltungsKürzel,
				Veranstaltung.Name AS VeranstaltungName,
				Veranstaltung.Sprache AS VeranstaltungSprache,
				Veranstaltung.Jahr AS VeranstaltungJahr,
				Veranstaltung.Dauer AS VeranstaltungDauer,
				Instanz.Id AS InstanzID,
				Instanz.StartDatum AS InstanzStartDatum

			FROM Veranstaltung
			JOIN Instanz
				ON Instanz.Id_Veranstaltung == Veranstaltung.Id

			ORDER BY VeranstaltungJahr, VeranstaltungID, InstanzID").ToArray();

			int i = 0;
			while (i < query.Length) {
				var curr = query[i];
				int parentId = query[i].VeranstaltungID;
				var parentLang = curr.VeranstaltungSprache;
				var currIter = treeStore.AppendValues(curr.VeranstaltungID,
													curr.VeranstaltungName,
													curr.VeranstaltungsKürzel,
													parentLang,
													"" + curr.VeranstaltungJahr);
				do { // Childs (Instanzen)
					treeStore.AppendValues(currIter, curr.InstanzID,
											"",
											"",
											"",
											curr.InstanzStartDatum);
					i++;
					if (i < query.Length)
						curr = query[i];
				} while (i < query.Length && curr.VeranstaltungID == parentId);
			}

			return treeStore;
		}

		public void Close()
		{
			cached_ProgrammQuery = null;
			cached_CategorieQuery = null;
			db.Close();
		}

	}
}