using Gadgeteer.Modules.GHIElectronics;
using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GadgeteerRHT03;
using CW.NETMF;

namespace System.Diagnostics
{
    public enum DebuggerBrowsableState
    {
        Collapsed,
        Never,
        RootHidden
    }
}
namespace BreadboardExperiment
{
    public partial class Program
    {
        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            Debug.Print("Program Started");
            //thread untuk servo
            var th1 = new Thread(new ThreadStart(TestServo)) ;
            th1.Start();
            //thread untuk pir sensor dan led
            var th2 = new Thread(new ThreadStart(TestPIR));
            th2.Start();
            //thread untuk temp dht 11 sensor
            var th3 = new Thread(new ThreadStart(TestTempSensor));
            th3.Start();

        }

        void TestTempSensor()
        {
            
            //tancep ke pin 8 dan pin 9
            var pin8 = breadBoardX1.Socket.CpuPins[8];
            var pin9 = breadBoardX1.Socket.CpuPins[9];
            //pake 3.3V
            var RHT03 = new Dht11Sensor(pin8,pin9, PullUpResistor.Internal);
            while (true)
            {
                if (RHT03.Read())
                {
                    var Temperature = RHT03.Temperature;
                    var Humidity = RHT03.Humidity;
                    Debug.Print("DHT sensor Read() ok, RH = " + Humidity.ToString("F1") + "%, Temp = " + Temperature.ToString("F1") + "°C " + (Temperature * 1.8 + 32).ToString("F1") + "°F");
                }
                Thread.Sleep(1000);
            }
        }

        void TestPIR()
        {
            //led merah pake pin 6
            var ledRed = breadBoardX1.CreateDigitalOutput(GT.Socket.Pin.Six,false);
            //led ijo pake pin 5
            var greenLed = breadBoardX1.CreateDigitalOutput(GT.Socket.Pin.Five, false);
            //pir pake pin 3 + 5v
            var pir = breadBoardX1.CreateDigitalInput(Gadgeteer.Socket.Pin.Three, Gadgeteer.SocketInterfaces.GlitchFilterMode.Off,Gadgeteer.SocketInterfaces.ResistorMode.Disabled);
            while (true)
            {
                var detect = pir.Read();
                if(detect)
                {
                    ledRed.Write(true);
                    greenLed.Write(false);
                    Debug.Print("ada gerakan");
                }
                else
                {
                    greenLed.Write(true);
                    ledRed.Write(false);
                    Debug.Print("tidak ada gerakan");
                }
                Thread.Sleep(200);
            }
        }

        void TestServo()
        {
            //servo pake pin 7 + 5.5v
            var servo= breadBoardX1.CreatePwmOutput(Gadgeteer.Socket.Pin.Seven);
            //potension meter pake pin 4
            var potensio = breadBoardX1.CreateAnalogInput(GT.Socket.Pin.Four);
            var oldpct = 0.0;
            uint max = 2400;
            uint min = 600;
            uint delta = max-min;
            while (true)
            {
                var pct = potensio.ReadProportion();
                try
                {
                    if (System.Math.Abs(pct-oldpct)>=0.1)
                    {
                        var adds = (uint)(pct * delta);
                        uint hightime = min + adds;
                        servo.Set(20000, hightime, GT.SocketInterfaces.PwmScaleFactor.Microseconds);
                        oldpct = pct;
                        Thread.Sleep(100);
                    }
                    /*
                    //geser dari kiri ke kanan
                    servo.Set(20000, 600, GT.SocketInterfaces.PwmScaleFactor.Microseconds);
                    Thread.Sleep(1000);
                    servo.Set(20000, 750, GT.SocketInterfaces.PwmScaleFactor.Microseconds);
                    Thread.Sleep(1000);
                    servo.Set(20000, 1000, GT.SocketInterfaces.PwmScaleFactor.Microseconds);
                    Thread.Sleep(1000);
                    servo.Set(20000, 1250, GT.SocketInterfaces.PwmScaleFactor.Microseconds);
                    Thread.Sleep(1000);
                    servo.Set(20000, 1500, GT.SocketInterfaces.PwmScaleFactor.Microseconds);
                    Thread.Sleep(1000);
                    servo.Set(20000, 1750, GT.SocketInterfaces.PwmScaleFactor.Microseconds);
                    Thread.Sleep(1000);
                    servo.Set(20000, 2000, GT.SocketInterfaces.PwmScaleFactor.Microseconds);
                    Thread.Sleep(1000);
                    servo.Set(20000, 2250, GT.SocketInterfaces.PwmScaleFactor.Microseconds);
                    Thread.Sleep(1000);
                    servo.Set(20000, 2400, GT.SocketInterfaces.PwmScaleFactor.Microseconds);
                    Thread.Sleep(1000);*/
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message + "_" + ex.StackTrace);
                }
            }
    
        }
    }
}
