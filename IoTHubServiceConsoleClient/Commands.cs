using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IsmIoT.Commands
{
    // EventType represents the KEYS for event properties
    //
    public struct EventType
    {
        public static string C2D_COMMAND = "Y_C2D_Command";
        public static string D2C_COMMAND = "Y_D2C_Command";

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
        public static string UNPROVISION = "Y_Unprovision";
        public static string PROVISION = "Y_Provision";

        // C2D Commands
        public static string START = "Y_Start";
        public static string STOP = "Y_Stop";
        public static string START_PREVIEW = "Y_StartPreview";
        public static string STOP_PREVIEW = "Y_StopPreview";

        // C2D Dashboard Commands
        public static string GET_DEVICE_SETTINGS = "Y_GetDeviceSettings";
        public static string SET_DEVICE_SETTINGS = "Y_SetDeviceSettings";

        // D2C Commands
        /* Obsolete. Use CAPTURE_UPLOADED and look at the StateName field
         * RunState -> DAT
         * PreviewState -> PRV
         * 
        public static string DAT = "D_DAT";
        public static string PRV = "D_PRV";
        */
        public static string CAPTURE_UPLOADED = "Y_CaptureUploaded";

        // D2C Dashboard Commands 
        public static string UPDATE_DASHBOARD_CONTROLS = "Y_UpdateDashboardControls";
    };

    // More possible event types:
    // struct TelemetryType ...
    // struct InquiryType ...
    // struct NotificationType ...
    


    struct CommandStatus
    {
        public static string SUCCESS = "Y_Success";
        public static string FAILURE = "Y_Failure";
        public static string PENDING = "Y_Pending";
    };


}
