using Gtk;
using System;
using Tabellarius.ListFrameTypes;

namespace Tabellarius.EditFrameTypes.TabEditView
{
	public abstract class AbstractEditTab : AbstractEditView
	{

		protected readonly TextView textEntry = new TextView();
		protected string origTabName = "";
		protected TabHeader currHeader;

		public AbstractEditTab() : base()
		{
			// TODO: Nice layout
			this.Add(textEntry);
		}

		protected override bool Init
		{
			set { textEntry.Editable = init = value; }
		}

		public override void Clear()
		{
			textEntry.Buffer.Clear();
		}

		public virtual void EditTabRow(TabHeader tabHead, TreeStore treeStore)
		{
			currTreeStore = treeStore;
			currHeader = tabHead;
			origTabName = textEntry.Buffer.Text = tabHead.textLabel.Text;
		}

		protected override void OnCancel(object sender, EventArgs args)
		{
			textEntry.Buffer.Text = origTabName;
		}

		protected override bool IsDirty()
		{
			return textEntry.Editable && !textEntry.Buffer.Text.Equals(origTabName);
		}

		public override void Dispose() {
			textEntry.Buffer.Dispose();
			textEntry.Dispose();
			base.Dispose();
		}

	}
}