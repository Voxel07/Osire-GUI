using Osire.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Osire;

public partial class MainPage : ContentPage
{
    public MainPage()
	{
		InitializeComponent();
    }
    Leuchte lamp; 
    
    private void Rolf(object sender, EventArgs e)
	{
       	
	}

    private void OnButtonClicked(object sender, EventArgs e)
	{
        //lamp = new Leuchte(16);
        if (string.IsNullOrEmpty(entry_LedCount.Text))
        {
            lblInitLeute.Text = "LED Count is missing !";
            btnInitLeuchte.IsEnabled = false;
            return;
        }

        int anz; 
        if(!int.TryParse(entry_LedCount.Text,out anz))
        {
            lblInitLeute.Text = "LED Count war keine Zahl!";
            btnInitLeuchte.IsEnabled = false;
            return;
        }
       
        lamp = new Leuchte((ushort)anz);
        fuck.Text = lamp.LEDs.Count.ToString();
    }

    private async void InitLED(object sender, EventArgs e)
    {
        //Check IP
        //Send Command
        //Wait for response and
        //Update List view
        BtnInitLED.IsEnabled = false;
        countedLeds.ItemsSource = await Task.Run(async()=> await dummyLEDS());
        BtnInitLED.IsEnabled = true;

    }

    private async Task<List<LED>> dummyLEDS()
    {
        System.Threading.Thread.Sleep(5000);
        for (int i = 0; i < lamp.LedCount; i++)
        {
            lamp.LEDs.ElementAt(i).Address = (ushort)(i + 1);
        }
        return lamp.LEDs;
    }

    private void ActivateLed(object sender, EventArgs e)
    {
        if(BtnActivateLed.Text == "Activate LED")
        {
            BtnActivateLed.Text = "Deactivate LED";
        }
        else
        {
            BtnActivateLed.Text = "Activate LED";
        }
    }

    private void SetMaxCurrent(object sender, EventArgs e)
    {
        if(BtnMaxCurrent.Text == "10mA")
        {
            BtnMaxCurrent.Text = "50mA";
        }
        else
        {
            BtnMaxCurrent.Text = "10mA";
        }
    }


    private void HandleIpChange(object sender, TextChangedEventArgs e)
	{
		string text = entry_ipAddress.Text;
		int pos = text.Length; 
        if (pos > 0)
        {
            lblIp.Text = "Ip Address";
        }
        else
        {
            lblIp.Text = "";
        }
	
		if (pos == 3 || pos == 7 | pos == 11) 
		{
			text += ".";
		}

        entry_ipAddress.Text= text;
	}

    private void EntryLedAddr(object sender, EventArgs e)
    {
        string addresse = ledAddr.Text;
        int pos = int.Parse(addresse);
        countedLeds.ScrollTo(lamp.LEDs.ElementAt(pos - 1), ScrollToPosition.Start , true);
        lblSelectedLed.Text = addresse;
    }
       

    private void HandleLedChange(object sender, EventArgs e)
    {
        btnInitLeuchte.IsEnabled = true;
    }

    void OnSliderRedChanged(object sender, ValueChangedEventArgs e)
    {
		lableRed.Text = e.NewValue.ToString();

        float red = (float)sliderRed.Value / 32768;
		float green = (float)sliderGreen.Value / 32768;
		float blue = (float)sliderBlue.Value / 32768;

        colorBoxView.BackgroundColor = new Color(red, green, blue);
        colorBoxValue.TextColor = new Color(1 - red, 1 - green, 1 - blue);
        colorBoxValue.Text = colorBoxView.BackgroundColor.ToHex().ToString();
    }

    void OnSliderGreenChanged(object sender, ValueChangedEventArgs e)
    {
        lableGreen.Text = e.NewValue.ToString();

        float red = (float)sliderRed.Value / 32768;
        float green = (float)sliderGreen.Value / 32768;
        float blue = (float)sliderBlue.Value / 32768;

        colorBoxView.BackgroundColor = new Color(red, green, blue);
        colorBoxValue.TextColor = new Color(1 - red, 1 - green, 1 - blue);
        colorBoxValue.Text = colorBoxView.BackgroundColor.ToHex().ToString();

    }

    void OnSliderBlueChanged(object sender, ValueChangedEventArgs e)
    {
        lableBlue.Text = e.NewValue.ToString();

        float red = (float)sliderRed.Value / 32768;
        float green = (float)sliderGreen.Value / 32768;
        float blue = (float)sliderBlue.Value / 32768;

        colorBoxView.BackgroundColor = new Color(red, green, blue);
        colorBoxValue.TextColor = new Color(1 - red, 1 - green, 1 - blue);
        colorBoxValue.Text = colorBoxView.BackgroundColor.ToHex().ToString();
    }

    void OnSliderDelayChanged(object sender, ValueChangedEventArgs e)
    {
        lableDelay.Text = e.NewValue.ToString();
    }

    void svLedSelected (object sender, SelectedItemChangedEventArgs e)
    {
        LED led = e.SelectedItem as LED;
        lblSelectedLed.Text = led.Address.ToString();
    }

    private CancellationTokenSource _cts;

    async void cbRefreshCheckedChanged (object sender, CheckedChangedEventArgs e)
    {
        if (e.Value == true)
        {
            _cts = new CancellationTokenSource();
            //Start Backgorund update
            await Task.Run(() => DoWorkAsync(_cts.Token)).ConfigureAwait(false);
        }
        else
        {
            //Stop Background update
            _cts.Cancel();

            //Wait for the Background Task to finish
            while (!_cts.Token.IsCancellationRequested)
            {
                await Task.Delay(100).ConfigureAwait(false);
            }
        }
    }
    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        Random random = new Random();
        while (!cancellationToken.IsCancellationRequested)
        {
            Dispatcher.Dispatch(() => lblPwmRed.Text = sliderRed.Value.ToString());
            Dispatcher.Dispatch(() => lblPwmGreen.Text = sliderGreen.Value.ToString());
            Dispatcher.Dispatch(() => lblPwmBlue.Text = sliderBlue.Value.ToString());

            Dispatcher.Dispatch(() => testing.Text = random.Next(1, 1024).ToString());
            await Task.Delay(1000);
        }
    }
}

