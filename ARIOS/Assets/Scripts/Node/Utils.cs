using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
public static class Utils
{
    // ���׸� �޼���: T Ÿ���� �޾Ƽ� JSON ���ڿ��� T Ÿ�� ��ü�� ��ȯ
    public static T ParseJson<T>(string json)
    {
        try
        {
            // JSON ���ڿ��� ���׸� Ÿ�� ��ü�� ��ȯ
            T obj = JsonConvert.DeserializeObject<T>(json);
            return obj;
        }
        catch (Exception e)
        {
            Debug.LogError($"JSON �Ľ� �� ���� �߻�: {e.Message}");
            return default;
        }
    }
    public static List<T> ParseJsonFromList<T>(string json)
    {
        try
        {
            // JSON ���ڿ��� List<T> Ÿ�� ��ü�� ��ȯ
            List<T> objList = JsonConvert.DeserializeObject<List<T>>(json);
            return objList;
        }
        catch (Exception e)
        {
            Debug.LogError($"JSON �Ľ� �� ���� �߻�: {e.Message}");
            return null;
        }
    }
    public static string LoadJsonFile(string filePath)
    {
        string json = System.IO.File.ReadAllText(filePath);
        Debug.Log(json);
        return json;
    }
    // ���׸� �޼���: T Ÿ���� �޾Ƽ� JSON ������ T Ÿ�� ��ü�� ��ȯ
}


