using System;
using Gtk;
using Tabellarius.Assets;
using Tabellarius.Database;

namespace Tabellarius.EditFrameTypes.ListEditView
{
	public class EditEventList : AbstractTextEditList
	{

		private readonly EventTimeBox timeBox;
		private readonly ComboBox cbLang;
		private readonly ComboBox cbKrzl;
		private readonly string[] availKrzl, availLang;
		private int origLangIndex, origKrzlIndex;

		protected override bool HookInit
		{
			set { timeBox.IsEditable = value; }
		}


		public EditEventList() : base()
		{
			// Init UI
			timeBox = new EventTimeBox(-1);

			var cbBox = new HBox();

			availLang = API_Contract.SupportedLanguages;
			cbLang = new ComboBox(availLang);
			cbBox.PackStart(new Label("   Sprache  "), false, false, 0);
			cbBox.PackStart(cbLang, false, false, 0);

			availKrzl = API_Contract.SupportedKrzl;
			cbKrzl = new ComboBox(availKrzl);
			cbBox.PackStart(new Label("  Kürzel  "), false, false, 0);
			cbBox.PackStart(cbKrzl, false, false, 0);

			this.PackStart(timeBox, false, true, 5);
			this.PackStart(cbBox, false, false, 5);

			origKrzlIndex = origLangIndex = -1;
		}

		protected override void HookClear()
		{
			timeBox.Clear();
		}

		protected override void HookEditTreeRow(TreeView treeView, RowActivatedArgs args, object tabData)
		{
			string nameString, krzlString, langString;
			string timeString = (string)currTreeStore.GetValue(currTreeIter, (int)EventColumnId.Zeit);
			int krzlPos, langPos;

			if (IsParent(args)) {
				// Event is parent, just get values
				currParentIter = currTreeIter;

				nameString = (string)currTreeStore.GetValue(currTreeIter, (int)EventColumnId.Name);
				krzlString = (string)currTreeStore.GetValue(currTreeIter, (int)EventColumnId.Krzl);
				langString = (string)currTreeStore.GetValue(currTreeIter, (int)EventColumnId.Sprache);

				textEntryEditable = true;
				cbLang.Sensitive = cbKrzl.Sensitive = true;
			} else {
				// Instanz, is child, get parent, and necessary parent vals
				TreeIter parentIter;
				currTreeStore.IterParent(out parentIter, currTreeIter);
				currParentIter = parentIter;

				nameString = "";
				krzlString = (string)currTreeStore.GetValue(currParentIter, (int)EventColumnId.Krzl);
				langString = (string)currTreeStore.GetValue(currParentIter, (int)EventColumnId.Sprache);

				textEntryEditable = false;
				cbLang.Sensitive = cbKrzl.Sensitive = false;
			}

			// Set new values
			langPos = GtkHelper.FindInArray(API_Contract.SupportedLanguages, langString);
			krzlPos = GtkHelper.FindInArray(availKrzl, krzlString);

			cbKrzl.Active = krzlPos;
			cbLang.Active = langPos;
			timeBox.Time = timeString;
			CurrText = nameString;
			// Set default values
			origKrzlIndex = krzlPos;
			origLangIndex = langPos;
		}

		protected override void HookOnCancel(object sender, EventArgs args)
		{
			timeBox.Reset();
		}

		protected override bool HookOnSave()
		{
			// Assert data integrity
			if (!timeBox.ValidateTime())
				return false;

			// Mutable values
			string validTime = timeBox.Time;
			string currText = CurrText;
			int vId = int.Parse((string)currTreeStore.GetValue(currParentIter, (int)EventColumnId.ID));

			// Save on Database
			DatabaseTable orig, elem;
			if (IsCurrParent) {
				orig = new Table_Veranstaltung(vId, availKrzl[origKrzlIndex], OrigText,
									availLang[origLangIndex], int.Parse(timeBox.OrigTime));
				elem = new Table_Veranstaltung(vId, availKrzl[cbKrzl.Active],
									currText, availLang[cbLang.Active], int.Parse(validTime));
			} else {
				int iId = int.Parse((string)currTreeStore.GetValue(currTreeIter, (int)EventColumnId.ID));
				orig = new Table_Instanz(vId, iId, timeBox.OrigTime);
				elem = new Table_Instanz(vId, iId, validTime);
			}
			orig.Update(elem);

			// Save on UI
			if (IsCurrParent)
				currTreeStore.SetValue(currTreeIter, (int)EventColumnId.Name, currText);
			currTreeStore.SetValue(currTreeIter, (int)EventColumnId.Zeit, validTime);

			// Save on this
			timeBox.Time = validTime;
			origKrzlIndex = cbKrzl.Active;
			origLangIndex = cbLang.Active;
			return true;
		}

		protected override bool HookIsDirty()
		{
			return  origKrzlIndex != cbKrzl.Active
					|| origLangIndex != cbLang.Active
					|| timeBox.IsDirty;
		}

		public override void Dispose() {
			timeBox.Dispose();
			base.Dispose();
		}

	}
}