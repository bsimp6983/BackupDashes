using System;
using System.Collections.Generic;
using System.Web;

namespace DowntimeCollection_Demo
{
    public static class DateTimeHelper
    {

        /// <summary>
        /// 获取 time 所属的周的第一天
        /// </summary>
        /// <param name="time"></param>
        /// <param name="firstDayOfWeek"></param>
        /// <returns></returns>
        public static DateTime GetWeekStart(DateTime time, DayOfWeek firstDayOfWeek)
        {

            // 这里采用的是遍历的方法，从当前时间向前走，直到找到符合条件的日期
            while (true)
            {
                if (time.DayOfWeek == firstDayOfWeek)
                {
                    DateTime result = new DateTime(time.Year, time.Month, time.Day);
                    return result;
                }
                time = time.AddDays(-1.0);
            }
        }


        /// <summary>
        /// 为指定的 startDate 和 endDate 计算包含哪些星期，用每个星期的第一天标识。返回的结果中，第一个值小于等于 startDate
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static List<DateTime> SplitWeeks(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                throw new ArgumentException();
            }


            List<DateTime> result = new List<DateTime>();

            // 第一个星期
            DateTime firstWeekStart = GetWeekStart(startDate, DayOfWeek.Monday);

            //  最后一个星期的下一个星期
            DateTime outOfLastWeekStart = GetWeekStart(endDate, DayOfWeek.Monday);
            if (outOfLastWeekStart < endDate)
            {
                outOfLastWeekStart = outOfLastWeekStart.AddDays(7.0);
            }

            //  循环变量
            DateTime time = firstWeekStart;

            while (time < outOfLastWeekStart)
            {
                result.Add(time);

                //  向下走一个星期
                time = time.AddDays(7.0);

            }

            return result;
        }




        /// <summary>
        /// 获取 time 所属的月的第一天
        /// </summary>
        /// <param name="time"></param>
        /// <param name="firstDayOfWeek"></param>
        /// <returns></returns>
        public static DateTime GetMonthStart(DateTime time)
        {
            DateTime result = new DateTime(time.Year, time.Month, 1);
            return result;

        }

        /// <summary>
        /// 参见 SplitWeeks
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static List<DateTime> SplitMonths(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                throw new ArgumentException();
            }


            List<DateTime> result = new List<DateTime>();

            // 第一个月
            DateTime firstMonthStart = GetMonthStart(startDate);

            //  最后一个星期的下一个月
            DateTime outOfLastMonthStart = GetMonthStart(endDate);
            if (outOfLastMonthStart < endDate)
	        {
                outOfLastMonthStart = outOfLastMonthStart.AddMonths(1);
	        }

            //  循环变量
            DateTime time = firstMonthStart;

            while (time < outOfLastMonthStart)
            {
                result.Add(time);

                //  向下走一个月
                time = time.AddMonths(1);

            }

            return result;
        }

        public static List<int> SplitYears(DateTime startDate, DateTime endDate)
        {
            
            List<int> r=new List<int>();
            for(var i=startDate.Year;i<=endDate.Year;i++)
	        {
        		 r.Add(i);
	        }
            return r;                        
        }
    }
}
