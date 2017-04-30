using Gtk;
using Tabellarius.ListFramesTypes;

namespace Tabellarius
{
	public class ListFrameAdapter : Viewport
	{

		private static ListFrameAdapter instance;
		private AbstractListView currView;


		private ListFrameAdapter() : base()
		{
			currView = new ProgrammListView();
			this.Add(currView);
		}

		public static ListFrameAdapter GetInstance()
		{
			if (instance == null)
				instance = new ListFrameAdapter();
			return instance;
		}

		// Called by MainToolbar
		public void AddTab()
		{
			currView.AddTab();
		}

		// Called by MainToolbar
		public void AddParentEntry()
		{
			currView.AddParentEntry();
		}

		// Called by MainToolbar
		public void AddChildEntry()
		{
			currView.AddChildEntry();
		}

		public void Refresh()
		{
			currView.DataSetChanged();
		}

		// Called by MiddleToolbar
		public void ChangeMode(MiddleToolBar.DisplayMode mode)
		{
			Remove(currView); // Free Memory
			currView.Destroy();
			currView.Dispose();

			if (mode == MiddleToolBar.DisplayMode.Programm)
				currView = new ProgrammListView();
			else
				currView = new CategorieListView();

			Add(currView);
			currView.ShowAll();
		}

	}
}