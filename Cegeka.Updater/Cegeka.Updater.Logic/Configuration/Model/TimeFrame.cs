using System;
using System.Xml.Serialization;

namespace Cegeka.Updater.Logic.Configuration.Model
{
    [Serializable]
    public class TimeFrame
    {
        #region Properties

        [XmlAttribute(AttributeName = "groupName")]
        public string GroupName { get; set; }

        [XmlAttribute(AttributeName = "startTime")]
        public string StartTime { get; set; }

        [XmlAttribute(AttributeName = "endTime")]
        public string EndTime { get; set; }

        [XmlAttribute(AttributeName = "dayOfWeek")]
        public int DayOfWeek { get; set; }

        [XmlAttribute(AttributeName = "weekCount")]
        public int WeekCount { get; set; }

        #endregion
    }
}