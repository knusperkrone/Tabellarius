using Gtk;
using System;
using Tabellarius.Assets;
using Tabellarius.Database;

namespace Tabellarius.EditFrameTypes
{
	public class ProgrammEditView : AbstractEditView
	{

		private TimeBox timeBox;
		private readonly ScrolledWindow scrollText;
		private readonly TextView textEntry;
		private readonly Label typLabel;
		private readonly ComboBox cbTyp;

		private int origTyp;
		private string origText;
		private int day;

		protected override bool Init
		{
			get { return this.init; }
			set { this.init = textEntry.Editable = timeBox.IsEditable = value; }
		}


		public ProgrammEditView() : base()
		{
			// Init UI
			timeBox = new TimeBox(false);

			typLabel = new Label("   Typ");
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

			this.PackStart(timeBox, false, true, 5);
			this.PackStart(typBox, false, true, 5);
			this.PackStart(scrollText, true, true, 5);

			Init = false;
			origText = "";
		}

		protected override void EditTreeRow(TreeView treeView, RowActivatedArgs args, object tabData)
		{
			this.day = (int)tabData;

			string textString = (string)currTreeStore.GetValue(currTreeIter, (int)ProgrammColumnID.Text);
			string typString = (string)currTreeStore.GetValue(currTreeIter, (int)ProgrammColumnID.Typ);
			string timeString;

			if (IsParent(args)) {
				// Parent Node, just set flags
				currParentIter = currTreeIter;
				timeBox.IsEditable = true;
			} else {
				// Child Node, get Parent iter, set flags
				TreeIter parentIter;
				currTreeStore.IterParent(out parentIter, currTreeIter);
				currParentIter = parentIter;

				timeBox.IsEditable = false;
				textString = API_Contract.ConvertTreeViewToEditView(textString);
			}
			// Time is depending from parentIter
			timeString = (string)currTreeStore.GetValue(currParentIter, (int)ProgrammColumnID.Uhrzeit);

			// Set Type Arrays
			if (IsParent(args)) {
				// Termin is parent
				GtkHelper.FillComboBox(cbTyp, API_Contract.ProgrammTerminTypVal);
				cbTyp.Active = API_Contract.ProgrammTerminTypCR[typString];
			} else {
				GtkHelper.FillComboBox(cbTyp, API_Contract.ProgrammDescrTypVal);
				cbTyp.Active = API_Contract.ProgrammDescrTypCR[typString];
			}

			// Set new values
			timeBox.Time = timeString;
			textEntry.Buffer.Clear();
			textEntry.Buffer.Text = textString;
			// Set default values
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
			// Assert data integrity
			if (!timeBox.ValidateTime())
				return false;

			// Mutable values
			string validTime;
			string textString = textEntry.Buffer.Text;
			string origTime = timeBox.OrigTime;
			string typString = GtkHelper.ComboBoxActiveString(cbTyp);
			int typ = cbTyp.Active;

			// Prepare for Database
			DatabaseTable orig, newElem;
			if (IsCurrParent) {
				// Termin, set values, update children in database
				validTime = timeBox.Time;
				// Update children
				TreeIter childIter;
				if (currTreeStore.IterChildren(out childIter, currTreeIter)) {
					do {
						var childText = (string)currTreeStore.GetValue(childIter, (int)ProgrammColumnID.Text);
						var childTime = (string)currTreeStore.GetValue(childIter, (int)ProgrammColumnID.Uhrzeit);
						var childTyp = API_Contract.ProgrammDescrTypCR[(string)currTreeStore.GetValue(childIter, (int)ProgrammColumnID.Typ)];
						orig = new Table_Beschreibung(day, childTime, childText, childTyp);
						newElem = new Table_Beschreibung(day, validTime, childText, childTyp);
						dbAdapter.UpdateEntry(orig, newElem);
					} while (currTreeStore.IterNext(ref childIter));
				}

				orig = new Table_Termin(day, origTime, origText, origTyp);
				newElem = new Table_Termin(day, validTime, textString, typ);
			} else {
				// Descr
				validTime = origTime; // Parent time, we got on EditTreeRow()
				textString = API_Contract.ConvertEditViewToDatabase(textString);

				orig = new Table_Beschreibung(day, origTime, origText, origTyp);
				newElem = new Table_Beschreibung(day, validTime, textString, typ);
			}
			dbAdapter.UpdateEntry(orig, newElem);

			// Save on UI
			currTreeStore.SetValue(currTreeIter, (int)ProgrammColumnID.Uhrzeit, validTime);
			currTreeStore.SetValue(currTreeIter, (int)ProgrammColumnID.Typ, typString);

			// Set UI text value and sort in
			if (IsCurrParent) {
				// Sort by time, and clear conflicts
				currTreeStore.SetValue(currTreeIter, (int)ProgrammColumnID.Text, textString);
				var iter = GtkHelper.SortInByColumn(currTreeStore, (int)ProgrammColumnID.Uhrzeit, currTreeIter);
				validTime = API_Contract.ClearTimeConflicts(currTreeStore, iter);
			} else {
				// Sort by text
				currTreeStore.SetValue(currTreeIter, (int)ProgrammColumnID.Text, API_Contract.ConvertDatabaseToTreeChild(textString));
				GtkHelper.SortInByColumn(currTreeStore, (int)ProgrammColumnID.Text, currTreeIter);
			}

			// Save on this
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
			base.Dispose();
		}

	}
}