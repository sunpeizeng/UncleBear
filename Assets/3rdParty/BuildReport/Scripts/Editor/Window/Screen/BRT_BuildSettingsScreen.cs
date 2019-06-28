#if UNITY_5 && (!UNITY_5_0 && !UNITY_5_1)
#define UNITY_5_2_AND_GREATER
#endif

#if (UNITY_4 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
#define UNITY_5_2_AND_LESS
#endif

#if UNITY_4 || UNITY_5_0 || UNITY_5_1
#define UNITY_5_1_AND_LESSER
#endif

using UnityEngine;
using UnityEditor;



namespace BuildReportTool.Window.Screen
{

public class BuildSettings : BaseScreen
{
	public override string Name { get{ return Labels.BUILD_SETTINGS_CATEGORY_LABEL; } }

	public override void RefreshData(BuildInfo buildReport)
	{
		_selectedSettingsIdxFromDropdownBox = UnityBuildSettingsUtility.GetIdxFromBuildReportValues(buildReport);
	}

	Vector2 _scrollPos;

	const int SETTING_SPACING = 4;
	const int SETTINGS_GROUP_TITLE_SPACING = 3;
	const int SETTINGS_GROUP_SPACING = 18;
	const int SETTINGS_GROUP_MINOR_SPACING = 12;


	void DrawSetting(string name, bool val, bool showEvenIfEmpty = true)
	{
		DrawSetting(name, val.ToString(), showEvenIfEmpty);
	}

	void DrawSetting(string name, int val, bool showEvenIfEmpty = true)
	{
		DrawSetting(name, val.ToString(), showEvenIfEmpty);
	}

	void DrawSetting(string name, uint val, bool showEvenIfEmpty = true)
	{
		DrawSetting(name, val.ToString(), showEvenIfEmpty);
	}

	void DrawSetting(string name, string val, bool showEvenIfEmpty = true)
	{
		if (string.IsNullOrEmpty(val) && !showEvenIfEmpty)
		{
			return;
		}

		GUILayout.BeginHorizontal();
			GUILayout.Label(name, BuildReportTool.Window.Settings.SETTING_NAME_STYLE_NAME);
			GUILayout.Space(2);
			if (!string.IsNullOrEmpty(val))
			{
				GUILayout.TextField(val, BuildReportTool.Window.Settings.SETTING_VALUE_STYLE_NAME);
			}
		GUILayout.EndHorizontal();
		GUILayout.Space(SETTING_SPACING);
	}

	void DrawSetting(string name, string[] val, bool showEvenIfEmpty = true)
	{
		if ((val == null || val.Length == 0) && !showEvenIfEmpty)
		{
			return;
		}

		GUILayout.BeginHorizontal();
			GUILayout.Label(name, BuildReportTool.Window.Settings.SETTING_NAME_STYLE_NAME);
			GUILayout.Space(2);

			
			if (val != null)
			{
				GUILayout.BeginVertical();
					for (int n = 0, len = val.Length; n < len; ++n)
					{
						GUILayout.TextField(val[n], BuildReportTool.Window.Settings.SETTING_VALUE_STYLE_NAME);
					}
				GUILayout.EndVertical();
			}

		GUILayout.EndHorizontal();
		GUILayout.Space(SETTING_SPACING);
	}

	void DrawSettingsGroupTitle(string name)
	{
		GUILayout.Label(name, BuildReportTool.Window.Settings.INFO_TITLE_STYLE_NAME);
		GUILayout.Space(SETTINGS_GROUP_TITLE_SPACING);
	}

	// =================================================================================

	BuildSettingCategory _settingsShown = BuildSettingCategory.None;

	// ----------------------------------------------

	bool IsShowingWebPlayerSettings
	{
		get
		{
			return _settingsShown == BuildSettingCategory.WebPlayer;
		}
	}

	bool IsShowingWebGlSettings
	{
		get
		{
			return _settingsShown == BuildSettingCategory.WebGL;
		}
	}
	
	// ----------------------------------------------

	bool IsShowingStandaloneSettings
	{
		get
		{
			return IsShowingWindowsDesktopSettings || IsShowingMacSettings || IsShowingLinuxSettings;
		}
	}

	bool IsShowingWindowsDesktopSettings
	{
		get
		{
			return _settingsShown == BuildSettingCategory.WindowsDesktopStandalone;
		}
	}

	bool IsShowingWindowsStoreAppSettings
	{
		get
		{
			return _settingsShown == BuildSettingCategory.WindowsStoreApp;
		}
	}

	bool IsShowingMacSettings
	{
		get
		{
			return _settingsShown == BuildSettingCategory.MacStandalone;
		}
	}

	bool IsShowingLinuxSettings
	{
		get
		{
			return _settingsShown == BuildSettingCategory.LinuxStandalone;
		}
	}

	// ----------------------------------------------

	bool IsShowingMobileSettings
	{
		get
		{
			return IsShowingiOSSettings || IsShowingAndroidSettings || IsShowingBlackberrySettings;
		}
	}

	bool IsShowingiOSSettings
	{
		get
		{
			return _settingsShown == BuildSettingCategory.iOS;
		}
	}

	bool IsShowingAndroidSettings
	{
		get
		{
			return _settingsShown == BuildSettingCategory.Android;
		}
	}

	bool IsShowingBlackberrySettings
	{
		get
		{
			return _settingsShown == BuildSettingCategory.Blackberry;
		}
	}

	// ----------------------------------------------

	bool IsShowingXbox360Settings
	{
		get
		{
			return _settingsShown == BuildSettingCategory.Xbox360;
		}
	}

	bool IsShowingXboxOneSettings
	{
		get
		{
			return _settingsShown == BuildSettingCategory.XboxOne;
		}
	}
	
	bool IsShowingPS3Settings
	{
		get
		{
			return _settingsShown == BuildSettingCategory.PS3;
		}
	}

	bool IsShowingPS4Settings
	{
		get
		{
			return _settingsShown == BuildSettingCategory.PS4;
		}
	}

	bool IsShowingPSVitaSettings
	{
		get
		{
			return _settingsShown == BuildSettingCategory.PSVita;
		}
	}

	// ----------------------------------------------

	bool IsShowingSamsungTvSettings
	{
		get
		{
			return _settingsShown == BuildSettingCategory.SamsungTV;
		}
	}
	
	// =================================================================================

	bool UnityMajorVersionUsedIsAtMost(int versionAtMost, string unityVersionName)
	{
		return BuildReportTool.Util.UnityMajorVersionUsedIsAtMost(versionAtMost, unityVersionName);
	}
	
	bool UnityMajorVersionUsedIsAtLeast(int versionAtMost, string unityVersionName)
	{
		return BuildReportTool.Util.UnityMajorVersionUsedIsAtLeast(versionAtMost, unityVersionName);
	}
	
	// =================================================================================

	int _selectedSettingsIdxFromDropdownBox = 0;

	GUIContent[] _settingDropdownBoxLabels;
	string _buildTargetOfReport = string.Empty;

	void InitializeDropdownBoxLabelsIfNeeded()
	{
		if (_settingDropdownBoxLabels != null) { return; }

		_settingDropdownBoxLabels = UnityBuildSettingsUtility.GetBuildSettingsCategoryListForDropdownBox();
	}



	// =================================================================================

	void DrawProjectSettings(BuildInfo buildReportToDisplay, UnityBuildSettings settings)
	{
		DrawSettingsGroupTitle("Project");

		DrawSetting("Product name:", settings.ProductName);
		DrawSetting("Company name:", settings.CompanyName);
		DrawSetting("Build type:", buildReportToDisplay.BuildType);
		DrawSetting("Unity version:", buildReportToDisplay.UnityVersion);
		DrawSetting("Using Pro license:", settings.UsingAdvancedLicense);

		if (IsShowingiOSSettings)
		{
			GUILayout.Space(SETTINGS_GROUP_MINOR_SPACING);
			DrawSetting("App display name:", settings.iOSAppDisplayName);
			DrawSetting("Bundle identifier:", settings.MobileBundleIdentifier);
			DrawSetting("Bundle version:", settings.MobileBundleVersion);
		}
		else if (IsShowingAndroidSettings)
		{
			GUILayout.Space(SETTINGS_GROUP_MINOR_SPACING);
			DrawSetting("Package identifier:", settings.MobileBundleIdentifier);
			DrawSetting("Version name:", settings.MobileBundleVersion);
			DrawSetting("Version code:", settings.AndroidVersionCode);
		}
	}

	void DrawBuildSettings(BuildInfo buildReportToDisplay, UnityBuildSettings settings)
	{
		DrawSettingsGroupTitle("Build Settings");

		// --------------------------------------------------
		// build settings
		if (IsShowingStandaloneSettings)
		{
			DrawSetting("Enable headless mode:", settings.EnableHeadlessMode);
		}
		else if (IsShowingWindowsStoreAppSettings)
		{
			DrawSetting("Generate reference projects:", settings.WSAGenerateReferenceProjects);
			DrawSetting("Target Windows Store App SDK:", settings.WSASDK);
		}
		else if (IsShowingWebPlayerSettings)
		{
			DrawSetting("Web player streaming:", settings.WebPlayerEnableStreaming);
			DrawSetting("Web player offline deployment:", settings.WebPlayerDeployOffline);
			DrawSetting("First streamed level with \"Resources\" assets:", settings.WebPlayerFirstStreamedLevelWithResources);
			GUILayout.Space(SETTINGS_GROUP_MINOR_SPACING);
		}
		else if (IsShowingWebGlSettings)
		{
			DrawSetting("WebGL optimization level:", UnityBuildSettingsUtility.GetReadableWebGLOptimizationLevel(settings.WebGLOptimizationLevel));
			GUILayout.Space(SETTINGS_GROUP_MINOR_SPACING);
		}
		else if (IsShowingiOSSettings)
		{
			DrawSetting("SDK version:", settings.iOSSDKVersionUsed);
			DrawSetting("Target iOS version:", settings.iOSTargetOSVersion);
			DrawSetting("Target device:", settings.iOSTargetDevice);
			GUILayout.Space(SETTINGS_GROUP_MINOR_SPACING);
		}
		else if (IsShowingAndroidSettings)
		{
			DrawSetting("Build subtarget:", settings.AndroidBuildSubtarget);
			DrawSetting("Min SDK version:", settings.AndroidMinSDKVersion);
			DrawSetting("Target device:", settings.AndroidTargetDevice);
			DrawSetting("Automatically create APK Expansion File:", settings.AndroidUseAPKExpansionFiles);
			DrawSetting("Export Android project:", settings.AndroidAsAndroidProject);
			GUILayout.Space(SETTINGS_GROUP_MINOR_SPACING);

			
			DrawSetting("Is game:", settings.AndroidIsGame);
			DrawSetting("TV-compatible:", settings.AndroidTvCompatible);
			GUILayout.Space(SETTINGS_GROUP_MINOR_SPACING);


			DrawSetting("Force Internet permission:", settings.AndroidForceInternetPermission);
			DrawSetting("Force SD card permission:", settings.AndroidForceSDCardPermission);
			GUILayout.Space(SETTINGS_GROUP_MINOR_SPACING);


			DrawSetting("Key alias name:", settings.AndroidKeyAliasName);
			DrawSetting("Keystore name:", settings.AndroidKeystoreName);
		}

		if (IsShowingiOSSettings && UnityMajorVersionUsedIsAtMost(4, buildReportToDisplay.UnityVersion))
		{
			DrawSetting("Is appended build:", settings.iOSAppendedToProject);
		}
		DrawSetting("Install in build folder:", settings.InstallInBuildFolder);

		if (UnityMajorVersionUsedIsAtMost(4, buildReportToDisplay.UnityVersion))
		{
			DrawSetting("Physics code stripped:", settings.StripPhysicsCode);
		}

		DrawSetting("Prebake collision meshes:", settings.BakeCollisionMeshes);
		DrawSetting("Optimize mesh data:", settings.StripUnusedMeshComponents);

		if (IsShowingMobileSettings)
		{
			DrawSetting("Stripping level:", settings.StrippingLevelUsed);
		}
		else if (IsShowingWebGlSettings)
		{
			DrawSetting("Strip engine code (IL2CPP):", settings.StripEngineCode);
		}
	}

	void DrawRuntimeSettings(BuildInfo buildReportToDisplay, UnityBuildSettings settings)
	{
		DrawSettingsGroupTitle("Runtime Settings");

		if (IsShowingiOSSettings)
		{
			DrawSetting("Hide status bar:", settings.MobileHideStatusBar);
			DrawSetting("Status bar style:", settings.iOSStatusBarStyle);
			DrawSetting("Accelerometer frequency:", settings.MobileAccelerometerFrequency);
			DrawSetting("Requires persistent Wi-Fi:", settings.iOSRequiresPersistentWiFi);

			if (UnityMajorVersionUsedIsAtMost(4, buildReportToDisplay.UnityVersion))
			{
			DrawSetting("Exit on suspend:", settings.iOSExitOnSuspend);
			}
			if (UnityMajorVersionUsedIsAtLeast(5, buildReportToDisplay.UnityVersion))
			{
				DrawSetting("App-in-background behavior:", settings.iOSAppInBackgroundBehavior);
			}

			
			DrawSetting("Activity indicator on loading:", settings.iOSShowProgressBarInLoadingScreen);

			GUILayout.Space(SETTINGS_GROUP_MINOR_SPACING);
		}
		else if (IsShowingAndroidSettings)
		{
			DrawSetting("Hide status bar:", settings.MobileHideStatusBar);
			DrawSetting("Accelerometer frequency:", settings.MobileAccelerometerFrequency);
			DrawSetting("Activity indicator on loading:", settings.AndroidShowProgressBarInLoadingScreen);
			DrawSetting("Splash screen scale:", settings.AndroidSplashScreenScaleMode);

			DrawSetting("Preferred install location:", settings.AndroidPreferredInstallLocation);

			GUILayout.Space(SETTINGS_GROUP_MINOR_SPACING);
		}


		if (!IsShowingiOSSettings && !IsShowingAndroidSettings && IsShowingMobileSettings) // any mobile except iOS, Android
		{
			DrawSetting("Hide status bar:", settings.MobileHideStatusBar);
			DrawSetting("Accelerometer frequency:", settings.MobileAccelerometerFrequency);
			GUILayout.Space(SETTINGS_GROUP_MINOR_SPACING);
		}

		if (IsShowingStandaloneSettings || IsShowingWebPlayerSettings || IsShowingBlackberrySettings)
		{
			DrawSetting("Run in background:", settings.RunInBackground);
			GUILayout.Space(SETTINGS_GROUP_MINOR_SPACING);
		}
	}

	void DrawDebugSettings(BuildInfo buildReportToDisplay, UnityBuildSettings settings)
		{
		DrawSettingsGroupTitle("Debug Settings");

		DrawSetting("Is development build:", settings.EnableDevelopmentBuild);
		DrawSetting("Debug Log enabled:", settings.EnableDebugLog);
		DrawSetting("Auto-connect to Unity profiler:", settings.ConnectProfiler);
		DrawSetting("Enable internal profiler:", settings.EnableInternalProfiler);
		DrawSetting("Allow debugger:", settings.EnableSourceDebugging);
		DrawSetting("Enable explicit null checks:", settings.EnableExplicitNullChecks);
		DrawSetting("Action on .NET unhandled exception:", settings.ActionOnDotNetUnhandledException);
		DrawSetting("Enable CrashReport API:", settings.EnableCrashReportApi);
		DrawSetting("Force script optimization on debug builds:", settings.ForceOptimizeScriptCompilation);

		if (IsShowingiOSSettings)
		{
			DrawSetting("Log Objective-C uncaught exceptions:", settings.iOSLogObjCUncaughtExceptions);
		}
	}

	void DrawCodeSettings(BuildInfo buildReportToDisplay, UnityBuildSettings settings)
	{
		DrawSettingsGroupTitle("Code Settings");

		DrawSetting("Script Compilation Defines:", settings.CompileDefines);
		
		DrawSetting(".NET API compatibility level:", settings.NETApiCompatibilityLevel);
		DrawSetting("AOT options:", settings.AOTOptions, true);
		DrawSetting("Location usage description:", settings.LocationUsageDescription);

		if (IsShowingiOSSettings)
		{
			DrawSetting("Symlink libraries:", settings.iOSSymlinkLibraries);
			DrawSetting("Script call optimized:", settings.iOSScriptCallOptimizationUsed);
			//GUILayout.Space(SETTINGS_GROUP_MINOR_SPACING);
		}
	}

	void DrawGraphicsSettings(BuildInfo buildReportToDisplay, UnityBuildSettings settings)
	{
		DrawSettingsGroupTitle("Graphics Settings");

		DrawSetting("Use 32-bit display buffer:", settings.Use32BitDisplayBuffer);
		DrawSetting("Rendering path:", settings.RenderingPathUsed);
		DrawSetting("Color space:", settings.ColorSpaceUsed);
		DrawSetting("Use multi-threaded rendering:", settings.UseMultithreadedRendering);
		DrawSetting("Use GPU skinning:", settings.UseGPUSkinning);
		DrawSetting("Enable Virtual Reality Support:", settings.EnableVirtualRealitySupport);
#if UNITY_5_2_AND_GREATER
		DrawSetting("Graphics APIs Used:", settings.GraphicsAPIsUsed);
#endif
		GUILayout.Space(SETTINGS_GROUP_MINOR_SPACING);

		if (IsShowingMobileSettings)
		{
			DrawSetting("Default interface orientation:", settings.MobileDefaultOrientationUsed);

			DrawSetting("Use OS screen auto-rotate:", settings.MobileEnableOSAutorotation);
			DrawSetting("Auto-rotate to portrait:", settings.MobileEnableAutorotateToPortrait);
			DrawSetting("Auto-rotate to reverse portrait:", settings.MobileEnableAutorotateToReversePortrait);
			DrawSetting("Auto-rotate to landscape left:", settings.MobileEnableAutorotateToLandscapeLeft);
			DrawSetting("Auto-rotate to landscape right:", settings.MobileEnableAutorotateToLandscapeRight);
			GUILayout.Space(SETTINGS_GROUP_MINOR_SPACING);
		}
		else if (IsShowingStandaloneSettings)
		{
			string standaloneScreenSize = settings.StandaloneDefaultScreenWidth + " x " + settings.StandaloneDefaultScreenHeight;
			DrawSetting("Default screen size:", standaloneScreenSize);
			DrawSetting("Resolution dialog:", settings.StandaloneResolutionDialogSettingUsed);
			DrawSetting("Full-screen by default:", settings.StandaloneFullScreenByDefault);
			DrawSetting("Resizable window:", settings.StandaloneEnableResizableWindow);

			if (IsShowingWindowsDesktopSettings)
			{
#if UNITY_5_2_AND_LESS
				// not needed in Unity 5.3 since settings.GraphicsAPIsUsed shows better information
				DrawSetting("Use Direct3D11 if available:", settings.WinUseDirect3D11IfAvailable);
#endif
				DrawSetting("Direct3D9 Fullscreen Mode:", settings.WinDirect3D9FullscreenModeUsed);
#if UNITY_5
				DrawSetting("Direct3D11 Fullscreen Mode:", settings.WinDirect3D11FullscreenModeUsed);
#endif
				DrawSetting("Visible in background (for Fullscreen Windowed mode):", settings.VisibleInBackground);
			}
			else if (IsShowingMacSettings)
			{
				DrawSetting("Fullscreen mode:", settings.MacFullscreenModeUsed);
				GUILayout.Space(SETTINGS_GROUP_MINOR_SPACING);
			}

			DrawSetting("Allow OS switching between full-screen and window mode:", settings.StandaloneAllowFullScreenSwitch);
			DrawSetting("Darken secondary monitors on full-screen:", settings.StandaloneCaptureSingleScreen);
			DrawSetting("Force single instance:", settings.StandaloneForceSingleInstance);

			DrawSetting("Stereoscopic Rendering:", settings.StandaloneUseStereoscopic3d);
			DrawSetting("Supported aspect ratios:", settings.AspectRatiosAllowed);
			GUILayout.Space(SETTINGS_GROUP_MINOR_SPACING);
		}

		if (IsShowingWebPlayerSettings)
		{
			string webScreenSize = settings.WebPlayerDefaultScreenWidth + " x " + settings.WebPlayerDefaultScreenHeight;
			DrawSetting("Screen size:", webScreenSize);
			GUILayout.Space(SETTINGS_GROUP_MINOR_SPACING);
		}
		else if (IsShowingWebGlSettings)
		{
			string webScreenSize = settings.WebPlayerDefaultScreenWidth + " x " + settings.WebPlayerDefaultScreenHeight;
			DrawSetting("Screen size:", webScreenSize);
			GUILayout.Space(SETTINGS_GROUP_MINOR_SPACING);
		}
		else if (IsShowingiOSSettings)
		{
#if !UNITY_5_3
			// Unity 5.3 has a Screen.resolutions but I don't know which of those in the array would be the iOS target resolution
			DrawSetting("Target resolution:", settings.iOSTargetResolution);
#endif
#if UNITY_5_1_AND_LESSER
			// not used in Unity 5.2 since settings.GraphicsAPIsUsed shows better information
			DrawSetting("Target graphics:", settings.iOSTargetGraphics);
#endif

			DrawSetting("App icon pre-rendered:", settings.iOSIsIconPrerendered);
			GUILayout.Space(SETTINGS_GROUP_MINOR_SPACING);
		}
		else if (IsShowingAndroidSettings)
		{
			if (UnityMajorVersionUsedIsAtMost(4, buildReportToDisplay.UnityVersion))
			{
			DrawSetting("Use 24-bit depth buffer:", settings.AndroidUse24BitDepthBuffer);
		}
			if (UnityMajorVersionUsedIsAtLeast(5, buildReportToDisplay.UnityVersion))
			{
				DrawSetting("Disable depth and stencil buffers:", settings.AndroidDisableDepthAndStencilBuffers);
			}
			GUILayout.Space(SETTINGS_GROUP_MINOR_SPACING);
		}
	}
	
	void DrawPathSettings(BuildInfo buildReportToDisplay, UnityBuildSettings settings)
	{
		DrawSettingsGroupTitle("Paths");

		DrawSetting("Unity path:", buildReportToDisplay.EditorAppContentsPath);
		DrawSetting("Project path:", buildReportToDisplay.ProjectAssetsPath);
		DrawSetting("Build path:", buildReportToDisplay.BuildFilePath);
	}
	
	
	public override void DrawGUI(Rect position, BuildInfo buildReportToDisplay)
	{
		BuildSettingCategory b = ReportGenerator.GetBuildSettingCategoryFromBuildValues(buildReportToDisplay);
		_buildTargetOfReport = UnityBuildSettingsUtility.GetReadableBuildSettingCategory(b);

		UnityBuildSettings settings = buildReportToDisplay.UnityBuildSettings;

		if (settings == null)
		{
			Utility.DrawCentralMessage(position, "No \"Project Settings\" recorded in this build report.");
			return;
		}

		// ----------------------------------------------------------
		// top bar

		GUILayout.Space(1);
		GUILayout.BeginHorizontal();

			GUILayout.Label(" ", BuildReportTool.Window.Settings.TOP_BAR_BG_STYLE_NAME);

			GUILayout.Space(8);
			GUILayout.Label("Build Target: ", BuildReportTool.Window.Settings.TOP_BAR_LABEL_STYLE_NAME);

			InitializeDropdownBoxLabelsIfNeeded();
			_selectedSettingsIdxFromDropdownBox = EditorGUILayout.Popup(_selectedSettingsIdxFromDropdownBox, _settingDropdownBoxLabels, BuildReportTool.Window.Settings.FILE_FILTER_POPUP_STYLE_NAME);
			GUILayout.Space(15);

			GUILayout.Label("Note: Project was built in " + _buildTargetOfReport + " target", BuildReportTool.Window.Settings.TOP_BAR_LABEL_STYLE_NAME);

			GUILayout.FlexibleSpace();

			_settingsShown = UnityBuildSettingsUtility.GetSettingsCategoryFromIdx(_selectedSettingsIdxFromDropdownBox);

		GUILayout.EndHorizontal();

		// ----------------------------------------------------------

		_scrollPos = GUILayout.BeginScrollView(_scrollPos);

		GUILayout.BeginHorizontal();

		GUILayout.Space(10);
		GUILayout.BeginVertical();


		GUILayout.Space(10);



		// =================================================================
		DrawProjectSettings(buildReportToDisplay, settings);
		GUILayout.Space(SETTINGS_GROUP_SPACING);

		
		// =================================================================
		DrawPathSettings(buildReportToDisplay, settings);
		GUILayout.Space(SETTINGS_GROUP_SPACING);

		
		// =================================================================
		DrawBuildSettings(buildReportToDisplay, settings);
		GUILayout.Space(SETTINGS_GROUP_SPACING);

		
		// =================================================================
		DrawRuntimeSettings(buildReportToDisplay, settings);


		// --------------------------------------------------
		// security settings
		if (IsShowingMacSettings)
		{
			DrawSetting("Use App Store validation:", settings.MacUseAppStoreValidation);
		}
		else if (IsShowingAndroidSettings)
		{
			DrawSetting("Use license verification:", settings.AndroidUseLicenseVerification);
		}


		GUILayout.Space(SETTINGS_GROUP_SPACING);


		// =================================================================
		DrawDebugSettings(buildReportToDisplay, settings);
		GUILayout.Space(SETTINGS_GROUP_SPACING);


		// =================================================================
		DrawCodeSettings(buildReportToDisplay, settings);
		GUILayout.Space(SETTINGS_GROUP_SPACING);


		// =================================================================
		DrawGraphicsSettings(buildReportToDisplay, settings);
		GUILayout.Space(SETTINGS_GROUP_SPACING);


		GUILayout.Space(10);
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		GUILayout.EndScrollView();
	}
}

}



public static class ScriptReference
{

	//
	//   Example usage:
	//
	//   if (GUILayout.Button("doc"))
	//   {
	//      ScriptReference.GoTo("EditorUserBuildSettings.development");
	//   }
	//
	public static void GoTo(string pageName)
	{
		string pageUrl = "file:///";

		pageUrl += EditorApplication.applicationContentsPath;

		// unity 3
		pageUrl += "/Documentation/Documentation/ScriptReference/";

		pageUrl += pageName.Replace(".", "-");
		pageUrl += ".html";

		Debug.Log("going to: " + pageUrl);

		Application.OpenURL(pageUrl);
	}
}
