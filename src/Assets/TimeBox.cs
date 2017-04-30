using Gtk;
using System;

namespace Tabellarius.Assets
{
	public class TimeBox : HBox
	{

		private Gdk.RGBA invalidColor;
		private Gdk.RGBA validColor;

		public readonly Label hourLabel, minLabel, posLabel;
		public readonly Entry hourEntry, minEntry, posEntry;

		private string origHour, origMin, origPos;


		public TimeBox(bool start) : base()
		{
			invalidColor.Red = invalidColor.Alpha = 1;
			invalidColor.Blue = invalidColor.Green = 0;

			validColor.Blue = validColor.Green = validColor.Red = validColor.Alpha = 1;

			origHour = origMin = origPos = "";

			hourLabel = new Label("Stunde");
			minLabel = new Label("Minute");
			posLabel = new Label("Rang");

			hourEntry = new Entry();
			minEntry = new Entry();
			posEntry = new Entry();

			hourEntry.WidthChars = minEntry.WidthChars = posEntry.WidthChars = 5;

			if (start) {
				this.PackStart(hourLabel, false, false, 5);
				this.PackStart(hourEntry, false, false, 5);
				this.PackStart(minLabel, false, false, 5);
				this.PackStart(minEntry, false, false, 5);
				this.PackStart(posLabel, false, false, 5);
				this.PackStart(posEntry, false, false, 5);
			} else {
				this.PackEnd(posEntry, false, false, 5);
				this.PackEnd(posLabel, false, false, 5);
				this.PackEnd(minEntry, false, false, 5);
				this.PackEnd(minLabel, false, false, 5);
				this.PackEnd(hourEntry, false, false, 5);
				this.PackEnd(hourLabel, false, false, 5);
			}


		}

		public bool ValidateTime()
		{
			// TODO: Edit string if possible
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

		public string Time
		{
			set
			{
				hourLabel.OverrideColor(StateFlags.Normal, validColor);
				minLabel.OverrideColor(StateFlags.Normal, validColor);
				posLabel.OverrideColor(StateFlags.Normal, validColor);

				string[] data = value.Split(':');

				origHour = data[0];
				origMin = data[1];
				origPos = data[2];

				hourEntry.Text = data[0];
				minEntry.Text = data[1];
				posEntry.Text = data[2];
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


		public string DatabaseTime
		{
			get
			{
				return hourEntry.Text + ":" + minEntry.Text + ":" + posEntry.Text;
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