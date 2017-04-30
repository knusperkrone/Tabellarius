using Gtk;
using System;

namespace Tabellarius
{
	public class MiddleToolBar : HBox
	{

		public enum DisplayMode
		{
			Text, Programm
		}

		public static MiddleToolBar instance;
		private DisplayMode currMode;

		private MiddleToolBar() : base()
		{
			// Also a hardcodet frameManager
			this.Orientation = Orientation.Vertical;

			currMode = DisplayMode.Programm; // Default start Mode

			var listFrameAdapter = ListFrameAdapter.GetInstance();
			var editFrameAdapter = EditFrameAdapter.GetInstance();
			var mainToolBar = MainToolBar.GetInstance();

			var textButton = MakeHorizontalButton(" Text ");
			textButton.Clicked += delegate
			{
				if (currMode == DisplayMode.Text)
					return;
				currMode = DisplayMode.Text;
				mainToolBar.ChangeMode(currMode);
				if (editFrameAdapter.ChangeMode(currMode)) // Save can fail
					listFrameAdapter.ChangeMode(currMode);
			};

			var programButton = MakeHorizontalButton(" Programm ");
			programButton.Clicked += delegate
			{
				if (this.currMode == DisplayMode.Programm)
					return;
				currMode = DisplayMode.Programm;

				mainToolBar.ChangeMode(currMode);
				editFrameAdapter.ChangeMode(currMode);
				listFrameAdapter.ChangeMode(currMode);
			};

			this.PackStart(textButton, false, false, 0);
			this.PackStart(programButton, false, false, 0);
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