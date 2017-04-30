using Gtk;
//using System;

namespace Tabellarius
{
    public class TitleBar : MenuBar
    {

		private static TitleBar instance;


		private TitleBar(AccelGroup agr)
        {
            // Files
            MenuItem files = new MenuItem("Files");
            Menu fileMenu = new Menu();
            files.Submenu = fileMenu;
            // SubMenus_Files
            MenuItem exit_Files = new MenuItem("Exit");
            exit_Files.AddAccelerator("activate", agr, new AccelKey(Gdk.Key.q, Gdk.ModifierType.ControlMask, AccelFlags.Visible));
            exit_Files.Activated += MainFrame.OnDelete;
            fileMenu.Append(exit_Files);

            //MenuItem new_File, new_Event, update_File, upgrade_File
            this.Append(files);
        }

        public static TitleBar GetInstance(AccelGroup agr) {
            if (instance == null)
				instance = new TitleBar(agr);
			return instance;
		}

    }
}