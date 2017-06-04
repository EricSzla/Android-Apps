using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using UnityEngine.UI;
using Facebook.Unity;
using System.Text;
using LitJson;
using Tastybits.CamCap;

public class SelfieManager : MonoBehaviour {

	#region Variables
	public static SelfieManager Instance{ set; get; }
	public GameObject happyButton;
	public GameObject sadButton;
	public GameObject angryButton;
	public Text saveButtonText;
	public Text popUpText;
	public GameObject popUpPanel;
	private float popUpTime;
	public Text headerText;
	bool savingState = false;
	bool error = false;

	private int picInt = -1;
	bool saveAllowed = false;
	byte[] happyBytes;
	byte[] sadBytes;
	byte[] angryBytes;

	enum State {
		Wait = -1,
		Begin = 0,
		Capturing = 1,
		ShowResult = 3
	}
	//Camera variables
	public Texture2D finalImage;
	State state=State.Begin;
	public CameraCaptureUI cameraCaptureUI;
	#endregion Variables

	#region Game Methods
	// Use this for initialization
	void Start () {
		popUpTime = Time.time;
		popUpPanel.SetActive (false);
		/*PlayerPrefs.SetInt ("happyExist", 0);
		PlayerPrefs.SetInt ("sadExist", 0);
		PlayerPrefs.SetInt ("angryExist", 0);
		PlayerPrefs.SetInt ("isEditing", 0);*/
		Screen.orientation = ScreenOrientation.Portrait;
		state = State.Begin;
		finalImage = null;
		cameraCaptureUI.gameObject.SetActive (false);

		if (PlayerPrefs.GetInt ("isEditing") == 1) {
			headerText.text = "Choose new selfies ! ";
		} else {
			headerText.text = "Take three selfies!\nHappy, Sad and ANGRY ! ";
		}
	}

	void Awake() {
		Instance = this;
		Screen.orientation = ScreenOrientation.LandscapeRight;
		if( cameraCaptureUI == null ) {
			Debug.LogError("CameraCaptureUI of CameraCaptureDemo was not specified.");
		}
	}

	void Update() {

		
		if ((popUpPanel.activeSelf && (Time.time - popUpTime > 2f)) || savingState) {
			if (savingState) {
				popUpPanel.SetActive (true);
				if (!error) {
					popUpText.text = "Saving, please wait ... ";
				} else {
					popUpText.text = "Some problems occured, check your network and try again. ";
				}
				popUpTime = Time.time;
			} else {
				popUpPanel.SetActive (false);
			}
		}

		if( state==State.Begin ) {
			finalImage = null;
		} else if( state == State.ShowResult ) {
			Debug.Log ("Taking a picture");

			if (picInt == 1) {
				happyBytes = finalImage.EncodeToPNG ();
				ConvertToSprite (finalImage, happyButton);
				PlayerPrefs.SetInt ("happyExist", 1);
				Debug.Log ("After pic 1");
			} else if (picInt == 2) {
				sadBytes = finalImage.EncodeToPNG ();
				ConvertToSprite (finalImage, sadButton);
				PlayerPrefs.SetInt ("sadExist", 1);
				Debug.Log ("After pic 2");
			} else if (picInt == 3) {
				angryBytes = finalImage.EncodeToPNG ();
				ConvertToSprite (finalImage, angryButton);
				PlayerPrefs.SetInt ("angryExist", 1);
				Debug.Log ("After pic 3");
			}

				
			int hexists = PlayerPrefs.GetInt ("happyExist");
			int sadExists = PlayerPrefs.GetInt ("sadExist");
			int aexists = PlayerPrefs.GetInt ("angryExist");

			if (PlayerPrefs.GetInt ("isEditing") == 1) {
				saveAllowed = true;
			}

			if ((hexists == 1 && sadExists == 1 && aexists == 1) || saveAllowed) {
				saveAllowed = true;
				if (saveButtonText != null) {
					saveButtonText.color = UnityEngine.Color.black;
				}

				if (hexists == 1) {
					PlayerPrefs.SetInt ("happyExist", 0);
					PlayerPrefs.SetInt ("sadExist", 0);
					PlayerPrefs.SetInt ("angryExist", 0);
				}

			} else {
				saveAllowed = false;
				if (saveButtonText != null) {
					saveButtonText.color = UnityEngine.Color.grey;
				}
			}
			state = State.Begin;
			cameraCaptureUI.gameObject.SetActive(false);

			Debug.Log ("Finished");
		}


	}

	#endregion Game Methods

	#region Camera
	void OnPhotoCaptured( Texture2D tex ) {
		Debug.Log("DEMO: OnPhotoCaptured" );
		state = State.ShowResult;	
		finalImage = tex;
		TextureScale.Point (finalImage, 128, 128);
	}


	public void OnStartCameraClicked(){
		state = State.Wait;
		cameraCaptureUI.OnPhotoCaptured = this.OnPhotoCaptured;
		cameraCaptureUI.SetActiveAndStartCapture( false, ( bool ok )=>{
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

	#endregion Camera

	#region Pictures
	IEnumerator SavePicturesToDatabase(){
		error = false;
		Debug.Log ("Saving Picture To Database");
		var form = new WWWForm (); //here you create a new form connection
		var URL = "http://erpamdevelopment.com/Apps/FriendSlice/AppScripts/UploadSelfie.php"; //change for your URL
		form.AddField ("fbId", AccessToken.CurrentAccessToken.UserId); //add your hash code to the field myform_hash, check that this variable name is the same as in PHP file
		form.AddBinaryData ("happy", happyBytes);
		form.AddBinaryData ("sad", sadBytes);
		form.AddBinaryData ("angry", angryBytes);
		var w = new WWW (URL, form); //here we create a var called 'w' and we sync with our URL and the form
		yield return w; //we wait for the form to check the PHP file, so our game dont just hang
		if (w.error != null) {
			print (w.error); //if there is an error, tell us
			error = true;
		} else {
			print ("Test ok");
			string userFound = w.text; //here we return the data our PHP told us
			w.Dispose (); //clear our form in game
			Debug.Log (userFound);

			if (userFound.StartsWith ("1")) {
				Debug.Log ("Picture saved");
			} else {
				Debug.Log ("Error, picture not saved");
				error = true;
			}
		}
		Debug.Log ("End of saving picture function");

		if (!error) {
			savingState = false;
			ToMenu ();
		}
	}

	public void SavePictures(){
		if (saveAllowed) {
			StartCoroutine (SavePicturesToDatabase ());
			PlayerPrefs.SetInt ("isEditing", 1);
			savingState = true;
			saveAllowed = false;
		} else if (savingState) {
			popUpPanel.SetActive (true);
			popUpText.text = "Saving please wait... ! ";
			popUpTime = Time.time;
		}else {
			popUpPanel.SetActive (true);
			popUpText.text = "All three selfies are required ! ";
			popUpTime = Time.time;
		}
	}

	public static Texture2D LoadPNG(byte[] fileData) {

		Texture2D tex = null;	
		tex = new Texture2D(2, 2);
		tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
		return tex;
	}


	public void ConvertToSprite(Texture2D pic1,GameObject button){
		//Rect rec = new Rect(0, 0, pic1.width, pic1.height);
		//Sprite sprite = Sprite.Create(pic1,rec,new Vector2(0.5f,0.5f),100);
		Rect rec = new Rect(28, 0, 80 , 128);
		Sprite sprite = Sprite.Create(pic1,rec,new Vector2(0.5f,0.5f),1024);
		button.GetComponent<UnityEngine.UI.Image> ().sprite = sprite;
		//button.GetComponent<UnityEngine.UI.Image> ().overrideSprite = sprite;
	}

	#endregion Pictures

	#region Buttons

	public void ToMenu(){
		SceneManager.LoadScene ("MainMenu");
	}

	public void TakeASelfie(int i){
		picInt = i;
		OnStartCameraClicked ();
	}

	#endregion Buttons

	#region ExtraCode
	/*IEnumerator GetSelfiesFromTheDatabase(string what, GameObject button){
		Debug.Log ("Getting Picture From Database");
		var form = new WWWForm(); //here you create a new form connection
		var URL = "http://erpamdevelopment.com/Apps/FriendSlice/AppScripts/GetSelfie.php"; //change for your URL
		form.AddField("fbId", PlayerPrefs.GetString("fbId"));
		form.AddField ("selfieString", what);
		form.AddField ("check", "0");
		var w = new WWW(URL, form); //here we create a var called 'w' and we sync with our URL and the form
		yield return w; //we wait for the form to check the PHP file, so our game dont just hang
		if (w.error != null) {
			print(w.error); //if there is an error, tell us
		} else {
			print("Test ok");
			string data = w.text; //here we return the data our PHP told us
			w.Dispose(); //clear our form in game
			Debug.Log("Data: " + data);
			if(data.StartsWith("0")){
				Debug.Log("Error, picture not found");
			}else{
				Debug.Log("Picture recieved");
				byte[] picData = Convert.FromBase64String(data);
				Texture2D tex = LoadPNG (picData);
				ConvertToSprite (tex, button);
				counter++;
			}
		}
		Debug.Log ("End of getting the picture");

	}*/
	#endregion ExtraCode
}
