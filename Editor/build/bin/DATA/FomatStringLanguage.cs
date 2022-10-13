using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;


[System.Serializable]
public class FomatLanguage
{
	public string EnglishText;
	public string ChineseText;

	public FomatLanguage(string englishText, string chineseText)
	{
		EnglishText = englishText;
		ChineseText = chineseText;
	}
}

public class GameStringFunc
{
    public static FomatStringLanguage FOMAT_Empty = new FomatStringLanguage(new string[] { "", "" });
    public static FomatStringLanguage GetGameString(string str)
	{
        FomatStringLanguage kFomatStringLanguage;
		if(GameStringStruct.GameStringMap.TryGetValue(str, out kFomatStringLanguage))
		{
			return kFomatStringLanguage;
		}
		ViDebuger.Error("GameStringFunc::GetGameString find null:", str);
		return FOMAT_Empty;
	}
}

[System.Serializable]
public class FomatLanguageList
{
	public List<FomatLanguage> AllFomatString = new List<FomatLanguage>();

	public void Add(string englishText, string chineseText)
	{
		FomatLanguage item = new FomatLanguage(englishText, chineseText);
		AllFomatString.Add(item);
	}
}

public class ByteListData
{
	public ByteListData(byte[] kData)
	{
		Data = kData;
	}

	public byte[] Data;
}


public class FomatStringLanguage
{
    static public int m_iLanguage = (int)Language;
    static public LanguageType Language = LanguageType.English;

    //static StringBuilder STATIC_result = new StringBuilder(500);
    public static string StringToBinary(string s)
	{
#if !DATA_EDITOR
		//byte[] data = Encoding.Unicode.GetBytes(s);
		//STATIC_result.Clear();
		//int length = data.Length;
		//for (Int32 idx = 0; idx < length; ++idx)
		//      {
		//	STATIC_result.Append(Convert.ToString(data[idx], 2).PadLeft(8, '0'));
		//      }
		//      return STATIC_result.ToString();
		//
		//return JsonUtility.ToJson(new ByteListData(data)).Replace("\"", "\\\"");
		return s;
#else
		return "";
#endif
	}

	//public static FomatStringLanguage FOMAT_Empty1 = new FomatStringLanguage(new string[] { "asd", "asd", "wdqd" });
	public static void ToJson()
	{
#if UNITY_EDITOR
		//string json = JsonUtility.ToJson(_allFomatString, true);
		//File.WriteAllText("../Tools/LanguageText/FomatLanguageList.txt", json);
		//
		//
		//string pathString = "../Client/Assets/Scripts/Client/Define/GameString.cs";
		ViStringBuilder.Clear();
		//StreamWriter sw = new StreamWriter(pathString, false, System.Text.Encoding.Default);
		//if (sw != null)
		{
			if(ViSealedDB<FomatGameStringStruct>.IsLoaded == false)
			{
				UnityEditor.EditorUtility.DisplayDialog("", "请运行游戏，加载好后再导出", "朕知道了");
				return;
			}
			ViStringBuilder.WriteLine();
			ViStringBuilder.Write("using System;");
			ViStringBuilder.WriteLine();
			ViStringBuilder.Write("using System.Collections.Generic;");
			ViStringBuilder.WriteLine();
			ViStringBuilder.Write("public static class GameString");
			ViStringBuilder.WriteLine();
			ViStringBuilder.Write("{");
			ViStringBuilder.WriteLine();
			ViStringBuilder.Write("	public static FomatStringLanguage FOMAT_Empty = new FomatStringLanguage(new string[] { \"\", \"\" });");
			ViStringBuilder.WriteLine();
			Int32 count = ViSealedDB<FomatGameStringStruct>.Array.Count;
			for (Int32 idx = 0; idx < count; ++idx)
			{
				FomatGameStringStruct iterFomatGameStringStruct = ViSealedDB<FomatGameStringStruct>.Array[idx] as FomatGameStringStruct;
				if (string.IsNullOrEmpty(iterFomatGameStringStruct.Name))
				{
					continue;
				}
				bool bIOS_Only = iterFomatGameStringStruct.Name.ToUpperInvariant().StartsWith("FOMAT_IOS");
				if (bIOS_Only)
				{
					ViStringBuilder.Write("#if UNITY_IOS");
					ViStringBuilder.WriteLine();
				}
				AllLanguageStruct kAllLanguageStruct = iterFomatGameStringStruct.Message.Language.Data;
				ViStringBuilder.Write("	public static FomatStringLanguage ");
				ViStringBuilder.Write(iterFomatGameStringStruct.Name);
				ViStringBuilder.Write(" = new FomatStringLanguage(new string[] { \"");
				ViStringBuilder.Write(ChangeString(kAllLanguageStruct.English));//0
				ViStringBuilder.Write("\", \"");
				ViStringBuilder.Write(ChangeString(iterFomatGameStringStruct.Message.ChineseText));//1
				//
				ViStringBuilder.Write("\", \"");
				ViStringBuilder.Write(ChangeToBit(kAllLanguageStruct.French));//2
				ViStringBuilder.Write("\", \"");
				ViStringBuilder.Write(ChangeToBit(kAllLanguageStruct.German));//3
				ViStringBuilder.Write("\", \"");
				ViStringBuilder.Write(ChangeToBit(kAllLanguageStruct.Portuguese));//4
				ViStringBuilder.Write("\", \"");
				ViStringBuilder.Write(ChangeToBit(kAllLanguageStruct.Russian));//5
				ViStringBuilder.Write("\", \"");
				ViStringBuilder.Write(ChangeToBit(kAllLanguageStruct.Japanese));//6
				ViStringBuilder.Write("\", \"");
				ViStringBuilder.Write(ChangeToBit(kAllLanguageStruct.Korean));//7
				ViStringBuilder.Write("\", \"");
				ViStringBuilder.Write(ChangeToBit(kAllLanguageStruct.Spanish));//8
				ViStringBuilder.Write("\", \"");
				ViStringBuilder.Write(ChangeToBit(kAllLanguageStruct.Italian));//9
				ViStringBuilder.Write("\", \"");
				ViStringBuilder.Write(ChangeToBit(kAllLanguageStruct.Turkish));//10
				ViStringBuilder.Write("\", \"");
				ViStringBuilder.Write(ChangeToBit(kAllLanguageStruct.Traditional));//11
				ViStringBuilder.Write("\", \"");
				ViStringBuilder.Write(ChangeToBit(kAllLanguageStruct.Indonesian));//12
				ViStringBuilder.Write("\", \"");
				ViStringBuilder.Write(ChangeToBit(kAllLanguageStruct.Arabic));//13
				ViStringBuilder.Write("\", \"");
				ViStringBuilder.Write(ChangeToBit(kAllLanguageStruct.Vietnamese));//14越南语
				ViStringBuilder.Write("\", \"");
				ViStringBuilder.Write(ChangeToBit(kAllLanguageStruct.Thai));//15 泰语
				ViStringBuilder.Write("\" });");
				ViStringBuilder.WriteLine();
				//
				if (bIOS_Only)
				{
					ViStringBuilder.Write("#endif");
					ViStringBuilder.WriteLine();
				}
			}
			ViStringBuilder.WriteLine();
			ViStringBuilder.Write("}");
			ViStringBuilder.WriteLine();
			//ViStringBuilder.Close();
		}
		File.WriteAllText(UnityEngine.Application.dataPath + "/Scripts/Client/Define/GameString.cs", ViStringBuilder.Cache.ToString());
		//File.WriteAllText("c:/GameString.cs", ViStringBuilder.Cache.ToString());
#endif
	}

	static string ChangeString(string str)
	{
		return str.Replace("\"", "\\\"").Replace("$n", "\\n");
	}
	static string ChangeToBit(string str)
	{
		return StringToBinary(ChangeString(str));
	}

	public FomatStringLanguage(string[] text)
	{
#if !DATA_EDITOR
		Int32 iIdx = m_iLanguage;
		if(iIdx < text.Length)
		{
            if(string.IsNullOrEmpty(text[iIdx]) && text.Length > 0)
            {
                _text = new ViFomatString(FormatLanguageText(text[0]));
            }
            else
            {
                _text = new ViFomatString(FormatLanguageText(text[iIdx]));
            }
		}
		else if(text.Length > 0)
		{
			_text = new ViFomatString(FormatLanguageText(text[0]));
		}
		else
		{
			_text = new ViFomatString("");
		}
#endif
	}
	
	static public string FormatLanguageText(string str)
	{
		if(str == null)
		{
			return string.Empty;
		}
		return str.Replace("$R", "\r").Replace("$n", "\n");
		//return str.Replace("$R", "\r").Replace("$r", "\r").Replace("$N", "\n").Replace("$n", "\n");//技能描述$range再用
	}

	public FomatStringLanguage(AllLanguageStruct kLanguageStruct, string strDefault, Int32 ID)
	{
#if !DATA_EDITOR
		string value = kLanguageStruct.GetText(Language);
		if(string.IsNullOrEmpty(value))
		{
			ViDebuger.Notice("GameStringStruct::Start no language!", strDefault, ID);

			value = strDefault;
		}
		_text = new ViFomatString(FormatLanguageText(value));
#endif
	}

	ViFomatString FomatString
	{
		get
		{
#if !DATA_EDITOR
			return GetText(Language);
#else
			return new ViFomatString("");
#endif
		}
	}

	static public ViFomatString S_Empty_ViFomatString = new ViFomatString("");
	private ViFomatString GetText(LanguageType type)
	{
		if(_text != null)
		{
			return _text;
		}
		return S_Empty_ViFomatString;
	}

	public string Print()
	{
		return FomatString.Print();
	}

	public string Print(string value0)
	{
		return FomatString.Print(value0);
	}

	public string Print(string value0, string value1)
	{
		return FomatString.Print(value0, value1);
	}

	public string Print(string value0, string value1, string value2)
	{
		return FomatString.Print(value0, value1, value2);
	}

	public string Print(string value0, string value1, string value2, string value3)
	{
		return FomatString.Print(value0, value1, value2, value3);
	}

	public string Print(string value0, string value1, string value2, string value3, string value4)
	{
		return FomatString.Print(value0, value1, value2, value3, value4);
	}

	public string Print(string value0, string value1, string value2, string value3, string value4, string value5)
	{
		return FomatString.Print(value0, value1, value2, value3, value4, value5);
	}

	public string Print(string value0, string value1, string value2, string value3, string value4, string value5, string value6)
	{
		return FomatString.Print(value0, value1, value2, value3, value4, value5, value6);
	}

	public string Print(string[] valueArray)
	{
		return FomatString.Print(valueArray);
	}

	public override string ToString()
	{
		return FomatString.Print();
	}

	ViFomatString _text = null;

	public static implicit operator string(FomatStringLanguage data)
	{
		return data.FomatString;
	}

	public static implicit operator ViFomatString(FomatStringLanguage data)
	{
		return data.FomatString;
	}
}