using Gtk;
using SQLite;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System;

namespace Tabellarius.Database
{
	public class DatabaseChecker
	{

		private class InvalidStruct
		{
			public readonly DatabaseTable table;
			public readonly List<string> errors;

			public InvalidStruct(DatabaseTable table)
			{
				this.table = table;
				errors = new List<string>();
			}
		}


		private Window win;
		SafeCallDialog mainDiag;
		SafeCallDialog sideDiag;
		private SQLiteConnection conn;
		private HashSet<int> EventPKs = new HashSet<int>();
		private HashSet<int> InstanzPKs = new HashSet<int>();
		private List<InvalidStruct> invalidEntrys = new List<InvalidStruct>();
		private string name;
		private bool finished, wrong_schema, no_file;


		public DatabaseChecker(Window win)
		{
			Init(win);
		}

		public DatabaseChecker(Window win, string name)
		{
			this.name = name;
			Init(win);
		}

		public string Name
		{
			set { name = value; finished = wrong_schema = no_file = false; }
		}

		public bool IsChecked
		{
			get { return finished; }
		}

		public bool IsSupported
		{
			get { return !wrong_schema; }
		}

		private void Init(Window win)
		{
			this.win = win;
			finished = wrong_schema = no_file = false;
		}

		public bool CheckIntegrity()
		{
			if (name == null) // Invalid Usage
				throw new Exception("[Integrity Checker] No db was Set");

			if (finished) // Already done checking
				return invalidEntrys.Count == 0; // Compiler optimizes anyway

			finished = true;
			wrong_schema = no_file = false;
			invalidEntrys.Clear();
			EventPKs.Clear();
			InstanzPKs.Clear();

			if (!File.Exists(name)) {
				no_file = true;
				return false;
			}

			try {
				conn = new SQLite.SQLiteConnection(name);
				// Check tables
				CheckTableVeranstaltung();
				CheckTableInstanz();
				CheckTableTermin();
				CheckTableDescription();
				CheckTableKategorie();
				CheckTableKategorieTab();
				CheckTableKategorieTabTitel();
				CheckTableKategorieTabText();
			} catch (Exception e) {
				Console.WriteLine("[DatabaseChecker] CheckIntegrity:\n" + e);
				// Exception only raises if the db-table-schema is not supported
				wrong_schema = true;
				return false;
			} finally {
				if (conn != null)
					conn.Close();
				conn = null;
			}

			return invalidEntrys.Count == 0;
		}


		public bool ShowInvalidEntrys()
		{
			if (!finished) // Check Integrity first
				CheckIntegrity();

			if (wrong_schema) {
				mainDiag = new SafeCallDialog(new Label("Datenbank wird nicht untersützt!"), "Ok");
			} else if (no_file) {
				mainDiag = new SafeCallDialog(new Label("Keine solche Datei"), "Ok");
			} else {
				// Save ressources and only init that once!
				sideDiag = new SafeCallDialog("Soll der Eintrag wirklich gelösch werden?", "Ja", 0, "Nein", 1);
				sideDiag.Hide();

				// Pass invalid entrys to UI
				TreeView invalidItems = GenInvalidEntryView();

				VBox content = new VBox();
				content.PackStart(new Label("Inkonsistente Einträge:"), false, false, 5);
				content.PackStart(invalidItems, true, true, 2);
				mainDiag = new SafeCallDialog(content, "Ok");
			}

			mainDiag.Run();
			mainDiag.Destroy();
			mainDiag = null;

			if (sideDiag != null)
				sideDiag.Destroy();
			sideDiag = null;

			return invalidEntrys.Count == 0;
		}


		public TreeView GenInvalidEntryView()
		{
			// Tree Content
			var treeContent = new TreeStore(typeof(string));
			foreach (var elem in invalidEntrys) {
				// Make HR
				var strBuild = new StringBuilder(elem.table.IdString());
				foreach (string errMsg in elem.errors)
					strBuild.Append(" - " + errMsg); // Append errors
													 // Append in TreeContent
				treeContent.AppendValues(strBuild.ToString());
			}

			// Gen Tree TreeViewColumn
			var cell = new CellRendererText();
			cell.Editable = false;
			cell.SetPadding(5, 8);
			var textColumn = new TreeViewColumn("Text", cell, "text", 0);

			var tree = new TreeView();
			tree.AppendColumn(textColumn);
			tree.EnableGridLines = TreeViewGridLines.Both;
			tree.HeadersClickable = false;
			tree.HeadersVisible = true;
			tree.Model = treeContent;

			tree.RowActivated += DeleteCurrRow;

			return tree;
		}

		private void DeleteCurrRow(object sender, RowActivatedArgs args)
		{
			var ret = sideDiag.Run();
			sideDiag.Hide();
			if (ret == 1) // User decides otherwise
				return;

			SQLiteConnection conn = null;
			try {
				conn = new SQLiteConnection(name);
				TreeStore treeContent = (TreeStore)((TreeView)sender).Model;

				// Delete on: UI, SQL, intern list
				TreeIter iter;
				treeContent.GetIter(out iter, args.Path);
				treeContent.Remove(ref iter); // UI
				int index = int.Parse(args.Path.ToString()); // Get index
				SQLDelete(conn, invalidEntrys[index].table); // Database
				invalidEntrys.Remove(invalidEntrys[index]); // Intern List

				// Are all Entrys removed?
				if (invalidEntrys.Count == 0) {
					if (mainDiag == null)
						throw new InvalidDataException("main Diag not inited!");
					mainDiag.Respond(0); // Close mainDiag
				}
			} catch (Exception e) {
				Console.WriteLine("[DatabaseChecker] DeleteCurrRow:\n" + e);
			} finally {
				if (conn != null)
					conn.Close();
			}
		}

		private static void SQLDelete(SQLiteConnection conn, DatabaseTable elem)
		{
			try {
				conn.Execute(String.Format("DELETE FROM '{0}' WHERE {1}", elem.TableName, elem.IdString()));
			} catch (Exception e) {
				Console.WriteLine("[CHECKER] delete failed: " + elem.IdString());
				Console.WriteLine(e);
			}
		}

		private void CheckTableVeranstaltung()
		{
			bool year, lang;
			var query = conn.Table<Table_Veranstaltung>();

			foreach (var entry in query) {
				year = entry.Jahr >= 2017;
				lang = GtkHelper.FindInArray(API_Contract.SupportedLanguages, entry.Sprache) != -1;
				if (!year || !lang) {
					var invalid = new InvalidStruct(entry);
					if (!year)
						invalid.errors.Add("JAHR");
					if (!lang)
						invalid.errors.Add("SPRACHE");
					invalidEntrys.Add(invalid);
				}
				// Add pks
				EventPKs.Add(entry.Id);
			}
		}

		private void CheckTableInstanz()
		{
			bool event_id, date;
			var query = conn.Table<Table_Instanz>();
			foreach (var entry in query) {
				event_id = EventPKs.Contains(entry.Id_Veranstaltung);
				date = Regex.IsMatch(entry.StartDatum, "^20(1[7-9]|[2-9][0-9])\\/(0[0-9]|1[0,2])\\/[0-3][0-9]$");
				if (!event_id || !date) {
					var invalid = new InvalidStruct(entry);
					if (!event_id)
						invalid.errors.Add("EVENT_ID");
					if (!date)
						invalid.errors.Add("DATUM");
					invalidEntrys.Add(invalid);
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
					var invalid = new InvalidStruct(entry);
					if (!id_event)
						invalid.errors.Add("ID_VERANSTALTUNG");
					if (!time)
						invalid.errors.Add("UHRZEIT");
					if (!typ)
						invalid.errors.Add("TYP");
					invalidEntrys.Add(invalid);
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
					var invalid = new InvalidStruct(entry);
					if (!id_event)
						invalid.errors.Add("ID_VERANSTALTUNG");
					if (!id_instanz)
						invalid.errors.Add("ID_INSTANZ");
					if (!typ)
						invalid.errors.Add("TYP");
					invalidEntrys.Add(invalid);
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
					var invalid = new InvalidStruct(entry);
					invalid.errors.Add("TYP");
					invalidEntrys.Add(invalid);
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
					var invalid = new InvalidStruct(entry);
					invalid.errors.Add("TYP");
					invalidEntrys.Add(invalid);
				}
			}
		}

	}
}