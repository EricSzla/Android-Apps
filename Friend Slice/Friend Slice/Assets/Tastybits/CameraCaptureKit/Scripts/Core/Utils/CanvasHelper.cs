/**
 * @desc 	The following source-file is part of MobyShop - a solution for creating a Free-2-play ingame shop in your game.
 * @author 	Tastybits | www.tastybits.io
 * @license See assetstore for license details. 
 * @copyright 2016 Tastybits
 */
using UnityEngine;
using System.Collections;


namespace Tastybits.CamCap {


	public class CanvasHelper  {

		public static UnityEngine.Canvas GetCanvas( bool createIfNotExists=true ) {
			UnityEngine.Canvas canvas=null;
			var canvascomponents = UnityEngine.Component.FindObjectsOfType<UnityEngine.Canvas>();	

			// First try and find the object with the canvas on it that has the name Canvas and si active in hierarchy.
			foreach( var component in canvascomponents ) {
				// CONSIDER: also looking into the gameobject returned to see if the object has a set of Cavas components usually 
				// auto created by unity.
				if( component.enabled==true && component.name == "Canvas" && component.gameObject.activeInHierarchy == true ) {
					canvas=component;
					break;
				}
			}

			// Consider looking though hidden objects as well.

			// If the canvas is sstill null and there are other canvas components
			// avialable return one of them which is active.
			if (canvas == null) {
				foreach (var component in canvascomponents) {
					if (component.enabled == true ) {
						canvas = component;
						break;
					}
				}
			}

			// if the canvas is still null we will generate a canvas for our use.
			if( canvas == null && createIfNotExists ) {
				Debug.Log("MobyShop: Canvas not found - creating one.");
				var goCanvas = new GameObject("Canvas");
				canvas = goCanvas.AddComponent<UnityEngine.Canvas>();
				canvas.pixelPerfect = false;
				canvas.sortingOrder = 0;
				canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				var cs = goCanvas.AddComponent<UnityEngine.UI.CanvasScaler>();
				cs.scaleFactor = 1f;
				cs.referencePixelsPerUnit = 100;
				cs.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize;
				var rc = goCanvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
				rc.ignoreReversedGraphics = true;
				rc.blockingObjects = UnityEngine.UI.GraphicRaycaster.BlockingObjects.None;
				/*var rt = */goCanvas.AddComponent<RectTransform>();
				if( Component.FindObjectOfType<UnityEngine.EventSystems.EventSystem>()==null ) {
					var goEventSystem = new GameObject("EventSystem");
					/*var es = */goEventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
					/*var im = */goEventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
				}
			}
			return canvas;
		}

	}


}

