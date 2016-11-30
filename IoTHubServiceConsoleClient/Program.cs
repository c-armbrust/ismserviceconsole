// Switch the options to receive events with EventProcessorHost OR EventHubClient
#define EVENT_PROCESSOR_HOST

using IsmIoT.Commands;
using IsmIoTPortal.Models;
using Microsoft.Azure.Devices;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using ProcessD2CMessages;
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
        static string connectionString = "";
        static string iotHubD2cEndpoint = "messages/events";
        static string storageConnectionString = ""; // for eventprocessorhost leasing mechanism
        static EventHubClient eventHubClient; // Use EventHubClient for IoT Hubs too
        #endregion
        static string deviceId = "";

        private async static Task ReceiveMessagesFromDeviceAsync(string partition)
        {
            // This creates a receiver that will receive only message that are sent after it starts (DateTime.Now parameter)
            var eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.Now);
            while (true)
            {
                EventData eventData = await eventHubReceiver.ReceiveAsync();
                if (eventData == null) continue;

                // CommandType.UPDATE_DASHBOARD_CONTROLS
                //
                if (eventData.Properties.ContainsKey(EventType.D2C_COMMAND) && (string)eventData.Properties[EventType.D2C_COMMAND] == CommandType.UPDATE_DASHBOARD_CONTROLS)
                {
                    try
                    {
                        //var messageId = (string)eventData.SystemProperties["message-id"];
                        string serializedDeviceSettings = Encoding.UTF8.GetString(eventData.GetBytes());
                        DeviceSettings deviceSettings = JsonConvert.DeserializeObject<DeviceSettings>(serializedDeviceSettings);
                        Console.WriteLine(string.Format("\nReceived message, message-body=\n{0}", serializedDeviceSettings));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                // CommandType.DAT
                //...

                // CommandType.PRV
                // ...
            }
        }



        private async static Task SendSimpleCloudToDeviceMessageAsync(string cmd)
        {
            var commandMessage = new Message(Encoding.ASCII.GetBytes("Cloud to device message."));
            commandMessage.Ack = DeliveryAcknowledgement.Full;
            commandMessage.MessageId = Guid.NewGuid().ToString();
            commandMessage.Properties[EventType.C2D_COMMAND] = cmd;
            Console.WriteLine("Send message with MessageId: {0}", commandMessage.MessageId);
            await serviceClient.SendAsync(deviceId, commandMessage);
        }



        private async static Task SendTestObjectCloudToDeviceMessageAsync(string cmd, uint captureperiod, uint pulsewidth, uint predelay, uint gain, double exposure)
        {
            //// DeviceSettings object
            //DeviceSettings devicesettings = new DeviceSettings();
            //devicesettings.DeviceId = "[device id]";
            //devicesettings.StateName = "ReadyState";
            SimpleEventProcessor.deviceSettings.CapturePeriod = captureperiod;
            //devicesettings.CurrentCaptureUri = "https://uriofblob/blob.jpg";
            //devicesettings.VarianceThreshold = 0.0025;
            //devicesettings.DistanceMapThreshold = 8.5;
            //devicesettings.RGThreshold = 3.75;
            //devicesettings.RestrictedFillingThreshold = 4;
            //devicesettings.DilateValue = 16;
            SimpleEventProcessor.deviceSettings.Gain = gain;
            SimpleEventProcessor.deviceSettings.Exposure = exposure;
            SimpleEventProcessor.deviceSettings.PulseWidth = pulsewidth;
            //devicesettings.Current = 75;
            SimpleEventProcessor.deviceSettings.Predelay = predelay;

            string serializedDeviceState = JsonConvert.SerializeObject(SimpleEventProcessor.deviceSettings);
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
            uint captureperiod;
            uint pulsewidth;
            uint predelay;
            uint gain;
            double exposure;
            Console.WriteLine("Insert\n1. CapturePeriod in ms\n2. Pulsewidth in µs\n3. Predelay in µs\n4. Gain in %\n5. Exposure");
            captureperiod = Convert.ToUInt32(Console.ReadLine());
            pulsewidth = Convert.ToUInt32(Console.ReadLine());
            predelay = Convert.ToUInt32(Console.ReadLine());
            gain = Convert.ToUInt32(Console.ReadLine());
            exposure = Convert.ToDouble(Console.ReadLine());
            //SendSimpleCloudToDeviceMessageAsync(CommandType.SET_DEVICE_SETTINGS).Wait();
            SendTestObjectCloudToDeviceMessageAsync(CommandType.SET_DEVICE_SETTINGS, captureperiod, pulsewidth, predelay, gain, exposure).Wait();
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


#if EVENT_PROCESSOR_HOST
            string eventProcessorHostName = Guid.NewGuid().ToString();
            EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, iotHubD2cEndpoint, EventHubConsumerGroup.DefaultGroupName, connectionString, storageConnectionString, "msg-events");
            Console.WriteLine("Registering EventProcessor...");
            Task.Factory.StartNew(() => { 
            // EventProcessorHost
            //
            //string eventProcessorHostName = Guid.NewGuid().ToString();
            //EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, iotHubD2cEndpoint, EventHubConsumerGroup.DefaultGroupName, connectionString, storageConnectionString, "msg-events");
            Console.WriteLine("Registering EventProcessor...");
            eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>().Wait();

            //Console.WriteLine("Receiving. Press enter key to stop worker.");
            //Console.ReadLine();
            //eventProcessorHost.UnregisterEventProcessorAsync().Wait();
#else
                // EventHubClient
                //
                Console.WriteLine("Reading Events...");
                eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubD2cEndpoint);

                var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;

                foreach (string partition in d2cPartitions)
                {
                    ReceiveMessagesFromDeviceAsync(partition);
                }
                Console.ReadLine();
#endif
            });



            string cmd = "";
            while (cmd != "q")
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

                switch (cmd)
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
            // Shutdown eventprocessorhost and set checkpoint
            eventProcessorHost.UnregisterEventProcessorAsync().Wait();
        }
    }
}
