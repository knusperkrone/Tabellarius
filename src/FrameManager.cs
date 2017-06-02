using System;

namespace Tabellarius
{

	public enum DisplayMode
	{
		KATEGORIE, TEXTE, PROGRAMM, VERANSTALTUNG
	}

	public class FrameManager
	{

		private static FrameManager instance;

		private DatabaseChooser databaseChooser;
		private MainFrame mainFrame;
		private TitleBar titleBar;
		private MainToolBar mainToolBar;
		private MiddleToolBar middleToolBar;
		private ListFrameAdapter listFrameAdapter;
		private EditFrameAdapter editFrameAdapter;
		private DisplayMode currMode;


		public static FrameManager GetInstance()
		{
			if (instance == null)
				instance = new FrameManager();
			return instance;
		}

		public void StartGUI()
		{
			// XXX: Start DatabaseChooser
			DatabaseAdapter.SetDb("backend (copy).db", false);
			//new DatabaseChooser().ShowAll();
			InitMainWindow();
			mainFrame.ShowAll();
		}

		public void ChangeMainFrameMode(DisplayMode mode)
		{
			if (mainFrame == null)
				throw new Exception("No MainFrame inited!");

			if (mode != currMode) {
				// Check if save is possible
				if (!editFrameAdapter.ChangeMode(mode))
					return;
				mainToolBar.ChangeMode(mode); // Has to be first
				listFrameAdapter.ChangeMode(mode);

				currMode = mode;
			}
		}

		private void InitMainWindow()
		{
			currMode = DisplayMode.PROGRAMM; // Default start Mode
											 // TODO: Stop Singletons here!
			mainFrame = MainFrame.GetInstance();
			titleBar = TitleBar.GetInstance();
			mainToolBar = MainToolBar.GetInstance();
			middleToolBar = MiddleToolBar.GetInstance();
			listFrameAdapter = ListFrameAdapter.GetInstance();
			editFrameAdapter = EditFrameAdapter.GetInstance();
		}

		public static Gtk.Window ActiveWindow
		{
			get
			{
				var fm = GetInstance();
				if (fm.databaseChooser != null)
					return fm.databaseChooser;
				return fm.mainFrame;
			}
		}

		public MainToolBar GetMainToolBar()
		{
			if (mainToolBar == null)
				throw new InvalidProgramException("mainToolBar is not inited");
			return mainToolBar;
		}

	}
}