namespace Tabellarius.ListFramesTypes
{
    public abstract class AbstractListView : Gtk.ScrolledWindow
    {
		public abstract void AddTab();
		public abstract void AddParentEntry();
		public abstract void AddChildEntry();
		public abstract void DataSetChanged();
		protected abstract Gtk.ScrolledWindow GenScrollableTree(Gtk.TreeStore treeContent);
		public virtual new void Dispose() {
			base.Dispose();
		}
	}
}