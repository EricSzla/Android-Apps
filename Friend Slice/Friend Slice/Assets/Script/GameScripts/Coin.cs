using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Coin : MonoBehaviour {

	public bool isActive{set;get;}
	public bool isSliced = false;
	private float verticalVelocity;
	private float speed;
	private float GRAVITY = 2.0f;
	private float freezeSpeed = 1f;

	public Sprite[] sprites;
	public SpriteRenderer sRenderer;
	private int spriteIndex;
	private float lastSpriteUpdate;
	private float spriteUpdateDelta = 0.3f;

	private void Start(){
		sRenderer = GetComponent<SpriteRenderer>();
	}

	public void LaunchVegetable(float verticalVelocity, float xSpeed, float xStart,Sprite happySprite, Sprite sadSprite, Sprite angrySprite,float fspeed,float grav){
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
		sRenderer.sprite = sprites [spriteIndex]; 


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
		sRenderer.sprite = sprites [spriteIndex]; 

	}

	// Update is called once per frame
	void Update () {
		if (!isActive) {
			return;
		}

		verticalVelocity -= GRAVITY * Time.deltaTime;
		transform.position += new Vector3 (speed, verticalVelocity, 0) * Time.deltaTime * freezeSpeed;

		if (isSliced) {
			if (spriteIndex != sprites.Length -1  && Time.time - lastSpriteUpdate > spriteUpdateDelta) {
				lastSpriteUpdate = Time.time;
				spriteIndex++;
				sRenderer.sprite = sprites [spriteIndex];
			}
		}

		if (transform.position.y < -1) {
			isActive = false;
			if (!isSliced) {
				GameManager.Instance.LoseLP();
			}
		}

	}

	public void Slice(){
		if (isSliced)
			return;

		if (verticalVelocity < 0.5f) {
			verticalVelocity = 0.5f;
		}

		speed = speed * 0.5f;
		isSliced = true;

		GameManager.Instance.IncrementScore (1);
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
