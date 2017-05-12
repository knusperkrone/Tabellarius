using Gtk;
using System;


namespace Tabellarius
{

	public class GetUserArgs : IDisposable
	{
		public readonly Label textLabel;
		public readonly Widget inputEntry;

		public bool centerInput;

		public GetUserArgs(Label textLabel, Widget inputEntryRef)
		{
			this.textLabel = textLabel;
			this.inputEntry = inputEntryRef;
			centerInput = textLabel == null;
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
		public GetUserDataDialog(GetUserArgs[] args, Widget topWidget, string posLabel, int pos_id, string negLabel, int neg_id)
								: base("Benutzereingabe", MainFrame.GetInstance(), DialogFlags.DestroyWithParent, new object())
		{
			if (topWidget != null) {
				this.ContentArea.PackStart(topWidget, false, true, 2);
				if (args.Length != 0) // Dispose necessary?
					this.ContentArea.PackStart(new HSeparator(), false, true, 4);
			}

			Table table = new Table(Convert.ToUInt32(args.Length), 2, true);
			uint row = 0;
			foreach (var field in args) {

				if (field.centerInput) {
					table.Attach(field.inputEntry, 0, 2, row, row + 1, 0, 0, 0, 4);
				} else {
					if (field.textLabel != null)
						table.Attach(field.textLabel, 0, 1, row, row + 1, 0, 0, 0, 2);
					if (field.inputEntry != null)
						table.Attach(field.inputEntry, 1, 2, row, row + 1,
									AttachOptions.Expand | AttachOptions.Fill, 0, 0, 2);
				}
				row++;
			}
			this.ContentArea.PackStart(table, false, false, 2);

			if (posLabel != null)
				this.AddButton(posLabel, pos_id);
			if (negLabel != null)
				this.AddButton(negLabel, neg_id);

			this.ShowAll();
		}

	}
}