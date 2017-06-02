using Gtk;

namespace Tabellarius
{
	public class MiddleToolBar : HBox
	{
		public static MiddleToolBar instance;

		private MainToolBar mainToolBar;

		private MiddleToolBar() : base()
		{
			// Also a hardcodet frameManager
			this.Orientation = Orientation.Vertical;
			var fm = FrameManager.GetInstance();

			mainToolBar = MainToolBar.GetInstance();

			var textButton = MakeHorizontalButton("Kategorie");
			textButton.Clicked += delegate
			{
				fm.ChangeMainFrameMode(DisplayMode.TEXTE);
			};

			var programButton = MakeHorizontalButton("Programm");
			programButton.Clicked += delegate
			{
				fm.ChangeMainFrameMode(DisplayMode.PROGRAMM);
			};

			var eventButton = MakeHorizontalButton("Veranstaltung");
			eventButton.Clicked += delegate
			{
				fm.ChangeMainFrameMode(DisplayMode.VERANSTALTUNG);
			};

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

		private static Button MakeHorizontalButton(string text)
		{
			Label label = new Label(text);
			label.Angle = 90;
			return new Button(label);
		}

	}
}