using System;
using Gtk;
using Tabellarius.Assets;
using Tabellarius.Database;

namespace Tabellarius.EditFrameTypes
{
	public class EventEditView : AbstractEditView
	{

		private readonly TextView textEntry;
		private readonly TextView timeEntry;
		private readonly EventTimeBox timeBox;
		private readonly ComboBox cbLang;
		private readonly ComboBox cbKrzl;
		private readonly string[] availKrzl, availLang;
		private string origText;
		private int origLang, origKrzl;

		protected override bool Init
		{
			get { return this.init; }
			set { this.init = textEntry.Editable = timeBox.IsEditable = value; }
		}

		public EventEditView() : base()
		{
			// Init UI
			timeBox = new EventTimeBox();
			textEntry = new TextView();

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
			this.PackStart(textEntry, true, true, 5);

			origText = "";
		}

		public override void Clear()
		{
			textEntry.Buffer.Clear();
			timeBox.Clear();
		}

		protected override void EditTreeRow(TreeView treeView, RowActivatedArgs args, object tabData)
		{
			// @tabData is not important here
			// Only timeString is obvious here
			string nameString, krzlString, langString;
			string timeString = (string)currTreeStore.GetValue(currTreeIter, (int)EventColumnId.Zeit);
			int krzlPos, langPos;

			if (IsParent(args)) {
				// Event is parent, just get values
				currParentIter = currTreeIter;

				nameString = (string)currTreeStore.GetValue(currTreeIter, (int)EventColumnId.Name);
				krzlString = (string)currTreeStore.GetValue(currTreeIter, (int)EventColumnId.Krzl);
				langString = (string)currTreeStore.GetValue(currTreeIter, (int)EventColumnId.Sprache);

				textEntry.Editable = true;
				cbLang.Sensitive = cbKrzl.Sensitive = true;
			} else {
				// Instanz, is child, get parent, and necessary parent vals
				TreeIter parentIter;
				currTreeStore.IterParent(out parentIter, currTreeIter);
				currParentIter = parentIter;

				nameString = "";
				krzlString = (string)currTreeStore.GetValue(currParentIter, (int)EventColumnId.Krzl);
				langString = (string)currTreeStore.GetValue(currParentIter, (int)EventColumnId.Sprache);

				textEntry.Editable = false;
				cbLang.Sensitive = cbKrzl.Sensitive = false;
			}

			// Set new values
			langPos = GtkHelper.FindInArray(API_Contract.SupportedLanguages, langString);
			krzlPos = GtkHelper.FindInArray(availKrzl, krzlString);

			cbKrzl.Active = krzlPos;
			cbLang.Active = langPos;
			timeBox.Time = timeString;
			textEntry.Buffer.Text = nameString;
			// Set default values
			origKrzl = krzlPos;
			origLang = langPos;
			origText = nameString;
		}

		protected override void OnCancel(object sender, EventArgs args)
		{
			timeBox.Reset();
			textEntry.Buffer.Text = origText;
		}

		protected override bool OnSave()
		{
			// Assert data integrity
			if (!timeBox.ValidateTime())
				return false;

			// Mutable values
			string validTime = timeBox.Time;
			string currText = textEntry.Buffer.Text;
			int vId = (int)currTreeStore.GetValue(currParentIter, (int)EventColumnId.ID);

			// Save on Database
			DatabaseTable orig, elem;
			if (IsCurrParent) {
				orig = new Table_Veranstaltung(vId, availKrzl[origKrzl], origText,
											availLang[origLang], int.Parse(timeBox.OrigTime));
				elem = new Table_Veranstaltung(vId, availKrzl[cbKrzl.Active],
				 							currText, availLang[cbLang.Active], int.Parse(validTime));
			} else {
				int iId = (int)currTreeStore.GetValue(currTreeIter, (int)EventColumnId.ID);
				orig = new Table_Instanz(vId, iId, timeBox.OrigTime);
				elem = new Table_Instanz(vId, iId, validTime);
			}
			dbAdapter.UpdateEntry(orig, elem);

			// Save on UI
			if (IsCurrParent)
				currTreeStore.SetValue(currTreeIter, (int)EventColumnId.Name, currText);
			currTreeStore.SetValue(currTreeIter, (int)EventColumnId.Zeit, validTime);

			// Save on this
			timeBox.Time = validTime;
			origText = currText;
			origKrzl = cbKrzl.Active;
			origLang = cbLang.Active;
			return true;
		}

		protected override bool SaveNecessary()
		{
			return false; // TODO: Make it work
			/*return Init
						&& !textEntry.Buffer.Text.Equals(origText)
						|| origKrzl != cbKrzl.Active
						|| origLang != cbLang.Active
						|| timeBox.IsDirty;*/
		}

	}
}