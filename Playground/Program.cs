using System;
using Lucent.Core;   // gives access to ReportPeriodGenerator

var fyStart = new DateTime(2024, 4, 1);   // ← change if your FY starts elsewhere
DateTime? horizon = null;                 // null ⇒ run to FY-end

Console.WriteLine("Month-end (as-at) dates:");
foreach (var d in ReportPeriodGenerator.MonthEnds(fyStart, horizon))
    Console.WriteLine($"  {d:dd MMM yyyy}");

Console.WriteLine("\nP&L periods:");
foreach (var (from, to) in ReportPeriodGenerator.PLMonths(fyStart, horizon))
    Console.WriteLine($"  {from:dd MMM yyyy}  →  {to:dd MMM yyyy}");

// keep the window open when launched via F5
Console.WriteLine("\nPress any key to exit…");
Console.ReadKey();
