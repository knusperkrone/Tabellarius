using Gtk;
using System;

namespace Tabellarius.Assets
{
	public class TimeBox : HBox
	{

		private static Gdk.RGBA invalidColor { get { return API_Contract.invalidColor; } }
		private static Gdk.RGBA validColor { get { return API_Contract.validColor; } }

		private Label padLabel; // optinal padding
		private readonly Label hourLabel, minLabel, posLabel;
		public readonly Entry hourEntry, minEntry, posEntry;
		private string origHour, origMin, origPos;


		public TimeBox(bool padding) : base()
		{
			hourLabel = new Label(" Stunde");
			minLabel = new Label(" Minute");
			posLabel = new Label(" Rang");

			hourEntry = new Entry();
			minEntry = new Entry();
			posEntry = new Entry();

			origHour = origMin = origPos = "";

			if (padding)
				padLabel = new Label(""); // Padding


			hourEntry.WidthChars = minEntry.WidthChars = posEntry.WidthChars = 5;


			Table table;
			if (padding) {
				table = new Table(1, 8, true);
				table.Attach(padLabel, 0, 1, 0, 1);
				table.Attach(hourLabel, 1, 2, 0, 1);
				table.Attach(hourEntry, 2, 3, 0, 1);
				table.Attach(minLabel, 3, 4, 0, 1);
				table.Attach(minEntry, 4, 5, 0, 1);
				table.Attach(posLabel, 5, 6, 0, 1);
				table.Attach(posEntry, 6, 7, 0, 1);
			} else {
				table = new Table(1, 7, true);
				table.Attach(hourLabel, 0, 1, 0, 1);
				table.Attach(hourEntry, 1, 2, 0, 1);
				table.Attach(minLabel, 2, 3, 0, 1);
				table.Attach(minEntry, 3, 4, 0, 1);
				table.Attach(posLabel, 4, 5, 0, 1);
				table.Attach(posEntry, 5, 6, 0, 1);
			}


			this.PackStart(table, false, true, 5); // Middle
		}

		public bool ValidateTime()
		{
			int hour, minute, pos;

			bool valid = true;

			try {
				hour = int.Parse(hourEntry.Text);
				minute = int.Parse(minEntry.Text);
				pos = int.Parse(posEntry.Text);
			} catch (Exception) {
				return false;
			}

			// Signal bad input
			if (hour < 0 || hour > 24) {
				hourLabel.OverrideColor(StateFlags.Normal, invalidColor);
				valid = false;
			} else {
				hourLabel.OverrideColor(StateFlags.Normal, validColor);
			}
			if (minute < 0 || minute > 60) {
				minLabel.OverrideColor(StateFlags.Normal, invalidColor);
				valid = false;
			} else {
				minLabel.OverrideColor(StateFlags.Normal, validColor);
			}
			if (pos < 0) {
				posLabel.OverrideColor(StateFlags.Normal, invalidColor);
				valid = false;
			} else {
				posLabel.OverrideColor(StateFlags.Normal, validColor);
			}

			// Change to HH:MM:XX format
			if (valid) {
				if (hour < 10 && hourEntry.Text.Length == 1)
					hourEntry.Text = "0" + hourEntry.Text;
				if (minute < 10 && minEntry.Text.Length == 1)
					minEntry.Text = "0" + minEntry.Text;
			}

			return valid;
		}

		public string OrigTime
		{
			get { return origHour + ":" + origMin + ":" + origPos; }
		}

		public string Time
		{
			set
			{
				hourLabel.OverrideColor(StateFlags.Normal, validColor);
				minLabel.OverrideColor(StateFlags.Normal, validColor);
				posLabel.OverrideColor(StateFlags.Normal, validColor);

				string[] data = value.Split(':');
				origHour = hourEntry.Text = data[0];
				origMin = minEntry.Text = data[1];
				origPos = posEntry.Text = data[2];
			}
			get
			{
				return hourEntry.Text + ":" + minEntry.Text + ":" + posEntry.Text;
			}
		}

		public bool IsEditable
		{
			set
			{
				hourEntry.IsEditable = value;
				minEntry.IsEditable = value;
				posEntry.IsEditable = value;
			}
		}

		public bool IsDirty
		{
			get
			{
				return !(hourEntry.Text.Equals(origHour) && minEntry.Text.Equals(origMin)
						&& posEntry.Text.Equals(origPos));
			}
		}

		public void Reset()
		{
			hourEntry.Text = origHour;
			minEntry.Text = origMin;
			posEntry.Text = origPos;
		}

		public void Clear()
		{
			hourEntry.Text = minEntry.Text = posEntry.Text = "";
		}

		public new void Dispose()
		{
			if (padLabel != null)
				padLabel.Dispose();
			hourLabel.Dispose();
			minLabel.Dispose();
			posLabel.Dispose();
			hourEntry.Dispose();
			minEntry.Dispose();
			posEntry.Dispose();
			base.Dispose();
		}

	}
}