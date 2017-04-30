using Gtk;
using Cairo;
using System;

namespace Tabellarius
{
	public class DownloadWindow : Window // Copy paste
	{

		private double[,] trs = new double[,] {
			{ 0.0, 0.15, 0.30, 0.5, 0.65, 0.80, 0.9, 1.0 },
			{ 1.0, 0.0,  0.15, 0.30, 0.5, 0.65, 0.8, 0.9 },
			{ 0.9, 1.0,  0.0,  0.15, 0.3, 0.5, 0.65, 0.8 },
			{ 0.8, 0.9,  1.0,  0.0,  0.15, 0.3, 0.5, 0.65},
			{ 0.65, 0.8, 0.9,  1.0,  0.0,  0.15, 0.3, 0.5 },
			{ 0.5, 0.65, 0.8, 0.9, 1.0,  0.0,  0.15, 0.3 },
			{ 0.3, 0.5, 0.65, 0.8, 0.9, 1.0,  0.0,  0.15 },
			{ 0.15, 0.3, 0.5, 0.65, 0.8, 0.9, 1.0,  0.0, }
		};

		private short count = 0;
		private DrawingArea darea;


		public DownloadWindow(String msg) : base("Einen Moment...")
		{
			SetDefaultSize(250, 210);
			SetPosition(WindowPosition.Center);
			DeleteEvent += delegate { Application.Quit(); };

			GLib.Timeout.Add(100, new GLib.TimeoutHandler(OnTimer));

			var table = new Table(3, 5, false);

			darea = new DrawingArea();
			darea.Drawn += new DrawnHandler(OnExpose);
			table.Attach(new Label(msg), 0, 2, 0, 1, 0, 0, 10, 10);
			AttachOptions fill = AttachOptions.Fill | AttachOptions.Expand;
			table.Attach(darea, 0, 2, 3, 4, fill, fill, 0, 0);

			Add(table);
			ShowAll();
		}

		bool OnTimer()
		{
			count += 1;
			darea.QueueDraw();
			return true;
		}

		void OnExpose(object sender, EventArgs args)
		{
			Cairo.Context cr = Gdk.CairoHelper.Create(this.Window);

			cr.LineWidth = 3;
			cr.LineCap = LineCap.Round;

			int width, height;
			width = Allocation.Width;
			height = Allocation.Height;

			cr.Translate(width / 2, height / 2);

			for (int i = 0; i < 8; i++) {
				cr.SetSourceRGBA(0, 0, 0, trs[count % 8, i]);
				cr.MoveTo(0.0, -10.0);
				cr.LineTo(0.0, -40.0);
				cr.Rotate(Math.PI / 4);
				cr.Stroke();
			}

			cr.GetTarget().Dispose();
			cr.Dispose();
		}
	}
}