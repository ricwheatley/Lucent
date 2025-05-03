// Lucent.Core / ReportPeriodGenerator.cs
using System;
using System.Collections.Generic;

namespace Lucent.Core;

/// <summary>
/// Generates month-end “as-at” dates and month-by-month
/// (from,to) ranges for P&L reporting.
/// </summary>
public static class ReportPeriodGenerator
{
    /// <summary>
    /// Every calendar month-end from <paramref name="fyStart"/>
    /// up to <paramref name="horizon"/> (inclusive).
    /// If horizon is null, stops at financial-year-end
    /// (fyStart + 1 year − 1 day).
    /// </summary>
    public static IEnumerable<DateTime> MonthEnds(DateTime fyStart,
                                                  DateTime? horizon = null)
    {
        var stop = horizon ?? fyStart.AddYears(1).AddDays(-1);
        var cursor = new DateTime(fyStart.Year, fyStart.Month,
                                  DateTime.DaysInMonth(fyStart.Year, fyStart.Month));

        while (cursor <= stop)
        {
            yield return cursor;
            cursor = cursor.AddMonths(1);
            cursor = new DateTime(cursor.Year, cursor.Month,
                                  DateTime.DaysInMonth(cursor.Year, cursor.Month));
        }
    }

    /// <summary>
    /// For each month covered by <see cref="MonthEnds"/>,
    /// yields (first-of-month, month-end) pairs
    /// suitable for Profit & Loss.
    /// </summary>
    public static IEnumerable<(DateTime from, DateTime to)>
        PLMonths(DateTime fyStart, DateTime? horizon = null)
    {
        foreach (var to in MonthEnds(fyStart, horizon))
            yield return (new DateTime(to.Year, to.Month, 1), to);
    }
}
