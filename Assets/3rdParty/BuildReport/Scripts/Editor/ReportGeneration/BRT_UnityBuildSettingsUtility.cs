#if UNITY_5 && (!UNITY_5_0 && !UNITY_5_1)
#define UNITY_5_2_AND_GREATER
#endif

#if UNITY_5 && (!UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2)
#define UNITY_5_3_AND_GREATER
#endif

#if UNITY_4 || UNITY_5_0 || UNITY_5_1
#define UNITY_5_1_AND_LESSER
#endif

#if UNITY_4 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#define UNITY_5_2_AND_LESSER
#endif

#if UNITY_4 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3
#define UNITY_5_3_AND_LESSER
#endif

using System.Collections.Generic;
using System.Linq;
using DldUtil;
using UnityEditor;
using UnityEngine;

namespace BuildReportTool
{

    public static class UnityBuildSettingsUtility
    {
        // ================================================================================================

        public static GUIContent[] GetBuildSettingsCategoryListForDropdownBox()
        {
            // WARNING! changing contents here will require changing code in:
            //
            //  SetSelectedSettingsIdxFromBuildReportValues
            //  SetSettingsShownFromIdx
            //
            // as they rely on the array indices
            //
            return new GUIContent[]
            {
			/* 0 */ new GUIContent("Windows"),
			/* 1 */ new GUIContent("Mac"),
			/* 2 */ new GUIContent("Linux"),

			/* 3 */ new GUIContent("Web"),
			/*  4 */ new GUIContent("Web GL"),

			/*  5 */ new GUIContent("iOS"),
			/*  6 */ new GUIContent("Android"),
			/*  7 */ new GUIContent("Blackberry"),

			/*  8 */ new GUIContent("Xbox 360"),
			/*  9 */ new GUIContent("Xbox One"),
			/* 10 */ new GUIContent("Playstation 3"),
			/* 11 */ new GUIContent("Playstation 4"),

			/* 12 */ new GUIContent("Playstation Vita (Native)"),

			/* 13 */ new GUIContent("Samsung TV"),
            };
        }


        public static int GetIdxFromBuildReportValues(BuildInfo buildReportToDisplay)
        {
            BuildSettingCategory b = ReportGenerator.GetBuildSettingCategoryFromBuildValues(buildReportToDisplay);

            switch (b)
            {
                case BuildSettingCategory.WindowsDesktopStandalone:
                    return 0;
                case BuildSettingCategory.MacStandalone:
                    return 1;
                case BuildSettingCategory.LinuxStandalone:
                    return 2;

                case BuildSettingCategory.WebPlayer:
                    return 3;
                case BuildSettingCategory.WebGL:
                    return 4;

                case BuildSettingCategory.iOS:
                    return 5;
                case BuildSettingCategory.Android:
                    return 6;
                case BuildSettingCategory.Blackberry:
                    return 7;

                case BuildSettingCategory.Xbox360:
                    return 8;
                case BuildSettingCategory.XboxOne:
                    return 9;
                case BuildSettingCategory.PS3:
                    return 10;
                case BuildSettingCategory.PS4:
                    return 11;

                case BuildSettingCategory.PSVita:
                    return 12;

                case BuildSettingCategory.SamsungTV:
                    return 13;
            }
            return -1;
        }

        public static BuildSettingCategory GetSettingsCategoryFromIdx(int idx)
        {
            switch (idx)
            {
                case 0:
                    return BuildSettingCategory.WindowsDesktopStandalone;
                case 1:
                    return BuildSettingCategory.MacStandalone;
                case 2:
                    return BuildSettingCategory.LinuxStandalone;

                case 3:
                    return BuildSettingCategory.WebPlayer;
                case 4:
                    return BuildSettingCategory.WebGL;

                case 5:
                    return BuildSettingCategory.iOS;
                case 6:
                    return BuildSettingCategory.Android;
                case 7:
                    return BuildSettingCategory.Blackberry;

                case 8:
                    return BuildSettingCategory.Xbox360;
                case 9:
                    return BuildSettingCategory.XboxOne;
                case 10:
                    return BuildSettingCategory.PS3;
                case 11:
                    return BuildSettingCategory.PS4;

                case 12:
                    return BuildSettingCategory.PSVita;

                case 13:
                    return BuildSettingCategory.SamsungTV;
            }

            return BuildSettingCategory.None;
        }

        public static string GetReadableBuildSettingCategory(BuildSettingCategory b)
        {
            switch (b)
            {
                case BuildSettingCategory.WindowsDesktopStandalone:
                    return "Windows";

                case BuildSettingCategory.WindowsStoreApp:
                    return "Windows Store App";

                case BuildSettingCategory.WindowsPhone8:
                    return "Windows Phone 8";

                case BuildSettingCategory.MacStandalone:
                    return "Mac";

                case BuildSettingCategory.LinuxStandalone:
                    return "Linux";


                case BuildSettingCategory.WebPlayer:
                    return "Web Player";


                case BuildSettingCategory.Xbox360:
                    return "Xbox 360";
                case BuildSettingCategory.XboxOne:
                    return "Xbox One";

                case BuildSettingCategory.PS3:
                    return "Playstation 3";
                case BuildSettingCategory.PS4:
                    return "Playstation 4";

                case BuildSettingCategory.PSVita:
                    return "Playstation Vita (Native)";

                case BuildSettingCategory.PSM:
                    return "Playstation Mobile";

                case BuildSettingCategory.WebGL:
                    return "Web GL";
            }

            return b.ToString();
        }



        // ================================================================================================

        public static void Populate(UnityBuildSettings settings)
        {
            PopulateGeneralSettings(settings);
            PopulateWebSettings(settings);
            PopulateStandaloneSettings(settings);
            PopulateMobileSettings(settings);
            PopulateTvDeviceSettings(settings);
            PopulateBigConsoleGen07Settings(settings);
            PopulateBigConsoleGen08Settings(settings);
        }


        public static void PopulateGeneralSettings(UnityBuildSettings settings)
        {
            settings.CompanyName = PlayerSettings.companyName;
            settings.ProductName = PlayerSettings.productName;

            settings.UsingAdvancedLicense = PlayerSettings.advancedLicense;




            // debug settings
            // ---------------------------------------------------------------
            settings.EnableDevelopmentBuild = EditorUserBuildSettings.development;
            settings.EnableDebugLog = PlayerSettings.usePlayerLog;
            settings.EnableSourceDebugging = EditorUserBuildSettings.allowDebugging;
            settings.EnableExplicitNullChecks = EditorUserBuildSettings.explicitNullChecks;

#if UNITY_5
            settings.EnableCrashReportApi = PlayerSettings.enableCrashReportAPI;
            settings.EnableInternalProfiler = PlayerSettings.enableInternalProfiler;
            settings.ActionOnDotNetUnhandledException = PlayerSettings.actionOnDotNetUnhandledException.ToString();
#endif

            settings.ConnectProfiler = EditorUserBuildSettings.connectProfiler;

#if UNITY_5_3_AND_GREATER
            // this setting actually started appearing in Unity 5.2.2 (it is not present in 5.2.1)
            // but our script compilation defines can't detect the patch number in the version,
            // so we have no choice but to restrict this to 5.3
            settings.ForceOptimizeScriptCompilation = EditorUserBuildSettings.forceOptimizeScriptCompilation;
#endif


            // build settings
            // ---------------------------------------------------------------

            settings.EnableHeadlessMode = EditorUserBuildSettings.enableHeadlessMode;
            settings.InstallInBuildFolder = EditorUserBuildSettings.installInBuildFolder;
#if UNITY_5
            settings.ForceInstallation = EditorUserBuildSettings.forceInstallation;
            settings.BuildScriptsOnly = EditorUserBuildSettings.buildScriptsOnly;
            settings.BakeCollisionMeshes = PlayerSettings.bakeCollisionMeshes;
#endif

#if !UNITY_5
		settings.StripPhysicsCode = PlayerSettings.stripPhysics;
#endif
            settings.StripUnusedMeshComponents = PlayerSettings.stripUnusedMeshComponents;

#if UNITY_5_2_AND_GREATER
            settings.StripEngineCode = PlayerSettings.stripEngineCode;
#endif



            // code settings
            // ---------------------------------------------------------------

            Dictionary<string, DldUtil.GetRspDefines.Entry> customDefines = DldUtil.GetRspDefines.GetDefines();

            List<string> defines = new List<string>();
            defines.AddRange(EditorUserBuildSettings.activeScriptCompilationDefines);


            foreach (KeyValuePair<string, DldUtil.GetRspDefines.Entry> customDefine in customDefines)
            {
                if (customDefine.Value.TimesDefinedInBuiltIn == 0)
                {
                    defines.Add(customDefine.Key);
                }
            }

            settings.CompileDefines = defines.ToArray();




            settings.StrippingLevelUsed = PlayerSettings.strippingLevel.ToString();

            settings.NETApiCompatibilityLevel = PlayerSettings.apiCompatibilityLevel.ToString();

            settings.AOTOptions = PlayerSettings.aotOptions;
            settings.LocationUsageDescription = PlayerSettings.iOS.locationUsageDescription;




            // rendering settings
            // ---------------------------------------------------------------
            settings.ColorSpaceUsed = PlayerSettings.colorSpace.ToString();
            settings.UseMultithreadedRendering = PlayerSettings.MTRendering;
            settings.UseGPUSkinning = PlayerSettings.gpuSkinning;
            settings.RenderingPathUsed = PlayerSettings.renderingPath.ToString();
            settings.VisibleInBackground = PlayerSettings.visibleInBackground;

#if UNITY_5_2_AND_GREATER
            settings.EnableVirtualRealitySupport = PlayerSettings.virtualRealitySupported;
#endif

            // collect all aspect ratios
            UnityEditor.AspectRatio[] aspectRatios = {
            UnityEditor.AspectRatio.Aspect4by3,
            UnityEditor.AspectRatio.Aspect5by4,
            UnityEditor.AspectRatio.Aspect16by9,
            UnityEditor.AspectRatio.Aspect16by10,
            UnityEditor.AspectRatio.AspectOthers
        };
            List<string> aspectRatiosList = new List<string>();
            for (int n = 0, len = aspectRatios.Length; n < len; ++n)
            {
                if (PlayerSettings.HasAspectRatio(aspectRatios[n]))
                {
                    aspectRatiosList.Add(aspectRatios[n].ToString());
                }
            }

            if (aspectRatiosList.Count == 0)
            {
                aspectRatiosList.Add("none");
            }
            settings.AspectRatiosAllowed = aspectRatiosList.ToArray();

#if UNITY_5_2_AND_GREATER
            settings.GraphicsAPIsUsed = PlayerSettings.GetGraphicsAPIs(EditorUserBuildSettings.activeBuildTarget).Select(type => type.ToString()).ToArray();
#endif




            // shared settings
            // ---------------------------------------------------------------

            // shared between web and standalone
            settings.RunInBackground = PlayerSettings.runInBackground;
        }

        public static void PopulateWebSettings(UnityBuildSettings settings)
        {
            // web player settings
            // ---------------------------------------------------------------
            settings.WebPlayerDefaultScreenWidth = PlayerSettings.defaultWebScreenWidth;
            settings.WebPlayerDefaultScreenHeight = PlayerSettings.defaultWebScreenHeight;

            settings.WebPlayerEnableStreaming = EditorUserBuildSettings.webPlayerStreamed;
            settings.WebPlayerDeployOffline = EditorUserBuildSettings.webPlayerOfflineDeployment;

#if UNITY_5_3_AND_GREATER
            settings.WebPlayerFirstStreamedLevelWithResources = 0;
#else
		settings.WebPlayerFirstStreamedLevelWithResources = PlayerSettings.firstStreamedLevelWithResources;
#endif

#if UNITY_5_3_AND_LESSER
		settings.WebGLOptimizationLevel = EditorUserBuildSettings.webGLOptimizationLevel.ToString();
#endif
        }

        public static string GetReadableWebGLOptimizationLevel(string optimizationLevelCode)
        {
            switch (optimizationLevelCode)
            {
                case "1":
                    return "1: Slow (fast builds)";
                case "2":
                    return "2: Fast";
                case "3":
                    return "3: Fastest (very slow builds)";
            }

            return optimizationLevelCode;
        }


        public static void PopulateStandaloneSettings(UnityBuildSettings settings)
        {
            // standalone (windows/mac/linux) build settings
            // ---------------------------------------------------------------
            settings.StandaloneResolutionDialogSettingUsed = PlayerSettings.displayResolutionDialog.ToString();

            settings.StandaloneDefaultScreenWidth = PlayerSettings.defaultScreenWidth;
            settings.StandaloneDefaultScreenHeight = PlayerSettings.defaultScreenHeight;

            settings.StandaloneFullScreenByDefault = PlayerSettings.defaultIsFullScreen;
#if UNITY_5_3_AND_GREATER
            settings.StandaloneAllowFullScreenSwitch = PlayerSettings.allowFullscreenSwitch;
#endif

            settings.StandaloneCaptureSingleScreen = PlayerSettings.captureSingleScreen;

            settings.StandaloneForceSingleInstance = PlayerSettings.forceSingleInstance;
            settings.StandaloneEnableResizableWindow = PlayerSettings.resizableWindow;



            // windows only build settings
            // ---------------------------------------------------------------
#if UNITY_5_1_AND_LESSER
		settings.WinUseDirect3D11IfAvailable = PlayerSettings.useDirect3D11;
#endif
            settings.WinDirect3D9FullscreenModeUsed = PlayerSettings.d3d9FullscreenMode.ToString();
#if UNITY_5
            settings.WinDirect3D11FullscreenModeUsed = PlayerSettings.d3d11FullscreenMode.ToString();
#endif

#if UNITY_5_3_AND_LESSER
		settings.StandaloneUseStereoscopic3d = PlayerSettings.stereoscopic3D;
#endif



            // Windows Store App only build settings
            // ---------------------------------------------------------------
#if UNITY_5
            settings.WSAGenerateReferenceProjects = EditorUserBuildSettings.wsaGenerateReferenceProjects;
#endif
#if UNITY_5_2_AND_GREATER
            settings.WSASDK = EditorUserBuildSettings.wsaSDK.ToString();
#endif



            // mac only build settings
            // ---------------------------------------------------------------
            settings.MacUseAppStoreValidation = PlayerSettings.useMacAppStoreValidation;
            settings.MacFullscreenModeUsed = PlayerSettings.macFullscreenMode.ToString();
        }



        public static void PopulateMobileSettings(UnityBuildSettings settings)
        {
            // Mobile build settings
            // ---------------------------------------------------------------

            settings.MobileBundleIdentifier = PlayerSettings.bundleIdentifier; // ("Bundle Identifier" in iOS, "Package Identifier" in Android)
            settings.MobileBundleVersion = PlayerSettings.bundleVersion; // ("Bundle Version" in iOS, "Version Name" in Android)
            settings.MobileHideStatusBar = PlayerSettings.statusBarHidden;

            settings.MobileAccelerometerFrequency = PlayerSettings.accelerometerFrequency;

            settings.MobileDefaultOrientationUsed = PlayerSettings.defaultInterfaceOrientation.ToString();
            settings.MobileEnableAutorotateToPortrait = PlayerSettings.allowedAutorotateToPortrait;
            settings.MobileEnableAutorotateToReversePortrait = PlayerSettings.allowedAutorotateToPortraitUpsideDown;
            settings.MobileEnableAutorotateToLandscapeLeft = PlayerSettings.allowedAutorotateToLandscapeLeft;
            settings.MobileEnableAutorotateToLandscapeRight = PlayerSettings.allowedAutorotateToLandscapeRight;
            settings.MobileEnableOSAutorotation = PlayerSettings.useAnimatedAutorotation;

            settings.Use32BitDisplayBuffer = PlayerSettings.use32BitDisplayBuffer;



            // iOS only build settings
            // ---------------------------------------------------------------

            // Unity 5: EditorUserBuildSettings.appendProject is removed
#if !UNITY_5
		settings.iOSAppendedToProject = EditorUserBuildSettings.appendProject;
#endif
            settings.iOSSymlinkLibraries = EditorUserBuildSettings.symlinkLibraries;

            settings.iOSAppDisplayName = PlayerSettings.iOS.applicationDisplayName;

            settings.iOSScriptCallOptimizationUsed = PlayerSettings.iOS.scriptCallOptimization.ToString();

            settings.iOSSDKVersionUsed = PlayerSettings.iOS.sdkVersion.ToString();
            settings.iOSTargetOSVersion = PlayerSettings.iOS.targetOSVersionString;

            settings.iOSTargetDevice = PlayerSettings.iOS.targetDevice.ToString();

#if UNITY_5_3_AND_GREATER
            // not sure what the equivalent is for PlayerSettings.iOS.targetResolution in Unity 5.3
            // Unity 5.3 has a Screen.resolutions but I don't know which of those in the array would be the iOS target resolution
#else
		settings.iOSTargetResolution = PlayerSettings.iOS.targetResolution.ToString();
#endif

            settings.iOSIsIconPrerendered = PlayerSettings.iOS.prerenderedIcon;

            settings.iOSRequiresPersistentWiFi = PlayerSettings.iOS.requiresPersistentWiFi.ToString();

            settings.iOSStatusBarStyle = PlayerSettings.iOS.statusBarStyle.ToString();

#if !UNITY_5
		settings.iOSExitOnSuspend = PlayerSettings.iOS.exitOnSuspend;
#else
            settings.iOSAppInBackgroundBehavior = PlayerSettings.iOS.appInBackgroundBehavior.ToString();
#endif

            settings.iOSShowProgressBarInLoadingScreen = PlayerSettings.iOS.showActivityIndicatorOnLoading.ToString();

#if UNITY_5
            settings.iOSLogObjCUncaughtExceptions = PlayerSettings.logObjCUncaughtExceptions;
#endif

#if UNITY_5_1_AND_LESSER
		settings.iOSTargetGraphics = PlayerSettings.targetIOSGraphics.ToString();
#else
            settings.iOSTargetGraphics = string.Join(",", PlayerSettings.GetGraphicsAPIs(BuildTarget.iOS).Select(type => type.ToString()).ToArray());
#endif

            // Android only build settings
            // ---------------------------------------------------------------

            settings.AndroidBuildSubtarget = EditorUserBuildSettings.androidBuildSubtarget.ToString();

            settings.AndroidUseAPKExpansionFiles = PlayerSettings.Android.useAPKExpansionFiles;

#if UNITY_5
            settings.AndroidAsAndroidProject = EditorUserBuildSettings.exportAsGoogleAndroidProject;
            settings.AndroidIsGame = PlayerSettings.Android.androidIsGame;
            settings.AndroidTvCompatible = PlayerSettings.Android.androidTVCompatibility;
#endif

            settings.AndroidUseLicenseVerification = PlayerSettings.Android.licenseVerification;




#if !UNITY_5
		settings.AndroidUse24BitDepthBuffer = PlayerSettings.Android.use24BitDepthBuffer;
#else
            settings.AndroidDisableDepthAndStencilBuffers = PlayerSettings.Android.disableDepthAndStencilBuffers;
#endif

            settings.AndroidVersionCode = PlayerSettings.Android.bundleVersionCode;

            settings.AndroidMinSDKVersion = PlayerSettings.Android.minSdkVersion.ToString();
            settings.AndroidTargetDevice = PlayerSettings.Android.targetDevice.ToString();

            settings.AndroidSplashScreenScaleMode = PlayerSettings.Android.splashScreenScale.ToString();

            settings.AndroidPreferredInstallLocation = PlayerSettings.Android.preferredInstallLocation.ToString();

            settings.AndroidForceInternetPermission = PlayerSettings.Android.forceInternetPermission;
            settings.AndroidForceSDCardPermission = PlayerSettings.Android.forceSDCardPermission;

            settings.AndroidShowProgressBarInLoadingScreen = PlayerSettings.Android.showActivityIndicatorOnLoading.ToString();

            settings.AndroidKeyAliasName = PlayerSettings.Android.keyaliasName;
            settings.AndroidKeystoreName = PlayerSettings.Android.keystoreName;
        }


        public static void PopulateTvDeviceSettings(UnityBuildSettings settings)
        {

        }


        public static void PopulateBigConsoleGen07Settings(UnityBuildSettings settings)
        {

        }

        public static void PopulateBigConsoleGen08Settings(UnityBuildSettings settings)
        {

        }
    }

}
