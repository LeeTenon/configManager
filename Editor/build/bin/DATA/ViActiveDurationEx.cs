using System;

using ViTime64 = System.Int64;
public static class TimeAssisstant
{
    public static Int32 TimeZone = 0;//服务器传过来的时区
    public static Int64 VALUE_TimeFlag = 100000000000000;

    public static DateTime epochUnspecified = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
    public static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static long GetTime()
    {
        DateTime nowTime = new DateTime();

        return Convert.ToInt64((nowTime.ToUniversalTime() - epoch).TotalMilliseconds);
    }

    /// <summary>
    /// Time1970
    /// </summary>
    public static long GetTimeSeconds(DateTime time)
    {
        return Convert.ToInt64((time - epoch).TotalSeconds);
    }

    public static DateTime GetDateTime(Int64 time)
    {
        return epoch.AddMilliseconds(time * 10);
    }
    public static DateTime GetUnspecifiedDateTime(Int64 time)
    {
        return epochUnspecified.AddMilliseconds(time * 10);
    }

    public static DateTime GetGreenwichTime(Int64 time)
    {
        return epoch.AddMilliseconds(time * 10 - TimeZone * 3600000);
    }

    public static DateTimeOffset GetNewDateTimeOffset(long time)
    {
        return new DateTimeOffset(GetUnspecifiedDateTime(time), new TimeSpan(TimeZone, 0, 0));
    }
}

public struct ViDurationDateStruct
{
    public static readonly ViTime64 SECOND = 100;
    public static readonly ViTime64 MINUTE = SECOND * 60;
    public static readonly ViTime64 HOUR = MINUTE * 60;
    public static readonly ViTime64 DAY = HOUR * 24;
    public static readonly ViTime64 WEEK = DAY * 7;

    /// <summary>
    /// seconds
    /// </summary>
    public ViTime64 Value
	{
		get
		{
			ResetZone();
			return _Value;// + TimeAssisstant.TimeZone * 3600
		} 
	}

	public bool IsEmpty()
	{
		return (SetYear == 0) &&
				(SetMonth == 0) &&
				(SetDay == 0) &&
				(SetHour == 0) &&
				(Minute == 0) &&
				(Second == 0);
	}
	public bool IsPre(ViTime64 time)//time is Pre this, return true;
	{
		time = time / 100;
		return time < Value;
	}
	public bool IsAfter(ViTime64 time)//time is after this, return true;
	{
		time = time / 100;
		return time > Value;
	}


	public void Format()
	{
		if(SetYear == 0)
		{
			return;
		}
		if(CheckDataOK(SetYear, SetMonth, SetDay) == false)
		{
			ViDebuger.Error("ViDurationDateStruct::CheckDataOK:", SetYear, SetMonth, SetDay, SetHour, Minute, Second);
			return;
		}
		DateTime dt = new DateTime(SetYear, SetMonth, SetDay, SetHour, Minute, Second);
		_Value = TimeAssisstant.GetTimeSeconds(dt);
	}

	static public bool CheckDataOK(Int32 year, Int32 month, Int32 day)
	{
		if(year < 1)
		{
			return false;
		}
		if (year > 9999)
		{
			return false;
		}
		if(day <= 0)
		{
			return false;
		}
		if (month == 1 || month == 3 || month == 5 || month == 7 || month == 8 || month == 10 || month == 12)
		{
			return day <= 31;
		}
		else if (month == 4 || month == 6 || month == 9 || month == 11)
		{
			return day <= 30;
		}
		else if (month == 2)
		{
			if (year % 400 == 0 || year % 4 == 0 && year % 100 != 0)
			{
				return day <= 29;
			}
			else
			{
				return day <= 28;
			}
		}
		return false;
	}

	public Int32 SetYear;//不要直接取变量,用下面的函数Year
    public Int32 SetMonth;//不要直接取变量,用下面的函数Month
    public Int32 SetDay;//不要直接取变量,用下面的函数Day
    public Int32 SetHour;//不要直接取变量,用下面的函数Hour
    public Int32 Minute;
	public Int32 Second;
	public Int64 _Value;
	public ViEnum32<BoolValue> UseZone0;//TRUE 表示 使用本地时间，//false全球统一时间， 会做修改
	public Int32 _zone;
	//

	public void ResetZone()
	{
		if (UseZone0.Value == (Int32)BoolValue.FALSE)
		{
			return;
		}
		if (_zone == TimeAssisstant.TimeZone)
		{
			return;
		}
		_Value += (TimeAssisstant.TimeZone - _zone) * 3600;
		_zone = TimeAssisstant.TimeZone;
		DateTime newDateTime = TimeAssisstant.epoch.AddTicks(_Value * 10000000);
		SetYear = newDateTime.Year;
		SetMonth = newDateTime.Month;
		SetDay = newDateTime.Day;
		SetHour = newDateTime.Hour;
	}

	public Int32 Year
	{
		get
		{
			ResetZone();
			return SetYear;
		}
	}
	public Int32 Month
	{
		get
		{
			ResetZone();
			return SetMonth;
		}
	}
	public Int32 Day
	{
		get
		{
			ResetZone();
			return SetDay;
		}
	}
	public Int32 Hour
	{
		get
		{
			ResetZone();
			return SetHour;
		}
	}
}

public struct ViDurationStruct
{
	public void Format()
	{
		_Value = ViDurationDateStruct.DAY * Day + ViDurationDateStruct.HOUR * Hour + ViDurationDateStruct.MINUTE * Minute + ViDurationDateStruct.SECOND * Second;
	}

	public ViTime64 Value
	{
		get
		{
			return _Value;

		}
	}

	public Int32 Day;
	public Int32 Hour;
	public Int32 Minute;
	public Int32 Second;
	public Int64 _Value;
}


public struct ViDurationTimeStruct
{
	public ViTime64 Value
	{
		get
		{
			return _Value;

		}
	}

	public void Format()
	{
		_Value = ViDurationDateStruct.HOUR * Hour + ViDurationDateStruct.MINUTE * Minute + ViDurationDateStruct.SECOND * Second;
	}

	public Int32 Hour;
	public Int32 Minute;
	public Int32 Second;
	public Int64 _Value;
}

public struct ViActiveDurationDate
{
	public bool IsEmpty()
	{
		return StartTime.IsEmpty() && EndTime.IsEmpty();
	}

	public void Format()
	{
		StartTime.Format();
		EndTime.Format();
	}

	public bool IsActive(ViTime64 time)
	{
		time = time / 100;
		return time > StartTime.Value && time < EndTime.Value;
	}

	public ViDurationDateStruct StartTime;
	public ViDurationDateStruct EndTime;
}

public struct ViActiveDurationDay
{
	public bool IsEmpty()
	{
		return StartDay == 0 && EndDay == 0;
	}

	public bool IsActive(ViTime64 time)
	{
		if (IsEmpty())
		{
			return false;
		}
		DateTime t = new DateTime(time*10);
		int currentDay = (t.DayOfWeek == 0) ? 7 : (int)(t.DayOfWeek);
		
		if (StartDay <= EndDay)
		{
			return currentDay >= StartDay && currentDay <= EndDay;
		}
		else
		{
			return currentDay >= StartDay || currentDay <= EndDay;
		}
	}

	public Int32 StartDay;
	public Int32 EndDay;
}