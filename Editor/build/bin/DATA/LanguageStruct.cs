using System;
using System.Collections;
using System.Collections.Generic;
public enum LanguageType
{
    English = 0,      //英语
    Chinese = 1,      //简体中文
    French = 2,       //法语
    German = 3,       //德语
    Portuguese = 4,   //葡萄牙语
    Russian = 5,      //俄语
    Japanese = 6,     //日语
    Korean = 7,       //韩语
    Spanish = 8,      //西班牙语
    Italian = 9,      //意大利语
    Turkish = 10,     //土耳其语
    Traditional = 11, //繁体中文
    Indonesian = 12,  //印尼语
    Arabic = 13,      //阿拉伯语
    Vietnamese = 14,  //越南语
    Thai = 15,		  //泰国语

    Max,              //最大值,始终放最后
}
public struct LanguageStruct
{
	public static Int32 NotFindCount = 0;
	public static HashSet<string> IngoreFile = new HashSet<string>();
	static LanguageStruct()
	{
		//IngoreFile.Add("AnnouncementStruct");
	}
	public void Start(string className)
	{
		if (string.IsNullOrEmpty(ChineseText))
		{
			return;
		}
		if(IngoreFile.Contains(className))
		{
			return;
		}
		if(AllLanguageStruct.OldData.ContainsKey(className))
		{
			if(AllLanguageStruct.OldData[className].ContainsKey(ChineseText))
			{
				Language.Set(AllLanguageStruct.OldData[className][ChineseText].ID);
				return;
			}
		}
		++NotFindCount;
		//LogManager.ToMainThread_Warning("LanguageStruct::Start not find language!", className, ChineseText);
	}

	public void Start(string className, string key)
	{
		if (string.IsNullOrEmpty(ChineseText))
		{
			return;
		}
		if (IngoreFile.Contains(className))
		{
			return;
		}
		key = ChineseText + key;
		if (AllLanguageStruct.OldData.ContainsKey(className))
		{
			if (AllLanguageStruct.OldData[className].ContainsKey(key))
			{
				Language.Set(AllLanguageStruct.OldData[className][key].ID);
				return;
			}
		}
		++NotFindCount;
		//LogManager.ToMainThread_Warning("LanguageStruct::Start not find language!", className, key);
	}

	string LanguageString
	{
		get
		{
#if !DATA_EDITOR
            return FomatStringLanguage.FormatLanguageText(GetText(FomatStringLanguage.Language));
#else
            return FomatStringLanguage.FormatLanguageText(GetText(LanguageType.English));
#endif
        }
    }

	public string GetTextForTest(LanguageType type)
	{
		return FomatStringLanguage.FormatLanguageText(GetText(type));
	}

	public static ViConstValue<bool> VALUE_TestPrintLanguage = new ViConstValue<bool>("TestPrintLanguage", false);
	public string GetText(LanguageType type)
	{
		if (type == LanguageType.Chinese)
		{
			return ChineseText;
		}
		if (ViSealedDB<AllLanguageStruct>.IsLoaded == false)
		{
			ViDebuger.Error("LanguageStruct::GetText", type, ChineseText);
			return ChineseText;
		}

		AllLanguageStruct kAllLanguageStruct = null;
		try
		{
			kAllLanguageStruct = Language.Data;
		}
		catch(Exception ex)
		{
			ViDebuger.Error("LanguageStruct::GetText ID:", Language.Value, ex);
			return ChineseText;
		}
		if (kAllLanguageStruct == null || kAllLanguageStruct.ID == 0)
		{
			if(VALUE_TestPrintLanguage)
			{
				ViDebuger.Notice("GetText 1:", type, kAllLanguageStruct == null);
			}
			return ChineseText;
		}
		if (type < LanguageType.Max)
		{
			string text = kAllLanguageStruct.GetText(type);
			if (string.IsNullOrEmpty(text) == false)
			{
				return text;
			}
		}
		if (VALUE_TestPrintLanguage)
		{
			ViDebuger.Notice("GetText 2:", type, kAllLanguageStruct == null);
		}
		return ChineseText;
	}

	public static implicit operator string(LanguageStruct data)
	{
		return data.LanguageString;
	}

	public string[] Split(char[] separator, StringSplitOptions options)
	{
		return LanguageString.Split(separator, options);
	}

	public override string ToString()
	{
		return LanguageString;
	}

	public bool IsEmpty()
	{
		return (Language.Data == null || Language.Data.IsEmpty()) && string.IsNullOrEmpty(ChineseText);
	}

	public ViForeignKey<AllLanguageStruct> Language;
	public string ChineseText;
	public string EnglishText
	{
		get
		{
			return Language.Data != null ? Language.Data.English : string.Empty;
		}
	}
}

public partial class GameStringStruct : ViSealedData
{
	public static Dictionary<string, FomatStringLanguage> GameStringMap = new Dictionary<string, FomatStringLanguage>();
	public override void Start()
	{
		base.Start();
		if(ID == 0)
		{
			GameStringMap.Clear();
			return;
		}
		Message.Start(GetType().ToString());
		//
		if (GameStringMap.ContainsKey(Name))
		{
			ViDebuger.Warning("GameStringStruct warning Format key!!!", Name);
		}
		else
		{
			if (this.Message.Language.Value != 0 && this.Message.Language.Data != null)
			{
				GameStringMap[Name] = new FomatStringLanguage(this.Message.Language, this.Message.ChineseText, ID);
			}
			else
			{
				if(string.IsNullOrEmpty(this.Message.ChineseText))
				{
					GameStringMap[Name] = new FomatStringLanguage(new string[] { "" });
				}
				else
				{
					GameStringMap[Name] = new FomatStringLanguage(new string[] { this.Message.ChineseText });
#if !DATA_EDITOR
					ViDebuger.Notice("GameStringStruct::Start no language!", ID);
#endif
				}
			}
		}
	}

	public Int32 empty_0;
	public LanguageStruct Message = new LanguageStruct();
}


public partial class FomatGameStringStruct : ViSealedData
{
	public override void Start()
	{
		base.Start();
		if (ID == 0)
		{
			return;
		}
		Message.Start(GetType().ToString());
	}

	public Int32 empty_0;
	public LanguageStruct Message = new LanguageStruct();
}

public partial class ExtraGameStringStruct : ViSealedData
{
	public override void Start()
	{
		base.Start();
		if (ID == 0)
		{
			return;
		}
		Message.Start(GetType().ToString());
	}

	public Int32 empty_0;
	public LanguageStruct Message = new LanguageStruct();
}

public partial class UIWindowTextStruct : ViSealedData
{
	static public Dictionary<string, Int32> UIWindowTextStructMaps = new Dictionary<string, Int32>();

	public override void Format()
	{
		if(ID == 0)
		{
			UIWindowTextStructMaps.Clear();
		}
	}
	public override void Start()
	{
		base.Start();
		string strKey = Name + Note;
		Message.Start(GetType().ToString(), strKey);
		UIWindowTextStructMaps[strKey] = ID;
	}
	public LanguageStruct Message = new LanguageStruct();
}

[System.Serializable]
public partial class AllLanguageStruct : ViSealedData//name  note English key
{
	public static Dictionary<string, Dictionary<string, AllLanguageStruct>> OldData = new Dictionary<string, Dictionary<string, AllLanguageStruct>>();
	public override void Format()
	{
		base.Format();
		//
		RemoveYinHao(ref Name);
		RemoveYinHao(ref English);
		if (OldData.ContainsKey(Note) == false)
		{
			OldData[Note] = new Dictionary<string, AllLanguageStruct>();
		}
		string strKey = Name + key;
		if (OldData[Note].ContainsKey(strKey))
		{
			ViDebuger.Warning("AllLanguageStruct key!!", ID, Name, key, Note, OldData[Note][strKey].ID);
		}
		//
		tables = string.Empty;
		OldData[Note][strKey] = this;
		ClearOtherLanguage();
	}

	public void ClearOtherLanguage()
	{
#if !DATA_EDITOR
		//if (ReadField.VALUE_IgnoreOtherLanguage == false)
		//{
		//	return;
		//}
		string txt = GetText(FomatStringLanguage.Language);
		Name = "";
		Traditional = "";
		English = "";
		Russian = "";
		French = "";
		German = "";
		Spanish = "";
		Italian = "";
		Japanese = "";
		Indonesian = "";
		Portuguese = "";
		Arabic = "";
		Korean = "";
		Turkish = "";
		Vietnamese = "";
		Thai = "";
		Language_16 = "";
		Language_17 = "";
		Language_18 = "";
		Language_19 = "";
		Language_20 = "";

		SetText(FomatStringLanguage.Language, txt);
#endif
	}
	public string GetText(LanguageType type)
	{
		string text = string.Empty;
		switch (type)
		{
			case LanguageType.English:
				{
					text = English;
					break;
				}
			case LanguageType.Chinese:
				{
					text = Name;
					break;
				}
			case LanguageType.Traditional:
				{
					text = Traditional;
					break;
				}
			case LanguageType.Russian:
				{
					text = Russian;
					break;
				}
			case LanguageType.French:
				{
					text = French;
					break;
				}
			case LanguageType.German:
				{
					text = German;
					break;
				}
			case LanguageType.Spanish:
				{
					text = Spanish;
					break;
				}
			case LanguageType.Italian:
				{
					text = Italian;
					break;
				}
            case LanguageType.Portuguese:
                text = Portuguese;
                break;
            case LanguageType.Turkish:
                text = Turkish;
                break;
            case LanguageType.Indonesian:
                text = Indonesian;
                break;
            case LanguageType.Arabic:
                text = Arabic;
				break;
			case LanguageType.Japanese:
				text = Japanese;
                break;
			case LanguageType.Korean:
				text = Korean;
				break;
			case LanguageType.Vietnamese:
				text = Vietnamese;
				break;
			case LanguageType.Thai:
				text = Thai;
				break;
		}
		if (string.IsNullOrEmpty(text))
		{
			return English;
		}
		return text;
	}

	public void SetText(LanguageType type, string text)
	{
		switch (type)
		{
			case LanguageType.English:
				{
					English = text;
					break;
				}
			case LanguageType.Chinese:
				{
					Name = text;
					break;
				}
			case LanguageType.Traditional:
				{
					Traditional = text;
					break;
				}
			case LanguageType.Russian:
				{
					Russian = text;
					break;
				}
			case LanguageType.French:
				{
					French = text;
					break;
				}
			case LanguageType.German:
				{
					German = text;
					break;
				}
			case LanguageType.Spanish:
				{
					Spanish = text;
					break;
				}
			case LanguageType.Italian:
				{
					Italian = text;
					break;
				}
            case LanguageType.Portuguese:
				Portuguese = text;
                break;
            case LanguageType.Turkish:
				Turkish = text;
                break;
            case LanguageType.Indonesian:
				Indonesian = text;
                break;
            case LanguageType.Arabic:
				Arabic = text;
                break;
			case LanguageType.Japanese:
				Japanese = text;
				break;
			case LanguageType.Korean:
				Korean = text;
				break;
			case LanguageType.Vietnamese:
				Vietnamese = text;
				break;
			case LanguageType.Thai:
				Thai = text;
				break;
		}
	}

	public static void RemoveYinHao(ref string str)
	{
		if (str == null)
		{
			return;
		}
		if (str.Length >= 3 && str.StartsWith("\"") && str.EndsWith("\""))
		{
			str = str.Substring(1, str.Length - 2);
		}
	}

	public bool IsEmpty()
	{
		return string.IsNullOrEmpty(English);
	}
	public string tables;
	public Int32 Empty_0;
	public string key;
	public Int32 count;
	public string Traditional;
	public string English;
	public string Russian;
	public string French;
	public string German;
	public string Spanish;
	public string Italian;
	public string Japanese;
	public string Indonesian;
	public string Portuguese;
	public string Arabic;
	public string Korean;
	public string Turkish;
	public string Vietnamese;
	public string Thai;
	public string Language_16;
	public string Language_17;
	public string Language_18;
	public string Language_19;
	public string Language_20;
}



public struct SmallLanguageStruct
{
	public void Start(string className)
	{
	}

	string LanguageString
	{
		get
		{
			return GetText(_language);
		}
	}
	public bool IsEmpty()
	{
		return string.IsNullOrEmpty(ChineseText) && string.IsNullOrEmpty(EnglishText);
	}
	public string GetText(LanguageType type)
	{
		if (type == LanguageType.Chinese || type == LanguageType.Traditional)
		{
			return ChineseText;
		}
		return EnglishText;
	}

	public static implicit operator string(SmallLanguageStruct data)
	{
		return data.LanguageString;
	}

	public string[] Split(char[] separator, StringSplitOptions options)
	{
		return LanguageString.Split(separator, options);
	}

	public override string ToString()
	{
		return (string)this;
	}
	
	public string EnglishText;
	public string ChineseText;

	public static void SetLanguage(LanguageType language)
	{
		_language = language;
	}
	static LanguageType _language = LanguageType.English;
}
