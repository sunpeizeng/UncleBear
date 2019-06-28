
namespace BuildReportTool
{

[System.Serializable]
public class UnityBuildSettings
{
	public string CompanyName; // PlayerSettings.companyName
	public string ProductName; // PlayerSettings.productName

	public bool UsingAdvancedLicense; // PlayerSettings.advancedLicense / Application.HasProLicense() ?





	// debug settings
	// ---------------------------------------------------------------

	public bool EnableDevelopmentBuild; // EditorUserBuildSettings.development / Debug.isDebugBuild
	public bool EnableDebugLog; // PlayerSettings.usePlayerLog
	public bool EnableSourceDebugging; // EditorUserBuildSettings.allowDebugging
	public bool EnableExplicitNullChecks; // EditorUserBuildSettings.explicitNullChecks
	public bool EnableCrashReportApi; // PlayerSettings.enableCrashReportAPI
	public bool EnableInternalProfiler; // PlayerSettings.enableInternalProfiler
	public bool ConnectProfiler; // EditorUserBuildSettings.connectProfiler

	public string ActionOnDotNetUnhandledException; // Unity 5: PlayerSettings.actionOnDotNetUnhandledException

	public bool ForceOptimizeScriptCompilation; // Unity 5.2.2: EditorUserBuildSettings.forceOptimizeScriptCompilation
	


	// build settings
	// ---------------------------------------------------------------

	public bool EnableHeadlessMode; // EditorUserBuildSettings.enableHeadlessMode
	public bool InstallInBuildFolder; // EditorUserBuildSettings.installInBuildFolder
	public bool ForceInstallation; // EditorUserBuildSettings.forceInstallation
	public bool BuildScriptsOnly; // EditorUserBuildSettings.buildScriptsOnly
	public bool StripPhysicsCode; // in Unity 4: PlayerSettings.stripPhysics. Removed in Unity 5
	public bool BakeCollisionMeshes; // Unity 5: PlayerSettings.bakeCollisionMeshes
	public bool StripUnusedMeshComponents; // PlayerSettings.stripUnusedMeshComponents
	public bool StripEngineCode; // PlayerSettings.stripEngineCode



	// code settings
	// ---------------------------------------------------------------

	public string[] CompileDefines; // EditorUserBuildSettings.activeScriptCompilationDefines

	public string StrippingLevelUsed; // PlayerSettings.strippingLevel

	public string NETApiCompatibilityLevel; // PlayerSettings.apiCompatibilityLevel

	public string AOTOptions; // PlayerSettings.aotOptions

	public string LocationUsageDescription; // PlayerSettings.locationUsageDescription





	// rendering settings
	// ---------------------------------------------------------------

	public string ColorSpaceUsed; // PlayerSettings.colorSpace

	public bool UseMultithreadedRendering; // PlayerSettings.MTRendering

	// in Unity 3: only xbox 360 has this with PlayerSettings.xboxSkinOnGPU
	// in Unity 4, this is PlayerSettings.gpuSkinning
	public bool UseGPUSkinning;

	public string RenderingPathUsed; // Unity 4: PlayerSettings.renderingPath

	public bool VisibleInBackground; // PlayerSettings.visibleInBackground

	public string[] AspectRatiosAllowed; // PlayerSettings.HasAspectRatio

	public string[] GraphicsAPIsUsed; // Unity 5.3: PlayerSettings.GetGraphicsAPIs

	public bool EnableVirtualRealitySupport; // PlayerSettings.virtualRealitySupported



	// web player settings
	// ---------------------------------------------------------------

	public int WebPlayerDefaultScreenWidth; // PlayerSettings.defaultWebScreenWidth
	public int WebPlayerDefaultScreenHeight; // PlayerSettings.defaultWebScreenHeight

	public bool WebPlayerEnableStreaming; // EditorUserBuildSettings.webPlayerStreamed
	public bool WebPlayerDeployOffline; // EditorUserBuildSettings.webPlayerOfflineDeployment

	public int WebPlayerFirstStreamedLevelWithResources; // PlayerSettings.firstStreamedLevelWithResources. Removed in Unity 5.3



	// WebGL settings
	// ---------------------------------------------------------------
	public string WebGLOptimizationLevel; // EditorUserBuildSettings.webGLOptimizationLevel



	// flash player settings
	// ---------------------------------------------------------------
	// Unity 5: Flash support is dropped

	//public string FlashBuildSubtarget; // EditorUserBuildSettings.flashBuildSubtarget




	// shared by web and desktop
	// ---------------------------------------------------------------
	public bool RunInBackground; // PlayerSettings.runInBackground




	// desktop (windows/mac/linux) build settings
	// ---------------------------------------------------------------

	public string StandaloneResolutionDialogSettingUsed; // PlayerSettings.displayResolutionDialog

	public int StandaloneDefaultScreenWidth; // PlayerSettings.defaultScreenWidth
	public int StandaloneDefaultScreenHeight; // PlayerSettings.defaultScreenHeight

	public bool StandaloneFullScreenByDefault; // PlayerSettings.defaultIsFullScreen
	public bool StandaloneAllowFullScreenSwitch; // PlayerSettings.allowFullscreenSwitch. Unity 5.3

	public bool StandaloneCaptureSingleScreen; // PlayerSettings.captureSingleScreen

	public bool StandaloneForceSingleInstance; // Unity 4: PlayerSettings.forceSingleInstance
	public bool StandaloneEnableResizableWindow; // Unity 4: PlayerSettings.resizableWindow


	public bool StandaloneUseStereoscopic3d; // PlayerSettings.stereoscopic3D


	// windows only build settings
	// ---------------------------------------------------------------

	public bool WinUseDirect3D11IfAvailable; // Unity 4: PlayerSettings.useDirect3D11. Removed in Unity 5.3 in favor of PlayerSettings.GetGraphicsAPIs

	public string WinDirect3D9FullscreenModeUsed; // PlayerSettings.d3d9FullscreenMode
	public string WinDirect3D11FullscreenModeUsed; // PlayerSettings.d3d11FullscreenMode


	// mac only build settings
	// ---------------------------------------------------------------

	public bool MacUseAppStoreValidation; // PlayerSettings.useMacAppStoreValidation
	public string MacFullscreenModeUsed; // PlayerSettings.macFullscreenMode



	// Windows Store App only build settings
	// ---------------------------------------------------------------
	
	public bool WSAGenerateReferenceProjects; // EditorUserBuildSettings.metroGenerateReferenceProjects

	public string WSASDK; // EditorUserBuildSettings.wsaSDK





	// Mobile build settings
	// ---------------------------------------------------------------

	public string MobileBundleIdentifier; // PlayerSettings.bundleIdentifier ("Bundle Identifier" in iOS, "Package Identifier" in Android)
	public string MobileBundleVersion; // PlayerSettings.bundleVersion ("Bundle Version" in iOS, "Version Name" in Android)
	public bool MobileHideStatusBar; // PlayerSettings.statusBarHidden

	public int MobileAccelerometerFrequency; // PlayerSettings.accelerometerFrequency

	public string MobileDefaultOrientationUsed; // PlayerSettings.defaultInterfaceOrientation
	public bool MobileEnableAutorotateToPortrait; // PlayerSettings.allowedAutorotateToPortrait
	public bool MobileEnableAutorotateToReversePortrait; // PlayerSettings.allowedAutorotateToPortraitUpsideDown
	public bool MobileEnableAutorotateToLandscapeLeft; // PlayerSettings.allowedAutorotateToLandscapeLeft
	public bool MobileEnableAutorotateToLandscapeRight; // PlayerSettings.allowedAutorotateToLandscapeRight
	public bool MobileEnableOSAutorotation; // PlayerSettings.useOSAutorotation

	public bool Use32BitDisplayBuffer; // PlayerSettings.use32BitDisplayBuffer



	// iOS only build settings
	// ---------------------------------------------------------------

	// Unity 5: EditorUserBuildSettings.appendProject is removed
	public bool iOSAppendedToProject; // EditorUserBuildSettings.appendProject
	public bool iOSSymlinkLibraries; // EditorUserBuildSettings.symlinkLibraries

	public string iOSAppDisplayName; // PlayerSettings.iOS.applicationDisplayName

	public string iOSScriptCallOptimizationUsed; // PlayerSettings.iOS.scriptCallOptimization

	public string iOSSDKVersionUsed; // PlayerSettings.iOS.sdkVersion
	public string iOSTargetOSVersion; // PlayerSettings.iOS.targetOSVersion

	public string iOSTargetDevice; // PlayerSettings.iOS.targetDevice
	public string iOSTargetResolution; // PlayerSettings.iOS.targetResolution. Removed in Unity 5.3

	public bool iOSIsIconPrerendered; // PlayerSettings.iOS.prerenderedIcon

	public string iOSRequiresPersistentWiFi; // PlayerSettings.iOS.requiresPersistentWiFi

	public string iOSStatusBarStyle; // PlayerSettings.iOS.statusBarStyle



	// Unity 5: PlayerSettings.iOS.exitOnSuspend is replaced with PlayerSettings.iOS.appInBackgroundBehavior
	public bool iOSExitOnSuspend; // PlayerSettings.iOS.exitOnSuspend
	public string iOSAppInBackgroundBehavior; // PlayerSettings.iOS.appInBackgroundBehavior (undocumented as of Unity 5.0.0f4)


	public bool iOSLogObjCUncaughtExceptions; // PlayerSettings.logObjCUncaughtExceptions


	public string iOSShowProgressBarInLoadingScreen; // PlayerSettings.iOS.showActivityIndicatorOnLoading

	public string iOSTargetGraphics; // PlayerSettings.targetIOSGraphics. Removed in Unity 5.3
	
	
	

	// Android only build settings
	// ---------------------------------------------------------------

	public string AndroidBuildSubtarget; // EditorUserBuildSettings.androidBuildSubtarget

	public bool AndroidUseAPKExpansionFiles; // PlayerSettings.Android.useAPKExpansionFiles

	public bool AndroidAsAndroidProject; // EditorUserBuildSettings.exportAsGoogleAndroidProject

	public bool AndroidUseLicenseVerification; // PlayerSettings.Android.licenseVerification

	
	public bool AndroidIsGame; // Unity 5: PlayerSettings.Android.androidIsGame
	public bool AndroidTvCompatible; // Unity 5: PlayerSettings.Android.androidTVCompatibility
	
	

	// Unity 5: PlayerSettings.Android.use24BitDepthBuffer is replaced with PlayerSettings.Android.disableDepthAndStencilBuffers
	public bool AndroidUse24BitDepthBuffer; // PlayerSettings.Android.use24BitDepthBuffer
	public bool AndroidDisableDepthAndStencilBuffers; // PlayerSettings.Android.disableDepthAndStencilBuffers

	

	public int AndroidVersionCode; // PlayerSettings.Android.bundleVersionCode

	public string AndroidMinSDKVersion; // PlayerSettings.Android.minSdkVersion
	public string AndroidTargetDevice; // PlayerSettings.Android.targetDevice

	public string AndroidSplashScreenScaleMode; // PlayerSettings.Android.splashScreenScale

	public string AndroidPreferredInstallLocation; // PlayerSettings.Android.preferredInstallLocation

	public bool AndroidForceInternetPermission; // PlayerSettings.Android.forceInternetPermission
	public bool AndroidForceSDCardPermission; // PlayerSettings.Android.forceSDCardPermission

	public string AndroidShowProgressBarInLoadingScreen; // PlayerSettings.Android.showActivityIndicatorOnLoading

	public string AndroidKeyAliasName; // PlayerSettings.Android.keyaliasName
	public string AndroidKeystoreName; // PlayerSettings.Android.keystoreName

	// Derived Values
	// ---------------------------------------------------------------


	public bool HasValues
	{
		get
		{
			return !string.IsNullOrEmpty(CompanyName) && !string.IsNullOrEmpty(NETApiCompatibilityLevel);
		}
	}
}

}
