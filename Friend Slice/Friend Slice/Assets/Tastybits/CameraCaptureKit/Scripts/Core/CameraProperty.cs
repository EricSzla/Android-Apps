using System;


namespace Tastybits.CamCap {


	/**
	 * The Camera Property represents a property read from the
	 * native Camera SDK.
	 */
	public class CameraProperty {
		// The Name of the property.
		public string Name;
		// The Value of the property.
		public string Value;
		// A Set of available values.
		public string[] Available;


		/**
		 * Contains the camera property and the available values.
		 */
		public CameraProperty( string n, string v, string[] a ) {
			Name = n;
			Value = v;
			Available = a;
		}


		/**
		 * Returns a string with comma seperated values.
		 */
		public string StrAvailable {
			get {
				return string.Join(",",Available );
			}
		}
	}


}

