using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
public static class Utils
{
    // 제네릭 메서드: T 타입을 받아서 JSON 문자열을 T 타입 객체로 변환
    public static T ParseJson<T>(string json)
    {
        try
        {
            // JSON 문자열을 제네릭 타입 객체로 변환
            T obj = JsonConvert.DeserializeObject<T>(json);
            return obj;
        }
        catch (Exception e)
        {
            Debug.LogError($"JSON 파싱 중 오류 발생: {e.Message}");
            return default;
        }
    }
    public static List<T> ParseJsonFromList<T>(string json)
    {
        try
        {
            // JSON 문자열을 List<T> 타입 객체로 변환
            List<T> objList = JsonConvert.DeserializeObject<List<T>>(json);
            return objList;
        }
        catch (Exception e)
        {
            Debug.LogError($"JSON 파싱 중 오류 발생: {e.Message}");
            return null;
        }
    }
    public static string LoadJsonFile(string filePath)
    {
        string json = System.IO.File.ReadAllText(filePath);
        Debug.Log(json);
        return json;
    }
    // 제네릭 메서드: T 타입을 받아서 JSON 파일을 T 타입 객체로 변환
}


