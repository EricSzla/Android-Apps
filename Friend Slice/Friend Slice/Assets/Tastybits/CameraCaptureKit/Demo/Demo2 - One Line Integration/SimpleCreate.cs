using UnityEngine;
using System.Collections;
using Tastybits.CamCap;

/*
	==================================================================
	  Simple Camera Create 
 	==================================================================

	This example shows you how to open the CameraUI and grab a snapshot without setting anything up.
*/
public class SimpleCreate : MonoBehaviour {
	public UnityEngine.UI.RawImage resultImage;
	/**
	 * Callback invoked by the 
	 */
	public void OnStartCameraButtonClicked(){
		CameraCaptureUI.CreateUIAndCaptureImage( ( Texture2D capturedPhoto ) => {
			resultImage.texture = capturedPhoto;
		} );
	}
}




