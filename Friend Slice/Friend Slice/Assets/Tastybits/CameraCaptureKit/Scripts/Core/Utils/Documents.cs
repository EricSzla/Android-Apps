#pragma warning disable 414
#define UNITY_DOTNET_SUBSET
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Linq;


namespace Tastybits.CamCap {
	
	/**
	 * @Desc This class can be used as a utility for saving and reading Textures to the Document
	 * Directory. 
	 * 
	 * Use Documents.LoadTextureFromDocuments( "mytexture.png", out texture ) To Load a texture.
	 * The method returns true if succeeded, false if not. 
	 * 
	 * Use Documents.SaveTextureToDocuments("mytexure.png", texture ) to save a texture.
	 * 
	 */
	public class Documents {
		static string _documentDirectoryString=null;

		static string GetDocumentsDirectoryOniOS() {
			if( Application.platform != RuntimePlatform.IPhonePlayer ) {

				return Application.persistentDataPath;
			}
			if( string.IsNullOrEmpty(_documentDirectoryString) ) {
				_documentDirectoryString = _GetDocumentDirectory();
			}
			return _documentDirectoryString;
		}
			

		static string AlbumSaveFolder = "/Unity";
#if UNITY_ANDROID
		private static byte[] _Documents_LoadFromPhotosAlbum( string fn  ){
			var cls = new AndroidJavaClass("com.CameraCaptureKit.Documents");
			var obj = cls.CallStatic<AndroidJavaObject>( "loadImageFromExternal", new object[] { fn, AlbumSaveFolder } );
			if (obj.GetRawObject().ToInt32() != 0) {
				byte[] result = AndroidJNIHelper.ConvertFromJNIArray<byte[]>(obj.GetRawObject());
				if( result == null || result.Length == 0 ) {
					Debug.LogError("Error returning the bytes from the byte array (2)");
				}
				return result;	
			}
			return null;
		}
		private static void _Documents_WriteToSavedPhotosAlbum( string fn, byte[] bytes, int nBtes ){
			var cls = new AndroidJavaClass("com.CameraCaptureKit.Documents");
			string s = System.Convert.ToBase64String(bytes);
			bool ok = cls.CallStatic<bool>( "saveImageToExternal", new object[] { fn, s, AlbumSaveFolder } );
			Debug.Log("Documents: saved to photoalbum : " + ok );
		}
		private static string _GetDocumentDirectory() {
			return UnityEngine.Application.persistentDataPath;
		}
#elif UNITY_IPHONE
		[DllImport ("__Internal")]
		private static extern void _Documents_WriteToSavedPhotosAlbum( string fn_dummy, byte[] bytes, int nBytes );
		[DllImport ("__Internal")]
		private static extern string _GetDocumentDirectory();
		private static byte[] _Documents_LoadFromPhotosAlbum( string fn  ){
			throw new System.Exception("Not implemented for Platform : " + Application.platform );
		}
#else
		private static byte[] _Documents_LoadFromPhotosAlbum( string fn  ){
			throw new System.Exception("Not implemented for Platform : " + Application.platform );
		}
		static string _GetDocumentDirectory() {
			Debug.LogError("_GetDocumentDirectory - Not available on this platform : " + Application.platform );
			return "";
		}
		static void _Documents_WriteToSavedPhotosAlbum( string fn, byte[] bytes, int nBtes ) {
			Debug.LogError("_Documents_WriteToSavedPhotosAlbum - Not available on this platform : " + Application.platform );
			return;
		}
#endif

		public static void Delete( string fn ) {
			string _path = System.IO.Path.Combine(Documents.DocumentsPath, fn);
			if( System.IO.File.Exists(_path ) ) {
				System.IO.File.Delete(_path);
				if( System.IO.File.Exists(_path ) ) {
					Debug.LogError("Error: failed to delete file at path : " + _path );
				} else {			
					Debug.Log("Deleted file from documents : " + fn );
				}
			} else {
				Debug.LogError("Error: Cannot delete file at path " + _path );
			}
		}

		public static Texture2D LoadTextureFromAndroidPhotosAlbum( string fName ) {
			byte[] bytes = _Documents_LoadFromPhotosAlbum( fName );
			if( bytes == null ) {
				Debug.LogError("Error loading texture : " + fName );
				return null;
			}
			var ret = new Texture2D( 2,2 );
			ret.LoadImage( bytes );
			return ret;
		}

		public static bool LoadTextureFromDocuments( string fname, out Texture2D retval ) {
			string path = System.IO.Path.Combine(DocumentsPath, fname);
			if( !System.IO.File.Exists(path) ) {
				Debug.Log("Documents: TryLoadFromDocuments file at address does not exist : " + path);
				retval = null;

				return false;
			}

			byte[] bytes = null;
#if UNITY_DOTNET_SUBSET			
			var f = System.IO.File.OpenRead( path );
			byte[] buf = new byte[1024];
			int read=0;
			int iter=0;
			System.IO.MemoryStream ms = new System.IO.MemoryStream();
			while( ( read = f.Read( buf, 0, 1024 ) ) > 0 ) {
				ms.Write( buf, 0, read );
				if( iter++>(1024*15)) // 
					throw new System.Exception("too big - images more than 15 mb not supported");
			} 
			bytes = ms.ToArray(); 
#else
			bytes = System.IO.File.ReadAllBytes( path );
#endif
			if( bytes==null ) {
				Debug.LogError("Documents: Error calling ReadAllBytes at Path : " + path );
				retval = null;
				return false;
			}

			var tex = new Texture2D(2,2);
			tex.name = "Loaded from Docs";
			tex.LoadImage( bytes );
			retval = tex;

			return true;
		}


		public static string SaveTextureToDocuments( string fname, Texture2D texture ) {
			byte[] bytes = texture.EncodeToPNG();
			string path = System.IO.Path.Combine(DocumentsPath, fname);
#if UNITY_DOTNET_SUBSET
			var f = System.IO.File.OpenWrite( path );
			f.Write( bytes, 0, bytes.Length );
			f.Close();
#else
			System.IO.File.WriteAllBytes( path, bytes );
#endif
			return path;
		}


		public static string SavePNGBytesToDocuments( string fname, byte[] bytes ) {
			string path = System.IO.Path.Combine(DocumentsPath, fname);
#if UNITY_DOTNET_SUBSET
			var f = System.IO.File.OpenWrite( path );
			f.Write( bytes, 0, bytes.Length );
			f.Close();
#else
			System.IO.File.WriteAllBytes( path, bytes );
#endif
			return path;
		}


		public static void SaveTextureToPhotoAlbum( string fn, Texture2D texture ) {
			if( !fn.ToLower().EndsWith(".png") ) fn += ".png";
			if( fn.Contains("/" ) && Application.platform == RuntimePlatform.Android ) {
				Debug.LogError("Error the resulting filename cannot contain / characters" );
			}
			if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
				var bytes = texture.EncodeToPNG ();
				_Documents_WriteToSavedPhotosAlbum (fn, bytes, bytes.Length);
			} else if( Application.isEditor ) {
				SaveTextureToDocuments (fn, texture);
			} else {
				Debug.LogError("Error: Saving texture to album is disabled for platform : " + Application.platform );
			}
		}


		public static void SavePNGImageToPhotoAlbum( string fn, byte[] png_data ) {
			if( !fn.ToLower().EndsWith(".png") ) fn += ".png";
			if( fn.Contains("/" ) && Application.platform == RuntimePlatform.Android ) {
				Debug.LogError("Error the resulting filename cannot contain / characters" );
			}
			if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
				_Documents_WriteToSavedPhotosAlbum (fn, png_data, png_data.Length);
			} else if( Application.isEditor ) {
				SavePNGBytesToDocuments (fn, png_data);
			} else {
				Debug.LogError("Saving texture to album is disabled for platform : " + Application.platform );
			}
		}


		public static string DocumentsPath {
			get {
				return GetDocumentsPath();
			}
		}

		  
		static string GetDocumentsPath()  { 
			// Your game has read+write access to /var/mobile/Applications/XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX/Documents 
			// Application.dataPath returns              
			// /var/mobile/Applications/XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX/myappname.app/Data 
			// Strip "/Data" from path 
			if( Application.platform == RuntimePlatform.IPhonePlayer ) {
				string testDocDir = GetDocumentsDirectoryOniOS();
				//MiscNativStuff.Break( "Tell me this : " +  testDocDir );
				if( !string.IsNullOrEmpty(testDocDir) ) {
					return testDocDir;
				}
				Debug.LogError("Documents: Error getting documents path via native method.. retrying with Unity method");
				if( Application.dataPath.EndsWith( "/Data" ) ) {
					string path = Application.dataPath.Substring (0, Application.dataPath.Length - 5); 
					// Strip application name 
					path = path.Substring(0, path.LastIndexOf('/'));   // strip app name from path,,,
					path =  path +  "/Documents"; // add documnets to path.
					//Debug.Log ("Resulting documents path : " + path );
					return path; 
				} 
				Debug.LogError("Documents: Error datapath did not end with  /Data : " + Application.dataPath );

				Debug.LogError("Documents: Error generating document path ");
				return "";
			} else if( Application.isEditor ) {
				if( Application.dataPath.EndsWith( "/Assets" )==false ) {
					Debug.LogError("Documents: Error datapath does not end with Assets");
				}
				string path = Application.dataPath.Substring( 0, Application.dataPath.Length - 6 ); 
				string ret = path + "/Documents";
				if( System.IO.Directory.Exists( ret ) == false ) {
					System.IO.Directory.CreateDirectory( ret );
				}
				return ret;
			}
			// Document Path for Android.
			return Application.persistentDataPath;
		}


	}

}