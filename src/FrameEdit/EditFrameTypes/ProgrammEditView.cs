using Gtk;
using System;
using Tabellarius.Assets;
using Tabellarius.Database;

namespace Tabellarius.EditFrameTypes
{
	public class ProgrammEditView : AbstractEditView
	{
		//public override TreeIter currParentIter { get; private set; }
		private TreeStore currTreeStore { get; set; }

		private TimeBox timeBox;
		private readonly ScrolledWindow scrollText;
		private readonly TextView textEntry;
		private readonly Label typLabel;
		private readonly ComboBox cbTyp;
		private readonly Button saveButton, cancelButton;

		private int origTyp;
		private string origText;
		private int day;

		private bool init;
		private bool Init
		{
			get { return this.init; }
			set { this.init = textEntry.Editable = timeBox.IsEditable = value; }
		}


		public ProgrammEditView() : base()
		{
			// Init User Layout
			timeBox = new TimeBox(false);

			cbTyp = new ComboBox(new string[] { "     " });
			typLabel = new Label("Typ");

			var typBox = new HBox();
			typBox.PackEnd(cbTyp, false, true, 2);
			typBox.PackEnd(typLabel, false, true, 10);

			textEntry = new TextView();
			textEntry.BorderWidth = 10; // Add some Padding
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

			this.PackStart(timeBox, false, true, 5);
			this.PackStart(typBox, false, true, 5);
			this.PackStart(scrollText, true, true, 5);
			this.PackStart(buttonBox, false, true, 5);

			Init = false;
			origText = "";
		}

		public override void EditTreeRow(TreeView treeView, RowActivatedArgs args, int day)
		{
			Init = true;
			this.day = day;

			this.currTreeStore = (TreeStore)treeView.Model;
			TreeIter currIter;
			currTreeStore.GetIter(out currIter, args.Path);
			this.currTreeIter = currIter;

			string timeString;
			string textString = (string)currTreeStore.GetValue(currIter, (int)ListColumnID.Text);

			if (args.Path.Depth == 1) { // Parent Node
				currParentIter = currIter;
				timeString = (string)currTreeStore.GetValue(currIter, (int)ListColumnID.Uhrzeit);
			} else { // Child Node
				TreeIter parentIter;
				currTreeStore.IterParent(out parentIter, currIter);
				currParentIter = parentIter;
				// Get Parent to get the value for time
				timeBox.IsEditable = false;
				timeString = (string)currTreeStore.GetValue(parentIter, (int)ListColumnID.Uhrzeit); // Time is only saved in Parent Nodes
				textString = API_Contract.ConvertTreeViewToEditView(textString);
			}

			int val;
			var typString = (string)currTreeStore.GetValue(currIter, (int)ListColumnID.Typ);
			if (API_Contract.ProgrammDescrTypCR.TryGetValue(typString, out val)) {
				GtkHelper.FillComboBox(cbTyp, API_Contract.ProgrammDescrTypVal);
				cbTyp.Active = val;
			} else {
				GtkHelper.FillComboBox(cbTyp, API_Contract.ProgrammTerminTypVal);
				cbTyp.Active = API_Contract.ProgrammTerminTypCR[typString];
			}

			// Set new values
			timeBox.Time = timeString;
			textEntry.Buffer.Clear();
			textEntry.Buffer.Text = textString;

			origText = textString;
			origTyp = cbTyp.Active;
		}

		protected override void OnCancel(object sender, EventArgs args)
		{
			timeBox.Reset();
			textEntry.Buffer.Text = origText;
		}

		protected override bool OnSave()
		{
			if (!timeBox.ValidateTime()) // Time is not valid
				return false;

			string validTime = timeBox.DatabaseTime;
			string databaseString = API_Contract.ConvertEditViewToDatabase(textEntry.Buffer.Text);

			bool isTermin = currTreeIter.Equals(currParentIter);

			if (isTermin && timeBox.IsDirty && currTreeStore.IterHasChild(currTreeIter)) {
				// Save child Nodes
				TreeIter childIter;
				bool hasNext = currTreeStore.IterChildren(out childIter, currTreeIter);
				do {
					//var text = (string)currTreeStore.GetValue(childIter, (int)ListColumnID.Text);
					//var typ = (int)currTreeStore.GetValue(childIter, (int)ListColumnID.Typ);
					//var dbElem = new Table_Beschreibung(day, validTime, text, typ);
					hasNext = currTreeStore.IterNext(ref childIter);
				} while (hasNext);
			}
			//TODO: Save Node

			if (!isTermin) // Add some UI-Candy
				databaseString = API_Contract.ConvertDatabaseToTreeChild(databaseString);
			currTreeStore.SetValue(currTreeIter, (int)ListColumnID.Text, databaseString);

			if (isTermin) { // Only Parent Nodes can change Time
				currTreeStore.SetValue(currTreeIter, (int)ListColumnID.Uhrzeit, validTime);
				var iter = GtkHelper.SortInByColumn(currTreeStore, (int)ListColumnID.Uhrzeit, currTreeIter);
				ClearTimeConflicts(currTreeStore, iter);
			} else {
				GtkHelper.SortInByColumn(currTreeStore, (int)ListColumnID.Text, currTreeIter);
			}

			return true;
		}

		protected override bool SaveNecessary()
		{
			return Init && IsDirty();
		}

		private bool IsDirty()
		{
			return timeBox.IsDirty
					|| !textEntry.Buffer.Text.Equals(origText)
					|| origTyp != cbTyp.Active;
		}

		public override void Clear()
		{
			cbTyp.Active = origTyp = -1;
			timeBox.Clear();
			textEntry.Buffer.Clear();
			origText = "";
			Init = false;
		}

		private void ClearTimeConflicts(TreeStore TreeStore, TreeIter iter)
		{
			//TODO:
		}

		public override void Dispose()
		{
			timeBox.Dispose();
			typLabel.Dispose();
			cbTyp.Dispose();
			textEntry.Dispose();
			scrollText.Dispose();
			saveButton.Dispose();
			cancelButton.Dispose();
			base.Dispose();
		}

	}
}