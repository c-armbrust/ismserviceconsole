using IsmIoT.Commands;
using IsmIoTPortal.Models;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessD2CMessages
{
    class SimpleEventProcessor : IEventProcessor
    {
        private long currentBlockInitOffset;

        public static DeviceSettings deviceSettings;

        public SimpleEventProcessor()
        {

        }

        Task IEventProcessor.CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine("Processor Shutting Down. Partition '{0}', Reason: '{1}'.", context.Lease.PartitionId, reason);

            return context.CheckpointAsync(); // Checkpoint on shutdown
            //return Task.FromResult<object>(null);
        }

        Task IEventProcessor.OpenAsync(PartitionContext context)
        {
            Console.WriteLine("StoreEventProcessor initialized.  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset);

            if (!long.TryParse(context.Lease.Offset, out currentBlockInitOffset))
            {
                currentBlockInitOffset = 0;
            }

            return Task.FromResult<object>(null);
        }

        async Task IEventProcessor.ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (EventData eventData in messages)
            {
                byte[] data = eventData.GetBytes();

                // CommandType.UPDATE_DASHBOARD_CONTROLS
                //
                if (eventData.Properties.ContainsKey(EventType.D2C_COMMAND) && (string)eventData.Properties[EventType.D2C_COMMAND] == CommandType.UPDATE_DASHBOARD_CONTROLS)
                {
                    try
                    {
                        //var messageId = (string)eventData.SystemProperties["message-id"];
                        string serializedDeviceSettings = Encoding.UTF8.GetString(data);
                        deviceSettings = JsonConvert.DeserializeObject<DeviceSettings>(serializedDeviceSettings);

                        WriteHighlightedMessage(string.Format("\nUpdate Dashboard controls, message-body=\n{0}", serializedDeviceSettings));
                        continue;
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        continue;
                    }
                }

                // CommandType.CAPTURE_UPLOADED
                if (eventData.Properties.ContainsKey(EventType.D2C_COMMAND) && (string)eventData.Properties[EventType.D2C_COMMAND] == CommandType.CAPTURE_UPLOADED)
                {
                    try
                    { 
                        string serializedDeviceSettings = Encoding.UTF8.GetString(data);
                        deviceSettings = JsonConvert.DeserializeObject<DeviceSettings>(serializedDeviceSettings);
                        if(deviceSettings.StateName == "RunState")
                        {
                            Console.WriteLine("Process uploaded capture: {0}", deviceSettings.CurrentCaptureUri);
                            Console.WriteLine("Send the Data to DashboardBroker AND in Event Hub to process them further");
                        }
                        if(deviceSettings.StateName == "PreviewState")
                        {
                            Console.WriteLine("Process uploaded capture: {0}", deviceSettings.CurrentCaptureUri);
                            Console.WriteLine("JUST Send the Data to DashboardBroker");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        continue;
                    }
                }

            }
        }

        private void WriteHighlightedMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
