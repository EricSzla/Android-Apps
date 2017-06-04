using UnityEngine;
using System.Collections;

public class AdjustExposureCompensationController : MonoBehaviour {
	public RectTransform container;
	public RectTransform template;

	public bool testData = false;
	void Awake(){
		if( testData ) {
			SetupWithRange( -1, 4 );
		}
		template.gameObject.SetActive(false);

	}


	public void SetupWithRange( int min , int max ) {
		this.gameObject.SetActive(true);
		float _height = template.sizeDelta.y;
		if( _height == 0f ) {
			Vector3[] corners = new Vector3[4];
			template.GetLocalCorners(corners);
			var size = corners[2] - corners[0];
			_height = size.y;
		}

		float _con_height = container.sizeDelta.y;
		if( _con_height == 0f ) {
			Vector3[] corners = new Vector3[4];
			container.GetLocalCorners(corners);
			var size = corners[2] - corners[0];
			_con_height = size.y;
		}

		_height = _con_height / 10f;
		if( _height < (Screen.height / 15f) ) _height = (Screen.height / 15f);


		bool deleted=true;
		int itermax=0;
		while(deleted && itermax++<100) {
			deleted=false;
			for( int i  = 0; i<container.childCount; i++ ) {
				if( container.GetChild(i).name.Contains("template" )==false ) {
					GameObject.Destroy( container.GetChild(i).gameObject );
					deleted=true;
					break;
				} 
			}
		}

		int iter=0;
		int idx= 0;
		template.gameObject.SetActive(true);
		for( int i  = min; i<=max+1 && iter++<100; i++ ) {
			if( i == max+1 && Application.platform != RuntimePlatform.IPhonePlayer ) {
				// dont allow the auto button on iOS
				break;
			}
			var newObj = GameObject.Instantiate( template.gameObject );
			//newObj.name = "obj #" + (idx+1) + " val=" + i;
			var rtf = newObj.GetComponent<RectTransform>();
			rtf.SetParent( template.parent.GetComponent<RectTransform>(), false );
			var text = rtf.FindChild("Text");
			if( i == max+1 ) {
				text.GetComponent<UnityEngine.UI.Text>().text = "Auto"; // dont allow the auto..
				newObj.name = ""+(-999);
			} else {
				text.GetComponent<UnityEngine.UI.Text>().text = "" + i;
				newObj.name = ""+i;
			}
			var lp = rtf.anchoredPosition;
			lp.y = (-_height) * idx;
			rtf.anchoredPosition = lp;
			var sz = rtf.sizeDelta;
			sz.y = _height;
			rtf.sizeDelta = sz;
			idx++;
		}
		template.gameObject.SetActive(false);
	}

	public void OnOtherButtonClicked( GameObject goButton ){
		if( goButton.name.Contains("template") ) {
			Debug.Log("cannot click the template");
			return;
		}
		var i = int.Parse( goButton.name );
		if(OnDidDismiss!=null) OnDidDismiss ( i );
		OnDidDismiss=null;
		this.gameObject.SetActive(false);
	}


	public System.Action<int> OnDidDismiss = null;
}
