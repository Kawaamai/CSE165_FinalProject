using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenBox : MonoBehaviour {

	public GameObject crate;
	public GameObject crateLid;

    private void OnCollisionEnter(Collision collision)
    {
        Collider c = collision.collider;
        if (c.tag == "lightsaber")
        {
            SplitContainer();
        }
    }

	private void SplitContainer()
	{
		// unparent
		crate.transform.parent = null;
		crateLid.transform.parent = null;

		// give them rigidbodies
		Rigidbody crateRb = crate.AddComponent<Rigidbody>();
		Rigidbody crateLidRb = crateLid.AddComponent<Rigidbody>();

		crateRb.mass = 0.7f;
		crateLidRb.mass = 0.3f;

		// A force to push apart the lid and basebox
		crateRb.AddForce(transform.up * 100f);
		crateLidRb.AddForce((-1f * transform.up) * 100f);

		// Destroy this object
		Destroy(gameObject);
	}
}
