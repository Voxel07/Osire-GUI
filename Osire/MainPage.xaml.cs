using Osire.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;
using static Osire.Models.Message.PossibleCommands; 
using static Osire.Models.Message.MessageTypes;
using System.Text.RegularExpressions;
using System.IO.Ports;
using System.Text;
using static Osire.Models.Message;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Osire.Pages;
using System.Collections.ObjectModel;
using Osire.ViewModels;

namespace Osire;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new DemoViewModel();

    }

    //Calsses that will be filled with Data  by the UI
    Leuchte Light = new Leuchte();
    Message myMessage = new Message();



    private void Reset(object sender, EventArgs e)
    {
        Microsoft.Maui.Controls.Application.Current.MainPage = new MainPage();
    }

    private void GoToDemosPage(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync(nameof(Demos));
    }

    private async void DemoLauflicht (object sender, EventArgs e)
    {
        Button btn = sender as Button;
        myMessage.PSI = 8;
        myMessage.Type = DEMO;
        myMessage.Command = (PossibleCommands)PossibleDemos.LED_STRIPE;

        //await Task.Run(async () => await SendCommandAsync(btn));
        await ExecuteCommandAsync(async () => await SendCommandAsync(btn));
    }

    private async void DemoPingPong(object sender, EventArgs e)
    {
        Button btn = sender as Button;
        myMessage.PSI = 8;
        myMessage.Type = DEMO;
        myMessage.Command = (PossibleCommands)PossibleDemos.PINGPONG;
        await ExecuteCommandAsync(async () => await SendCommandAsync(btn));
    }

    private async void TempCompDemo(object sender, EventArgs e)
    {
        Button btn = sender as Button;
        myMessage.PSI = 8;
        myMessage.Type = DEMO;
        myMessage.Command = (PossibleCommands)PossibleDemos.TEMPCOMP;
        await ExecuteCommandAsync(async () => await SendCommandAsync(btn));
    }

   

    private async void IOL(object sender, EventArgs e)
    {
        Button btn = sender as Button;
        myMessage.PSI = 8;
        myMessage.Type = DEMO;
        myMessage.Command = (PossibleCommands)PossibleDemos.IOL;
        await ExecuteCommandAsync(async () => await SendCommandAsync(btn));
    }

    private async void CommandReset(object sender, EventArgs e)
    {
        HandleIpChange(sender, e);
        HandleLedCountChange(sender, e);

        Button btn = sender as Button;
        if (!Light.IpSet || !Light.LedCountSet)
        {
            lblLightState.Text = "Light config missing";
            return;
        }

        //Reset
        myMessage.SetMessageToReset();
        await ExecuteCommandAsync(async () => await SendCommandAsync(btn));

        if (!myMessage.rawErrorCode.Equals(0))
        {
            lblLightState.Text = "Error while resetting";
            return;
        }

        cbStateLightReset.IsChecked = true;
        Light.LedsWereReset = true;

        //Init
        if (!await InitLED(sender, e)) return;
        if (!myMessage.rawErrorCode.Equals(0))
        {
            lblLightState.Text = "Error during init";
            return;
        }

        //Config
        if (!await SetConfig(sender, e)) return;
        if (!myMessage.rawErrorCode.Equals(0))
        {
            lblLightState.Text = "Error during setup";
            return;
        }

        myMessage.Address = 0;
    }

    private async Task<bool> InitLED(object sender, EventArgs e)
    {
        if (!Light.IpSet || !Light.LedCountSet)
        {
            lblLightState.Text = "Light config missing";
            return false;
        }

        Button btn = sender as Button;

        myMessage.SetMessageToInit();
        await ExecuteCommandAsync(async () => await SendCommandAsync(btn));

        if (!myMessage.rawErrorCode.Equals(0)) return false;

        if (myMessage.LedCount != Light.LedCount)
        {
            Dispatcher.Dispatch(() => lblLightState.Text = "LED cnt missmatch Detected:" + myMessage.Address + "| Expected: " + EntryLedCount.Text);
            
            return false;
        }

        Light.LedsWereInitialized = true;
        Light.LedCount = myMessage.LedCount;
        Light.SetLeds(myMessage.LedCount);
        Light.InitLeds();
        countedLeds.ItemsSource = Light.LEDs;
        cbStateLightInit.IsChecked = true;
        countedLeds.IsVisible = true;
        EntryLedAddr.IsVisible = true;
        lblledAddr.IsVisible = true;

        if (!UpdateUi())
        {
            LblLedState.Text = "Updating the UI failed";
        }

        return true;
    }

    private async Task<bool> SetConfig(object sender, EventArgs e)
    {
        if (!Light.IpSet || !Light.LedCountSet)
        {
            lblLightState.Text = "Light config missing";
            return false;
        }

        Button btn = sender as Button;

        myMessage.SetMessageToDefaultConf();

        await ExecuteCommandAsync(async () => await SendCommandAsync(btn));

        if (myMessage.rawErrorCode.Equals(0))
        {
            cbStateLightSetup.IsChecked = true;
            BtnActivateLight.IsEnabled = true;
            Light.LedsWereReset = true;
            //CbAutoRefresh.IsEnabled = true;

        }
        return true;
    }

    private async void ActivateLight(object sender, EventArgs e)
    {
        Button btn = sender as Button;
        myMessage.Type = MessageTypes.COMMAND_WITH_RESPONSE;
        myMessage.PSI = 8;
        myMessage.Address = 0;

        if (Light.LedsAreActive)
        {
            myMessage.Command = PossibleCommands.GOSLEEP;
        }
        else
        {
            myMessage.Command = PossibleCommands.GOACTIVE;
        }
        await ExecuteCommandAsync(async () => await SendCommandAsync(btn));

        if (myMessage.rawErrorCode.Equals(0))
        {
            cbStateLedsActiv.IsChecked = !cbStateLedsActiv.IsChecked;
            Light.LedsAreActive = !Light.LedsAreActive;
            BtnPingPong.IsEnabled = true;
            ToolTipProperties.SetText(BtnPingPong, "Start the running light demo");
            BtnLauflicht.IsEnabled = true;
            ToolTipProperties.SetText(BtnLauflicht, "Start the ping pong demo");

        }
    }

    private async void ToggleLed(object sender, EventArgs e)
    {
        Button btn = sender as Button;

        if (Light.LedsWereInitialized == false)
        {
            LblLedState.Text = "init not complete";
            return;
        }
        LED led = Light.LEDs[Light.SelectedLed - 1];

        myMessage.PSI = 8;

        if (led.State == "ACTIVE")
        {
            myMessage.Command = PossibleCommands.GOSLEEP;
            BtnToggleLed.Text = "Off";
        }
        else
        {
            myMessage.Command = PossibleCommands.GOACTIVE;
            BtnToggleLed.Text = "On";
        }

        await ExecuteCommandAsync(async () => await SendCommandAsync(btn));

    }

    private void HandleIpChange(object sender, EventArgs e)
    {
        string ipString = entry_ipAddress.Text;
        int pos = ipString.Length;

        string validationPattern = @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";

        if (pos > 0)
        {
            lblIp.Text = "Ip Address";
        }
        else
        {
            lblIp.Text = "";
        }
        Regex regex = new Regex(validationPattern);
        if (regex.IsMatch(ipString))
        {
            cbStateIp.IsChecked = true;
            Light.SetIp(ipString);
            Light.IpSet = true;

            if (Light.LedCountSet)
            {
                btnResetLed.IsEnabled = true;
            }
        }
        else
        {
            Light.IpSet = false;
            cbStateIp.IsChecked = false;
        }
    }

    private void HandleLedCountChange(object sender, EventArgs e)
    {
        string ledCountText = EntryLedCount.Text;
        int pos = ledCountText.Length;

        if (pos > 0)
        {
            lblLedCount.Text = "LED Count";
        }
        else
        {
            lblLedCount.Text = "";
        }

        if (!uint.TryParse(ledCountText, out uint ledCount))
        {
            lblLedCount.Text = "Must be a Number 0 > X > 1024";
            cbStateLightCount.IsChecked = false;
            return;
        }

        if (ledCount > 0 && ledCount < 1024)
        {
            cbStateLightCount.IsChecked = true;
            Light.LedCountSet = true;
            Light.LedCount = (ushort)ledCount;

            if (Light.IpSet)
            {
                btnResetLed.IsEnabled = true;
            }
        }
        else
        {
            lblLedCount.Text = "Must be a Number 0 > X > 1024";
            cbStateLightCount.IsChecked = false;
        }
    }

    //Handle Selected LED
    private void HandleLedSelection(object sender, EventArgs e)
    {
        string addresse = EntryLedAddr.Text;
        if (!int.TryParse(addresse, out int pos))
        {
            return;
        }

        if (pos > Light.LedCount)
        {
            pos = Light.LedCount;
        }

        if (pos > 0)
        {
            countedLeds.ScrollTo(Light.LEDs.ElementAt(pos - 1).Address, position: ScrollToPosition.Start, animate: true);
            //countedLeds.SelectedItem
            EntryLedAddr.Text = pos.ToString();
            countedLeds.SelectedItem = Light.LEDs.ElementAt(pos - 1) ;
        }
        lblSelectedLed.Text = pos.ToString();
        myMessage.Address = (ushort)pos;
        Light.SelectedLed = (ushort)pos;
        UpdateUiWhenSelectedLedChanged();
    }

    //Handle LED selection from ListView
    void svLedSelected(object sender, SelectionChangedEventArgs e)
    {
        LED led = e.CurrentSelection.FirstOrDefault() as LED;
        if(led != null)
        {
            countedLeds.SelectedItem = led;
            lblSelectedLed.Text = led.Address.ToString(); //Fehler möglich 
            myMessage.Address = led.Address;
            Light.SelectedLed = led.Address;
            EntryLedAddr.Text = led.Address.ToString();
            UpdateUiWhenSelectedLedChanged();
        }
    }

    async void Refresh(object sender, EventArgs e)
    {
        if (Light.SelectedLed == 0)
        {
            LblLedState.Text = "Select a LED first";
            return;
        }
        LblLedState.Text = "";
        Button btn = sender as Button;
        String timeStamp = DateTime.Now.ToString("HH:mm:ss");
        myMessage.Address = Light.SelectedLed;
        switch (btn.ClassId)
        {
            case "BtnRefreshStatus": myMessage.Command = PossibleCommands.READSTATUS; LblRefreshStatus.Text = timeStamp; break;
            case "BtnRefreshLedSt": myMessage.Command = PossibleCommands.READLEDST; LblRefreshLedSt.Text = timeStamp; break;
            case "BtnRefreshComstats": myMessage.Command = PossibleCommands.READCOMST; LblRefreshComstats.Text = timeStamp; break;
            case "BtnRefreshTemp": myMessage.Command = PossibleCommands.READTEMP; break;
            case "BtnRefreshOTTH": myMessage.Command = PossibleCommands.READOTTH; break;
            case "BtnRefreshSetup": myMessage.Command = PossibleCommands.READSETUP; LblRefreshSetup.Text = timeStamp; break;
            case "BtnRefreshPwm": myMessage.Command = PossibleCommands.READPWM; LblRefreshPwm.Text = timeStamp; break;
            case "BtnRefreshOtp": myMessage.Command = PossibleCommands.READOTP; break;
            default:
                LblLedState.Text = "Unknown command";
                return;
        }
        myMessage.Type = MessageTypes.COMMAND_WITH_RESPONSE;
        myMessage.PSI = 8; // PSI (1) + Type (1) + Command (1) + Address (2) +  CRC (2)

        await ExecuteCommandAsync(async () => await SendCommandAsync(btn));
    }

    private double scaleNumber(double value, double oldMin, double oldMax, double newMin, double newMax)
    {
        return ((value - oldMin) / (oldMax - oldMin)) * (newMax - newMin) + newMin;
    }

    private async void GetGamut(object sender, EventArgs e)
    {
        if (!Light.LedCountSet || !Light.IpSet || !Light.LedsWereReset || !Light.LedsWereInitialized)
        {
            lblLightState.Text = "Init not complete";
            return;
        }
        Button btn = sender as Button;
        myMessage.Type = MessageTypes.COMMAND_WITH_RESPONSE;
        myMessage.PSI = 8; // PSI (1) + Type (1) + Command (1) + Address (2) +  CRC (2)
        myMessage.Address = Light.SelectedLed;
        myMessage.Command = PossibleCommands.GETGAMUT;

        await ExecuteCommandAsync(async () => await SendCommandAsync(btn));

        if (!myMessage.rawErrorCode.Equals(0))
        {
            lblLightState.Text = "Error during GetGamut";
            return;
        }
        // U = X | V = Y
        Point cornerRedDay =    new Point(scaleNumber(BitConverter.ToUInt32(myMessage.Gamut, 0),  0, 65535, 0, img.Width), img.Height - scaleNumber(BitConverter.ToUInt32(myMessage.Gamut, 4),  0, 65535, 0, img.Height));
        Point cornerGreenDay =  new Point(scaleNumber(BitConverter.ToUInt32(myMessage.Gamut, 12), 0, 65535, 0, img.Width), img.Height - scaleNumber(BitConverter.ToUInt32(myMessage.Gamut, 16), 0, 65535, 0, img.Height));
        Point cornerBlueDay =   new Point(scaleNumber(BitConverter.ToUInt32(myMessage.Gamut, 24), 0, 65535, 0, img.Width), img.Height - scaleNumber(BitConverter.ToUInt32(myMessage.Gamut, 28), 0, 65535, 0, img.Height));

        Point cornerRedNight =      new Point(scaleNumber(BitConverter.ToUInt32(myMessage.Gamut, 36), 0, 65535, 0, img.Width), img.Height - scaleNumber(BitConverter.ToUInt32(myMessage.Gamut, 40), 0, 65535, 0, img.Height));
        Point cornerGreenNight =    new Point(scaleNumber(BitConverter.ToUInt32(myMessage.Gamut, 48), 0, 65535, 0, img.Width), img.Height - scaleNumber(BitConverter.ToUInt32(myMessage.Gamut, 52), 0, 65535, 0, img.Height));
        Point cornerBlueNight =     new Point(scaleNumber(BitConverter.ToUInt32(myMessage.Gamut, 60), 0, 65535, 0, img.Width), img.Height - scaleNumber(BitConverter.ToUInt32(myMessage.Gamut, 64), 0, 65535, 0, img.Height));

        PathGeometry pathDay = (PathGeometry)GammutDay.Data;
        pathDay.Figures[0].StartPoint = cornerBlueDay;

        PathSegmentCollection segmentsDay = pathDay.Figures[0].Segments;

        ((LineSegment)segmentsDay[0]).Point = cornerRedDay;
        ((LineSegment)segmentsDay[1]).Point = cornerGreenDay;
        ((LineSegment)segmentsDay[2]).Point = cornerBlueDay;

        RGD.TranslationX = cornerRedDay.Y;
        RGD.TranslationY = cornerRedDay.X;

        //ToolTipProperties.SetText(RGD, $"u:{cornerRedDay.Y}|v: {cornerRedDay.X}");
        //await RGD.TranslateTo(cornerRedDay.Y, cornerRedDay.X, 1000, Easing.Linear);
        //ToolTipProperties.SetText(RBD, $"u:{cornerGreenDay.Y}|v: {cornerGreenDay.X}");
        //await RBD.TranslateTo(cornerGreenDay.Y, cornerGreenDay.X, 0, Easing.Linear);
        //ToolTipProperties.SetText(GBD, $"u:{cornerBlueDay.Y}|v: {cornerBlueDay.X}");
        //await GBD.TranslateTo(cornerBlueDay.Y, cornerBlueDay.X, 0, Easing.Linear);

        PathGeometry pathNight = (PathGeometry)GammutNight.Data;
        pathNight.Figures[0].StartPoint = cornerBlueNight;

        PathSegmentCollection segmentsNight = pathNight.Figures[0].Segments;

        ((LineSegment)segmentsNight[0]).Point = cornerRedNight;
        ((LineSegment)segmentsNight[1]).Point = cornerGreenNight;
        ((LineSegment)segmentsNight[2]).Point = cornerBlueNight;

    }

    private void CbBroadcastChanged (object sender, CheckedChangedEventArgs e)
    {
        if(e.Value)
        {
            EntryLedAddr.Text = "0";
            cbStatusRequest.IsChecked = false;
        }
    }
    private void CbStatusRequestChanged (object sender, CheckedChangedEventArgs e)
    {
        if(e.Value)
        {
            cbBroadcast.IsChecked = false;
        }
    }
    private void CheckBroadcast()
    {
        if (cbBroadcast.IsChecked)
        {
            myMessage.Address = 0;
        }
        else
        {
            myMessage.Address = Light.SelectedLed;
        }
    }

    private async void SetCommand(object sender, EventArgs e)
    {
        if (!Light.LedCountSet || !Light.IpSet)
        {
            LblLedState.Text = "Init not complete";
            return;
        }
        Button btn = sender as Button;
        //myMessage.PSI = 8;

        CheckBroadcast();

        switch (btn.ClassId)
        {
            case "BtnSetSetup":
                myMessage.Command = cbStatusRequest.IsChecked ? PossibleCommands.SETSETUPSR : PossibleCommands.SETSETUP;
                myMessage.PSI = 9; // PSI 2 + Type 1 + command 1 + addr 2 + setup + crc 2
                break;
            case "BtnSetOtth":
                myMessage.PSI = 11; // PSI 2 + Type 1 + command 1 + addr 2 + otth 3 + crc 2
                myMessage.Command = cbStatusRequest.IsChecked ? PossibleCommands.SETOTTHSR :PossibleCommands.SETOTTH;
                break;
            default:
                break;
        }
        await ExecuteCommandAsync(async () => await SendCommandAsync(btn));
    }

    private async void SetSetupChanged(object sender, CheckedChangedEventArgs e)
    {
        CheckBox cb = sender as CheckBox;
        bool state = cb.IsChecked;
        ushort selectedLed = Light.SelectedLed;
        if (selectedLed == 0)
        {
            LblLedState.Text = "No LED selected";
            return;
        }
        LED led = Light.LEDs.ElementAt(selectedLed - 1);

        switch (cb.ClassId)
        {
            case "CbSetPwmF":
                if (state)
                {
                    LblSetPwmF.Text = "1172 Hz / 14 bit";
                    led.PWM_F = "1172 Hz / 14 bit";
                }
                else
                {
                    LblSetPwmF.Text = "586 Hz / 15 bit";
                    led.PWM_F = "586 Hz / 15 bit";
                }
                myMessage.Setup = SetBit(myMessage.Setup, 7, state);
                break;
            case "ClkInv":
                if (state)
                {
                    LblSetClkP.Text = "LOW";
                    led.CLK_INV = "LOW";
                }
                else
                {
                    LblSetClkP.Text = "HIGH";
                    led.CLK_INV = "HIGH";
                }
                myMessage.Setup = SetBit(myMessage.Setup, 6, state);
                break;
            case "CbSetCrcEn":
                if (state)
                {
                    LblSetCrcEn.Text = "ENABLED";
                    led.CRC_EN = "ENABLED";
                }
                else
                {
                    LblSetCrcEn.Text = "DISABLED";
                    led.CRC_EN = "DISABLED";
                }
                myMessage.Setup = SetBit(myMessage.Setup, 5, state);
                break;
            case "CbSetTempClk":
                if (state)
                {
                    LblSetTempClk.Text = "2.4 kHz";
                    led.TEMPCLK = "2.4 kHz";
                }
                else
                {
                    LblSetTempClk.Text = "19.2 kHz";
                    led.TEMPCLK = "19.2 kHz";
                }
                myMessage.Setup = SetBit(myMessage.Setup, 4, state);
                break;
            case "CbSetCe":
                if (state)
                {
                    LblSetCe.Text = "RAISE & SLEEP";
                    led.CE_FSAVE = "RAISE & SLEEP";
                }
                else
                {
                    LblSetCe.Text = "RAISE";
                    led.CE_FSAVE = "RAISE";
                }
                myMessage.Setup = SetBit(myMessage.Setup, 3, state);
                break;
            case "CbSetLos":
                if (state)
                {
                    LblSetLos.Text = "RAISE & SLEEP";
                    led.LOS_FSAVE = "RAISE & SLEEP";
                }
                else
                {
                    LblSetLos.Text = "RAISE";
                    led.LOS_FSAVE = "RAISE";
                }
                myMessage.Setup = SetBit(myMessage.Setup, 2, state);
                break;
            case "CbSetOt":
                if (state)
                {
                    LblSetOt.Text = "RAISE & SLEEP";
                    led.OT_FSAVE = "RAISE & SLEEP";
                }
                else
                {
                    LblSetOt.Text = "RAISE";
                    led.OT_FSAVE = "RAISE";
                }
                myMessage.Setup = SetBit(myMessage.Setup, 1, state);
                break;
            case "CbSetUv":
                if (state)
                {
                    LblSetUv.Text = "RAISE & SLEEP";
                    led.UV_FSAVE = "RAISE & SLEEP";
                }
                else
                {
                    LblSetUv.Text = "RAISE";
                    led.UV_FSAVE = "RAISE";
                }
                myMessage.Setup = SetBit(myMessage.Setup, 0, state);
                break;
            default:
                break;
        }


        if (CbLiveUpdate.IsChecked)
        {
            Button dummy = new Button();
            myMessage.Command = cbStatusRequest.IsChecked ? PossibleCommands.SETSETUPSR : PossibleCommands.SETSETUP;
            myMessage.Type = MessageTypes.COMMAND_WITH_RESPONSE;
            myMessage.PSI = 8;
            await ExecuteCommandAsync(async () => await SendCommandAsync(dummy));
        }

    }

    private byte SetBit(byte value, int postioin, bool bit)
    {
        return (byte)(bit ? (value | (1 << postioin)) : (value & ~(1 << postioin)));
    }

    private async void SetOtthChanged(object sender, EventArgs e)
    {
        Entry entry = sender as Entry;

        if (!Byte.TryParse(entry.Text, out byte val))
        {
            LblLedState.Text = "Has to be a Number";
            return;
        }

        switch (entry.ClassId)
        {
            case "EnSetOrCycle":
                if (val >= 1 && val < 5)
                {
                    myMessage.OTTH[2] = ((byte)(val - 1));
                    EntryOrCycle.Text = val.ToString();
                    LblLedState.Text = "";
                }
                else
                {
                    LblLedState.Text = "Number between 1 and 4";
                }
                break;
            case "EnSetOtLow":
                if (val >= 0 && val < 143)
                {
                    if (myMessage.OTTH[0] > val + 113)
                    {
                        myMessage.OTTH[1] = (byte)(val + 113);
                        EntryOtLow.Text = val.ToString();
                        LblLedState.Text = "";
                    }
                    else
                    {
                        LblLedState.Text = "Temp Config error low >= high";
                    }
                }
                else
                {
                    LblLedState.Text = "Number between 0 and 142";
                }
                break;
            case "EnSetOtHigh":
                if (val >= 0 && val < 143)
                {
                    if (myMessage.OTTH[1] < val + 113)
                    {
                        myMessage.OTTH[0] = (byte)(val + 113);
                        EntryOtHigh.Text = val.ToString();
                        LblLedState.Text = "";
                    }
                    else
                    {
                        LblLedState.Text = "Temp Config error low > high";
                    }
                }
                else
                {
                    LblLedState.Text = "Number between 0 and 142";
                }
                break;
            default:
                break;
        }
        if (CbLiveUpdate.IsChecked)
        {
            Button dummy = new Button();
            myMessage.Command = cbStatusRequest.IsChecked ? PossibleCommands.SETOTTHSR : PossibleCommands.SETOTTH;
            myMessage.Type = MessageTypes.COMMAND_WITH_RESPONSE;
            myMessage.PSI = 11; // PSI 2 + Type 1 + command 1 + addr 2 + oth 3 + crc 2
            await ExecuteCommandAsync(async () => await SendCommandAsync(dummy));
        }
    }


    private CancellationTokenSource _cts;

    void cbRefreshCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value == true)
        {
            _cts = new CancellationTokenSource();
            //Start Backgorund update
            Task.Run(() => DoWorkAsync(_cts.Token));
        }
        else
        {
            //Stop Background update
            _cts.Cancel();

            //Wait for the Background Task to finish
            while (!_cts.Token.IsCancellationRequested)
            {
                Task.Delay(100);
            }
        }
    }
    Random random = new Random();
    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        Button dummy = new Button();

        myMessage.Type = MessageTypes.COMMAND_WITH_RESPONSE;
        myMessage.PSI = 8;
        myMessage.Command = PossibleCommands.READTEMPST;
        myMessage.Address = Light.SelectedLed;

        while (!cancellationToken.IsCancellationRequested)
        {
            await ExecuteCommandAsync(async () => await SendCommandAsync(dummy));
            Dispatcher.Dispatch(() => LblTest.Text = random.Next(1, 1000).ToString());
            
           // LblTest.Text = random.Next(1, 1000).ToString();
             await Task.Delay(1000, cancellationToken);
        }
    }

    private async void Listen()
    {
        LED led = Light.GetSlectedLed();
        byte[] data = new byte[64];
       
        try
        {
            while (!cTs.IsCancellationRequested)
            {
                try // catch cancel
                {
                    myMessage.Status = data[1];
                    myMessage.Temperature = (byte)(data[2] - 113);
                    myMessage.PwmBlue = BitConverter.ToUInt16(data, 3);
                    myMessage.PwmGreen = BitConverter.ToUInt16(data, 5);
                    myMessage.PwmRed = BitConverter.ToUInt16(data, 7);
                }
                catch (Exception)
                {
                    break;
                    throw;
                }
                string ans = "";
                data = await Light.connection.Receive(cTs.Token);
               
                for (int i = 0; i < data.Length; i++)
                {
                    ans += data[i].ToString() + "|";
                }

                Dispatcher.Dispatch(() => LblRawAnswer.Text = ans);
                Dispatcher.Dispatch(() => time.Text = DateTime.Now.ToString("HH:mm:ss:ff"));
                led.SetTempSt(ref myMessage);
                Dispatcher.Dispatch(() => EntryCurrentTemp.Text = led.Temperature.ToString());
                Dispatcher.Dispatch(() => LblState.Text = led.State);
                Dispatcher.Dispatch(() => LblOtpCrc.Text = led.OtpCRC);
                Dispatcher.Dispatch(() => LblCom.Text = led.Com);
                Dispatcher.Dispatch(() => LblStatusCe.Text = led.Error_ce);
                Dispatcher.Dispatch(() => LblStatusLos.Text = led.Error_los);
                Dispatcher.Dispatch(() => LblStatusOt.Text = led.Error_ot);
                Dispatcher.Dispatch(() => LblStatusUv.Text = led.Error_ce);
                Dispatcher.Dispatch(() => LblRefreshStatus.Text = led.TimestampStatus);
                Dispatcher.Dispatch(() => LblRefreshOtth.Text = led.TimestampOtth);

                led.SetPWM(ref myMessage);
                Dispatcher.Dispatch(() => lblPwmRed.Text = led.PwmRed);
                Dispatcher.Dispatch(() => lblPwmGreen.Text = led.PwmGreen);
                Dispatcher.Dispatch(() => lblPwmBlue.Text = led.PwmBlue);
                Dispatcher.Dispatch(() => LblRefreshPwm.Text = led.TimeStampPwm);

            }
            Dispatcher.Dispatch(() => lblLightState.Text = "Not receving");
        }
        catch (Exception)
        {

            throw;
        }
    }

    private static bool _isTaskRunning = false;
    private static object _look = new object();

    private async Task ExecuteCommandAsync(Func<Task> command)
    {
        Button btn = new Button();
        //Func<Task> command
        lock (_look)
        {
            if (_isTaskRunning)
            {
                Dispatcher.Dispatch(() => lblLightState.Text = "There is already a command running");
                return;
            }
            else
            {
                //Dispatcher.Dispatch(() => lblLightState.Text = "");
                _isTaskRunning = true;
            }
        }

        try
        {
            await Task.Run(async () => await SendCommandAsync(btn));
        }
        finally
        {
            _isTaskRunning = false;
            Dispatcher.Dispatch(() => lblLightState.Text = "");
        }
    }
    CancellationTokenSource cTs = new();

    private void CancleTask(object sender, EventArgs e)
    {
        cTs.Cancel();
        _isTaskRunning = false;
        Dispatcher.Dispatch(() => lblLightState.Text = "");
    }

    private async Task<bool> SendCommandAsync(Button btn)
    {
        //cTs.Cancel(); // Ensure any running task is canceld
        Dispatcher.Dispatch(() => btn.IsEnabled = false); //Disable the button

        Dispatcher.Dispatch(() => runningCommands.Text = myMessage.getCommand()); //Update Ui 

        Light.connection.SendMessage(myMessage.Serialize()); //MSG -> Byte[] //Send message

        if (myMessage.Type != MessageTypes.COMMAND_WITH_RESPONSE)
        {
            Light.connection2.Waiting = false;
            Dispatcher.Dispatch(() => btn.IsEnabled = true); //Reanable the button
            if (myMessage.Command == (PossibleCommands)PossibleDemos.TEMPCOMP )
            {
                await Task.Run(() => Listen());
            }
            return true;
        }

        // Handle Answer
        byte[] tmp = await Task.Run(() => Light.connection.ReceiveMessage(cTs.Token)); //Start receive

        Light.connection2.Waiting = false; //Next command can be send now

        if (tmp.Length == 0) //return if PSI != bytes received
        {
            Dispatcher.Dispatch(() => lblLightState.Text = "Length mismatch");
            Dispatcher.Dispatch(() => runningCommands.Text = "");
            return false;
        }

        Dispatcher.Dispatch(() => btn.IsEnabled = true); //Reanable the button after answer is received
        Dispatcher.Dispatch(() => runningCommands.Text = ""); //Remove running command
        //Display raw Answer data
        string ans = "";
        for (int i = 0; i < tmp.Length; i++)
        {
            ans += tmp[i].ToString() + "|";
        }
        Dispatcher.Dispatch(() => LblRawAnswer.Text = "Antwort: " + ans);

        //Byte[] -> MSG
        if (!myMessage.DeSerialize(tmp))
        {
            Dispatcher.Dispatch(() => lblLightState.Text = "CRC Error");
            return false;
        }

        //Display the Error if one accured
        if (!myMessage.rawErrorCode.Equals(0))
        {
            Dispatcher.Dispatch(() => LblLedState.Text = myMessage.getErrorCode());
            return false;
        }
        else
        {
            Dispatcher.Dispatch(() => LblLedState.Text = ""); //clear old error text
        }

        if (Light.LedsWereInitialized) //Cant update the ui before init is complete
        {
            if (!UpdateUi())
            {
                Dispatcher.Dispatch(() => LblLedState.Text = "Updating the UI failed");
            }
            else
            {
                Dispatcher.Dispatch(() => LblLedState.Text = "");
            }
        }


        Dispatcher.Dispatch(() => btn.IsEnabled = true); //Reanable the button after answer is received
        //Light.connection2.Waiting = false; //Next command can be send now
        //Everything worked :)
        return true;
    }

    private bool UpdateUi()
    {
        if (Light.SelectedLed > Light.LedCount)
        {
            Dispatcher.Dispatch(() => lblLightState.Text = "Selected light out of range");
            return false;
        }
        ushort addr = Light.SelectedLed;
        if (addr > 0) { addr -= 1; } // Handle offset from addr 1... zu array 0...
        LED led = Light.LEDs.ElementAt(addr);
        switch (myMessage.Command)
        {
            case PossibleCommands.RESET_LED:
                break;
            case PossibleCommands.CLEAR_ERROR:
                break;
            case PossibleCommands.INITBIDIR:
                Light.SelectedLed = (ushort)(myMessage.LedCount);
                try
                {
                    led = Light.LEDs.ElementAt(Light.SelectedLed - 1);
                }
                catch
                (Exception ex)
                {
                    Dispatcher.Dispatch(() => lblLightState.Text = "Selected light out of range");

                    return false;
                }
                led.SetTempSt(ref myMessage);
                Dispatcher.Dispatch(() => EntryCurrentTemp.Text = led.Temperature.ToString());
                Dispatcher.Dispatch(() => LblState.Text = led.State);
                Dispatcher.Dispatch(() => LblOtpCrc.Text = led.OtpCRC);
                Dispatcher.Dispatch(() => LblCom.Text = led.Com);
                Dispatcher.Dispatch(() => LblStatusCe.Text = led.Error_ce);
                Dispatcher.Dispatch(() => LblStatusLos.Text = led.Error_los);
                Dispatcher.Dispatch(() => LblStatusOt.Text = led.Error_ot);
                Dispatcher.Dispatch(() => LblStatusUv.Text = led.Error_ce);
                Dispatcher.Dispatch(() => LblRefreshStatus.Text = led.TimestampStatus);
                Dispatcher.Dispatch(() => LblRefreshOtth.Text = led.TimestampOtth);
                Dispatcher.Dispatch(() => lblSelectedLed.Text = Light.SelectedLed.ToString());
                break;
            case PossibleCommands.INITLOOP:
                break;
            case PossibleCommands.GOSLEEP:
                Dispatcher.Dispatch(() => BtnActivateLight.Text = "On");
                break;
            case PossibleCommands.GOACTIVE:
                Dispatcher.Dispatch(() => BtnActivateLight.Text = "Off");
                break;
            case PossibleCommands.GODEEPSLEEP:
                break;
            case PossibleCommands.READSTATUS:
                led.SetStatus(ref myMessage);
                Dispatcher.Dispatch(() => LblState.Text = led.State);
                Dispatcher.Dispatch(() => LblOtpCrc.Text = led.OtpCRC);
                Dispatcher.Dispatch(() => LblCom.Text = led.Com);
                Dispatcher.Dispatch(() => LblStatusCe.Text = led.Error_ce);
                Dispatcher.Dispatch(() => LblStatusLos.Text = led.Error_los);
                Dispatcher.Dispatch(() => LblStatusOt.Text = led.Error_ot);
                Dispatcher.Dispatch(() => LblStatusUv.Text = led.Error_ce);
                break;
            case PossibleCommands.SETOTTHSR:
            case PossibleCommands.SETSETUPSR:
            case PossibleCommands.SETPWMSR:
            case PossibleCommands.SETLUVSR:
            case PossibleCommands.READTEMPST:
                led.SetTempSt(ref myMessage);
                Dispatcher.Dispatch(() => EntryCurrentTemp.Text = led.Temperature.ToString());
                Dispatcher.Dispatch(() => LblState.Text = led.State);
                Dispatcher.Dispatch(() => LblOtpCrc.Text = led.OtpCRC);
                Dispatcher.Dispatch(() => LblCom.Text = led.Com);
                Dispatcher.Dispatch(() => LblStatusCe.Text = led.Error_ce);
                Dispatcher.Dispatch(() => LblStatusLos.Text = led.Error_los);
                Dispatcher.Dispatch(() => LblStatusOt.Text = led.Error_ot);
                Dispatcher.Dispatch(() => LblStatusUv.Text = led.Error_ce);
                Dispatcher.Dispatch(() => LblRefreshStatus.Text = led.TimestampStatus);
                Dispatcher.Dispatch(() => LblRefreshOtth.Text = led.TimestampOtth);
                Dispatcher.Dispatch(() => LblRefreshPwm.Text = led.TimeStampPwm);

                break;
            case PossibleCommands.READCOMST:
                led.SetComStats(ref myMessage);
                Dispatcher.Dispatch(() => LblSio1.Text = led.Cs_SIO1);
                Dispatcher.Dispatch(() => LblSio2.Text = led.Cs_SIO2);
                break;
            case PossibleCommands.READLEDST:
                led.SetLedStatus(ref myMessage);
                Dispatcher.Dispatch(() => LblRo.Text = led.RO);
                Dispatcher.Dispatch(() => LblGo.Text = led.GO);
                Dispatcher.Dispatch(() => LblBo.Text = led.BO);
                Dispatcher.Dispatch(() => LblRs.Text = led.RS);
                Dispatcher.Dispatch(() => LblGs.Text = led.BS);
                Dispatcher.Dispatch(() => LblBs.Text = led.GS);
                break;
            case PossibleCommands.READTEMP:
                led.SetTemp(ref myMessage);
                Dispatcher.Dispatch(() => EntryCurrentTemp.Text = led.Temperature.ToString());
                Dispatcher.Dispatch(() => LblRefreshOtth.Text = led.TimestampOtth);
                break;
            case PossibleCommands.READOTTH:
                led.SetOtth(ref myMessage);
                Dispatcher.Dispatch(() => EntryOrCycle.Text = led.OrCycle.ToString());
                Dispatcher.Dispatch(() => EntryOtLow.Text = led.OtLowValue.ToString());
                Dispatcher.Dispatch(() => EntryOtHigh.Text = led.OtHighValue.ToString());
                Dispatcher.Dispatch(() => LblRefreshOtth.Text = led.TimestampOtth);
                break;
            case PossibleCommands.SETOTTH:
                break;
            case PossibleCommands.READSETUP:
                led.SetSetup(ref myMessage);
                Dispatcher.Dispatch(() => LblSetPwmF.Text = led.PWM_F);
                Dispatcher.Dispatch(() => CbSetPwmF.IsChecked = led.PWM_F == "1172 Hz / 14 bit");
                Dispatcher.Dispatch(() => LblSetClkP.Text = led.CLK_INV);
                Dispatcher.Dispatch(() => CbSetClkP.IsChecked = led.CLK_INV == "LOW");
                Dispatcher.Dispatch(() => LblSetCrcEn.Text = led.CRC_EN);
                Dispatcher.Dispatch(() => CbSetCrcEn.IsChecked = led.CRC_EN == "ENABLED");
                Dispatcher.Dispatch(() => LblSetTempClk.Text = led.TEMPCLK);
                Dispatcher.Dispatch(() => CbSetTempClk.IsChecked = led.TEMPCLK == "2.4 kHz");
                Dispatcher.Dispatch(() => LblSetCe.Text = led.CE_FSAVE);
                Dispatcher.Dispatch(() => CbSetCe.IsChecked = led.CE_FSAVE == "RAISE & SLEEP");
                Dispatcher.Dispatch(() => LblSetLos.Text = led.LOS_FSAVE);
                Dispatcher.Dispatch(() => CbSetLos.IsChecked = led.LOS_FSAVE == "RAISE & SLEEP");
                Dispatcher.Dispatch(() => LblSetOt.Text = led.OT_FSAVE);
                Dispatcher.Dispatch(() => CbSetOt.IsChecked = led.OT_FSAVE == "RAISE & SLEEP");
                Dispatcher.Dispatch(() => LblSetUv.Text = led.UV_FSAVE);
                Dispatcher.Dispatch(() => CbSetUv.IsChecked = led.UV_FSAVE == "RAISE & SLEEP");

                break;
            case PossibleCommands.SETSETUP:
                break;
            case PossibleCommands.READPWM:
                led.SetPWM(ref myMessage);
                Dispatcher.Dispatch(() => lblPwmRed.Text = led.PwmRed);
                Dispatcher.Dispatch(() => lblPwmGreen.Text = led.PwmGreen);
                Dispatcher.Dispatch(() => lblPwmBlue.Text = led.PwmBlue);
                Dispatcher.Dispatch(() => LblRefreshPwm.Text = led.TimeStampPwm);

                break;
            case PossibleCommands.SETPWM:
                break;
            case PossibleCommands.READOTP:
                led.SetOTP(ref myMessage);
                Dispatcher.Dispatch(() => LblRedDayU.Text = led.RedDayU.ToString());
                Dispatcher.Dispatch(() => LblRedDayV.Text = led.RedDayV.ToString());
                Dispatcher.Dispatch(() => LblRedDayLv.Text = led.RedDayLv.ToString());
                Dispatcher.Dispatch(() => LblRedNightU.Text = led.RedNightU.ToString());
                Dispatcher.Dispatch(() => LblRedNightV.Text = led.RedNightV.ToString());
                Dispatcher.Dispatch(() => LblRedNightLv.Text = led.RedNightLv.ToString());

                Dispatcher.Dispatch(() => LblGreenDayU.Text = led.GreenDayU.ToString());
                Dispatcher.Dispatch(() => LblGreenDayV.Text = led.GreenDayV.ToString());
                Dispatcher.Dispatch(() => LblGreenDayLv.Text = led.GreenDayLv.ToString());
                Dispatcher.Dispatch(() => LblGreenNightU.Text = led.GreenNightU.ToString());
                Dispatcher.Dispatch(() => LblGreenNightV.Text = led.GreenNightV.ToString());
                Dispatcher.Dispatch(() => LblGreenNightLv.Text = led.GreenNightLv.ToString());

                Dispatcher.Dispatch(() => LblBlueDayU.Text = led.BlueDayU.ToString());
                Dispatcher.Dispatch(() => LblBlueDayV.Text = led.BlueDayV.ToString());
                Dispatcher.Dispatch(() => LblBlueDayLv.Text = led.BlueDayLv.ToString());
                Dispatcher.Dispatch(() => LblBlueNightU.Text = led.BlueNightU.ToString());
                Dispatcher.Dispatch(() => LblBlueNightV.Text = led.BlueNightV.ToString());
                Dispatcher.Dispatch(() => LblBlueNightLv.Text = led.BlueNightLv.ToString());

                Dispatcher.Dispatch(() => LblChipId.Text = led.ChipId.ToString());
                Dispatcher.Dispatch(() => LblWaverId.Text = led.WaverId.ToString());
                break;
            default:
                break;
        }
        return true;
    }

    private void ButtonIncrement(object sender, EventArgs e)
    {
        Button button = sender as Button;

        switch (button.ClassId)
        {
            case "BtnIncBrightnes":
                myMessage.Lv += 1;
                lableBrightnes.Text = myMessage.Lv.ToString();
                sliderBrightnes.Value = myMessage.Lv;
                break;
            case "BtnIncRed":
                myMessage.PwmRed += 1;
                lablePwmRed.Text = myMessage.PwmRed.ToString();
                sliderPwmRed.Value = myMessage.PwmRed;
                break;
            case "BtnIncGreen":
                myMessage.PwmGreen += 1;
                lablePwmGreen.Text = myMessage.PwmGreen.ToString();
                sliderPwmGreen.Value = myMessage.PwmGreen;
                break;
            case "BtnIncBlue":
                myMessage.PwmBlue += 1;
                lablePwmBlue.Text = myMessage.PwmBlue.ToString();
                sliderPwmBlue.Value = myMessage.PwmBlue;
                break;
            default:
                break;
        }
    }

    private void ButtonDecrement(object sender, EventArgs e)
    {
        Button button = sender as Button;
        ushort val = 1;
        switch (button.ClassId)
        {
            case "BtnDecBrightnes":
                myMessage.Lv -= 1;
                lableBrightnes.Text = myMessage.Lv.ToString();
                sliderBrightnes.Value = myMessage.Lv;
                break;
            case "BtnDecRed":
                myMessage.PwmRed -= 1;
                lablePwmRed.Text = myMessage.PwmRed.ToString();
                sliderPwmRed.Value = myMessage.PwmRed &= (ushort)~(val << 15);
                break;
            case "BtnDecGreen":
                myMessage.PwmGreen -= 1;
                lablePwmGreen.Text = myMessage.PwmGreen.ToString();
                sliderPwmGreen.Value = myMessage.PwmGreen &= (ushort)~(val << 15);
                break;
            case "BtnDecBlue":
                myMessage.PwmBlue -= 1;
                lablePwmBlue.Text = myMessage.PwmBlue.ToString();
                sliderPwmBlue.Value = myMessage.PwmBlue &= (ushort)~(val << 15);
                break;
            default:
                break;
        }
    }

    private async void OnSliderChanged(object sender, ValueChangedEventArgs e)
    {
        Slider slider = sender as Slider;
        switch (slider.ClassId)
        {
            case "sliderBrightnes":
                lableBrightnes.Text = e.NewValue.ToString();
                myMessage.Lv = (ushort)e.NewValue;
                break;
            case "sliderPwmRed":
                lablePwmRed.Text = e.NewValue.ToString();
                myMessage.PwmRed = (ushort)e.NewValue;
                break;
            case "sliderPwmGreen":
                lablePwmGreen.Text = e.NewValue.ToString();
                myMessage.PwmGreen = (ushort)e.NewValue;
                break;
            case "sliderPwmBlue":
                lablePwmBlue.Text = e.NewValue.ToString();
                myMessage.PwmBlue = (ushort)e.NewValue;
                break;
            default:
                break;
        }

        if (CbLiveUpdate.IsChecked)
        {
            if (!Light.LedCountSet || !Light.IpSet || !Light.LedsWereReset || !Light.LedsWereInitialized)
            {
                lblLightState.Text = "Init not complete";
                return;
            }
            if (CbPwmLuv.IsChecked && slider.ClassId != "sliderBrightnes")
            {
                myMessage.Command = cbStatusRequest.IsChecked ? PossibleCommands.SETPWMSR : PossibleCommands.SETPWM;
            }
            else if(!CbPwmLuv.IsChecked && slider.ClassId == "sliderBrightnes")
            {
                myMessage.Command = cbStatusRequest.IsChecked ? PossibleCommands.SETLUVSR : PossibleCommands.SETLUV;
            }
            else
            {
                lblLightState.Text = "No update, wrong slider";
                return;
            }
            myMessage.Type = MessageTypes.COMMAND_WITH_RESPONSE;
            myMessage.PSI = 14; // PSI (2) + Type (1) + Command (1) + Address (2) + Payload(6) +  CRC (2)
            Button btn = new();
            CheckBroadcast();
            await ExecuteCommandAsync(async () => await SendCommandAsync(btn));
        }
    }

    private void OnUvChange(object sender, EventArgs e)
    {
        Entry entry = sender as Entry;

        if(!UInt16.TryParse(entry.Text, out UInt16 value))
        {
            lblLightState.Text = "Nur Zahlen >= 0";
            return;
        }   
        if(value > 65535)
        {
            lblLightState.Text = "Wert muss kleiner 65535 sein";
            return;
        }
        lblLightState.Text = "";
        switch (entry.ClassId)
        {
            case "EntryU":
                myMessage.U = value;
                break;
            case "EntryV":
                myMessage.V = value;
                break;
            default:
                break;
        }
    }

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

        EntryU.Text = u.ToString();
        EntryV.Text = v.ToString();
        myMessage.U = u;
        myMessage.V = v;

        if (CbLiveUpdate.IsChecked)
        {
            if (!Light.LedCountSet || !Light.IpSet || !Light.LedsWereReset || !Light.LedsWereInitialized)
            {
                lblLightState.Text = "Init not complete";
                return;
            }

            if (CbPwmLuv.IsChecked)
            {
                myMessage.Command = cbStatusRequest.IsChecked ? PossibleCommands.SETPWMSR : PossibleCommands.SETPWM;
            }
            else
            {
                myMessage.Command = cbStatusRequest.IsChecked ? PossibleCommands.SETLUVSR : PossibleCommands.SETLUV;
            }
            myMessage.Type = MessageTypes.COMMAND_WITH_RESPONSE;
            myMessage.PSI = 14; // PSI (2) + Type (1) + Command (1) + Address (2) + Payload(6) +  CRC (2)
            Button btn = new();
            CheckBroadcast();
            await ExecuteCommandAsync(async () => await SendCommandAsync(btn));
        }
        ToolTipProperties.SetText(elli, $"u:{u}|v: {v}");
        await elli.TranslateTo(p.Value.X -120, p.Value.Y - 107, 200, Easing.Linear);
    }

    private async void UpdateColor(object sender, EventArgs e)
    {
        if (!Light.LedCountSet || !Light.IpSet || !Light.LedsWereReset || !Light.LedsWereInitialized)
        {
            lblLightState.Text = "Init not complete";
            return;
        }

        Button btn = sender as Button;
        if (CbPwmLuv.IsChecked)
        {
            myMessage.Command = cbStatusRequest.IsChecked ? PossibleCommands.SETPWMSR : PossibleCommands.SETPWM;
        }
        else
        {
            myMessage.Command = cbStatusRequest.IsChecked ? PossibleCommands.SETLUVSR : PossibleCommands.SETLUV;
        }
        myMessage.Type = MessageTypes.COMMAND_WITH_RESPONSE;
        myMessage.PSI = 14; // PSI (2) + Type (1) + Command (1) + Address (2) + Payload(6) +  CRC (2)
        CheckBroadcast();
        await ExecuteCommandAsync(async () => await SendCommandAsync(btn));
    }

    private void CheckboxChanged(object sender, CheckedChangedEventArgs e)
    {
        CheckBox cb = sender as CheckBox;

        switch (cb.ClassId)
        {
            case "cbCurrentRed":
                lblCurrentRed.Text = (e.Value == true ? "Red 10mA": "Red 50mA");
                myMessage.CurrentRed = !e.Value;
                break;
            case "cbCurrentGreen":
                lblCurrentGreen.Text = (e.Value == true ? "Green 10mA" : "Green 50mA");
                myMessage.CurrentGreen = !e.Value;
                break;
            case "cbCurrentBlue":
                lblCurrentBlue.Text = (e.Value == true ? "Blue 10mA" : "Blue 50mA");
                myMessage.CurrentBlue = !e.Value;
                break;
            case "CbPwmLuv":
                LblColor.Text = (e.Value == true ? "PWM" : "Luv");
                break;

            default:
                break;
        }
    }

    private void UpdateUiWhenSelectedLedChanged()
    {
        if(Light.SelectedLed == 0)
        {
            return;
        }

        LED led = Light.LEDs.ElementAt(Light.SelectedLed - 1);

        //Comstats
        LblSio1.Text = led.Cs_SIO1;
        LblSio2.Text = led.Cs_SIO2;
        LblRefreshComstats.Text = led.TimeStampComstats;
        //Stauts
        LblState.Text = led.State;
        LblOtpCrc.Text = led.OtpCRC;
        LblCom.Text = led.Com;
        LblStatusCe.Text = led.Error_ce;
        LblStatusLos.Text = led.Error_los;
        LblStatusOt.Text = led.Error_ot;
        LblStatusUv.Text = led.Error_ce;
        LblRefreshStatus.Text = led.TimestampStatus;
        //Temp and OTTH
        EntryCurrentTemp.Text = led.Temperature.ToString();
        EntryOrCycle.Text = led.OrCycle.ToString();
        EntryOtLow.Text = led.OtLowValue.ToString();
        EntryOtHigh.Text = led.OtHighValue.ToString();
        LblRefreshOtth.Text = led.TimestampOtth;
        //PWM
        lblPwmRed.Text = led.PwmRed;
        lblPwmGreen.Text = led.PwmGreen;
        lblPwmBlue.Text = led.PwmBlue;
        LblRefreshPwm.Text = led.TimeStampPwm;
        //LED Stat
        LblRo.Text = led.RO;
        LblGo.Text = led.GO;
        LblBo.Text = led.BO;
        LblRs.Text = led.RS;
        LblGs.Text = led.BS;
        LblBs.Text = led.GS;
        LblRefreshLedSt.Text = led.TimestampLedState;
        //Setup
        LblSetPwmF.Text = led.PWM_F;
        CbSetPwmF.IsChecked = led.PWM_F == "1172 Hz / 14 bit";
        LblSetClkP.Text = led.CLK_INV;
        CbSetClkP.IsChecked = led.CLK_INV == "LOW";
        LblSetCrcEn.Text = led.CRC_EN;
        CbSetCrcEn.IsChecked = led.CRC_EN == "ENABLED";
        LblSetTempClk.Text = led.TEMPCLK;
        CbSetTempClk.IsChecked = led.TEMPCLK == "2.4 kHz";
        LblSetCe.Text = led.CE_FSAVE;
        CbSetCe.IsChecked = led.CE_FSAVE == "RAISE & SLEEP";
        LblSetLos.Text = led.LOS_FSAVE;
        CbSetLos.IsChecked = led.LOS_FSAVE == "RAISE & SLEEP";
        LblSetOt.Text = led.OT_FSAVE;
        CbSetOt.IsChecked = led.OT_FSAVE == "RAISE & SLEEP";
        LblSetUv.Text = led.UV_FSAVE;
        CbSetUv.IsChecked = led.UV_FSAVE == "RAISE & SLEEP";
        LblRefreshSetup.Text = led.TimestampSetup;
        //OTP
        LblRedDayU.Text = led.RedDayU.ToString();
        LblRedDayV.Text = led.RedDayV.ToString();
        LblRedDayLv.Text = led.RedDayLv.ToString();
        LblRedNightU.Text = led.RedNightU.ToString();
        LblRedNightV.Text = led.RedNightV.ToString();
        LblRedNightLv.Text = led.RedNightLv.ToString();
        LblGreenDayU.Text = led.GreenDayU.ToString();
        LblGreenDayV.Text = led.GreenDayV.ToString();
        LblGreenDayLv.Text = led.GreenDayLv.ToString();
        LblGreenNightU.Text = led.GreenNightU.ToString();
        LblGreenNightV.Text = led.GreenNightV.ToString();
        LblGreenNightLv.Text = led.GreenNightLv.ToString();
        LblBlueDayU.Text = led.BlueDayU.ToString();
        LblBlueDayV.Text = led.BlueDayV.ToString();
        LblBlueDayLv.Text = led.BlueDayLv.ToString();
        LblBlueNightU.Text = led.BlueNightU.ToString();
        LblBlueNightV.Text = led.BlueNightV.ToString();
        LblBlueNightLv.Text = led.BlueNightLv.ToString();
        LblChipId.Text = led.ChipId.ToString();
        LblWaverId.Text = led.WaverId.ToString();

        //Btn and LED Addr
        lblSelectedLed.Text = Light.SelectedLed.ToString();
        BtnActivateLight.Text = led.State == "Active" ? "ON" : "OFF";
    }
}
