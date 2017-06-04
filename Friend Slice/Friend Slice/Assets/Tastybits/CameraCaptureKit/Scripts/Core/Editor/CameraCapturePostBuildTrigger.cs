/**
 * @desc 	Class to handle postprocess build additions primarily on iOS
 * 
 * @author 	Tastybits
 * @link 	www.tastybits.io
 */
using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
#if UNITY_IPHONE
using UnityEditor.iOS.Xcode;
#endif

public class PostBuildTrigger : MonoBehaviour {

	[PostProcessBuild]
	public static void OnPostprocessBuild( BuildTarget buildTarget, string path ) {
		if( buildTarget != BuildTarget.iOS ) {
			return;
		}

#if UNITY_IPHONE

		//Debug.Log ("Post processing iOS build" );
		Debug.Log("Removing CameraCapture.mm/.h from project");
		string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
		PBXProject pbx= new UnityEditor.iOS.Xcode.PBXProject();
		pbx.ReadFromFile( projPath );
		var guid1 = pbx.FindFileGuidByProjectPath( "Classes/Unity/CameraCapture.mm" );
		var guid2 = pbx.FindFileGuidByProjectPath( "Classes/Unity/CameraCapture.h" );

		string target = pbx.TargetGuidByName("Unity-iPhone");
		pbx.AddFrameworkToProject( target, "CoreImage.framework", true );

		pbx.RemoveFile( guid1 );
		pbx.RemoveFile( guid2 );

		//pbx.AddFrameworkToProject( "asdsad", "asdasd", true );
		pbx.WriteToFile(projPath);


		// Get plist

		// This is only if you want to control the default Statusbar behaviour
		bool statusBarModuleEnabled= false;
		#if STATUSBAR_MODUlE
			statusBarModuleEnabled = true;
		#endif
		string plistPath = path + "/Info.plist";
		PlistDocument plist = new PlistDocument();
		plist.ReadFromString(File.ReadAllText(plistPath));
		PlistElementDict rootDict = plist.root;

		bool statusBarHidden = EditorPrefs.GetBool("CameraCapture_StatusbarHidden", PlayerSettings.statusBarHidden );
		bool manuallyControlStatusbar = EditorPrefs.GetBool("CameraCapture_StatusbarManuallyControl", true );

		Debug.Log("CameraCaptureKit: Setting statusbar settings; statusBarHidden="+statusBarHidden+ " UIViewControllerBasedStatusBarAppearance=" +manuallyControlStatusbar   );

		if( statusBarModuleEnabled ) {
			rootDict.SetBoolean( "UIViewControllerBasedStatusBarAppearance", manuallyControlStatusbar ); // Unity is configured to control the status bar per viewcontroller.
			rootDict.SetBoolean( "UIStatusBarHidden", statusBarHidden );  // in order to controlt he statusbar it must be visible upon launch.
		} else {
			PlayerSettings.statusBarHidden = EditorPrefs.GetBool("CameraCapture_StatusbarHidden", PlayerSettings.statusBarHidden );
			rootDict.SetBoolean( "UIStatusBarHidden", statusBarHidden );  // in order to controlt he statusbar it must be visible upon launch.
			rootDict.SetBoolean( "UIViewControllerBasedStatusBarAppearance", manuallyControlStatusbar ); // Unity is configured to control the status bar per viewcontroller.
		}

		File.WriteAllText(plistPath, plist.WriteToString());
#endif

	}


}
