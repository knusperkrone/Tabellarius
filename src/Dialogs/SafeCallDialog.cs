using Gtk;

namespace Tabellarius
{
	public class SafeCallDialog : Dialog
	{
		public SafeCallDialog(string text, string posLabel, int pos_id, string negLabel, int neg_id)
			: base("Moment!", FrameManager.ActiveWindow, DialogFlags.DestroyWithParent, new object())
		{
			Init(this, text, posLabel, pos_id, negLabel, neg_id);
		}

		public SafeCallDialog(string text, string buttLabel)
			: base("Moment!", FrameManager.ActiveWindow, DialogFlags.DestroyWithParent, new object())
		{
			this.ContentArea.PackStart(new Label(text), true, true, 0);
			this.AddButton(buttLabel, 0);
			this.ShowAll();
		}

		public SafeCallDialog(Widget widget, string buttLabel)
			: base("Moment!", FrameManager.ActiveWindow, DialogFlags.DestroyWithParent, new object())
		{
			this.ContentArea.PackStart(widget, true, true, 0);
			this.AddButton(buttLabel, 0);
			this.ShowAll();
		}

		private static void Init(SafeCallDialog diag, string text, string posLabel, int pos_id, string negLabel, int neg_id)
		{
			diag.ContentArea.PackStart(new Label(text), true, true, 0);
			if (negLabel != null)
				diag.AddButton(negLabel, neg_id);
			if (posLabel != null)
				diag.AddButton(posLabel, pos_id);

			diag.ShowAll();
		}

	}
}