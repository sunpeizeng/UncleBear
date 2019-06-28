using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ItemKey : ICSVDeserializable
{
    public string mKey;

    public void CSVDeserialize(Dictionary<string, string[]> data, int index)
    {
        mKey = data["ID"][index];
    }
}

public class GameTools
{
    [MenuItem("GameTools/Import Item Key to Consts")]
    static void ImportItemKey2Consts()
    {
        string scriptPath = Path.Combine(Application.dataPath, "Scripts/Consts.cs");
        StreamReader fileReader = new StreamReader(scriptPath);
        List<string> allLines = new List<string>();

        string line;
        while ((line = fileReader.ReadLine()) != null)
        {
            allLines.Add(line);
        }
        fileReader.Close();

        int regionStart = -1;
        int regionEnd = -1;
        for (int i = 0; i < allLines.Count; ++i)
        {
            if (allLines[i].Trim().StartsWith("#region 物品ID"))
            {
                regionStart = i;
            }
            else if (allLines[i].Trim().StartsWith("#endregion") && regionStart >= 0)
            {
                regionEnd = i;
                break;
            }
        }

        if (regionStart >= 0 && regionEnd >= 0)
        {
            allLines.RemoveRange(regionStart + 1, regionEnd - regionStart - 1);

            List<ItemKey> keyList = SerializationManager.LoadFromCSV<ItemKey>("Data/Items");
            if (keyList == null)
            {
                LogUtil.LogErrorNoTag("Error with loading 'Data/Items.csv'");
                return;
            }

            for (int i = 0; i < keyList.Count; ++i)
            {
                allLines.Insert(regionStart + 1 + i, string.Format(@"    public const string {0} = ""{1}"";", keyList[i].mKey.ToUpper(), keyList[i].mKey));
            }
        }
        else
        {
            LogUtil.LogErrorNoTag("make sure '#region 物品ID' and '#endregion' come in pair");
        }

        StreamWriter fileWriter = new StreamWriter(scriptPath);
        for(int i = 0; i < allLines.Count; ++i)
        {
            fileWriter.WriteLine(allLines[i]);
        }
        fileWriter.Close();
        AssetDatabase.ImportAsset("Assets/Scripts/Consts.cs");

        Resources.UnloadUnusedAssets();
    }
}
