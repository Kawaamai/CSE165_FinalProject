using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public static GameManager Inst;
	public enum FlyDir {
		None = 0,
		Forward = 1,
		Backward = 2,
	};

	public CharacterController playerController;
	public SphereCollider hoverCollider;
	public Transform hmd;
	public Transform[] controllers;

	// player information
	private float flySpeed = 2.2f;
	private float hoverHeight = 0.5f;
	private bool playerIsFlying = false;
	// height threshold for activating flying (hand below this height)
	// TODO: make this value configurable by the user
	private float handFlyHeightThresh = 1.2f; // approx 4ft
	// threshold for controller facing down for flying
	private float handFlyAngleThresh = 20f;
	private float handFlyDeadzone = 5f; // deadzone for not moving when flying

	// object manipulation
	private Manipulatable currSelectedObj;
	public Manipulatable CurrSelectedObj
	{
		get
		{
			return this.currSelectedObj;
		}
	}

	// hand information
	private float initRHandObjDist;
	private Quaternion rHandDiffQuat;
	private Vector3 rHandLastPos;

	// Use this for initialization
	void Start () {
		Inst = this;
		hoverCollider.enabled = false;
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

        // not touching any of the triggers
        // TODO: determine if we also want not touching any of the face buttons
        if (isHandOpen(0) && isHandOpen(1) && bothHandsFacingDown() && bothHandsBelowThresh())
        {
			playerIsFlying = true;
			hoverCollider.enabled = true;
			playerController.Move(Vector3.Up);
        }
        else
        {
			playerIsFlying = false;
			hoverCollider.enabled = false;
			playerController.Move(Vector3.Down);
        }

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

	private bool isHandOpen(int hand) {
		bool ret = false;

		// this make me sad
        if (hand == 1)
        {
            if (!OVRInput.Get(OVRInput.RawTouch.RIndexTrigger) &&
                !OVRInput.Get(OVRInput.RawButton.RHandTrigger) &&
                !OVRInput.Get(OVRInput.RawTouch.RThumbRest) &&
                !OVRInput.Get(OVRInput.RawTouch.A) &&
                !OVRInput.Get(OVRInput.RawTouch.B) &&
                !OVRInput.Get(OVRInput.RawTouch.RThumbstick))
            {
                ret = true;
            }
        }
        else
        {
            if (!OVRInput.Get(OVRInput.RawTouch.LIndexTrigger) &&
                !OVRInput.Get(OVRInput.RawButton.LHandTrigger) &&
                !OVRInput.Get(OVRInput.RawTouch.LThumbRest) &&
                !OVRInput.Get(OVRInput.RawTouch.X) &&
                !OVRInput.Get(OVRInput.RawTouch.Y) &&
                !OVRInput.Get(OVRInput.RawTouch.LThumbstick))
            {
                ret = true;
            }
        }

		return ret;
	}

	private FlyDir getFlyDirection() {
		if (bothHandXRotThresh(270f, handFlyDeadzone))
			return FlyDir.None;
		else if (controllers[0].rotation.eulerAngles.x > 270f && controllers[1].rotation.eulerAngles.x > 270f)
			return FlyDir.Backward;
		else
			return FlyDir.Forward;
	}

	private bool bothHandsFacingDown() {
		return bothHandXRotThresh(270f, handFlyAngleThresh);
	}

	private bool bothHandXRotThresh(float dir, float thresh) {
		// TODO: account for modular arithmetic that degrees use
		foreach (Transform controller in controllers) {
			if (controller.rotation.eulerAngles.x < (dir - thresh) || controller.rotation.eulerAngles.x > (dir + thresh))
				return false;
		}
		return true;
	}

	private bool bothHandsBelowThresh() {
		return (controllers[0].position.y < handFlyHeightThresh && controllers[1].position.y < handFlyHeightThresh);
	}
}
