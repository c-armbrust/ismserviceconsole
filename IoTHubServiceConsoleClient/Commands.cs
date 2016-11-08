using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTHubServiceConsoleClient
{
    public struct CommandType
    {
    // Device identity registry Commands
        public static string UNPROVISION = "Unprovision";
        public static string PROVISION = "Provision";

        // C2D Commands
        public static string START = "Start";
        public static string STOP = "Stop";
        public static string START_PREVIEW = "StartPreview";
        public static string STOP_PREVIEW = "StopPreview";

        // C2D Dashboard Commands
        public static string GET_DEVICE_SETTINGS = "GetDeviceSettings";
        public static string SET_DEVICE_SETTINGS = "SetDeviceSettings";

        // D2C Commands
        public static string DAT = "DAT";
        public static string PRV = "PRV";

        // D2C Dashboard Commands 
        public static string UPDATE_DASHBOARD_CONTROLS = "UpdateDashboardControls";
    };


    struct CommandStatus
    {
        public static string SUCCESS = "Success";
        public static string FAILURE = "Failure";
        public static string PENDING = "Pending";
    };


}
