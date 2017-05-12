using Gtk;
using System;

namespace Tabellarius.Assets
{
	public class EventTimeBox : HBox
	{
		private readonly int[] daysPerMonth = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

		private Gdk.RGBA invalidColor { get { return API_Contract.invalidColor; } }
		private Gdk.RGBA validColor { get { return API_Contract.validColor; } }

		private Label padLabel, yearLabel, monthLabel, dayLabel;
		private Entry yearEntry, monthEntry, dayEntry;
		private string origYear, origMonth, origDay;
		private bool isParent;

		public EventTimeBox(bool isParent) : base()
		{
			Init();
			this.IsParent = isParent;
		}

		public EventTimeBox() : base()
		{
			Init();
		}

		private void Init()
		{
			padLabel = new Label(""); // Padding
			yearLabel = new Label(" Jahr");
			monthLabel = new Label(" Monat");
			dayLabel = new Label(" Tag");

			yearEntry = new Entry();
			monthEntry = new Entry();
			dayEntry = new Entry();
			yearEntry.WidthChars = monthEntry.WidthChars = dayEntry.WidthChars = 5;

			var tmpBox = new Table(1, 8, true);
			tmpBox.Attach(padLabel, 0, 1, 0, 1);
			tmpBox.Attach(yearLabel, 1, 2, 0, 1);
			tmpBox.Attach(yearEntry, 2, 3, 0, 1);
			tmpBox.Attach(monthLabel, 3, 4, 0, 1);
			tmpBox.Attach(monthEntry, 4, 5, 0, 1);
			tmpBox.Attach(dayLabel, 5, 6, 0, 1);
			tmpBox.Attach(dayEntry, 6, 7, 0, 1);

			this.PackStart(tmpBox, false, true, 5);

			origYear = origMonth = origDay = "";
		}

		public bool ValidateTime()
		{
			bool valid = true;
			int year, month, day;
			year = month = day = -1;

			try {
				year = int.Parse(yearEntry.Text);
				valid = year >= 2017;
				if (!valid) {
					yearLabel.OverrideColor(StateFlags.Normal, invalidColor);
					valid = false;
				} else {
					yearLabel.OverrideColor(StateFlags.Normal, validColor);
				}
			} catch (Exception) {
				yearLabel.OverrideColor(StateFlags.Normal, invalidColor);
				return false;
			}

			if (!isParent) {
				try {
					month = int.Parse(monthEntry.Text);
					day = int.Parse(dayEntry.Text);
				} catch (Exception) {
					if (month == -1)
						yearLabel.OverrideColor(StateFlags.Normal, invalidColor);
					if (day == -1)
						yearLabel.OverrideColor(StateFlags.Normal, invalidColor);
					return false;
				}

				// Validate Input:
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
					if (month < 10 && monthEntry.Text.Length == 1)
						monthEntry.Text = "0" + monthEntry.Text;
					if (day < 10 && dayEntry.Text.Length == 1)
						dayEntry.Text = "0" + dayEntry.Text;
				}
			}

			return valid;
		}

		public string OrigTime
		{
			get
			{
				if (isParent) {
					return origYear;
				} else {
					return origYear + "/" + origMonth + "/" + origDay;
				}
			}
		}

		public string Time
		{
			set
			{
				string[] data = value.Split('/');
				IsParent = data.Length == 1;

				yearLabel.OverrideColor(StateFlags.Normal, validColor);
				monthLabel.OverrideColor(StateFlags.Normal, validColor);
				dayLabel.OverrideColor(StateFlags.Normal, validColor);

				if (isParent) {
					origYear = yearEntry.Text = value;
					origMonth = origDay = "";
				} else {
					origYear = yearEntry.Text = data[0];
					origMonth = monthEntry.Text = data[1];
					origDay = dayEntry.Text = data[2];
				}
			}

			get
			{
				if (isParent) {
					return yearEntry.Text;
				} else {
					return yearEntry.Text + "/" + monthEntry.Text + "/" + dayEntry.Text;
				}

			}
		}

		public bool IsParent
		{
			set
			{
				isParent = value;
				dayEntry.IsEditable = monthEntry.IsEditable = !isParent;
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