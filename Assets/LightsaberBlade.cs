using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightsaberBlade : MonoBehaviour {

	public LineRenderer lr;
	public Transform startPos;
	public Transform endPos;
	private float length = 15f;
	float bladeExtendLerp = 0f;

	private float textureOffset = 0f;
	private bool extended;

	// Use this for initialization
	void Start () {
		extended = false;
		endPos.localPosition = startPos.localPosition;
		lr.SetPosition(0, startPos.position);
		lr.SetPosition(1, startPos.position);
	}

	// Update is called once per frame
	void Update () {

		lr.SetPosition(0, startPos.position);

		// FIXME: what is this?
		textureOffset -= Time.deltaTime*2f;
		if (textureOffset < -10f)
		{
			textureOffset += 10f;
		}
		lr.sharedMaterials[1].SetTextureOffset("_MainTex", new Vector2(textureOffset, 0f));

		if(!extended && lr.enabled == true)
		{
			bladeExtendLerp += Time.deltaTime * 2f;
			endPos.localPosition = Vector3.Lerp(startPos.localPosition, new Vector3(0f, length, 0f), bladeExtendLerp);
			if (bladeExtendLerp >= 1f)
				extended = true;
		}
		else
		{
			endPos.localPosition = startPos.localPosition + new Vector3(0f, length, 0f);
		}
		lr.SetPosition(1, endPos.position);
	}

	public void ExtendBlade()
	{
		if(GetComponent<AudioSource>() != null)
		{
			GetComponent<AudioSource>().Play();
		}
		lr.enabled = true;
		if(GetComponent<Collider>() != null)
		{
			GetComponent<Collider>().enabled = true;
		}
	}

	public void RetractBlade()
	{
		if (GetComponent<AudioSource>() != null)
		{
			GetComponent<AudioSource>().Stop();
		}
		lr.enabled = false;
		bladeExtendLerp = 0f;
		endPos.position = startPos.position;
		extended = false;
		if (GetComponent<Collider>() != null)
		{
			GetComponent<Collider>().enabled = false;
		}
	}
}
