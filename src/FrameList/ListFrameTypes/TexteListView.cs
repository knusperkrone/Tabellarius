using Gtk;
using Tabellarius.Database;

namespace Tabellarius.ListFrameTypes
{
	public class TextsListView : AbstractListView
	{

		private string[] tabNames;

		public TextsListView() : base() { }

		protected override void PopulateTabView()
		{
			tabNames = dbAdapter.GetCategorieTabNames();

			for (int i = 0; i < tabNames.Length; i++) {
				var treeStore = dbAdapter.GetTextFrameContentFor(i);
				var tabContent = GenerateTabContent(treeStore);
				AddTab(tabContent, tabNames[i]);
			}
		}

		public override void AddTab()
		{
			Label nameLabel = new Label("Titel");
			Entry nameEntry = new Entry();
			GetUserArgs[] args = {
				new GetUserArgs(nameLabel, nameEntry),
			};
			var diag = new GetUserDataDialog(args, null, "Speichern", 0, "Abbrechen", 1);

			if (diag.Run() == 0 && AssertNotEmpty(diag, nameEntry)) {
				string text = nameEntry.Text;
				// Save on UI
				var treeStore = new TreeStore(typeof(int), typeof(string), typeof(string));
				var tabContent = GenerateTabContent(treeStore);
				AddTab(tabContent, text);
				// Save on Database
				dbAdapter.InsertEntry(new Table_Kategorie_Tab(text, tabs));
			}
			diag.Destroy();
		}
		public override void AddParentEntry()
		{
			if (CurrTabIndex == -1)
				return;

			Entry nameEntry = new Entry();
			var cbTyp = new ComboBox(API_Contract.CategorieTextTypParentVal);
			cbTyp.Active = 0;
			GetUserArgs[] args = {
					new GetUserArgs(new Label("Titel"), nameEntry),
					new GetUserArgs(new Label("Typ"), cbTyp),
			};
			var diag = new GetUserDataDialog(args, null, "Speichern", 0, "Abbrechen", 1);

			if (diag.Run() == 0 && AssertNotEmpty(diag, nameEntry)) {
				var treeContent = CurrTreeStore;
				string text = nameEntry.Text;
				string typString = GtkHelper.ComboBoxActiveString(cbTyp);
				int typ = API_Contract.CategorieTextParentTypCR[typString];
				int row;
				TreeIter firstIt;
				if (treeContent.GetIterFirst(out firstIt)) { // Has childs
					var lastIt = GtkHelper.GetLastIter(treeContent, firstIt);
					row = int.Parse((string)treeContent.GetValue(lastIt, (int)CategorieColumnID.Rang));
					row += 1;
				} else { // Is empty
					row = 0;
				}
				// Save on UI
				treeContent.AppendValues(row, typString, text);
				// Save in Database
				var insert = new Table_Kategorie_Tab_Titel(tabNames[CurrTabIndex],
															text, typ, row);
				dbAdapter.InsertEntry(insert);
			}
			// Free memory
			diag.Destroy();
			foreach (var arg in args)
				arg.Dispose();
		}

		public override void AddChildEntry()
		{
			TreeIter parentIter = editFrameAdapter.ActiveParentTreeIter;
			if (CurrTabIndex == -1 || parentIter.Equals(TreeIter.Zero))
				return;

			ComboBox cbTyp = new ComboBox(API_Contract.CategorieTextTypChildVal);
			cbTyp.Active = 0; // Text is default
			Entry textEntry = new Entry();
			GetUserArgs[] args = {
				new GetUserArgs(new Label("Typ"), cbTyp),
				new GetUserArgs(new Label("Text"), textEntry),
			};
			var diag = new GetUserDataDialog(args, null, "Ok", 0, "Abburch", 1);

			if (diag.Run() == 0 && AssertNotEmpty(diag, textEntry)) {
				var treeContent = CurrTreeStore;
				string text = textEntry.Text;
				string typString = GtkHelper.ComboBoxActiveString(cbTyp);
				int typ = API_Contract.CategorieTextChildTypCR[typString];
				int row;
				TreeIter firstIt; // Get row
				if (treeContent.IterChildren(out firstIt, parentIter)) {
					var lastIt = GtkHelper.GetLastIter(treeContent, firstIt);
					row = int.Parse((string)treeContent.GetValue(lastIt, (int)CategorieColumnID.Rang));
					row += 1;
				} else {
					row = 0;
				}
				// Save on UI
				treeContent.AppendValues(parentIter, row, typString, text);
				// Save on Database
				var titelName = (string)treeContent.GetValue(parentIter, (int)CategorieColumnID.Text);
				var insert = new Table_Kategorie_Tab_Text(tabNames[CurrTabIndex],
															titelName, text, typ, row);
				dbAdapter.InsertEntry(insert);
			}
			// Free memory
			diag.Destroy();
			foreach (var arg in args)
				arg.Dispose();
		}

		private static bool AssertNotEmpty(GetUserDataDialog diag, Entry textEntry)
		{
			while ((textEntry.Text.Length == 0)) { // Invalid input
				textEntry.OverrideColor(0, API_Contract.invalidColor); // Show problem
				if (diag.Run() == 1) // User aborts
					return false;
			}
			return true;
		}

		protected override TabContent GenerateTabContent(TreeStore treeContent)
		{
			var tc = RegisterTabContent();

			var rangColumn = new TreeViewColumn("Rang", GenTextCell(), "text", CategorieColumnID.Rang);
			var timeColumn = new TreeViewColumn("Typ", GenTextCell(), "text", CategorieColumnID.Typ);
			var textColumn = new TreeViewColumn("Text", GenTextCell(), "text", CategorieColumnID.Text);

			tc.tree.AppendColumn(rangColumn);
			tc.tree.AppendColumn(timeColumn);
			tc.tree.AppendColumn(textColumn);
			tc.tree.Model = treeContent;
			tc.tree.RowActivated += delegate (object sender, RowActivatedArgs args)
			{
				if (CurrTabIndex < tabNames.Length)
					editFrameAdapter.PassToEditView((TreeView)sender, args, tabNames[CurrTabIndex]);
			};

			return tc;
		}

	}
}