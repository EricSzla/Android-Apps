using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
using Tastybits.CamCap;


public class CameraController : MonoBehaviour
{
	enum State {
		Wait = -1,
		Begin = 0,
		Capturing = 1,
		ShowResult = 3
	}
			
	public Texture2D finalImage;
	State state=State.Begin;
	public CameraCaptureUI cameraCaptureUI;

	void Awake() {
		Screen.orientation = ScreenOrientation.Portrait;
		if( cameraCaptureUI == null ) {
			Debug.LogError("CameraCaptureUI of CameraCaptureDemo was not specified.");
		}
	}

	void Start(){
		Screen.orientation = ScreenOrientation.Portrait;
		state = State.Begin;
		finalImage = null;
		cameraCaptureUI.gameObject.SetActive(false);
		OnStartCameraClicked ();

	}

	void Update() {
		if( state==State.Begin ) {
			finalImage = null;
		} else if( state == State.ShowResult ) {
			Debug.Log ("Taking a picture");
			//mCamera.Stop ();
			Debug.Log ("Encoding bytes to png");
			//Encode to png
			byte[] bytes = finalImage.EncodeToPNG();
			Debug.Log ("TakeAPicture - Done encoding file");
			StartCoroutine(SavePictureToDatabase (bytes));
			//TODO: CALL THE CLASS
		}
	}

	void OnPhotoCaptured( Texture2D tex ) {
		Debug.Log("DEMO: OnPhotoCaptured" );
		state = State.ShowResult;	
		finalImage = tex;
	}


	public void OnStartCameraClicked(){
		state = State.Wait;
		cameraCaptureUI.OnPhotoCaptured = this.OnPhotoCaptured;
		cameraCaptureUI.SetActiveAndStartCapture( true, ( bool ok )=>{
			state = ok ? State.Capturing : State.Begin;
			if( ! ok )  {
				Debug.LogError("Error starting camera.");
			} else {
			}
		});
	}

	// For testing.
	void DrawDebugLabel() {
		var webCamTexture = cameraCaptureUI.controller.WebCamTexture;
		int vra = -1;
		int wi = -1;
		int he = -1;
		int rw = -1;
		int rh = -1;
		if( webCamTexture!=null ) {
			vra=webCamTexture.videoRotationAngle;
			wi = webCamTexture.width;
			he = webCamTexture.height;
			rw = webCamTexture.requestedWidth;
			rh = webCamTexture.requestedHeight;
		}
		bool isBack = cameraCaptureUI.controller.IsCurrentBackFacing()==false;
		GUI.Label( new Rect(0f,Screen.height-30f,Screen.width,30f), "VRA:"+ vra + " BackF:"+isBack + " WebCamTexDims:" + wi +  "," + he + " reqWH=" + rw+"," +rh  );
	}

	// GUIStyle used when drawing describtion label.
	GUIStyle labelStyle;
	GUIStyle LabelStyle {
		get {
			if( labelStyle==null ) {
				labelStyle = new GUIStyle();
				labelStyle = GUI.skin.label;
				labelStyle.richText=true;
				labelStyle.fontStyle = FontStyle.Bold;
				labelStyle.alignment = TextAnchor.MiddleCenter;
				var _tex = new Texture2D(2,2); 
				_tex.SetPixels( new Color[]{new Color(0,0,0,0.25f),new Color(0,0,0,0.25f),new Color(0,0,0,0.25f),new Color(0,0,0,0.25f) } );
				_tex.name = "Black TEx";
				_tex.Apply();
				labelStyle.normal.background = _tex;

				labelStyle.fontSize = 15;
				if( Screen.width > 1000 || Screen.height > 1000 ) {
					labelStyle.fontSize = 30;
				}

			}
			return labelStyle;
		}
	}

	// The style of the buttons used.
	GUIStyle buttonStyle;
	GUIStyle ButtonStyle {
		get {
			if( buttonStyle == null ) {
				buttonStyle = new GUIStyle( GUI.skin.button );
				int newFontSz = 8;
				buttonStyle.fontSize = buttonStyle.fontSize * newFontSz; 
			}
			return buttonStyle;
		}
	}

	IEnumerator SavePictureToDatabase(byte[] picBytes){
		int prefab = PlayerPrefs.GetInt("pic");
		string what = string.Empty;
		if(prefab == 1){
			what = "happy";
		}else if(prefab == 2){
			what = "sad";
		}else if(prefab == 3){
			what = "angry";
		}


		Debug.Log ("Saving Picture To Database");
		var form = new WWWForm(); //here you create a new form connection
		var URL = "http://erpamdevelopment.com/Apps/FriendSlice/AppScripts/UploadSelfie.php"; //change for your URL
		form.AddField( "fbId", AccessToken.CurrentAccessToken.UserId ); //add your hash code to the field myform_hash, check that this variable name is the same as in PHP file
		form.AddField( "selfieString",what);
		form.AddBinaryData ("selfie", picBytes);
		var w = new WWW(URL, form); //here we create a var called 'w' and we sync with our URL and the form
		yield return w; //we wait for the form to check the PHP file, so our game dont just hang
		if (w.error != null) {
			print(w.error); //if there is an error, tell us
		} else {
			print("Test ok");
			string userFound = w.text; //here we return the data our PHP told us
			w.Dispose(); //clear our form in game
			Debug.Log(userFound);

			if(userFound.StartsWith("1")){
				Debug.Log("Picture saved");
				// Start the selfie scene
				SceneManager.LoadScene ("Selfies");
			}else{
				Debug.Log("Error, picture not saved");
				// Start the main game
				SceneManager.LoadScene ("CameraScene");
			}
		}
		Debug.Log ("End of saving picture function");

	}
}
