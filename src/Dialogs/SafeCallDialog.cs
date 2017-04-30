using Gtk;

namespace Tabellarius
{
	public class SafeCallDialog : Dialog
	{
		public SafeCallDialog(string text, string posLabel, int pos_id, string negLabel, int neg_id)
		: base("Moment!", MainFrame.GetInstance(), DialogFlags.DestroyWithParent, new object())
		{
			this.ContentArea.PackStart(new Label(text), true , true, 0);
			if (negLabel != null)
				this.AddButton(negLabel, neg_id);
			if (posLabel != null)
				this.AddButton(posLabel, pos_id);

			ShowAll();
		}

	}
}