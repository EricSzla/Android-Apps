/**
 * @desc 	Utility class to invoke a System.Action<Hashtable> from iOS with a serialized JSON struct 
 * 			Which make it possible to send more advanced data over.
 * 
 * @author 	Tastybits
 * @link 	www.tastybits.io
 */
using UnityEngine;
using System.Collections;


namespace Tastybits.CamCap {


	public class NativeDelegate : MonoBehaviour {
		public string randomId;
		bool invoked = false;
		float timeout=-1337f;
		System.Action< System.Collections.Hashtable > deleg;

		public static string Create( System.Action< System.Collections.Hashtable > methodToCall ) {
			int rndId = Random.Range( 100000, 999999 );
			int iter = 0;
			string go_name = "NativeDelegate_" + rndId;

			while( GameObject.Find( go_name ) != null && iter++ < 100 ) {
				if( iter >= 100 ) {
					Debug.LogError("Error infinite loop might have been detected, or random funtion might not return random numbes");
				}
				rndId = Random.Range( 100000, 999999 );
				go_name = "NativeDelegate_" + rndId;
			}

			if( GameObject.Find( go_name ) != null ) {
				Debug.LogError( "Error there is allready a callback with the existing id : " + go_name );
			}

			GameObject delegRoot = GameObject.Find( "NativeDelegates" );
			if( delegRoot == null ) {
				delegRoot = new GameObject( "NativeDelegates");
			}

			GameObject goDeleg = new GameObject( go_name );
			goDeleg.name = go_name;
			goDeleg.transform.parent = delegRoot.transform;
			NativeDelegate test = goDeleg.AddComponent<NativeDelegate>() as NativeDelegate;
			test.deleg = methodToCall;

			return test.name;
		}


		void Update() {
			if( !(timeout <= -1337 ) ) {
				if( timeout > 0f ) {
					timeout -= Time.fixedTime;
					if( timeout <= 0f ) {
						OnTimeout();
					}
				}
			}
		}


		void OnTimeout() {

		}


		public void CallDelegateFromNative( string strData ) {
			if( invoked ) {
				Debug.LogError( "Error the delegate is allready invoked" );
				return;
			}

			// 	
			System.Collections.Hashtable retParams = Tastybits.CamCap.JSON.Deserialize( strData ) as System.Collections.Hashtable;

			/*string strTmp = "{ \n"+
				"\"succeeded\" : \"false\", \n"+
				"\"path\" : \"\", \n" +
				"\"cancelled\" : \"true\" \n"+
			"}\n";*/

			if( retParams == null ) {
				Debug.LogError( "Error parsing ret" );
				return;
			}

			Debug.Log ( "NativeDelegate: Str= " + retParams + " str data = " + strData );

			deleg( retParams );
			this.enabled = false;
			this.gameObject.SetActive( false );
			GameObject.DestroyObject( this.gameObject ); // <- destroy this game object. to be sure we dont call it anymore
		}



	}

}