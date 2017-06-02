using Gtk;
using System.Collections.Generic;
using System;

namespace Tabellarius.ListFrameTypes
{
	public class TabHeader : HBox
	{

		public readonly Label textLabel;
		public readonly Button button;
		public readonly int rang; //Not changable


		public TabHeader(string labelText, int rang) : base()
		{
			textLabel = new Label(labelText);
			this.rang = rang;
			button = new Button();
			button.Image = new Image(Stock.Edit);
			this.PackStart(textLabel, true, true, 2);
			this.PackStart(button, true, true, 2);
			textLabel.Show();
		}

		public TabHeader() : base() { }
	}

	public abstract class AbstractListView : Gtk.ScrolledWindow
	{

		protected class TabContent
		{
			public readonly ScrolledWindow scrollWin;
			public readonly TreeView tree; // Just TreeViews make Sense here

			protected internal TabContent() : base()
			{
				//XXX: make the constructur nested class exclusive
				scrollWin = new ScrolledWindow();
				scrollWin.ShadowType = ShadowType.EtchedOut;
				scrollWin.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
				tree = new TreeView();
				tree.EnableGridLines = TreeViewGridLines.Both; // Readability
				tree.HeadersClickable = false; // No sorting
				tree.RulesHint = true; // Dunno
				scrollWin.Add(tree);
			}
		}

		protected DatabaseAdapter dbAdapter;
		protected EditFrameAdapter editFrameAdapter;

		protected int tabs { private set; get; }
		private Gtk.Notebook tabView;
		private List<TabHeader> tabList = new List<TabHeader>();
		private List<TabContent> contentList = new List<TabContent>();



		protected AbstractListView() : base()
		{
			// Init Window and references
			this.SetPolicy(Gtk.PolicyType.Automatic, Gtk.PolicyType.Never);
			dbAdapter = DatabaseAdapter.GetInstance();
			editFrameAdapter = EditFrameAdapter.GetInstance();
			tabView = new Gtk.Notebook();
			// template init methods
			PopulateTabView();
			AddWithViewport(tabView);

			// Set TabHead Behavior: only active tab has a visible button
			tabView.SwitchPage += delegate
			{
				int i = 0;
				foreach (var head in tabList) {
					if (i == tabView.CurrentPage)
						head.button.Show();
					else
						head.button.Hide();
					i++;
				}
			};
		}

		public abstract void AddParentEntry();
		public abstract void AddChildEntry();
		protected abstract TabContent GenerateTabContent(Gtk.TreeStore treeContent);
		protected abstract void PopulateTabView();
		public abstract void AddTab();

		protected void AddTab(TabContent tabContent, string text)
		{
			if (!contentList.Contains(tabContent)) { // XXX: Keep for testing
				throw new Exception("The TabContent is not registert");
			}

			tabs++;
			var tabHead = new TabHeader(text, tabs);
			tabHead.button.Clicked += delegate
			{
				editFrameAdapter.PassToTabView(tabHead, CurrTreeStore);
			};

			tabList.Add(tabHead);
			tabView.AppendPage(tabContent.scrollWin, tabHead);
		}

		protected TabContent RegisterTabContent()
		{
			var tc = new TabContent();
			contentList.Add(tc);  // Register
			return tc;
		}

		public void DataSetChanged()
		{
			// Clear data
			for (uint i = 0; i < tabs; i++)
				tabView.RemovePage(0);
			tabList.Clear();
			tabs = 0;

			foreach (var content in contentList)
				content.tree.Dispose();
			contentList.Clear();

			// Repopulate
			PopulateTabView();
			tabView.ShowAll();
		}

		protected TreeStore CurrTreeStore
		{
			get { return (TreeStore)contentList[CurrTabIndex].tree.Model; }
		}

		protected int CurrTabIndex
		{
			get { return tabView.CurrentPage; }
		}

		protected bool ShowTabs
		{
			set { tabView.ShowTabs = value; }
		}

		protected static CellRendererText GenTextCell()
		{
			var cell = new Gtk.CellRendererText();
			cell.Editable = false;
			cell.SetPadding(5, 8);
			return cell;
		}

		public new void Dispose()
		{
			foreach (TabHeader header in tabList) {
				header.textLabel.Dispose();
				header.button.Dispose();
			}
			foreach (TabContent content in contentList) {
				content.tree.Dispose();
				content.scrollWin.Dispose();
			}
			tabList.Clear();
			contentList.Clear();

			tabView.Dispose();
			base.Dispose();
		}

	}
}