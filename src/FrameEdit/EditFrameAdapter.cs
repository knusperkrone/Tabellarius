using System;
using Gtk;
using Tabellarius.EditFrameTypes.ListEditView;
using Tabellarius.EditFrameTypes.TabEditView;
using Tabellarius.EditFrameTypes;
using Tabellarius.ListFrameTypes;


namespace Tabellarius
{
	public class EditFrameAdapter : Viewport
	{

		private enum EditViewMode { TAB_EDIT, LIST_EDIT };

		private static EditFrameAdapter instance;
		private AbstractEditView currView;
		private DisplayMode displayMode;


		private EditFrameAdapter() : base()
		{
			displayMode = DisplayMode.PROGRAMM;
			currView = new EditProgrammList();
			this.Add(currView);
		}

		public static EditFrameAdapter GetInstance()
		{
			if (instance == null)
				instance = new EditFrameAdapter();
			return instance;
		}

		public void PassToTabView(TabHeader header, TreeStore treeStore)
		{
			if (!(currView is AbstractEditTab)) {
				ApplyEditView(EditViewMode.TAB_EDIT);
			}

			((AbstractEditTab)currView).EditTabRow(header, treeStore);
		}

		public void PassToEditView(TreeView treeView, RowActivatedArgs args, object detail)
		{
			if (!(currView is AbstractEditList)) {
				ApplyEditView(EditViewMode.LIST_EDIT);
			}

			if (currView.SaveWithDialog()) {
				((AbstractEditList)currView).PassTreeRow(treeView, args, detail);
			} else {
				// Save failed - switch back to old UI selection
				treeView.Selection.SelectIter(currView.currTreeIter);
			}
		}

		public TreeIter ActiveParentTreeIter
		{
			get
			{
				if (currView == null) return TreeIter.Zero;
				return currView.currParentIter;
			}
		}

		public bool ChangeMode(DisplayMode mode)
		{
			if (!currView.SaveWithDialog())
				return false;

			displayMode = mode;
			ApplyEditView(EditViewMode.LIST_EDIT);

			return true;
		}

		private void ApplyEditView(EditViewMode viewMode)
		{
			this.Remove(currView);
			currView.Destroy();
			currView.Dispose();

			if (viewMode == EditViewMode.LIST_EDIT) {
				switch (displayMode) {
					case DisplayMode.PROGRAMM:
						currView = new EditProgrammList(); break;
					case DisplayMode.KATEGORIE:
						currView = new EditCategorieList(); break;
					case DisplayMode.TEXTE:
						currView = new EditTabCategoryList(); break;
					case DisplayMode.VERANSTALTUNG:
						currView = new EditEventList(); break;
					default:
						throw new NotImplementedException("Invalid [LIST_EDIT] displayMode");
				}
			} else if (viewMode == EditViewMode.TAB_EDIT) {
				switch (displayMode) {
					case DisplayMode.PROGRAMM:
						currView = new EditProgrammTab(); break;
					case DisplayMode.TEXTE:
						currView = new EditCategorieTab(); break;
					default:
						throw new NotImplementedException("Invalid [TAB_EDIT] displayMode");
				}
			} else
				throw new NotImplementedException("Invalid [ViewMode]");

			this.Add(currView);
			currView.ShowAll();
		}

		public void Refresh()
		{
			currView.Clear();
		}

	}
}