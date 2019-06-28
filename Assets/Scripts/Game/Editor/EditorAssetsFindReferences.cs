using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;

public class EditorAssetsFindReferences
{
    [MenuItem("Assets/Find References", false)]
    static private void Find()
    {
        List<string> selectPaths = new List<string>();
        EditorSettings.serializationMode = SerializationMode.ForceText;
        string mainPath = "";//AssetDatabase.GetAssetPath(Selection.activeObject);
        for (int i = 0; i < Selection.objects.Length; i++)
        {
            string selectPath = AssetDatabase.GetAssetPath(Selection.objects[i]);
            if (selectPath.IndexOf('.') == -1)
                selectPaths.Add(selectPath);
            else
            {
                if (string.IsNullOrEmpty(mainPath))
                    mainPath = selectPath;
                else
                {
                    Debug.LogError("do not select multi source!");
                }
            }
        }
       
        if (!string.IsNullOrEmpty(mainPath))
        {
            string guid = AssetDatabase.AssetPathToGUID(mainPath);
            List<string> withoutExtensions = new List<string>() { ".prefab", ".unity", ".mat", ".asset" };//只考虑这些类型

            List<string> searchPaths = new List<string>();
            string[] files = null;
            if (selectPaths.Count == 0)
                files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories).Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
            else
            {
                for (int i = 0; i < selectPaths.Count; i++)
                {
                    string[] temp = Directory.GetFiles(selectPaths[i], "*.*", SearchOption.AllDirectories).Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
                    if (temp != null)
                    {
                        for (int j = 0; j < temp.Length; j++)
                        {
                            if (!searchPaths.Contains(temp[j]))
                                searchPaths.Add(temp[j]);
                        }
                    }
                }
                files = searchPaths.ToArray();
            }

            int startIndex = 0;

            if (files.Length == 0)
            {
                Debug.Log("No available file/path to search, maybe you have chose a wrong directory.");
                return;
            }

            EditorApplication.update = () => {
                string file = files[startIndex];
                bool isCancel = EditorUtility.DisplayCancelableProgressBar("Searching...", file, (float)startIndex);

                if (Regex.IsMatch(File.ReadAllText(file), guid))
                {
                    //这边把找到的东西debug出来
                    Debug.Log(file, AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(file)));
                }

                startIndex++;
                if (isCancel || startIndex >= files.Length)
                {
                    EditorUtility.ClearProgressBar();
                    EditorApplication.update = null;
                    startIndex = 0;
                    Debug.Log("Finished.");
                }
            };
        }
    }

    static private string GetRelativeAssetsPath(string path)
    {
        return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
    }
}
