using System;
using UnityEngine;

public static class DateTimeExtension
{
    private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0);

    public static long ToUnixTime(this DateTime dateTime)
    {
        TimeSpan t = dateTime - Epoch;
        return (long) t.TotalSeconds;
    }

    public static DateTime FromUnixTime(long unixTime)
    {
        return Epoch.AddSeconds(unixTime);
    }

    public static DateTime FromUnixTime(string unixTime, DateTime defaultTime)
    {
        try
        {
            long t;
            if (long.TryParse(unixTime, out t))
                return FromUnixTime(t);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        return defaultTime;
    }

    public static void RunReadWriteTest()
    {
        string t = PlayerPrefs.GetString("UNIX_TIME", "0");
        Debug.Log("LAST UNIX TIME : " + t);
        DateTime time = FromUnixTime(t, DateTime.MinValue);

        Debug.Log("LAST DATETIME : " + time);

        time = DateTime.Now;
        t = time.ToUnixTime().ToString();
        Debug.Log("Current UNIX TIME : " + t);

        PlayerPrefs.SetString("UNIX_TIME", t);
        PlayerPrefs.Save();
    }
}
