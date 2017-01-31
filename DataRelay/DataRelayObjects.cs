﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace DataRelay
{
    [DataContract]
    public class Account
    {
        [DataMember]
        public int? birthyear { get; set; }
        [DataMember]
        public int? weight { get; set; }
        [DataMember]
        public int? height { get; set; }
        [DataMember]
        public string sex { get; set; }
    }
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
        public TotalStat(string type, string totalDuration, float totalDistance, int totalCalories)
        {
            this.type = type;
            total_duration = totalDuration;
            total_distance = totalDistance;
            total_calories = totalCalories;
        }

        [DataMember]
        public string type { get; set; }
        [DataMember]
        public string total_duration { get; set; }
        [DataMember]
        public float total_distance { get; set; }
        [DataMember]
        public int total_calories { get; set; }
    }
    [DataContract]
    public class AllStat
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
    [DataContract]
    public class Path
    {
        [DataMember]
        public string path { get; set; }
    }
    [DataContract]
    public class Ticket
    {
        [DataMember]
        public string title { get; set; }
        [DataMember]
        public string description { get; set; }
        [DataMember]
        public string gps { get; set; }
        [DataMember]
        public string imageLink { get; set; }
        [DataMember]
        public string date { get; set; }
        [DataMember]
        public string ticketType { get; set; }
        [DataMember]
        public string color { get; set; }
        [DataMember]
        public int active { get; set; }
    }
}