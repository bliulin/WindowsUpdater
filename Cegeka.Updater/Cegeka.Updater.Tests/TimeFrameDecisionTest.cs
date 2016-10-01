using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cegeka.Updater.Logic.Configuration.Model;
using Cegeka.Updater.Logic.Utils;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Cegeka.Updater.Tests
{
    [TestClass]
    public class TimeFrameDecisionTest
    {
        [TestMethod]
        public void When_DayOfWeekIsDifferent_Then_ReturnFalse()
        {
            var sut = createSut();

            var now = DateTime.UtcNow;
            var test = now.AddDays(1);

            var tf = new TimeFrame
            {
                DayOfWeek = (int) test.DayOfWeek,
                EndTime = test.AddHours(6).ToLongDateString(),
                GroupName = "",
                StartTime = test.ToShortTimeString(),
                WeekCount = getWeekCount(test)
            };

            Assert.IsFalse(sut.IsWithinTimeFrame(tf));
        }

        [TestMethod]
        public void When_DayOfWeekIsEqual_And_HourIsOutOfTimeFrame_Then_ReturnFalse()
        {
            var sut = createSut();

            var now = DateTime.UtcNow;
            var test = now.AddHours(1);

            var tf = new TimeFrame
            {
                DayOfWeek = (int)test.DayOfWeek,
                EndTime = test.AddHours(6).ToShortTimeString(),
                GroupName = "",
                StartTime = test.ToShortTimeString(),
                WeekCount = getWeekCount(test)
            };

            Assert.IsFalse(sut.IsWithinTimeFrame(tf));
        }

        [TestMethod]
        public void When_DayOfWeekIsEqual_And_HourIsWithinTimeFrame_Then_ReturnTrue()
        {
            var sut = createSut();

            var now = DateTime.UtcNow;

            var tf = new TimeFrame
            {
                DayOfWeek = (int)now.DayOfWeek,
                EndTime = now.AddHours(6).ToShortTimeString(),
                GroupName = "",
                StartTime = now.Subtract(new TimeSpan(0, 1, 0, 0)).ToShortTimeString(),
                WeekCount = getWeekCount(now)
            };

            Assert.IsTrue(sut.IsWithinTimeFrame(tf));
        }

        [TestMethod]
        public void When_TimeFrameIsInTheLastDayOfTheMonth()
        {
            var sut = createSut();

            var now = DateTime.UtcNow;
            var test = now;
            bool sameDay = true;

            while (test.AddDays(1).Month == now.Month)
            {
                test = test.AddDays(1);
                sameDay = false;
            }

            var tf = new TimeFrame
            {
                DayOfWeek = (int)test.DayOfWeek,
                EndTime = test.AddHours(6).ToShortTimeString(),
                StartTime = test.ToShortTimeString(),
                GroupName = "",                
                WeekCount = getWeekCount(test)
            };

            if (sameDay)
            {
                Assert.IsTrue(sut.IsWithinTimeFrame(tf));
            }
            else
            {
                Assert.IsFalse(sut.IsWithinTimeFrame(tf));
            }
        }        

        private TimeFrameDecision createSut()
        {
            var sut = new TimeFrameDecision();
            return sut;
        }

        private int getWeekCount(DateTime dt)
        {
            int nr = 0;
            int dw = (int) dt.DayOfWeek;
            int curMonth = dt.Month;
            DateTime cursor = dt;

            do
            {
                cursor = cursor.Subtract(new TimeSpan(7, 0, 0, 0));
                nr++;
            }
            while (curMonth == cursor.Month);
            
            cursor = cursor.AddDays(7 - dw);
            nr = cursor.Month == dt.Month ? nr + 1 : nr;
            return nr;
        }        
    }
}
