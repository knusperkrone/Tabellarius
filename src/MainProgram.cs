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

			try {
				Application.Init(); // Check for installed GTK#
			} catch (Exception) {
				Console.WriteLine("Failed to init GUI!");
				var OS = Environment.OSVersion;
				switch (OS.Platform) {
					case PlatformID.Unix:
						Console.WriteLine("[Unix] Install mono + gtk#"); break;
					case PlatformID.MacOSX:
						Console.WriteLine("[MacOS] Install mono + gtk#"); break;
					default:
						Console.WriteLine("[Windows] Install gtk#"); break;
				}
				return; // Exit programm
			}

			GLib.ExceptionManager.UnhandledException += UExecptionHandler;

			//new DatabaseChooser().ShowAll();
			FrameManager.GetInstance().StartGUI();

			Application.Run();
			Application.Quit();
		}

		public static void UExecptionHandler(UnhandledExceptionEventArgs args)
		{
			if (args.IsTerminating)
				Console.WriteLine(args.ToString());
			else
				new SafeCallDialog("Das programm hat sich mit folgender Fehlermeldung beendet:\n" + args.ExceptionObject, "Ok", 0, null, 0).Run();
			Application.Quit();
		}

	}
}