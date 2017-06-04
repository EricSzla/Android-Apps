/**
 * @desc 	Misc tools for UI additions as well as copying and transforming the pixels of a texture 
 * 
 * @author 	Tastybits
 * @link 	www.tastybits.io
 */
using UnityEngine;
using System.Collections;

namespace Tastybits.CamCap {

	public enum CopyRotate {
		None,
		Left = 1,
		Right = 2,
		LeftX2 = 3
	}
	public enum CopyFlip {
		None = 0,
		Horisontal = 1
	}

	public static class RectTransformExt {
		public static float GetPixelWidth( this RectTransform self ) {
			var tf = self;
			//bool ok = false;
			while( tf!=null ) {
				
				tf = tf.parent as RectTransform;
				if( tf != null ) {
					if( tf.GetComponent<Canvas>()!=null ) {
						//ok=true;
						tf=null;
					}
				}
			}
			return 0f;
			//self.parent != null && self.parent.
		}
		
		
		public static float GetAnchorWidthAbs( this RectTransform self ) {
			var tf = self;
			bool ok = false;
			float wi = 1f;
			while( tf!=null ) {
				wi *= tf.GetAnchorWidth(); 
				tf = tf.parent as RectTransform;
				if( tf != null && tf.GetComponent<Canvas>()!=null ) {
					ok=true;
				} else {
					tf = null;
				}
			}
			if( !ok ) {
				Debug.Log ("Error resolving width maybe elemnet is not part of hierachy with Canvas as top element" );
			}
			return wi;
			//self.parent != null && self.parent.
		}
		
		
		public static float GetAnchorWidthAbsInPixels( this RectTransform self ) {
			return self.GetAnchorWidthAbs() * Screen.width;
		}
		
		
		public static void FitAnchors( this RectTransform t ) {
			RectTransform pt = t.parent as RectTransform;
			if( t == null || pt == null ) return;
			Vector2 newAnchorsMin = new Vector2(t.anchorMin.x + t.offsetMin.x / pt.rect.width,
			                                    t.anchorMin.y + t.offsetMin.y / pt.rect.height);
			Vector2 newAnchorsMax = new Vector2(t.anchorMax.x + t.offsetMax.x / pt.rect.width,
			                                    t.anchorMax.y + t.offsetMax.y / pt.rect.height);
			t.anchorMin = newAnchorsMin;
			t.anchorMax = newAnchorsMax;
			t.offsetMin = t.offsetMax = new Vector2(0, 0);
		}
		public static void FitCorners( this RectTransform t ) {
			t.offsetMin = t.offsetMax = new Vector2(0, 0);
		}
		
		public static void SetAnchors( this RectTransform t, Vector2 min, Vector2 max ) {
			t.anchorMin = min;
			t.anchorMax = max;
		}
		
		
		public static void SetAnchorsAndFitCorners( this RectTransform t, Vector2 min, Vector2 max ) {
			t.anchorMin = min;
			t.anchorMax = max;
			t.offsetMin = t.offsetMax = new Vector2(0, 0);
		}
		
		public static float GetAnchorWidth( this RectTransform t ) {
			return t.anchorMax.x - t.anchorMin.x;
		}
		
		public static float GetAnchorHeight( this RectTransform t ) {
			return t.anchorMax.y - t.anchorMin.y;
		}
		
	}

	public static class TextureUtils {
		public static Texture2D CreateTextureFromBytes( int w, int h, byte[] bytes ) {
			Debug.Log("CameraCaptureKit: Creating texture w/h=" + w +  ", " + h + " bytes=" + bytes.Length );
			var tex = new Texture2D(w,h );
			tex.LoadImage( bytes );
			return tex;
		}
	}

	static public class TextureExtensions 
	{


		/*static Texture2D MakeSquare( this Texture2D t) {
			int x = t.width;
			int y = t.height;
			int overflow = 0;
			Texture2D te;
			if (x > y) {
				overflow = x-y;
				te = new Texture2D(y, y);
				te.SetPixels(t.GetPixels(overflow/2, 0, y, y));
			}
			else {
				overflow = y-x;
				te = new Texture2D(x, x);
				te.SetPixels(t.GetPixels(0, overflow/2, x, x));
				
			}
			te.Apply();
			return te;
		}*/


		public static Texture2D ScaledDown(this Texture2D oldTex)
		{
			var oldPixels = oldTex.GetPixels32();
			var newTexture = new Texture2D(oldTex.width/2, oldTex.height/2);
			for (int i=0; i<oldTex.width; i++)
			{
				for (int j=0; j<oldTex.height; j++)
				{
					int newX = i/2;
					int newY = j/2;
					try
					{
						newTexture.SetPixel(newX, newY, oldPixels [i + j * oldTex.width]);
					} catch (System.Exception exc)
					{
						Debug.Log("Exception: " + exc.Message);
						Debug.LogError("i: " + newX + ", " + "j: " + newY + ",id: " + (j + i * oldTex.width)); 
						return null;
					}
				}
			}
			newTexture.Apply();
			return newTexture;
		}


		public static Texture2D SameSame(this Texture2D oldTex) {
			var oldPixels = oldTex.GetPixels32();
			var newTexture = new Texture2D(oldTex.width, oldTex.height);
			for (int i=0; i<oldTex.width; i++)
			{
				for (int j=0; j<oldTex.height; j++)
				{
					int newX = i;
					int newY = j;
					try
					{
						newTexture.SetPixel(newX, newY, oldPixels [i + j * oldTex.width]);
					} catch (System.Exception exc)
					{
						Debug.Log("Exception: " + exc.Message);
						Debug.LogError("i: " + newX + ", " + "j: " + newY + ",id: " + (j + i * oldTex.width)); 
						return null;
					}
				}
			}
			newTexture.Apply();
			return newTexture;
		}


		public static Texture2D FlipHoriz(this Texture2D oldTex)
		{
			var oldPixels = (Color32[])oldTex.GetPixels32().Clone();
			//oldPixels.C
			var newTexture = new Texture2D(oldTex.width, oldTex.height);
			for( int i=0; i<oldTex.width; i++ )
			{
				for (int j=0; j<oldTex.height; j++)
				{
					int newX = oldTex.width-i;
					int newY = j;
					try
					{
						newTexture.SetPixel( newX, newY, oldPixels [i + j * oldTex.width] );
					} catch (System.Exception exc)
					{
						Debug.Log("Exception: " + exc.Message);
						Debug.LogError("i: " + newX + ", " + "j: " + newY + ",id: " + (j + i * oldTex.width)); 
						return null;
					}
				}
			}
			newTexture.Apply();
			return newTexture;
		}


		public static Texture2D RotatedLeft(this Texture2D oldTex, Color32[] pixels ) {
			var oldPixels = pixels;
			var newTexture = new Texture2D(oldTex.height, oldTex.width);
			for (int i=0; i<oldTex.height; i++)
			{
				for (int j=0; j<oldTex.width; j++)
				{
					int newX = oldTex.width - j;
					int newY = i;
					try
					{
						newTexture.SetPixel(newY, newX, oldPixels [j + i * oldTex.width]);
					} catch (System.Exception exc)
					{
						Debug.Log("Exception: " + exc.Message);
						Debug.LogError("i: " + newX + ", " + "j: " + newY + ",id: " + (j + i * oldTex.width)); 
						return null;
					}
				}
			}
			newTexture.Apply();
			return newTexture;
		}


		public static Texture2D RotatedLeft(this Texture2D oldTex) {
			var oldPixels = oldTex.GetPixels32();
			var newTexture = new Texture2D(oldTex.height, oldTex.width);
			for (int i=0; i<oldTex.height; i++)
			{
				for (int j=0; j<oldTex.width; j++)
				{
					int newX = oldTex.width - j;
					int newY = i;
					try
					{
						newTexture.SetPixel(newY, newX, oldPixels [j + i * oldTex.width]);
					} catch (System.Exception exc)
					{
						Debug.Log("Exception: " + exc.Message);
						Debug.LogError("i: " + newX + ", " + "j: " + newY + ",id: " + (j + i * oldTex.width)); 
						return null;
					}
				}
			}
			newTexture.Apply();
			return newTexture;
		}

		public static Texture2D RotatedLeftOptimized(this Texture2D oldTex)
		{
			var oldPixels = oldTex.GetPixels32();
			var newTexture = new Texture2D(oldTex.height, oldTex.width);
			for (int i=0; i<oldTex.height; i++)
			{
				for (int j=0; j<oldTex.width; j++)
				{
					int newX = oldTex.width - j;
					int newY = i;
					try {
						newTexture.SetPixel(newY, newX, oldPixels [j + i * oldTex.width]);
					} catch (System.Exception exc) {
						Debug.Log("Exception: " + exc.Message);
						Debug.LogError("i: " + newX + ", " + "j: " + newY + ",id: " + (j + i * oldTex.width)); 
						return null;
					}
				}
			}
			newTexture.Apply();
			return newTexture;
		}


		public static Texture2D Copy( this WebCamTexture src, CopyRotate rotate, CopyFlip flip ) {
			Color32[] c = src.GetPixels32();
			int src_width = src.width;
			int src_height = src.height;
			return CreateTextureFromColors( c, src_width, src_height, rotate, flip );
		}



		public static Texture2D CreateTextureFromColorsHW( Color32[] c, int src_width, int src_height, CopyRotate rotate, CopyFlip flip ) {

			Texture2D dst = null;
			if( rotate == CopyRotate.None ) {
				dst = new Texture2D( src_width, src_height, TextureFormat.RGB24, false );
				dst.name = "Texture Copy";
				dst.SetPixels32( c );
			} 



			return dst;
		}


		public static Texture2D CreateTextureFromColors( Color32[] c, int src_width, int src_height, CopyRotate rotate, CopyFlip flip ) {
			Texture2D dst = null;

			if( rotate == CopyRotate.None ) {
				dst = new Texture2D( src_width, src_height, TextureFormat.RGB24, false );
				dst.name = "Texture Copy";
				dst.SetPixels32( c );
			}
			else if( rotate == CopyRotate.Left ) {
				dst = new Texture2D(src_height, src_width);
				dst.name = "Texture Copy";
				for (int i=0; i<src_height; i++) {
					for (int j=0; j<src_width; j++) {
						int newX = src_width - j;
						int newY = i;
						try {
							dst.SetPixel(newY, newX, c [j + i * src_width]);
						} catch (System.Exception exc) {
							Debug.LogError( "Exception: " + exc.Message );
							Debug.LogError( "i: " + newX + ", " + "j: " + newY + ",id: " + (j + i * src_width) ); 
							return null;
						}
					}
				}
			}
			else if( rotate == CopyRotate.LeftX2 ) {
				dst = new Texture2D(src_width,src_height);
				for (int i=0; i<src_height; i++) {
					for (int j=0; j<src_width; j++) {
						int newX = src_width  - j;
						int newY = src_height - i;
						try {
							dst.SetPixel( newX, newY, c [j + i * src_width] );
						} catch (System.Exception exc) {
							Debug.LogError( "Exception: " + exc.Message );
							Debug.LogError( "i: " + newX + ", " + "j: " + newY + ",id: " + (j + i * src_width) ); 
							return null;
						}
					}
				}
				/*c = (Color32[])dst.GetPixels32().Clone(); // clone the array shift it once again,,,, hacky solution.. todo rewrit the top code to fit it.
				for (int i=0; i<src_height; i++) {
					for (int j=0; j<src_width; j++) {
						int newX = src_width - j;
						int newY = i;
						try {
							dst.SetPixel( newY, newX, c [j + i * src_width] );
						} catch (System.Exception exc) {
							Debug.LogError( "Exception: " + exc.Message );
							Debug.LogError( "i: " + newX + ", " + "j: " + newY + ",id: " + (j + i * src_width) ); 
							return null;
						}
					}
				}*/
			}
			else if( rotate == CopyRotate.Right ) {
				dst = new Texture2D(src_height, src_width);
				dst.name = "Texture Copy";
				for (int i=0; i<src_height; i++) {
					for (int j=0; j<src_width; j++) {
						int newX = j;
						int newY = src_height-i;
						try {
							dst.SetPixel(newY, newX, c [j + i * src_width]);
						} catch (System.Exception exc) {
							Debug.LogError("Exception: " + exc.Message);
							Debug.LogError("i: " + newX + ", " + "j: " + newY + ",id: " + (j + i * src_width)); 
							return null;
						}
					}
				}
			}

			// Flip horizontal.
			if( flip ==  CopyFlip.Horisontal ) {
				var oldPixels = (Color32[])dst.GetPixels32().Clone();
				for( int i=0; i<dst.width; i++ ) {
					for (int j=0; j<dst.height; j++) {
						int newX = dst.width-i;
						int newY = j;
						try {
							dst.SetPixel( newX, newY, oldPixels [i + j * dst.width] );
						} catch (System.Exception exc) {
							Debug.LogError( "Exception: " + exc.Message);
							Debug.LogError( "i: " + newX + ", " + "j: " + newY + ",id: " + (j + i * dst.width)); 
							return null;
						}
					}
				}
			}

			dst.Apply();
			return dst;
		}



		/*public static Texture2D CreateTextureFromBytes( byte[] bytes, int src_width, int src_height, CopyRotate rotate, CopyFlip flip ) {
			Texture2D dst = null;
			
			if( rotate == CopyRotate.None ) {
				dst = new Texture2D( src_width, src_height, TextureFormat.RGB24, false );
				dst.SetPixels32( bytes );
			}
			else if( rotate == CopyRotate.Left ) {
				dst = new Texture2D(src_height, src_width);
				for (int i=0; i<src_height; i++) {
					for (int j=0; j<src_width; j++) {
						int newX = src_width - j;
						int newY = i;
						try {
							dst.SetPixel(newY, newX, bytes [j + i * src_width]);
						} catch (System.Exception exc) {
							Debug.LogError( "Exception: " + exc.Message );
							Debug.LogError( "i: " + newX + ", " + "j: " + newY + ",id: " + (j + i * src_width) ); 
							return null;
						}
					}
				}
			}
			else if( rotate == CopyRotate.Right ) {
				dst = new Texture2D(src_height, src_width);
				for (int i=0; i<src_height; i++) {
					for (int j=0; j<src_width; j++) {
						int newX = j;
						int newY = src_height-i;
						try {
							dst.SetPixel(newY, newX, bytes [j + i * src_width]);
						} catch (System.Exception exc) {
							Debug.LogError("Exception: " + exc.Message);
							Debug.LogError("i: " + newX + ", " + "j: " + newY + ",id: " + (j + i * src_width)); 
							return null;
						}
					}
				}
			}
			
			// Flip horizontal.
			if( flip ==  CopyFlip.Horisontal ) {
				var oldPixels = (Color32[])dst.GetPixels32().Clone();
				for( int i=0; i<dst.width; i++ ) {
					for (int j=0; j<dst.height; j++) {
						int newX = dst.width-i;
						int newY = j;
						try {
							dst.SetPixel( newX, newY, oldPixels [i + j * dst.width] );
						} catch (System.Exception exc) {
							Debug.LogError( "Exception: " + exc.Message);
							Debug.LogError( "i: " + newX + ", " + "j: " + newY + ",id: " + (j + i * dst.width)); 
							return null;
						}
					}
				}
			}
			
			dst.Apply();
			return dst;
		}*/





		public static Texture2D RotatedRight(this Texture2D oldTex)
		{
			var oldPixels = oldTex.GetPixels32();
			var newTexture = new Texture2D(oldTex.height, oldTex.width);
			newTexture.name = "Copy of " + oldTex.name;
			for (int i=0; i<oldTex.height; i++)
			{
				for (int j=0; j<oldTex.width; j++)
				{
					int newX = j;
					int newY = oldTex.height-i;
					try
					{
						newTexture.SetPixel(newY, newX, oldPixels [j + i * oldTex.width]);
					} catch (System.Exception exc)
					{
						Debug.Log("Exception: " + exc.Message);
						Debug.LogError("i: " + newX + ", " + "j: " + newY + ",id: " + (j + i * oldTex.width)); 
						return null;
					}
				}
			}
			
			newTexture.Apply();
			return newTexture;
		}

	

	}
}