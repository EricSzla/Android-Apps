using System;

namespace Tastybits.CamCap {

	// 
	public enum FlashModes {
		Off    			= 0, // Flash is turned of  			- AVCaptureFlashModeOff    = 0,
		On 	    		= 1, // Flash is on						- AVCaptureFlashModeOn     = 1, 
		FlashModeAuto   = 2  // Turns on when its dark.			- AVCaptureFlashModeAuto   = 2
	}


	public enum TorchModes {
		Off    			= 0, // Torch is turned of				- AVCaptureTorchModeOff
		On 	    		= 1, // Torch is on						- AVCaptureTorchModeOn = 1
		Auto 		    = 2  // Turns on when its dark.			- AVCaptureTorchModeAuto = 2
	}

	public enum FocusModes {
		Off 	= 0,		// doesn't focus 								- AVCaptureFocusModeLocked                = 0,
		Autofocus = 1,		// automaticly focus 						  	- AVCaptureFocusModeAutoFocus             = 1,			
		ContAutoFocus = 2	// keeps refocusing ( again and again )	 		- AVCaptureFocusModeContinuousAutoFocus   = 2,
	}


	public enum AndroidFocusModes {
		FOCUS_MODE_AUTO = 0,
		FOCUS_MODE_CONTINUOUS_PICTURE,
		FOCUS_MODE_CONTINUOUS_VIDEO
	}


	public enum CaptureMode {
		BestCaptureQuality = 0,
		QuickCapture = 1,
	} 


}

