using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using SQLite;

namespace Tabellarius.Database
{
	public class DatabaseChecker
	{

		private readonly string name;
		private SQLiteConnection conn;
		private readonly List<string> invalidEntrys;
		private HashSet<int> EventPKs;
		private HashSet<int> InstanzPKs;

		private bool finished, wrong_schema, no_file;

		public DatabaseChecker(string name)
		{
			invalidEntrys = new List<string>();
			finished = wrong_schema = no_file = false;
		}


		public bool CheckIntegrity()
		{
			finished = true;

			if (!File.Exists(name)) {
				no_file = true;
				return false;
			}

			conn = new SQLite.SQLiteConnection(name);

			if (!CheckDatabaseSchema()) {
				wrong_schema = true;
				return false;
			}

			try {
				CheckTableVeranstaltung();
				CheckTableInstanz();
				CheckTableTermin();
				CheckTableDescription();
				CheckTableKategorie();
				CheckTableKategorieTab();
				CheckTableKategorieTabTitel();
				CheckTableKategorieTabText();
			} catch {
				wrong_schema = true;
				return false;
			}
			return invalidEntrys.Count == 0;
		}


		public void ShowInvalidEntrys()
		{
			if (!finished)
				CheckIntegrity();

			Gtk.Dialog diag;
			if (wrong_schema) {
				diag = new SafeCallDialog("Datenbank wird nicht untersützt!", "Ok", 0, null, 1);
			} else if (no_file) {
				diag = new SafeCallDialog("Keine solche Datei!", "Ok", 0, null, 1);
			 }else {
				Gtk.Widget invalidItems = GenInvalidEntryView();
				GetUserArgs[] args = { };
				diag = new GetUserDataDialog(args, invalidItems,  "Ok", 0, null, 1);
			}
			diag.Run();
			diag.Destroy();
		}

		private bool CheckDatabaseSchema()
		{
			var query = conn.Query<IntegrityQuery>("SELECT name, sql FROM sqlite_master WHERE type='table' ORDER BY name");
			return query.Count == 8;
		}

		private void CheckTableVeranstaltung()
		{
			EventPKs = new HashSet<int>();


			bool year, lang;
			var query = conn.Table<Table_Veranstaltung>();
			foreach (var entry in query) {
				year = entry.Jahr >= 2017;
				lang = GtkHelper.FindInArray(API_Contract.SupportedLanguages, entry.Sprache) != -1;
				if (!year || !lang) {
					StringBuilder err = new StringBuilder(entry.ToString());
					if (!year)
						err.Append(" - JAHR");
					if (!lang)
						err.Append(" - SPRACHE");
					invalidEntrys.Add(err.ToString());
				}
				// Add pks
				EventPKs.Add(entry.Id);
			}
		}

		private void CheckTableInstanz()
		{
			InstanzPKs = new HashSet<int>();

			bool event_id, date;
			var query = conn.Table<Table_Instanz>();
			foreach (var entry in query) {
				event_id = EventPKs.Contains(entry.Id_Veranstaltung);
				date = Regex.IsMatch(entry.StartDatum, "^20(1[7-9]|[2-9][0-9])\\/(0[0-9]|1[0,2])\\/[0-3][0-9]$");
				if (!event_id || !date) {
					StringBuilder err = new StringBuilder(entry.ToString());
					if (!event_id)
						err.Append(" - EVENT_ID");
					if (!date)
						err.Append(" - DATUM");
					invalidEntrys.Add(err.ToString());
				}
				// Add pks
				InstanzPKs.Add(entry.Id);
			}
		}

		private void CheckTableTermin()
		{
			string tmp;
			bool id_event, time, typ;
			var query = conn.Table<Table_Termin>();
			foreach (var entry in query) {
				id_event = EventPKs.Contains(entry.Id_Veranstaltung);
				time = Regex.IsMatch(entry.Uhrzeit, "^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-9]*$");
				typ = API_Contract.ProgrammTerminTypHR.TryGetValue(entry.Typ, out tmp);
				if (!id_event || !time || !typ) {
					StringBuilder err = new StringBuilder(entry.ToString());
					if (!id_event)
						err.Append(" - ID_VERANSTALTUNG");
					if (!time)
						err.Append(" - UHRZEIT");
					if (!typ)
						err.Append(" - TYP");
					invalidEntrys.Add(err.ToString());
				}
			}
		}

		private void CheckTableDescription()
		{
			string tmp;
			bool id_event, id_instanz, typ;
			var query = conn.Table<Table_Beschreibung>();
			foreach (var entry in query) {
				id_event = EventPKs.Contains(entry.Id_Veranstaltung);
				id_instanz = InstanzPKs.Contains(entry.Id_Instanz);
				typ = API_Contract.ProgrammDescrTypHR.TryGetValue(entry.Typ, out tmp);
				if (!id_event || !id_instanz || !typ) {
					StringBuilder err = new StringBuilder(entry.ToString());
					if (!id_event)
						err.Append(" - ID_VERANSTALTUNG");
					if (!id_instanz)
						err.Append(" - ID_INSTANZ");
					if (!typ)
						err.Append(" - TYP");
					invalidEntrys.Add(err.ToString());
				}
			}
		}

		private void CheckTableKategorie()
		{
			return; // Nothing to Check!
		}

		private void CheckTableKategorieTab()
		{
			return; // Nothing to Check!
		}

		private void CheckTableKategorieTabTitel()
		{
			string tmp;
			bool typ;
			var query = conn.Table<Table_Kategorie_Tab_Titel>();
			foreach (var entry in query) {
				typ = API_Contract.CategorieTextParentTypHR.TryGetValue(entry.Typ, out tmp);
				if (!typ) {
					string err = entry.ToString() + " - TYP";
					invalidEntrys.Add(err);
				}
			}
		}

		private void CheckTableKategorieTabText()
		{
			string tmp;
			bool typ;
			var query = conn.Table<Table_Kategorie_Tab_Text>();
			foreach (var entry in query) {
				typ = API_Contract.CategorieTextParentTypHR.TryGetValue(entry.Typ, out tmp);
				if (!typ) {
					string err = entry.ToString() + " - TYP";
					invalidEntrys.Add(err);
				}
			}
		}

        public Gtk.TreeView GenInvalidEntryView() {
			var treeContent = new Gtk.TreeStore(typeof(string));
            foreach(var elem in invalidEntrys)
				treeContent.AppendValues(elem);

			var cell = new Gtk.CellRendererText();
			cell.Editable = false;
			cell.SetPadding(5, 8);
			var textColumn = new Gtk.TreeViewColumn("Text", cell, "text", CategorieColumnID.Text);

			var tree = new Gtk.TreeView();
			tree.AppendColumn(textColumn);
			tree.EnableGridLines = Gtk.TreeViewGridLines.Both;
			tree.HeadersClickable = false;
			tree.HeadersVisible = true;
			tree.Model = treeContent;
			return tree;
		}

	}
}