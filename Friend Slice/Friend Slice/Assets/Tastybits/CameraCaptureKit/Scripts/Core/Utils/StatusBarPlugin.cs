/**
 * @desc 	StatusBar plugin for iOS which makes it possible to show or hide a statusbar.
 * 
 * @author 	Tastybits
 * @link 	www.tastybits.io
 */
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

#if STATUSBAR_MODULE

// Currently this is iPhone only.
public class SystemBarHelper {
#if !UNITY_EDITOR && UNITY_IPHONE
	[DllImport ("__Internal")]
	private static extern void _SystemBarHelper_setStatusBarHidden( bool value );
#endif

	public static void setStatusBarHidden( bool value ) {
#if !UNITY_EDITOR && UNITY_IPHONE
		_SystemBarHelper_setStatusBarHidden( value );
#else
		//Debug.Log("only available for iOS");
#endif
	}


}

#endif 

public class Statusbar {
	static bool _Vis = true;
	public static bool Visible {
		set {
#if STATUSBAR_MODULE
			SystemBarHelper.setStatusBarHidden(!value);

#endif
			_Vis = value;
		}
		get {
			return _Vis;
		}
	}
}


