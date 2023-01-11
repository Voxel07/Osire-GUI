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

    private async void CommandReset(object sender, EventArgs e)
    {
        Button btn = sender as Button;
        if (!Light.IpSet || !Light.LedCountSet)
        {
            lblLightState.Text = "Light config missing";
            return;
        }

        myMessage.SetMessageToReset();
        await Task.Run(async () => await SendCommandAsync(btn));

        if (myMessage.Error == 0)
        {
            cbStateLightReset.IsChecked = true;
            BtnInitLED.IsEnabled = true;
            Light.LedsWereReset = true;
        }
    }

    private async void InitLED(object sender, EventArgs e)
    {
        if (!Light.IpSet || !Light.LedCountSet)
        {
            lblLightState.Text = "Light config missing";
            return;
        }

        Button btn = sender as Button;

        myMessage.SetMessageToInit();
        await Task.Run(async () => await SendCommandAsync(btn));

        if (myMessage.Error == 0)
        {
            Light.LedsWereInitialized = true;
            Light.InitLeds();
            countedLeds.ItemsSource = Light.LEDs;
            cbStateLightInit.IsChecked = true;
            BtnSetConfig.IsEnabled = true;
            //Activate the led selection
            countedLeds.IsVisible = true;
            ledAddr.IsVisible = true;
            lblledAddr.IsVisible = true;
           
        }
      
    }

    private async void SetConfig(object sender, EventArgs e)
    {
        if (!Light.IpSet || !Light.LedCountSet)
        {
            lblLightState.Text = "Light config missing";
            return;
        }

        Button btn = sender as Button;

        myMessage.SetMessateToDefaultConf();

        await Task.Run(async () => await SendCommandAsync(btn));

        if (myMessage.Error == 0)
        {
            cbStateLightSetup.IsChecked = true;
            BtnActivateLed.IsEnabled = true;
            Light.LedsWereReset = true;
        }


    }

    private async void ActivateLed(object sender, EventArgs e)
    {
        Button btn = sender as Button;
        myMessage.Type = MessageTypes.COMMAND_WITH_RESPONSE;
        myMessage.PSI = 7;
        myMessage.Address = 0;

        if (BtnActivateLed.Text == "On")
        {
            BtnActivateLed.Text = "Off";
            myMessage.Command = PossibleCommands.GOSLEEP;

        }
        else
        {
            BtnActivateLed.Text = "On";
            myMessage.Command = PossibleCommands.GOACTIVE;
        }
            await Task.Run(async () => await SendCommandAsync(btn));
    }

    private async void ToggleLed(object sender, EventArgs e)
    {
        Button btn = sender as Button;
        LED led = Light.LEDs[Light.SelectedLed - 1];
        
        myMessage.PSI = 7;
        
        if(led.State == "ACTIVE")
        {
            myMessage.Command = PossibleCommands.GOSLEEP;
            BtnToggleLed.Text = "Off";
        }
        else
        {
            myMessage.Command = PossibleCommands.GOACTIVE;
            BtnToggleLed.Text = "On";
        }

        await Task.Run(async () => await SendCommandAsync(btn));

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
        BtnInitLED.IsEnabled = true;
        //if(pos > 0) { pos -= 1; } 
        //countedLeds.ScrollTo(Light.LEDs.ElementAt(pos), ScrollToPosition.Start, true);
        lblSelectedLed.Text = pos.ToString();
        ledAddr.Text = pos.ToString();
        myMessage.Address = (ushort)pos;
        Light.SelectedLed = (ushort)pos;

    }

    //Handle LED selection from ListView
    void svLedSelected(object sender, SelectedItemChangedEventArgs e)
    {
        LED led = e.SelectedItem as LED;
        lblSelectedLed.Text = led.Address.ToString();
        myMessage.Address = led.Address;
        Light.SelectedLed = led.Address;
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
        myMessage.PSI = 7; // PSI (1) + Type (1) + Command (1) + Address (2) +  CRC (2)

        await Task.Run(async () => await SendCommandAsync(btn));

    }
    private async void SetCommand(object sender , EventArgs e)
    {
        if(!Light.LedCountSet || !Light.IpSet)
        {
            LblLedState.Text = "Init not complete";
            return;
        }
        Button btn = sender as Button;
        myMessage.PSI = 8;

        switch (btn.ClassId)
        {
            case "BtnSetSetup":
                myMessage.Command = PossibleCommands.SETSETUP;
                break;
            case "BtnSetOtth":
                myMessage.Command = PossibleCommands.SETOTTH;
                break;
            default:
                break;
        }
        await Task.Run(async () => await SendCommandAsync(btn));
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
        LED led = Light.LEDs.ElementAt(selectedLed);

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
                myMessage.Setup |= (byte)((state ? 1 : 0) << 8);
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
                myMessage.Setup |= (byte)((state ? 1 : 0) << 7);
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
                myMessage.Setup |= (byte)((state ? 1 : 0) << 6);
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
                myMessage.Setup |= (byte)((state ? 1 : 0) << 5);
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
                myMessage.Setup |= (byte)((state ? 1 : 0) << 4);
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
                myMessage.Setup |= (byte)((state ? 1 : 0) << 3);
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
                myMessage.Setup |= (byte)((state ? 1 : 0) << 2);
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
                myMessage.Setup |= (byte)((state ? 1 : 0) << 1);
                break;
            default:
                break;
        }

        Button dummy = new Button();
        myMessage.Command = PossibleCommands.SETSETUP;
        myMessage.Type = MessageTypes.COMMAND_WITH_RESPONSE;
        myMessage.PSI = 8;
        await Task.Run(async () => await SendCommandAsync(dummy));

    }

    private async void SetOtthChanged(object sender, EventArgs e)
    {
        Entry entry = sender as Entry;
        
        if(!Byte.TryParse(entry.Text, out byte val))
        {
            LblLedState.Text = "Has to be a Number";
            return;
        }

        switch (entry.ClassId)
        {
            case "EnSetOrCycle":
                if(val >= 1 && val < 5)
                {
                    myMessage.OTTH[2] = ((byte)(val - 1));
                    LblSetOrCycle.Text = val + " cycle";
                    LblLedState.Text = "";
                }
                else
                {
                    LblLedState.Text = "Number between 1 and 4";
                }
                break;
            case "EnSetOtLow":
                if(val >= 0 && val < 143)
                {
                    myMessage.OTTH[1] = (byte)(val + 113);
                    LblSetOtLow.Text = val.ToString() + "°C";
                    LblLedState.Text = "";
                }
                else
                {
                    LblLedState.Text = "Number between 0 and 142";
                }
                break;
            case "EnSetOtHigh":
                if (val >= 0 && val < 143)
                {
                    if (myMessage.OTTH[1] < val)
                    {
                        myMessage.OTTH[0] = (byte)(val + 113);
                        LblSetOtHigh.Text = val.ToString() + "°C";
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
        Button dummy = new Button();
        myMessage.Command = PossibleCommands.SETOTTH;
        myMessage.Type = MessageTypes.COMMAND_WITH_RESPONSE;
        myMessage.PSI = 10;
        await Task.Run(async () => await SendCommandAsync(dummy));
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
        Dispatcher.Dispatch(() => btn.IsEnabled = false); //Disable the button

        Light.connection.Waiting = true; //Block till command is complete
        Dispatcher.Dispatch(() => runningCommands.Text = myMessage.getCommand()); //Update Ui 
       
        Light.connection.SendMessage(myMessage.Serialize()); //MSG -> Byte[] //Send message

        if(myMessage.Type != MessageTypes.COMMAND_WITH_RESPONSE)
        {
            Light.connection2.Waiting = false;
            Dispatcher.Dispatch(() => btn.IsEnabled = true); //Reanable the button
            return true;
        }

        // Handle Answer
        byte[] tmp = await Task.Run(() => Light.connection.ReceiveMessage()); //Start receive
       
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
        if (!myMessage.DeSerialize(tmp))
        {
            Dispatcher.Dispatch(() => lblLightState.Text = "CRC Error");
            return false;
        }

        //Display the Error if one accured
        if (myMessage.Error != 0)
        {
            Dispatcher.Dispatch(() => LblLedState.Text = myMessage.getErrorCode());
            return false;
        }
        else
        {
            Dispatcher.Dispatch(() => LblLedState.Text =""); //clear old error text
        }

        if(!UpdateUi())
        {
            Dispatcher.Dispatch(() => LblLedState.Text = "Updating the UI failed");
        }

        Dispatcher.Dispatch(() => btn.IsEnabled = true); //Reanable the button after answer is received
        Light.connection2.Waiting = false; //Next command can be send now
        //Everything worked :)
        return true;
    }

    private bool UpdateUi()
    {   
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
                led = Light.LEDs.ElementAt(Light.SelectedLed - 1);
                led.SetTempSt(ref myMessage);
                Dispatcher.Dispatch(() => LblTemp.Text = led.Temperature + "°C");
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
                break;
            case PossibleCommands.GOACTIVE:
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
            case PossibleCommands.READTEMPST:
                led.SetTempSt(ref myMessage);
                Dispatcher.Dispatch(() => LblTemp.Text = led.Temperature.ToString());
                Dispatcher.Dispatch(() => LblState.Text = led.State);
                Dispatcher.Dispatch(() => LblOtpCrc.Text = led.OtpCRC);   
                Dispatcher.Dispatch(() => LblCom.Text = led.Com);
                Dispatcher.Dispatch(() => LblStatusCe.Text = led.Error_ce);
                Dispatcher.Dispatch(() => LblStatusLos.Text = led.Error_los);
                Dispatcher.Dispatch(() => LblStatusOt.Text = led.Error_ot);
                Dispatcher.Dispatch(() => LblStatusUv.Text = led.Error_ce);
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
                Dispatcher.Dispatch(() => LblTemp.Text = led.Temperature.ToString());
                break;
            case PossibleCommands.READOTTH:
                led.SetOtth(ref myMessage);
                Dispatcher.Dispatch(() => LblOrCycle.Text = led.OrCycle + " cycle");
                Dispatcher.Dispatch(() => LblTempMin.Text = led.OtLowValue + "°C");
                Dispatcher.Dispatch(() => LblTempMax.Text = led.OtHighValue + "°C");

                break;
            case PossibleCommands.SETOTTH:
                break;
            case PossibleCommands.READSETUP:
                led.SetSetup(ref myMessage);
                Dispatcher.Dispatch(() => LblPwmF.Text = led.PWM_F);
                Dispatcher.Dispatch(() => LblClkPolarity.Text = led.CLK_INV);
                Dispatcher.Dispatch(() => LblCrc.Text = led.CRC_EN);
                Dispatcher.Dispatch(() => LblTempClk.Text = led.TEMPCLK);
                Dispatcher.Dispatch(() => LblSetupCe.Text = led.CE_FSAVE);
                Dispatcher.Dispatch(() => LblSetupLos.Text = led.LOS_FSAVE);
                Dispatcher.Dispatch(() => LblSetupOt.Text = led.OT_FSAVE);
                Dispatcher.Dispatch(() => LblSetupUv.Text = led.UV_FSAVE);
                break;
            case PossibleCommands.SETSETUP:
                break;
            case PossibleCommands.READPWM:
                led.SetPWM(ref myMessage);
                Dispatcher.Dispatch(() => lblPwmRed.Text = led.PwmRed);
                Dispatcher.Dispatch(() => lblPwmGreen.Text = led.PwmGreen);
                Dispatcher.Dispatch(() => lblPwmBlue.Text = led.PwmBlue);
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

        float red = (float)myMessage.PwmRed / 32767;
        float green = (float)myMessage.PwmGreen / 32767;
        float blue = (float)myMessage.PwmBlue / 32767;
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



