using UnityEditor.Callbacks;
using UnityEditor;
using EditorPlugins.iOS.Xcode;
using EditorPlugins.iOS.Xcode.PBX;
using System.IO;

public class PostBuild
{
	[PostProcessBuild(1000)]
	public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
	{
		if (buildTarget == BuildTarget.iOS) 
		{
			string pbxPath = Path.Combine (path, "Unity-iPhone.xcodeproj/project.pbxproj");
			PBXProject pbxProject = new PBXProject ();
			pbxProject.ReadFromFile (pbxPath);

			string target = pbxProject.TargetGuidByName (PBXProject.GetUnityTargetName());
			pbxProject.SetTargetAttributes ("DevelopmentTeam", "P444JSG57R");
			pbxProject.SetBuildPropertyForConfig (pbxProject.BuildConfigByName(target, "Debug"), "PROVISIONING_PROFILE", "9eff9443-c0dc-4260-b17d-501c3e997508");
			pbxProject.SetBuildPropertyForConfig (pbxProject.BuildConfigByName(target, "Debug"), "PROVISIONING_PROFILE_SPECIFIER", "Restaurant_dev");
			pbxProject.SetBuildPropertyForConfig (pbxProject.BuildConfigByName(target, "Debug"), "DEVELOPMENT_TEAM", "P444JSG57R");

			pbxProject.WriteToFile (pbxPath);

			FileEditor xcscheme = new FileEditor( Path.Combine(path, "Unity-iPhone.xcodeproj/xcshareddata/xcschemes/Unity-iPhone.xcscheme"));
			if (xcscheme.Find ("<LaunchAction", FileEditor.MatchPattern.StartsWith) >= 0)
			{
				xcscheme.ReplaceSubString (@"buildConfiguration=""ReleaseForRunning""", @"buildConfiguration=""Debug""");
			}
			xcscheme.Save ();
		}
	}
}
