using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	//Tutorial shit
	private bool tutorialMode = true;
	public GameObject panel1;
	public GameObject panel2;
	public GameObject panel3;
	public GameObject panel4;
	public GameObject scoreboard;
	public GameObject tutorialBox;

	public bool debugEnabled = false;
	public Transform DebugHead;
	public Transform DebugHeight;
	public LineRenderer rightIndicator;
	public LineRenderer leftIndicator;
	public Transform ai;
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
	public ParticleSystem hoverEffect;

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
	private float moveSpeed = 5f;
	private float hoverHeight = 0.02f;
	private bool isHovering = false;
	private float hoverHandHeightThresh = 1.9f;
	private float hoverHandDeadzone = 0.15f;
	private float hoverHandAngleThresh = 15f;
	private MoveDir lastMoveDir;

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

	// Lightsaber
	public Transform holster;
	public Transform lightsaber;
	// TODO make this a method for lightsaber itself
	public LightsaberBlade lightsaberBladeOuter;
	public LightsaberBlade lightsaberBladeInner;
	private bool holdingLightsaber = false;

    // human joystick
    public bool isHumanJoystick = false;
    private bool initBaseHeadPosSet = false;
    private Vector3 baseHeadPosition;

	// Use this for initialization
	void Start () {
		rightIndicator.enabled = false;
		leftIndicator.enabled = false;
		Inst = this;
		hoverHandHeightThresh = CalcHoverHeightThresh();
		hoverEffect.Stop();
		unequiptLightsaber();
		DebugHead.gameObject.SetActive(debugEnabled);
		DebugHeight.gameObject.SetActive(debugEnabled);
        isHumanJoystick = false;
		if(tutorialMode) {
			panel1.SetActive(true);
		}
	}

	private void castRay(Hand hand)
	{
		int controller = (int)hand;
		Vector3 actualHeadPos = CalcActualHeadPos();
		// Ray ray = new Ray(controllers[controller].position, controllers[controller].position - hmd.position);
		Ray ray = new Ray(controllers[controller].position, controllers[controller].position - actualHeadPos);
		Manipulatable[] objs = (Manipulatable[])FindObjectsOfType<Manipulatable>();
		float min = float.MaxValue;
		int idx = 0;
		for (int i = 0; i < objs.Length; i++)
		{
			float dist = Vector3.Cross(ray.direction, objs[i].transform.position - ray.origin).magnitude;
			// if (dist < min && dist < (Mathf.Sin(15f * Mathf.Deg2Rad) * Vector3.Distance(objs[i].transform.position, hmd.position)))
			// if (dist < min && dist < (Mathf.Sin(15f * Mathf.Deg2Rad) * Vector3.Distance(objs[i].transform.position, actualHeadPos))
			// 	&& Vector3.Distance(objs[i].transform.position, controllers[controller].position) < 30f)
			if (dist < min && dist < (Mathf.Sin(15f * Mathf.Deg2Rad) * Vector3.Distance(objs[i].transform.position, actualHeadPos))
				&& Vector3.Distance(objs[i].transform.position, controllers[controller].position) < 100f)
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
		Vector3 actualHeadPos = CalcActualHeadPos();
		Quaternion headToController = Quaternion.LookRotation(controllers[controller].position - actualHeadPos);
		if(controller == 1)
		{
			currSelectedRObj = currRHandObservedObj;
			// Quaternion headToObject = Quaternion.LookRotation(currSelectedRObj.transform.position - hmd.position);
			Quaternion headToObject = Quaternion.LookRotation(currSelectedRObj.transform.position - actualHeadPos);
			rHandDiffQuat = headToObject * Quaternion.Inverse(headToController);
			//Save the distances from the hand to the object and the hand from the headset.
			rHandObjDist = Vector3.Distance(controllers[controller].position, currSelectedRObj.transform.position);
			// initRHandDistFromHmd = Vector3.Distance(controllers[controller].position, hmd.position);
			initRHandDistFromHmd = Vector3.Distance(controllers[controller].position, actualHeadPos);
			currSelectedRObj.GetComponent<Rigidbody>().velocity = Vector3.zero;
		}
		else
		{
			currSelectedLObj = currLHandObservedObj;
			// Quaternion headToObject = Quaternion.LookRotation(currSelectedLObj.transform.position - hmd.position);
			Quaternion headToObject = Quaternion.LookRotation(currSelectedLObj.transform.position - actualHeadPos);
			lHandDiffQuat = headToObject * Quaternion.Inverse(headToController);
			//Save the distances from the hand to the object and the hand from the headset.
			lHandObjDist = Vector3.Distance(controllers[controller].position, currSelectedLObj.transform.position);
			// initLHandDistFromHmd = Vector3.Distance(controllers[controller].position, hmd.position);
			initLHandDistFromHmd = Vector3.Distance(controllers[controller].position, actualHeadPos);
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
		Vector3 actualHeadPos = CalcActualHeadPos();
		// Vector3 direction = controllers[controller].position - hmd.position;
		Vector3 direction = controllers[controller].position - actualHeadPos;
		//Add the quaternion difference to get the direction toward the object's new position
		if (controller == 1)
			direction = rHandDiffQuat * direction;
		else
			direction = lHandDiffQuat * direction;
		direction.Normalize();
		//Get the distance of the controller from the hmd to calculate how to move the object
		// float currDistFromHmd = Vector3.Distance(controllers[controller].position, hmd.position);
		float currDistFromHmd = Vector3.Distance(controllers[controller].position, actualHeadPos);
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
		// Debug.Log(movementAmt);
		if (movementAmt > 0f)
			movementAmt *= 2.5f;
		else if (movementAmt < 0f && movementAmt > -0.1f)
			movementAmt = 0f;
		else if (movementAmt <= -0.1f)
			movementAmt += 0.1f;
		if (controller == 1)
			rHandObjDist = rHandObjDist + movementAmt * 30f * Time.deltaTime;
		else
			lHandObjDist = lHandObjDist + movementAmt * 30f * Time.deltaTime;
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
			float magnitude = Vector3.Distance(controllers[1].position, rHandLastPos);
			currSelectedRObj.gameObject.GetComponent<Rigidbody>().AddForce(direction * magnitude * 30000f);
			currSelectedRObj = null;
		}
		else
		{
			currSelectedLObj.gameObject.GetComponent<Rigidbody>().useGravity = true;
			Vector3 direction = controllers[0].position - lHandLastPos;
			direction.Normalize();
			float magnitude = Vector3.Distance(controllers[0].position, lHandLastPos);
			currSelectedLObj.gameObject.GetComponent<Rigidbody>().AddForce(direction * magnitude * 30000f);
			currSelectedLObj = null;
		}
	}

	private bool isBothHandsBelowHoverThresh()
	{
		return (controllers[(int)Hand.RIGHT].position.y < hoverHandHeightThresh) && (controllers[(int)Hand.LEFT].position.y < hoverHandHeightThresh);
	}

	public Vector3 CalcActualHeadPos()
	{
		return hmd.position - (0.13f * Vector3.Normalize(hmd.forward));
	}

	private bool isBothHandsInDeadzone()
	{
		Vector3 flatHmdForward = new Vector3(hmd.forward.x, 0f, hmd.forward.z);
		flatHmdForward.Normalize();
		// TODO: hmd-head offset
		Vector3 actualHeadPos = CalcActualHeadPos();
		Vector3 rHandProj = Vector3.Project(controllers[(int)Hand.RIGHT].position - actualHeadPos, flatHmdForward);
		Vector3 lHandProj = Vector3.Project(controllers[(int)Hand.LEFT].position - actualHeadPos, flatHmdForward);

		return (rHandProj.magnitude < hoverHandDeadzone) && (lHandProj.magnitude < hoverHandDeadzone);
	}

	// check ground height (via raycast) directly below speed * forward * timeDelta
	// move char to hoverheight above ground intersection
	private void HoverMove(MoveDir dir)
	{
		if (dir != MoveDir.NONE)
		{
			Vector3 forwardDir = new Vector3(hmd.forward.x, 0f, hmd.forward.z);
			forwardDir.Normalize();
			if (dir == MoveDir.BACKWARD)
				forwardDir *= -1;

			playerCharController.Move(forwardDir * moveSpeed * Time.deltaTime);
		}
	}

	// version of hover move that user can control direction with controllers rather than hmd
	// moves in dir you pointish
	private void HoverMove()
	{
		// TODO figure out how to decide if we move or not
		if (!isBothHandsFaceDown())
		{
            Quaternion avgRot = GetAverageControllerRotation();
            // Vector3 forwardDir = avgRot * Vector3.back;
            Vector3 forwardDir = avgRot * Vector3.forward;
            forwardDir = new Vector3(forwardDir.x, 0f, forwardDir.z);
            forwardDir.Normalize();
			// Debug.Log(forwardDir);
			// Debug.Log(Vector3.Normalize(new Vector3(hmd.forward.x, 0f, hmd.forward.z)));
            playerCharController.Move(forwardDir * moveSpeed * Time.deltaTime);
		}
	}

	private MoveDir GetMoveDir()
	{
		Vector3 flatHmdForward = new Vector3(hmd.forward.x, 0f, hmd.forward.z);
		flatHmdForward.Normalize();
		// TODO: hmd-head offset
		Vector3 actualHeadPos = CalcActualHeadPos(); 
		Vector3 rHandProj = Vector3.Project(controllers[(int)Hand.RIGHT].position - actualHeadPos, flatHmdForward);
		Vector3 lHandProj = Vector3.Project(controllers[(int)Hand.LEFT].position - actualHeadPos, flatHmdForward);

		// if ((rHandProj.magnitude < hoverHandDeadzone) && (lHandProj.magnitude < hoverHandDeadzone))
		// 	return MoveDir.NONE;

		if (isBothHandsInDeadzone())
		{
			lastMoveDir = MoveDir.NONE;
			return MoveDir.NONE;
		}

		if (lastMoveDir == MoveDir.NONE)
		{
            if (flatHmdForward == Vector3.Normalize(rHandProj) && flatHmdForward == Vector3.Normalize(lHandProj)) // if hands forwardish
            {
                if ((rHandProj.magnitude > hoverHandDeadzone) && (lHandProj.magnitude > hoverHandDeadzone))
				{
					lastMoveDir = MoveDir.BACKWARD;
                    return MoveDir.BACKWARD;
				}
            }
            else // hands backwardish
            if (flatHmdForward == -1f * Vector3.Normalize(rHandProj) && flatHmdForward == -1 * Vector3.Normalize(lHandProj)) // if hands forwardish
            {
                if ((rHandProj.magnitude > hoverHandDeadzone) && (lHandProj.magnitude > hoverHandDeadzone))
				{
					lastMoveDir = MoveDir.FORWARD;
                    return MoveDir.FORWARD;
				}
            }
		}

		return lastMoveDir;
	}

    private void HumanJoystickMove()
    {
        if (isHumanJoystickOverThresh())
        {
            Vector3 dir = getHumanJoystickMoveDir();
            playerCharController.Move(dir * moveSpeed * Time.deltaTime);
        }
    }

	// will need later for movement
	private float getGroundHeight(Vector3 pos)
	{
		Ray ray = new Ray(pos, Vector3.down);
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit, 1000f))
		{
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
		Vector3 actualHeadPos = CalcActualHeadPos();
		for (int i = 0; i < objs.Length; i++)
		{
			float dist = Vector3.Cross(ray.direction, objs[i].transform.position - ray.origin).magnitude;
			// if (dist < (Mathf.Sin(45f * Mathf.Deg2Rad) * Vector3.Distance(objs[i].transform.position, hmd.position)))
			if (dist < (Mathf.Sin(45f * Mathf.Deg2Rad) * Vector3.Distance(objs[i].transform.position, actualHeadPos)))
			{
				Vector3 direction = objs[i].transform.position - controllers[controller].position;
				direction.Normalize();
				float magnitude = 10000f + (chargeTime * 10000f);
				float distance = Vector3.Distance(objs[i].transform.position, hmd.position);
				objs[i].GetComponent<Rigidbody>().AddForce(direction * magnitude * (4/(distance*distance)));
			}
		}
	}

	private float CalcHoverHeightThresh()
	{
		return CalcActualHeadPos().y - 0.6f;
	}

	private Quaternion GetAverageControllerRoll()
	{
		// Quaternion rHandRoll = Quaternion.FromToRotation(controllers[(int)Hand.RIGHT].transform.forward, Vector3.forward) * controllers[(int)Hand.RIGHT].rotation;
		// Quaternion lHandRoll = Quaternion.FromToRotation(controllers[(int)Hand.LEFT].transform.forward, Vector3.forward) * controllers[(int)Hand.LEFT].rotation;
		Quaternion rHandRoll = Quaternion.FromToRotation(controllers[(int)Hand.RIGHT].transform.forward, hmd.forward) * controllers[(int)Hand.RIGHT].rotation;
		Quaternion lHandRoll = Quaternion.FromToRotation(controllers[(int)Hand.LEFT].transform.forward, hmd.forward) * controllers[(int)Hand.LEFT].rotation;
		return Quaternion.Lerp(rHandRoll, lHandRoll, 0.5f);
	}

	private void HoverRotate()
	{
		Quaternion roll = GetAverageControllerRoll();
		float controllerRoll = roll.eulerAngles.z;
		controllerRoll = (controllerRoll > 180) ? controllerRoll - 360 : controllerRoll;
		if (controllerRoll > -5f && controllerRoll < 5f)
			return;
		Quaternion rotation = Quaternion.Euler(0f, -controllerRoll, 0f);
		// if (controllerRoll < 5f || controllerRoll > 355f)
		// 	return;
		rotation = Quaternion.Lerp(Quaternion.identity, rotation, 0.005f);
		playerCharController.transform.rotation = playerCharController.transform.rotation * rotation;
	}

	private bool inHolsterRange()
	{
		if (Vector3.Distance(controllers[(int)Hand.RIGHT].position, holster.position) < 0.2f)
			return true;
		return false;
	}

	private void equiptLightsaber()
	{
		lightsaber.parent = controllers[(int)Hand.RIGHT];
		lightsaber.localPosition = new Vector3(0.01f, 0.1f, 0.02f);
		lightsaber.localRotation = Quaternion.Euler(15f, 0f, 0f);
		// TODO combine into one
		lightsaberBladeOuter.ExtendBlade();
		lightsaberBladeInner.ExtendBlade();
		holdingLightsaber = true;
	}

	private void unequiptLightsaber()
	{
		lightsaber.parent = holster;
		lightsaber.localPosition = Vector3.up * 0.5f;
		lightsaber.localRotation = Quaternion.identity;
		// TODO combine into one
		lightsaberBladeOuter.RetractBlade();
		lightsaberBladeInner.RetractBlade();
		holdingLightsaber = false;
	}

	private Quaternion GetAverageControllerRotation()
	{
		return Quaternion.Lerp(controllers[(int)Hand.RIGHT].rotation, controllers[(int)Hand.LEFT].rotation, 0.5f);
	}

	private bool isBothHandsFaceDown()
	{
		Quaternion avgRot = GetAverageControllerRotation();
		float angleDiff = Vector3.Angle(Quaternion.AngleAxis(90f, Vector3.right) * Vector3.forward, avgRot * Vector3.forward); 
		return (angleDiff < hoverHandAngleThresh);
	}

    // HUMAN JOYSTICK SHIT
    public void SetBaseHeadPos()
    {
        baseHeadPosition = hmd.localPosition;
    }

    private Vector3 GetHeadBaseOffset()
    {
        return new Vector3(hmd.localPosition.x - baseHeadPosition.x, 0f, hmd.localPosition.z - baseHeadPosition.z);
    }
    
    private bool isHumanJoystickOverThresh()
    {
        return GetHeadBaseOffset().sqrMagnitude > 0.03f;
    }

    private Vector3 getHumanJoystickMoveDir()
    {
        return playerCharController.transform.rotation * GetHeadBaseOffset().normalized;
    }
    // END HUMAN JOYSTICK SHIT

	// Update is called once per frame
	private void Update () {
		if(tutorialMode && hmd.position.z > -40.76f)
		{
			tutorialMode = false;
			scoreboard.SetActive(true);
		}

        if (!initBaseHeadPosSet && baseHeadPosition == Vector3.zero && hmd.localPosition != Vector3.zero)
        {
            SetBaseHeadPos();
            initBaseHeadPosSet = true;
        }
        
		if (debugEnabled)
		{
            DebugHead.position = CalcActualHeadPos();
            DebugHeight.position = new Vector3(DebugHeight.position.x, CalcHoverHeightThresh(), DebugHeight.position.z);
		}

        hoverHandHeightThresh = CalcHoverHeightThresh();
        if (!isHumanJoystick)
        {
            if (!OVRInput.Get(OVRInput.RawButton.RIndexTrigger) && !OVRInput.Get(OVRInput.RawButton.RHandTrigger) &&
                !OVRInput.Get(OVRInput.RawButton.LIndexTrigger) && !OVRInput.Get(OVRInput.RawButton.LHandTrigger) &&
                isBothHandsBelowHoverThresh())
            {
                // if (isBothHandsInDeadzone())
                if (isBothHandsFaceDown())
                {
                    isHovering = true;
                    if (tutorialMode && panel4.activeSelf)
                    {
                        panel4.GetComponentInChildren<Text>().text = "Nicely done!  The game will begin when you enter the arena.  Destroy the turrets and save your ally!";
                        tutorialMode = false;
                        StartCoroutine(DisablePanel(5f, panel4, scoreboard));
                    }
                    if (!hoverEffect.isPlaying)
                        hoverEffect.Play();
                    // TODO: make this more smooth
                    float groundHeight = getGroundHeight(playerCharController.transform.position);
                    if (playerCharController.isGrounded || groundHeight - playerCharController.height < hoverHeight)
                    {
                        playerCharController.Move(Vector3.up * Time.deltaTime);
                    }
                    else if (groundHeight == 1000f) // walked off edge. fly!
                    {
                        playerCharController.Move(Vector3.up * Time.deltaTime);
                    }
                }
            }
            else
            {
                if (isHovering)
                {
                    if (hoverEffect.isPlaying)
                    	hoverEffect.Stop();
                }
                playerCharController.Move(Vector3.down * Time.deltaTime);
                lastMoveDir = MoveDir.NONE;
                isHovering = false;
            }
        }
        else
        {
            if (isBothHandsBelowHoverThresh() && isBothHandsFaceDown())
            {
                SetBaseHeadPos();
            }

            if (isHumanJoystickOverThresh())
            {
                isHovering = true;
                if (tutorialMode && panel4.activeSelf)
                {
                    panel4.GetComponentInChildren<Text>().text = "Nicely done!  The game will begin when you enter the arena.  Destroy the turrets and save your ally!";
                    tutorialMode = false;
                    StartCoroutine(DisablePanel(5f, panel4, scoreboard));
                }
                if (!hoverEffect.isPlaying)
                	hoverEffect.Play();
                // TODO: make this more smooth
                float groundHeight = getGroundHeight(playerCharController.transform.position);
                if (playerCharController.isGrounded || groundHeight - playerCharController.height < hoverHeight)
                {
                    playerCharController.Move(Vector3.up * Time.deltaTime);
                }
                else if (groundHeight == 1000f) // walked off edge. fly!
                {
                    playerCharController.Move(Vector3.up * Time.deltaTime);
                }
            }
            else
            {
                if (isHovering)
                {
                    if (hoverEffect.isPlaying)
						hoverEffect.Stop();
                }
                playerCharController.Move(Vector3.down * Time.deltaTime);
                lastMoveDir = MoveDir.NONE;
                isHovering = false;
            }
        }

        if (isHumanJoystick || !isHovering)
		{
            if (OVRInput.GetDown(OVRInput.RawButton.RHandTrigger))
            {
				// TODO:

				if (inHolsterRange())
				{
					if (!holdingLightsaber)
						equiptLightsaber();
					else
						unequiptLightsaber();
				}
				else if (currRHandObservedObj != null)
				{
                    initializeGrab(Hand.RIGHT);
					if (tutorialMode && panel1.activeSelf)
					{
						panel1.GetComponentInChildren<Text>().text = "Awesome!  Now you can move it around.  Extend your arm to move the object away and retract your arm to pull it towards you.";
						IEnumerator coroutine = DisablePanel(10f, panel1, panel2);
						StartCoroutine(coroutine);
					}
				}
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
				if (tutorialMode && panel1.activeSelf)
				{
					panel1.GetComponentInChildren<Text>().text = "Awesome!  Now you can move it around.  Extend your arm to move the object away and retract your arm to pull it towards you.";
					IEnumerator coroutine = DisablePanel(10f, panel1, panel2);
					StartCoroutine(coroutine);
				}
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
				rightIndicator.enabled = true;
			}
			else if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger) && currSelectedRObj == null)
			{
				rightIndicator.SetPosition(0, controllers[1].position);
				float indicatorLength = Mathf.Min(10f, (Time.time - startRChargeTime) * 5f);
				rightIndicator.SetPosition(1, controllers[1].position + controllers[1].forward * indicatorLength);
			}
			else if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger) && currSelectedRObj != null)
			{
				rightIndicator.SetPosition(0, currSelectedRObj.transform.position);
				float indicatorLength = Mathf.Min(10f, (Time.time - startRChargeTime) * 5f);
				rightIndicator.SetPosition(1, currSelectedRObj.transform.position + controllers[1].forward * indicatorLength);
				if (indicatorLength == 10f && tutorialMode && panel2.activeSelf)
				{
					panel2.GetComponentInChildren<Text>().text = "Aim and release.";
				}
			}
			else if (OVRInput.GetUp(OVRInput.RawButton.RIndexTrigger) && currSelectedRObj != null)
			{
				rightIndicator.enabled = false;
				releasePushWithObj(Hand.RIGHT);
				if(tutorialMode && panel2.activeSelf)
				{
					StartCoroutine(DisablePanel(2f, panel2, panel3));
				}
			}
			else if (OVRInput.GetUp(OVRInput.RawButton.RIndexTrigger) && currSelectedRObj == null)
			{
				rightIndicator.enabled = false;
				releasePushWithoutObj(Hand.RIGHT);
			}

			if(OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
			{
				chargePush(Hand.LEFT);
				leftIndicator.enabled = true;
			}
			else if (OVRInput.Get(OVRInput.RawButton.LIndexTrigger) && currSelectedLObj == null)
			{
				leftIndicator.SetPosition(0, controllers[0].position);
				float indicatorLength = Mathf.Min(10f, (Time.time - startLChargeTime) * 5f);
				leftIndicator.SetPosition(1, controllers[0].position + controllers[0].forward * indicatorLength);
			}
			else if (OVRInput.Get(OVRInput.RawButton.LIndexTrigger) && currSelectedLObj != null)
			{
				leftIndicator.SetPosition(0, currSelectedLObj.transform.position);
				float indicatorLength = Mathf.Min(10f, (Time.time - startLChargeTime) * 5f);
				leftIndicator.SetPosition(1, currSelectedLObj.transform.position + controllers[0].forward * indicatorLength);
				if (indicatorLength == 10f && tutorialMode && panel2.activeSelf)
				{
					panel2.GetComponentInChildren<Text>().text = "Aim and release.";
				}
			}
			else if (OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger) && CurrSelectedLObj != null)
			{
				leftIndicator.enabled = false;
				releasePushWithObj(Hand.LEFT);
				if (tutorialMode && panel2.activeSelf)
				{
					StartCoroutine(DisablePanel(2f, panel2, panel3));
				}
			}
			else if (OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger) && currSelectedLObj == null)
			{
				leftIndicator.enabled = false;
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

		if (isHovering)// hovering
		{
			// find direction of travel
			// MoveDir dir = GetMoveDir();
			// HoverMove(dir);
            if (!isHumanJoystick)
            {
                HoverMove();
                HoverRotate();
            }
            else
            {
                HumanJoystickMove();
            }
        }
	} // end of Update()

	IEnumerator DisablePanel(float time, GameObject go, GameObject nextPanel)
	{
		yield return new WaitForSeconds(time);
		go.SetActive(false);
		nextPanel.SetActive(true);
		yield break;
	}

	public void EndGame()
	{

	}

	public void Reset()
	{
		GameObject[] turrets = TurretBehavior.deactivatedTurrets;
		for(int i = 0; i < TurretBehavior.deactivatedArrayHead; i++)
		{
			turrets[i].SetActive(true);
		}
		TurretBehavior.deactivatedArrayHead = 0;
        TurretBehavior.remaining = 7;
        ai.GetComponent<AIBehavior>().Reset();
	}

	public void ContinueTutorial()
	{
		if(panel3.activeSelf)
		{
			panel3.GetComponentInChildren<Text>().text = "Great!  You can also use the index trigger without holding an object to push multiple objects.";
			StartCoroutine(DisablePanel(12f, panel3, panel4));
		}
	}
}
