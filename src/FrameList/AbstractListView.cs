using System.Collections.Generic;


namespace Tabellarius.ListFrameTypes
{
    public abstract class AbstractListView : Gtk.ScrolledWindow
    {

		protected DatabaseAdapter dbAdapter;
		protected EditFrameAdapter editFrameAdapter;

		protected int tabs;
		protected Gtk.Notebook tabView;
		protected List<Gtk.TreeView> treeList;

		protected AbstractListView() : base() {
			this.SetPolicy(Gtk.PolicyType.Automatic, Gtk.PolicyType.Never);

			dbAdapter = DatabaseAdapter.GetInstance();
			editFrameAdapter = EditFrameAdapter.GetInstance();

			tabView = new Gtk.Notebook();
			treeList = new List<Gtk.TreeView>();

			PopulateTabView();

			AddWithViewport(tabView);
		}

		protected abstract void PopulateTabView();
		public abstract void AddTab();
		public abstract void AddParentEntry();
		public abstract void AddChildEntry();
		public void DataSetChanged() {
			for (uint i = 0; i < tabs; i++) {
				tabView.RemovePage(0);
			}
			tabs = 0;

			foreach (Gtk.TreeView tree in treeList)
				tree.Dispose();
			treeList.Clear();

			// Repopulate
			PopulateTabView();
			tabView.ShowAll();
		}
		protected abstract Gtk.ScrolledWindow GenScrollableTree(Gtk.TreeStore treeContent);
		public new void Dispose() {
			foreach (var tree in treeList)
				tree.Dispose();
			tabView.Dispose();
			base.Dispose();
		}

		protected static Gtk.CellRendererText GenTextCell()
		{
			var cell = new Gtk.CellRendererText();
			cell.Editable = false;
			cell.SetPadding(5, 8);
			return cell;
		}
	}
}