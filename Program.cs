using Timer = System.Windows.Forms.Timer;

namespace JTray;

//----------------------------------------------------------------------------------------
// Application Lifecycle:
//----------------------------------------------------------------------------------------

internal static class Program {
	[STAThread]
	private static void Main() {
		ApplicationConfiguration.Initialize();
		using JTrayApplicationContext context = new JTrayApplicationContext();
		Application.Run(context);
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
		trayMenu.Items.Add("Exit", null, TrayMenu_Exit);

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
		iconTimer.Tick += IconTimer_Tick;
		iconTimer.Start();

		// handle clicking the tray icon.
		trayIcon.MouseClick += TrayIcon_MouseClick;
	}

	//----------------------------------------------------------------------------------------
	// Application Lifecycle:
	//----------------------------------------------------------------------------------------

	protected override void Dispose(bool disposing) {
		if (disposing) {

			// unhook delegates.
			trayIcon.MouseClick -= TrayIcon_MouseClick;
			iconTimer.Tick -= IconTimer_Tick;

			// dispose of resources.
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

	private void IconTimer_Tick(object? sender, EventArgs e) {
		ToggleIcon();
	}

	private void TrayMenu_Exit(object? sender, EventArgs e) {
		ExitThread();
	}

	//----------------------------------------------------------------------------------------
	// Private Methods:
	//----------------------------------------------------------------------------------------

	private void ToggleIcon() {
		trayIcon.Icon = (usingIcon1) ? icon2 : icon1;
		usingIcon1 = !usingIcon1;
	}
}
