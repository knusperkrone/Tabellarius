using System.Collections.Generic;
using System.IO;
using SQLite;

namespace Tabellarius.Database
{
	public class DatabaseWriter
	{

		private readonly string fullPath;
		private readonly SQLiteConnection db;


		public DatabaseWriter(string fullPath)
		{
			this.fullPath = fullPath;
			if (!File.Exists(fullPath))
				CreateNewDatabase(fullPath);

			db = new SQLiteConnection(fullPath);
		}

		private DatabaseWriter(SQLiteConnection database)
		{
			fullPath = null;
			db = database;
		}

		public void DbInsert(DatabaseTable elem)
		{
			try {
				db.Insert(elem);
			} catch (SQLiteException) {
				new WarnWindow("\tDer Eintrag konnte nicht eingefügt werden\n\tWahrscheinlich ist der Eintrag schon vorhanden.");
			}
		}

		public void DbUpdate(DatabaseTable oldElem, DatabaseTable newElem)
		{
			SQLiteCommand cmd = db.CreateCommand("UPDATE " + newElem.TableName +
												" SET " + newElem.SETString() +
												" WHERE " + oldElem.UPDATEString());
			System.Console.WriteLine(cmd.CommandText);
			System.Console.WriteLine();
			try {
				cmd.ExecuteNonQuery();
			} catch (SQLiteException e) {
				new WarnWindow("\tDer Eintrag konnte nicht geändert werden\n\tWahrscheinlich ist der Eintrag so schon vorhanden.\n" + e.ToString());
			}
		}

		public void DbDelete(DatabaseTable elem)
		{
			db.Delete(elem); // Does not throw an exception
		}

		private void MergeWith(SQLiteConnection otherDb)
		{
			if (this.db.Equals(otherDb)) {
				new WarnWindow("Die Datenbanken sind identisch.");
				return;
			}
			//var list = new List<DatabaseTable>();

			//TODO: Merging!
			//Veranstaltung
			//Instanz
			//Termin
			//Beschreibung
			//Text
			//Text_Tab
			//Tab_Inhalt
		}

		public void Close()
		{
			db.Commit();
			db.Close();
		}

		private static void CreateNewDatabase(string dbName)
		{
			File.Create(dbName);
			var con = new SQLiteConnection(dbName);
			//XXX: con.CreateTable<T> does not allow multiple PKs
			con.CreateCommand("CREATE TABLE `Veranstaltung` ( `Id` INTEGER, `Kürzel` TEXT NOT NULL, `Name` TEXT NOT NULL, `Sprache` TEXT NOT NULL, `Jahr` INTEGER NOT NULL, `Dauer` INTEGER NOT NULL, PRIMARY KEY(`Id`))").ExecuteNonQuery();
			con.CreateCommand("CREATE TABLE `Instanz` ( `Id_Veranstaltung` INTEGER, `id` INTEGER, `StartDatum` TEXT NOT NULL, PRIMARY KEY(`Id_Veranstaltung`,`id`), FOREIGN KEY(`Id_Veranstaltung`) REFERENCES Veranstaltung(Id) )").ExecuteNonQuery();
			con.CreateCommand("CREATE TABLE `Termin` ( `Id_Veranstaltung` INTEGER, `Tag` INTEGER NOT NULL, `Uhrzeit` TEXT NOT NULL, `Titel` TEXT NOT NULL, `Typ` INTEGER DEFAULT 0, PRIMARY KEY(`Id_Veranstaltung`,`Tag`,`Uhrzeit`), FOREIGN KEY(`Id_Veranstaltung`) REFERENCES `Veranstaltungen`(`Id`) )").ExecuteNonQuery();
			con.CreateCommand("CREATE TABLE 'Beschreibung' ( `Id_Veranstaltung` INTEGER, `Id_Instanz` INTEGER, `Termin_Tag` INTEGER NOT NULL, `Termin_Uhrzeit` TEXT, `Text` TEXT NOT NULL, `Typ` INTEGER DEFAULT 1, PRIMARY KEY(`Id_Veranstaltung`,`Id_Instanz`,`Termin_Tag`,`Termin_Uhrzeit`,`Text`), FOREIGN KEY(`Id_Veranstaltung`) REFERENCES `Veranstaltung`(`Id`), FOREIGN KEY(`Id_Instanz`) REFERENCES `Instanz`(`Id`), FOREIGN KEY(`Termin_Tag`) REFERENCES `Termin`(`Tag`), FOREIGN KEY(`Termin_Uhrzeit`) REFERENCES `Termin`(`Uhrzeit`) )").ExecuteNonQuery();
			con.CreateCommand("CREATE TABLE `Text` ( `Id_Veranstaltung` INTEGER, `Titel` TEXT, PRIMARY KEY(`Id_Veranstaltung`,`Titel`), FOREIGN KEY(`Id_Veranstaltung`) REFERENCES Veranstaltung(Id) )").ExecuteNonQuery();
			con.CreateCommand("CREATE TABLE `Text_Tab` ( `Id_Veranstaltung` INTEGER, `Titel_Text` TEXT, `Name` TEXT, `Rang` INTEGER, PRIMARY KEY(`Id_Veranstaltung`,`Titel_Text`,`Name`,`Rang`), FOREIGN KEY(`Id_Veranstaltung`) REFERENCES Veranstaltung(Id), FOREIGN KEY(`Titel_Text`) REFERENCES Text(Titel) )").ExecuteNonQuery();
			con.CreateCommand("CREATE TABLE 'Tab_Inhalt' ( `Id_Veranstaltung` INTEGER, `Titel_Tab` TEXT, `Text` TEXT NOT NULL, `Typ` INTEGER NOT NULL, `Rang` INTEGER NOT NULL, PRIMARY KEY(`Id_Veranstaltung`,`Titel_Tab`,`Text`,`Typ`,`Rang`), FOREIGN KEY(`Id_Veranstaltung`) REFERENCES `Veranstaltung`(`Id`), FOREIGN KEY(`Titel_Tab`) REFERENCES `Text_Tab`(`Name`) )").ExecuteNonQuery();
			con.Close();
		}

	}
}