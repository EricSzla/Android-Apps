using UnityEngine;
using System.Collections;

public class UpdateAspectRatio : MonoBehaviour {

	void Awake() {
		DoIt ();
	}

	void OnEnable() {
		DoIt ();
	}
	
	void Update() {
		DoIt ();
	}


	void LateUpdate() {
		DoIt ();
	}


	public void DoIt() {
		var arf = this.GetComponent<UnityEngine.UI.AspectRatioFitter> ();
		if (arf == null)
			return;

		var rawImage = this.GetComponent<UnityEngine.UI.RawImage> ();
		if (rawImage != null && rawImage.texture!=null ) {
			var aspect = (float)rawImage.texture.width / (float)rawImage.texture.height;
			if (arf.aspectRatio != aspect) {
				arf.enabled = false;
				arf.aspectRatio = aspect;
				var mi = arf.GetType ().GetMethod ("UpdateRect", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
				if (mi != null) {
					mi.Invoke ( arf, new object[]{} );
				}
				arf.enabled = true;
			}
		}
	}


}
