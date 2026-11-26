using System;
using System.Collections.Generic;
using System.Globalization;

namespace NSMSOLUTION.Services
{
    public enum ReportPeriod
    {
        Daily,
        Weekly,
        Monthly
    }

    public class ReportDateRange
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string Title { get; set; }
        public string Label { get; set; }
    }

    /// <summary>
    /// Shared calendar / schedule rules for Daily, Weekly and Monthly reports.
    /// Used by any report controller (AR, Sale, ...). No DB or HTTP dependencies.
    /// </summary>
    public static class ReportScheduleService
    {
        // Sunday is a non-working day: no reports at all.
        public static bool IsWorkingDay(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Sunday;
        }

        // Which periods must be generated on this date?
        //   Daily   : Monday-Saturday
        //   Weekly  : every Saturday, or on the monthly report date
        //   Monthly : actual month-end (Sunday month-end -> preceding Saturday)
        public static List<ReportPeriod> GetScheduledPeriods(DateTime date)
        {
            var periods = new List<ReportPeriod>();
            date = date.Date;

            if (!IsWorkingDay(date))
                return periods;

            periods.Add(ReportPeriod.Daily);

            bool isWeekly = date.DayOfWeek == DayOfWeek.Saturday;
            bool isMonthly = date == GetMonthlyReportDate(date);

            if (isWeekly || isMonthly)
                periods.Add(ReportPeriod.Weekly);

            if (isMonthly)
                periods.Add(ReportPeriod.Monthly);

            return periods;
        }

        public static DateTime GetMonthEnd(DateTime date)
        {
            return new DateTime(
                date.Year,
                date.Month,
                DateTime.DaysInMonth(date.Year, date.Month));
        }

        // Month-end Mon-Sat -> month-end. Month-end Sunday -> Saturday before.
        public static DateTime GetMonthlyReportDate(DateTime date)
        {
            DateTime monthEnd = GetMonthEnd(date);

            return monthEnd.DayOfWeek == DayOfWeek.Sunday
                ? monthEnd.AddDays(-1)
                : monthEnd;
        }

        public static DateTime GetMondayOfWeek(DateTime date)
        {
            int daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
            return date.AddDays(-daysSinceMonday);
        }

        public static ReportDateRange GetRange(ReportPeriod period, DateTime referenceDate)
        {
            DateTime date = referenceDate.Date;
            CultureInfo ci = CultureInfo.InvariantCulture;

            DateTime monthStart = new DateTime(date.Year, date.Month, 1);
            DateTime monthEnd = GetMonthEnd(date);

            switch (period)
            {
                case ReportPeriod.Weekly:
                    {
                        // Monday-Saturday, clamped to month boundaries.
                        DateTime monday = GetMondayOfWeek(date);
                        DateTime saturday = monday.AddDays(5);

                        DateTime from = monday < monthStart ? monthStart : monday;
                        DateTime to = saturday > monthEnd ? monthEnd : saturday;

                        int weekNo = ((date.Day - 1) / 7) + 1;

                        return new ReportDateRange
                        {
                            From = from,
                            To = to,
                            Title = "Weekly",
                            Label = "Week " + weekNo +
                                    " of " + from.ToString("MMMM yyyy", ci) +
                                    " (" + from.ToString("dd-MM-yyyy", ci) +
                                    " to " + to.ToString("dd-MM-yyyy", ci) + ")"
                        };
                    }

                case ReportPeriod.Monthly:
                    {
                        return new ReportDateRange
                        {
                            From = monthStart,
                            To = monthEnd,
                            Title = "Monthly",
                            Label = "Month: " + monthStart.ToString("MMMM yyyy", ci) +
                                    " (" + monthStart.ToString("dd-MM-yyyy", ci) +
                                    " to " + monthEnd.ToString("dd-MM-yyyy", ci) + ")"
                        };
                    }

                case ReportPeriod.Daily:
                default:
                    {
                        return new ReportDateRange
                        {
                            From = date,
                            To = date,
                            Title = "Daily",
                            Label = "Date: " + date.ToString("dd-MM-yyyy", ci)
                        };
                    }
            }
        }
    }
}