using System;
using Gtk;
using Tabellarius.Assets;
using Tabellarius.Database;

namespace Tabellarius.EditFrameTypes.ListEditView
{
	public class EditTabCategoryList : AbstractEditList
	{
		private readonly ScrolledWindow scrollText;
		private readonly ComboBox cbTyp;
		private readonly Label categoryLabel;
		private readonly ToggleButton boldButton, italicButton;
		private readonly Button upButton, downButton;
		private readonly TagTextView textEntry;

		private string tabName;
		private int origTyp, origRang;
		private string origDbText, origText;

		protected override bool Init
		{
			set { this.init = textEntry.Editable = value; }
		}


		public EditTabCategoryList() : base()
		{
			// Init UI
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

			textEntry = new TagTextView(ref boldButton, ref italicButton);
			var font = new Pango.FontDescription() { Family = "Droid Sans" };
			textEntry.OverrideFont(font);
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
			tabName = origDbText = origText = "";
		}

		protected override void EditTreeRow(TreeView treeView, RowActivatedArgs args, object tabData)
		{
			this.tabName = (string)tabData;

			string typString = (string)currTreeStore.GetValue(currTreeIter, (int)CategorieColumnID.Typ);
			origDbText = (string)currTreeStore.GetValue(currTreeIter, (int)CategorieColumnID.Text);
			origRang = int.Parse((string)currTreeStore.GetValue(currTreeIter, (int)CategorieColumnID.Rang));
			int activeTyp;

			// Get ParentIter and Type
			if (IsParent(args)) {
				// Parent Node, just get values
				currParentIter = currTreeIter;

				GtkHelper.FillComboBox(cbTyp, API_Contract.CategorieTextTypParentVal);
				activeTyp = API_Contract.CategorieTextParentTypCR[typString];
			} else {
				// Child Node, get parent and values
				TreeIter parentIter;
				currTreeStore.IterParent(out parentIter, currTreeIter);
				currParentIter = parentIter;

				GtkHelper.FillComboBox(cbTyp, API_Contract.CategorieTextTypChildVal);
				activeTyp = API_Contract.CategorieTextChildTypCR[typString];
			}

			// Set new values
			cbTyp.Active = activeTyp;
			textEntry.Buffer.Clear();
			var buff = textEntry.Buffer;
			API_Contract.ConvertDatabaseToEditCategorie(origDbText, ref buff);

			// Set default values
			origText = buff.Text;
			origTyp = activeTyp;
			boldButton.Active = italicButton.Active = false;
		}

		protected override void OnCancel(object sender, EventArgs args)
		{
			cbTyp.Active = origTyp;
			textEntry.Buffer.Clear();
			var buff = textEntry.Buffer;
			API_Contract.ConvertDatabaseToEditCategorie(origDbText, ref buff);
		}

		protected override bool IsDirty()
		{
			return origTyp != cbTyp.Active
					|| !origText.Equals(textEntry.Buffer.Text); ;
		}

		protected override bool OnSave()
		{
			// Mutable values
			string currTypString = GtkHelper.ComboBoxActiveString(cbTyp);
			origDbText = API_Contract.ConvertEditCategorieToDatabse(textEntry.Buffer);

			// Save on Database
			int currRang = int.Parse((string)currTreeStore.GetValue(currTreeIter, (int)CategorieColumnID.Rang));
			DatabaseTable origElem, newElem;
			if (IsCurrParent) {
				int currTyp = API_Contract.CategorieTextParentTypCR[currTypString];
				origElem = new Table_Kategorie_Tab_Titel(tabName, origDbText, origTyp, origRang);
				newElem = new Table_Kategorie_Tab_Titel(tabName, origDbText, currTyp, currRang);
			} else {
				int currTyp = API_Contract.CategorieTextChildTypCR[currTypString];
				string tmpTitel = (string)currTreeStore.GetValue(currParentIter, (int)CategorieColumnID.Text);
				origElem = new Table_Kategorie_Tab_Text(tabName, tmpTitel, this.origDbText, origTyp, origRang);
				newElem = new Table_Kategorie_Tab_Text(tabName, tmpTitel, origDbText, currTyp, currRang);
			}
			origElem.Update(newElem);
			// Save on UI
			currTreeStore.SetValue(currTreeIter, (int)CategorieColumnID.Text, origDbText);
			currTreeStore.SetValue(currTreeIter, (int)CategorieColumnID.Typ, currTypString);
			// Save on this
			origTyp = cbTyp.Active;
			origText = textEntry.Buffer.Text;
			return true;
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

			if (!canChange)
				return; // Nothing to do here!

			// Save on Database
			int changePos = int.Parse((string)currTreeStore.GetValue(changeIter, (int)CategorieColumnID.Rang));
			string changeText = (string)currTreeStore.GetValue(changeIter, (int)CategorieColumnID.Text);
			DatabaseTable orig_Elem, origTmp_Elem, change_Elem, changeTmp_Elem;
			if (IsCurrParent) {
				int changeTyp = API_Contract.CategorieTextParentTypCR[
						(string)currTreeStore.GetValue(changeIter, (int)CategorieColumnID.Typ)];
				orig_Elem = new Table_Kategorie_Tab_Titel(tabName, origDbText, origTyp, currPos);
				origTmp_Elem = new Table_Kategorie_Tab_Titel(tabName, origDbText, origTyp, int.MaxValue);
				change_Elem = new Table_Kategorie_Tab_Titel(tabName, changeText, changeTyp, changePos);
				changeTmp_Elem = new Table_Kategorie_Tab_Titel(tabName, changeText, changeTyp, currPos);
			} else {
				int changeTyp = API_Contract.CategorieTextChildTypCR[
						(string)currTreeStore.GetValue(changeIter, (int)CategorieColumnID.Typ)];
				string tmpTitel = (string)currTreeStore.GetValue(currParentIter, (int)CategorieColumnID.Text);
				orig_Elem = new Table_Kategorie_Tab_Text(tabName, tmpTitel, this.origDbText, origTyp, currPos);
				origTmp_Elem = new Table_Kategorie_Tab_Text(tabName, tmpTitel, this.origDbText, origTyp, int.MaxValue);
				change_Elem = new Table_Kategorie_Tab_Text(tabName, tmpTitel, changeText, changeTyp, changePos);
				changeTmp_Elem = new Table_Kategorie_Tab_Text(tabName, tmpTitel, changeText, changeTyp, currPos);
			}
			// origPos -> Int.Max | changePos -> origPos | origPos -> ChangePos
			orig_Elem.Update(origTmp_Elem);
			change_Elem.Update(changeTmp_Elem);
			if (IsCurrParent)
				((Table_Kategorie_Tab_Titel)orig_Elem).Rang = changePos;
			else
				((Table_Kategorie_Tab_Text)orig_Elem).Rang = changePos;
			origTmp_Elem.Update(orig_Elem);

			// Save on UI
			currTreeStore.SetValue(currTreeIter, (int)CategorieColumnID.Rang, changePos);
			currTreeStore.SetValue(changeIter, (int)CategorieColumnID.Rang, currPos);
			// Sort on UI
			if (sender == upButton)
				GtkHelper.SortInByColumn(currTreeStore, (int)CategorieColumnID.Rang, changeIter);
			else
				GtkHelper.SortInByColumn(currTreeStore, (int)CategorieColumnID.Rang, currTreeIter);
			// Save on this
			origRang = changePos;
		}

		public override void Clear()
		{
			Init = false;
			boldButton.Active = italicButton.Active = false;
			origTyp = cbTyp.Active = -1;
			textEntry.Clear();
			textEntry.Buffer.Clear();
			origDbText = "";
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