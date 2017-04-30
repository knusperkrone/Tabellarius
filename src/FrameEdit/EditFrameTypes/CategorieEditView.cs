using System;
using Gtk;

namespace Tabellarius.EditFrameTypes
{
	public class CategoryEditView : AbstractEditView
	{

		private TreeStore currTreeStore { get; set; }

		private readonly ScrolledWindow scrollText;
		private readonly ComboBox cbTyp;
		private readonly Label categoryLabel;
		private readonly Button saveButton;
		private readonly Button cancelButton;
		private readonly TextView textEntry;

		private int origTyp, day;
		private string origText;

		private bool init;
		private bool Init
		{
			get { return this.init; }
			set { this.init = textEntry.Editable = value; }
		}

		public CategoryEditView() : base()
		{
			categoryLabel = new Label("Typ: ");
			cbTyp = new ComboBox(new string[] { "      " });
			var typBox = new HBox();
			typBox.PackEnd(cbTyp, false, true, 5);
			typBox.PackEnd(categoryLabel, false, true, 10);

			textEntry = new TextView(); // Add Tags
			textEntry.Buffer.TagTable.Add(API_Contract.boldTag);
			textEntry.Buffer.TagTable.Add(API_Contract.italicTag);
			textEntry.BorderWidth = 10; // Add some Padding

			scrollText = new ScrolledWindow();
			scrollText.ShadowType = ShadowType.EtchedOut;
			scrollText.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
			scrollText.Add(textEntry);

			saveButton = new Button("Speichern");
			cancelButton = new Button("Zurücksetzen");
			saveButton.Clicked += delegate { OnSave(); }; // CallBack restrictions
			cancelButton.Clicked += OnCancel;
			var buttonBox = new HBox();
			buttonBox.PackStart(saveButton, false, true, 5);
			buttonBox.PackStart(cancelButton, false, true, 2);

			this.PackStart(typBox, false, true, 5);
			this.PackStart(scrollText, true, true, 5);
			this.PackStart(buttonBox, false, true, 5);

			Init = false;
			cbTyp.Active = origTyp = -1;
			origText = "";
		}

		public override void EditTreeRow(TreeView treeView, RowActivatedArgs args, int day)
		{
			Init = true;
			this.day = day;

			currTreeStore = (TreeStore)treeView.Model;
			TreeIter currIter;
			currTreeStore.GetIter(out currIter, args.Path);
			this.currTreeIter = currIter;

			string typString = (string)currTreeStore.GetValue(currIter, (int)TextColumnID.Typ);
			string textString = (string)currTreeStore.GetValue(currIter, (int)TextColumnID.Text);

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
			textEntry.Buffer.Clear();
			var buff = textEntry.Buffer;
			API_Contract.ConvertDatabaseToEditCategorie(textString, ref buff);

			origText = textString;
			origTyp = cbTyp.Active;
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

			// Save on this
			this.origTyp = cbTyp.Active;
			this.origText = dbString;

			// Save on Ui
			currTreeStore.SetValue(currTreeIter, (int)TextColumnID.Typ, cbTyp);
			currTreeStore.SetValue(currTreeIter, (int)TextColumnID.Text, dbString);

			//TODO: Save in database


			return true;
		}

		private bool IsDirty()
		{
			return origTyp != cbTyp.Active;
			//TODO: Check if Text is dirty
		}

		public override void Clear()
		{
			origTyp = cbTyp.Active = -1;
			textEntry.Buffer.Clear();
			origText = "";
			Init = false;
		}

		public override void Dispose()
		{
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