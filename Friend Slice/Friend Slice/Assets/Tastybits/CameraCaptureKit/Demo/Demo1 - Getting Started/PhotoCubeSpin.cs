using UnityEngine;
using System.Collections;

public class PhotoCubeSpin : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
		this.transform.Rotate( 0f, 90 * Time.deltaTime,0f );
	}
}
 