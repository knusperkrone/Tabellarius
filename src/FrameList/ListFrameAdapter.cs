using Gtk;
using Tabellarius.ListFrameTypes;

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
		public void ChangeMode(DisplayMode mode)
		{
			Remove(currView); // Free Memory
			currView.Destroy();
			currView.Dispose();

			switch (mode) {
				case DisplayMode.PROGRAMM:
					currView = new ProgrammListView();
					break;
				case DisplayMode.KATEGORIE:
					currView = new CategorieListView();
					break;
				case DisplayMode.TEXTE:
					currView = new TextsListView();
					break;
				case DisplayMode.VERANSTALTUNG:
					currView = new EventListView();
					break;
				default:
					throw new System.NotImplementedException("Invalid DisplayMode");
			}

			Add(currView);
			currView.ShowAll();
		}

	}
}