using Tabellarius.Database;

namespace Tabellarius.EditFrameTypes.TabEditView
{
	public class EditCategorieTab : AbstractEditTab
	{

		public EditCategorieTab() : base()
		{
			textEntry.Editable = true;
			//TODO: Make rang Editable!
		}

		protected override bool OnSave()
		{
			string newTabName = textEntry.Buffer.Text;
			// Update on Database
			var origTab = new Table_Kategorie_Tab(origTabName, currHeader.rang);
			var newTab = new Table_Kategorie_Tab(newTabName, currHeader.rang);
			origTab.Update(newTab);
			// Update on using
			currHeader.textLabel.Text = newTabName;
			// Update on this
			origTabName = textEntry.Buffer.Text;
			return true;
		}
	}
}