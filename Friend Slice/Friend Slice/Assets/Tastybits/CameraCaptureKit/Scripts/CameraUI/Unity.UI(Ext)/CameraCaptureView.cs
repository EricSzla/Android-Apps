/**
 * @desc 	The Camera Capture View is a class that extends the Raw Image, the reason for doing so is
 * 			to be able to manipulate the UV coords to mirror and rotate the displayed image, this is needed
 * 			to keep the complexity of the displayed Preview less since using scale and rotation for the same job
 * 			clutter up the code with mixed up width/height of the view when making the preview fit the screen.
 * 
 * @author 	Tastybits
 * @link 	www.tastybits.io
 */
using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Tastybits.CamCap {


	// The Extended Raw image lets us swap the vertices.
	[AddComponentMenu ("UI/Camera Capture View", 12)]
	public class CameraCaptureView : RawImage {	
		public int nRotate90 = 0; 
		public bool flipY = false;
		public bool flip2Y = false;
		public bool flipYPost = false;


		public void SetFlipY( bool v ) {
			if( flipY != v ) {
				flipY = v;
				this.SetVerticesDirty ();
			}
		}


		public void SetFlipAfterRotate( bool v ) {
			if( flip2Y != v ) {
				flip2Y = v;
				this.SetVerticesDirty ();
			}
		}


		public void SetRotate90Degrees( int nTimesToRotate ) {
			if( nTimesToRotate != nRotate90 ) {
				while( nTimesToRotate < 0 ) nTimesToRotate+=4;
				while( nTimesToRotate > 4 ) nTimesToRotate-=4;
				nRotate90 = nTimesToRotate;
				this.SetVerticesDirty ();
			}
		}


		protected override void OnPopulateMesh (VertexHelper vh) {
			//base.OnPopulateMesh(vh); return;
			Texture mainTexture = this.mainTexture;
			vh.Clear ();
			if (mainTexture != null)
			{
				Rect pixelAdjustedRect = base.GetPixelAdjustedRect ();
				Vector4 vector = new Vector4 (pixelAdjustedRect.x, pixelAdjustedRect.y, pixelAdjustedRect.x + pixelAdjustedRect.width, pixelAdjustedRect.y + pixelAdjustedRect.height );
				Color color = base.color;


				var uvr = this.uvRect;
				Vector2[] uvs = new Vector2[] {
					new Vector2 (uvr.xMin , uvr.yMin ), // 1
					new Vector2 (uvr.xMin , uvr.yMax ), // 2
					new Vector2 (uvr.xMax , uvr.yMax ), // 3
					new Vector2 (uvr.xMax , uvr.yMin ), // 4
				};
				if( flipY ) {
					uvs = new Vector2[] {
						new Vector2 (uvr.xMax , uvr.yMin ), // 4
						new Vector2 (uvr.xMax , uvr.yMax ), // 3
						new Vector2 (uvr.xMin , uvr.yMax ), // 2
						new Vector2 (uvr.xMin , uvr.yMin ), // 1
					};
				}
				Vector2[] uvstf = new Vector2[4] {
					uvs[(nRotate90+0) % 4],
					uvs[(nRotate90+1) % 4],
					uvs[(nRotate90+2) % 4],
					uvs[(nRotate90+3) % 4],
				};
				if( flip2Y ) {
					uvstf = new Vector2[4] {
						uvstf[3], 
						uvstf[2], 
						uvstf[1], 
						uvstf[0]
					};
				}

				// verts...
				var verts = new Vector3[]{
					new Vector3 (vector.x, vector.y),
					new Vector3 (vector.x, vector.w),
					new Vector3 (vector.z, vector.w),
					new Vector3 (vector.z, vector.y)
				};

				while( nRotate90 < 0 ) nRotate90+=4;
				vh.AddVert( verts[0], color, uvstf[0] );
				vh.AddVert( verts[1], color, uvstf[1] );
				vh.AddVert( verts[2], color, uvstf[2] );
				vh.AddVert( verts[3], color, uvstf[3] );

				vh.AddTriangle (0, 1, 2);
				vh.AddTriangle (2, 3, 0);

			}
			return;
		}
		

	}

}

