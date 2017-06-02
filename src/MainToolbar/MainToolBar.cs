using Gtk;
using Tabellarius.ToolbarItem;

namespace Tabellarius
{
	public class MainToolBar : HBox
	{

		private static MainToolBar instance;

		private KategorieChooserTBItem kategorieItem;
		private ProgrammChooserTBItem programmItem;

		private MainToolBar() : base()
		{
			var listAdapter = ListFrameAdapter.GetInstance();

			// Setup toolbar (icon) buttons
			Toolbar toolbar = new Toolbar();
			toolbar.ToolbarStyle = ToolbarStyle.Icons;

			ToolButton addDay = new ToolButton(Stock.New);
			ToolButton addParent = new ToolButton(Stock.Add);
			ToolButton addChild = new ToolButton(Stock.Convert);
			addDay.Clicked += delegate { listAdapter.AddTab(); };
			addParent.Clicked += delegate { listAdapter.AddParentEntry(); };
			addChild.Clicked += delegate { listAdapter.AddChildEntry(); };

			toolbar.Insert(addDay, 0);
			toolbar.Insert(addParent, 1);
			toolbar.Insert(addChild, 2);
			toolbar.Insert(new SeparatorToolItem(), 3);

			programmItem = new ProgrammChooserTBItem();

			this.PackStart(toolbar, false, true, 2);
			this.PackStart(programmItem, false, false, 10);
		}

		public static MainToolBar GetInstance()
		{
			if (instance == null)
				instance = new MainToolBar();
			return instance;
		}

		public void ChangeMode(DisplayMode mode)
		{
			if (mode == DisplayMode.TEXTE) {
				// Add extra items, if they are not alredy there
				if (kategorieItem == null) {
					kategorieItem = new KategorieChooserTBItem();
					this.PackStart(kategorieItem, false, false, 0);
					kategorieItem.ShowAll();
				}
			} else if (mode != DisplayMode.KATEGORIE
							&& kategorieItem != null) {
				// Free Items
				this.Remove(kategorieItem);
				kategorieItem.Destroy();
				kategorieItem.Dispose();
				kategorieItem = null;
			}
		}

		public void DataChanged()
		{

			if (kategorieItem != null)
				kategorieItem.DataChanged();
			if (programmItem != null)
				programmItem.DataChanged();
		}

	}
}