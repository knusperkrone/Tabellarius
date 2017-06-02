namespace Tabellarius.EditFrameTypes.TabEditView
{
	public class EditProgrammTab : AbstractEditTab
	{

        public EditProgrammTab() : base() {
			textEntry.Editable = false;
		}

		protected override bool OnSave()
		{
			return true; // Nothing to edit, nothing to save!
		}

	}
}