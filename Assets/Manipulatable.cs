using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manipulatable : MonoBehaviour {

	public GameObject indicator;

	// Use this for initialization
	void Start () {
		if (GetComponent<Rigidbody>() == null)
		{
			Debug.LogError("Error: Manipulatable objects should have Rigidbodies.");
		}
	}
	
	// Update is called once per frame
	void Update () {
		
		if (GameManager.Inst.CurrSelectedRObj == this || GameManager.Inst.CurrSelectedLObj == this || GameManager.Inst.CurrLHandObservedObj == this || GameManager.Inst.CurrRHandObservedObj == this)
		{
			indicator.SetActive(true);
		}
		else
		{
			indicator.SetActive(false);
		}

	}
}
