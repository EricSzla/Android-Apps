/**
 * @desc 	CameraCaptureController Contains a set of functioality to handle camera functionality and capturing Still images
 * 			used as textures or saved or uploaded to a server as PNG's.
 * 
 * @author 	Tastybits
 * @link 	www.tastybits.io
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


namespace Tastybits.CamCap {


	public class CameraCaptureController : MonoBehaviour {
		public static CameraCaptureController instance;
		public bool verbose = false;
		static bool firstTime = true;
		public bool EnableAutoFocusOnAndroid = true;

		// Contains the last ISO value set.
		string _LastISOValueSet = "";

		// contains the buffered value of wether the flash is available on the running device.
		static bool? _isFlashAvailable=null;

		// continas the buffered set of supported ISO values.
		static string[] supportedISOValues = null;

		// contains the buffered value of wether the Exposure compensation can be set on the running device.
		static bool? _IsExposureCompensationAvailable = null; 

		// Enum used to desctibe which behaviour that the CameraCaptureController showld
		// try and do when the captured photo is diffirent than the screen.
		public enum PreviewScaleMode{
			EnvelopePreviewToFitParent = 0,
			DontModifyPreview = 1,
			iPhone_PortraitMode_ScaleAndChopCamFeedToFitFullscreen = 2, // scales the raw texture to fit the screen and the left/right might be chopped.
			iPhone_PortraitMode_DisplayCameraFeedAsIs = 3, // scales the RawTexture to fit the screen and makes sure nothing is chopped.
		}

		// Enum with the supported FPS values of the Preview view.
		public enum PreviewFPS {
			FPS15 = 15,
			FPS30 = 30,
			FPS60 = 60
		}

		/**
		 * Returns if we have verbose set in the system.
		 */
		public static bool Verbose {
			get {
				if (instance == null)
					return true;
				return instance.verbose;
			}
		}

		// Buffered properties for the current running device.
		CameraProperty[] _properties = null;

		// Enumerates and returns the properties of the current running camera device.
		public CameraProperty[] CameraProperties {
			get {
				CheckRunning();
				if( _properties == null || _properties.Length == 0 ) {
					if( _properties == null ) {
						_properties = new CameraProperty[]{};
					}
					_properties = CameraCapture.GetAvailableProperties( this.webCamTexture );
				}
				return _properties;
			}
		}

		// Describes the state of the Camera. 
		public enum CameraState {
			None = 0,
			StartingUp = 1,
			ShuttingDown = 2,
			Running,
			Error
		}

		// Tells the Controller how to scale the preview.
		[UnityEngine.SerializeField]
		PreviewScaleMode previewScaleMode = PreviewScaleMode.EnvelopePreviewToFitParent; // 

		//[UnityEngine.SerializeField]
		//PreviewScaleMode rotateView = PreviewRotate.EnvelopePreviewToFitParent; // 

		//public CanvasRenderer cameraFeedContainer;
		[UnityEngine.SerializeField]
		CameraCaptureView previewView;

		// FPS
		public PreviewFPS DefaultPreviewFPS_IOS = PreviewFPS.FPS60;

		// FPS
		public PreviewFPS DefaultPreviewFPS_Android = PreviewFPS.FPS30;

		// this is per default true, it means that you can
		// adjust the FPS on the devices base on it.
		public bool TurnDownPreviewFPSOnLowRezDevices = true;

		// This meathod is made for those who wants to quickly hook up some existing
		// code with CameraCaptureController and integrate it for a quick test.
		// essentially its possible.
		// It replaces the RawImage given with a Preview (CameraCaptureView) and
		// returns a new instance of a CameraCaptureController which you can use
		// to start rendering to the preview and then take a snapshot.
		public static CameraCaptureController Create( RawImage previewRenderer ) {
			Debug.Log ("Replacing RawImage with CameraCaptureView for preview");

			var tex = previewRenderer.texture;
			var maskable = previewRenderer.maskable;
			var color = previewRenderer.color;
			var mat = previewRenderer.material;
			var ray = previewRenderer.raycastTarget;
			var enabled = previewRenderer.enabled;
			var tag = previewRenderer.tag;
			var name = previewRenderer.name;
			var hideFlags = previewRenderer.hideFlags;

			GameObject go = previewRenderer .gameObject;

			Component.DestroyImmediate( previewRenderer );

			var ret = go.AddComponent<CameraCaptureView>();

			ret.texture = tex;
			ret.maskable = maskable;
			ret.color = color;
			ret.material = mat;
			ret.raycastTarget = ray;
			ret.enabled = enabled;
			ret.tag = tag;
			ret.name = name;
			ret.hideFlags = hideFlags;

			return Create (go);
		}


		public static  CameraCaptureController Create( GameObject go, bool createCameraViewIfNotExists=false ) {
			if( go.GetComponent<CameraCaptureView>()==null && createCameraViewIfNotExists==false ) {
				throw new System.Exception("Error CameraCaptureView is null on gameObject: " + go.name );
			}
			else if( go.GetComponent<CameraCaptureView>() == null && createCameraViewIfNotExists ) {
				go.AddComponent<CameraCaptureView>();
			}
			var ret = go.GetComponent<CameraCaptureController>();
			if( ret == null ) {
				ret = go.AddComponent<CameraCaptureController>();
			}

			ret.previewView = ret.GetComponent<CameraCaptureView>();

			return ret;
		}

		// This applies only to android and describes the default focus mode.
		public AndroidFocusModes PreferAndroidFocusMode = AndroidFocusModes.FOCUS_MODE_AUTO;

		/**
		 * this can be set to make the Controller autogenerate the preview view when 
		 * none is setup.
		 */
		[HideInInspector]
		public bool AutoCreateUnityUIPreview = false;

		/**
		 * Property uused to get the Preview view.
		 */
		public CameraCaptureView view {
			get {
				if( previewView!=null ) return previewView;
				previewView = this.GetComponent<CameraCaptureView>();
				if( previewView == null && AutoCreateUnityUIPreview ) {
					Debug.Log("No camera view exists on CameraCapture, creating one now.");
					previewView = this.gameObject.AddComponent<CameraCaptureView>();
				}
				return previewView;
			}
		}

		// referance to the webCamTexture, serialized since we want to be able to recompile the app.
		[HideInInspector]
		[UnityEngine.SerializeField]
		WebCamTexture webCamTexture;

		/**
		 * Returns the webCamTexture.
		 */
		public WebCamTexture GetWebCamTexture() {
			return webCamTexture;
		}


		// referance to the WebCamTexture.
		public WebCamTexture WebCamTexture {
			get {
				return this.webCamTexture;
			}
		}

		// the index of hte current device if the phone/tablet has a front and a backfacing camera.
		int currDeviceIndex=-1;

		// internally used to get the camera rotation angle even if there is no instance of 
		// webcamtexture available.
		int webCamTextureRotationAngle {
			get {
				if( webCamTexture == null ) return 0;
				return webCamTexture.videoRotationAngle;
			}
		}


		/**
		 * Enum used to describe the current facing direction of the camera.
		 */
		public enum CameraFacingDirection {
			None = -1,
			BackFacing = 0,
			FrontFacing = 1
		}


		/**
		 * enum used to describe the prefered orientation.
		 */ 
		public enum PreferCameraDirection {
			BackFacing = 0, // Normal camera..., Camera you use if you want to tkae a photo of someone else.
			FrontFacing = 1 // Selfie camera..., Camera facing you...
		}


		/**
		 * Enum used to described the prefered torch / flash mode.
		 */
		public enum PreferFlashAndTorchMode {
			NotSet = -1,
			FlashOff = 0,
			FlashOn = 1,
			FlashAuto = 2,
			TorchOn = 3
		}
				
		// prefered initial camera orientation.
		[UnityEngine.SerializeField]
		PreferCameraDirection PreferedCameraDirection = PreferCameraDirection.FrontFacing;

		// prefered flash and torhmodes.
		[UnityEngine.SerializeField]
		PreferFlashAndTorchMode PreferedFlashAndTorch = PreferFlashAndTorchMode.NotSet;

		/**
		 * Sets the initial prefered camera orientation.
		 */ 
		public void SetPreferedCameraDirection( PreferCameraDirection mode ) {
			PreferedCameraDirection = mode;
		}


		/**
		 * Set Statusbar hidden; This is used to show the statusbar, as in some apps the 
		 * statusbar should be shown when the camera is visible.
		 */
		public void SetStatusBarHidden (bool statusBarHidden) {
			CameraCapture.SetStatusBarHidden( statusBarHidden );
		}

		// 
		[HideInInspector]
		public float AutofocusMoveTreshold = 0.125f;

		// use this to shutdown and return the texture.
		public void CaptureStillImageAndShutdownCamera( System.Action<bool,Texture2D > callback ) {
			CaptureStillImage( ( bool ok, StillImageResult result )=>{
				if( !ok ) {
					Debug.LogError("Failed to capture still image from camera");
					callback(false, null);
				} else {
					GetStillImageAndCleanup( ( Texture2D tex ) =>{
						if( tex==null ) {
							callback(false,null);
						}else{
							callback(true,tex);
						}
					}); 
				}
			} );
		}

		// use this if you just want to get an "ok" or not. in case you might want to add the "retake photo" option.
		public void CaptureStillImage( System.Action<bool> callback ) {
			CaptureStillImage( ( bool ok, StillImageResult result )=>{
				var tx = this.PreviewTexture;
				if( CameraCaptureController.Verbose ) 
					Debug.Log( "CameraCaptureKit: CaptureStillImage - returned still image, ok="+ok + " width,height = " + tx.width + "," + tx.height );
				if( previewView != null ) {
					previewView.texture = tx;
					UpdatePreviewLayout2( 0, false, tx.width, tx.height );
				}
				callback(ok);
			} );
		}


		// Capturing still image and return a still image result.
		public void CaptureStillImage( System.Action<bool,StillImageResult> callback ) {
			if(verbose)Debug.Log( "CaptureStillImage - returned");

			bool imageGottenWithSuccess = false;
			var stillResult = new StillImageResult();
			//stillResult.GetType().GetField( "ccc", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic ).SetValue(stillResult,this);
			stillResult.ccc = this;
			if(verbose)Debug.Log( "CaptureStillImage - returned");


			if(verbose)Debug.Log( "CaptureStillImage.....");
			if( Application.platform == RuntimePlatform.Android ){
#if UNITY_ANDROID && !UNITY_EDITOR
				CameraCapture.UnityCamera_SetBestPictureSize( Screen.width, Screen.height );
#endif
			}
			CameraCapture.CaptureStillImage( webCamTexture, this.captureMode, ( bool ok )=>{
				if(verbose) Debug.Log( "CaptureStillImage returned......." );

				if( webCamTexture==null) {
					Debug.LogError("webCamTexture==null");
				}

				if( callback==null ) {
					Debug.LogError("Error callback for returning still image is null");
				}

				if( !ok ) {
					Debug.LogError("CameraCaptureKit: Error capturing Still Image");
				}

				webCamTexture.Pause(); // pause it now... we got a still image...
				//capturingStill = false;
				imageGottenWithSuccess = ok;

				if( ok ) {
					if( Application.platform == RuntimePlatform.Android ) {
						overwritePreviewTexture = CameraCapture.GetAndroidStillImageTaken();
						Debug.LogError( "CameraCaptureKit: overwriting preview texture with texture : " + PreviewTexture.width  + "," + PreviewTexture.height + " aspect=" + CameraDeviceAspectRatio );
						UpdatePreviewLayout2( 0, false, overwritePreviewTexture.width, overwritePreviewTexture.height );
					}
				}

				callback(imageGottenWithSuccess,stillResult);
			} );
				
		}


		public void ResumePreview(){
			webCamTexture.Play();
		}


		public void PausePreview() {
			webCamTexture.Pause();
		}


		public void GetStillImageAndCleanup( System.Action<Texture2D> cb ) {
			//if(verbose)Debug.LogError("GetStillImageAndCleanup");
			GetStillImage( cb, ActionAfterGetStill.Cleanup );
		}


		// returns the still image without cleaning up.
		public void GetStillImage( System.Action<Texture2D> cb, ActionAfterGetStill action  ) {
			//Debug.LogError("GetStillImageAndCleanup");
			if( webCamTexture == null ) {
				Debug.LogError("Failed to get StillImage. (WebCamTexture is null)");
				return;
			}

			Texture2D	tex = ConvertWebCamTextureToFinalTexture( );
			if(verbose)Debug.Log( "After Convert WebCamTextureToFinalTexture" );
			useTorch = false;

			if( action == ActionAfterGetStill.Cleanup ) {
				if( webCamTexture != null ) {
					webCamTexture.Stop();
				} else {
					Debug.LogError("Webcam texture is null this was not supposed to happen" );
				}
			}

			cb( tex );

			if( action == ActionAfterGetStill.Cleanup ) {
				ReleaseWebCamTexture();
			}
			//CameraCapture.BreakPoint("what is memory now after release and every thing?");
		}


		//public UIRoot root;
		[UnityEngine.HideInInspector] [UnityEngine.SerializeField]
		bool hasDevice = false;
		int activeDeviceIndex {
			get {
				if( currDeviceIndex == -1 ) { // selct front facing... first..
					currDeviceIndex = 0;
					if( PreferedCameraDirection == PreferCameraDirection.FrontFacing ) {
						for( int i = 0; i<WebCamTexture.devices.Length; i++ ) {
							if( WebCamTexture.devices[i].isFrontFacing ) {
								currDeviceIndex = i;
								break;
							}
						}
					} 
					else if( PreferedCameraDirection == PreferCameraDirection.BackFacing ) {
						for( int i = 0; i<WebCamTexture.devices.Length; i++ ) {
							if( !WebCamTexture.devices[i].isFrontFacing ) {
								currDeviceIndex = i;
								break;
							}
						}
					} 
				}
				return currDeviceIndex;
			}
			set {
				currDeviceIndex = value;
			}
		}

		[UnityEngine.SerializeField] [HideInInspector]
		private CameraState cameraState;

		// replace these with the camera state...
		bool shuttingDownWebCam = false;
		bool startingCamera = false;
		bool webCamRunning= false;
		//bool _autofocus = false;

		[HideInInspector]
		public int camwidth = 0;

		[HideInInspector]
		public int camheight = 0;

		// Per request we have added this to be able to inject a webCamTexture.
		// the idea is that you can injet the webCamTexture when starting the capture session
		// letting you use/reuse a webcamtexture for your purose.
		[Tooltip("This is used to inject a WebCamTexture referance to use for capturing - you can inject your upon creation of the stream")]
		public WebCamTexture injectWebCamTexture;

		// Constructor.
		protected CameraCaptureController():base() {
			instance = this;
		}

		// Depricated: Use CurrentCameraFacingDirection instead.
		public bool IsCurrentFrontFacing() {
			return WebCamTexture.devices[activeDeviceIndex].isFrontFacing;
		}

		// Depricated: Use CurrentCameraFacingDirection instead.
		public bool IsCurrentBackFacing(){
			return !WebCamTexture.devices[activeDeviceIndex].isFrontFacing;
		}

		// Returns the direction which the current running device is facing.
		public CameraFacingDirection CurrentCameraFacingDirection {
			get {
				if( activeDeviceIndex >= 0 && activeDeviceIndex <= WebCamTexture.devices.Length -1 && WebCamTexture.devices.Length> 0 ) {
					if( WebCamTexture.devices[activeDeviceIndex].isFrontFacing ) {
						return CameraFacingDirection.FrontFacing;
					}
					return CameraFacingDirection.BackFacing;
				}
				return CameraFacingDirection.None;
			}
		}

		// Contains the number of Camerea devices available on the Mobile device.
		public static bool HasCameraDevices {
			get {
				return WebCamTexture.devices.Length > 0;
			}
		}


		void Awake() {
			instance = this;
			if( Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android ){
				verbose = true; // turning on verbose on the iPhone player.
			}
		}


		void OnDisable() {
			if( webCamTexture != null ) {
				webCamTexture.Stop();	
				ReleaseWebCamTexture();
			}	
		}


		void Start() {
		}

		// Main method to get the camera up and running.
		public void StartCamera( System.Action<bool> onDone = null ) {
			OnStartingCameraDone = onDone;
			StartCameraInternal();
		}

		// Alternative version of StartCamera in which you can give it some parameters to let it start with another faceing
		// direction or with the torch turned on/off, or autoflash turned on as default.
		public void StartCamera( PreferCameraDirection startFaceDirection, PreferFlashAndTorchMode flashAndTorchMode, System.Action<bool> onDone = null ) {
			this.PreferedCameraDirection = startFaceDirection;
			this.PreferedFlashAndTorch = flashAndTorchMode;
			StartCamera( onDone );
		}

		// After a still image has been taken this method is run to Destoy the previous image and continue 
		// running the camera to take a new one.
		public void DiscardStillImageAndContinue() {
			if( verbose) Debug.Log("Discard still image and continue");
			if( previewView != null ) {
				previewView.texture = webCamTexture;
			}
			ClearCachedDeviceInfo();
			webCamTexture.Play();
			if( Application.platform == RuntimePlatform.Android ) {
				CameraCapture.Android_DiscardStillImageAndContinue();
			}
		}


		void ReleaseWebCamTexture(){
			webCamTexture = null;
			if(view!=null )
				view.texture = null;
			Resources.UnloadUnusedAssets();
			System.GC.Collect();
		}


		public void StopCamera( System.Action callback=null ) {
			if( callback==null ) {
				callback = ()=>{}; // just set it to something internally.
			}
			StopWebCamImpl( true, callback );
		}


		public int NumDevices {
			get{
				return WebCamTexture.devices.Length;
			}
		}


		public bool CanFlipBetweenMultipleCameras {
			get {
				return NumDevices>1;
			}
		}

		// Returns true if the camera can be flipped around.
		public bool IsFlipAvailable {
			get {
				return NumDevices>1;
			}
		}


		// Return true when the ISO can be adjusted by the camera.
		// Depricated: Use IsISOAvailable instead.
		public bool CanAdjustISO {
			get {
				return IsISOAvailable;
			}
		}

		// Return true when the ISO is availab.e
		public bool IsISOAvailable {
			get{
				if( supportedISOValues == null ) {
					supportedISOValues = webCamTexture.GetSupportedISOValues();
				}
				return supportedISOValues != null && supportedISOValues.Length>1;
			}
		}

		// Returns the supported ISO values.
		public string[] SupportedISOValues {
			get {
				if( supportedISOValues == null ) {
					supportedISOValues = webCamTexture.GetSupportedISOValues();
				}
				return supportedISOValues;
			}
		}

		// Due to a bug in the iOS native integration iOS , the ISO is flucturating 
		// even though its set to a specific value , using this will in turn return the last iso set
		public string LastISOValueSet {
			get {
				if( !IsISOAvailable ) {
					return "";
				}
				if( string.IsNullOrEmpty(_LastISOValueSet) ) {
					_LastISOValueSet = ISOValue;
				}
				return _LastISOValueSet;
			}
		}

		// Instructs the Camera to return the given ISO, NOTE that the behaviour
		// on iOS/Android is abit diffirent when it comes to the ISO - the ISO might flucturate on iOS.
		// If you want to get a non flucturating ISO value use the property LastISOValueSet instead of getting this property.
		public string ISOValue {
			get {
				CheckRunning();
				return webCamTexture.GetISOValue( );
			}
			set {
				CheckRunning();
				_LastISOValueSet = value;
				webCamTexture.SetISOValue( value );
			}
		}

		// Checks if the camera is running and reports an error if not.
		// This is implemented to not type the same thing over and over again.
		bool CheckRunning() {
			if( webCamTexture == null || !webCamRunning ) {
				// since we need the device itself to detemrine if flash is available we need a running camera.
				Debug.LogError("CameraCaptureKit: Camera not running");
				return false;
			}
			return true;
		}

		// this is used to clear the previous device infomtion 
		// cached. Its invoked every time we flip the camera or when we start and restrat the camera.
		void ClearCachedDeviceInfo() {
			_supportedModes = null;
			_properties = null;
			_LastISOValueSet = "";
			supportedISOValues = null;
			_isFlashAvailable = null;
			flashModeSupportedCache = null;
			_IsExposureCompensationAvailable = null;
			isFocusSupported=null;
			overwritePreviewTexture = null;
		}

		// Note: Use IsFlashAvailable instead of HasFlashFunctionality.
		public bool HasFlashFunctionality {
			get {
				return IsFlashAvailable;
			}
		}

		// Returns true when the Flash is available.
		public bool IsFlashAvailable {
			get {
				if( !CheckRunning() ) {
					return false;
				}
				if( _isFlashAvailable == null ) {
					_isFlashAvailable = webCamTexture.HasFlashMode( FlashModes.On ) || webCamTexture.HasFlashMode( FlashModes.FlashModeAuto );
				}
				return _isFlashAvailable.Value;
			}
		}

		// Test if a given FalshMode is available.
		static List<bool?> flashModeSupportedCache = null;
		public bool IsFlashModeSupported( FlashModes flashMode ) {
			if( !CheckRunning() ) {
				return false;
			}
			if( flashModeSupportedCache ==null ) {
				flashModeSupportedCache = new List<bool?>();
			}
			if( flashModeSupportedCache.Count >= (int)flashMode && flashModeSupportedCache[(int)flashMode] != null ) {
				return (bool)flashModeSupportedCache[(int)flashMode];
			}
			bool yes = webCamTexture.HasFlashMode( flashMode );
			while( flashModeSupportedCache.Count < (int)flashMode ) {
				flashModeSupportedCache.Add( null );
			}
			if( flashModeSupportedCache[(int)flashMode] == null ) {
				flashModeSupportedCache[(int)flashMode] = yes;
			}
			return flashModeSupportedCache[(int)flashMode].Value;
		}


		// Depricated:
		// Returns true if the running device has exposure compensation turned on.
		// Same as IsExposureCompensationAvailable
		public bool CanAdjustExposureCompensation {
			get {
				return IsExposureCompensationAvailable;
			}
		}

		// Returns true when the exposure compensation is available.
		public bool IsExposureCompensationAvailable {
			get {
				if( !CheckRunning() ) {
					return false;
				}
				if( _IsExposureCompensationAvailable == null ) {
					_IsExposureCompensationAvailable = webCamTexture.IsAutoExposureLockSupported();
				}
				return _IsExposureCompensationAvailable.Value;
			}
		}


		public void GetExposureCompensationRange( out int min, out int max ) {
			if( !CheckRunning() ) {
				min = 0;
				max = 0;
				return;
			}
			webCamTexture.GetAvailableExposureCompensationRange( out min, out max );
		}


		public void SetExposureCompensation( int value ) {
			if( !CheckRunning() ) {
				return;
			}
			webCamTexture.SetAutoExposureLock( true );
			webCamTexture.SetExposureCompensation( value );
		}


		public void SetExposureCompensationDisabled() {
			if( !CheckRunning() ) {
				return;
			}
			webCamTexture.SetAutoExposureLock( false );
		}


		static bool? isFocusSupported=null;
		public bool IsFocusSupported {
			get {
				if( !CheckRunning() ) {
					return false;
				}
				if( isFocusSupported == null ) {
					isFocusSupported = webCamTexture.IsFocusSupported( );
				}
				return isFocusSupported.Value;
			}
		}


		static List<string> _supportedModes=null;
		public System.Collections.Generic.List<string> SupportedFlashModes {
			get {
				if( _supportedModes==null || _supportedModes.Count == 0 ) {
					var ret = new System.Collections.Generic.List<string>();
					if( !CheckRunning() ) {
						Debug.LogError("CapturaCameraKit: Cannot get supported flash modes - No Camera device running.");
						return ret;
					}
					if( webCamTexture.HasFlashMode( FlashModes.Off ) ) {
						ret.Add( "Off" );
					}
					if( webCamTexture.HasFlashMode( FlashModes.On ) ) {
						ret.Add( "On" );
					}
					if( webCamTexture.HasFlashMode( FlashModes.FlashModeAuto ) ) {
						ret.Add( "Auto" );
					}
					_supportedModes = ret;
					if( verbose ) Debug.Log("CameraCaptureKit: Initialized available FlashModes = " + string.Join(",", _supportedModes.ToArray() ) );
				}
				return _supportedModes;
			}
		}


		int currentFlashModeIndex = -1;
		public string SetNextFlashMode() {
			if( !CheckRunning() ) {
				return "";
			}

			if( SupportedFlashModes.Count == 0 )  {
				Debug.LogError("SetNextFlashMode cannot be done since there is zero modes available.");
				return "off";
			}

			string curmode = SupportedFlashModes[currentFlashModeIndex];
			if( SupportedFlashModes.Count > 1 ) {
				if( ++currentFlashModeIndex > SupportedFlashModes.Count-1 ) { // Cycle the flash modes.
					currentFlashModeIndex = 0;
				}
			}
			var nextmode = SupportedFlashModes[currentFlashModeIndex];

			Debug.Log("Setting next flash mode, currentmode : " + curmode + " nextmode:" + nextmode + " available modes:" + string.Join(",", SupportedFlashModes.ToArray() ) );
			if( nextmode=="On") {
				SetFlashMode( FlashModes.On );
				return "On";
			}
			else if( nextmode=="Off") {
				SetFlashMode( FlashModes.Off );
				return "Off";
			}
			else if( nextmode=="Auto") {
				SetFlashMode( FlashModes.FlashModeAuto );
				return "Auto";
			} else {
				Debug.LogError("Unknown mode : " + nextmode );
			}

			return "off";
		}


		bool switchingCamera = false;
		public void FlipCameraFacing( System.Action callback ) {
			gotAspect = false; // should reset aspect
			
			int deviceCount = WebCamTexture.devices.Length;
			if( deviceCount == 0 ) {
				Debug.LogError("Cannot flip camera. No camera available");
			}
			else if( deviceCount <= 1 ) {
				Debug.LogError("Cannot flip the camera only one camera is available. ( going back to current camera )" );
			}

			if( switchingCamera ) {
				Debug.LogError("Already in the process of switching camera.");
				return;
			}
				

			switchingCamera = true;
			StartCoroutine( StopWebCamImpl( true, ()=>{
				ReleaseWebCamTexture();
				switchingCamera=false;
				if( activeDeviceIndex+1 >= deviceCount ) {
					activeDeviceIndex = 0;
				} else {
					activeDeviceIndex++;
				}
				StartCameraInternal();
				if( callback!=null ) {
					callback ();
				}
			}) );
		}


		void StartCameraInternal( System.Action callback=null ) {
			if( this.gameObject.activeSelf==false ) {
				this.gameObject.SetActive(true);
			}
			ClearCachedDeviceInfo();
			if( firstTime ) {
				if( verbose ) { Debug.Log("StartCameraInternal: First init preferred direction : " + this.PreferedCameraDirection); }
				firstTime = false;
				// pick the front facing camera first.
				bool startWithCameraFacing = this.PreferedCameraDirection == PreferCameraDirection.FrontFacing;
				for( int cameraIndex = 0; cameraIndex < WebCamTexture.devices.Length; cameraIndex++) {
					if (WebCamTexture.devices[cameraIndex].isFrontFacing == startWithCameraFacing) {
						activeDeviceIndex = cameraIndex;
						if(verbose)Debug.Log("Selected active camera device : " + activeDeviceIndex );
						break;
					}
				}
				StartCameraInternal();
				return;
			} else {
				if( verbose ) { Debug.Log("StartCameraInternal"); }
				if( startingCamera ) {
					Debug.LogError("CameraController is already starting a capture session.");
					return;
				}
				this.StartCoroutine( StartWebCamImpl( callback ) );
			}

		}
			

		IEnumerator StartWebCamImpl( System.Action callback=null ) {
			if( verbose ) Debug.Log ( "CameraCapture: - Heating up Web Camera" );
			if( startingCamera ) {
				if(verbose)Debug.Log("WebCamera is already starting");
				OnStartingCameraDone( false );
				yield break;
			}
			if( webCamRunning ) {
				if( verbose ) Debug.LogError("Webcamera is alreay running (boolean set)");
			} 
			hasDevice = WebCamTexture.devices.Length >= 1;
			if( !hasDevice ) {
				Debug.LogError("Warning there is no device available");
				cameraState = CameraState.Error;
				OnStartingCameraDone( false );
				yield break;
			}

			cameraState = CameraState.StartingUp;
			webCamRunning = false;
			startingCamera = true;
			currentFlashModeIndex = -1;
			useTorch = false;

			// if already shutting down the camera.
			while( shuttingDownWebCam ) {
				if(verbose)Debug.Log("Waiting for shutdown of camera");
				yield return new WaitForSeconds( 0.5f );
			}


			bool gettingPermissions = true;
			float maxtime = 0f;
			bool timedout = false;
			CameraPermissions.AVAuthorizationStatus permissions = CameraPermissions.AVAuthorizationStatus.Last;
			CameraPermissions.GetPermissions( ( CameraPermissions.AVAuthorizationStatus status) => {
				permissions = status;
				gettingPermissions=false;
			} );
			while( gettingPermissions && !timedout ) {
				maxtime += Time.deltaTime;
				if (maxtime >= 15f) {
					timedout = true;
				}
				yield return 0;
			}
			Debug.Log( "CameraCapture: Determined camera permissions : " + permissions );
			if( timedout || permissions != CameraPermissions.AVAuthorizationStatus.AVAuthorizationStatusAuthorized ) {
				Debug.LogError ("Failed to get permissions for starting the camera.");
				cameraState = CameraState.Error;
				webCamTexture = null;
				startingCamera = false;
				OnStartingCameraDone(false);
				yield break;
			} 

			
			bool gottenFirstFrame = false;
			try {
				if( webCamTexture==null && hasDevice ) {
					var deviceName = WebCamTexture.devices [activeDeviceIndex].name;
					if(verbose)Debug.Log( "webcam - activating webcam : " + deviceName );


					// Special case for iPhone in portrait mode.
					int fps = (int)this.DefaultPreviewFPS_IOS;
					int max = 1024;
					int wi = 1024;
					int he = 1024;
#if UNITY_IPHONE
					if( Application.platform == RuntimePlatform.IPhonePlayer ) {
					 	max = 2048;
						if( (int)UnityEngine.iOS.Device.generation <= (int)UnityEngine.iOS.DeviceGeneration.iPhone4S ) {
							max = 1024;
							if( TurnDownPreviewFPSOnLowRezDevices ) {
								fps = (int)PreviewFPS.FPS30;
							}
						}
						he = Screen.height;
						if( he > max ) {
							he = max;
						}
						wi = (int)(Camera.main.aspect * he);
						if( wi > max )  {
							wi = max;
						}
						wi = 1536;
						if( (int)UnityEngine.iOS.Device.generation <= (int)UnityEngine.iOS.DeviceGeneration.iPhone4S ) {
							wi /= 2;
							he /= 2;
						}
						wi = 1536;
						he = 2048;
					}
#else 
					max = 2048;
					fps = (int)DefaultPreviewFPS_Android;
					he = Screen.height;

					if( TurnDownPreviewFPSOnLowRezDevices ) {
						float heighest = Screen.height;
						if( Screen.width > Screen.height ) {
							heighest = Screen.width;
						}
						if( heighest <= 800 ) {
							fps = (int)DefaultPreviewFPS_Android / 2;
						}
					}

					// 
					if( he > max ) {
						he = max;
					}
					wi = (int)(Camera.main.aspect * he);
					if( wi > max )  
						wi = max;
#endif
					if(verbose)Debug.Log( "Creating new WebCamTexture " + wi + ","+he + " fps:" + fps );
					if( injectWebCamTexture != null ) {
						webCamTexture = CameraCapture.ReuseWebCamTexture( deviceName, wi, he, fps, injectWebCamTexture );
						wi = webCamTexture.requestedWidth;
						he = webCamTexture.requestedHeight;
					} else {
						webCamTexture = CameraCapture.CreateTexture( deviceName, wi, he, fps );
					}
					webCamTexture.name = ""; // <- using the name for userdata
				}
			} catch (System.Exception e ) {
				Debug.LogError( "Exception when starting webCam : " + e.ToString() +  " state=" + cameraState );
				hasDevice = false;            
				cameraState = CameraState.Error;
				webCamTexture = null;
				OnStartingCameraDone(false);
				yield break;
			}

			if( hasDevice ) {
				if(verbose)Debug.Log( "Invoking Play on WebCam Texture" );
				CameraCapture.StartCapture( webCamTexture, true, ( bool ok )=>{ 
					if(verbose)Debug.Log ("Callback - got first frame."); 
					gottenFirstFrame=true;
				} );
			} 

			// hide the view until texture is ready.
			if(view!=null)view.enabled = false;
			if(verbose)Debug.Log ("Disabling preview view until camera is ready"); 

			if( Application.platform == RuntimePlatform.IPhonePlayer ) { 
				while( !gottenFirstFrame ) { // on iphone we can use the gottenFirstFrame boolean.
					if(verbose)Debug.Log ("Waiting for first frame");
					yield return 0;
				}
				while( webCamTexture.isPlaying == false ) {
					if(verbose)Debug.Log ("Waiting for webcam to be playing");
					yield return 0;
				}
			} else {
				// give it space... give it space...
				while( webCamTexture.isPlaying == false ) {
					yield return 0;
				}
				yield return new WaitForSeconds( 0.25f ); 
			}

			// wait some extra time before showing..
			if( webCamTexture!=null && !webCamTexture.didUpdateThisFrame ) {
				if(verbose)Debug.Log ( "Webcam - waiting for webcam to boot up" );
				yield return 0; // wait for next frame.
			}

			if(verbose)Debug.Log ("Enabling preview view, now ready to render camera preview feed."); 
			if( view!=null ) {
				view.texture = webCamTexture;
				view.enabled = true;
			}
			startingCamera = false;
			webCamRunning = true;

			// HACK:take a dummysnapshot to get the width/height of the image..
			// set the dummyCnt to 0.25 in order to retry taking a snapshot 
			// in the update loop.
			if(verbose)Debug.Log ("Take a dummy snapshot."); 
			TryDummySnapshot(); 
			dummyCnt = 0.25f; 

			// Focus related things.
			// On Android enable autofocus if available.
			#if !UNITY_EDITOR && UNITY_ANDROID 
				UnityCaptureTestResolve();
				if(EnableAutoFocusOnAndroid ) {
					EnableAutofocusOnAndroid( PreferAndroidFocusMode );
				}
			#endif

			if( PreferedFlashAndTorch != PreferFlashAndTorchMode.NotSet ) {
				if( PreferedFlashAndTorch != PreferFlashAndTorchMode.TorchOn ){
					this.SetFlashMode( (FlashModes)(int)PreferedFlashAndTorch );
					this.useTorch = this.IsTorchEnabled;
				} else {
					this.useTorch = true;
					this.SetTorchEnabled( true );
					this.useTorch = this.IsTorchEnabled;
					if( this.useTorch!=true) {
						Debug.LogWarning("We expected useTorch to be true");
					}
				}
			} 

			// Flash mode related things.
			currentFlashModeIndex = this.SupportedFlashModes.Count > 0 ? 0 : -1;
			var flashMode = this.webCamTexture.GetFlashMode();
			if( this.SupportedFlashModes.Count > 0 ) {
				if( flashMode == FlashModes.Off ) {
					currentFlashModeIndex = 0;
				}
				else if( flashMode == FlashModes.On ) {
					currentFlashModeIndex = 1;
				}
				else if( flashMode == FlashModes.FlashModeAuto ) {
					currentFlashModeIndex = 2;
				}
			}
			if( verbose) Debug.Log( "Started with Flash mode = " + flashMode + " cur flash index:" + currentFlashModeIndex + " hasTorch : " + this.webCamTexture.GetHasTorch() );

			OnStartingCameraDone( true && webCamTexture!=null );
			if( callback!=null ) {
				callback();
			} 
		}

	
		public string CurrentFlashModeAsStr {
			get {
				if( this.SupportedFlashModes.Count > 0 ) {
					var flashMode = this.webCamTexture.GetFlashMode();
					if( flashMode == FlashModes.Off ) {
						return "Off";
					}
					else if( flashMode == FlashModes.On ) {
						return "On";
					}
					else if( flashMode == FlashModes.FlashModeAuto ) {
						return "Auto";
					}
				}
				return "off";
			}
		}


		public FlashModes CurrentFlashMode {
			get {
				if( this.SupportedFlashModes.Count > 0 ) {
					var flashMode = this.webCamTexture.GetFlashMode();
					return flashMode; 
				}
				return FlashModes.Off;
			}
		}


		public void SetFlashMode( FlashModes mode ) {
			webCamTexture.SetFlashMode( mode );
		}


		public void SetTorchEnabled( bool value ) {
			this.webCamTexture.SetTorchEnabled( value );
		}


		IEnumerator StopWebCamImpl( bool disableWebCam, System.Action doneCallback ) {
			shuttingDownWebCam = true;
			if( disableWebCam ) {
				if(view!=null) {
					view.enabled = false;
				}
			}
			yield return new WaitForSeconds(1f);
			if( webCamTexture != null ) webCamTexture.Stop();

			cameraState = CameraState.None;
			shuttingDownWebCam = false;
			webCamRunning = false;

			if( webCamTexture != null ) {
				//WebCamTexture.Destroy( webCamTexture );
			}

			ReleaseWebCamTexture();

			if( doneCallback != null ) {
				doneCallback();
			}
		}


		// ANDREAS added this: In some cases we apparently don't get correct width and height until we have tried to read pixels
		// from the buffer.
		void TryDummySnapshot( ) {
			if(!gotAspect) {
				if( webCamTexture.width>16 ) {
					if( Application.platform == RuntimePlatform.IPhonePlayer ) {
						if(verbose)Debug.Log("Already got width height of WebCamTexture.");
					} else { 
						if(verbose)Debug.Log("Already got width height of WebCamTexture. - taking a snapshot no matter what.");
						var tmpImg = new Texture2D( webCamTexture.width, webCamTexture.height, TextureFormat.RGB24, false );
						tmpImg.name = "dummysnap";
						Color32[] c = webCamTexture.GetPixels32();
						tmpImg.SetPixels32(c);
						tmpImg.Apply();
						Texture2D.Destroy(tmpImg);
					}
					gotAspect = true;
				} else {
					if(verbose)Debug.Log ("Taking dummy snapshot");
					var tmpImg = new Texture2D( webCamTexture.width, webCamTexture.height, TextureFormat.RGB24, false );
					Color32[] c = webCamTexture.GetPixels32();
					tmpImg.SetPixels32(c);
					tmpImg.Apply();
					Texture2D.Destroy(tmpImg);
				}
			}
		}


		bool gotAspect = false;
		float dummyCnt = 0.0f;
		float lastTime = 0f;
		void Update() {
			if( webCamTexture==null ) {
				return;
			}

			if( webCamRunning ) {		
				TryDummySnapshot();

				// HACK ANDREAS this is a very fast hack to get the front camera aspect right
				// ( sometimes we need to retake the dummy snap after some time )
				if( dummyCnt>0.0f ) {
					dummyCnt -= Time.deltaTime;
					if( dummyCnt<=0.0f ) {
						dummyCnt = 0.0f;
						TryDummySnapshot();
					}
				}

				this.camwidth = webCamTexture.width;
				this.camheight = webCamTexture.height;

				// counter rotate the video input..
				int rotAngle = cameraPreviewRotatationAngle;
				while( rotAngle < 0 ) {
					rotAngle += 360;
				}
				while( rotAngle > 360 ) {
					rotAngle -= 360;
				}
				int numRotate = rotAngle / 90;
				// on android its the other way around we dont want to flip. ( on editor we want to flip and in iOs as well )
				bool postFlipY = !IsCurrentFrontFacing();
				if( Application.platform == RuntimePlatform.Android ) {
					postFlipY = !webCamTexture.videoVerticallyMirrored; 
					if( IsCurrentBackFacing() ) {
						postFlipY = !postFlipY;
					}
				} else if( Application.platform == RuntimePlatform.IPhonePlayer ) {
					// 
					if( Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight ) {
						numRotate += 2;
					} else if( Input.deviceOrientation == DeviceOrientation.Portrait ) {

					}
					postFlipY = !WebCamTexture.videoVerticallyMirrored;
					if( IsCurrentBackFacing() ) {
						postFlipY = !postFlipY;
					}

				}

				// If a unity UI.View has been set for the Preview 
				// update the Preview layout according to the ruleset.
				float aspect = CameraDeviceAspectRatio; // also ysed to set the last aspect.
				if(view!=null) {
					UpdatePreviewLayout( numRotate, postFlipY, aspect );
				} else {
					if( UpdatePreviewLayoutDeleg!=null ) {
						UpdatePreviewLayoutDeleg( numRotate, postFlipY, aspect );
					}
				}


#if UNITY_ANDROID && !UNITY_EDITOR
				// When running on android check if camera has been shaken and refocus
				// if within a certain amount of time.
				if(EnableAutoFocusOnAndroid){
					UpdateRefocusCamera();
				}
#endif


			}

		}

		public System.Action<int, bool,float> UpdatePreviewLayoutDeleg = null;

		// used to calculate the delta acceleration from the Accelerameter.
		[HideInInspector]
		public Vector3 accelLast; 

		[HideInInspector]
		public bool Refocus = false;

		// The cooldown of how often we refucs the camera.
		[HideInInspector]
		public float TryRefocusingCooldown = 0f;

		// This is used to fake an input preview stream rotation angle.
		[HideInInspector]
		public int FakeCameraStreamRotationAngle=-1;

		public CaptureMode captureMode = CaptureMode.BestCaptureQuality;

		float lastAspect = 1f;

		// return the aspect ratio of the camera feed.
		public float CameraDeviceAspectRatio {
			get {
				int w,h;
				float ratio = 1f;

				if( overwritePreviewTexture != null ) {
					w = overwritePreviewTexture.width;
					h = overwritePreviewTexture.height;

					if( webCamTexture != null ) {
						if( webCamTexture.videoRotationAngle == 90 || webCamTexture.videoRotationAngle == 270 ) {
							var t = w;
							w = h;
							h = t;
						}
					} 
					ratio = (float)w/(float)h;
					if(ratio < 0.5f) ratio = 0.5f;
					if(ratio > 2.0f) ratio = 2.0f;
					lastAspect = ratio;
					return ratio;
				}

				if( webCamTexture==null ) {
					return lastAspect;
				}
				w = webCamTexture.width;
				h = webCamTexture.height;

				// The Camera is flipped.
				if( (webCamTexture.videoRotationAngle == 90 || webCamTexture.videoRotationAngle == 270) ) {
					var t = w;
					w = h;
					h = t;
				}

				ratio = (float)w/(float)h;
				if(ratio < 0.5f) ratio = 0.5f;
				if(ratio > 2.0f) ratio = 2.0f;
				lastAspect = ratio;
				return ratio;
			}
		}


		// ==============================================================================
		// 
		// This is used to set the layout of the preview view.
		//
		// iPhone Camera Settings ( the iPhone Camera so far operates like this )
		//	When using a AVSession set to Photo resolution
		//  The camera will be sharp and the image will have various resolutions on various
		//  devices - however its allways follows the aspect 4/3 if not chopped.
		void UpdatePreviewLayout( int numRotate ,  bool postFlipY, float rawAspect ){
			AspectRatioFitter asf = view.GetComponent<AspectRatioFitter>();
			if( asf == null ) {
				Debug.Log("GameObject containing CameraCaptureView did not contain a AspectRatioFitter, adding one now");
				asf= view.gameObject.AddComponent<AspectRatioFitter>();
			}
			var rt = this.GetComponent<RectTransform>();
			int w = PreviewTexture.width;
			int h = PreviewTexture.height;

			if( verbose ) {
				if( Time.time > lastTime + 10 || lastTime == 0 ) {
					Debug.Log ("Camera Running. Texture w/h:" + w + " , " + h ); 
					lastTime = Time.time;
				}
			}

			// Fullscreen mode.. ( iPhone camera stretched )
			if( previewScaleMode == PreviewScaleMode.iPhone_PortraitMode_ScaleAndChopCamFeedToFitFullscreen ) { // works for iPhone....
				asf.enabled=false;
				float scrasp = ((float)Screen.width / (float)Screen.height);

				float imgasp = 0.75f; // 1440 x 1080 // <- on iPhone we can calculate the aspect here..
				float extra = (((imgasp / scrasp))-1f) / 2f;
				rt.SetAnchorsAndFitCorners( new Vector2(0.0f-extra,0.0f), new Vector2(1f+extra,1f) );

			} else if( previewScaleMode == PreviewScaleMode.iPhone_PortraitMode_DisplayCameraFeedAsIs ) { // currently only works for iPhone camera, nothing else.
				asf.enabled=false;
				rt.SetAnchorsAndFitCorners( new Vector2(0.0f,0.25f), new Vector2(1.0f,1.0f) );

			} else if( previewScaleMode == PreviewScaleMode.DontModifyPreview ) { // currently only works for iPhone camera, nothing else.
				// Do nothing to the preview.

			} else if( previewScaleMode == PreviewScaleMode.EnvelopePreviewToFitParent ) { // scale to fit envelope in parent. 
				asf.enabled=true;
				asf.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
				if(h==0) h=1;
				// The aspect is diffirent when the  camera is rotated...

				if( (webCamTextureRotationAngle == 90 || webCamTextureRotationAngle == 270) ) {
					var t = w;
					w = h;
					h = t;
				}
				float ratio = (float)w/(float)h;
				if( ratio < 0.5f ) ratio = 0.5f;
				if( ratio > 2.0f ) ratio = 2.0f;
				//if( verbose ) Debug.Log("Envelope in parent, " + " aspect =" + ratio + " w = " +w + " h=" + h );

				asf.aspectRatio = ratio;

				//Debug.Log("Envelope in parent, " + " aspect =" + ratio + " w = " +w + " h=" + h );
			}

			int nr = numRotate;
			while( nr < 0 ) nr+=4;
			while( nr > 4 ) nr-=4;
			if( Application.platform == RuntimePlatform.IPhonePlayer ){
				Debug.Log( "Preview Mode numRotate=" + (nr*90) + " postFlipY=" + postFlipY +  " webCamMirror = " + WebCamTexture.videoVerticallyMirrored + " wemCamFlip=" + WebCamTexture.videoRotationAngle  );
			}
	
			view.SetRotate90Degrees( numRotate );
			view.SetFlipAfterRotate( postFlipY );
		}


		void UpdatePreviewLayout2( int numRotate, bool postFlipY, int w, int h ){
			AspectRatioFitter asf = view.GetComponent<AspectRatioFitter>();
			if( asf == null ) {
				Debug.Log("GameObject containing CameraCaptureView did not contain a AspectRatioFitter, adding one now");
				asf= view.gameObject.AddComponent<AspectRatioFitter>();
			}
			//var rt = this.GetComponent<RectTransform>();

			asf.enabled=true;
			asf.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
			if(h==0) h=1;
			// The aspect is diffirent when the  camera is rotated...
			float ratio = (float)w/(float)h;
			if( ratio < 0.5f ) ratio = 0.5f;
			if( ratio > 2.0f ) ratio = 2.0f;
			asf.aspectRatio = ratio;
			int nr = numRotate;
			while( nr < 0 ) nr+=4;
			while( nr > 4 ) nr-=4;
			view.SetRotate90Degrees( numRotate );
			view.SetFlipAfterRotate( postFlipY );
		}


		int cameraPreviewRotatationAngle {
			get {
#if UNITY_EDITOR
				if( FakeCameraStreamRotationAngle != -1 ) {
					return FakeCameraStreamRotationAngle;
				}
#endif
				int rotAngle = 0;
				if( webCamTexture!=null ){
					rotAngle = -webCamTexture.videoRotationAngle;
				} else {

				}
				return rotAngle;
			}
		}

		[System.NonSerialized]
		bool useTorch = false;

		// this is used when the preview texture is overwritten by another type of texture.
		Texture2D overwritePreviewTexture = null;

		// 
		public Texture PreviewTexture {
			get {
				if( overwritePreviewTexture != null ) {
					return this.overwritePreviewTexture;
				}
				return this.webCamTexture;
			}
		}


		private float VideoRotationAngle2 {
			get {
				if( overwritePreviewTexture != null ) {
					return 0f;
				}
				if( this.webCamTexture == null ) {
					return 0f;
				}
				return this.webCamTexture.videoRotationAngle;
			}
		}


		// Callback used when requesting to start the camera.
		public static System.Action<bool> OnStartingCameraDone;


		// This functionality is Android only - On Android autofocus continiously needs to be called
		// and we've implemented a basic way to do this by using the accelerameter. When the devices has
		// been rotated a certain deta with the gyro/accel we trigger a request for refocus
		// in order not to refocus too often we will only do it each 3 seconds to not cancel out the current 
		// attempt to focus on something.
#if UNITY_ANDROID && !UNITY_EDITOR
		void UpdateRefocusCamera(){
			if( Application.platform == RuntimePlatform.Android ) {
				if( (accelLast.x == 0f) && (accelLast.y == 0f) && (accelLast.z == 0f) ) {
					accelLast = Input.acceleration;
				}
				var acceldelta = (accelLast.normalized - Input.acceleration.normalized);
				float delta = acceldelta.magnitude;
				//Debug.Log("acceldelta.magnitude = " + delta );

				// A treshold like 0.125 seemed like a good value on Andreas Android mobile.
				// making it a good choice.
				if( delta > AutofocusMoveTreshold ) { 
					Refocus=true;
					accelLast = Input.acceleration;
				} 

				TryRefocusingCooldown -= Time.deltaTime;
				if( Refocus && TryRefocusingCooldown <= 0f) {
					Refocus = false;
					TryRefocusingNow();	
					Debug.Log("Refocus - request");
					TryRefocusingCooldown = 3f;
				}
			}
		}


		void TryRefocusingNow() {
			CameraCapture.Android_autofocus();
		}


		void UnityCaptureTestResolve() {
			CameraCapture.UnityCaptureTestResolve();
		}


		void EnableAutofocusOnAndroid( AndroidFocusModes preferFocusMode ) {
			if( Application.platform == RuntimePlatform.Android ) {
				CameraCapture.Android_EnableAutoFocus( preferFocusMode );
			}
		}
#endif


		public bool IsTorchAvailable {
			get {
				return this.webCamTexture.GetHasTorch();
			}
		}


		public void ToggleTorch() {
			if( !( webCamRunning && webCamTexture!= null ) ) {
				Debug.LogWarning("Cannot toggle torch at this time since the webCamTexture is not running");
				useTorch = !useTorch;
				return;
			}
			bool enabled = IsTorchEnabled;
			enabled = !enabled;
			Debug.Log( "" + ( !enabled ? "Turning torch off" : "Turning Torch On" ) );
			SetTorchEnabled( enabled );
		}


		public bool IsTorchEnabled {
			get {
				if(!( webCamRunning && webCamTexture!= null )) {
					Debug.LogWarning("You cannot call IsTorchEnabled when camera is not running");
					return useTorch;
				} 
				if( webCamTexture.GetHasTorch() ) {
					return webCamTexture.IsTorchEnabled();
				} 
				Debug.LogWarning("Device has no torch - torch not enabled"); 
				return false;
			}
		}



		public void ResetFocus() {
#if UNITY_ANDROID && !UNITY_EDITOR
			if( EnableAutoFocusOnAndroid && Application.platform == RuntimePlatform.Android ) {
				try {
					TryRefocusingNow();
				} catch(System.Exception e ) {
					Debug.LogError(""+e);	
				}
			} else {
				Debug.LogError("Set focus is only available on Device");
			}
#else
			Debug.LogError("Reset focus is only available on Android.");
#endif
		}



		Texture2D ConvertWebCamTextureToFinalTexture( ) {
			if( webCamTexture == null ) {
				Debug.LogError("Error there is no webCameraTexture Available");
				return null;
			}
			if(verbose)Debug.Log("ConvertWebCamTextureToFinalTexture");

			var rotationAngle = webCamTexture.videoRotationAngle;
			CopyFlip flip = CopyFlip.None;
			bool flipY = false;
			if( Application.isEditor ) {
				flipY=true;
				flip = CopyFlip.Horisontal;
			} else {
				if( Application.platform == RuntimePlatform.Android ) {
					flipY = IsCurrentFrontFacing();
				} else if( Application.platform == RuntimePlatform.IPhonePlayer ) {
					flipY = webCamTexture.videoVerticallyMirrored;
				}
			}

			CopyRotate copyRotate = CopyRotate.None;
			if( rotationAngle == 0 ) {
				copyRotate = CopyRotate.None;
			} else if( rotationAngle == 90 ) {
				copyRotate = CopyRotate.Left;
			} else if( rotationAngle == 270 ) {
				copyRotate = CopyRotate.Right;
			} else if( rotationAngle == 180 ) {
				copyRotate = CopyRotate.LeftX2;
			}

			// Android should flip it..
			if (Application.platform == RuntimePlatform.Android) {
				flipY = !webCamTexture.videoVerticallyMirrored; 
				if (IsCurrentBackFacing ()) {
					flipY = !flipY;
				}
				if (flipY)
					flip = CopyFlip.Horisontal;
			} else if( Application.platform == RuntimePlatform.IPhonePlayer ) {
				flip = CopyFlip.None;
				if (flipY)
					flip = CopyFlip.Horisontal;
			}
				
			if(verbose) Debug.Log("CameraCaptureKit: Getting final Texture; video rotation angle: " + rotationAngle + " flipy: " + flipY );
			var tex = CameraCapture.GetBufferAndStopWebCamTexture( webCamTexture, (float)rotationAngle, copyRotate, flip );

			return tex;
		}


	}

	// used to describe which action the Controller should take after running GetStillImage
	// in most cases we want to cleanup but there might be casese where you want the camera to continue
	// running , for instance when capturing multiple images in a sequence.
	public enum ActionAfterGetStill {
		Cleanup = 0,
		ContinueCapturing = 1
	}


	// This is a simple wrapper to make it possible to capture the still
	// in a call and make the call to retrieve the texture in the returned
	// struct as well.
	public class StillImageResult {
		public CameraCaptureController ccc=null;
		public void GetTextureAndShutdownCamera( System.Action<Texture2D> callback )  {
			ccc.GetStillImageAndCleanup( callback ); 
		}
	}



}
