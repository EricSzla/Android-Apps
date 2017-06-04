using UnityEngine;
using System.Collections;
using Tastybits.CamCap;


/*
	==================================================================
	  3. Example - Fun With Camera
 	==================================================================

	This is an basic example provided to you to see how to to build an app wher you take a photo of a person and give a beard on him or her as well as saving 
	the imge to the disk.
*/
public class FunWithCameraDemo : MonoBehaviour {

	enum State {
		Wait = -1,
		Begin = 0,
		Capturing = 1,
		ShowResult = 3
	}
	public Texture2D finalImage;

	State state=State.Begin;
	public CameraCaptureUI cameraCaptureUI;

	public GameObject template;
	public GameObject hairAndBeard;
	public GameObject caption;
	public RectTransform extraParent;

	public Shader uiShader;

	// The 
	public GameObject beginContainer;

	// 
	public GameObject resultsContainer;

	//
	public UnityEngine.UI.RawImage rawImageResult;



	void Awake() {
		if( cameraCaptureUI == null ) {
			Debug.LogError("CameraCaptureUI of CameraCaptureDemo was not specified.");
		}
	}


	void Start() {
		cameraCaptureUI.gameObject.SetActive(false);
		caption.SetActive(false);
		template.SetActive(false);
		hairAndBeard.SetActive(false);

		caption.GetComponent<RectTransform>().SetParent( extraParent );
		hairAndBeard.GetComponent<RectTransform>().SetParent( extraParent );
		template.GetComponent<RectTransform>().SetParent( extraParent );
	}


	void Update() {
		if (state == State.Begin) {
			this.beginContainer.SetActive (true);
			this.resultsContainer.SetActive (false);

		} else if (state == State.ShowResult) {
			this.beginContainer.SetActive (false);
			this.resultsContainer.SetActive (true);
			this.rawImageResult.texture = this.finalImage;
		} else if (state == State.Capturing || state == State.Wait ) {
			this.beginContainer.SetActive (false);
			this.resultsContainer.SetActive (false);
		}
	}


	public void OnStartCameraClicked() {
		state = State.Wait;
		cameraCaptureUI.OnPhotoCaptured = this.OnPhotoCaptured;
		cameraCaptureUI.OnRetakePhoto  = this.OnRetakePhoto;
		cameraCaptureUI.OnStillCaptured = OnStillCaptured;
		caption.GetComponent<UnityEngine.UI.Text>().text = "Fit the face.";
		cameraCaptureUI.SetActiveAndStartCapture( true, ( bool ok )=>{
			state = ok ? State.Capturing : State.Begin;
			if( ! ok )  {
				Debug.LogError("Error starting camera.");
			} else {
				template.SetActive(true);
				hairAndBeard.SetActive(false);
				caption.SetActive(true);
			}
		});
	}


	public void OnRestartClicked() {
		state = State.Begin;
	}


	public void OnSaveToAlbumClicked(){
		if (finalImage == null)
			return;
		Documents.SaveTextureToDocuments ("beardexample.png", this.finalImage);
	}


	void OnStillCaptured(){
		Debug.Log("Demo: On still captured");
		caption.GetComponent<UnityEngine.UI.Text>().text = "Does it suit you?";
		template.SetActive(false);
		hairAndBeard.SetActive(true);
	}


	void OnRetakePhoto() {
		Debug.Log("Demo: Retake clciked");
		template.SetActive(true);
		hairAndBeard.SetActive(false);
		caption.GetComponent<UnityEngine.UI.Text>().text = "Fit the face.";
	}


	// Callback called when the photo has been taken and ready for you to use 
	// in this example we set the maintexture of a material to point at it.

	//bool convert=false;
	void OnPhotoCaptured( Texture2D tx ) {
		Debug.Log ("Demo: OnPhotoCaptured");
		state = State.ShowResult;	
		finalImage = CombineBeardWithFinalPhoto( tx );
		cameraCaptureUI.gameObject.SetActive(false);
		template.SetActive(false);
		hairAndBeard.SetActive(false);
		caption.SetActive(false);
	}


	Texture2D CombineBeardWithFinalPhoto( Texture2D tx ){
		if (tx == null) {
			Debug.LogError ("Error texture is null");
			return null;
		}
			
		float vpWidth = (float)Screen.width; // _width * (tx.height/_height);
		float vpHeight = (float)Screen.height;
		float vpAspect = vpWidth / vpHeight;
		float txAspect = (float)tx.width / (float)tx.height;
		float drawWidth = 1f;
		float drawHeight = 1f;

		var pre_rt = RenderTexture.active; 
		var rt = RenderTexture.GetTemporary( (int)vpWidth, (int)vpHeight, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1 );
		RenderTexture.active = rt;                   

		GL.Clear( true, true, Color.black );
		GL.PushMatrix ();
		GL.LoadPixelMatrix( 0f, vpWidth, vpHeight, 0f );

		var shader = uiShader; 
		//if( shader == null ) shader = uiShader = Shader.Find ("UI/Default");
		Material material=null;
		if (shader != null) {
			material = new Material (shader);
		} else {
			Debug.LogError( "Error:Shader not present: UI/Default; you might need to add it to the build settings." );
		}


		float drawAspect = txAspect;
		drawWidth = vpHeight * txAspect;
		drawHeight = vpHeight;
		float xoff = (vpWidth - drawWidth) / 2f;//(vpHeight - drawHeight);
		float yoff = 0f;// (vpWidth - drawWidth) / 2f;
		//Graphics.DrawTexture( new Rect( xoff, yoff,  drawWidth, drawHeight ), tx );
		Graphics.DrawTexture( new Rect( xoff, yoff,  drawWidth, drawHeight ), tx, material );

		// Draw Beard.
		var beardTexture = this.hairAndBeard.GetComponent<UnityEngine.UI.RawImage> ().texture;

		drawAspect = (float)beardTexture.width / (float)beardTexture.height;
		if( drawAspect > vpAspect ) {
			drawWidth = vpWidth;
			drawHeight = vpWidth / drawAspect;
			xoff = 0f;
			yoff = (vpHeight - drawHeight) / 2f;
		} else if (drawAspect <= vpAspect) {
			drawWidth = vpHeight * drawAspect;
			drawHeight = vpHeight;
			xoff = (vpWidth - drawWidth ) / 2f;
			yoff = (vpHeight - drawHeight) / 2f;
		}
		Graphics.DrawTexture (new Rect (xoff, yoff, drawWidth, drawHeight), beardTexture, material);

		GL.PopMatrix ();    
		GL.End ();

		var tex = new Texture2D( (int)vpWidth, (int)vpHeight, TextureFormat.ARGB32, false );
		tex.ReadPixels( new Rect(0,0,(int)vpWidth, (int)vpHeight), 0, 0, false );
		tex.Apply ();

		RenderTexture.ReleaseTemporary (rt);
		rt = null;

		RenderTexture.active = pre_rt;
		return tex;
	}


	// GUIStyle used when drawing describtion label.
	GUIStyle LabelStyle {
		get {
			return labelStyle;
		}
	}

	// The style of the buttons used.
	GUIStyle buttonStyle;
	GUIStyle ButtonStyle {
		get {
			if( buttonStyle == null ) {
				buttonStyle = new GUIStyle( GUI.skin.button );
				buttonStyle.fontSize = 15;
				if( Screen.width > 1000 || Screen.height > 1000 ) {
					buttonStyle.fontSize = 30;
				}
			}
			return buttonStyle;
		}
	}

	bool _labelStyleSet=false;
	GUIStyle _labelStyle = new GUIStyle();
	GUIStyle labelStyle {
		get {
			if(!_labelStyleSet){
				_labelStyleSet=true;
				_labelStyle = new GUIStyle( GUI.skin.label );
				_labelStyle.fontSize = 15;

				_labelStyle.fontStyle = FontStyle.Bold;
				_labelStyle.alignment = TextAnchor.MiddleLeft;
				var _tex = new Texture2D(2,2); 
				_tex.SetPixels( new Color[]{new Color(0,0,0,0.25f),new Color(0,0,0,0.25f),new Color(0,0,0,0.25f),new Color(0,0,0,0.25f) } );
				_tex.Apply();
				_labelStyle.normal.background = _tex;

				_labelStyle.fontSize = 10;
				if( Screen.width > 1000 || Screen.height > 1000 ) {
					_labelStyle.fontSize = 24;
				}
			}
			return _labelStyle;
		}
	}


}
