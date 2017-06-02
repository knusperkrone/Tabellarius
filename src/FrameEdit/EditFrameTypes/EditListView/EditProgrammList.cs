using Gtk;
using System;
using Tabellarius.Assets;
using Tabellarius.Database;

namespace Tabellarius.EditFrameTypes.ListEditView
{
	public class EditProgrammList : AbstractTextEditList
	{

		private TimeBox timeBox;
		private readonly Label typLabel;
		private readonly ComboBox cbTyp;

		private int origTyp;
		private int day;

		protected override bool HookInit
		{
			set { timeBox.IsEditable = value; }
		}


		public EditProgrammList() : base()
		{
			// Init UI
			timeBox = new TimeBox(false);

			typLabel = new Label("   Typ");
			cbTyp = new ComboBox(new string[] { "     " });

			var typBox = new HBox();
			typBox.PackStart(typLabel, false, true, 10);
			typBox.PackStart(cbTyp, false, true, 2);

			this.PackStart(timeBox, false, true, 5);
			this.PackStart(typBox, false, true, 5);

			Init = false;
		}

		protected override void HookEditTreeRow(TreeView treeView, RowActivatedArgs args, object tabData)
		{
			this.day = (int)tabData;

			CurrText = (string)currTreeStore
							.GetValue(currTreeIter, (int)ProgrammColumnID.Text);
			string typString = (string)currTreeStore
							.GetValue(currTreeIter, (int)ProgrammColumnID.Typ);
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
				CurrText = API_Contract.ConvertTreeViewToEditView(CurrText);
			}
			// Time is depending from parentIter
			timeString = (string)currTreeStore
							.GetValue(currParentIter, (int)ProgrammColumnID.Uhrzeit);

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
			// Set default values
			origTyp = cbTyp.Active;
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
			string validTime;
			string textString = CurrText;
			string origTime = timeBox.OrigTime;
			string typString = GtkHelper.ComboBoxActiveString(cbTyp);
			int typ = cbTyp.Active;

			// Save on Database
			DatabaseTable orig, newElem;
			if (IsCurrParent) {
				validTime = timeBox.Time;
				orig = new Table_Termin(day, origTime, OrigText, origTyp);
				newElem = new Table_Termin(day, validTime, textString, typ);
			} else {
				validTime = origTime; // Parent time, we got on EditTreeRow()
				textString = API_Contract.ConvertEditViewToDatabase(textString);
				orig = new Table_Beschreibung(day, origTime, OrigText, origTyp);
				newElem = new Table_Beschreibung(day, validTime, textString, typ);
			}
			orig.Update(newElem);

			// Save on UI
			if (IsCurrParent) // Only Parent time can change
				currTreeStore.SetValue(currTreeIter, (int)ProgrammColumnID.Uhrzeit, validTime);
			currTreeStore.SetValue(currTreeIter, (int)ProgrammColumnID.Typ, typString);

			// Set UI text value and sort in
			if (IsCurrParent) {
				// Sort by time, and clear conflicts
				currTreeStore.SetValue(currTreeIter, (int)ProgrammColumnID.Text, textString);
				var iter = GtkHelper.SortInByColumn(currTreeStore,
									(int)ProgrammColumnID.Uhrzeit, currTreeIter);
				validTime = API_Contract.ClearTimeConflicts(currTreeStore, iter);
			} else {
				// Sort by text
				currTreeStore.SetValue(currTreeIter, (int)ProgrammColumnID.Text,
								API_Contract.ConvertDatabaseToTreeChild(textString));
				GtkHelper.SortInByColumn(currTreeStore,
									(int)ProgrammColumnID.Text, currTreeIter);
			}

			// Save on this
			timeBox.Time = validTime;
			origTyp = cbTyp.Active;
			return true;
		}

		protected override bool HookIsDirty()
		{
			return timeBox.IsDirty
					|| origTyp != cbTyp.Active;
		}

		protected override void HookClear()
		{
			cbTyp.Active = origTyp = -1;
			timeBox.Clear();
			Init = false;
		}

		public override void Dispose()
		{
			timeBox.Dispose();
			typLabel.Dispose();
			cbTyp.Dispose();
			base.Dispose();
		}

	}
}