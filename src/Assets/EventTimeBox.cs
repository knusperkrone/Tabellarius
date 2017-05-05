using Gtk;
using System;

namespace Tabellarius.Assets
{
	public class EventTimeBox : HBox
	{

		private readonly int[] daysPerMonth = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

		private Gdk.RGBA invalidColor;
		private Gdk.RGBA validColor;

		private Label yearLabel, monthLabel, dayLabel;
		private Entry yearEntry, monthEntry, dayEntry;
		private string origYear, origMonth, origDay;
		private bool isParent;

		public EventTimeBox(bool isParent) : base()
		{
			this.IsParent = isParent;
			Init();
		}

		public EventTimeBox() : base()
		{
			Init();
		}

		private void Init()
		{
			invalidColor.Red = invalidColor.Alpha = 1;
			invalidColor.Blue = invalidColor.Green = 0;

			validColor.Blue = validColor.Green = validColor.Red = validColor.Alpha = 1;

			yearLabel = new Label(" Jahr");
			monthLabel = new Label(" Monat");
			dayLabel = new Label(" Tag");

			yearEntry = new Entry();
			monthEntry = new Entry();
			dayEntry = new Entry();
			yearEntry.WidthChars = monthEntry.WidthChars = dayEntry.WidthChars = 5;

			var tmpBox = new HBox();
			tmpBox.PackStart(yearLabel, false, true, 3);
			tmpBox.PackStart(yearEntry, false, true, 3);
			tmpBox.PackStart(monthLabel, false, true, 3);
			tmpBox.PackStart(monthEntry, false, true, 3);
			tmpBox.PackStart(dayLabel, false, true, 3);
			tmpBox.PackStart(dayEntry, false, true, 3);

			this.PackStart(tmpBox, false, true, 5);

			origYear = origMonth = origDay = "";
		}

		public bool ValidateTime()
		{
			bool valid = true;
			if (isParent) {
				try {
					int year = int.Parse(yearEntry.Text);
					if ((valid = year < 2017))
						yearLabel.OverrideColor(StateFlags.Normal, invalidColor);
					else
						yearLabel.OverrideColor(StateFlags.Normal, validColor);
				} catch (Exception) {
					yearLabel.OverrideColor(StateFlags.Normal, invalidColor);
					valid = false;
				}
			} else {
				int year, month, day;

				try {
					year = int.Parse(yearEntry.Text);
					month = int.Parse(monthEntry.Text);
					day = int.Parse(dayEntry.Text);
				} catch (Exception) {
					yearLabel.OverrideColor(StateFlags.Normal, invalidColor);
					yearLabel.OverrideColor(StateFlags.Normal, invalidColor);
					yearLabel.OverrideColor(StateFlags.Normal, invalidColor);
					return false;
				}

				// Validate Input:
				if (year < 2017) {
					yearLabel.OverrideColor(StateFlags.Normal, invalidColor);
					valid = false;
				} else {
					yearLabel.OverrideColor(StateFlags.Normal, validColor);
				}
				if (month <= 0 || month > 12) {
					monthLabel.OverrideColor(StateFlags.Normal, invalidColor);
					valid = false;
				} else {
					monthLabel.OverrideColor(StateFlags.Normal, validColor);
				}
				if (day < 0 || ((month > 0 && month < 12) && day > daysPerMonth[month - 1])) {
					dayLabel.OverrideColor(StateFlags.Normal, invalidColor);
					valid = false;
				} else {
					dayLabel.OverrideColor(StateFlags.Normal, validColor);
				}

				// Change to YYYY/MM/DD format
				if (valid) {
					if (month < 10 && yearEntry.Text.Length == 1)
						yearEntry.Text = "0" + yearEntry.Text;
					if (day < 10 && monthEntry.Text.Length == 1)
						monthEntry.Text = "0" + monthEntry.Text;
				}
			}
			return valid;
		}

		public string Time
		{
			set
			{
				string[] data = value.Split('/');
				IsParent = data.Length == 3;

				yearLabel.OverrideColor(StateFlags.Normal, validColor);
				monthLabel.OverrideColor(StateFlags.Normal, validColor);
				dayLabel.OverrideColor(StateFlags.Normal, validColor);

				if (isParent) {
					origYear = yearEntry.Text = data[0];
					origMonth = monthEntry.Text = data[1];
					origDay = dayEntry.Text = data[2];
				} else {
					origYear = yearEntry.Text = value;
					origMonth = origDay = "";
				}
			}

			get { return yearEntry.Text; }
		}

		public bool IsParent
		{
			set
			{
				dayEntry.IsEditable = monthEntry.IsEditable =
				isParent = value;
			}
		}

		public bool IsEditable
		{
			set
			{
				yearEntry.IsEditable = value;
				monthEntry.IsEditable = value;
				dayEntry.IsEditable = value;
			}
		}

		public bool IsDirty
		{
			get
			{
				return !(dayEntry.Text.Equals(origDay) && !isParent && (monthEntry.Text.Equals(origMonth)
						&& dayEntry.Text.Equals(origDay)));
			}
		}

		public void Reset()
		{
			yearEntry.Text = origYear;
			monthEntry.Text = origMonth;
			dayEntry.Text = origDay;
		}

		public void Clear()
		{
			yearEntry.Text = monthEntry.Text = dayEntry.Text = "";
		}

		public new void Dispose()
		{
			yearLabel.Dispose();
			monthLabel.Dispose();
			dayLabel.Dispose();
			dayEntry.Dispose();
			monthEntry.Dispose();
			dayEntry.Dispose();
			base.Dispose();
		}

	}
}