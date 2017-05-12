using Gtk;

namespace Tabellarius
{
	public class WarnWindow : Window
	{
		public WarnWindow(string msg) : base("Oh, ein Fehler!")
		{
			SetDefaultSize(450, 200);
			SetPosition(WindowPosition.Center);
			DeleteEvent += delegate { this.Destroy(); this.Dispose(); };

			var box = new VBox();
			Label text = new Label("Fehler:\n" + msg);
			box.PackStart(text, true, true, 5);

			Button quit = new Button("Schließen");
			quit.Clicked += delegate { this.Destroy(); };
			box.PackEnd(quit, false, true, 0);

			Add(box);
			this.ShowAll();
		}
	}
}