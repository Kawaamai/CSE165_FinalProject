using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightsaberBlade : MonoBehaviour {

	LineRenderer lr;
	public Transform startPos;
	public Transform endPos;

	private float textureOffset = 0f;

	// Use this for initialization
	void Start () {
		lr = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		lr.SetPosition(0, startPos.position);
		lr.SetPosition(1, endPos.position);

		textureOffset -= Time.deltaTime*2f;
		if (textureOffset < -10f)
		{
			textureOffset += 10f;
		}
		lr.sharedMaterials[1].SetTextureOffset("_MainTex", new Vector2(textureOffset, 0f));
	}

	public void ExtendBlade()
	{
		lr.enabled = true;
	}

	public void RetractBlade()
	{
		lr.enabled = false;
	}
}
