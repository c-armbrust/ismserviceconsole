﻿using IsmIoT.Commands;
using IsmIoTPortal.Models;
using Microsoft.Azure.Devices;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTHubServiceConsoleClient
{
    class Program
    {
        static ServiceClient serviceClient;
        #region connection string
        static string connectionString = "[connection string]";
        #endregion
        static string deviceId = "[device id]";



        private async static Task SendSimpleCloudToDeviceMessageAsync(string cmd)
        {
            var commandMessage = new Message(Encoding.ASCII.GetBytes("Cloud to device message."));
            commandMessage.Ack = DeliveryAcknowledgement.Full;
            commandMessage.MessageId = Guid.NewGuid().ToString();
            commandMessage.Properties[EventType.C2D_COMMAND] = cmd;
            Console.WriteLine("Send message with MessageId: {0}", commandMessage.MessageId);
            await serviceClient.SendAsync(deviceId, commandMessage);
        }



        private async static Task SendTestObjectCloudToDeviceMessageAsync(string cmd)
        {
            // DeviceSettings object
            DeviceSettings devicesettings = new DeviceSettings();
            devicesettings.DeviceId = "[device id]";
            devicesettings.StateName = "ReadyState";
            devicesettings.CapturePeriod = 10;
            devicesettings.CurrentCaptureUri = "https://uriofblob/blob.jpg";
            devicesettings.VarianceThreshold = 0.0025;
            devicesettings.DistanceMapThreshold = 8.5;
            devicesettings.RGThreshold = 3.75;
            devicesettings.RestrictedFillingThreshold = 4;
            devicesettings.DilateValue = 16;
            devicesettings.Brightness = 1;
            devicesettings.Exposure = 1;
            devicesettings.PulseWidth = 50;
            devicesettings.Current = 75;
            devicesettings.Predelay = 5;
            devicesettings.IsOn = true;

            string serializedDeviceState = JsonConvert.SerializeObject(devicesettings);
            var commandMessage = new Message(Encoding.ASCII.GetBytes(serializedDeviceState));
            commandMessage.Ack = DeliveryAcknowledgement.Full;
            commandMessage.MessageId = Guid.NewGuid().ToString();
            commandMessage.Properties[EventType.C2D_COMMAND] = CommandType.SET_DEVICE_SETTINGS;
            Console.WriteLine("Send message with MessageId: {0}", commandMessage.MessageId);
            await serviceClient.SendAsync(deviceId, commandMessage);
        }


        private async static void ReceiveFeedbackAsync()
        {
            var feedbackReceiver = serviceClient.GetFeedbackReceiver();

            Console.WriteLine("\nReceiving c2d feedback from service");
            while (true)
            {
                var feedbackBatch = await feedbackReceiver.ReceiveAsync();
                if (feedbackBatch == null) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Received feedback: {0}", string.Join(", ", feedbackBatch.Records.Select(f => f.StatusCode)));
                Console.WriteLine("Received feedback OriginalMessageId: {0}", string.Join(", ", feedbackBatch.Records.Select(f => f.OriginalMessageId)));
                Console.ResetColor();

                await feedbackReceiver.CompleteAsync(feedbackBatch);
            }
        }



        static void Start()
        {
            SendSimpleCloudToDeviceMessageAsync(CommandType.START).Wait();
        }

        static void Stop()
        {
            SendSimpleCloudToDeviceMessageAsync(CommandType.STOP).Wait();
        }

        static void StartPreview()
        {
            SendSimpleCloudToDeviceMessageAsync(CommandType.START_PREVIEW).Wait();
        }

        static void StopPreview()
        {
            SendSimpleCloudToDeviceMessageAsync(CommandType.STOP_PREVIEW).Wait();
        }

        static void SetDeviceSettings()
        {
            //SendSimpleCloudToDeviceMessageAsync(CommandType.SET_DEVICE_SETTINGS).Wait();
            SendTestObjectCloudToDeviceMessageAsync(CommandType.SET_DEVICE_SETTINGS).Wait();
        }

        static void GetDeviceSettings()
        {
            SendSimpleCloudToDeviceMessageAsync(CommandType.GET_DEVICE_SETTINGS).Wait();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Send Cloud-to-Device message\n");
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);

            // Let a feedback receiver work asynchronously in a while(1) loop
            ReceiveFeedbackAsync();


            string cmd = "";
            while(cmd != "q")
            {
                Console.WriteLine("Send a C2D Message:");
                Console.WriteLine("Start:               1");
                Console.WriteLine("Stop:                2");
                Console.WriteLine("StartPreview:        3");
                Console.WriteLine("StopPreview:         4");
                Console.WriteLine("SetDeviceSettings:   5");
                Console.WriteLine("GetDeviceSettings:   6");
                Console.WriteLine("Quit                 q");

                cmd = Console.ReadLine();

                switch(cmd)
                {
                    case "1":
                        Start();
                        break;
                    case "2":
                        Stop();
                        break;
                    case "3":
                        StartPreview();
                        break;
                    case "4":
                        StopPreview();
                        break;
                    case "5":
                        SetDeviceSettings();
                        break;
                    case "6":
                        GetDeviceSettings();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
