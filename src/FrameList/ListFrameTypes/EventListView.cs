using Gtk;
using System;

namespace Tabellarius.ListFrameTypes
{
	public class EventListView : AbstractListView
	{

		public EventListView() { }

		protected override void PopulateTabView()
		{
			tabs = 1;
			var treeWindow = GenScrollableTree(dbAdapter.GetEventContent());
			tabView.AppendPage(treeWindow, new Label("Veranstaltungen"));
		}

		public override void AddChildEntry()
		{
			throw new NotImplementedException();
		}

		public override void AddParentEntry()
		{
			throw new NotImplementedException();
		}

		public override void AddTab()
		{
			return;
		}

		public override void DataSetChanged()
		{
			throw new NotImplementedException();
		}

		protected override ScrolledWindow GenScrollableTree(TreeStore treeContent)
		{
			var scrollWin = new ScrolledWindow();
			scrollWin.ShadowType = ShadowType.EtchedOut;
			scrollWin.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

			var idColumn = new TreeViewColumn("Id", GenTextCell(), "text", EventColumnId.ID);
			var nameColumn = new TreeViewColumn("Name", GenTextCell(), "text", EventColumnId.Name);
			var krzlColumn = new TreeViewColumn("Kürzel", GenTextCell(), "text", EventColumnId.Krzl);
			var langColumn = new TreeViewColumn("Sprache", GenTextCell(), "text", EventColumnId.Sprache);
			var timeColumn = new TreeViewColumn("Zeit", GenTextCell(), "text", EventColumnId.Zeit);

			var tree = new TreeView();
			tree.AppendColumn(idColumn);
			tree.AppendColumn(nameColumn);
			tree.AppendColumn(krzlColumn);
			tree.AppendColumn(langColumn);
			tree.AppendColumn(timeColumn);
			tree.EnableGridLines = TreeViewGridLines.Both;
			tree.HeadersClickable = false;
			tree.HeadersVisible = true;
			tree.Model = treeContent;
			tree.RulesHint = true;
			tree.RowActivated += delegate (object sender, RowActivatedArgs args)
			{
				editFrameAdapter.PassToEditView((TreeView)sender, args, 0);
			};

			this.treeList.Add(tree);
			scrollWin.Add(tree);

			return scrollWin;
		}

	}
}