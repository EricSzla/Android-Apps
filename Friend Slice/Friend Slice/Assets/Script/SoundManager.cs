using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour {

	public static SoundManager Instance { set; get; }
	public AudioClip[] allSounds;
	public AudioClip[] sliceSounds;
	public AudioSource source;

	private void Start(){
		Instance = this;
		DontDestroyOnLoad (gameObject);
		SceneManager.LoadScene ("MainMenu");
	}

	public void PlaySound(int soundIndex){
		AudioSource.PlayClipAtPoint (allSounds [soundIndex], transform.position);
	}

	public void PlaySlice(int index){
		AudioSource.PlayClipAtPoint (sliceSounds [index], transform.position);
	}
}
