using Gtk;
using System;

namespace Tabellarius
{
	public class MiddleToolBar : HBox
	{

		public enum DisplayMode
		{
			Kategorie, Programm, Veranstaltung
		}

		public static MiddleToolBar instance;
		private DisplayMode currMode;

		private ListFrameAdapter listFrameAdapter;
		private EditFrameAdapter editFrameAdapter;
		private MainToolBar mainToolBar;

		private MiddleToolBar() : base()
		{
			// Also a hardcodet frameManager
			this.Orientation = Orientation.Vertical;

			currMode = DisplayMode.Programm; // Default start Mode

			listFrameAdapter = ListFrameAdapter.GetInstance();
			editFrameAdapter = EditFrameAdapter.GetInstance();
			mainToolBar = MainToolBar.GetInstance();

			var textButton = MakeHorizontalButton("Kategorie");
			textButton.Clicked += delegate { Change(DisplayMode.Kategorie); };

			var programButton = MakeHorizontalButton("Programm");
			programButton.Clicked += delegate { Change(DisplayMode.Programm); };

			var eventButton = MakeHorizontalButton("Veranstaltung");
			eventButton.Clicked += delegate { Change(DisplayMode.Veranstaltung); };

			this.PackStart(textButton, false, false, 0);
			this.PackStart(programButton, false, false, 0);
			this.PackStart(eventButton, false, false, 0);
		}

		public static MiddleToolBar GetInstance()
		{
			if (instance == null)
				instance = new MiddleToolBar();
			return instance;
		}

		private void Change(DisplayMode mode)
		{
			if (mode != currMode && editFrameAdapter.ChangeMode(mode)) {
				// Not already applied and Save succeded
				mainToolBar.ChangeMode(mode); // Has to be first
				listFrameAdapter.ChangeMode(mode);
				currMode = mode;
			}
		}

		private static Button MakeHorizontalButton(string text)
		{
			Label label = new Label(text);
			label.Angle = 90;
			return new Button(label);
		}

	}
}