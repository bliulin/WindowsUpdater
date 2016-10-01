using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Cegeka.Updater.Logic.Configuration.Model;
using log4net;

namespace Cegeka.Updater.Logic.Utils
{
    public class TimeFrameDecision : ITimeFrameDecision
    {        
        public bool IsWithinTimeFrame(TimeFrame timeFrame)
        {
            if (timeFrame == null)
            {
                throw new ArgumentNullException("timeFrame");
            }

            if (timeFrame.DayOfWeek < 0 || timeFrame.DayOfWeek > 6)
            {
                throw new ConfigurationErrorsException(string.Format("Day of week must be between 0 (Sunday) and 6 (Saturday). Current value = {0}.", 
                    timeFrame.DayOfWeek));
            }            

            var weekDay = (DayOfWeek)(timeFrame.DayOfWeek % 7);
            DateTime utcNow = DateTime.UtcNow;
            var firstOfMonth = new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            int offset = weekDay - firstOfMonth.DayOfWeek;
            if (offset < 0)
            {
                offset += 7;
            }
            DateTime maintenanceTimeFrame = firstOfMonth.AddDays(offset + 7 * (timeFrame.WeekCount - 1));
            if (utcNow.Date != maintenanceTimeFrame.Date)
            {
                return false;
            }

            DateTime startDate;
            if (DateTime.TryParse(timeFrame.StartTime, new CultureInfo("nl-NL"), DateTimeStyles.AdjustToUniversal,
                    out startDate) == false)
            {
                throw new ConfigurationErrorsException(string.Format("Maintenance start date '{0}' for group is not in the correct format.",
                    timeFrame.StartTime));
            }

            DateTime endDate;
            if (DateTime.TryParse(timeFrame.EndTime, new CultureInfo("nl-NL"), DateTimeStyles.AdjustToUniversal,
                    out endDate) == false)
            {
                throw new ConfigurationErrorsException(string.Format("Maintenance end date '{0}' is not in the correct format.", timeFrame.EndTime));                
            }

            DateTime startDateTime = maintenanceTimeFrame.AddHours(startDate.Hour).AddMinutes(startDate.Minute);
            DateTime endDateTime = maintenanceTimeFrame.AddHours(endDate.Hour).AddMinutes(endDate.Minute);

            bool isWithinTimeFrame = utcNow.CompareTo(startDateTime) >= 0 && utcNow.CompareTo(endDateTime) <= 0;
            return isWithinTimeFrame;
        }
    }
}
