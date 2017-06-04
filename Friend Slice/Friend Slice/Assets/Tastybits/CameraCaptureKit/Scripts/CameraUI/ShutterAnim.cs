/**
 * @desc 	ShutterAnim is a experimental class to display a shutter in the Camera Unity UI.
 * 
 * @author 	Tastybits
 * @link 	www.tastybits.io
 */
using UnityEngine;
using System.Collections;


[ExecuteInEditMode]
public class ShutterAnim : MonoBehaviour {

	// Use this for initialization
	public float time  = 0f;
	int numSlices = 8;
	public float timeMax = 24f;

	public bool animating = false;

	public System.Action animfunc = null;

	public int frameNum = 0;
	public float tmp=0f;
	public float framesPerSecond = 24f; // framerate is 24 frames per second.
	int numFrames = 24; // second... ( 24 frames in total )

	
	public void Close( System.Action callback=null ) {
		if( animating ) {
			if(callback!=null)callback();
			return;
		}
		time = 0f;
		timeMax = 23.99f;
		animating = true;
		animfunc = ()=>{
			if( time >= timeMax ) {
				if( time > timeMax ) {
					time = timeMax;
				}
				animating=false;
				animfunc = null;
				if(callback!=null)callback();
				return;
			}
			time += Time.deltaTime * framesPerSecond;
			if( time > timeMax ) {
				time = timeMax;
			}
		};
	}


	public void Open( System.Action callback=null ) {
		if( animating ) {
			if(callback!=null)callback();
			return;
		}
		time = 23.99f;
		timeMax = 0f;
		animating = true;
		animfunc = ()=>{
			if( time <= timeMax ) {
				if( time < timeMax ) {
					time = timeMax;
				}
				animating=false;
				animfunc = null;
				if(callback!=null)callback();
				return;
			}
			time -= Time.deltaTime * framesPerSecond;
			if( time < timeMax ) {
				time = timeMax;
			}
		};
	}



	// Update is called once per frame
	void Update () {

		frameNum = Mathf.RoundToInt(time % framesPerSecond);


		float rotateStep = 360f/(float)numSlices;

		float angleStep = ( 180f / (float)numFrames ) * 0.33f;//*Mathf.PI/180f;


		for( int i=0; i<numSlices; i++ ) {
			tmp = 1f - ((float)frameNum / (float)numFrames);


			var go = GameObject.Find("Slice - " + (i+1) );
			var rt = go.GetComponent<RectTransform>();
			rt.localPosition = new Vector3(0,0,-i);

			// Each frame the entire thing rotates..
			rt.localRotation = Quaternion.Euler( 0, 0, i*rotateStep );
			rt.sizeDelta = new Vector2 (200f,  200f);


			// Rotate the Image with ethe anchor in the bottom of the image.
			var rt2 = rt.FindChild("Img").GetComponent<RectTransform>();
			var interpolate = (frameNum)*angleStep;
			var re = GameObject.Find("ShutterAnim").GetComponent<RectTransform>().rect;

			rt2.localPosition = new Vector3(0,re.height * 0.5f * tmp,rt2.localPosition.z);
			rt2.localRotation = Quaternion.Euler( rt2.localRotation.eulerAngles.x,rt2.localRotation.eulerAngles.y, interpolate );


			rt2.sizeDelta = new Vector2 (re.width*1.3f, re.height);

			
		}

		if( animfunc != null && animating ) {
			animfunc();
		}

	}


}

#if UNITY_EDITOR

[UnityEditor.CustomEditor(typeof(ShutterAnim))]
public class ShutterAnimEditor : UnityEditor.Editor {
	public override void OnInspectorGUI () {
		base.OnInspectorGUI ();
		if( GUILayout.Button("close") ) {
			(this.target as ShutterAnim).Close();
		}
		if( GUILayout.Button ("open") ) {
			(this.target as ShutterAnim).Open();
		}
	}
}


#endif 
