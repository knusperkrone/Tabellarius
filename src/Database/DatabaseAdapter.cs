using System.IO;
using System.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using Tabellarius.Database;

namespace Tabellarius
{

	public class DatabaseAdapter
	{
		private static DatabaseAdapter instance;
		private static readonly string downloadPath = "https://github.com/knusperkrone/Konfi-Castle/raw/master/DatenBankSync/KonfiCastle.db";
		private static readonly string dataFolder = "." + Path.DirectorySeparatorChar.ToString() + "Projekte" + Path.DirectorySeparatorChar.ToString();

		private bool init = false;
		private bool activeVeranstaltung = false;
		private bool activeCategorie = false;

		DatabaseWriter dbWriter;
		DatabaseReader dbReader;

		private int curr_veranstaltungsId;
		public int Curr_veranstaltungsId
		{
			get { return this.curr_veranstaltungsId; }
			set
			{
				activeVeranstaltung = (value >= 0);
				curr_veranstaltungsId = value;
			}
		}

		private int curr_instanzId;
		public int Curr_instanzId
		{
			get { return this.curr_instanzId; }
			set
			{
				activeVeranstaltung = (value >= 0);
				curr_instanzId = value;
			}
		}

		private string curr_lang;
		public string Curr_lang
		{
			get { return curr_lang; }
			set
			{
				if (value != null)
					curr_lang = value;
			}
		}

		private string curr_categorie;
		public string Curr_categorie
		{
			get { return curr_lang; }
			set
			{
				if (value != null)
					curr_categorie = value;
			}
		}


		private DatabaseAdapter(string dbName)
		{
			Init(dbName);
		}

		private void Init(string dbName)
		{
			if (!Directory.Exists(dataFolder)) // Default dir
				Directory.CreateDirectory(dataFolder);

			if (init) // Close existing connections
				Close();
			init = true;

			dbWriter = new DatabaseWriter(dataFolder + dbName);
			dbReader = new DatabaseReader(dataFolder + dbName);
		}

		public static DatabaseAdapter GetInstance()
		{
			if (instance == null) {
				//TODO: Database chooser
				throw new Exception("No inited Database");
			}
			return instance;
		}

		public static void setDb(string name)
		{
			if (instance == null)
				instance = new DatabaseAdapter(name);
			else
				instance.Init(name);
		}

		public Gtk.TreeStore GetListFrameContentFor(uint day)
		{
			if (!init)
				throw new Exception("DatabaseAdapter is already closed");

			return dbReader.GetListFrameDataFor(day, curr_veranstaltungsId,
												curr_instanzId, curr_lang);
		}

		public string[] GetCategoriesNames()
		{
			if (!activeVeranstaltung)
				throw new Exception("No Content Avaiable");

			var query = dbReader.GetCategoriesFor(curr_veranstaltungsId);

			string[] tmp = new string[query.Count()];
			uint i = 0;
			foreach (var textCategorie in query) {
				tmp[i++] = textCategorie.Titel;
			}
			return tmp;
		}

		public string[] GetCategorieTabNames()
		{
			if (!activeVeranstaltung)
				throw new Exception("No Content Avaiable");

			var query = dbReader.GetKategorieTabNamesFor(curr_veranstaltungsId, curr_categorie);

			string[] tmp = new string[query.Count()];
			uint i = 0;
			foreach (var elem in query) {
				tmp[i++] = elem.TabName;
			}
			return tmp;
		}


		public Gtk.TreeStore GetTextFrameContentFor(int tabIndex)
		{
			if (!activeVeranstaltung)
				throw new Exception("No Content Avaiable");
			return dbReader.GetTextContentFor(curr_veranstaltungsId, curr_categorie, tabIndex);
		}

		public void insertEntry(DatabaseTable elem)
		{
			if (!init)
				throw new Exception("DatabaseAdapter is already closed");
			dbWriter.DbInsert(elem);
		}

		public void updateEntry(DatabaseTable oldElem, string setStatement)
		{
			if (!init)
				throw new Exception("DatabaseAdapter is already closed");
			dbWriter.DbUpdate(oldElem, setStatement);
		}

		public void deleteEntry(DatabaseTable elem)
		{
			if (!init || !activeVeranstaltung)
				throw new Exception("DatabaseAdapter is already closed");
			dbWriter.DbDelete(elem);
		}

		public void GetToolbarData(out string[] dataKrzl, out string[] dataVeranstaltung,
									out int[] dataVeranstaltungIds,
									out string[] dataSprache, out string[][] dataInstanz)
		{
			var query = dbReader.GetEvents();

			// Fill up arrays
			dataKrzl = new String[query.Count()];
			dataVeranstaltung = new String[query.Count()];
			dataVeranstaltungIds = new int[query.Count()];
			dataSprache = new String[query.Count()];
			dataInstanz = new String[query.Count()][];

			uint i = 0;
			foreach (var elem in query) {
				dataKrzl[i] = elem.Kürzel;
				dataVeranstaltung[i] = elem.Name;
				dataVeranstaltungIds[i] = elem.Id;
				dataSprache[i] = elem.Sprache;

				uint j = 0;
				var instanzen = dbReader.GetInstanzFor(elem.Id);
				dataInstanz[i] = new string[instanzen.Count()];
				foreach (var instanz in instanzen) {
					dataInstanz[i][j++] = instanz.Id + ": " + instanz.StartDatum;
				}
				i++;
			}
		}

		public void NoActiveCategorie()
		{
			activeCategorie = false;
		}

		public void NoActiveVeranstaltung()
		{
			activeVeranstaltung = false;
		}

		public void Close()
		{
			dbWriter.Close();
			dbReader.Close();
			init = false;
		}

		private void DownloadDatabase()
		{
			//TODO:
			if (!init || !activeVeranstaltung)
				throw new Exception("DatabaseAdapter is already closed");

			try {
				var diag = new DownloadWindow("Lade Datenbank herunter...");
				new WebClient().DownloadFile(downloadPath, dataFolder + "MAGIC_NAME"); //TODO:
				diag.Destroy();
				Console.WriteLine("Db heruntergeladen");
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}

	}
}