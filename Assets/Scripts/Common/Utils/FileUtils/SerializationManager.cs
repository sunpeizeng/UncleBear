using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializationManager
{
    static public List<T> LoadFromCSV<T>(string filePath) where T : ICSVDeserializable, new()
    {
        TextAsset ta = Resources.Load<TextAsset>(filePath);
        if (ta == null)
        {
            LogUtil.LogError("SerializationManager", "file: {0} not found", filePath);
            return null;
        }

        Dictionary<string, string[]> csvData = ParseCSV(ta.text);

        int count = 0;
        foreach (string[] value in csvData.Values)
        {
            count = value.Length;
            break;
        }

        if (count > 0)
        {
            List<T> retVal = new List<T>();

            for (int i = 0; i < count; ++i) 
            {
                T element = new T();
                element.CSVDeserialize(csvData, i);
                retVal.Add(element);
            }

            return retVal;
        }

        return null;
    }

    static Dictionary<string, string[]> ParseCSV(string csvText)
    {
        csvText = csvText.Trim();
        string[] lines = csvText.Split('\n');

        if (lines.Length < 1)
        {
            return null;
        }

        Dictionary<string, string[]> retVal = new Dictionary<string, string[]>();

        //判断CSV文件的最后一行是否为有效数据，防止出现空行
        int validLineCount = string.IsNullOrEmpty(lines[lines.Length - 1]) ? lines.Length - 2 : lines.Length - 1;//最后行数还要减去1，因为表头占一行

        string[] headSplits = SplitCSVLine(lines[0]);
        for (int i = 0; i < headSplits.Length; ++i)
        {
            retVal.Add(headSplits[i], new string[validLineCount]);
        }

        for (int i = 1; i < validLineCount + 1; ++i)
        {
            string[] values = SplitCSVLine(lines[i]);
            for (int j = 0; j < values.Length; ++j)
            {
                retVal[headSplits[j]][i - 1] = values[j];
            }
        }

        return retVal;
    }

    // splits a CSV row 
    static string[] SplitCSVLine(string line)
    {
        line = line.Trim();
        return line.Split(',');
    }

    //后面可以增加例如从XML中读取配置文件并反序列化的方法

}
