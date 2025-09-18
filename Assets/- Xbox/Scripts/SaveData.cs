using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

[Serializable]
//public class SaveDataInstance
//{
//    public Dictionary<string, int> SavedInt { get; set; }
//    public Dictionary<string, float> SavedFloat { get; set; }
//    public Dictionary<string, string> SavedString { get; set; }
//    public Dictionary<string, bool> SavedBool { get; set; }

//}
public  class SaveData
{
    public  Dictionary<string, int> SavedInt = new Dictionary<string, int>();
    public  Dictionary<string, float> SavedFloat = new Dictionary<string, float>();
    public  Dictionary<string, string> SavedString = new Dictionary<string, string>();
    public  Dictionary<string, bool> SavedBool = new Dictionary<string, bool>();


    public  bool HasKey(string key)
    {
        if (SavedFloat.ContainsKey(key)) return true;
        if (SavedInt.ContainsKey(key)) return true;
        if (SavedString.ContainsKey(key)) return true;
        if (SavedBool.ContainsKey(key)) return true;
        return false;
    }

    //public  string ToJson()
    //{
    //    var instance = new SaveDataInstance
    //    {
    //        SavedInt = this.SavedInt,
    //        SavedFloat = this.SavedFloat,
    //        SavedString = this.SavedString,
    //        SavedBool = this.SavedBool
    //    };

    //    return JsonUtility.ToJson(instance);
    //}

    //public  void FromJson(string json)
    //{
    //    var instance = JsonUtility.FromJson<SaveDataInstance>(json);
    //    this.SavedInt = instance.SavedInt;
    //    this.SavedFloat = instance.SavedFloat;
    //    this.SavedString = instance.SavedString;
    //    this.SavedBool = instance.SavedBool;
    //}

    public string GetSummary()
    {
        return $"\n---------------------------------------------\nSavedInt: {SavedInt.Count} \nSavedFloat: {SavedFloat.Count}\nSavedString: {SavedString.Count}\nSaveBool: {SavedBool.Count}\n---------------------------------------------\n";
    }

    public  int GetInt(string key, int defaultValue = 0)
    {
        int returnValue = defaultValue;
        if (!SavedInt.TryGetValue(key, out returnValue))
        {
            returnValue = defaultValue;// PlayerPrefs.GetInt(key, defaultValue);
        }
        return returnValue;
    }

    public  float GetFloat(string key, float defaultValue = 0)
    {
        float returnValue = defaultValue;
        if (!SavedFloat.TryGetValue(key, out returnValue))
        {
            returnValue = defaultValue;//PlayerPrefs.GetFloat(key, defaultValue);
        }

        //Debug.Log($"\n ----------- {key} = {returnValue} ----------------\n");
        return returnValue;
    }

    public  string GetString(string key, string defaultValue = "")
    {
        string returnValue = defaultValue;
        if (!SavedString.TryGetValue(key, out returnValue))
        {
            returnValue = defaultValue;//PlayerPrefs.GetString(key, defaultValue);
        }
        return returnValue;
    }
    public  bool GetBool(string key, bool defaultValue = false)
    {
        bool returnValue = defaultValue;
        if (!SavedBool.TryGetValue(key, out returnValue))
        {
            returnValue = defaultValue;// bool.Parse(PlayerPrefs.GetString($"{key}_bool", defaultValue.ToString()));
        }
        return returnValue;
    }

    public  void SetInt(string key, int setValue)
    {
        SavedInt[key] = setValue;
    }

    public  void SetFloat(string key, float setValue)
    {
        SavedFloat[key] = setValue;
    }

    public  void SetString(string key, string setValue)
    {
        SavedString[key] = setValue;
    }

    public  void SetBool(string key, bool setValue)
    {
        SavedBool[key] = setValue;
    }
    public  void DeleteAll()
    {
        SavedInt.Clear();
        SavedFloat.Clear();
        SavedString.Clear();
        SavedBool.Clear();
    }


    public  void DeleteKey(string keyToDel)
    {
        if (SavedInt.ContainsKey(keyToDel)) SavedInt.Remove(keyToDel);
        if (SavedFloat.ContainsKey(keyToDel)) SavedFloat.Remove(keyToDel);
        if (SavedString.ContainsKey(keyToDel)) SavedString.Remove(keyToDel);
        if (SavedBool.ContainsKey(keyToDel)) SavedString.Remove(keyToDel);
    }
}