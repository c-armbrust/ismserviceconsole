// Switch the options to receive events with EventProcessorHost OR EventHubClient
#define EVENT_PROCESSOR_HOST

using IsmIoT.Commands;
using IsmIoTPortal.Models;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ProcessD2CMessages
{
    class Program
    {

        static string connectionString = "[connection string]";
        static string iotHubD2cEndpoint = "messages/events"; 
        static string storageConnectionString = "[storage connection string]"; // for eventprocessorhost leasing mechanism
        static EventHubClient eventHubClient; // Use EventHubClient for IoT Hubs too

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

        static void Main(string[] args)
        {
#if EVENT_PROCESSOR_HOST
            // EventProcessorHost
            //
            string eventProcessorHostName = Guid.NewGuid().ToString();
            EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, iotHubD2cEndpoint, EventHubConsumerGroup.DefaultGroupName, connectionString, storageConnectionString, "messages-events");
            Console.WriteLine("Registering EventProcessor...");
            eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>().Wait();

            Console.WriteLine("Receiving. Press enter key to stop worker.");
            Console.ReadLine();
            eventProcessorHost.UnregisterEventProcessorAsync().Wait();
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
        }
    }
}
