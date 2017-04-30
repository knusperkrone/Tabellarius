using Gtk;
using System;
using System.Collections.Generic;

namespace Tabellarius.MainToolbar
{
	public class TextToolbar : HBox
	{

		private DatabaseAdapter dbAdapter;
		private ListFrameAdapter listAdapter;
		private EditFrameAdapter editAdapter;
		ComboBox cbCategories;
		string[] currentCategories;


		public TextToolbar() : base()
		{
			dbAdapter = DatabaseAdapter.GetInstance();
			listAdapter = ListFrameAdapter.GetInstance();
			editAdapter = EditFrameAdapter.GetInstance();

			cbCategories = new ComboBox();
			UpdateValues();

			this.PackStart(cbCategories, false, false, 0);
		}

		public void UpdateValues()
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

		public new void Dispose()
		{
			cbCategories.Dispose();
			base.Dispose();
		}

	}
}