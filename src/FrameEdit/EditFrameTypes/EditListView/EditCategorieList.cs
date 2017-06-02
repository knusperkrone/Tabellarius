using Gtk;
using System;
using Tabellarius.Database;

namespace Tabellarius.EditFrameTypes.ListEditView
{
	public class EditCategorieList : AbstractTextEditList
	{

		protected override bool HookInit { set { return; } }

		public EditCategorieList()
		{
			Label textLabel = new Label("Name");
			this.PackStart(textLabel, false, false, 2);
		}

		protected override void HookEditTreeRow(TreeView treeView, RowActivatedArgs args, object tabData)
		{
			string newText = (string)currTreeStore.GetValue(currTreeIter, 0);
			CurrText = newText;
		}

		protected override void HookOnCancel(object sender, EventArgs args) { return; }

		protected override bool HookOnSave()
		{
			if (!AssertValidInput()) {
				return false;
			}
			string newText = CurrText;
			// Save on Database
			Table_Kategorie origElem, newElem;
			origElem = new Table_Kategorie(OrigText);
			newElem = new Table_Kategorie(newText);
			origElem.Update(newElem);
			//Save on UI
			FrameManager.GetInstance().GetMainToolBar().DataChanged();
			CurrText = newText; // DataChanged() kills buffer cache
			currTreeStore.SetValue(currTreeIter, 0, newText);
			// Save on this -> base
			return true;
		}

		private bool AssertValidInput() { return CurrText.Length != 0; }

		protected override void HookClear() { return; }

		protected override bool HookIsDirty() { return false; }

	}
}