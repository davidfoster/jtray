using Timer = System.Windows.Forms.Timer;

namespace JTray
{
	//----------------------------------------------------------------------------------------
	// Application Lifecycle:
	//----------------------------------------------------------------------------------------

	internal static class Program {
		[STAThread]
		private static void Main() {
			ApplicationConfiguration.Initialize();
			Application.Run(new JTrayApplicationContext());
		}
	}

	public class JTrayApplicationContext : ApplicationContext {

		//----------------------------------------------------------------------------------------
		// Constants:
		//----------------------------------------------------------------------------------------

		private const string OPEN_JENKINS_URL = "http://localhost:8080/";

		//----------------------------------------------------------------------------------------
		// Private Fields:
		//----------------------------------------------------------------------------------------

		private readonly NotifyIcon trayIcon;
		private readonly ContextMenuStrip trayMenu;

		private readonly Icon icon1;
		private readonly Icon icon2;

		private bool usingIcon1 = true;
		private readonly Timer iconTimer;

		//----------------------------------------------------------------------------------------
		// Constructors:
		//----------------------------------------------------------------------------------------

		public JTrayApplicationContext() {

			// load icons.
			icon1 = new Icon("Resources/icon-build.ico");
			icon2 = new Icon("Resources/icon-standby.ico");

			// set up tray menu with exit option.
			trayMenu = new ContextMenuStrip();
			trayMenu.Items.Add("Exit", null, Exit);

			// set up tray icon as a /NotifyIcon/.
			trayIcon = new NotifyIcon {
				Icon = icon1,
				Text = "JTray",
				ContextMenuStrip = trayMenu,
				Visible = true
			};

			// set up the timer to switch icons every second.
			iconTimer = new Timer {
				Interval = 1000
			};

			// bind toggle delegate and kick off timer.
			iconTimer.Tick += (_, _) => ToggleIcon();
			iconTimer.Start();

			// handle double-clicking the tray icon.
			trayIcon.MouseClick += TrayIcon_MouseClick;
		}

		//----------------------------------------------------------------------------------------
		// Application Lifecycle:
		//----------------------------------------------------------------------------------------

		protected override void Dispose(bool disposing) {
			if (disposing) {
				iconTimer.Dispose();
				trayIcon.Dispose();
				trayMenu.Dispose();
				icon1.Dispose();
				icon2.Dispose();
			}

			base.Dispose(disposing);
		}

		//----------------------------------------------------------------------------------------
		// Event Handlers:
		//----------------------------------------------------------------------------------------

		private static void TrayIcon_MouseClick(object? sender, MouseEventArgs e) {
			if (e.Button != MouseButtons.Left) return;

			try {
				System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
					FileName = OPEN_JENKINS_URL,
					UseShellExecute = true
				});
			} catch (Exception ex) {
				MessageBox.Show($"Failed to open URL: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		//----------------------------------------------------------------------------------------
		// Private Methods:
		//----------------------------------------------------------------------------------------

		private void ToggleIcon() {
			trayIcon.Icon = usingIcon1 ? icon2 : icon1;
			usingIcon1 = !usingIcon1;
		}

		private void Exit(object? sender, EventArgs e) {
			iconTimer.Stop();
			iconTimer.Dispose();

			trayIcon.Visible = false;
			trayIcon.Dispose();
			trayMenu.Dispose();
			icon1.Dispose();
			icon2.Dispose();

			ExitThread();
		}
	}
}