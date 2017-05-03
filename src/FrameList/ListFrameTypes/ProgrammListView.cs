using Gtk;
using System;
using System.Collections;
using System.Collections.Generic;
using Tabellarius.Database;
using Tabellarius.Assets;

namespace Tabellarius.ListFrameTypes
{
	public class ProgrammListView : AbstractListView
	{

		private DatabaseAdapter dbAdapter;
		private EditFrameAdapter editFrameAdapter;


		public ProgrammListView() : base()
		{
			this.SetPolicy(PolicyType.Never, PolicyType.Never);

			dbAdapter = DatabaseAdapter.GetInstance();
			editFrameAdapter = EditFrameAdapter.GetInstance();

			tabView = new Notebook();
			treeList = new List<TreeView>();

			PopulateTabView();

			AddWithViewport(tabView);
		}

		private void PopulateTabView()
		{
			tabs = 0;
			// Adding a Tab with (scrollable) TreeStore for every day
			TreeStore dayQuery = dbAdapter.GetListFrameContentFor(tabs);
			while (dayQuery != null) {
				var tabContent = GenScrollableTree(dayQuery);
				tabs++; // UI days start with 1
				tabView.AppendPage(tabContent, new Label("Tag " + tabs));
				dayQuery = dbAdapter.GetListFrameContentFor(tabs);
			}
		}

		public override void AddTab() // Add a day
		{
			// AddDay() does not get saved in the database - just UI
			var emptyStore = new TreeStore(typeof(string), typeof(string));
			var tabContent = GenScrollableTree(emptyStore);
			tabs++;
			tabView.AppendPage(tabContent, new Label("Tag " + tabs));
			ShowAll();
		}

		public override void AddParentEntry() // Add a Termin
		{
			var treeContent = (TreeStore)(treeList[tabView.CurrentPage].Model);

			TimeBox timeBox = new TimeBox(true);
			timeBox.posEntry.Text = "0"; // Default value

			Entry textEntry = new Entry();
			// Get Data from User
			int activeVisibility = 2; // Getting the active ComboBox is pretty hacky
			ComboBox cbVisibility = new ComboBox(
				new string[] { "Notiz", "Mitarbeiter", "Alle" });
			cbVisibility.Active = activeVisibility;
			cbVisibility.Changed += delegate { activeVisibility = cbVisibility.Active; };

			int activeType = 1; // Getting the active ComboBox is pretty hacky
			ComboBox cbType = new ComboBox(
				new string[] { "aufbau", "programm", "freiwillig" });
			cbType.Active = activeType;
			cbType.Changed += delegate { activeType = cbType.Active; };

			GetUserArgs[] args = new GetUserArgs[] {
				new GetUserArgs(null, timeBox),
				new GetUserArgs(new Label("Text"), textEntry),
				new GetUserArgs(new Label("Sichtbarkeit"), cbVisibility),
				new GetUserArgs(new Label("Typ"), cbType)
			};


			var diag = new GetUserDataDialog(args, "Speichern", 0, "Abbruch", 1);
			if (diag.Run() == 0) {
				bool validated;
				while (!(validated = timeBox.ValidateTime())) {
					if (diag.Run() == 1) {
						validated = false;
						break;
					}
				}
				if (validated) {
					// Insert input into UI
					TreeIter insertIter, firstIter;
					insertIter = treeContent.AppendValues(timeBox.DatabaseTime, textEntry.Text);
					treeContent.GetIterFirst(out firstIter);
					GtkHelper.SortInByColumn(treeContent, (int)ProgrammColumnID.Uhrzeit, insertIter);

					// TODO: Save on Database
				}
			}
			diag.Destroy();

			foreach (var arg in args) // Free args
				arg.Dispose();
		}

		public override void AddChildEntry() // Add a Beschreibung
		{
			TreeIter parentIter = editFrameAdapter.GetActiveParentTreeIter();
			if (parentIter.Equals(TreeIter.Zero)) {
				var error = new SafeCallDialog("Kein Element ausgewählt", "Ok", 0, null, 1);
				error.Run();
				error.Destroy();
				return;
			}

			var treeContent = (TreeStore)(treeList[tabView.CurrentPage].Model);

			Entry userText = new Entry();
			var args = new GetUserArgs[] {
				new GetUserArgs(new Label("Text:"), userText)
			};

			var diag = new GetUserDataDialog(args, "Ok", 0, "Abbruch", 1);
			if (diag.Run() == 0) {
				// Insert and sort in
				TreeIter insertIter, firstIter;
				insertIter = treeContent.AppendValues(parentIter, "└──", "\t" + userText.Text);
				treeContent.IterNthChild(out firstIter, parentIter, 0);
				GtkHelper.SortInByColumn(treeContent, (int)ProgrammColumnID.Text, insertIter);

				// TODO: Save on Database
			}
			diag.Destroy();

			foreach (var arg in args)
				arg.Dispose();
		}

		protected override ScrolledWindow GenScrollableTree(TreeStore treeContent)
		{
			var scrollWin = new ScrolledWindow();
			scrollWin.ShadowType = ShadowType.EtchedOut;
			scrollWin.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

			var timeColumn = new TreeViewColumn(" Uhrzeit ", GenRenderCell(), "text", ProgrammColumnID.Uhrzeit);
			var textColumn = new TreeViewColumn("Text", GenRenderCell(), "text", ProgrammColumnID.Text);
			textColumn.Resizable = true;
			var typColumn = new TreeViewColumn(" Typ ", GenRenderCell(), "text", ProgrammColumnID.Typ);

			var tree = new TreeView();
			tree.AppendColumn(timeColumn);
			tree.AppendColumn(textColumn);
			tree.AppendColumn(typColumn);
			tree.EnableGridLines = TreeViewGridLines.Both;
			tree.HeadersClickable = false;
			tree.HeadersVisible = true;
			tree.Model = treeContent;
			tree.RulesHint = true;
			tree.RowActivated += delegate (object sender, RowActivatedArgs args)
			{
				editFrameAdapter.PassToEditView((TreeView)sender, args, tabView.CurrentPage);
			};

			this.treeList.Add(tree);

			scrollWin.Add(tree);
			return scrollWin;
		}

		public override void DataSetChanged()
		{
			// Clear everything out
			for (uint i = 0; i < tabs; i++)
				tabView.RemovePage(0);
			tabs = 0;

			foreach (TreeView tree in treeList) {
				tree.Destroy();
				tree.Dispose();
			}
			treeList.Clear();

			// Repopulate
			PopulateTabView();
			ShowAll();
		}

		public override void Dispose()
		{
			foreach (TreeView tree in treeList) {
				tree.Destroy();
				tree.Dispose();
			}
			tabView.Destroy();
			tabView.Dispose();
			base.Dispose();
		}

		private static CellRendererText GenRenderCell()
		{
			var cell = new CellRendererText();
			cell.Editable = false;
			cell.SetPadding(5, 8);
			return cell;
		}

	}
}