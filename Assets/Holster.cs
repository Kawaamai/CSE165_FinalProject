using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Holster : MonoBehaviour {

	public Transform hmd;

	// Use this for initialization
	void Start () {
		transform.position = hmd.position + (0.5f * Vector3.up) + (0.4f * Vector3.Normalize(new Vector3(hmd.forward.x, 0f, hmd.forward.z)));
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = hmd.position - (0.5f * Vector3.up) + (0.4f * new Vector3(hmd.forward.x, 0f, hmd.forward.z));
	}
}
