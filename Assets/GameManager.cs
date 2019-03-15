using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public enum Hand
	{
		LEFT = 0,
		RIGHT = 1
	}

	public enum MoveDir
	{
		NONE = 0,
		FORWARD = 1,
		BACKWARD = 2,
	}

	public static GameManager Inst;

	public Transform hmd;
	public CharacterController playerCharController;
	public OVRPlayerController playerController; // may not need this

	// movement
	// Activate movement by:
	// 1. move hands below hoverHandHeightThresh
	// 2. keep hands between (forward/backward) head +- hoverHandDeadzone
	// 3. open hands (might include removing thumb from controller)
	// move forward: move hands slightly behind body; move backwards: move hands slightly infront of body
	// Note: body direction determined by head direction
	// both hands must be in same area to move, e.g. one hand front and one hand back will not move
	// Deactivate movement by doing one of the following:
	// 1. Close hands
	// 2. move hands above hoverHandHeightThresh
	private float moveSpeed = 2.2f;
	private float hoverHeight = 0.5f;
	private bool isHovering = false;
	private float hoverHandHeightThresh = 2f;
	private float hoverHandDeadzone = 0.15f;
	private float hoverHandAngleThresh = 5f;

	// controller
	public Transform[] controllers;

	// Manipulation
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
	private Manipulatable currRHandObservedObj;
	public Manipulatable CurrRHandObservedObj
	{
		get
		{
			return this.currRHandObservedObj;
		}
	}
	private Manipulatable currLHandObservedObj;
	public Manipulatable CurrLHandObservedObj
	{
		get
		{
			return this.currLHandObservedObj;
		}
	}
	private float rHandObjDist;
	private Quaternion rHandDiffQuat;
	private Vector3 rHandLastPos;
	private float initRHandDistFromHmd;
	private float startRChargeTime = 0f;

	private float lHandObjDist;
	private Quaternion lHandDiffQuat;
	private Vector3 lHandLastPos;
	private float initLHandDistFromHmd;
	private float startLChargeTime = 0f;

	// Use this for initialization
	void Start () {
		Inst = this;
		hoverHandHeightThresh = hmd.position.y - 0.6f;
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
			currRHandObservedObj = objs[idx];
		}
		else if (min != float.MaxValue && controller == 0)
		{
			currLHandObservedObj = objs[idx];
		}
		else if (min == float.MaxValue && controller == 1)
		{
			currRHandObservedObj = null;
		}
		else
		{
			currLHandObservedObj = null;
		}

	}

	private void initializeGrab(Hand hand)
	{
		int controller = (int)hand;
		// Calculate Quaternion distance between the head to the controller and the head to the object
		Quaternion headToController = Quaternion.LookRotation(controllers[controller].position - hmd.position);
		if(controller == 1)
		{
			currSelectedRObj = currRHandObservedObj;
			Quaternion headToObject = Quaternion.LookRotation(currSelectedRObj.transform.position - hmd.position);
			rHandDiffQuat = headToObject * Quaternion.Inverse(headToController);
			//Save the distances from the hand to the object and the hand from the headset.
			rHandObjDist = Vector3.Distance(controllers[controller].position, currSelectedRObj.transform.position);
			initRHandDistFromHmd = Vector3.Distance(controllers[controller].position, hmd.position);
			currSelectedRObj.GetComponent<Rigidbody>().velocity = Vector3.zero;
		}
		else
		{
			currSelectedLObj = currLHandObservedObj;
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
		Debug.Log(movementAmt);
		if (movementAmt > 0f)
			movementAmt *= 2.5f;
		else if (movementAmt < 0f && movementAmt > -0.1f)
			movementAmt = 0f;
		else if (movementAmt <= -0.1f)
			movementAmt += 0.1f;
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
		//TODO - change currSelectedL/RObj to an array so that we can index with (int)hand
		if (controller == 1)
		{
			//TODO - Calculate magnitude of change along controller's forward axis.  Add to magnitude.
			currSelectedRObj.gameObject.GetComponent<Rigidbody>().useGravity = true;
			Vector3 direction = controllers[1].position - rHandLastPos;
			direction.Normalize();
			//float magnitude = Vector3.Distance(currSelectedRObj.transform.position, hmd.position);
			float magnitude = Vector3.Distance(controllers[1].position, rHandLastPos);
			currSelectedRObj.gameObject.GetComponent<Rigidbody>().AddForce(direction * magnitude * 30000f);
			currSelectedRObj = null;
		}
		else
		{
			currSelectedLObj.gameObject.GetComponent<Rigidbody>().useGravity = true;
			Vector3 direction = controllers[0].position - lHandLastPos;
			direction.Normalize();
			//float magnitude = Vector3.Distance(currSelectedLObj.transform.position, hmd.position);
			float magnitude = Vector3.Distance(controllers[0].position, lHandLastPos);
			currSelectedLObj.gameObject.GetComponent<Rigidbody>().AddForce(direction * magnitude * 30000f);
			currSelectedLObj = null;
		}
	}

	private bool isBothHandsBelowHoverThresh()
	{
		return (controllers[(int)Hand.RIGHT].position.y < hoverHandHeightThresh) && (controllers[(int)Hand.LEFT].position.y < hoverHandHeightThresh);
	}

	private bool isBothHandsInDeadzone()
	{
		Vector3 flatHmdForward = new Vector3(hmd.forward.x, 0f, hmd.forward.z);
		flatHmdForward.Normalize();
		Vector3 acutalHeadPos = hmd.position - (0.1f * flatHmdForward); // hmd position slightly infront head position
		Vector3 rHandProj = Vector3.Project(controllers[(int)Hand.RIGHT].position - hmd.position, flatHmdForward);
		Vector3 lHandProj = Vector3.Project(controllers[(int)Hand.LEFT].position - hmd.position, flatHmdForward);

		return (rHandProj.magnitude < hoverHandDeadzone) && (lHandProj.magnitude < hoverHandDeadzone);
	}

	// check ground height (via raycast) directly below speed * forward * timeDelta
	// move char to hoverheight above ground intersection
	private void HoverMove(MoveDir dir)
	{
		if (dir == MoveDir.NONE)
		{

		}
		else
		{
			Vector3 forwardDir = new Vector3(hmd.forward.x, 0f, hmd.forward.z);
			if (dir == MoveDir.BACKWARD)
				forwardDir *= -1;

			// TODO:
		}
	}

	// will need later for movement
	private float getGroundHeight(Vector3 pos)
	{
		Ray ray = new Ray(pos, Vector3.down);
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit, 1000f))
		{
			if (hit.collider.tag == "Ground")
				return hit.distance;
		}

		return 1000f;
	}

	private void chargePush(Hand hand)
	{
		int controller = (int)hand;
		if (controller == 1)
		{
			startRChargeTime = Time.time;
		}
		if (controller == 0)
		{
			startLChargeTime = Time.time;
		}
	}

	private void releasePushWithObj(Hand hand)
	{
		int controller = (int)hand;
		if (controller == 1)
		{
			if(currSelectedRObj == null)
			{
				return;
			}
			//Direction should be away from the player
			//Vector3 direction = currSelectedRObj.transform.position - hmd.position;
			Vector3 direction = controllers[controller].forward;
			direction.Normalize();
			//Magnitude should be based on how long the button was held
			float chargeTime = Time.time - startRChargeTime;
			chargeTime = Mathf.Min(chargeTime, 2f);
			currSelectedRObj.GetComponent<Rigidbody>().AddForce(direction * (1000f + (chargeTime * 1000f)));
			currSelectedRObj.gameObject.GetComponent<Rigidbody>().useGravity = true;
			currSelectedRObj = null;
		}
		else
		{
			if (currSelectedLObj == null)
			{
				return;
			}
			//Direction should be away from the player
			//Vector3 direction = currSelectedLObj.transform.position - hmd.position;
			Vector3 direction = controllers[controller].forward;
			direction.Normalize();
			//Magnitude should be based on how long the button was held
			float chargeTime = Time.time - startLChargeTime;
			chargeTime = Mathf.Min(chargeTime, 2f);
			currSelectedLObj.GetComponent<Rigidbody>().AddForce(direction * (1000f + (chargeTime * 1000f)));
			currSelectedLObj.gameObject.GetComponent<Rigidbody>().useGravity = true;
			currSelectedLObj = null;
		}
	}

	private void releasePushWithoutObj(Hand hand)
	{
		int controller = (int)hand;
		float chargeTime = 0f;
		if (controller == 1)
		{
			chargeTime = Time.time - startRChargeTime;
		}
		else
		{
			chargeTime = Time.time - startLChargeTime;
		}
		chargeTime = Mathf.Min(chargeTime, 2f);
		Ray ray = new Ray(controllers[controller].position, controllers[controller].forward);
		Manipulatable[] objs = (Manipulatable[])FindObjectsOfType<Manipulatable>();
		for (int i = 0; i < objs.Length; i++)
		{
			float dist = Vector3.Cross(ray.direction, objs[i].transform.position - ray.origin).magnitude;
			if (dist < (Mathf.Sin(45f * Mathf.Deg2Rad) * Vector3.Distance(objs[i].transform.position, hmd.position)))
			{
				Vector3 direction = objs[i].transform.position - controllers[controller].position;
				direction.Normalize();
				objs[i].GetComponent<Rigidbody>().AddForce(direction * (1000f + (chargeTime * 1000f)));
			}
		}
	}
	
	// Update is called once per frame
	private void Update () {

		hoverHandHeightThresh = hmd.position.y - 0.6f;
		// Debug.Log(hoverHandHeightThresh);
		// Debug.Log(controllers[1].position.y);
		if (!OVRInput.Get(OVRInput.RawButton.RIndexTrigger) && !OVRInput.Get(OVRInput.RawButton.RHandTrigger) &&
			!OVRInput.Get(OVRInput.RawButton.LIndexTrigger) && !OVRInput.Get(OVRInput.RawButton.LHandTrigger) &&
			isBothHandsBelowHoverThresh()) // TODO: remove this false to get hovering
		{
			if (isBothHandsInDeadzone())
			{
				isHovering = true;
				// move charater up 0.5
				playerController.GravityModifier = 0;
                // float groundDist = getGroundHeight(playerCharController.transform.position);
                // Debug.Log(groundDist);
                // if (groundDist - playerCharController.height < hoverHeight)
                //     playerCharController.Move(Vector3.up * 0.5f);
				// TODO: fix the bounce/falling after moving up
				if (playerCharController.isGrounded)
                    playerCharController.Move(Vector3.up * 0.5f);
			}
		}
		else
		{
            if (isHovering)
			{
				playerController.GravityModifier = 1;
                if (!playerCharController.isGrounded)
                    playerCharController.Move(Vector3.down * 0.5f);
			}
			isHovering = false;
		}

		if (!isHovering)
		{
            if (OVRInput.GetDown(OVRInput.RawButton.RHandTrigger) && currRHandObservedObj != null)
            {
                initializeGrab(Hand.RIGHT);
            }
            else if (OVRInput.Get(OVRInput.RawButton.RHandTrigger) && currSelectedRObj != null)
            {
                maintainGrab(Hand.RIGHT);
            }
            else if (OVRInput.GetUp(OVRInput.RawButton.RHandTrigger) && currSelectedRObj != null)
            {
                endGrab(Hand.RIGHT);
                castRay(Hand.RIGHT);
            }
            else
            {
                castRay(Hand.RIGHT);
            }

            if (OVRInput.GetDown(OVRInput.RawButton.LHandTrigger) && currLHandObservedObj != null)
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

			if(OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
			{
				chargePush(Hand.RIGHT);
			}
			else if (OVRInput.GetUp(OVRInput.RawButton.RIndexTrigger) && currSelectedRObj != null)
			{
				releasePushWithObj(Hand.RIGHT);
			}
			else if (OVRInput.GetUp(OVRInput.RawButton.RIndexTrigger) && currSelectedRObj == null)
			{
				releasePushWithoutObj(Hand.RIGHT);
			}

			if(OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
			{
				chargePush(Hand.LEFT);
			}
			else if (OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger) && CurrSelectedLObj != null)
			{
				releasePushWithObj(Hand.LEFT);
			}
			else if (OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger) && currSelectedLObj == null)
			{
				releasePushWithoutObj(Hand.LEFT);
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
		else // hovering
		{

		}
	} // end of Update()
}
