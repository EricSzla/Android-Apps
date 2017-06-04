/**
 * @desc 	Inspector for CameraCaptureController.
 * 			So far its not used.
 * 
 * @author 	Tastybits
 * @link 	www.tastybits.io
 */
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Tastybits.CamCap {

	[CustomEditor( typeof(CameraCaptureController) ) ]
	public class CameraCaptureControllerInspector : Editor {
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
		}
	}

}