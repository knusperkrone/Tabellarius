using Gtk;
using System;
using Tabellarius.EditFrameTypes;

namespace Tabellarius
{

	public class EditFrameAdapter : Viewport
	{
		public static EditFrameAdapter instance;
		private AbstractEditView currView;


		private EditFrameAdapter() : base()
		{
			currView = new ProgrammEditView();
			this.Add(currView);
		}

		public static EditFrameAdapter GetInstance()
		{
			if (instance == null)
				instance = new EditFrameAdapter();
			return instance;
		}

		public void PassToEditView(TreeView treeView, RowActivatedArgs args, object detail)
		{
			if (currView.SaveWithDialog()) {
				currView.PassTreeRow(treeView, args, detail);
			} else {
				// Switch back to old UI selection
				treeView.Selection.SelectIter(currView.currTreeIter);
			}
		}

		public TreeIter ActiveParentTreeIter
		{
			get { return currView.currParentIter; }
		}

		public bool ChangeMode(MiddleToolBar.DisplayMode mode)
		{
			if (!currView.SaveWithDialog())
				return false;

			// Free Memory
			this.Remove(currView);
			currView.Destroy();
			currView.Dispose();

			if (mode == MiddleToolBar.DisplayMode.Programm) {
				currView = new ProgrammEditView();
			} else if (mode == MiddleToolBar.DisplayMode.Kategorie) {
				currView = new CategoryEditView();
			} else {
				currView = new EventEditView();
			}

			this.Add(currView);
			currView.ShowAll();
			return true;
		}

		public void Refresh()
		{
			currView.Clear();
		}

	}
}