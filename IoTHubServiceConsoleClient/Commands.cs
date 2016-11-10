using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTHubServiceConsoleClient
{
    // EventType represents the KEYS for event properties
    //
    public struct EventType
    {
        public static string COMMAND = "D_Command";

        // More possible event types:
        //public static string TELEMETRY ...
        //public static string INQUIRY ...
        //public static string NOTIFICATION ...
    }

    // CommandType represents the VALUES for event properties (for the KEY EventType::COMMAND)
    //
    public struct CommandType
    {
        // Device identity registry Commands
        public static string UNPROVISION = "D_Unprovision";
        public static string PROVISION = "D_Provision";

        // C2D Commands
        public static string START = "D_Start";
        public static string STOP = "D_Stop";
        public static string START_PREVIEW = "D_StartPreview";
        public static string STOP_PREVIEW = "D_StopPreview";

        // C2D Dashboard Commands
        public static string GET_DEVICE_SETTINGS = "D_GetDeviceSettings";
        public static string SET_DEVICE_SETTINGS = "D_SetDeviceSettings";

        // D2C Commands
        public static string DAT = "D_DAT";
        public static string PRV = "D_PRV";

        // D2C Dashboard Commands 
        public static string UPDATE_DASHBOARD_CONTROLS = "D_UpdateDashboardControls";
    };

    // More possible event types:
    // struct TelemetryType ...
    // struct InquiryType ...
    // struct NotificationType ...
    


    struct CommandStatus
    {
        public static string SUCCESS = "D_Success";
        public static string FAILURE = "D_Failure";
        public static string PENDING = "D_Pending";
    };


}
