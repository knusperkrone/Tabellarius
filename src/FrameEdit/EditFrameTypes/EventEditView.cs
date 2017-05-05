using System;
using Gtk;
using Tabellarius.Assets;

namespace Tabellarius.EditFrameTypes
{
	public class EventEditView : AbstractEditView
	{

		private readonly TextView textEntry;
		private readonly TextView timeEntry;
		private readonly EventTimeBox timeBox;
		private readonly ComboBox cbLang;
		private readonly ComboBox cbKrzl;
		private readonly string[] availableKrzl;
		private string origText;
		private int origLang, origKrzl;

		protected override bool Init
		{
			get { return this.init; }
			set { this.init = textEntry.Editable = timeBox.IsEditable = value; }
		}

		public EventEditView() : base()
		{
			timeBox = new EventTimeBox();
			textEntry = new TextView();

			cbLang = new ComboBox(API_Contract.SupportedLanguages);

			var cbBox = new HBox();
			cbBox.PackStart(new Label("   Sprache  "), false, false, 0);
			cbBox.PackStart(cbLang, false, false, 0);

			availableKrzl = new string[] { "KC", "TST" };
			cbKrzl = new ComboBox(availableKrzl); // TODO: DatabaseAdaper GetKrzl
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

		public override void EditTreeRow(TreeView treeView, RowActivatedArgs args, object tabData)
		{
			Init = true;

			TreeIter currIter;
			this.currTreeStore = (TreeStore)treeView.Model;
			this.currTreeStore.GetIter(out currIter, args.Path);
			this.currTreeIter = currIter;

			string nameString, krzlString, langString;
			string timeString = (string)currTreeStore.GetValue(currIter, (int)EventColumnId.Zeit);
			int krzlPos, langPos;

			if (IsParent(args)) {
				currParentIter = currIter;

				nameString = (string)currTreeStore.GetValue(currIter, (int)EventColumnId.Name);
				krzlString = (string)currTreeStore.GetValue(currIter, (int)EventColumnId.Krzl);
				langString = (string)currTreeStore.GetValue(currIter, (int)EventColumnId.Sprache);

				textEntry.Editable = true;
				cbLang.Sensitive = cbKrzl.Sensitive = true;
			} else {
				TreeIter parentIter;
				currTreeStore.IterParent(out parentIter, currIter);
				currParentIter = parentIter;

				nameString = "";
				krzlString = (string)currTreeStore.GetValue(currParentIter, (int)EventColumnId.Krzl);
				langString = (string)currTreeStore.GetValue(currParentIter, (int)EventColumnId.Sprache);

				textEntry.Editable = false;
				cbLang.Sensitive = cbKrzl.Sensitive = false;
			}

			// Set new values
			langPos = FindInArray(API_Contract.SupportedLanguages, langString);
			krzlPos = FindInArray(availableKrzl, krzlString);

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
			// TODO: Save on UI
			// TODO: Save on Database
			// TODO: Save on this
			return true;
		}

		protected override bool SaveNecessary()
		{
			return false;
			//return !textEntry.Buffer.Text.Equals(origEntry) || timeBox.IsDirty;
		}

		private static int FindInArray(string[] arr, string val)
		{
			for (int i = 0; i < arr.Length; i++) {
				if (arr[i].Equals(val))
					return i;
			}
			return -1;
		}

	}
}