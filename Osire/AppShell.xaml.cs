using Osire.Pages;

namespace Osire;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute(nameof(Demos), typeof(Demos));
	}
}
