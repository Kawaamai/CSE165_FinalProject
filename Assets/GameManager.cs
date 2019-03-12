using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public static GameManager Inst;

	public Transform hmd;
	public Transform[] controllers;

	private Manipulatable currSelectedObj;
	public Manipulatable CurrSelectedObj
	{
		get
		{
			return this.currSelectedObj;
		}
	}
	private float initRHandObjDist;
	private Quaternion rHandDiffQuat;
	private Vector3 rHandLastPos;

	// Use this for initialization
	void Start () {
		Inst = this;
	}

	private void castRay(int controller)
	{
		Ray ray = new Ray(controllers[controller].position, controllers[controller].position - hmd.position);
		Manipulatable[] objs = (Manipulatable[])FindObjectsOfType<Manipulatable>();
		float min = float.MaxValue;
		int idx = 0;
		for (int i = 0; i < objs.Length; i++)
		{
			float dist = Vector3.Cross(ray.direction, objs[i].transform.position - ray.origin).magnitude;
			if (dist < min && dist < (Mathf.Sin(15f * Mathf.Deg2Rad) * Vector3.Distance(objs[i].transform.position, hmd.position)))
			{
				min = dist;
				idx = i;
			}
		}
		if (min != float.MaxValue)
			currSelectedObj = objs[idx];
		else
			currSelectedObj = null;

	}
	
	// Update is called once per frame
	void Update () {

		if(OVRInput.GetDown(OVRInput.RawButton.RHandTrigger) && currSelectedObj != null)
		{
			Quaternion headToController = Quaternion.LookRotation(controllers[1].position - hmd.position);
			Quaternion headToObject = Quaternion.LookRotation(currSelectedObj.transform.position - hmd.position);
			rHandDiffQuat = headToObject* Quaternion.Inverse(headToController);
			initRHandObjDist = Vector3.Distance(controllers[1].position, currSelectedObj.transform.position);
		}
		else if(OVRInput.Get(OVRInput.RawButton.RHandTrigger) && currSelectedObj != null)
		{
			currSelectedObj.gameObject.GetComponent<Rigidbody>().useGravity = false;
			Vector3 direction = controllers[1].position - hmd.position;
			direction = rHandDiffQuat * direction;
			direction.Normalize();
			currSelectedObj.gameObject.GetComponent<Rigidbody>().MovePosition(controllers[1].position + direction * initRHandObjDist);
			rHandLastPos = controllers[1].position;
		}
		else if(OVRInput.GetUp(OVRInput.RawButton.RHandTrigger) && currSelectedObj != null)
		{
			currSelectedObj.gameObject.GetComponent<Rigidbody>().useGravity = true;
			currSelectedObj.gameObject.GetComponent<Rigidbody>().AddForce((controllers[1].position - rHandLastPos) * 30000f);
			currSelectedObj = null;
			castRay(1);
		}
		else
		{
			castRay(1);
		}

	}
}
