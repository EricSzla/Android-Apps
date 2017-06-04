using UnityEngine;
using System.Collections;

// This cass makes it possible to tweak what functionality the cameraUI prefab will utilize when 
// running.
public class CameraCaptureUIDemoSettings : MonoBehaviour {
	public Tastybits.CamCap.CameraCaptureUI cameraUI;
	public bool EnableAdjustExposureIfAvail=true;
	public bool EnableAdjustISOIfAvail=true;
	public bool EnableAdjustFlashIfAvailable = true;
	public bool EnableFlipCameraIfAvailable = true;
	public Tastybits.CamCap.CameraCaptureController.PreferCameraDirection PreferedDirection = Tastybits.CamCap.CameraCaptureController.PreferCameraDirection.FrontFacing;
	void Awake(){
		cameraUI.EnableAdjustExposureIfAvail = EnableAdjustExposureIfAvail;
		cameraUI.EnableAdjustISOIfAvail = EnableAdjustISOIfAvail;
		cameraUI.EnableAdjustFlashIfAvailable = EnableAdjustFlashIfAvailable;
		cameraUI.EnableFlipCameraIfAvailable = EnableFlipCameraIfAvailable;
		cameraUI.controller.SetPreferedCameraDirection( PreferedDirection );
	}
}
