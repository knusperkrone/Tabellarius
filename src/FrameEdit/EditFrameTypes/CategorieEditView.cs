using System;
using Gtk;
using Tabellarius.Assets;
using Tabellarius.Database;

namespace Tabellarius.EditFrameTypes
{
	public class CategoryEditView : AbstractEditView
	{

		private readonly ScrolledWindow scrollText;
		private readonly ComboBox cbTyp;
		private readonly Label categoryLabel;
		private readonly ToggleButton boldButton, italicButton;
		private readonly Button upButton, downButton;
		private readonly TagTextView textEntry;

		private string tabName;
		private int origTyp;
		private string origText;

		protected override bool Init
		{
			get { return this.init; }
			set { this.init = textEntry.Editable = value; }
		}


		public CategoryEditView() : base()
		{
			upButton = new Button("↑");
			downButton = new Button("↓");
			upButton.Clicked += MoveRow;
			downButton.Clicked += MoveRow;

			boldButton = new ToggleButton("");
			italicButton = new ToggleButton("");
			// Label with corresponding Markup
			((Label)boldButton.Child).Markup = "<b>F</b>";
			((Label)italicButton.Child).Markup = "<i>K</i>";

			categoryLabel = new Label(" Typ:");
			cbTyp = new ComboBox(new string[] { "      " });

			var typBox = new HBox();
			typBox.PackStart(new Label(" "), false, false, 0);
			typBox.PackStart(upButton, false, true, 0);
			typBox.PackStart(downButton, false, true, 0);
			typBox.PackStart(new Label(" "), false, false, 0);
			typBox.PackStart(boldButton, false, false, 0);
			typBox.PackStart(italicButton, false, false, 0);
			typBox.PackStart(categoryLabel, false, true, 5);
			typBox.PackStart(cbTyp, false, true, 3);

			textEntry = new TagTextView(boldButton, italicButton);
			var font = new Pango.FontDescription() { Family = "Droid Sans" };
			textEntry.ModifyFont(font);
			textEntry.Buffer.TagTable.Add(API_Contract.boldTag);
			textEntry.Buffer.TagTable.Add(API_Contract.italicTag);
			textEntry.BorderWidth = 4; // Add some Padding

			scrollText = new ScrolledWindow();
			scrollText.ShadowType = ShadowType.EtchedOut;
			scrollText.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
			scrollText.Add(textEntry);

			this.PackStart(typBox, false, true, 5);
			this.PackStart(scrollText, true, true, 5);

			// Init values
			Init = false;
			cbTyp.Active = origTyp = -1;
			tabName = origText = "";
		}

		public override void EditTreeRow(TreeView treeView, RowActivatedArgs args, object tabData)
		{
			Init = true;
			this.tabName = (string)tabData;

			TreeIter currIter;
			this.currTreeStore = (TreeStore)treeView.Model;
			this.currTreeStore.GetIter(out currIter, args.Path);
			this.currTreeIter = currIter;

			string typString = (string)currTreeStore.GetValue(currIter, (int)CategorieColumnID.Typ);
			string textString = (string)currTreeStore.GetValue(currIter, (int)CategorieColumnID.Text);
			int activeTyp;

			// Get ParentIter and Type
			if (IsParent(args)) { // Parent Node
				currParentIter = currIter;

				GtkHelper.FillComboBox(cbTyp, API_Contract.CategorieTextTypParentVal);
				activeTyp = API_Contract.CategorieTextParentTypCR[typString];
			} else { // Child Node
				TreeIter parentIter;
				currTreeStore.IterParent(out parentIter, currIter);
				currParentIter = parentIter;

				GtkHelper.FillComboBox(cbTyp, API_Contract.CategorieTextTypChildVal);
				activeTyp = API_Contract.CategorieTextChildTypCR[typString];
			}

			// Set new values
			cbTyp.Active = activeTyp;
			textEntry.Buffer.Clear();
			var buff = textEntry.Buffer;
			API_Contract.ConvertDatabaseToEditCategorie(textString, ref buff);

			// Set default values
			origText = API_Contract.ConvertEditCategorieToDatabse(buff);
			origTyp = activeTyp;
			boldButton.Active = italicButton.Active = false;
		}

		protected override void OnCancel(object sender, EventArgs args)
		{
			cbTyp.Active = origTyp;
			textEntry.Buffer.Clear();
			var buff = textEntry.Buffer;
			API_Contract.ConvertDatabaseToEditCategorie(origText, ref buff);
		}

		protected override bool SaveNecessary()
		{
			return Init && IsDirty();
		}

		protected override bool OnSave()
		{
			string typ = GtkHelper.ComboBoxActiveString(cbTyp);
			string dbString = API_Contract.ConvertEditCategorieToDatabse(textEntry.Buffer);

			// Save on Database
			string currText = API_Contract.ConvertEditCategorieToDatabse(textEntry.Buffer);
			int currRang = int.Parse((string)currTreeStore.GetValue(currTreeIter, (int)CategorieColumnID.Rang));
			if (IsCurrParent) { // Parent
				SaveParentEntry(currTreeIter, true, currText, cbTyp.Active, currRang);
			} else {
				string tmpTitel = (string)currTreeStore.GetValue(currParentIter, (int)CategorieColumnID.Text);
				SaveChildEntry(currTreeIter, currParentIter, tmpTitel, currText, cbTyp.Active, currRang);
			}

			// Save on this
			this.origTyp = cbTyp.Active;
			this.origText = dbString;

			return true;
		}

		private bool IsDirty()
		{
			return origTyp != cbTyp.Active
					|| !origText.Equals(API_Contract.ConvertEditCategorieToDatabse(textEntry.Buffer));
		}

		public override void Clear()
		{
			Init = false;
			boldButton.Active = italicButton.Active = false;
			origTyp = cbTyp.Active = -1;
			textEntry.Clear();
			textEntry.Buffer.Clear();
			origText = "";
		}

		private void MoveRow(object sender, EventArgs args)
		{   // Moves a Row in the ListView and saves new position
			if (!init)
				return;

			int currPos = int.Parse((string)currTreeStore.GetValue(currTreeIter, (int)CategorieColumnID.Rang));

			TreeIter changeIter = currTreeIter;
			bool canChange;
			if (sender == upButton) // Go up or down
				canChange = currTreeStore.IterPrevious(ref changeIter);
			else // sender == downButton
				canChange = currTreeStore.IterNext(ref changeIter);

			if (canChange) {
				var changePos = int.Parse((string)currTreeStore.GetValue(changeIter, (int)CategorieColumnID.Rang));
				var changeText = (string)currTreeStore.GetValue(changeIter, (int)CategorieColumnID.Text);
				int changeTyp;

				if (IsCurrParent) {
					changeTyp = API_Contract.CategorieTextParentTypCR[
							(string)currTreeStore.GetValue(changeIter, (int)CategorieColumnID.Typ)];
					SaveParentEntry(currTreeIter, false, origText, origTyp, int.MaxValue); // tmp
					SaveParentEntry(changeIter, false, changeText, changeTyp, currPos);
					SaveParentEntry(currTreeIter, false, origText, origTyp, changePos);
				} else {
					changeTyp = API_Contract.CategorieTextChildTypCR[
							(string)currTreeStore.GetValue(changeIter, (int)CategorieColumnID.Typ)];
					string tmpTitel = (string)currTreeStore.GetValue(currParentIter, (int)CategorieColumnID.Text);
					SaveChildEntry(currTreeIter, currParentIter, tmpTitel, origText, origTyp, int.MaxValue); //tmp
					SaveChildEntry(changeIter, currParentIter, tmpTitel, changeText, changeTyp, currPos);
					SaveChildEntry(currTreeIter, currParentIter, tmpTitel, origText, origTyp, changePos);
				}

				if (sender == upButton)
					GtkHelper.SortInByColumn(currTreeStore, (int)CategorieColumnID.Rang, changeIter);
				else
					GtkHelper.SortInByColumn(currTreeStore, (int)CategorieColumnID.Rang, currTreeIter);

			}
		}

		private void SaveParentEntry(TreeIter iter, bool childs, string elemText, int elemTyp, int elemRang)
		{
			var origRang = int.Parse((string)currTreeStore.GetValue(iter, (int)CategorieColumnID.Rang));
			var origText = (string)currTreeStore.GetValue(iter, (int)CategorieColumnID.Text);
			var origTyp = API_Contract.CategorieTextParentTypCR[(string)currTreeStore.GetValue(iter, (int)CategorieColumnID.Typ)];
			// Database
			var orig = new Table_Kategorie_Tab_Titel(tabName, origText, origTyp, origRang);
			var elem = new Table_Kategorie_Tab_Titel(tabName, elemText, elemTyp, elemRang);
			if (childs)
				SaveChildEntrysFor(iter, elemText);
			dbAdapter.UpdateEntry(orig, elem);
			// UI
			var elemListTyp = API_Contract.CategorieTextParentTypHR[elemTyp];
			currTreeStore.SetValue(iter, (int)CategorieColumnID.Rang, elemRang);
			currTreeStore.SetValue(iter, (int)CategorieColumnID.Typ, elemListTyp);
			currTreeStore.SetValue(iter, (int)CategorieColumnID.Text, elemText);
		}

		private void SaveChildEntrysFor(TreeIter parent, string titel)
		{
			Console.WriteLine("\nTitel " + titel + '\n');
			TreeIter child;
			bool hasNext = currTreeStore.IterChildren(out child, parent);
			while (hasNext) {
				var childText = (string)currTreeStore.GetValue(child, (int)CategorieColumnID.Text);
				var childTyp = API_Contract.CategorieTextChildTypCR[(string)currTreeStore.GetValue(child, (int)CategorieColumnID.Typ)];
				var childRang = int.Parse((string)currTreeStore.GetValue(child, (int)CategorieColumnID.Rang));
				SaveChildEntry(child, parent, titel, childText, childTyp, childRang);
				hasNext = currTreeStore.IterNext(ref child);
			}
		}

		private void SaveChildEntry(TreeIter iter, TreeIter parent, string newTitel, String elemText, int elemTyp, int elemRang)
		{
			var origRang = int.Parse((string)currTreeStore.GetValue(iter, (int)CategorieColumnID.Rang));
			var origTitel = (string)currTreeStore.GetValue(parent, (int)CategorieColumnID.Text);
			var origText = (string)currTreeStore.GetValue(iter, (int)CategorieColumnID.Text);
			var origTyp = API_Contract.CategorieTextChildTypCR[(string)currTreeStore.GetValue(iter, (int)CategorieColumnID.Typ)];
			// Database
			var orig = new Table_Kategorie_Tab_Text(tabName, origTitel, origText, origTyp, origRang);
			var elem = new Table_Kategorie_Tab_Text(tabName, newTitel, elemText, elemTyp, elemRang);
			dbAdapter.UpdateEntry(orig, elem);
			// UI
			var elemListTyp = API_Contract.CategorieTextChildTypHR[elemTyp];
			currTreeStore.SetValue(iter, (int)CategorieColumnID.Rang, elemRang);
			currTreeStore.SetValue(iter, (int)CategorieColumnID.Typ, elemListTyp);
			currTreeStore.SetValue(iter, (int)CategorieColumnID.Text, elemText);
		}

		public override void Dispose()
		{
			boldButton.Dispose();
			italicButton.Dispose();
			cbTyp.Dispose();
			textEntry.Dispose();
			scrollText.Dispose();
			categoryLabel.Dispose();
			saveButton.Dispose();
			cancelButton.Dispose();
			base.Dispose();
		}

	}
}