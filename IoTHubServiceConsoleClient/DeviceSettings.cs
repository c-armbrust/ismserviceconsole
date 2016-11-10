using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace IsmIoTPortal.Models
{
    [DataContract]
    public class DeviceSettings
    {
        // 
        [DataMember]
        public string DeviceId { get; set; }
        [DataMember]
        public string StateName { get; set; }
        [DataMember]
        public int CapturePeriod { get; set; }
        [DataMember]
        public string CurrentCaptureUri { get; set; }

        // Matlab Filament-Algorithm Params
        [DataMember]
        public double VarianceThreshold { get; set; }
        [DataMember]
        public double DistanceMapThreshold { get; set; }
        [DataMember]
        public double RGThreshold { get; set; }
        [DataMember]
        public double RestrictedFillingThreshold { get; set; }
        [DataMember]
        public double DilateValue { get; set; }

        // Camera Settings
        [DataMember]
        public int Brightness { get; set; }
        [DataMember]
        public int Exposure { get; set; }

        // Pulser Settings
        [DataMember]
        public int PulseWidth { get; set; }
        [DataMember]
        public int Current { get; set; }
        [DataMember]
        public int Predelay { get; set; }
        [DataMember]
        public bool IsOn { get; set; }
    }
}