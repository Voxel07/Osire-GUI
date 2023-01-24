using Osire.Models;
using Osire.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;
using static Osire.Models.Message;

namespace Osire.Pages;

public partial class Demos : ContentPage
{
	public Demos()
	{
		InitializeComponent();
        this.BindingContext = new DemoViewModel();
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
    //private List <myColor> ColorList { get; set; }



    private async void OnTab(object sender, TappedEventArgs e)
    {
        double maxX = 65535;
        double maxY = 65535;

        Point? p = e.GetPosition((View)sender);

        double width = img.Width;
        double heigth = img.Height;

        double currentX = (p.Value.X / width) * maxX;
        double currentY = maxY - ((p.Value.Y / heigth) * maxY);
        if (currentX < 0) { currentX = 0; }
        if (currentY < 0) { currentY = 0; }

        ushort u = (ushort)(currentX);
        ushort v = (ushort)(currentY);


        //Point? point = e.GetPosition((View)sender);
        //Ellipse ellipse = new Ellipse
        //{
        //    WidthRequest = 10,
        //    HeightRequest = 10,
        //    AnchorX = e.GetPosition.((View)sender),
        //    AnchorY = p.Value.Y
        //};

    }

    public ObservableCollection<RunningLight> MyItems { get; set; } = new ObservableCollection<RunningLight>();
    public string SelectedItem { get; set; }


    void svLedSelected(object sender, SelectedItemChangedEventArgs e)
    {
        SelectedItem = e.SelectedItem as string;

    }



    private void AddBarToRunningLight(object sender, EventArgs e)
    {
        //BarList.ItemsSource = MyItems;
        //BarList.SelectedItem = SelectedItem;
        MyItems.Add(new RunningLight());
        //ColorList.Add(new myColor());
    }
    private void RemoveBarFromRunningLight(object sender, EventArgs e)
    {
        //BarList.ItemsSource = MyItems;
        //BarList.SelectedItem = SelectedItem;
        //MyItems.Remove(SelectedItem);
        //ColorList.Remove(SelectedItem);
    }
}