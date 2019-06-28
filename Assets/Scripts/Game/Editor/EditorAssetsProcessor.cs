using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class EditorAssetsProcessor : AssetPostprocessor
{

    //导入模型前调用
    void OnPreprocessModel()
    {
        ModelImporter modelImporter = (ModelImporter)assetImporter;
        modelImporter.globalScale = 1.0f;
        if(assetPath.IndexOf("Meshes/") != -1)
            modelImporter.importMaterials = false;
        if (assetPath.IndexOf("Meshes/Test/") != -1 || assetPath.IndexOf("Meshes/Animations/") != -1)
        {
            modelImporter.animationType = ModelImporterAnimationType.Legacy;
            modelImporter.generateAnimations = ModelImporterGenerateAnimations.GenerateAnimations;
            modelImporter.animationCompression = ModelImporterAnimationCompression.KeyframeReduction;
        }
        else if (assetPath.IndexOf("Meshes/Characters/") != -1)
        {
            modelImporter.animationType = ModelImporterAnimationType.Legacy;
            modelImporter.generateAnimations = ModelImporterGenerateAnimations.GenerateAnimations;
            modelImporter.importAnimation = false;
        }
    }

    //导入图片钱调用
    void OnPreprocessTexture()
    {
        TextureImporter textureImporter = (TextureImporter)assetImporter;
        if (assetPath.IndexOf("Meshes/Test/") != -1)
        {
            textureImporter.textureType = TextureImporterType.Default;
        }
        textureImporter.mipmapEnabled = false;
      
    }


    [MenuItem("Tools/UncleBear/GenAnimationClips")]
    static void GenAnimationClips()
    {
        EditorWindow.GetWindow<GenAnimPrefabsByType>(false, "Gen Anim Pre", true);
    }

    [MenuItem("Tools/UncleBear/GenModelPrefabs")]
    static void GenModelPrefabs()
    {
        EditorWindow.GetWindow<CreatePrefabByType>(false, "Gen Pre", true);
    }
}

#region 动画
public class GenAnimPrefabsByType : EditorWindow
{
    private string folderName = "";

    private void OnGUI()
    {
        EditorGUIUtility.LookLikeControls(100f);
        folderName = EditorGUILayout.TextField("FolderName", folderName);

        bool create = GUILayout.Button("Create", GUILayout.Width(150f));

        if (create)
        {
            string asset_path = "Assets/Meshes/Animations/";
            if (folderName != "")
            {
                asset_path += folderName + "/";
            }

            //string asset_path = "Assets/Projects/Models/LDK/Animations/" + folderName + "/";

            string[] files = Directory.GetDirectories(asset_path);

            foreach (string file in files)
            {
                ForeachPath(file);
            }

            string prefab_path = asset_path.Replace("Meshes/", "Resources/Prefabs/");
            ConvertAvatarAnimations(asset_path, prefab_path);

            EditorUtility.DisplayDialog("CreateAnimPrefabs", "Success", "Ok");

        }
    }


    //! 遍历文件夹
    private void ForeachPath(string path)
    {
        //! 处理当前文件夹内的obj
        string model_path = path + "/";
        string prefab_path = model_path.Replace("Meshes/", "Resources/Prefabs/");
        ConvertAvatarAnimations(model_path, prefab_path);

        //! 遍历子文件夹
        string[] files = Directory.GetDirectories(model_path);
        foreach (string file in files)
        {
            ForeachPath(file);
        }
    }

    private void ConvertAvatarAnimations(string model_path, string prefab_path)
    {
        Dictionary<string, AnimationEvent[]> old_events_list = new Dictionary<string, AnimationEvent[]>();

        if (!Directory.Exists(prefab_path))
        {
            Directory.CreateDirectory(prefab_path);
        }
        else
        {
            string[] delete_files = Directory.GetFiles(prefab_path);
            foreach (string delete_file in delete_files)
            {
                AnimationClip old_clip = AssetDatabase.LoadAssetAtPath(delete_file, typeof(AnimationClip)) as AnimationClip;
                if (null != old_clip)
                {
                    AnimationEvent[] old_events = AnimationUtility.GetAnimationEvents(old_clip);
                    if (old_events.Length > 0)
                    {
                        old_events_list.Add(old_clip.name, old_events);
                    }
                }
                AssetDatabase.DeleteAsset(delete_file);
            }
        }
        string[] files = Directory.GetFiles(model_path);
        foreach (string file in files)
        {
            AnimationClip clip = (AnimationClip)AssetDatabase.LoadAssetAtPath(file, typeof(AnimationClip));
            if (clip is AnimationClip)
            {
                AnimationClip clone_clip = Object.Instantiate(clip) as AnimationClip;
                clone_clip.name = clip.name;
                if (old_events_list.ContainsKey(clone_clip.name))
                {
                    AnimationUtility.SetAnimationEvents(clone_clip, old_events_list[clone_clip.name]);
                }

                string path = prefab_path + clone_clip.name + ".anim";
                if (clone_clip.name.Contains("idle") ||
                    clone_clip.name.Contains("standby") ||
                    clone_clip.name.Contains("run") ||
                    clone_clip.name.Contains("move"))
                {
                    clone_clip.wrapMode = WrapMode.Loop;
                }

                AssetDatabase.CreateAsset(clone_clip, path);
            }
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }
}
#endregion

#region 普通物件
public class CreatePrefabByType : EditorWindow
{
    private string folderName = "";

    private void OnGUI()
    {
        EditorGUIUtility.LookLikeControls(100f);
        folderName = EditorGUILayout.TextField("FolderName", folderName);

        bool create = GUILayout.Button("Create", GUILayout.Width(150f));

        if (create)
        {
            string asset_path = "Assets/Meshes/";
            if (folderName != "")
            {
                asset_path += folderName + "/";
            }

            //string asset_path = "Assets/Projects/Models/LDK/Characters/" + folderName + "/";

            string[] files = Directory.GetDirectories(asset_path);

            foreach (string file in files)
            {
                ForeachPath(file);
            }

            string prefab_path = asset_path.Replace("Meshes", "Resources/Prefabs");
            ConvertModels(asset_path, prefab_path);

            EditorUtility.DisplayDialog("CreateWeaponPrefabs", "Success", "Ok");

        }
    }

    //! 遍历文件夹
    private void ForeachPath(string path)
    {
        //! 处理当前文件夹内的obj
        string model_path = path + "/";
        string prefab_path = model_path.Replace("Meshes", "Resources/Prefabs");
        ConvertModels(model_path, prefab_path);

        //! 遍历子文件夹
        string[] files = Directory.GetDirectories(model_path);
        foreach (string file in files)
        {
            ForeachPath(file);
        }
    }


    private void ConvertModels(string model_path, string prefab_path)
    {
        if (!Directory.Exists(prefab_path))
        {
            Directory.CreateDirectory(prefab_path);
        }
        else
        {
            string[] delete_files = Directory.GetFiles(prefab_path);
            foreach (string delete_file in delete_files)
            {
                AssetDatabase.DeleteAsset(delete_file);
            }
        }


        string[] files = Directory.GetFiles(model_path);
        foreach (string file in files)
        {
            GameObject modelObj = (GameObject)AssetDatabase.LoadAssetAtPath(file, typeof(GameObject));
          
            if (modelObj is GameObject)
            {
                Object prefab = null;
                var modelTrans = modelObj.GetComponentsInChildren<Transform>();
                for (int i = 0; i < modelTrans.Length; i++)
                {
                    modelTrans[i].localScale = Vector3.one;
                }
                string path = prefab_path + modelObj.name + ".prefab";
                if (!File.Exists(path))
                {
                    prefab = PrefabUtility.CreateEmptyPrefab(path);
                }
                else
                {
                    prefab = (Object)AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                }

                PrefabUtility.ReplacePrefab(modelObj, prefab);
            }
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }
}
#endregion
