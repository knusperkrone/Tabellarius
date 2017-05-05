using Gtk;
using System;
using Tabellarius.MainToolbar;

namespace Tabellarius
{
	public class MainToolBar : HBox
	{

		private static MainToolBar instance;

		private TextToolbar textToolbar = null;

		private MainToolBar() : base()
		{
			var listAdapter = ListFrameAdapter.GetInstance();

			// Setup toolbar (icon) buttons
			Toolbar toolbar = new Toolbar();
			toolbar.ToolbarStyle = ToolbarStyle.Icons;

			ToolButton addDay = new ToolButton(Stock.New);
			addDay.Clicked += delegate { listAdapter.AddTab(); };

			ToolButton addParent = new ToolButton(Stock.Add);
			addParent.Clicked += delegate { listAdapter.AddParentEntry(); };

			ToolButton addChild = new ToolButton(Stock.Convert);
			addChild.Clicked += delegate { listAdapter.AddChildEntry(); };

			toolbar.Insert(addDay, 0);
			toolbar.Insert(addParent, 1);
			toolbar.Insert(addChild, 2);
			toolbar.Insert(new SeparatorToolItem(), 3);

			this.PackStart(toolbar, false, true, 2);
			this.PackStart(new ProgrammToolbar(), false, false, 10);
		}

		public static MainToolBar GetInstance()
		{
			if (instance == null)
				instance = new MainToolBar();
			return instance;
		}

		public void ChangeMode(MiddleToolBar.DisplayMode mode)
		{
			if (textToolbar != null) {
				this.Remove(textToolbar);
				textToolbar.Destroy();
				textToolbar.Dispose();
				textToolbar = null;
			}

			if (mode == MiddleToolBar.DisplayMode.Kategorie) {
				textToolbar = new TextToolbar();
				this.PackStart(textToolbar, false, false, 0);
				textToolbar.ShowAll();
			}
			Console.WriteLine("MainToolBar ChangeMode");
		}

	}
}