using Gtk;
using System;
using Tabellarius.Assets;
using Tabellarius.Database;

namespace Tabellarius.ListFrameTypes
{
	public class EventListView : AbstractListView
	{

		public EventListView() : base(/*Magic happens here*/) {
			// TODO: Set tab visibility to false
		}

		protected override void PopulateTabView()
		{
			tabs = 1;
			var treeWindow = GenScrollableTree(dbAdapter.GetEventContent());
			tabView.AppendPage(treeWindow, new Label("Veranstaltungen"));
		}

		public override void AddChildEntry()
		{
			var parIter = editFrameAdapter.ActiveParentTreeIter;
			if (parIter.Equals(TreeIter.Zero))
				return;

			// Get User data
			EventTimeBox timeBox = new EventTimeBox(false);
			GetUserArgs[] args = { };
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
					var treeContent = (TreeStore)treeList[0].Model;
					string time = timeBox.Time;
					int vId = (int)treeContent.GetValue(parIter, (int)EventColumnId.ID);
					int iId = treeContent.IterNChildren(parIter) + 1;
					if (iId != 1) // XXX: Start counting with 1 here (beta)
						iId++;
					// Save on UI
					treeContent.AppendValues(parIter, iId, "", "", "", time);
					// Save on Database
					var elem = new Table_Instanz(vId, iId, time);
					dbAdapter.InsertEntry(elem);
				}
			}

			diag.Destroy();
			foreach (var arg in args)
				arg.Dispose();
		}

		public override void AddParentEntry()
		{
			var parIter = editFrameAdapter.ActiveParentTreeIter;
			var treeContent = (TreeStore)treeList[0].Model;

			TreeIter lastIter = GtkHelper.GetLastIter(treeContent, parIter);

			EventTimeBox timeBox = new EventTimeBox(true);
			ComboBox cbKrz = new ComboBox(API_Contract.SupportedKrzl);
			Entry krzlEntry = new Entry();
			krzlEntry.Changed += delegate
			{
				krzlEntry.Text = krzlEntry.Text.Trim();
				if (krzlEntry.Text.Length != 0) { // New Krzl
					cbKrz.Active = -1;
					cbKrz.Sensitive = false;
				} else { // Existing KRZL
					cbKrz.Sensitive = true;
				}
			};
			ComboBox cbLang = new ComboBox(API_Contract.SupportedLanguages);
			Entry textEntry = new Entry();
			GetUserArgs[] args = {
				new GetUserArgs(new Label("Sprache"), cbLang),
				new GetUserArgs(new Label("Kürzel"), cbKrz),
				new GetUserArgs(new Label("Neues Kürzel"), krzlEntry),
				new GetUserArgs(new Label("Name"), textEntry),
			};

			var diag = new GetUserDataDialog(args, timeBox, "Speichern", 0, "Abbruch", 1);
			if (diag.Run() == 0) {
				bool validated; // While the Input is invalid
				while (!(validated = timeBox.ValidateTime() && ValidateKrzl(args[3]))) {
					if (diag.Run() == 1) { // User cancels
						validated = false;
						break;
					}
				}

				if (validated) { // There is valid user data
					string text = textEntry.Text;
					string time = timeBox.Time;
					string lang = API_Contract.SupportedLanguages[cbLang.Active];
					string krzl;
					int id = treeContent.IterNChildren(parIter) + 1;
					if (id != 1) // XXX: Start counting with 1 here (beta)
						id++;
					if (cbKrz.Active > 0) { // No new Krzl
						krzl = API_Contract.SupportedLanguages[cbKrz.Active];
					} else {
						krzl = krzlEntry.Text;
					}
					// Save on UI
					treeContent.AppendValues(parIter, id, text, krzl, lang, time);
					// Save on Database
					var elem = new Table_Veranstaltung(id, text, krzl, lang, int.Parse(time));
					dbAdapter.InsertEntry(elem);
				}
			}
			// Free memory
			diag.Destroy();
			foreach (var arg in args)
				arg.Dispose();
		}

		private static bool ValidateKrzl(GetUserArgs arg)
		{
			var krzlLabel = arg.textLabel;
			var krzlEntry = (Entry)arg.inputEntry;

			krzlEntry.Text = krzlEntry.Text.ToUpper();
			if (GtkHelper.FindInArray(API_Contract.SupportedKrzl, krzlEntry.Text) == -1) {
				krzlLabel.OverrideColor(0, API_Contract.validColor);
				return true;
			} else {
				krzlLabel.OverrideColor(0, API_Contract.invalidColor);
				return false;
			}
		}

		public override void AddTab()
		{
			return; // Nothing to do here!
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
				editFrameAdapter.PassToEditView((TreeView)sender, args, null);
			};

			this.treeList.Add(tree);
			scrollWin.Add(tree);

			return scrollWin;
		}

	}
}