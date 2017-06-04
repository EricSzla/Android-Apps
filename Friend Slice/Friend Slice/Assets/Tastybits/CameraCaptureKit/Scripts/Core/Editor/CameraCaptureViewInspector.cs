/**
 * @desc 	Inspector for the Preview ( CameraCaptureView )
 * 
 * @author 	Tastybits
 * @link 	www.tastybits.io
 */
using UnityEngine;
using System.Collections;
using UnityEditor;


namespace Tastybits.CamCap {


	[CustomEditor(typeof(CameraCaptureView))]
	public class CameraCaptureViewInspector : UnityEditor.UI.RawImageEditor {

		public override void OnInspectorGUI() {
			var targ = this.target as CameraCaptureView;

			GUILayout.Label("Flipped " + targ.flip2Y + " Rotated:" + targ.nRotate90 );

			/*targ.flipY = GUILayout.Toggle( targ.flipY, "Flip Y" );
			targ.nRotate90 = EditorGUILayout.IntField( "NRotate", targ.nRotate90 );
			var newV = GUILayout.Toggle( targ.flip2Y, "Flip Y ( After rotate )" );
			if( newV != targ.flip2Y ) {
				targ.SetFlipAfterRotate( newV );
			}*/

			base.OnInspectorGUI ();
		}

	}

}

