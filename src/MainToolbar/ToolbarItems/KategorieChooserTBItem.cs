using Gtk;
using System;

namespace Tabellarius.ToolbarItem
{
	public class KategorieChooserTBItem : HBox, IToolbarItem
	{

		private static readonly DatabaseAdapter dbAdapter = DatabaseAdapter.GetInstance();
		private static readonly ListFrameAdapter listAdapter = ListFrameAdapter.GetInstance();
		private static readonly EditFrameAdapter editAdapter = EditFrameAdapter.GetInstance();
		private static readonly MiddleToolBar midToolbar = MiddleToolBar.GetInstance();
		private ComboBox cbCategories;
		private ToggleButton editCatsButton;
		private string[] currentCategories;


		public KategorieChooserTBItem() : base()
		{
			cbCategories = new ComboBox(); // Empty box
			UpdateValues();

			var fm = FrameManager.GetInstance();

			editCatsButton = new ToggleButton("Edit");
			editCatsButton.Clicked += delegate
			{
				if (editCatsButton.Active)
					fm.ChangeMainFrameMode(DisplayMode.KATEGORIE);
				else
					fm.ChangeMainFrameMode(DisplayMode.TEXTE);
			};

			this.PackStart(cbCategories, false, false, 0);
			this.PackStart(editCatsButton, false, false, 0);
		}

		private void UpdateValues()
		{
			currentCategories = dbAdapter.GetCategoriesNames();

			if (currentCategories.Length != 0) {

				GtkHelper.FillComboBox(cbCategories, currentCategories);

				cbCategories.Active = 0;
				cbCategories.Changed += OnCategorieChanged;

				dbAdapter.Curr_categorie = currentCategories[0];

			} else {
				cbCategories.Clear();
				cbCategories.Changed -= OnCategorieChanged;
				dbAdapter.NoActiveCategorie();
			}
		}

		private void OnCategorieChanged(object sender, EventArgs args)
		{
			dbAdapter.Curr_categorie = currentCategories[cbCategories.Active];
			editAdapter.Refresh();
			listAdapter.Refresh();
		}

		public void DataChanged()
		{
			currentCategories = dbAdapter.GetCategoriesNames();
			int currentIndex = cbCategories.Active;
			GtkHelper.FillComboBox(cbCategories, currentCategories);
			cbCategories.Active = currentIndex;
		}

		public new void Dispose()
		{
			cbCategories.Dispose();
			base.Dispose();
		}

	}
}