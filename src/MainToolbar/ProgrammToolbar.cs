using Gtk;
using System;

namespace Tabellarius.MainToolbar
{
	public class ProgrammToolbar : HBox, IToolbar
	{

		private DatabaseAdapter dbAdapter;
		private ListFrameAdapter listAdapter;
		private EditFrameAdapter editAdapter;

		private int activeVeranstaltung; // 1D index

		private String[] dataVeranstaltung;
		private int[] dataVeranstaltungIds;
		private String[] dataSprache;
		private String[] dataKrzl;
		private String[][] dataInstanz;

		private ComboBox cbVeranstaltung;
		private ComboBox cbInstanz;


		public ProgrammToolbar() : base()
		{
			dbAdapter = DatabaseAdapter.GetInstance();
			listAdapter = ListFrameAdapter.GetInstance();
			editAdapter = EditFrameAdapter.GetInstance();

			cbVeranstaltung = new ComboBox();
			cbInstanz = new ComboBox();

			PrepareComboBoxes();

			this.PackStart(cbVeranstaltung, false, true, 0);
			this.PackStart(cbInstanz, false, true, 0);
		}

		public void PrepareComboBoxes()
		{
			// Get Databse data
			dbAdapter.GetToolbarData(out dataKrzl, out dataVeranstaltung, out dataVeranstaltungIds,
									out dataSprache, out dataInstanz);

				if (dataVeranstaltung.Length != 0) { // CurrDatabase is not Empty
												 // Temporay dataArray to make it human readable

				String[] dataVeranstaltungSummary = new String[dataVeranstaltung.Length];
				for (int i = 0; i < dataVeranstaltung.Length; i++) {
					dataVeranstaltungSummary[i] = "'" + dataKrzl[i] +
						 "' - '" + dataVeranstaltung[i] + "' - '" + dataSprache[i] + "'";
				}

				GtkHelper.FillComboBox(cbVeranstaltung, dataVeranstaltung);
				GtkHelper.FillComboBox(cbInstanz, dataInstanz[0]);

				// Set default box value and register callbacks
				cbVeranstaltung.Active = cbInstanz.Active = 0;
				cbVeranstaltung.Changed += OnVeranstaltungsBoxChanged;
				cbInstanz.Changed += OnInstanzBoxChanged;

				// Set DbAdapter default values
				activeVeranstaltung = dataVeranstaltungIds[0];
				dbAdapter.Curr_veranstaltungsId = dataVeranstaltungIds[0];
				dbAdapter.Curr_lang = dataSprache[0];
				dbAdapter.Curr_instanzId = int.Parse(dataInstanz[0][0].Substring(0, dataInstanz[0][0].IndexOf(":")));

				// Draw frames
				editAdapter.Refresh();
				listAdapter.Refresh();

			} else { // Create empty boxes
				activeVeranstaltung = -1;
				dbAdapter.NoActiveVeranstaltung();
				cbVeranstaltung.Changed -= OnVeranstaltungsBoxChanged;
				cbInstanz.Changed -= OnInstanzBoxChanged;

				cbInstanz.Clear();
				cbVeranstaltung.Clear();
			}
		}

		private void OnVeranstaltungsBoxChanged(object sender, EventArgs args)
		{
			int newVal = cbVeranstaltung.Active;
			activeVeranstaltung = newVal;
			if (newVal >= 0) {
				// Set DatabaseAdapter values
				dbAdapter.Curr_lang = dataSprache[newVal];
				dbAdapter.Curr_veranstaltungsId = dataVeranstaltungIds[newVal];

				// Prepare InstanzBox
				GtkHelper.FillComboBox(cbInstanz, dataInstanz[newVal]);
				cbInstanz.Active = 0;
			} else {
				dbAdapter.Curr_veranstaltungsId = -1;
			}
			OnInstanzBoxChanged(null, null); // implicit redraws frames
		}

		private void OnInstanzBoxChanged(object sender, EventArgs args)
		{
			if (activeVeranstaltung >= 0 && cbInstanz.Active >= 0) {
				// Get the id (first chars until ':' buried in dataString)
				var instanzIdstring = dataInstanz[activeVeranstaltung][cbInstanz.Active];
				dbAdapter.Curr_instanzId = int.Parse(instanzIdstring.Substring(0, instanzIdstring.IndexOf(":")));
			} else {
				dbAdapter.Curr_instanzId = -1;
			}
			editAdapter.Refresh();
			listAdapter.Refresh();
		}

		public new void Dispose()
		{
			cbVeranstaltung.Dispose();
			cbInstanz.Dispose();
			base.Dispose();
		}

	}
}