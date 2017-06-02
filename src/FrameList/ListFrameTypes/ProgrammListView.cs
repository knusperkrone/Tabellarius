using Gtk;
using Tabellarius.Database;
using Tabellarius.Assets;

namespace Tabellarius.ListFrameTypes
{
	public class ProgrammListView : AbstractListView
	{

		public ProgrammListView() : base() { }

		protected override void PopulateTabView()
		{
			// Adding a Tab with (scrollable) TreeStore for every day
			TreeStore dayQuery = dbAdapter.GetListFrameContentFor(tabs);
			while (dayQuery != null) {
				var tabContent = GenerateTabContent(dayQuery);
				AddTab(tabContent, "Tag " + (tabs + 1));
				dayQuery = dbAdapter.GetListFrameContentFor(tabs);
			}
		}

		public override void AddTab() // Add a day
		{
			// AddDay() does not get saved in the database - just UI
			var emptyStore = new TreeStore(typeof(string), typeof(string));
			var tabContent = GenerateTabContent(emptyStore);
			AddTab(tabContent, "Tag " + tabs + 1);
		}

		public override void AddParentEntry() // Add a Termin
		{
			TimeBox timeBox = new TimeBox(true);
			timeBox.posEntry.Text = "0"; // Default pos
			Entry textEntry = new Entry();
			ComboBox cbType = new ComboBox(API_Contract.ProgrammTerminTypVal);
			cbType.Active = 1; // Default is 'Programm'
			GetUserArgs[] args = new GetUserArgs[] {
				new GetUserArgs(new Label("Text"), textEntry),
				new GetUserArgs(new Label("Sichtbarkeit"), cbType),
			};
			var diag = new GetUserDataDialog(args, timeBox, "Speichern", 0, "Abbruch", 1);

			if (diag.Run() == 0) {
				bool validated;
				while (!(validated = timeBox.ValidateTime())) {
					if (diag.Run() == 1) {
						validated = false;
						break;
					}
				}
				if (validated) { // There is valid user data
					var treeContent = CurrTreeStore;
					string finalTime;
					string tmpTime = timeBox.Time;
					string text = textEntry.Text;
					string typString = GtkHelper.ComboBoxActiveString(cbType);
					int typ = cbType.Active;
					// Save on UI
					TreeIter insertIter;
					insertIter = treeContent.AppendValues(tmpTime, text, typString);
					GtkHelper.SortInByColumn(treeContent, (int)ProgrammColumnID.Uhrzeit, insertIter);
					finalTime = API_Contract.ClearTimeConflicts(treeContent, insertIter);
					// Save on Database
					var insert = new Table_Termin(CurrTabIndex, finalTime, text, typ);
					dbAdapter.InsertEntry(insert);
				}
			}
			// Free Memory
			diag.Destroy();
			foreach (var arg in args) // Free args
				arg.Dispose();
		}

		public override void AddChildEntry() // Add a Beschreibung
		{
			// Is there a active parent?
			TreeIter parentIter = editFrameAdapter.ActiveParentTreeIter;
			if (parentIter.Equals(TreeIter.Zero)) {
				var error = new SafeCallDialog("Kein Element ausgewählt", "Ok", 0, null, 1);
				error.Run();
				error.Destroy();
				return;
			}

			ComboBox cbTyp = new ComboBox(API_Contract.ProgrammDescrTypVal);
			cbTyp.Active = 2; // Default is 'All'
			Entry textEntry = new Entry();
			var args = new GetUserArgs[] {
				new GetUserArgs(new Label("Text"), textEntry),
				new GetUserArgs(new Label("Typ"), cbTyp),
			};
			var diag = new GetUserDataDialog(args, null, "Ok", 0, "Abbruch", 1);

			if (diag.Run() == 0) {
				var treeContent = CurrTreeStore;
				string text = textEntry.Text;
				string typString = GtkHelper.ComboBoxActiveString(cbTyp);
				int typ = cbTyp.Active;
				// Save on UI
				TreeIter insertIter, firstIter;
				insertIter = treeContent.AppendValues(parentIter, "└──", "\t" + text, typString);
				treeContent.IterNthChild(out firstIter, parentIter, 0);
				GtkHelper.SortInByColumn(treeContent, (int)ProgrammColumnID.Text, insertIter);
				// XXX: Save on Database
				string time = (string)treeContent.GetValue(parentIter, (int)ProgrammColumnID.Uhrzeit);
				var insert = new Table_Beschreibung(CurrTabIndex, time, text, typ);
				dbAdapter.InsertEntry(insert);
			}
			// Free Memory
			diag.Destroy();
			foreach (var arg in args)
				arg.Dispose();
		}

		protected override TabContent GenerateTabContent(TreeStore treeContent)
		{
			var tc = RegisterTabContent();

			var timeColumn = new TreeViewColumn(" Uhrzeit ", GenTextCell(), "text", ProgrammColumnID.Uhrzeit);
			var textColumn = new TreeViewColumn("Text", GenTextCell(), "text", ProgrammColumnID.Text);
			textColumn.Resizable = true;
			var typColumn = new TreeViewColumn(" Typ ", GenTextCell(), "text", ProgrammColumnID.Typ);

			tc.tree.AppendColumn(timeColumn);
			tc.tree.AppendColumn(textColumn);
			tc.tree.AppendColumn(typColumn);
			tc.tree.HeadersVisible = true;
			tc.tree.Model = treeContent;
			tc.tree.RowActivated += delegate (object sender, RowActivatedArgs args)
			{
				editFrameAdapter.PassToEditView((TreeView)sender, args, CurrTabIndex);
			};

			return tc;
		}

	}
}