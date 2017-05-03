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
			timeBox = new TimeBox(true);

			typLabel = new Label(" Typ");
			cbTyp = new ComboBox(new string[] { "     " });

			var typBox = new HBox();
			typBox.PackStart(typLabel, false, true, 10);
			typBox.PackStart(cbTyp, false, true, 2);

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

			DatabaseTable orig, newElem;
			// TODO: Save on Database
			var origTime = (string)currTreeStore.GetValue(currParentIter, (int)ListColumnID.Uhrzeit);
			if (isTermin) {
				// Save child Nodes
				TreeIter childIter;
				if (currTreeStore.IterChildren(out childIter, currTreeIter)) {
					do {
						var childText = (string)currTreeStore.GetValue(childIter, (int)ListColumnID.Text);
						var childTime = (string)currTreeStore.GetValue(childIter, (int)ListColumnID.Uhrzeit);
						var childTyp = API_Contract.ProgrammDescrTypCR[(string)currTreeStore.GetValue(childIter, (int)ListColumnID.Typ)];
						orig = new Table_Beschreibung(day, childTime, childText, childTyp);
						newElem = new Table_Beschreibung(day, validTime, childText, childTyp);
						dbAdapter.updateEntry(orig, newElem);
					} while (currTreeStore.IterNext(ref childIter));
				}
				orig = new Table_Termin(day, origTime, origText, origTyp);
				newElem = new Table_Termin(day, validTime, databaseString, cbTyp.Active);

			} else {
				orig = new Table_Beschreibung(day, origTime, origText, origTyp);
				newElem = new Table_Beschreibung(day, origTime, databaseString, cbTyp.Active);
			}
			dbAdapter.updateEntry(orig, newElem);

			// Add on UI
			if (!isTermin)
				databaseString = API_Contract.ConvertDatabaseToTreeChild(databaseString);
			currTreeStore.SetValue(currTreeIter, (int)ListColumnID.Text, databaseString);


			if (isTermin) { // Only Parent Nodes can change Time
				currTreeStore.SetValue(currTreeIter, (int)ListColumnID.Uhrzeit, validTime);
				// Set new value, sort it in and update UI valiue
				var iter = GtkHelper.SortInByColumn(currTreeStore, (int)ListColumnID.Uhrzeit, currTreeIter);
				API_Contract.ClearTimeConflicts(currTreeStore, iter);
				timeBox.Time = (string)currTreeStore.GetValue(currTreeIter, (int)ListColumnID.Uhrzeit);
			} else {
				GtkHelper.SortInByColumn(currTreeStore, (int)ListColumnID.Text, currTreeIter);
			}

			// Add on this
			timeBox.Time = validTime;
			origText = textEntry.Buffer.Text;
			origTyp = cbTyp.Active;
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

		public override void EditTreeRow(TreeView treeView, RowActivatedArgs args, string tabName)
		{
			throw new NotImplementedException();
		}
	}
}