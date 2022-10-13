using System;

using ViTime64 = System.Int64;


public partial class DateTimeStruct : ViSealedData
{
    public class TestStructList
    {
        public int x;
        public int y;
        public string str1;
        public ViStaticArray<Int32> Ports = new ViStaticArray<Int32>(4);
    }


    public override void Format()
	{
		base.Format();
		Time.Format();
#if UNITY_EDITOR
		if (ID > 2000000 && ID < 9000000)
		{
			Check(10);//20 05 12 X
		}
		if (ID > 20000000 && ID < 90000000)
		{
			Check(100);//20 05 12 XX
		}
		if (ID > 200000000 && ID < 300000000)
		{
			Check(10);//2020 05 12 X
		}
		if (ID > 1200000000 && ID < 1300000000)
		{
			Check(10);// 1 2020 05 12 XX
		}
		if (ID > 2000000000 && ID < 2147483647)
		{
			Check(100);//2020 05 12 X
		}
#endif
	}

	public void Check(int per)
	{
		int iYear = ID / (per * 10000);
		Int32 iMonth = (ID - iYear * (per * 10000)) / (per * 100);
		Int32 iDay = (ID - iYear * (per * 10000) - iMonth * (per * 100)) / per;
		if (iYear % 100 != Time.SetYear % 100 || iMonth != Time.SetMonth || iDay != Time.SetDay)
		{
			ViDebuger.Error("DateTimeStruct::Format error?", ID);
		}
	}

	public ViDurationDateStruct Time;
    public int Empty_0;
    public ViStaticArray<TestStructList> list = new ViStaticArray<TestStructList>(3);

#if !DATA_EDITOR
	//public bool IsActive()
	//{
	//	if (ID == 0)
	//	{
	//		return true;
	//	}
	//	if (ServerTimer.Instance == null)
	//	{
	//		return true;
	//	}
	//	if (Time.IsEmpty())
	//	{
	//		return true;
	//	}
	//	if (Time.IsAfter(ServerTimer.Instance.Time1970))
	//	{
	//		return true;
	//	}
	//	return false;
	//}
#endif
}