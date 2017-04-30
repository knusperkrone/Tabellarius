using Gtk;
using System;


namespace Tabellarius
{

	public class GetUserArgs : IDisposable
	{
		public readonly Label textLabel;
		public readonly Widget inputEntry;

		public GetUserArgs(Label textLabel, Widget inputEntryRef)
		{
			this.textLabel = textLabel;
			this.inputEntry = inputEntryRef;
		}

		public void Dispose()
		{
			if (textLabel != null)
				textLabel.Dispose();
			if (inputEntry != null)
				inputEntry.Dispose();
		}
	}


	public class GetUserDataDialog : Dialog
	{
				public GetUserDataDialog(GetUserArgs[] args, string posLabel, int pos_id, string negLabel, int neg_id)
		: base("Benutzereingabe", MainFrame.GetInstance(), DialogFlags.DestroyWithParent, new object())
		{

			foreach (var field in args) {
				this.ContentArea.PackStart(MakeInputLable(field.textLabel, field.inputEntry), false, false, 0);
			}

			if (posLabel != null)
				this.AddButton(posLabel, pos_id);
			if (negLabel != null)
				this.AddButton(negLabel, neg_id);

			ShowAll();
		}

		private static VBox MakeInputLable(Label text, Widget inputEntry)
		{
			var box = new VBox();
			if (text != null)
				box.PackStart(text, false, false, 2);
			box.PackStart(inputEntry, false, true, 4);
			return box;
		}

	}
}