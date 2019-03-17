using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cuttable : MonoBehaviour {

	public GameObject CuttableObjectPrefab;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void OnCollisionEnter(Collision collision)
    {
        // foreach (ContactPoint contact in collision.contacts)
        // {
        //     Debug.DrawRay(contact.point, contact.normal, Color.white);
		// 	Debug.Log("butts");
        // }
		Collider c = collision.collider;
		if (c.tag == "lightsaber")
		{
			Debug.Log("lightbutt");
			Debug.Log(collision.relativeVelocity);
			Debug.Log(collision.contacts.Length); // usually 1, sometimes 2
			ContactPoint contact = collision.contacts[0];
			Debug.Log(contact.point - transform.position);
		}
    }

	private void Split()
	{
		// TODO:
		// Create two other CuttableObjectPrefabs
		// position and size correctly
		// remove this one
		// maybe add a force so that the two smaller objects split
	}
}
