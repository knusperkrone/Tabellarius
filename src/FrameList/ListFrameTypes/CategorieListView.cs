using Gtk;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Tabellarius.ListFrameTypes
{
	public class CategorieListView : AbstractListView
	{

		private DatabaseAdapter dbAdapter;
		private EditFrameAdapter editFrameAdapter;
		private string[] tabNames;

		public CategorieListView()
		{
			this.SetPolicy(PolicyType.Automatic, PolicyType.Never);

			dbAdapter = DatabaseAdapter.GetInstance();
			editFrameAdapter = EditFrameAdapter.GetInstance();

			tabView = new Notebook();
			treeList = new List<TreeView>();

			PopulateTabView();

			AddWithViewport(tabView);
		}

		private void PopulateTabView()
		{
			tabNames = dbAdapter.GetCategorieTabNames();
			this.tabs = tabNames.Length;

			for (int i = 0; i < tabNames.Length; i++) {
				var treeStore = dbAdapter.GetTextFrameContentFor(i);
				var treeView = GenScrollableTree(treeStore);
				tabView.AppendPage(treeView, new Label(tabNames[i]));
			}
		}

		public override void AddTab()
		{
			Entry nameEntry = new Entry();

			GetUserArgs[] args = {
				new GetUserArgs(new Label("Titel"), nameEntry) ,
			};
			var diag = new GetUserDataDialog(args, "Speichern", 0, "Abbrechen", 1);
			var ret = diag.Run();
			diag.Destroy();
			if (ret == 1)
				return;

			var emptyStore = new TreeStore(typeof(int), typeof(string), typeof(string));
			var tabContent = GenScrollableTree(emptyStore);
			tabs++;
			tabView.AppendPage(tabContent, new Label(nameEntry.Text));

			// TODO: Save on Database

			tabView.ShowAll();
		}
		public override void AddParentEntry()
		{
			if (tabView.CurrentPage == -1)
				return;

			Entry nameEntry = new Entry();

			var cbTyp = new ComboBox(API_Contract.CategorieTextTypParentVal);
			cbTyp.Active = 0;
			GetUserArgs[] args = {
					new GetUserArgs(new Label("Titel"), nameEntry) ,
					new GetUserArgs(new Label("Typ"), cbTyp) ,
				};
			var diag = new GetUserDataDialog(args, "Speichern", 0, "Abbrechen", 1);
			if (diag.Run() == 0) {
				string text = nameEntry.Text;
				string typ = GtkHelper.ComboBoxActiveString(cbTyp);

				var treeContent = (TreeStore)treeList[tabView.CurrentPage].Model;
				TreeIter firstIt;
				treeContent.GetIterFirst(out firstIt);
				int rows = treeContent.IterNChildren(firstIt);
				var inIter = treeContent.AppendValues(1, typ, text);
				// TODO:  Save in Database

			}
			diag.Destroy();
			foreach (var arg in args)
				arg.Dispose();
		}

		public override void AddChildEntry()
		{
			if (tabView.CurrentPage == -1)
				return;

			// TODO:

			// TODO: Save on Database

		}

		protected override ScrolledWindow GenScrollableTree(TreeStore treeContent)
		{
			var scrollWin = new ScrolledWindow();
			scrollWin.ShadowType = ShadowType.EtchedOut;
			scrollWin.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

			var rangColumn = new TreeViewColumn("Rang", GenTextCell(), "text", CategorieColumnID.Rang);
			var timeColumn = new TreeViewColumn("Typ", GenTextCell(), "text", CategorieColumnID.Typ);
			var textColumn = new TreeViewColumn("Text", GenTextCell(), "text", CategorieColumnID.Text);

			var tree = new TreeView();
			tree.AppendColumn(rangColumn);
			tree.AppendColumn(timeColumn);
			tree.AppendColumn(textColumn);
			tree.EnableGridLines = TreeViewGridLines.Both;
			tree.HeadersClickable = false;
			tree.HeadersVisible = true;
			tree.Model = treeContent;
			tree.RulesHint = true;
			tree.RowActivated += delegate (object sender, RowActivatedArgs args)
			{
				if (tabView.CurrentPage < tabNames.Length)
					editFrameAdapter.PassToEditView((TreeView)sender, args, tabNames[tabView.CurrentPage]);
			};

			this.treeList.Add(tree);

			scrollWin.Add(tree);
			return scrollWin;
		}

		public override void DataSetChanged()
		{
			// Clear everything out
			for (uint i = 0; i < tabs; i++) {
				tabView.RemovePage(0);
			}
			tabs = 0;

			foreach (TreeView tree in treeList)
				tree.Dispose();
			treeList.Clear();

			// Repopulate
			PopulateTabView();
			tabView.ShowAll();
		}

		public override void Dispose()
		{
			foreach (TreeView tree in treeList)
				tree.Dispose();
			tabView.Dispose();
			base.Dispose();
		}

		private static CellRendererText GenTextCell()
		{
			var cell = new CellRendererText();
			cell.Editable = false;
			cell.SetPadding(5, 8);
			return cell;
		}

	}
}