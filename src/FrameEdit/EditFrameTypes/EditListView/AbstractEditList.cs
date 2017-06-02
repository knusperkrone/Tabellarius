using System;

namespace Tabellarius.EditFrameTypes.ListEditView
{
    public abstract class AbstractEditList : AbstractEditView
    {

		/* Fills the UI, with the passed ListRow and make it editable */
		protected abstract void EditTreeRow(Gtk.TreeView treeView, Gtk.RowActivatedArgs args, object tabData);

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

    }
}