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

		public void PassToEditView(TreeView treeView, RowActivatedArgs args, int day)
		{
			if (!currView.SaveWithDialog())
				return;

			currView.EditTreeRow(treeView, args, day);
		}

		public void PassToEditView(TreeView treeView, RowActivatedArgs args, string tabName)
		{
			if (!currView.SaveWithDialog())
				return;

			currView.EditTreeRow(treeView, args, tabName);
		}

		public TreeIter GetActiveParentTreeIter()
		{
			return currView.currParentIter;
		}

		public bool ChangeMode(MiddleToolBar.DisplayMode mode)
		{
			if (!currView.SaveWithDialog())
				return false;

			Remove(currView); // Free Memory
			currView.Destroy();
			currView.Dispose();

			if (mode == MiddleToolBar.DisplayMode.Programm)
				currView = new ProgrammEditView();
			else
				currView = new CategoryEditView();

			Add(currView);
			currView.ShowAll();

			return true;
		}

		public void Refresh()
		{
			currView.Clear();
		}

	}
}