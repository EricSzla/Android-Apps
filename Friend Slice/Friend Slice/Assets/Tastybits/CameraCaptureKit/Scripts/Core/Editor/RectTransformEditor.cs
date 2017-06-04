using System;
using System.Collections.Generic;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEditor;
using System.Linq;


//[CustomEditor(typeof(RectTransform))]
public class RectTransformEditor : Editor {

	public UnityEditor.Editor realInspector;
	public System.Type type;

	System.Reflection.BindingFlags bf = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;


	void OnEnable(){
		

		if( type==null ) {
			foreach( var mod in System.Reflection.Assembly.GetCallingAssembly().GetModules() ) {
				foreach( var t in mod.GetTypes() ) {
					if( t.Name.ToLower().Contains("rect") ) {
						//	Debug.Log(""+t.Name);
					}
					if( t.Name == "RectTransformEditor" ) {
						//Debug.Log("found it!");
						type = t;
						break;
					}
				}
				if( type!=null ) break;
			}
		}


		if( type != null ) {
			realInspector = (UnityEditor.Editor)Activator.CreateInstance( type );
			var t = realInspector.GetType();
			var pi = t.GetMethod( "InternalSetTargets", bf );
			pi.Invoke( realInspector, new object[]{ new UnityEngine.Object[]{this.target} } );
		}
	}

	public override void OnInspectorGUI() {
		if( realInspector!=null ) {
			GUILayout.Label("type = " + realInspector.GetType() );
			var t = realInspector.GetType();
			var mi  = t.GetMethod("OnInspectorGUI", bf );
			mi.Invoke( realInspector, new object[]{} );

		}
	}

}

