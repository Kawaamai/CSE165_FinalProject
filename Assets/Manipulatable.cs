using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manipulatable : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
		if (GameManager.Inst.CurrSelectedObj == this)
		{
			gameObject.GetComponent<Renderer>().material.color = Color.green;
		}
		else
		{
			gameObject.GetComponent<Renderer>().material.color = Color.white;
		}

	}
}
