using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;


public class CameraCaptureConfig : EditorWindow {
	[MenuItem("Window/Tastybits/Camera Capture Kit/Camera Capture Config")]
	public static void _() {
		var wnd = EditorWindow.GetWindow<CameraCaptureConfig>();
		wnd.titleContent = new GUIContent ("Camera Capture");
		wnd.Show( );
	}

	public bool enableStatusBar {
		get {
			string str = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
			bool ret= str.Split( new char[]{';'} ).
				Contains("STATUSBAR_MODULE");
			//Debug.Log("statusbar module enabled :"  + ret);
			return ret;
		}
	}

	public void OnGUI(){
		var newValue = EditorGUILayout.ToggleLeft( "Enable Statusbar Module (iOS)", enableStatusBar );
		GUILayout.TextArea("On iOS you might want to display the camera in fullscreen and toggeling the statusbar on/off. Click this on to enable the Statusbar.Visible = true/false functionality" );
		if( !enableStatusBar && newValue) {
			var symbols = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
			var comps = symbols.Split(new char[]{';'});
			if( comps.ToList().Contains( "STATUSBAR_MODULE" ) == false ) {
				var tmp = comps.ToList();
				tmp.Add( "STATUSBAR_MODULE" );
				var strsymbols = string.Join( ";", tmp.ToArray() );
				UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS,strsymbols);
				UnityEditor.AssetDatabase.Refresh( ImportAssetOptions.ForceUpdate );
			}
		} else if( enableStatusBar && !newValue ) {
			var symbols = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
			var comps = symbols.Split(new char[]{';'});
			if( comps.ToList().Contains( "STATUSBAR_MODULE" )  ) {
				var tmp = comps.ToList();
				int iter = 0;
				while( tmp.Remove( "STATUSBAR_MODULE" ) && iter++<10 ) {
				}
				if(iter >= 10 ) Debug.LogError("overflow");
				var strsymbols = string.Join( ";" , tmp.ToArray() );
				UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS,strsymbols);
				UnityEditor.AssetDatabase.Refresh( ImportAssetOptions.ForceUpdate );
			}
		}
	}
}
