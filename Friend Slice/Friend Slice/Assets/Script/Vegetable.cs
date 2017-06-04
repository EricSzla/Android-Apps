using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Vegetable : MonoBehaviour {

	public bool isActive{set;get;}
	public bool isSliced = false;
	private float verticalVelocity;
	private float speed;
	private float GRAVITY = 2.0f;
	private float freezeSpeed = 1f;

	public Sprite[] sprites;
	public Sprite[] coinSprites;
	private int coinIndex;
	private float coinTime;
	public SpriteRenderer sRenderer;
	private int spriteIndex;
	private float lastSpriteUpdate;
	private float spriteUpdateDelta = 0.3f;

	private bool isCoin = false;
	private bool isFriend = false;

	private void Start(){
		sRenderer = GetComponent<SpriteRenderer>();
	}

	public void LaunchVegetable(float verticalVelocity, float xSpeed, float xStart,Sprite happySprite, Sprite sadSprite, Sprite angrySprite,float fspeed,float grav ){
		this.GRAVITY = grav;
		this.freezeSpeed = fspeed;
		isActive = true;
		speed = xSpeed;
		this.verticalVelocity = verticalVelocity;
		transform.position = new Vector3 (xStart, 0, 5);
		isSliced = false;
		spriteIndex = 0; 
		sprites [0] = happySprite;
		sprites [1] = sadSprite;
		sprites [2] = angrySprite;
		Material newMat = Resources.Load("Circle", typeof(Material)) as Material;
		sRenderer.material = newMat;
		sRenderer.sprite = sprites [spriteIndex]; 
		this.isCoin = false;
		this.isFriend = true;
		this.GetComponent<Animation> ().enabled = false;
	}

	public void LaunchVegetable(float verticalVelocity, float xSpeed, float xStart,float fspeed,float grav){
		this.GRAVITY = grav;
		this.freezeSpeed = fspeed;
		isActive = true;
		speed = xSpeed;
		this.verticalVelocity = verticalVelocity;
		transform.position = new Vector3 (xStart, 0, 5);
		isSliced = false;
		spriteIndex = 0; 
		this.isCoin = false;
		sRenderer.sprite = sprites [spriteIndex]; 
	}

	public void LaunchCoin(float verticalVelocity, float xSpeed, float xStart,float fspeed,float grav){
		this.GRAVITY = grav;
		this.freezeSpeed = fspeed;
		isActive = true;
		speed = xSpeed;
		this.verticalVelocity = verticalVelocity;
		transform.position = new Vector3 (xStart, 0, 5);
		isSliced = false;
		coinIndex = 0;
		isCoin = true;
		this.coinTime = Time.time;
		sRenderer.sprite = coinSprites [coinIndex]; 
		Material mat = new Material (Shader.Find ("Sprites/Default"));
		sRenderer.material = mat;
	}
	
	// Update is called once per frame
	void Update () {
		if (!isActive) {
			return;
		}
			
		verticalVelocity -= GRAVITY * Time.deltaTime;
		transform.position += new Vector3 (speed, verticalVelocity, 0) * Time.deltaTime * freezeSpeed;

		if (isSliced) {
			if (!isCoin) {
				if (spriteIndex != sprites.Length - 1 && Time.time - lastSpriteUpdate > spriteUpdateDelta) {
					lastSpriteUpdate = Time.time;
					spriteIndex++;
					sRenderer.sprite = sprites [spriteIndex];
				}
			} else {
				if (coinIndex > 5) {
					coinIndex = 0;
				}
				sRenderer.sprite = coinSprites [coinIndex];
				coinIndex++;
				coinTime = Time.time;
			}
		} else {
			if (isCoin) {
				if (Time.time - coinTime > 0.1f) {
					if (coinIndex > 5) {
						coinIndex = 0;
					}
					sRenderer.sprite = coinSprites [coinIndex];
					coinIndex++;
					coinTime = Time.time;
				}
			}
		}

		if (transform.position.y < -1 && !isCoin) {
			isActive = false;
			if (!isSliced) {
				GameManager.Instance.LoseLP();
			}
		}

	}

	public void Slice(bool isBomb){
		if (isSliced)
			return;


		if (verticalVelocity < 0.5f) {
			verticalVelocity = 0.5f;
		}

		speed = speed * 0.5f;
		isSliced = true;

		if (!isCoin) {
			SoundManager.Instance.PlaySound (3);

			if (!isBomb) {
				int i = Random.Range (0, 14);
				SoundManager.Instance.PlaySlice (i);
			}

			GameManager.Instance.IncrementScore (1);
		} else {
			SoundManager.Instance.PlaySound (2);
			GameManager.Instance.IncrementCoin (1);
		}
	}

	public void Animate(){
		if (isFriend) {
			if (this.GetComponent<Animation> ().enabled) {
				this.GetComponent<Animation> ().enabled = false;
			}
			this.GetComponent<Animation> ().enabled = true;
			this.GetComponent<Animation> ().Play ();
		}
	}
	public void setGravity(){
		GRAVITY = 2.0f;
		freezeSpeed = 1.0f;
	}

	public void setFreezeGravity (){
		GRAVITY = 0.3f;
		freezeSpeed = 0.1f;
	}
}
