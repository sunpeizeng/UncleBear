using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class BuildTools : MonoBehaviour {

	public enum BuildType
	{
		iOS,
		Android
	}

    static string[] paramList;
	static string desProjPath;

    // get a list of scene paths to be built
    static string[] GetBuildScenes()
    {
        List<string> names = new List<string>();

        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null)
                continue;
            if (e.enabled)
                names.Add(e.path);
        }
        return names.ToArray();
    }

	/* Command line parameters sample (for android):
    [0] /Applications/Unity5.5.0/Unity.app/Contents/MacOS/Unity 
    [1] -quit 
    [2] -batchmode 
    [3] -projectPath 
    [4] "project path" 
    [5] -buildTarget 
    [6] android
    [7] -executeMethod 
    [8] BuildTools.BuildAndroid
    [9] "./build/Android"
    [10] "com.biemore.android.Restaurant"
    [11] "1.0.0"
    [12] "1"
    */
    [MenuItem("Build/Build Android")]
    static void BuildAndroid()
    {
		Build (BuildType.Android);
    }

	/* Command line parameters sample (for iOS):
    [0] /Applications/Unity5.5.0/Unity.app/Contents/MacOS/Unity 
    [1] -quit 
    [2] -batchmode 
    [3] -projectPath 
    [4] "project path" 
    [5] -buildTarget 
    [6] ios
    [7] -executeMethod 
    [8] BuildTools.BuildIOS
    [9] "./build/iOS"
    [10] "com.biemore.ios.Restaurant"
    [11] "1.0.0"
    [12] "1"
    */
	[MenuItem("Build/Build iOS")]
	static void BuildIOS()
	{
		Build (BuildType.iOS);
	}

	static void Build(BuildType type)
	{
		string[] scenes = GetBuildScenes();
		if (scenes == null || scenes.Length == 0)
			return;

		paramList = System.Environment.GetCommandLineArgs();
		//set a default value to desProjPath in case no commandline parameters given (e.g. build from unity menu)
		if (type == BuildType.Android) 
			desProjPath = "./build/android";
		if (type == BuildType.iOS)
			desProjPath = "./build/xcode";

		if (paramList != null && paramList.Length > 1 && paramList[1] == "-quit")
		{
			//specify a location where this project is exported to
			desProjPath = paramList[9];
			PlayerSettings.bundleIdentifier = paramList[10];
			PlayerSettings.bundleVersion = paramList [11];

			if (type == BuildType.Android) 
			{
				PlayerSettings.Android.bundleVersionCode = int.Parse (paramList [12]);
			} 
			else if (type == BuildType.iOS)
			{
				PlayerSettings.iOS.buildNumber = paramList [12];
			}
		}

		PlayerSettings.apiCompatibilityLevel = ApiCompatibilityLevel.NET_2_0_Subset;
		PlayerSettings.strippingLevel = StrippingLevel.StripByteCode;
		EditorUserBuildSettings.development = false;

		if (Directory.Exists(desProjPath))
		{
			Directory.Delete (desProjPath, true);
		}
		Directory.CreateDirectory(desProjPath);

		if (type == BuildType.iOS) 
		{
			BuildPipeline.BuildPlayer(scenes, desProjPath, BuildTarget.iOS, BuildOptions.AcceptExternalModificationsToPlayer);
		}
		else if (type == BuildType.Android)
		{
			//set androidBuildSystem is a must, otherwise you get an error
			EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.ADT;
			string res = BuildPipeline.BuildPlayer(scenes, desProjPath, BuildTarget.Android, BuildOptions.AcceptExternalModificationsToPlayer);

			if (string.IsNullOrEmpty (res))
			{
				//create a version file to put in version and version code from player settings
				FileStream fs = null;
				string versionFile = Path.Combine (desProjPath, "version");
				if (!File.Exists (versionFile)) {
					fs = File.Create (versionFile);
				} else {
					fs = new FileStream (versionFile, FileMode.Truncate);
				}

				byte[] data = System.Text.Encoding.UTF8.GetBytes (string.Format ("version={0}\n", PlayerSettings.bundleVersion)); 
				fs.Write (data, 0, data.Length);

				data = System.Text.Encoding.UTF8.GetBytes (string.Format ("versionCode={0}\n", PlayerSettings.Android.bundleVersionCode));
				fs.Write (data, 0, data.Length);

				data = System.Text.Encoding.UTF8.GetBytes (string.Format ("productName={0}\n", PlayerSettings.productName));
				fs.Write (data, 0, data.Length);

				fs.Flush ();
				fs.Close ();
			}
		}
	}
}
