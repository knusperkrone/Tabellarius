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

		public abstract void EditTreeRow(Gtk.TreeView treeView, Gtk.RowActivatedArgs args, object tabData);
		public abstract void Clear();
		protected abstract void OnCancel(object sender, System.EventArgs args);
		protected abstract bool OnSave();
		protected abstract bool SaveNecessary();

		public virtual new void Dispose()
		{
			saveButton.Dispose();
			cancelButton.Dispose();
			base.Dispose();
		}

		protected static bool IsParent(Gtk.RowActivatedArgs args) {
			return args.Path.Depth == 1;
		}

		protected bool IsCurrParent
		{
			get { return currParentIter.Equals(currTreeIter); }
		}

		public bool SaveWithDialog()
		{
			if (!SaveNecessary())
				return true;

			bool ret = true;
			var saveDiag = new SafeCallDialog("Soll die Änderung gespeichert werden?", "Speichern", 0, "Abbruch", 1);
			if (saveDiag.Run() == 0) { // User wants to safe
				if (!OnSave()) {// But User has invalid input
					var diag = new SafeCallDialog("Es konnte nicht gespeichert werden!", "Ok", 0, null, 1);
					diag.Run();
					diag.Destroy();
					ret = false;
				}
			} else { // User does not want to safe;
				System.Console.WriteLine("Don't pass!");
				ret = false;
			}
			saveDiag.Destroy();
			return ret;
		}

	}
}