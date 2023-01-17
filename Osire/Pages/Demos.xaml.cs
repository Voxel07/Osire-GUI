using Osire.Models;
using static Osire.Models.Message;

namespace Osire.Pages;

public partial class Demos : ContentPage
{
	public Demos()
	{
		InitializeComponent();
	}

	Message DemoMessage = new Message();
    Leuchte DemoLight = new Leuchte();

	private long Delay { get; set; }
	private myColor ColorBackground { get; set; }
	private myColor ColorForeground { get; set; }
	private ushort NumberOfDots { get; set; }
	private ushort DistanzeToPrevios { get; set; }
	private ushort LedCount { get; set; }
	private ushort PositionStart { get; set; }
	private ushort PositionEnd { get; set; }

	private async void Lauflicht(object sender, EventArgs e)
	{
		DemoLight.SetIp("192.168.1.104");
		LedCount = 21;
        PositionStart = 1;
		DemoMessage.Command = (PossibleCommands)PossibleDemos.LED_STRIPE;
        DemoMessage.Type = MessageTypes.DEMO;


    }




}