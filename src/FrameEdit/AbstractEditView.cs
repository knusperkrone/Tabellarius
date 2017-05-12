using System;

namespace Tabellarius.EditFrameTypes
{
	public abstract class AbstractEditView : Gtk.VBox
	{

		protected DatabaseAdapter dbAdapter;

		protected Gtk.TreeStore currTreeStore { get; set; }
		public Gtk.TreeIter currParentIter { get; protected set; }
		public Gtk.TreeIter currTreeIter { get; protected set; }

		protected bool init;
		protected abstract bool Init { get; set; }

		protected readonly Gtk.Button saveButton, cancelButton;

		protected AbstractEditView()
		{
			// Inits necessary UI and values
			dbAdapter = DatabaseAdapter.GetInstance();

			saveButton = new Gtk.Button("Speichern");
			cancelButton = new Gtk.Button("Zurücksetzen");

			saveButton.Clicked += delegate { OnSave(); };
			cancelButton.Clicked += OnCancel;

			var buttonBox = new Gtk.HBox();
			buttonBox.PackStart(saveButton, false, true, 5);
			buttonBox.PackStart(cancelButton, false, true, 2);
			this.PackEnd(buttonBox, false, false, 5);
		}

		/* Fills the UI, with the passed ListRow and make it editable */
		protected abstract void EditTreeRow(Gtk.TreeView treeView, Gtk.RowActivatedArgs args, object tabData);
		/* Clears out the Whole UI Elements */
		public abstract void Clear();
		/* Resets all UI values to the last Saved values */
		protected abstract void OnCancel(object sender, System.EventArgs args);
		/* Checks if data integrity and save, also indicates invalid input */
		protected abstract bool OnSave();
		/* Checks if the View needs to be saved, before beeing dismissed */
		protected abstract bool SaveNecessary();

		/* Setup/Validate data for the GUI View */
		public void PassTreeRow(Gtk.TreeView treeView, Gtk.RowActivatedArgs args, object tabData)
		{
			if (treeView == null || args == null) {
				Clear();
				Init = false;
				throw new Exception("Es können keine leeren Listenelemente übergeben werden");
			}

			// Save necessary tree data
			Gtk.TreeIter currIter;
			this.currTreeStore = (Gtk.TreeStore)treeView.Model;
			if (!this.currTreeStore.GetIter(out currIter, args.Path)) {
				Clear();
				Init = false;
				throw new Exception("Das Listenelement konnte nicht gefunden werden!");
			}
			this.currTreeIter = currIter;

			Init = true;
			EditTreeRow(treeView, args, tabData); // Call template Method
		}

		/* Returns if the TreeRowArgs are for a parent Element */
		protected static bool IsParent(Gtk.RowActivatedArgs args)
		{
			return args.Path.Depth == 1;
		}

		/* Returns if the current ListRow is a Parent element */
		protected bool IsCurrParent
		{
			get { return currParentIter.Equals(currTreeIter); }
		}

		/* Setups a Dialog Window, that reminds the User to safe */
		public bool SaveWithDialog()
		{
			if (!SaveNecessary())
				return true;

			bool ret = true;
			var saveDiag = new SafeCallDialog("Soll die Änderung gespeichert werden?", "Speichern", 0, "Abbruch", 1);
			if (saveDiag.Run() == 0) { // User wants to safe
				if (!OnSave()) { // But User has invalid input
					var diag = new SafeCallDialog("Es konnte nicht gespeichert werden!", "Ok", 0, null, 1);
					diag.Run();
					diag.Destroy();
					ret = false;
				}
			} else { // User does not want to safe;
				ret = false;
			}
			saveDiag.Destroy();
			return ret;
		}

		public virtual new void Dispose()
		{
			saveButton.Dispose();
			cancelButton.Dispose();
			base.Dispose();
		}

	}
}