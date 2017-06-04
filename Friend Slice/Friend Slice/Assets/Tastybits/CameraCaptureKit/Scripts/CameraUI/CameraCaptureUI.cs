/**
 * @desc 	CameraCaptureUI - this class is the heart of the visual representation of the camera using Unity.UI. 
 * 			To see it in action check out the CameraCaptureUIDemo scene  which demo's the use of this Component.
 * 
 * @author 	Tastybits
 * @link 	www.tastybits.io
 */
using UnityEngine;
using System.Collections;


namespace Tastybits.CamCap {


	public class CameraCaptureUI : MonoBehaviour {
		public enum StateTypes {
			Wait = -1,
			Begin = 0,
			CapturingPreview = 1,
			CapturedStill = 2,
			ShowFinal = 3
		}


		StateTypes state=StateTypes.Begin;

		/**
		 * can be used to determine if we're debugging or not. 
		 */
		public bool verbose {
			get { 
				return CameraCaptureController.Verbose;
			}
		}


		/**
		 * Returns the state of the component.
		 */ 
		public StateTypes State {
			get {
				return state;
			}
		}

		// referance to the Controller.
		public CameraCaptureController controller;

		// Singleton referance.
		public static CameraCaptureUI instance;

		// referance to the capture button.
		public GameObject buttonsCapture;

		// referance to the retake button.
		public GameObject buttonsRetake;

		// set by the initializing code.
		bool allowRetake=true;

		// set this to try if the final photo should be shown by the CameraUI in the result container.
		public bool allowShowFinal = true;

		// referance to the final texture grapped.
		public Texture2D FinalTexture;

		// referance to the UI element showing the final texture.
		public UnityEngine.UI.RawImage finalCaptureImage;

		// referance to the container showing the final texture.
		public UnityEngine.GameObject resultContainer;

		// callback that will be invoked when a final image has been taken.
		public System.Action<Texture2D> OnPhotoCaptured=null;

		// callback invoked when the Retake Photo has been clicked 
		// this is exposed for the outside world since some application will use this.
		public System.Action OnRetakePhoto = null;

		// Callback when the still image has been captured. This si called every time we grap a photo
		// however sometimes the user choees to retake the image and the code using this 
		public System.Action OnStillCaptured = null;

		// referance to the preview.
		public GameObject preview;

		// referance to the flash button.
		public GameObject flashButton;

		// referance to the flipButton.
		public GameObject flipButton;
		public GameObject exposureButton;
		public Color inactivePreviewColor=Color.black;
		public Color cameraBackgroundColor = new Color32(26,26,26,0xFF);
		public UnityEngine.UI.RawImage bg;
		public bool EnableClickToFocus = false;
		public bool AdjustUIToDeviceRotation = false;
		DeviceOrientation orientation = DeviceOrientation.Portrait;
		public GameObject buttonISO;
		public GameObject exposureCompOverlay;

		public Sprite flashButtonSpriteOff;
		public Sprite flashButtonSpriteOn;
		public Sprite flashButtonSpriteAuto;

		public bool EnableAdjustFlashIfAvailable = true;
		public bool EnableAdjustExposureIfAvail = true;
		public bool EnableAdjustISOIfAvail = true;
		public bool EnableFlipCameraIfAvailable = true;



		public DeviceOrientation Orientation {
			get {
				return orientation;
			}
			set {
				if( orientation!=value ) {
					orientation=value;
					OnOrientationChanged( value );
				}
			}
		}


		public static void CreateUIAndCaptureImage( System.Action<Texture2D> callback ) {
			var canvas = CanvasHelper.GetCanvas (true);
			if( canvas == null ) {
				Debug.LogError ("Error: Cannot find canvas");
				callback (null);
			} else {
				if (instance == null) {
					var cameraCaptureUI = Resources.Load ("CameraCaptureUI", typeof(GameObject) );
					GameObject.Instantiate (cameraCaptureUI);
					var rt = instance.gameObject.GetComponent<RectTransform>();
					rt.SetParent( canvas.GetComponent<RectTransform>(), true );
					rt.SetAsLastSibling ();
					rt.anchorMax = Vector2.zero;
					rt.anchorMax = Vector2.one;
					rt.offsetMin = rt.offsetMax = Vector2.zero;
				}
				if (instance == null) {
					Debug.LogError ("Error creating camera capture ui");
					callback (null);
				} else {
					System.Action<Texture2D> grap = null;
					grap = ( Texture2D texret) => {
						if (texret == null) {
							Debug.LogError ("Error the photo captured was null");
						}
						instance.OnPhotoCaptured -= grap;
						instance.gameObject.SetActive(false);
						callback (texret);
					};
					instance.OnPhotoCaptured += grap;
					instance.SetActiveAndStartCapture( false, ( bool ok ) => {
						if( !ok ) {
							Debug.LogError("Error starting capture session");
							instance.gameObject.SetActive(false);
							callback(null);
						}
					} );
					 
				}
			}
		}


		void Awake() {
			if( instance != null && instance != this ) {
				Debug.LogWarning( "More than one instance of the Camera Capture UI has been created." );
			}
			instance = this;
			resultContainer.SetActive(false);
			buttonsCapture.SetActive(false);
			buttonsRetake.SetActive(false);
			buttonISO.SetActive(false);
			preview.SetActive(false);
			if(bg!=null){
				bg.color = cameraBackgroundColor;	
			}
		}


		void OnDestroy() {
			if (instance == this) {
				instance = null;
			}
		}

		// Local values that contains the state of the torch.
		bool torchenabled = false;

		// Local value that contains the current ISO set.
		string currentIsoLevel = "";

		// array of available ISO values.
		string[] availableISOValues;

		/**
		 * Callback invoked by the UI when you click the Touch trigger button.
		 */
		public void ButtonTorchClicked(){
			// Toggle Torch
			torchenabled = !torchenabled;
			controller.SetTorchEnabled( torchenabled );
		}

		/**
		 * Callback invoked by the UI when you start capturing with the camera.
		 */
		public void SetActiveAndStartCapture( bool allowRetake, System.Action<bool> callback ) {
			gameObject.SetActive(true);
			this.allowRetake = allowRetake;
			resultContainer.SetActive(false);
			buttonsCapture.SetActive(false);
			buttonsRetake.SetActive(false);
			preview.SetActive(true);
			//torchenabled = false;

			if( state==StateTypes.ShowFinal ) {
				state=StateTypes.ShowFinal;
			}

			if( state!=StateTypes.Begin ) {
			}
			if( !gameObject.activeInHierarchy ) {
				Debug.LogError("Error gameobject : " + this.gameObject.name + " is not active in hierachy", this.gameObject );
				callback(false);
				return;
			}


			flashButton.SetActive( false );
			flipButton.SetActive( false );
			buttonISO.SetActive(false);
			exposureButton.SetActive(false);

			// (using non capital off to signal that its not set)
			SetFlashButtonIcon( "" ); 

			preview.GetComponent<CamCap.CameraCaptureView>().color = inactivePreviewColor;

			controller.StartCamera( ( bool ok ) => {
				if( ok ) {
					SetFlashButtonIcon( "" + controller.CurrentFlashModeAsStr );
					flashButton.SetActive( controller.HasFlashFunctionality && EnableAdjustFlashIfAvailable );
					exposureButton.SetActive( controller.CanAdjustExposureCompensation && EnableAdjustISOIfAvail );
					flipButton.SetActive( controller.CanFlipBetweenMultipleCameras && EnableFlipCameraIfAvailable );
					buttonISO.SetActive( controller.CanAdjustISO && EnableAdjustExposureIfAvail );
					currentIsoLevel = "";
					availableISOValues = new string[]{};
					if( controller.CanAdjustISO ) {
						availableISOValues = controller.SupportedISOValues;
						currentIsoLevel = controller.ISOValue; 
						buttonISO.transform.FindChild( "Text" ).GetComponent<UnityEngine.UI.Text>().text = ""+currentIsoLevel;
					}
					state = StateTypes.CapturingPreview;
					buttonsCapture.SetActive(true);
					preview.GetComponent<CamCap.CameraCaptureView>().color = Color.white;
					if( Application.isEditor == false ) {
						controller.SetTorchEnabled( torchenabled );
					}
				}
				callback(ok);
			} );
		}
			

		/**
		 * Used to set the flash icon on the Flash button.
		 */
		void SetFlashButtonIcon( string str ) {
			if(verbose)Debug.Log("Set flash button : " +str );

			// its not set...
			Sprite spr = null;
			if( str == "" || str == "off" ) {
				spr = this.flashButtonSpriteOff;
			} else if( str == "Off" ) {
				spr = this.flashButtonSpriteOff;
			} else if( str == "On" ) {
				spr = this.flashButtonSpriteOn;
			} else if( str == "Auto" ) {
				spr = this.flashButtonSpriteAuto;
			} else {
				Debug.LogError("CameraCaptureKit: Error: Unknown FlashButtonIcon : "+ str );
			}

			var image = flashButton.GetComponent<UnityEngine.UI.Image>();
			image.sprite = spr;
			image.enabled = true;
			if( spr == null ) {
				Debug.LogError("CameraCaptureKit: Sprite for flash button is null");
				image.enabled=false;
			}
		}


		/**
		 *  called when you change the ISO.
		 */
		public void OnChangeISOClicked() {
			string newValue = currentIsoLevel;
			for( int i = 0; i<availableISOValues.Length; i++ ){
				if( availableISOValues[i] == currentIsoLevel ) {
					i++;
					if( i >= availableISOValues.Length ) {
						i = 0;
					}
					newValue =  availableISOValues[i];
					break;
				}
			} 
			if( newValue != currentIsoLevel ) {
				currentIsoLevel = newValue;
				controller.ISOValue = currentIsoLevel;
				buttonISO.transform.FindChild( "Text" ).GetComponent<UnityEngine.UI.Text>().text = ""+currentIsoLevel;	
				if( controller.ISOValue != currentIsoLevel ) {
					Debug.LogError("Seems like there was a problem getting the ISO value" );
				}
			} else {
				Debug.LogError("The new value is the same as the old. ( " + newValue + " ) ");
			}
		}


		/**
		 * Invoked by the UI when the button to adjust the ISO is clicked.
		 */
		public void OnAdjustExposureCompentationClicked() {
			//Debug.Log("OnAdjustExposureCompentationClicked");
			exposureCompOverlay.SetActive(true);
			var overlayController = exposureCompOverlay.GetComponent<AdjustExposureCompensationController>();
			int min; int max;
			controller.GetExposureCompensationRange( out min, out max );
			overlayController.SetupWithRange( min, max );
			overlayController.OnDidDismiss = ( int newValue ) => {
				if( newValue == -999 ) { // cancel.
					controller.SetExposureCompensationDisabled();
				} else {
					controller.SetExposureCompensation( newValue );
				}
			};
		}
			

		/**
		 * Invoked by the UI when the Flip Camera button is clicked. 
		 */
		public void OnFlipButtonClicked() {
			if( state != StateTypes.CapturingPreview ) {
				Debug.Log("Not the right time to flip the camera.");
				return;
			}
			preview.GetComponent<CamCap.CameraCaptureView>().color = inactivePreviewColor;
			state = StateTypes.Wait;
			controller.FlipCameraFacing( ()=>{
				state = StateTypes.CapturingPreview;
				preview.GetComponent<CamCap.CameraCaptureView>().color = Color.white;
			} );
		}


		/**
		 * Invoked by the UI when the Flash button is clicked.
		 */
		public void OnFlashButtonClicked() {
			if( state != StateTypes.CapturingPreview ) {
				Debug.LogError("CamereaUI: Cannot change flash mode now, the camera is not showing a preview.");
				return;
			}
			state = StateTypes.Wait;
			string mode_set = controller.SetNextFlashMode(  );
			Debug.Log("CameraUI: Set flash next mode : " + mode_set );
			SetFlashButtonIcon( "" +mode_set );
			state = StateTypes.CapturingPreview;
		}


		/**
		 * Invoked by the UI when the Capture Button has been clicked.
		 */
		public void OnCaptureStillImageButtonClicked() {
			if( state != StateTypes.CapturingPreview ) {
				Debug.LogError("CamereaUI: Cannot change flash mode now, the camera is not showing a preview.");
				return;
			}

			if( allowRetake ) {
				state = StateTypes.Wait;
				controller.CaptureStillImage( (bool ok)=>{ 			
					if( OnStillCaptured!=null ) {
						OnStillCaptured();
					}
					buttonsCapture.SetActive(false);
					buttonsRetake.SetActive(true);
					// Wait for the user to recognize the photo or not...
					state = StateTypes.CapturedStill;
					if( !ok ) Debug.LogError( "CameraUI: Error taking still image" );
				} );
			} else {
				state = StateTypes.Wait;
				// we dont allow to retake the image and inspect it before stopping so just stop here.
				controller.CaptureStillImageAndShutdownCamera( (bool ok, Texture2D tex )=>{ 
					if( OnStillCaptured!=null ) {
						OnStillCaptured();
					}
					buttonsCapture.SetActive(false);
					buttonsRetake.SetActive(false);
					ProcessPhotoTaken( tex );
				} );

			}
		}


		/**
		 * This method is called when the final texture is ready for processing.
		 */
		void ProcessPhotoTaken( Texture2D tex ) {
			FinalTexture = tex;
			preview.SetActive(false);
			if( OnPhotoCaptured!=null ) {
				OnPhotoCaptured( tex );
			}
			if( allowShowFinal ) {
				resultContainer.SetActive(true);
				this.finalCaptureImage.texture = FinalTexture;
				if( verbose ) Debug.Log( "Final Texture Set");
				state = StateTypes.ShowFinal;
			} else {
				resultContainer.SetActive(false);
				state = StateTypes.Begin;
			}
		}


		public void OnCapturingScreenClicked() {
			if(verbose) Debug.Log("On Screen clicked : " + Input.mousePosition.x + " , " + Input.mousePosition.y );
			if( Application.platform == RuntimePlatform.IPhonePlayer ) {
				
			} else if( Application.platform == RuntimePlatform.Android ) {
				controller.ResetFocus();
			} else {
				
			}
		}


		// this is a function to let the controller know that we want to 
		// retake the still image.
		public void OnRetakePhotoClicked() {
			if( state != StateTypes.CapturedStill ) {
				Debug.Log("Wrong time to retake the still image.");
				return;
			}
			buttonsCapture.SetActive(true);
			buttonsRetake.SetActive(false);
			controller.DiscardStillImageAndContinue();
			SetFlashButtonIcon( controller.CurrentFlashModeAsStr );

			state = StateTypes.CapturingPreview;
			if( OnRetakePhoto!=null ) {
				OnRetakePhoto();
			}
		}


		public void OnClickToFocusClicked(){
			var ce = UnityEngine.EventSystems.EventSystem.current;
			UnityEngine.EventSystems.PointerEventData ped = new UnityEngine.EventSystems.PointerEventData(ce); 
			ped.position = Input.mousePosition;

			Vector2 localCursor;
			bool insideRect = !RectTransformUtility.ScreenPointToLocalPointInRectangle(
								GetComponent<RectTransform>(), 
								ped.position, 
								ped.pressEventCamera, 
								out localCursor);
			Debug.Log( "insideRect=" + insideRect + " EventPressPosition : " + ped.pressPosition.x+", "+ped.pressPosition.y + " localCursor = "+ localCursor.x + ", " + localCursor.y );

			if( insideRect ) {
				
			}
		}


		public void OnUseImageClicked() {
			if( state != StateTypes.CapturedStill ) {
				Debug.Log("Wrong time to retrieve the still image.");
				return;
			}
			state = StateTypes.Wait;
			buttonsRetake.SetActive(false);
			controller.GetStillImageAndCleanup( ( Texture2D tex ) => {
				ProcessPhotoTaken( tex );
			} ); 
		}


		void UpdateOrient() {
			//Debug.Log( "Input.deviceOrientation = " + Input.deviceOrientation + " Screen.orientation=" + Screen.orientation );

			// LandScape Right ( flip på hovedet )

			if( Screen.orientation == ScreenOrientation.AutoRotation ) {
				// cannot do anything when we are auto rotating.
			} else if( Screen.orientation == ScreenOrientation.LandscapeLeft ) {
				if( Orientation != DeviceOrientation.LandscapeLeft ) {
					Orientation = DeviceOrientation.LandscapeLeft;
				}
			} else if( Screen.orientation == ScreenOrientation.LandscapeRight ) {
				if( Orientation != DeviceOrientation.LandscapeRight ) {
					Orientation = DeviceOrientation.LandscapeRight;
				}
			} else if( Screen.orientation == ScreenOrientation.Portrait ){
				if( Orientation != DeviceOrientation.Portrait ) {
					Orientation = DeviceOrientation.Portrait;
				}
			} else if( Screen.orientation == ScreenOrientation.PortraitUpsideDown ) {
				if( Orientation != DeviceOrientation.PortraitUpsideDown ) {
					Orientation = DeviceOrientation.PortraitUpsideDown;
				}
			} else {

			} // Unknown....
		}



		RectTransform TopRectTF {
			get {
				var tf = this.GetComponent<RectTransform>();
				while( (tf.parent as RectTransform) != null ) {
					tf  = tf.parent as RectTransform;
				}
				return tf;
			}
		}


		// Orientation changed now...
		void OnOrientationChanged( DeviceOrientation orient ) {
			Debug.Log("Orientation changed : " + orient );

			if( AdjustUIToDeviceRotation && Screen.orientation != ScreenOrientation.AutoRotation  ) {
				// time to update the UI....
				/*if( state == StateTypes.CapturingPreview ) {
					
				}*/

				float w = TopRectTF.sizeDelta.x;
				float h = TopRectTF.sizeDelta.y;

				// make it squiare...
				if( w > h ) w = h;
				else h = w;

				var rt = this.GetComponent<RectTransform>();
				rt.sizeDelta = new Vector2( w, h );



			}
		}


		void Update() {
			UpdateOrient();
			if( allowShowFinal && state == StateTypes.ShowFinal ) {
				//Debug.Log("showing result container");
				var arf = this.finalCaptureImage.GetComponent<UnityEngine.UI.AspectRatioFitter>();
				if( arf!=null && this.finalCaptureImage.gameObject.activeInHierarchy && finalCaptureImage.texture!=null ) {
					var tx = finalCaptureImage.texture;
					float ratio = (float)tx.width / (float)tx.height;
					//float ratio = (float) Screen.width / (float) Screen.height;
					//Debug.Log( "Updating aspect of final texture : " + tx.width + "," + tx.height + " ratio=" + ratio );
					arf.enabled = true;
					arf.aspectRatio = ratio;
					arf.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.EnvelopeParent;
				} else {
					arf.enabled=false;
				}
			}
		}


	}



}


#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(Tastybits.CamCap.CameraCaptureUI))]
public class CameraCaptureUIInspector : UnityEditor.Editor {
	public override void OnInspectorGUI(){
		base.OnInspectorGUI();

		var targ = (this.target as Tastybits.CamCap.CameraCaptureUI);
		GUI.color = Color.red;

		if( targ.controller==null ){
			GUILayout.Label("Warning: the referance to the controller is not set");
		}
		if( targ.GetComponent<RectTransform>()==null ){
			GUILayout.Label("Warning: there is no Rect Transform on the CameraCaptureUI object.");
		}
		Transform tf = targ.transform;
		bool fnd = false;
		while( !fnd && tf!=null ) {
			fnd = tf.GetComponent<UnityEngine.Canvas>()!=null;
			tf = tf.parent;
		}
		if( !fnd ){
			GUILayout.Label("Warning: The object is a Unity.UI object and is supposed to reside inside a hierachy with a canvas.");
		}
	}
}
#endif