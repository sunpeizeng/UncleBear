using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using EditorPlugins.iOS.Xcode;
using EditorPlugins.iOS.Xcode.PBX;
using System.IO;

public static class iOSPostBuild
{
	[PostProcessBuild(999)]
	public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
	{
		if (buildTarget == BuildTarget.iOS)
		{
			PlistDocument infoPlist = new PlistDocument ();
			infoPlist.ReadFromFile (Path.Combine(path, "Info.plist"));
			PlistElementArray localizations = infoPlist.root.CreateArray("CFBundleLocalizations");
			localizations.AddString ("en");
			localizations.AddString ("zh_CN");
			localizations.AddString ("zh_TW");
			infoPlist.root.SetString ("NSBluetoothPeripheralUsageDescription", "是否允许此App访问您的蓝牙？");
			infoPlist.root.SetString ("NSCalendarsUsageDescription", "是否允许此App使用日历？");
			infoPlist.root.SetString ("NSCameraUsageDescription", "是否允许此App使用您的相机？");
			infoPlist.root.SetString ("NSContactsUsageDescription", "是否允许此App访问您的通讯录？");
			infoPlist.root.SetString ("NSLocationAlwaysUsageDescription", "是否允许此App始终访问您的地理位置？");
			infoPlist.root.SetString ("NSLocationWhenInUseUsageDescription", "是否允许此App在使用时访问您的地理位置？");
			infoPlist.root.SetString ("NSMicrophoneUsageDescription", "是否允许此App使用您的麦克风？");
			infoPlist.root.SetString ("NSPhotoLibraryUsageDescription", "是否允许此App访问您的相册？");
			infoPlist.WriteToFile (Path.Combine(path, "Info.plist"));

			string pbxPath = Path.Combine (path, "Unity-iPhone.xcodeproj/project.pbxproj");

			PBXProject pbxProject = new PBXProject ();
			pbxProject.ReadFromFile (pbxPath);
			string target = pbxProject.TargetGuidByName (PBXProject.GetUnityTargetName ());

			pbxProject.AddFileToBuild(target, pbxProject.AddFolderReference (Path.Combine (path, "../AppNameLocalization/iOS/en.lproj"), "en.lproj"));
			pbxProject.AddFileToBuild(target, pbxProject.AddFolderReference (Path.Combine (path, "../AppNameLocalization/iOS/zh-Hans.lproj"), "zh-Hans.lproj"));
			pbxProject.AddFileToBuild(target, pbxProject.AddFolderReference (Path.Combine (path, "../AppNameLocalization/iOS/zh-Hant.lproj"), "zh-Hant.lproj"));

			pbxProject.SetTargetAttributes ("ProvisioningStyle", "Manual");
			pbxProject.SetBuildProperty (target, "ENABLE_BITCODE", "NO");
			pbxProject.AddBuildProperty (target, "OTHER_LDFLAGS", "-ObjC");
			pbxProject.AddBuildProperty (target, "OTHER_LDFLAGS", "$(inherited)");

			//set dependent library project
			string libProjPath = "../lib_sg_projects/lib_common_ios/SG_project_ios.xcodeproj";

			pbxProject.AddExternalProjectDependency (libProjPath,
				Path.GetFileName (libProjPath),
				PBXSourceTree.Source);

			pbxProject.AddExternalLibraryDependency (target, 
				string.Format ("lib{0}.a", Path.GetFileNameWithoutExtension (libProjPath)),
				PBXGUID.Generate (), 
				libProjPath, 
				Path.GetFileNameWithoutExtension (libProjPath));

			pbxProject.AddFrameworkToProject (target, "GameKit.framework", false);
			pbxProject.AddFrameworkToProject (target, "AdSupport.framework", true);
			pbxProject.AddFrameworkToProject (target, "StoreKit.framework", false);
			pbxProject.AddFrameworkToProject (target, "WebKit.framework", false);
			//pbxProject.AddFrameworkToProject (target, "CoreTelephony.framework", false);
			//pbxProject.AddFrameworkToProject (target, "Security.framework", false);

			AddTbd (pbxProject, target, "usr/lib/libz.1.1.3.tbd");
			AddTbd (pbxProject, target, "usr/lib/libsqlite3.tbd");
			
			//友盟相关
			AddCustomFramework (pbxProject, target, "../lib_sg_projects/lib_common_ios/umeng/UMMobClick.framework", "Frameworks/Umeng");
			//Google AdMob
			AddCustomFramework (pbxProject, target, "../iOSFrameworks/GoogleMobileAds.framework", "Frameworks/GoogleAdMob");
			//Vungle
			AddCustomFramework (pbxProject, target, "../iOSFrameworks/VungleSDK.framework", "Frameworks/VungleAds");

			pbxProject.WriteToFile (pbxPath);

			//modify UnityAppController.mm
			FileEditor uac_mm = new FileEditor (Path.Combine (path, "Classes/UnityAppController.mm"));
			uac_mm.Find (@"#include ""PluginBase/AppDelegateListener.h""");
			uac_mm.Append (@"// ****** Post Process Build ******");
			uac_mm.Append (@"#include ""Libraries/Plugins/iOS/UnityBridge/SDKBridge.h""");

			if (uac_mm.Find (@"- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions") >= 0)
			{
				uac_mm.Find (@"return YES;");
				uac_mm.Insert (@"// ****** Post Process Build ******");
				uac_mm.Append (@"[SDKBridge initSDK];");
			}

			uac_mm.Save ();
		}
	}

	static void AddCustomFramework(PBXProject pbxProj, string target, string frameworkPath, string pathInProject = "Frameworks")
	{
		string frameworkName = Path.GetFileName (frameworkPath);
		string frameworkGUID = pbxProj.AddFile (frameworkPath, Path.Combine(pathInProject, frameworkName), PBXSourceTree.Source);

		pbxProj.AddFileToBuild (target, frameworkGUID);

		pbxProj.AddBuildProperty (target, "FRAMEWORK_SEARCH_PATHS", Path.Combine ("$(SRCROOT)", Path.GetDirectoryName(frameworkPath)));
	}

	static void AddTbd(PBXProject pbxProj, string target, string tbdPath, string pathInProject = "")
	{
		string tbdGUID = pbxProj.AddFile (tbdPath, Path.Combine (pathInProject, Path.GetFileName (tbdPath)), PBXSourceTree.Sdk);
		pbxProj.AddFileToBuild (target, tbdGUID);
	}
}