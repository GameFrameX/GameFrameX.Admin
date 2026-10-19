namespace GameFrameX.Core.Utility;

public static class DateTimeUtility
{
    /// <summary>
    /// 毫秒转天时分秒
    /// </summary>
    /// <param name="ms"></param>
    /// <returns></returns>
    public static string FormatTime(long ms)
    {
        var ss = 1000;
        var mi = ss * 60;
        var hh = mi * 60;
        var dd = hh * 24;

        var day = ms / dd;
        var hour = (ms - day * dd) / hh;
        var minute = (ms - day * dd - hour * hh) / mi;
        var second = (ms - day * dd - hour * hh - minute * mi) / ss;
        var milliSecond = ms - day * dd - hour * hh - minute * mi - second * ss;

        var sDay = day < 10 ? "0" + day : "" + day; //天
        var sHour = hour < 10 ? "0" + hour : "" + hour; //小时
        var sMinute = minute < 10 ? "0" + minute : "" + minute; //分钟
        var sSecond = second < 10 ? "0" + second : "" + second; //秒
        var sMilliSecond = milliSecond < 10 ? "0" + milliSecond : "" + milliSecond; //毫秒
        sMilliSecond = milliSecond < 100 ? "0" + sMilliSecond : "" + sMilliSecond;

        return string.Format("{0} 天 {1} 小时 {2} 分 {3} 秒", sDay, sHour, sMinute, sSecond);
    }
}
