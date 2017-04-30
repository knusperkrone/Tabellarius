using Gtk;
using System;

namespace Tabellarius
{
	public class MainFrame : Window
	{

		private static MainFrame instance;


		private MainFrame() : base("Tabellarius")
		{
			// TODO: get Display resolution
			SetDefaultSize(900, 600);
			SetPosition(WindowPosition.Center);
			DeleteEvent += new DeleteEventHandler(OnDelete);

			AccelGroup agr = new AccelGroup();

			var mainBox = new VBox(false, 2);
			// Titlebar - mainBox top
			var titleBar = TitleBar.GetInstance(agr);
			// MainToolBar - mainBox middle
			var mainToolBar = MainToolBar.GetInstance();

			// ListFrame - Table left
			var listFrameAdapter = ListFrameAdapter.GetInstance();
			// Toolbar - Table middle
			var middleToolBar = MiddleToolBar.GetInstance();
			// EditFrame - Table right
			var editFrameAdapter = EditFrameAdapter.GetInstance();

			var paneBox = new HBox();
			paneBox.PackStart(listFrameAdapter, true, true, 0);
			paneBox.PackStart(new VSeparator(), false, true, 0);
			paneBox.PackStart(middleToolBar, false, true, 0);
			paneBox.PackStart(new VSeparator(), false, true, 0);
			var paned = new Paned(Orientation.Horizontal);
			paned.Add1(paneBox);
			paned.Add2(editFrameAdapter);
			paned.Position = 500;

			// Pack stuff
			mainBox.PackStart(titleBar, false, true, 0);
			mainBox.PackStart(mainToolBar, false, true, 0);
			mainBox.PackStart(new HSeparator(), false, true, 0);
			mainBox.PackStart(paned, true, true, 0);
			Add(mainBox);
			ShowAll();
		}

		public static MainFrame GetInstance()
		{
			if (instance == null)
				instance = new MainFrame();
			return instance;
		}

		public static void OnDelete(object obj, EventArgs args)
		{
			//EditFrameAdapter.GetInstance().SaveAndExit();
			Application.Quit();
		}

	}
}