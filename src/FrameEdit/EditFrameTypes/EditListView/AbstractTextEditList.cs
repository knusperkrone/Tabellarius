using System;
using Gtk;

namespace Tabellarius.EditFrameTypes.ListEditView
{
	public abstract class AbstractTextEditList : AbstractEditList
	{

		private readonly TextView textEntry;
		protected string CurrText
		{
			get { return textEntry.Buffer.Text; }
			set { textEntry.Buffer.Text = value; }
		}

		protected bool textEntryEditable {
			get { return textEntry.Editable; }
			set { textEntry.Editable = value; }
		}

		private string origText;
		protected string OrigText
		{
			get { return origText; }
		}

		public AbstractTextEditList() : base()
		{
			textEntry = new TextView();
			textEntry.BorderWidth = 10; // Add some Padding
			textEntry.Editable = false;
			var scrollText = new ScrolledWindow();
			scrollText.ShadowType = ShadowType.EtchedOut;
			scrollText.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
			scrollText.Add(textEntry);
			this.PackEnd(scrollText, true, true, 2);

			origText = "";
		}

		protected override bool Init
		{
			set { textEntry.Editable = HookInit = value; }
		}

		public override void Clear()
		{
			textEntry.Buffer.Clear();
			HookClear();
		}

		protected override void EditTreeRow(Gtk.TreeView treeView, Gtk.RowActivatedArgs args, object tabData) {
			HookEditTreeRow(treeView, args, tabData);
			origText = CurrText;
		}

		protected override void OnCancel(object sender, EventArgs args)
		{
			CurrText = origText;
		}

		protected override bool OnSave(){
			if (HookOnSave()) {
				origText = CurrText;
				return true;
			}
			return false;
		}

		protected override bool IsDirty()
		{
			return CurrText.Equals(origText) || HookIsDirty();
		}

		public override void Dispose() {
			textEntry.Buffer.Dispose();
			textEntry.Dispose();
			base.Dispose();
		}


		protected abstract bool HookInit { set; }
		protected abstract void HookClear();
		protected abstract bool HookIsDirty();
		protected abstract void HookEditTreeRow(Gtk.TreeView treeView, Gtk.RowActivatedArgs args, object tabData);
		protected abstract void HookOnCancel(object sender, EventArgs args);
		protected abstract bool HookOnSave();

	}
}