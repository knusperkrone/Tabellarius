namespace Tabellarius.EditFrameTypes
{
	public abstract class AbstractEditView : Gtk.VBox
	{

		public Gtk.TreeIter currParentIter { get; protected set; }
		public Gtk.TreeIter currTreeIter { get; protected set; }
		protected Gtk.TreeStore currTreeStore { get; set; }
		protected readonly Gtk.Button saveButton, cancelButton;

		protected bool init;
		protected abstract bool Init {  set; }


		protected AbstractEditView()
		{

			saveButton = new Gtk.Button("Speichern");
			cancelButton = new Gtk.Button("Zurücksetzen");

			saveButton.Clicked += delegate { OnSave(); };
			cancelButton.Clicked += OnCancel;

			var buttonBox = new Gtk.HBox();
			buttonBox.PackStart(saveButton, false, true, 5);
			buttonBox.PackStart(cancelButton, false, true, 2);
			this.PackEnd(buttonBox, false, false, 5);
		}

		/* Clears out the Whole UI Elements */
		public abstract void Clear();

		public virtual new void Dispose()
		{
			saveButton.Dispose();
			cancelButton.Dispose();
			base.Dispose();
		}

		/* Resets all UI values to the last Saved values */
		protected abstract void OnCancel(object sender, System.EventArgs args);

		/* Checks if data integrity and save, also indicates invalid input */
		protected abstract bool OnSave();



		protected bool SaveNecessary() {
			return init && IsDirty();
		}

		protected abstract bool IsDirty();

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

	}
}