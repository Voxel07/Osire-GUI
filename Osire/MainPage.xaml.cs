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

namespace Osire;

public partial class MainPage : ContentPage
{
    public MainPage()
	{
		InitializeComponent();
    }

    //Calsses that will be filled with Data  by the UI
    Leuchte Light = new Leuchte();
    Message myMessage = new Message();

    private void SendCommand(object sender, EventArgs e)
    {
        //SendStuff.IsEnabled= false;
        myMessage.setCommand(SETPWM);
        myMessage.Type = COMMAND_WITH_RESPONSE;
        myMessage.Address = 0;

        //Task.Run(() => SendCommandAsync());
        //SendCommandAsync();

        //Update UI
        runningCommands.Text = "Sending Dummy";
        //runningCommandsContainer.ItemsSource = runningCommandsContainer.It
        
    }

    private async void CommandReset(object sender, EventArgs e)
    {
        Button btn = sender as Button;
        if (!Light.IpSet || !Light.LedCountSet)
        {
            lblLightState.Text = "Light config missing";
            return;
        }

        myMessage.SetMessageToReset();
        btn.IsEnabled = false;
        await Task.Run(async () => await SendCommandAsync(btn));
        if(myMessage.Error == 0)
        {
            cbStateLightReset.IsChecked = true;
            BtnInitLED.IsEnabled= true;
            Light.LedsWereReset = true;
        }
    }

    private async void InitLED(object sender, EventArgs e)
    {
        Button btn = sender as Button;

        myMessage.SetMessageToInit();
        BtnInitLED.IsEnabled = false;
        await Task.Run(async () => await SendCommandAsync(btn));


        //if (myMessage.Error != 0) return;
        
        Light.LedsWereInitialized = true;
        Light.InitLeds(myMessage.LedCount);

        countedLeds.ItemsSource = Light.LEDs;
        cbStateLightInit.IsChecked = true;
        //Activate the led selection
        countedLeds.IsVisible = true;
        ledAddr.IsVisible = true;
        lblledAddr.IsVisible = true;
 

    }

    private async void ActivateLed(object sender, EventArgs e)
    {
        Button btn = sender as Button;

        if (BtnActivateLed.Text == "On")
        {
            BtnActivateLed.Text = "Off";
        }
        else
        {
            myMessage.Command = PossibleCommands.GOSLEEP;
            myMessage.Type = MessageTypes.COMMAND_WITH_RESPONSE;
            myMessage.PSI = 7;
            await Task.Run(async () => await SendCommandAsync(btn));

            BtnActivateLed.Text = "On";
        }
    }

    private void HandleIpChange(object sender, TextChangedEventArgs e)
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
            cbStateIp.IsChecked= true;
            Light.SetIp(ipString);
            Light.IpSet = true;

            if(Light.LedCountSet)
            {
                btnResetLed.IsEnabled = true;
            }
        }
        else
        {
            Light.IpSet = false;
            cbStateIp.IsChecked= false;
        }
	}

    private void HandleLedCountChange(object sender, TextChangedEventArgs e)
    {
        string ledCountText = entry_LedCount.Text;
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
        }

        if (ledCount > 0 && ledCount < 1024)
        {
            cbStateLightCount.IsChecked = true;
            Light.LedCountSet = true;
            Light.SetLeds((ushort)ledCount);

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
    private void EntryLedAddr(object sender, EventArgs e)
    {
        string addresse = ledAddr.Text;
        if (!int.TryParse(addresse, out int pos))
        {
            return;
        }
        if (pos > Light.LedCount)
        {
            pos = Light.LedCount;
        }
        BtnInitLED.IsEnabled= true;
        countedLeds.ScrollTo(Light.LEDs.ElementAt(pos - 1), ScrollToPosition.Start , true);
        lblSelectedLed.Text = pos.ToString();
        ledAddr.Text = pos.ToString();
        myMessage.Address = (ushort)pos;

    }

    //Handle LED selection from ListView
    async void svLedSelected(object sender, SelectedItemChangedEventArgs e)
    {
        LED led = e.SelectedItem as LED;
        lblSelectedLed.Text = led.Address.ToString();
        myMessage.Address = led.Address;
    }

    async void Refresh(object sender, EventArgs e)
    {
        if(myMessage.Address == 0)
        {
            LblLedState.Text = "Select a LED first";
            return;
        }
        LblLedState.Text = "";
        Button btn = sender as Button;
        String timeStamp = DateTime.Now.ToString("HH:mm:ss");

        switch (btn.ClassId)
        {
            case "BtnRefreshStatus": myMessage.Command = PossibleCommands.READSTATUS; LblRefreshStatus.Text = timeStamp; break;
            case "BtnRefreshComstats": myMessage.Command = PossibleCommands.READCOMST; LblRefreshComstats.Text = timeStamp; break;
            case "BtnRefreshError": myMessage.Command = PossibleCommands.READLEDST; LblRefreshError.Text = timeStamp; break;
            case "BtnRefreshTemp": myMessage.Command = PossibleCommands.READTEMP; break;
            case "BtnRefreshOTTH": myMessage.Command = PossibleCommands.READOTTH;  break;
            case "BtnRefreshSetup": myMessage.Command = PossibleCommands.READSETUP; LblRefreshSetup.Text = timeStamp; break;
            case "BtnRefreshPwm": myMessage.Command = PossibleCommands.READPWM; LblRefreshPwm.Text = timeStamp; break;
            case "BtnRefreshOtp": myMessage.Command = PossibleCommands.READOTP; break;
            default:
                break;
        }
        myMessage.Type = MessageTypes.COMMAND_WITH_RESPONSE;
        myMessage.PSI = 7; // PSI (1) + Type (1) + Command (1) + Address (2) +  CRC (2)

        btn.IsEnabled = false;
        await Task.Run(async() => await SendCommandAsync(btn));

    }


    private CancellationTokenSource _cts;

    async void cbRefreshCheckedChanged (object sender, CheckedChangedEventArgs e)
    {
        //Use READTEMPST instead of READTEMP and READST
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

            await Task.Delay(1000);
        }
    }

    private async Task<bool> SendCommandAsync(Button btn)
    {
        //Check if a command is still running
        if (Light.connection2.Waiting)
        {
            Dispatcher.Dispatch(() => lblLightState.Text = "There is already a command running");
            return false;
        }

        Light.connection2.Waiting = true; //Block till command is complete
        Dispatcher.Dispatch(() => runningCommands.Text = myMessage.getCommand()); //Update Ui 
        
        Light.connection2.SendMessage(myMessage.Serialize()); //MSG -> Byte[] //Send message

        if(myMessage.Type != MessageTypes.COMMAND_WITH_RESPONSE)
        {
            Light.connection2.Waiting = false;
            Dispatcher.Dispatch(() => btn.IsEnabled = true); //Reanable the button
            return true;
        }

        // Handle Asnwer
        byte[] tmp = await Task.Run(() => Light.connection2.ReceiveMessage()); //Start receive
       
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
        Dispatcher.Dispatch(() => lblLightState.Text = "Antwort: " + ans);

        //Byte[] -> MSG
        if(!myMessage.DeSerialize(tmp))
        { 
            Dispatcher.Dispatch(() => lblLightState.Text = "CRC Error");
            return false;
        }

        //Display the Error if one accured
        if (myMessage.Error != 0)
        {
            Dispatcher.Dispatch(() => LblLedState.Text = myMessage.getErrorCode());
        }

        if(UpdateUi())
        {
            Dispatcher.Dispatch(() => LblLedState.Text = "Updating the UI failed");
        }


        //Everything worked :)
        return true;
    }

    private bool UpdateUi()
    {
        LED led = Light.LEDs.ElementAt(myMessage.Address);
        switch (myMessage.Command)
        {
            case PossibleCommands.RESET_LED:
                break;
            case PossibleCommands.CLEAR_ERROR:
                break;
            case PossibleCommands.INITBIDIR:
                break;
            case PossibleCommands.INITLOOP:
                break;
            case PossibleCommands.GOSLEEP:
                break;
            case PossibleCommands.GOACTIVE:
                break;
            case PossibleCommands.GODEEPSLEEP:
                break;
            case PossibleCommands.READSTATUS:
                led.SetStatus(ref myMessage);
                LblState.Text = led.State;
                LblOtpCrc.Text = led.OtpCRC;
                LblCom.Text = led.Com;
                LblStatusCe.Text = led.Error_ce;
                LblStatusLos.Text = led.Error_los;
                LblStatusOt.Text = led.Error_ot;
                LblStatusUv.Text = led.Error_ce;
                break;
            case PossibleCommands.READTEMPST:
                break;
            case PossibleCommands.READCOMST:
                led.SetComStats(ref myMessage);
                LblSio1.Text = led.Cs_SIO1;
                LblSio2.Text = led.Cs_SIO2;
                break;
            case PossibleCommands.READLEDST:
                break;
            case PossibleCommands.READTEMP:
                break;
            case PossibleCommands.READOTTH:
                break;
            case PossibleCommands.SETOTTH:
                break;
            case PossibleCommands.READSETUP:
                led.SetSetup(ref myMessage);
                LblPwmF.Text = led.PWM_F;
                LblClkPolarity.Text = led.CLK_INV;
                LblCrc.Text = led.CRC_EN;
                LblTempClk.Text = led.TEMPCLK;
                LblSetupCe.Text = led.CE_FSAVE;
                LblSetupLos.Text = led.LOS_FSAVE;
                LblSetupOt.Text = led.OT_FSAVE;
                LblSetupUv.Text = led.UV_FSAVE;
                break;
            case PossibleCommands.SETSETUP:
                break;
            case PossibleCommands.READPWM:
                led.SetPWM(ref myMessage);
                lblPwmRed.Text = led.PwmRed;
                lblPwmGreen.Text = led.PwmGreen;
                lblPwmBlue.Text = led.PwmBlue;
                break;
            case PossibleCommands.SETPWM:
                break;
            case PossibleCommands.READOTP:
                break;
            default:
                break;
        }
        return true;
    }

    async void OnSliderChanged(object sender, ValueChangedEventArgs e)
    {

        Slider slider = sender as Slider;
        switch (slider.ClassId)
        {
            case "sliderRed":
                lableRed.Text = e.NewValue.ToString();
                myMessage.PwmRed = (ushort)e.NewValue;
                break;
            case "sliderGreen":
                lableGreen.Text = e.NewValue.ToString();
                myMessage.PwmGreen = (ushort)e.NewValue;
                break;
            case "sliderBlue":
                lableBlue.Text = e.NewValue.ToString();
                myMessage.PwmBlue = (ushort)e.NewValue;
                break;
            case "sliderDelay":
                lableDelay.Text = e.NewValue.ToString();
                myMessage.Delay = (ushort)e.NewValue;
                break;
            default:
                break;
        }

        float red = (float)myMessage.PwmRed / 32768;
        float green = (float)myMessage.PwmGreen / 32768;
        float blue = (float)myMessage.PwmBlue / 32768;
        colorBoxView.BackgroundColor = new Color(red, green, blue);
        colorBoxValue.TextColor = new Color(1 - red, 1 - green, 1 - blue);
        colorBoxValue.Text = colorBoxView.BackgroundColor.ToHex().ToString();

        if (CbLiveUpdate.IsChecked)
        {
            if (!Light.LedCountSet || !Light.IpSet || !Light.LedsWereReset || !Light.LedsWereInitialized)
            {
                lblLightState.Text = "Init not complete";
                return;
            }
            myMessage.Command = PossibleCommands.SETPWM;
            myMessage.PSI = 13; // PSI (1) + Type (1) + Command (1) + Address (2) + Payload(6) +  CRC (2)
            Button btn = new();
            await Task.Run(async () => await SendCommandAsync(btn));
        }
    }

    private async void UpdateColor(object sender, EventArgs e)
    {
        if (!Light.LedCountSet || !Light.IpSet || !Light.LedsWereReset || !Light.LedsWereInitialized)
        {
            lblLightState.Text = "Init not complete";
            return;
        }
        Button btn = sender as Button;
        btn.IsEnabled= false;
        myMessage.Command = PossibleCommands.SETPWM;
        myMessage.Type = MessageTypes.COMMAND_WITH_RESPONSE;
        myMessage.PSI = 13; // PSI (1) + Type (1) + Command (1) + Address (2) + Payload(6) +  CRC (2)
        await Task.Run(async () => await SendCommandAsync(btn));

    }

    private void cbCurrentRedChanged(object sender, CheckedChangedEventArgs e)
    {
        if(e.Value)
        {
            lblCurrentRed.Text = "Red 10mA";
            myMessage.CurrentRed = false;
        }
        else
        {
            lblCurrentRed.Text = "Red 50mA";
            myMessage.CurrentRed = true;
        }
    }

    private void cbCurrentGreenChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value)
        {
            lblCurrentGreen.Text = "Green 10mA";
            myMessage.CurrentGreen = false;
        }
        else
        {
            lblCurrentGreen.Text = "Green 50mA";
            myMessage.CurrentGreen = true;
        }
    }

    private void cbCurrentBlueChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value)
        {
            lblCurrentBlue.Text = "Blue 10mA";
            myMessage.CurrentBlue = false;
        }
        else
        {
            lblCurrentBlue.Text = "Blue 50mA";
            myMessage.CurrentBlue = true;
        }
    }
}



