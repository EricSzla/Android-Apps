===========================================================================
  Camera Capture Kit 
===========================================================================

Capture Capture Kit gives you a foundation for capturing Photos and still images in your project.

It enables the functionality related to take a still image such as :
 + Anti-shake.
 + Flashlight / Torch.
 + Auto focus adjust.

The package contains a Unity UI prefab of a Camera App which can be used directly in a project as an example
on how the functionality can be integrated.

The plugin works as an extension based around WebCamTexture and works by extending with native functioanlity. If native functionality is not added
it will fall back on the default WebCamTexture behaviour.


===========================================================================
 How to use
===========================================================================

Check out the DemoScene called CameraCaptureUIDemo placed in Assets/Tastybits/CameraCaptureKit/Demo here the basic functionality is 
shown how to utilisize the camera.


===========================================================================
  Coding guidelines.
===========================================================================

To use the camera features in another project drag the CameraCaptureUI prefab residing in Tastybits/CameraCaptureKit/Prefabs
into an already existing canvas , make it deactivated by default. Save a referance to the CameraCaptureUI component in a 
controller class somewhere. 

When you want to start capturing a photo activate the CamreaCaptureUI by calling CamreaCaptureUIController.SetActiveAndStartCapture

Such an example can be seen in the CameraCaptureUI demo where the capturing starts when the user clicks an OnGUI button. ( Check out the scene Tastybits/CameraCaptureKit/Demo/CameraCaptureUIDemo.unity)

-

If you don't want to use the component using unity's UI system you can also integrate the capturing system directly using the component CameraCaptureController
an exampel of how this is done can be seen in CameraCaptureOnGUIDemo. ( Check out the scene Tastybits/CameraCaptureKit/Demo/CameraCaptureOnGUIDemo.unity)


===========================================================================
 How does it work ? (behind the curtains)
===========================================================================

CameraCaptureKit extends the Unity component called WebCamTexture by adding native plugins in order to turn on things like Auto-focus and Flash.

For iOS the source to the native plugin resides in CameraCapture.mm and on Android the source resides within AndroidPlugin.zip


===========================================================================
  Feedback & support
===========================================================================

If you want to see more features or better Windows Phone and/or Android integration please don't hessitate to write us and we can help you 
to implement it or we will update the plugin with the functionality. (tastybits8@gmail.com)





