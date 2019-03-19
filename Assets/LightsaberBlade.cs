﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightsaberBlade : MonoBehaviour {

	public LineRenderer lr;
	public Transform startPos;
	public Transform endPos;
	private float length = 15f;
	float bladeExtendLerp = 0f;

	private float textureOffset = 0f;

	// Use this for initialization
	void Start () {
		endPos.localPosition = startPos.localPosition;
		lr.SetPosition(0, startPos.position);
		lr.SetPosition(1, startPos.position);
	}

	// Update is called once per frame
	void Update () {
		lr.SetPosition(0, startPos.position);

		textureOffset -= Time.deltaTime*2f;
		if (textureOffset < -10f)
		{
			textureOffset += 10f;
		}
		lr.sharedMaterials[1].SetTextureOffset("_MainTex", new Vector2(textureOffset, 0f));

		if(lr.enabled == true)
		{
			bladeExtendLerp += Time.deltaTime * 2f;
			endPos.localPosition = Vector3.Lerp(startPos.localPosition, new Vector3(0f, length, 0f), bladeExtendLerp);
		}
		lr.SetPosition(1, endPos.position);
	}

	public void ExtendBlade()
	{
		lr.enabled = true;
		if(GetComponent<Collider>() != null)
		{
			GetComponent<Collider>().enabled = true;
		}
	}

	public void RetractBlade()
	{
		lr.enabled = false;
		bladeExtendLerp = 0f;
		endPos.position = startPos.position;
		if (GetComponent<Collider>() != null)
		{
			GetComponent<Collider>().enabled = false;
		}
	}
}
