using Gtk;
using System.IO;
using System;


namespace Tabellarius
{
	public class MainProgram
	{

		public static void Main(string[] args)
		{
			// Change to default-Dir
			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

			// Check for installed GTK#
			try {
				Application.Init();
			} catch (Exception) {
				// TODO: Installer dialog
				var OS = Environment.OSVersion;
				switch (OS.Platform) {
					case PlatformID.Unix:
						Console.WriteLine("Install mono + gtk#"); break;
					case PlatformID.MacOSX:
						Console.WriteLine("Install mono + gtk#"); break;
					default:
						Console.WriteLine("Install gtk#"); break;
				}
			}

			GLib.ExceptionManager.UnhandledException += UExecptionHandler;

			// TODO: DatabaseFileChooser Dialog
			DatabaseAdapter.setDb("backend (copy).db");

			MainFrame.GetInstance().ShowAll();
			Application.Run();
			Application.Quit();
		}

		public static void UExecptionHandler(UnhandledExceptionEventArgs args) {
			if (args.IsTerminating)
				Console.WriteLine(args.ToString());
			else
				new SafeCallDialog("Das programm hat sich mit folgender Fehlermeldung beendet:\n" + args.ExceptionObject.ToString(), "Ok", 0, null, 0).Run();
		}

	}
}