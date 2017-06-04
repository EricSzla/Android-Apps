#pragma warning disable 414
/**
 * @desc 	This class services as eyecandy and to tuck the functionality neatly into 
 * 			a familiar syntax when using the WebCamTexture.
 * 
 * @author 	Tastybits
 * @link 	www.tastybits.io
 */
using UnityEngine;
using System.Collections;
using Tastybits.CamCap;

/**
  * @Desc This class contains extensions to WebCamTexture, we want you to still be able to just use
  * the built-in WebCamTexture class and use the new functionality with an existing project using WebCamTexture.
  */
public static class WebCamTextureExt {


#if !UNITY_EDITOR && UNITY_IPHONE	
	public static string GetPointer( this WebCamTexture self ) {
		if( string.IsNullOrEmpty(self.name) || string.IsNullOrEmpty(self.name.Trim()) ) {
			throw new System.Exception("invalid string is empty - not a pointer");
		}
		if( !self.name.ToLower().StartsWith("0x") ) {
			throw new System.Exception("invalid string: " + self.name + " not a pointer");
		}
		return self.name;
	}
#endif

	public static bool HasFlashMode( this WebCamTexture self, FlashModes mode ) {
#if !UNITY_EDITOR && UNITY_IPHONE	
		return Tastybits.CamCap.CameraCapture.GetFlashModeSupported( self, mode );
#elif !UNITY_EDITOR && UNITY_ANDROID	
		return CameraCapture.Android_GetFlashModeSupported( mode );
#else
		return false;
#endif
	}

	public static void SetFlashMode( this WebCamTexture self, FlashModes mode ) {
#if !UNITY_EDITOR && UNITY_IPHONE	
		Tastybits.CamCap.CameraCapture.SetFlashMode( self, mode );
#elif !UNITY_EDITOR && UNITY_ANDROID
		CameraCapture.Android_SetFlashMode( (int)mode );
#else
#endif
	}

	// Eyecandy
	public static void CaptureStillImage( this WebCamTexture self, CaptureMode captureMode, System.Action<bool> callback ) {
		Tastybits.CamCap.CameraCapture.CaptureStillImage( self, captureMode, callback );
	}

	// Eyecandy
	public static Texture2D GetBufferAndStopWebCamTexture(this WebCamTexture self, float rotationAngle, CopyRotate rotate, CopyFlip flip ) {
		return CameraCapture.GetBufferAndStopWebCamTexture( self, rotationAngle, rotate, flip );
	}


	public static bool GetHasTorch( this WebCamTexture self ) {
#if !UNITY_EDITOR && UNITY_IPHONE
		return CameraCapture.GetHasTorch( self );
#elif !UNITY_EDITOR && UNITY_ANDROID	
		return CameraCapture.Android_HasTorch();
#else
		return false;
#endif
	}


	public static bool IsTorchEnabled( this WebCamTexture self ) {
#if !UNITY_EDITOR && UNITY_IPHONE
		return CameraCapture.UnityGetTorchMode( self ) == TorchModes.On;
#elif !UNITY_EDITOR && UNITY_ANDROID	
		return CameraCapture.Android_IsTorchEnabled();
#else
		return false;
#endif
	}


	public static FlashModes GetFlashMode( this WebCamTexture self ) {
#if !UNITY_EDITOR && UNITY_IPHONE
		return (FlashModes)CameraCapture.GetFlashMode( self );
#elif !UNITY_EDITOR && UNITY_ANDROID	
		return (FlashModes)CameraCapture.Android_GetFlashMode();
#else
		return FlashModes.Off;
#endif
	}


	public static void SetTorchEnabled( this WebCamTexture self, bool _value ) {
#if !UNITY_EDITOR && UNITY_IPHONE
		CameraCapture.SetTorch( self, _value, ( bool ok )=>{
			if(!ok)Debug.LogError("Error enabling torch");
		} );
#elif !UNITY_EDITOR && UNITY_ANDROID	
		CameraCapture.Android_SetTorchEnabled(_value);
#else
		Debug.LogError("SetTorchEnabled: not available on platform :" + Application.platform );
#endif
	}


	public static bool IsFocusSupported( this WebCamTexture self ) {
#if !UNITY_EDITOR && UNITY_IPHONE
		Debug.LogError("IsFocusSupposed not implemneted for iOS");
		return true;
#elif !UNITY_EDITOR && UNITY_ANDROID	
		return CameraCapture.Android_IsFocusSupported();
#else
		Debug.LogError("IsFocusSupported: not available on platform :" + Application.platform );
		return false;
#endif
	}


	public static string GetISOValue( this WebCamTexture self ) {
#if !UNITY_EDITOR && UNITY_IPHONE
		return CameraCapture.GetISO( self );
#elif !UNITY_EDITOR && UNITY_ANDROID	
		return CameraCapture.GetISO();
#else
		Debug.LogError("GetISOValue not available on platform: " + Application.platform );
		return "";
#endif
	}


	public static void SetISOValue( this WebCamTexture self, string isoValue ) {
#if !UNITY_EDITOR && UNITY_IPHONE
		CameraCapture.SetISO( self, isoValue );
#elif !UNITY_EDITOR && UNITY_ANDROID	
		CameraCapture.SetISO( isoValue );
#else
		Debug.LogError("SetISOValue not available on platform: " + Application.platform );
		return;
#endif
	}


	static bool GetSupportedISOValues_ErrorOnce = false;
	public static string[] GetSupportedISOValues( this WebCamTexture self ) {
#if !UNITY_EDITOR && UNITY_IPHONE
		return CameraCapture.GetSupportedISOValues( self );
#elif !UNITY_EDITOR && UNITY_ANDROID	
		return CameraCapture.GetSupportedISOValues();
#else
		if( Application.isEditor ) {
			if( !GetSupportedISOValues_ErrorOnce ) {
				GetSupportedISOValues_ErrorOnce=true;
				if( CameraCaptureController.Verbose ) 
					Debug.Log("GetSupportedISOValues not available on platform: " + Application.platform );
			}
		} else {
			Debug.LogError("GetSupportedISOValues not available on platform: " + Application.platform );
		}
		return new string[]{};
#endif	
	}


	public static void EnableShutterSound( this WebCamTexture self, bool enableShutterSound ) {
#if !UNITY_EDITOR && UNITY_IPHONE
		CameraCapture.EnableShutterSound( self, enableShutterSound );
#elif !UNITY_EDITOR && UNITY_ANDROID	
		CameraCapture.EnableShutterSound( enableShutterSound );
#else
		Debug.LogError("EnableShutterSound: not available on platform :" + Application.platform );
		return;
#endif	
	}


	public static bool CanEnableAndDisableShutterSound( this WebCamTexture self ) {
#if !UNITY_EDITOR && UNITY_IPHONE
		return false;
#elif !UNITY_EDITOR && UNITY_ANDROID	
		return CameraCapture.CanEnableAndDisableShutterSound(  );
#else
		return false;
#endif	
	}


	public static bool IsAutoExposureLockSupported( this WebCamTexture self ) {
#if !UNITY_EDITOR && UNITY_IPHONE
		return CameraCapture.IsAutoExposureLockSupported( self );
#elif !UNITY_EDITOR && UNITY_ANDROID	
		return CameraCapture.IsAutoExposureLockSupported(  );
#else
		return false;
#endif	
	}

	public static void SetAutoExposureLock( this WebCamTexture self, bool value ) {
#if !UNITY_EDITOR && UNITY_IPHONE
		CameraCapture.SetAutoExposureLock( self, value );
#elif !UNITY_EDITOR && UNITY_ANDROID	
		CameraCapture.SetAutoExposureLock( value );
#else
		Debug.LogError("SetAutoExposureLock: not available on this platform : " +Application.platform );
#endif	
	}

	public static void GetAvailableExposureCompensationRange( this WebCamTexture self, out int minval, out int maxval  ) {
#if !UNITY_EDITOR && UNITY_IPHONE
		CameraCapture.GetMinMaxExposure( self, out minval, out maxval );
#elif !UNITY_EDITOR && UNITY_ANDROID	
		CameraCapture.GetMinMaxExposure( out minval, out maxval );
#else
		minval = 0;
		maxval = 0;
#endif	
	}


	public static void SetExposureCompensation( this WebCamTexture self, int value ) {
#if !UNITY_EDITOR && UNITY_IPHONE
		CameraCapture.SetExposureCompensation( self, value );
#elif !UNITY_EDITOR && UNITY_ANDROID	
		CameraCapture.SetExposureCompensation( value );
#else
		Debug.LogError("SetExposureCompensation: not available on this platform : " +Application.platform );
#endif	
	}


	// This is highly experimental 
	// It's a potential fix for a problem some people are experiemenzing on an LG Phone mode. The picture is is very dark and the
	// image has been taken. 
#if UNITY_ANDROID
	public static void ApplyLGFix( this WebCamTexture self ){ 
	#if !UNITY_EDITOR
		CameraCapture.ApplyLGFix();
	#endif
	}
#endif


}
