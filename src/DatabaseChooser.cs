using Gtk;
using System.IO;
using System;
using Tabellarius.Database;

namespace Tabellarius
{
	public class DatabaseChooser : Window
	{

		enum status { NO_FILE, BAD_FILE, TO_CHECK, INVALID, VALID }

		private static readonly string No_File = "Keine Datei ausgewählt";
		private static readonly string Bad_File = "Datei wird nicht unterstützt";
		private static readonly string To_Check = "Datenbank muss noch geprüft werden";
		private static readonly string Valid = "Die Datenbank ist sauber";
		private static readonly string Invalid = "Die Datenbank hat ein paar Ungereimtheiten";

		DatabaseChecker dbChecker;
		FileChooserWidget fileChooser;
		Label statusLabel;
		Entry textEntry;
		private int dbStatus;


		public DatabaseChooser() : base("Wähle eine Datenbank")
		{
			// Init global data
			dbChecker = new DatabaseChecker(this);
			fileChooser = new FileChooserWidget(new FileChooserAction());
			statusLabel = new Label();
			textEntry = new Entry();
			textEntry.IsEditable = false; // Only FileChooserWidget can choose
			dbStatus = (int)status.NO_FILE;

			// Data chooser
			Label pathLabel = new Label("Datei");
			var pathBox = new HBox();
			pathBox.PackStart(pathLabel, false, true, 5);
			pathBox.PackStart(textEntry, true, true, 0);

			// FileFilter
			var allFilter = new FileFilter();
			allFilter.Name = "Alle Dateien";
			allFilter.AddPattern("*");

			var dbFilter = new FileFilter();
			dbFilter.Name = "Datenbanken";
			dbFilter.AddPattern("*.db");
			dbFilter.AddMimeType("application/sql");
			dbFilter.AddMimeType("text/sql");
			dbFilter.AddMimeType("text/x-sql");
			//dbFilter.AddMimeType("text/plain"); //XXX: Wait for feedback
			fileChooser.AddFilter(dbFilter);
			fileChooser.AddFilter(allFilter);
			// On Action handler
			fileChooser.FileActivated += OnFileChoosed;
			// Show current File on the side
			fileChooser.ExtraWidget = pathBox;
			fileChooser.ExtraWidget.WidthRequest = 500;

			// Buttons
			Button validateButton = new Button("Datenbank prüfen");
			validateButton.Clicked += delegate { Validate(true); };
			Button okButton = new Button("Ok");
			okButton.Clicked += OnOk;
			var buttonBox = new Table(1, 5, true);
			buttonBox.Attach(validateButton, 3, 4, 0, 1);
			buttonBox.Attach(okButton, 4, 5, 0, 1);

			// Pack on this
			var mainBox = new VBox();
			mainBox.PackStart(fileChooser, true, true, 0);
			mainBox.PackStart(new HSeparator(), false, false, 2);
			mainBox.PackStart(buttonBox, false, false, 3);
			this.Add(mainBox);
		}

		private int DbStatus
		{
			set
			{
				dbStatus = value;
				switch (value) { // Set Label
					case (int)status.BAD_FILE:
						statusLabel.Text = Bad_File; break;
					case (int)status.NO_FILE:
						statusLabel.Text = No_File; break;
					case (int)status.INVALID:
						statusLabel.Text = Invalid; break;
					case (int)status.TO_CHECK:
						statusLabel.Text = To_Check; break;
					case (int)status.VALID:
						statusLabel.Text = Valid; break;
					default:
						statusLabel.Text = "Ups."; break;
				}
			}
			get { return dbStatus; }
		}

		private void OnFileChoosed(object sender, EventArgs args)
		{
			string text = fileChooser.Filename;
			if (textEntry.Text.Equals(text))
				return; // Same File!

			// Set values
			textEntry.Text = text;
			if (text.Length == 0 || !File.Exists(text)) {
				DbStatus = (int)status.NO_FILE;
			} else {
				DbStatus = (int)status.TO_CHECK;
			}
		}

		private void OnOk(object sender, EventArgs args)
		{
			Dialog diag = null;
			switch (DbStatus) {
				case (int)status.BAD_FILE:
					diag = new SafeCallDialog(Bad_File, "Ok");
					break;
				case (int)status.NO_FILE:
					diag = new SafeCallDialog(No_File, "Ok");
					break;
				case (int)status.INVALID:
					diag = new SafeCallDialog(Invalid, "Ok, Weiter", 0, "Abbrechen", 1);
					if (diag.Run() == 0)
						StartMainWindow();
					diag.Destroy();
					diag = null;
					break;
				case (int)status.TO_CHECK:
					Validate(false); // Check db first
					OnOk(null, null); // Recursive call
					break;
				case (int)status.VALID:
					StartMainWindow();
					break;
			}
			if (diag != null) {
				diag.Run();
				diag.Destroy();
			}
		}

		private void Validate(bool showEntrys)
		{
			dbChecker.Name = textEntry.Text;
			if (!dbChecker.CheckIntegrity()) {
				if (!dbChecker.IsSupported) {
					DbStatus = (int)status.BAD_FILE;
				} else {
					DbStatus = (int)status.INVALID;
					if (showEntrys) { // We want to see the entrys
						if (dbChecker.ShowInvalidEntrys()) {
							// User deleted all invalid Entrys!
							DbStatus = (int)status.VALID;
						}
					}
				}
			} else {
				DbStatus = (int)status.VALID;
			}
		}

		private void StartMainWindow()
		{
			DatabaseAdapter.SetDb(textEntry.Text, false);
			// TODO: MainFrame StartMainWindow();
			MainFrame.GetInstance().ShowAll();
			this.Destroy();
		}

	}
}