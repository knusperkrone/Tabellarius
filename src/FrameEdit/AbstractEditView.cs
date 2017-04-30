
namespace Tabellarius.EditFrameTypes
{
	public abstract class AbstractEditView : Gtk.VBox
	{
		public Gtk.TreeIter currParentIter { get; protected set; }
		public Gtk.TreeIter currTreeIter { get; protected set; }

		protected abstract bool OnSave();
		protected abstract void OnCancel(object sender, System.EventArgs args);
		public abstract void EditTreeRow(Gtk.TreeView treeView, Gtk.RowActivatedArgs args, int day);
		public abstract void Clear();
		protected abstract bool SaveNecessary();

		public bool SaveWithDialog()
		{
			if (!SaveNecessary())
				return true;

			bool ret = true;
			var saveDiag = new SafeCallDialog("Soll die Änderung gespeichert werden?", "Speichern", 0, "Nein", 1);
			if (saveDiag.Run() == 0) { // User wants to safe
				if (!OnSave()) {// But User has invalid input
					var diag = new SafeCallDialog("Es konnte nicht gespeichert werden!", "Ok", 0, null, 1);
					diag.Run();
					diag.Destroy();
					ret = false;
				}
			} else { // User does not want to safe;
				OnCancel(null, null);
			}
			saveDiag.Destroy();
			return ret;
		}

		public virtual new void Dispose()
		{
			base.Dispose();
		}
	}
}