using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;


namespace Tastybits.CamCap {
	

	public static class CameraPermissions {


		public enum AVAuthorizationStatus {
			Error = -1,
			Undefined = 0,
			AVAuthorizationStatusAuthorized = 1,
			AVAuthorizationStatusNotDetermined = 2,
			AVAuthorizationStatusRestricted = 3,
			AVAuthorizationStatusDenied = 4,
			Last
		}


#if UNITY_IOS
		[DllImport ("__Internal")]
		static extern int _CameraPermissions_AquireAuth( string callbackobj );
#endif


		public static void GetPermissions( System.Action<AVAuthorizationStatus> cb ){
#if UNITY_IOS && !UNITY_EDITOR
			string cb_Name = 
			Tastybits.CamCap.NativeDelegate.Create( ( Hashtable hash) => {
				bool granted = false;
				if( hash.ContainsKey("granted") ) {
					granted = (bool)System.Convert.ToBoolean( hash["granted"] );
				} else {
					Debug.LogError( "Response does not contain a granted result" );
				}
				if( granted ) {
					cb( AVAuthorizationStatus.AVAuthorizationStatusAuthorized );
				} else {
					cb( AVAuthorizationStatus.AVAuthorizationStatusDenied );
				}
			} );
			int ret = _CameraPermissions_AquireAuth ( cb_Name );
			if( ret == -1 ) {
				Debug.LogError("Error there is already a request to aquire permissions");
				cb(AVAuthorizationStatus.Error);
				return;
			}
			if ((int)ret >= (int)AVAuthorizationStatus.Undefined && (int)ret < (int)AVAuthorizationStatus.Last ) {
				Debug.Log("CameraPermissions: Current permission state : " + ret );
				var ret_cast = (AVAuthorizationStatus)ret;
				if (ret_cast == AVAuthorizationStatus.AVAuthorizationStatusNotDetermined || 
					ret_cast == AVAuthorizationStatus.AVAuthorizationStatusRestricted ||
					ret_cast == AVAuthorizationStatus.AVAuthorizationStatusDenied  ) 
				{
					Debug.Log( "Waiting for permissions to be determined." );
					return;
				}

				// Delete the callback object there is no need for it.
				var go = GameObject.Find (cb_Name);
				if (go != null) {
					GameObject.DestroyImmediate(go);
				}

				// Delete the cast object..
				cb( ret_cast );
				return;
			}
			Debug.LogError ("Permissions: Invalid return from native API");
			cb (AVAuthorizationStatus.Undefined);
#else
			if( Application.isEditor ) {
				cb( AVAuthorizationStatus.AVAuthorizationStatusAuthorized );
				return;
			}
			if( Application.platform == RuntimePlatform.Android ) {
				cb( AVAuthorizationStatus.AVAuthorizationStatusAuthorized );
				return;
			}
			Debug.LogError( "Not implemented for platform: " + Application.platform );
			cb( AVAuthorizationStatus.Undefined );
#endif
		}




	}

}