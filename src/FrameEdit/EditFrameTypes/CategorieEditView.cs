using System;
using Gtk;
using Tabellarius.Assets;
using Tabellarius.Database;

namespace Tabellarius.EditFrameTypes
{
	public class CategoryEditView : AbstractEditView
	{

		private TreeStore currTreeStore { get; set; }

		private readonly ScrolledWindow scrollText;
		private readonly ComboBox cbTyp;
		private readonly Label categoryLabel;
		private readonly ToggleButton boldButton, italicButton;
		private readonly Button upButton, downButton;
		private readonly Button saveButton, cancelButton;
		private readonly TagTextView textEntry;

		private string tabName;
		private int origTyp;
		private string origText;

		private bool init;
		private bool Init
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

			saveButton = new Button("Speichern");
			cancelButton = new Button("Zurücksetzen");
			saveButton.Clicked += delegate { OnSave(); };
			cancelButton.Clicked += OnCancel;
			var buttonBox = new HBox();
			buttonBox.PackStart(saveButton, false, true, 5);
			buttonBox.PackStart(cancelButton, false, true, 2);
			this.PackStart(typBox, false, true, 5);
			this.PackStart(scrollText, true, true, 5);
			this.PackStart(buttonBox, false, true, 5);

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

			// Get ParentIter and Type
			if (args.Path.Depth == 1) { // Parent Node
				GtkHelper.FillComboBox(cbTyp, API_Contract.CategorieTextTypParentVal);
				cbTyp.Active = API_Contract.CategorieTextParentTypCR[typString];

				currParentIter = currIter;
			} else { // Child Node
				GtkHelper.FillComboBox(cbTyp, API_Contract.CategorieTextTypChildVal);
				cbTyp.Active = API_Contract.CategorieTextChildTypCR[typString];

				TreeIter parentIter;
				currTreeStore.IterParent(out parentIter, currIter);
				currParentIter = parentIter;
			}

			// Set new values
			textEntry.Clear();
			textEntry.Buffer.Clear();
			var buff = textEntry.Buffer;
			API_Contract.ConvertDatabaseToEditCategorie(textString, ref buff);

			origText = textString;
			origTyp = cbTyp.Active;
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
			DatabaseTable orig, elem;
			if (currParentIter.Equals(currTreeIter)) { // Parent
				// Adjust childs
				TreeIter child;
				if (currTreeStore.IterChildren(out child, currTreeIter)) {
					do {
						var childText = (string)currTreeStore.GetValue(child, (int)CategorieColumnID.Text);
						int childTyp = API_Contract.CategorieTextChildTypCR[(string)currTreeStore.GetValue(child, (int)CategorieColumnID.Typ)];
						int childRang = int.Parse((string)currTreeStore.GetValue(child, (int)CategorieColumnID.Rang));
						orig = new Table_Kategorie_Tab_Text(tabName, origText, childText, childTyp, childRang);
						elem = new Table_Kategorie_Tab_Text(tabName, currText, childText, childTyp, childRang);
						dbAdapter.UpdateEntry(orig, elem);
					} while (currTreeStore.IterNext(ref child));
				}
				orig = new Table_Kategorie_Tab_Titel(tabName, origText, origTyp, currRang);
				elem = new Table_Kategorie_Tab_Titel(tabName, currText, cbTyp.Active, currRang);

			} else {
				var titelName = (string)currTreeStore.GetValue(currParentIter, (int)CategorieColumnID.Text);
				orig = new Table_Kategorie_Tab_Text(tabName, titelName, origText, origTyp, currRang);
				elem = new Table_Kategorie_Tab_Text(tabName, titelName, currText, cbTyp.Active, currRang);
			}
			dbAdapter.UpdateEntry(orig, elem);

			// Save on Ui
			currTreeStore.SetValue(currTreeIter, (int)CategorieColumnID.Typ, cbTyp);
			currTreeStore.SetValue(currTreeIter, (int)CategorieColumnID.Text, dbString);

			// Save on this
			this.origTyp = cbTyp.Active;
			this.origText = dbString;

			return true;
		}

		private bool IsDirty()
		{
			return origTyp != cbTyp.Active;
			//TODO: Check if Text is dirty
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

			int rangID = (int)CategorieColumnID.Rang;
			string pos = (string)currTreeStore.GetValue(currTreeIter, rangID);

			TreeIter changeIter = currTreeIter;
			bool canChange;
			if (sender == upButton) // Go up or down
				canChange = currTreeStore.IterPrevious(ref changeIter);
			else // sender == downButton
				canChange = currTreeStore.IterNext(ref changeIter);

			if (canChange) {
				// Change on UI
				string beforePos = (string)currTreeStore.GetValue(changeIter, rangID);
				currTreeStore.SetValue(currTreeIter, rangID, beforePos);
				currTreeStore.SetValue(changeIter, rangID, pos);
				GtkHelper.SortInByColumn(currTreeStore, rangID, currTreeIter);

				//Save in Database
				bool isParent = currTreeIter.Equals(currParentIter);
				Table_Kategorie_Tab_Titel orig, newElem;

				// Get values from ListView
				var currTitel = (string)currTreeStore.GetValue(currParentIter, (int)CategorieColumnID.Text);
				var currText = (string)currTreeStore.GetValue(currTreeIter, (int)CategorieColumnID.Text);
				int currTyp;
				if (isParent)
					currTyp = API_Contract.CategorieTextParentTypCR[(string)currTreeStore.GetValue(currTreeIter, (int)CategorieColumnID.Typ)];
				else
					currTyp = API_Contract.CategorieTextChildTypCR[(string)currTreeStore.GetValue(currTreeIter, (int)CategorieColumnID.Typ)];
				orig = new Table_Kategorie_Tab_Titel(tabName, currTitel, currTyp, int.Parse(pos));
				newElem = new Table_Kategorie_Tab_Titel(tabName, currTitel, currTyp, int.Parse(beforePos));
				dbAdapter.UpdateEntry(orig, newElem);

				// Get only necessary values from ListView
				currText = (string)currTreeStore.GetValue(changeIter, (int)CategorieColumnID.Text);
				if (isParent)
					currTyp = API_Contract.CategorieTextParentTypCR[(string)currTreeStore.GetValue(currTreeIter, (int)CategorieColumnID.Typ)];
				else
					currTyp = API_Contract.CategorieTextChildTypCR[(string)currTreeStore.GetValue(currTreeIter, (int)CategorieColumnID.Typ)];
				orig.Titel = newElem.Titel = currText;
				orig.Typ = newElem.Typ = currTyp;
				orig.Rang = int.Parse(beforePos);
				newElem.Rang = int.Parse(pos);
				dbAdapter.UpdateEntry(orig, newElem);
			}
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