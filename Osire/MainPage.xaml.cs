using Osire.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

namespace Osire;

public partial class MainPage : ContentPage
{
    public MainPage()
	{
		InitializeComponent();
    }
    Leuchte lamp; 
    
    private void InitLight(object sender, EventArgs e)
	{
        if (string.IsNullOrEmpty(entry_LedCount.Text))
        {
            lblLightState.Text = "LED Count is missing !";
            btnInitLeuchte.IsEnabled = false;
            return;
        }

        if(!int.TryParse(entry_LedCount.Text,out int anz))
        {
            lblLightState.Text = "LED Count war keine Zahl!";
            btnInitLeuchte.IsEnabled = false;
            return;
        }

        if(anz == 0)
        {
            lblLightState.Text = "Count == 0!";
            btnInitLeuchte.IsEnabled = false;
            return;
        }
       
        lamp = new Leuchte((ushort)anz);
        BtnInitLED.IsEnabled = true;
    }

    private async void InitLED(object sender, EventArgs e)
    {
        //Check IP
        //Send Command
        //Wait for response and
        //Update List view
        BtnInitLED.IsEnabled = false;
        countedLeds.ItemsSource = await Task.Run(async()=> await dummyLEDS());
       
        //Activate the led selection
        countedLeds.IsVisible = true;
        ledAddr.IsVisible= true;
        lblledAddr.IsVisible= true;

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
        if(BtnActivateLed.Text == "On")
        {
            BtnActivateLed.Text = "Off";
        }
        else
        {
            BtnActivateLed.Text = "On";
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

    private void HandleIpComplete(object sender, EventArgs e)
    {
        cbStateIp.IsChecked = true;
    }

    //Handle Selected LED
    private void EntryLedAddr(object sender, EventArgs e)
    {
        string addresse = ledAddr.Text;
        if (!int.TryParse(addresse, out int pos))
        {
            return;
        }
        if (pos > lamp.LedCount)
        {
            pos = lamp.LedCount;
        }
        BtnInitLED.IsEnabled= true;
        countedLeds.ScrollTo(lamp.LEDs.ElementAt(pos - 1), ScrollToPosition.Start , true);
        lblSelectedLed.Text = pos.ToString();
        ledAddr.Text = pos.ToString();
    }

    //Handle LED selection from ListView
    void svLedSelected(object sender, SelectedItemChangedEventArgs e)
    {
        LED led = e.SelectedItem as LED;
        lblSelectedLed.Text = led.Address.ToString();
    }

    //Handle LED count Change
    private void HandleLedChange(object sender, EventArgs e)
    {
        string text = entry_ipAddress.Text;
        int pos = text.Length;
        if (pos > 0)
        {
            lblLedCount.Text = "LED Count";
        }
        else
        {
            lblLedCount.Text = "";
        }
    }

    private void HandleLedCountComplete(object sender, EventArgs e)
    {
        cbStateLedCount.IsChecked = true;
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

