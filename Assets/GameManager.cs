using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public enum Hand
	{
		LEFT = 0,
		RIGHT = 1
	}

	public static GameManager Inst;

	public Transform hmd;
	public Transform[] controllers;
	public GameObject[] testObjects;

	private Manipulatable currSelectedRObj;
	public Manipulatable CurrSelectedRObj
	{
		get
		{
			return this.currSelectedRObj;
		}
	}
	private Manipulatable currSelectedLObj;
	public Manipulatable CurrSelectedLObj
	{
		get
		{
			return this.currSelectedLObj;
		}
	}
	private float rHandObjDist;
	private Quaternion rHandDiffQuat;
	private Vector3 rHandLastPos;
	private float initRHandDistFromHmd;

	private float lHandObjDist;
	private Quaternion lHandDiffQuat;
	private Vector3 lHandLastPos;
	private float initLHandDistFromHmd;

	// Use this for initialization
	void Start () {
		Inst = this;
	}

	private void castRay(Hand hand)
	{
		int controller = (int)hand;
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
		if (min != float.MaxValue && controller == 1)
		{
			currSelectedRObj = objs[idx];
		}
		else if (min != float.MaxValue && controller == 0)
		{
			currSelectedLObj = objs[idx];
		}
		else if (min == float.MaxValue && controller == 1)
		{
			currSelectedRObj = null;
		}
		else
		{
			currSelectedLObj = null;
		}

	}

	private void initializeGrab(Hand hand)
	{
		int controller = (int)hand;
		// Calculate Quaternion distance between the head to the controller and the head to the object
		Quaternion headToController = Quaternion.LookRotation(controllers[controller].position - hmd.position);
		if(controller == 1)
		{
			Quaternion headToObject = Quaternion.LookRotation(currSelectedRObj.transform.position - hmd.position);
			rHandDiffQuat = headToObject * Quaternion.Inverse(headToController);
			//Save the distances from the hand to the object and the hand from the headset.
			rHandObjDist = Vector3.Distance(controllers[controller].position, currSelectedRObj.transform.position);
			initRHandDistFromHmd = Vector3.Distance(controllers[controller].position, hmd.position);
			currSelectedRObj.GetComponent<Rigidbody>().velocity = Vector3.zero;
		}
		else
		{
			Quaternion headToObject = Quaternion.LookRotation(currSelectedLObj.transform.position - hmd.position);
			lHandDiffQuat = headToObject * Quaternion.Inverse(headToController);
			//Save the distances from the hand to the object and the hand from the headset.
			lHandObjDist = Vector3.Distance(controllers[controller].position, currSelectedLObj.transform.position);
			initLHandDistFromHmd = Vector3.Distance(controllers[controller].position, hmd.position);
			currSelectedLObj.GetComponent<Rigidbody>().velocity = Vector3.zero;
		}

	}
	private void maintainGrab(Hand hand)
	{
		int controller = (int)hand;
		if (controller == 1)
		{
			currSelectedRObj.gameObject.GetComponent<Rigidbody>().useGravity = false;
		}
		else
		{
			currSelectedLObj.gameObject.GetComponent<Rigidbody>().useGravity = false;
		}
		//Find the direction of the raycast
		Vector3 direction = controllers[controller].position - hmd.position;
		//Add the quaternion difference to get the direction toward the object's new position
		if (controller == 1)
			direction = rHandDiffQuat * direction;
		else
			direction = lHandDiffQuat * direction;
		direction.Normalize();
		//Get the distance of the controller from the hmd to calculate how to move the object
		float currDistFromHmd = Vector3.Distance(controllers[controller].position, hmd.position);
		//Use the following value to determine how much to move the object:
		float movementAmt = 0f;
		if (controller == 1)
		{
			movementAmt = currDistFromHmd - initRHandDistFromHmd;
		}
		else
		{
			movementAmt = currDistFromHmd - initLHandDistFromHmd;
		}
		if (movementAmt > 0)
			movementAmt *= 2.5f;
		if (controller == 1)
			rHandObjDist = rHandObjDist + movementAmt * 15f * Time.deltaTime;
		else
			lHandObjDist = lHandObjDist + movementAmt * 15f * Time.deltaTime;
		Vector3 newPos = Vector3.zero;
		if (controller == 1)
			newPos = controllers[controller].position + direction * rHandObjDist;
		else
			newPos = controllers[controller].position + direction * lHandObjDist;
		Vector3 velocity = Vector3.zero;
		if (controller == 1)
		{
			currSelectedRObj.gameObject.GetComponent<Rigidbody>().MovePosition(Vector3.SmoothDamp(currSelectedRObj.transform.position, newPos, ref velocity, 0.05f));
			rHandLastPos = controllers[1].position;
		}
		else
		{
			currSelectedLObj.gameObject.GetComponent<Rigidbody>().MovePosition(Vector3.SmoothDamp(currSelectedLObj.transform.position, newPos, ref velocity, 0.05f));
			lHandLastPos = controllers[0].position;
		}
	}

	private void endGrab(Hand hand)
	{
		int controller = (int)hand;
		if (controller == 1)
		{
			//TODO - Calculate magnitude of change along controller's forward axis.  Add to magnitude.
			currSelectedRObj.gameObject.GetComponent<Rigidbody>().useGravity = true;
			Vector3 direction = controllers[1].position - rHandLastPos;
			direction.Normalize();
			float magnitude = Vector3.Distance(currSelectedRObj.transform.position, hmd.position);
			magnitude = magnitude / Vector3.Distance(controllers[1].position, hmd.position);
			currSelectedRObj.gameObject.GetComponent<Rigidbody>().AddForce(direction * magnitude * 75f);
			currSelectedRObj = null;
		}
		else
		{
			currSelectedLObj.gameObject.GetComponent<Rigidbody>().useGravity = true;
			Vector3 direction = controllers[0].position - lHandLastPos;
			direction.Normalize();
			float magnitude = Vector3.Distance(currSelectedLObj.transform.position, hmd.position);
			magnitude = magnitude / Vector3.Distance(controllers[0].position, hmd.position);
			currSelectedLObj.gameObject.GetComponent<Rigidbody>().AddForce(direction * magnitude * 75f);
			currSelectedLObj = null;
		}
	}
	
	// Update is called once per frame
	void Update () {

		if(OVRInput.GetDown(OVRInput.RawButton.RHandTrigger) && currSelectedRObj != null)
		{
			initializeGrab(Hand.RIGHT);
		}
		else if(OVRInput.Get(OVRInput.RawButton.RHandTrigger) && currSelectedRObj != null)
		{
			maintainGrab(Hand.RIGHT);
		}
		else if(OVRInput.GetUp(OVRInput.RawButton.RHandTrigger) && currSelectedRObj != null)
		{
			endGrab(Hand.RIGHT);
			castRay(Hand.RIGHT);
		}
		else
		{
			castRay(Hand.RIGHT);
		}

		if (OVRInput.GetDown(OVRInput.RawButton.LHandTrigger) && currSelectedLObj != null)
		{
			initializeGrab(Hand.LEFT);
		}
		else if (OVRInput.Get(OVRInput.RawButton.LHandTrigger) && currSelectedLObj != null)
		{
			maintainGrab(Hand.LEFT);
		}
		else if (OVRInput.GetUp(OVRInput.RawButton.LHandTrigger) && currSelectedLObj != null)
		{
			endGrab(Hand.LEFT);
			castRay(Hand.LEFT);
		}
		else
		{
			castRay(Hand.LEFT);
		}

		if (OVRInput.GetUp(OVRInput.RawButton.A))
		{
			testObjects[0].transform.position = new Vector3(0f, 2f, 8f);
			testObjects[1].transform.position = new Vector3(-3.78f, 2f, 6.16f);
			testObjects[2].transform.position = new Vector3(4.06f, 2f, 6.16f);
			for (int i = 0; i < testObjects.Length; i++)
			{
				testObjects[i].transform.rotation = Quaternion.identity;
				testObjects[i].GetComponent<Rigidbody>().velocity = Vector3.zero;
			}
		}

	}
}
