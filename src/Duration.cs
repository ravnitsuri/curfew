using System;

static class Duration
{
    public static string Compact(int minutes)
    {
        if (minutes < 60) return minutes + "m";
        if (minutes % 60 == 0) return (minutes / 60) + "h";
        return (minutes / 60) + "h" + (minutes % 60);
    }

    public static string Clock(TimeSpan left)
    {
        if (left.TotalHours >= 1)
            return string.Format("{0}:{1:00}:{2:00}", (int)left.TotalHours, left.Minutes, left.Seconds);
        return string.Format("{0}:{1:00}", left.Minutes, left.Seconds);
    }

    public static string Spoken(TimeSpan left)
    {
        if (left.TotalHours >= 1)
            return string.Format("{0} h {1} min", (int)left.TotalHours, left.Minutes);
        if (left.TotalMinutes >= 1)
            return string.Format("{0} min {1} s", left.Minutes, left.Seconds);
        return left.Seconds + " s";
    }

    public static void Split(int minutes, out string amount, out string unit)
    {
        if (minutes < 60)
        {
            amount = minutes.ToString();
            unit = minutes == 1 ? "minute" : "minutes";
        }
        else if (minutes % 60 == 0)
        {
            amount = (minutes / 60).ToString();
            unit = minutes == 60 ? "hour" : "hours";
        }
        else
        {
            amount = (minutes / 60) + "h " + (minutes % 60) + "m";
            unit = "";
        }
    }
}
