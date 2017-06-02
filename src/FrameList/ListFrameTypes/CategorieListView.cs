using Gtk;
using Tabellarius.Database;

namespace Tabellarius.ListFrameTypes
{
	public class CategorieListView : AbstractListView
	{

		public CategorieListView()
		{
			ShowTabs = false;
		}

		protected override void PopulateTabView()
		{
			TreeStore content = dbAdapter.GetCategorieTabNamesContent();
			var treeView = GenerateTabContent(content);
			AddTab(treeView, "");
		}

		public override void AddChildEntry()
		{
			Entry nameEntry = new Entry();
			GetUserArgs[] args = {
				new GetUserArgs(new Label("Name"), nameEntry),
			};

			var diag = new GetUserDataDialog(args, null, "Ok", 0, "Abbruch", 1);
			if (diag.Run() == 0) {
				var kategorie = new Table_Kategorie(nameEntry.Text);
				dbAdapter.InsertEntry(kategorie);
				CurrTreeStore.AppendValues(nameEntry.Text);
				// TODO:
				//FrameManager.GetInstance().Toolbar.Refresh();
			}

			diag.Destroy();
			foreach (var arg in args)
				arg.Dispose();
		}

		public override void AddParentEntry()
		{
			return; //TODO: Dialog
		}

		public override void AddTab()
		{
			return; //TODO: Dialog
		}

		protected override TabContent GenerateTabContent(TreeStore treeContent)
		{
			var tc = RegisterTabContent();

			var name = new TreeViewColumn("Name", GenTextCell(), "text", 0);

			tc.tree.AppendColumn(name);
			tc.tree.HeadersVisible = false;
			tc.tree.Model = treeContent;
			tc.tree.RowActivated += delegate (object sender, RowActivatedArgs args)
			{
				editFrameAdapter.PassToEditView((TreeView)sender, args, CurrTabIndex);
			};

			return tc;
		}

	}
}