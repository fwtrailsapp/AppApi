﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace DataRelay
{
    [DataContract]
    public class Activity
    {
        [DataMember]
        public string time_started { get; set; }
        [DataMember]
        public string duration { get; set; }
        [DataMember]
        public float mileage { get; set; }
        [DataMember]
        public int calories_burned { get; set; }
        [DataMember]
        public string exercise_type { get; set; }
    }
    [DataContract]
    public class TotalStat
    {
        [DataMember]
        public string type { get; set; }
        [DataMember]
        public string total_duration { get; set; }
        [DataMember]
        public float total_distance { get; set; }
        [DataMember]
        public int total_calories { get; set; }
    }
}