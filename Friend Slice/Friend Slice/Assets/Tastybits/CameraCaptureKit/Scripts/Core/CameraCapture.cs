/**
 * @desc 	Camera Capture is the main class in the NativeHooks of the CameraCaptureKit.
 * 
 * 			It uses a combination of diffirent techniques to enable, anti-shake, focus, highrez still image capturing 
 * 			flash/torch.
 * 
 * @author 	Tastybits
 * @link 	www.tastybits.io
 * @copyright 2016 Tastybits
 */
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using System.Linq;



namespace Tastybits.CamCap {


	/**
	 * @Desc This class is the Interface to the Native camera functions. If you want to add
	 * some new functionality, it would be here.
	 */
	public static class CameraCapture {
		static Texture2D take_picture_cache_android=null;


		public static WebCamTexture CreateTexture (string deviceName, int reqWidth, int reqHeight, int fps) {
			//Debug.Log( "Creating webcamtexture"  );
			var ret = new WebCamTexture( deviceName, reqWidth, reqHeight, fps );
			ret.name = "";
			if( Application.platform == RuntimePlatform.Android ) {
				ret.name = "WebCamTexture";
			}
			return ret;
		}


		public static WebCamTexture ReuseWebCamTexture(string deviceName, int requestedWidth, int requestedHeight, int fps, WebCamTexture webCamTex) {
			//Debug.Log( "Creating webcamtexture"  );
			var ret = webCamTex;
			//new WebCamTexture( deviceName, i, i2, fps );
			ret.deviceName = deviceName;
			ret.requestedWidth = requestedWidth;
			ret.requestedHeight = requestedHeight;
			ret.name = "";
			if( Application.platform == RuntimePlatform.Android ) {
				ret.name = "WebCamTexture";
			}
			return ret;
		}
		  

		// Return a preview texture.
		public static void CaptureStillImage( WebCamTexture webCamTexture, CaptureMode capturemode, System.Action<bool> callback ) {
			if( Application.platform == RuntimePlatform.IPhonePlayer ) {
#if UNITY_IPHONE && !UNITY_EDITOR
				if( string.IsNullOrEmpty(webCamTexture.name) ){
					Debug.LogError("Error parsing id of WebCameraTeuxtre");
					return;
				}
				string deleg=
					NativeDelegate.Create( ( Hashtable args ) => {
						string succeeded = System.Convert.ToString(args["succeeded"]);
						//Debug.Log("succeeded = " + succeeded );
						if( callback!=null ) {
							callback( succeeded=="true" );
						}
					} );

				UnityCaptureStillImage( webCamTexture.GetPointer(), deleg );
#endif
			} else if( Application.platform == RuntimePlatform.Android ) {
#if UNITY_ANDROID && !UNITY_EDITOR
				take_picture_cache_android = null; // Resetting Android image.
				//webCamTexture.Pause();
				var ts = DateTime.Now;
				Debug.Log("CameraCaptureKit: Initiating Android Snapshot");
				string deleg=
					NativeDelegate.Create( ( Hashtable args ) => {
					Debug.Log("CameraCaptureKit: Android Snapshot returned! after duration=" + (DateTime.Now-ts).TotalSeconds+" secs" );
						bool ok = args.ContainsKey("ok") ? System.Convert.ToBoolean(args["ok"]) : false;
						string msg = args.ContainsKey("msg") ? System.Convert.ToString(args["msg"]) : "";
						if( string.IsNullOrEmpty(msg) == false ) {
							Debug.LogError( ""+msg );
						}
						if(!args.ContainsKey("ok") ) {
							Debug.LogError("CameraCaptureKit: TakePicture Return callback didn't contain ok." );
						} 
						if( ok ) {
							if( !args.ContainsKey("data") ) {
								Debug.LogError("CameraCaptureKit: TakePicture return didnt contain data");
							}
						} else {
							if( !args.ContainsKey("msg") ) {
								Debug.LogError("CameraCaptureKit: TakePicture return didnt contain a msg");
							}
						}

						if( ok ) {

							int wi,he;
							byte[] raw_bytes = AndroidCamera_PopCapturedPicture( out wi, out he );
							if( raw_bytes != null && raw_bytes.Length > 0 ) {
								take_picture_cache_android = CamCap.TextureUtils.CreateTextureFromBytes( wi, he, raw_bytes );
							} else {
								string str_data = !args.ContainsKey("data") ? "" : (string)args["data"];
								if( !string.IsNullOrEmpty( str_data ) ) {
									var bytes = System.Convert.FromBase64String(str_data);
									if( bytes == null || bytes.Length == 0 ) {
										Debug.LogError("CameraCaptureKit: Error decoding answer from Android Camera");
									} else {
										take_picture_cache_android = new Texture2D(2,2);
										take_picture_cache_android.name = "CapturedByAndroid";
										take_picture_cache_android.LoadImage(bytes); 
										if( take_picture_cache_android.width == 2 ) {
											Debug.LogError("CameraCaptureKit: Failed to load takePicture result");
											take_picture_cache_android = null; // Fallback on grapping pixels from the WebCamTexture.
										}
										Debug.Log("CameraCaptureKit: Gotten image from Android SDK : " + take_picture_cache_android.width + " , " + take_picture_cache_android.height );
									}
								} else  {
									Debug.LogError("CameraCaptureKit: Response from Android Camera contained no imagedata");
								}
							}

						}

						Debug.Log("CameraCaptureKit: TakePictureCallback ok = " + ok + " time spend waiting = " + (DateTime.Now-ts).TotalSeconds + " secs" );

						webCamTexture.Pause();

						if( callback!=null ) {
							callback( ok );
						}
					} );

				UnityCamera_TakePicture( deleg );
				Debug.Log("CameraCaptureKit: Waiting for Android Snapshot");

				//callback(true);
#endif
			} else {
				webCamTexture.Pause();
				callback(true);
			}
		}


		public static Texture2D GetAndroidStillImageTaken() {
			if( Application.platform == RuntimePlatform.Android ) {
				return take_picture_cache_android;
			}
			return null;
		}



		public static void SetStatusBarHidden( bool value ) {
#if UNITY_IPHONE  && !UNITY_EDITOR
			UnitySetStatusBarHidden( value );
#else
			Debug.Log("SetStatusBarButton not enabled in editor");
#endif
		}


		static CameraProperty[] DecodeJSONProperties( string str_json ) {
			Hashtable props = (Hashtable)Tastybits.CamCap.JSON.Deserialize( str_json );
			if( props == null ) {
				Debug.LogError("Error decoding json string : "+ str_json );
				return new CameraProperty[]{};
			}
			System.Collections.Generic.List<CameraProperty> ret = new System.Collections.Generic.List<CameraProperty>();

			foreach( var o_key in props.Keys ) {
				string key = (string)o_key;
				Hashtable tbl = (Hashtable)props[o_key];

				var avail = new System.Collections.Generic.List<string>();
				string name = key;
				string value = System.Convert.ToString(tbl["value"]);
				var arr_avail = (ArrayList)tbl["avail"];
				foreach( var avail_elem in arr_avail ) {
					string str_av = System.Convert.ToString( avail_elem );
					avail.Add( str_av );
				}
				var p = new CameraProperty(name,value, avail.ToArray() );
				ret.Add(p);
			}
			return ret.ToArray();
		}


		public static CameraProperty[] GetAvailableProperties( WebCamTexture webCamTexture ){
			if( Application.platform == RuntimePlatform.IPhonePlayer ) {
				//string str_json = "";
#if UNITY_IPHONE && !UNITY_EDITOR
				string str_json = UnityCaptureGetProperties( webCamTexture.GetPointer() );
				return DecodeJSONProperties(str_json);
#endif
			} else if( Application.platform == RuntimePlatform.Android ) {
				//string str_json = "";
#if UNITY_ANDROID && !UNITY_EDITOR
				string str_json = AndroidCamera_GetProperties(  );
				return DecodeJSONProperties(str_json);
#endif
			} else {
				var prop1 = new CameraProperty( "IsWebCam", "true", new string[]{"true"} );
				var prop2 = new CameraProperty( "TestProp", "2", new string[]{"1","2", "3"} );
				var ret = new System.Collections.Generic.List<CameraProperty>();
				ret.Add(prop1);
				ret.Add(prop2);
				return ret.ToArray();
			}
			return null;
		}


		public static void StartCapture( WebCamTexture webCamTexture, bool stillImage, System.Action<bool> callback ) {
			if( Application.platform == RuntimePlatform.IPhonePlayer ) {
#if UNITY_IPHONE && !UNITY_EDITOR
				//BreakPoint( "mem now ? " );
				if( stillImage ) {
					UnitySetNextSessionIsStillImage();
				}

				string strcb = NativeDelegate.Create( ( Hashtable args )=>{
					callback( true );
				} );

				if( string.IsNullOrEmpty(strcb) ) {
					throw new System.Exception("Callback is Emptystring - invalid callback id." );
				}

				//BreakPoint( "before play" );
				webCamTexture.Play();
				//BreakPoint( "after play" );

				string pointer = UnityGetLastCameraCapturePointer();
				webCamTexture.name = "" + pointer;
				Debug.Log ("Got webCam referance : " + webCamTexture.name + " firstframecb = " + strcb );

				UnitySetFirstFrameCallback( webCamTexture.GetPointer(), ""+strcb );
				//Debug.Log ("Got webCam referance 222 : " + webCamTexture.name + " firstframecb = " + strcb );

				//BreakPoint( "mem now ? " );
#endif
			} else if( Application.platform == RuntimePlatform.Android ) {
#if UNITY_ANDROID && !UNITY_EDITOR
				//CameraCapture.AndroidCamera_SetPreviewSize( Screen.width, Screen.height );
				webCamTexture.name = "" + UnityEngine.Random.Range( 0, 99999 );
				webCamTexture.Play();
				callback( true );
#endif
			} else {
				webCamTexture.name = "" + UnityEngine.Random.Range( 0, 99999 );
				webCamTexture.Play();
				callback( true );
			}
			//NativeDelegate.FireDelegate( strcb );
		}

		public static void SetTorch( WebCamTexture webCamTexture, bool value, System.Action<bool> callback ) {
#if UNITY_IPHONE && !UNITY_EDITOR
			if( string.IsNullOrEmpty(webCamTexture.name) ){
				Debug.LogError("Error parsing id of WebCameraTeuxtre");
				return;
			}
			string strcb = NativeDelegate.Create( ( Hashtable args )=>{
				//Debug.Log ("First frame ready");
			} );
			int mode = (int)(value ? TorchModes.On : TorchModes.Off);
			UnityCaptureSetTorchMode( webCamTexture.GetPointer(), mode, strcb );
#else
			Debug.Log("CameraCaptureKit: Setting Flash not support in Editor");
#endif
		}


		public static Texture2D GetBufferAndStopWebCamTexture( WebCamTexture webCamTexture, float rotationAngle, CopyRotate rotate, CopyFlip flip ) {
			if( Application.platform ==  RuntimePlatform.IPhonePlayer ) {
#if UNITY_IPHONE && !UNITY_EDITOR
				int width = webCamTexture.width;
				int height = webCamTexture.height;

				if( string.IsNullOrEmpty(webCamTexture.name) ) {
					Debug.LogError( "Error parsing id of WebCameraTexture : " + webCamTexture.name + ( string.IsNullOrEmpty(webCamTexture.name) ? "noname" : "" ) );
					return null;
				}

				int nBytes = 0; 
				IntPtr ptrBytes = UnityCameraCaptureReadPngAndStop( webCamTexture.GetPointer(), ref nBytes, width, height, webCamTexture.videoVerticallyMirrored );
				byte[] bytes = new byte[nBytes];
				Marshal.Copy( ptrBytes, bytes, 0, nBytes );
				Marshal.FreeHGlobal( ptrBytes );
				Texture2D tex = new Texture2D( 2, 2 );
				tex.LoadImage( bytes );
				tex.name = "Final Photo";

				return tex;
#else
				return null;
#endif
			} else if( Application.platform == RuntimePlatform.Android && CameraCapture.take_picture_cache_android!=null ) {
				Debug.Log("CameraCaptureKit: Getting Android photo from take_picture_cache_android, width/height= " + take_picture_cache_android.width + " , " + take_picture_cache_android.height + " needsRotate="+rotate + " needsFlip=" + flip);

				var ts = DateTime.Now;

				var pixels = CameraCapture.take_picture_cache_android.GetPixels32();

				var imageOfSnapshot = TextureExtensions.CreateTextureFromColors( pixels, take_picture_cache_android.width, take_picture_cache_android.height, rotate, flip );
				imageOfSnapshot.name  = "Final Photo";
				imageOfSnapshot.Apply();

				Texture2D.Destroy(take_picture_cache_android);
				take_picture_cache_android = null;

				Debug.Log("CameraCaptureKit: Time spend scaling image :" + ( DateTime.Now - ts ).TotalSeconds + " secs");

				return imageOfSnapshot;

			} else {

				if( Application.platform == RuntimePlatform.Android ) {
					Debug.Log("CameraCaptureKit: On Android we didnt have a cached image, just using the WebCamTexture");
				}

				/*bool pauseIt = false;
				if( !webCamTexture.isPlaying ) {
					pauseIt = true;
				}*/

				var pixels = webCamTexture.GetPixels32();
				/*if( pauseIt ) {
					//webCamTexture.Pause();
				}*/

				if ( Application.isEditor ) { // overwrite flip and rotate in the editor 
					rotate = CopyRotate.None;
				} else {
					
				}
				var imageOfSnapshot = TextureExtensions.CreateTextureFromColors( pixels, webCamTexture.width, webCamTexture.height, rotate, flip );
				imageOfSnapshot.name  = "Final Photo";
				imageOfSnapshot.Apply();

				return imageOfSnapshot;

			}
		}

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// iPhone Interface 
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#if !UNITY_EDITOR && UNITY_IPHONE	

		// Indicates whether the capture device has a flash. (read-only)
		public static bool HasFlash( WebCamTexture texture ) {
			return UnityCaptureHasFlash( texture.GetPointer() );
		}
		[DllImport ("__Internal")]
		private static extern bool UnityCaptureHasFlash( string spcapture  );

		// used to return if a flash mode is supported
		public static bool GetFlashModeSupported( WebCamTexture texture, FlashModes mode ) {
			return UnityFlashModeSupported( texture.GetPointer(), (int)mode );
		}
		[DllImport ("__Internal")]
		private static extern bool UnityFlashModeSupported( string spcapture, int flashMode  );

		// used to return if a flash mode is supported
		public static FlashModes GetFlashMode( WebCamTexture texture ) {
			return (FlashModes)UnityGetFlashMode( texture.GetPointer() );
		}
		[DllImport ("__Internal")]
		private static extern int UnityGetFlashMode( string spcapture  );

		// used to set the flash mode when running.
		public static void SetFlashMode( WebCamTexture texture, FlashModes mode ) {
			UnitySetFlashMode( texture.GetPointer(), (int)mode );
		}
		[DllImport ("__Internal")]
		private static extern void UnitySetFlashMode( string spcapture, int flashMode  );

		// used to get if the the focus mode is supported.
		public static bool IsFocusModeSupported( WebCamTexture texture, FlashModes mode ) {
			return UnityIsFocusModeSupported( texture.GetPointer(), (int)mode );
		}
		[DllImport ("__Internal")]
		private static extern bool UnityIsFocusModeSupported( string spcapture, int mode  );

		// used to set the focus mode.
		public static void UnitySetFocusMode( WebCamTexture texture, FocusModes mode ) {
			UnitySetFocusMode( texture.GetPointer(), (int)mode );
		}
		[DllImport ("__Internal")]
		private static extern void UnitySetFocusMode( string spcapture, int mode  );

		// used to get the current focus mode.
		public static int GetFocusMode( WebCamTexture texture ) {
			return UnityGetFocusMode( texture.GetPointer() );
		}
		[DllImport ("__Internal")]
		private static extern int UnityGetFocusMode( string spcapture  );

		// 
		[DllImport ("__Internal")]
		private static extern void UnitySetNextSessionIsStillImage();

		[DllImport ("__Internal")]
		private static extern string UnityGetLastCameraCapturePointer();

		[DllImport ("__Internal")]
		private static extern void UnitySetFirstFrameCallback( string spcapture, string callback );

		[DllImport ("__Internal")]
		private static extern void UnityCaptureStillImage( string spcapture, string callback );


		[DllImport ("__Internal")]
		private static extern string UnityCaptureGetProperties( string spcapture );



		[DllImport ("__Internal")]
		private static extern void UnityCaptureSetTorchMode( string spcapture, int mode, string callback );


		[DllImport ("__Internal")]
		private static extern int UnityCaptureGetTorchMode( string spcapture );


		public static TorchModes UnityGetTorchMode( WebCamTexture texture ) {
			return (TorchModes)UnityCaptureGetTorchMode( texture.GetPointer() );
		}

		public static bool GetHasTorch( WebCamTexture texture ) {
			return UnityCaptureGetHasTorch( texture.GetPointer() );
		}
		[DllImport ("__Internal")]
		private static extern bool UnityCaptureGetHasTorch( string spcapture );


		[DllImport ("__Internal")]
		private static extern IntPtr UnityCameraCaptureReadPngAndStop( string spcapture, ref int nbytes, int w, int h, bool mirrored );

		[DllImport ("__Internal")]
		private static extern void UnitySetStatusBarHidden( bool value );

		[DllImport ("__Internal")]
		private static extern void UnityBreakPoint( string msg );

		[DllImport ("__Internal")]
		private static extern int UnityCameraCaptureGetWidth( string spcapture );

		[DllImport ("__Internal")]
		private static extern int UnityCameraCaptureGetHeight( string spcapture );


		/**********************************************************************
		 * New stuff..
		 **********************************************************************/
		public static void SetExposureCompensation( WebCamTexture texture, int value ) {
			UnitySetExposureCompensation( texture.GetPointer(), value );
		} 
		[DllImport ("__Internal")]
		private static extern void UnitySetExposureCompensation( string spcapture, int value  );

		// TODO: rename to GetMinMaxExposureCompensation!!!
		public static void GetMinMaxExposure( WebCamTexture texture, out int minval, out int maxval ) {
			minval = UnityGetMinExposureCompensationValue( texture.GetPointer() );
			maxval = UnityGetMaxExposureCompensationValue( texture.GetPointer() );
		}

		[DllImport ("__Internal")]
		private static extern int UnityGetMinExposureCompensationValue( string spcapture  );

		[DllImport ("__Internal")]
		private static extern int UnityGetMaxExposureCompensationValue( string spcapture  );


		// Note: Know that this is not used yet..
		[DllImport ("__Internal")]
		private static extern int UnityGetMinExposureValue( string spcapture  );

		// Note: Know that this is not used yet..
		[DllImport ("__Internal")]
		private static extern int UnityGetMaxExposureValue( string spcapture  );


		public static bool IsAutoExposureLockSupported( WebCamTexture texture ){
			return UnityCanAdjustExposureMode( texture.GetPointer() );
		}
		[DllImport ("__Internal")]
		private static extern bool UnityCanAdjustExposureMode( string spcapture  );


		public static void SetAutoExposureLock( WebCamTexture texture, bool value ) {
			UnitySetAutoExposureLock( texture.GetPointer(), value );
		}
		[DllImport ("__Internal")]
		private static extern void UnitySetAutoExposureLock( string spcapture, bool value  );


		public static string[] GetSupportedISOValues( WebCamTexture texture ) {
			var str = UnityGetSupportedISOValues( texture.GetPointer() );
			return str.Split( new char[]{' ',','} , System.StringSplitOptions.RemoveEmptyEntries );
		}
		[DllImport ("__Internal")]
		private static extern string UnityGetSupportedISOValues( string spcapture  );


		public static void SetISO( WebCamTexture texture, string value ) {
			UnitySetCurrentISO( texture.GetPointer(), value );
		}

		[DllImport ("__Internal")]
		private static extern void UnitySetCurrentISO( string spcapture, string str_value  );


		public static string[] iOS_GetSupportedFlashModeNames(  ) {
			throw new System.NotImplementedException();
			//return new string[]{};
		}


		public static string GetISO( WebCamTexture texture ) {
			float currentExposure = UnityGetCurrentISO( texture.GetPointer() );
			if( currentExposure == -999 ) return "auto";
			return ""+currentExposure;
		}
		[DllImport ("__Internal")]
		private static extern float UnityGetCurrentISO( string spcapture  );


		public static void EnableShutterSound( WebCamTexture texture, bool shutterSoundOn ) {
			Debug.Log("EnableShutterSound is not possible on iOS");
		} 

#endif

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// Android Interface 
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#if !UNITY_EDITOR && UNITY_ANDROID
		static AndroidJavaClass _CameraCaptureCls;
		public static AndroidJavaClass CameraCaptureCls {
			get {
				if(_CameraCaptureCls==null ) {
					_CameraCaptureCls = new AndroidJavaClass("com.CameraCaptureKit.WebCamTextureCameraExt");
				}
				return _CameraCaptureCls;
			}
		}

#endif

	   	/**
	 	 * @Class Run this when the Still image is to be discarded and submitted again.
	 	 **/
		public static void Android_DiscardStillImageAndContinue() {
			if( take_picture_cache_android!=null ) 
				Texture2D.Destroy(take_picture_cache_android);
			take_picture_cache_android = null;
			Resources.UnloadUnusedAssets();
#if UNITY_ANDROID && !UNITY_EDITOR
			CameraCaptureCls.CallStatic("UnityCamera_StartPreview", new object[]{} );
#endif
		}

#if !UNITY_EDITOR && UNITY_ANDROID

		public static void UnityCaptureTestResolve() { 
			CameraCaptureCls.CallStatic("UnityCaptureTestResolve", new object[] { (int)Screen.width, (int)Screen.height } );
		}

		// Run the autofocus Process within the Android SDK making it
		// which is a limited time thing.
		public static void Android_autofocus() {
			CameraCaptureCls.CallStatic("UnityCaptureAutofocus", new object[] {} );
		}

		// According to http://forum.unity3d.com/threads/webcamtexture-on-android-focus-mode-fix.327956/
		// this can be used to get a referance to the Camera object unity has intiialized
		// futhermore enableAutoFocus is used to enable the autofocus functionality.
		public static void Android_EnableAutoFocus( AndroidFocusModes preferMode ) {
			CameraCaptureCls.CallStatic("UnityCaptureEnableAutofocus", new object[] { (int) preferMode } );
		}

		public static void Android_SetTorchEnabled( bool value ) {
			CameraCaptureCls.CallStatic("UnityCamera_SetTorchEnabled", new object[] { value } );
		}

		public static int Android_GetFlashMode(  ) {
			return CameraCaptureCls.CallStatic<int>("UnityCamera_GetFlashMode", new object[] { } );
		}

		public static bool Android_IsTorchEnabled(  ) {
			return CameraCaptureCls.CallStatic<int>("UnityCamera_IsTorchEnabled", new object[] { } ) == 1;
		}
	
		public static void Android_SetFlashMode( int value ) {
			CameraCaptureCls.CallStatic("UnityCamera_SetFlashMode", new object[] { value } );
		}


		public static bool Android_HasTorch(){
			return CameraCaptureCls.CallStatic<bool>("UnityCamera_HasTorch", new object[] { } );
		}


		public static string AndroidCamera_GetProperties() {
			return CameraCaptureCls.CallStatic<string>("UnityCamera_GetProperties", new object[] { } );
		}


		public static bool Android_GetFlashModeSupported( FlashModes mode ) {
			return CameraCaptureCls.CallStatic<bool>( "UnityCamera_GetFlashModeSupported", new object[] { (int)mode } );
		}


		public static string Android_GetSupportedFlashModeNames( FlashModes mode ) {
			return CameraCaptureCls.CallStatic<string>("Android_GetSupportedFlashModeNames", new object[] { } );
		}


		public static bool Android_IsFocusSupported( ) {
			return CameraCaptureCls.CallStatic<bool>("UnityCapture_IsFocusSupported", new object[] { } );
		}


		public static void SetExposureCompensation( int value ) {
			CameraCaptureCls.CallStatic("UnityCamera_SetExposureCompensation", new object[] {value} );		
			//Debug.Log("SetExposureCompensation is not available on iOS");
		} 

		//TODO: rename to GetMinMaxExposureCompensation!!!
		public static void GetMinMaxExposure( out int minval, out int maxval ) {
			minval = System.Convert.ToInt32( CameraCaptureCls.CallStatic<int>("UnityCamera_GetMinExposureCompensation", new object[] {} ) );
			maxval = System.Convert.ToInt32( CameraCaptureCls.CallStatic<int>("UnityCamera_GetMaxExposureCompensation", new object[] {} ) );
		}


		public static void UnityCamera_TakePicture( string cbid ) {
			CameraCaptureCls.CallStatic( "UnityCamera_TakePicture", new object[] { cbid } );
		}


		// This is very experimental and very untestet, need someone to replicate the issue
		// on their phone and get back to us before we can really do something about this.
		public static void ApplyLGFix(){
			CameraCaptureCls.CallStatic("UnityCamera_LGFIX_Apply", new object[] {} );
		}


		public static bool IsAutoExposureLockSupported(){
			bool ret=System.Convert.ToBoolean( CameraCaptureCls.CallStatic<bool>("UnityCamera_IsAutoExposureLockSupported", new object[] {} ) );
			return ret;
		}

		public static void SetAutoExposureLock( bool value ) {
			CameraCaptureCls.CallStatic("UnityCamera_SetAutoExposureLock", new object[] {value} );
		}

		public static string[] GetSupportedISOValues(  ) {
			var jc = CameraCaptureCls;
			try {
				string strSupportedValues = jc.CallStatic<string>("UnityCamera_GetSupportedISOValues", new object[] {} );
				return strSupportedValues.Split( new char[]{','} ).ToArray();
			} catch (System.Exception e ) {
				Debug.LogError(e.ToString());
			}
			return new string[]{};
		}


		public static void SetISO( string value ) {
			CameraCaptureCls.CallStatic("UnityCamera_SetISO", new object[] {value} );
		}


		public static string GetISO() {
			return System.Convert.ToString( CameraCaptureCls.CallStatic<string>("UnityCamera_GetISO", new object[] {} ) );
		}

		public static void EnableShutterSound( bool shutterSoundOn ) {
			CameraCaptureCls.CallStatic("UnityCamera_EnableShutterSound", new object[] { shutterSoundOn } );
		} 

		public static bool CanEnableAndDisableShutterSound() {
			return System.Convert.ToBoolean( CameraCaptureCls.CallStatic<bool>("UnityCamera_CanEnableAndDisableShutterSound", new object[] {  } ) );
		}


		public static void UnityCamera_SetBestPictureSize( int w, int h ) {
			CameraCaptureCls.CallStatic( "UnityCamera_SetBestPictureSize", new object[] { w, h } );
		} 


		public static void AndroidCamera_SetPictureQuality( int qualityLevel, bool setPreview, bool setPicture ) {
			CameraCaptureCls.CallStatic( "UnityCamera_SetPictureQuality", new object[] { qualityLevel, setPreview, setPicture } );
		} 


		/*public static void AndroidCamera_SetPreviewSize( int width, int height ) {
			CameraCaptureCls.CallStatic( "UnityCamera_SetPreviewSize", new object[] { width, height } );
		}*/


		public static byte[] AndroidCamera_PopCapturedPicture( out int wi, out int he ) {
			wi = CameraCaptureCls.CallStatic<int>( "UnityCamera_GetLastCapturedWidth", new object[]{} );
			he = CameraCaptureCls.CallStatic<int>( "UnityCamera_GetLastCapturedHeight", new object[]{} );
			var obj = CameraCaptureCls.CallStatic<AndroidJavaObject>("UnityCamera_PopCapturedPicture", new object[] {  } );
			if (obj.GetRawObject().ToInt32() != 0) {
				byte[] result = AndroidJNIHelper.ConvertFromJNIArray<byte[]>(obj.GetRawObject());
				if( result == null || result.Length == 0 ) {
					Debug.LogError("Error returning the bytes from the byte array (2)");
				}
				return result;	
			}
			Debug.LogError("Error returning bytes from last raw picture captured. (1)");
			return null;
		} 



#endif

		public static void BreakPoint( string msg="") {
			if( Application.isEditor ) return;
#if UNITY_IPHONE && !UNITY_EDITOR
			UnityBreakPoint(msg);
#endif
		}


	}




}



