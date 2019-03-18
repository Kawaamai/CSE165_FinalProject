using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cuttable : MonoBehaviour {

	//public GameObject CuttableObjectPrefab;

    private enum HitDirection {  None, Top, Bottom, Forward, Back, Left, Right }
    private enum CubeAxis { None, X, Y, Z }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void OnCollisionEnter(Collision collision)
    {
		Collider c = collision.collider;
		if (c.tag == "lightsaber")
		{
			Debug.Log("lightbutt");
			//Debug.Log(collision.relativeVelocity);
			Debug.Log(collision.contacts.Length); // usually 1, sometimes 2
			ContactPoint contact = collision.contacts[0];
			//ContactPoint contact = collision.contacts[collision.contacts.Length - 1];
            //Debug.Log(contact.point - transform.position);
            //Debug.Log(ReturnDirection(contact.point));
            CubeAxis axis;
            float magnitude;
            GetCutAxis(collision.gameObject, contact.point, out axis, out magnitude);
            Debug.Log("axix " + axis);
            Debug.Log("magnitude " + magnitude);
		}
    }

    private void GetInnerCollisionNormal(Vector3 contactPoint, out Vector3 innerNormal, out RaycastHit innerRayHit)
    {
        // this should be collision contact point
        Vector3 direction = (contactPoint - transform.position).normalized;
        Ray innerRay = new Ray(transform.position, direction);
        innerNormal = Vector3.zero;

        if (Physics.Raycast(innerRay, out innerRayHit))
        {
            if (innerRayHit.collider != null)
            {
                innerNormal = innerRayHit.normal;
                //Debug.Log("innerNormal before transform " + innerNormal);
                //innerNormal = innerRayHit.transform.TransformDirection(innerNormal);
            }
        }
    }

    private bool isVectorClose(Vector3 a, Vector3 b)
    {
        float p = 0.2f;
        Vector3 diff = a - b;
        Debug.Log(diff);
        return Mathf.Abs(diff.x) < p && Mathf.Abs(diff.y) < p && Mathf.Abs(diff.z) < p;
    }

//    private HitDirection ReturnDirection(Vector3 contactPoint)
//    {
//        HitDirection hitDirection = HitDirection.None;
//        Vector3 innerNormal;
//        RaycastHit innerRayHit;
//        GetInnerCollisionNormal(contactPoint, out innerNormal, out innerRayHit);
//        innerNormal = -1f * innerNormal;
//        //Debug.Log("innerNormal after transform: " + innerNormal);
//        //Debug.Log("a;skldfj" + innerRayHit.transform.up);
//
//        //if (innerNormal == innerRayHit.transform.up)
//        //    hitDirection = HitDirection.Top;
//        //if (innerNormal == -1f * innerRayHit.transform.up)
//        //    hitDirection = HitDirection.Bottom;
//        //if (innerNormal == innerRayHit.transform.forward)
//        //    hitDirection = HitDirection.Forward;
//        //if (innerNormal == -1f * innerRayHit.transform.forward)
//        //    hitDirection = HitDirection.Back;
//        //if (innerNormal == innerRayHit.transform.right)
//        //    hitDirection = HitDirection.Right;
//        //if (innerNormal == -1f * innerRayHit.transform.right)
//        //    hitDirection = HitDirection.Left;
//        if (isVectorClose(innerNormal, innerRayHit.transform.up))
//            hitDirection = HitDirection.Top;
//        if (isVectorClose(innerNormal, -1f * innerRayHit.transform.up))
//            hitDirection = HitDirection.Bottom;
//        if (isVectorClose(innerNormal, innerRayHit.transform.forward))
//            hitDirection = HitDirection.Forward;
//        if (isVectorClose(innerNormal, -1f * innerRayHit.transform.forward))
//            hitDirection = HitDirection.Back;
//        if (isVectorClose(innerNormal, innerRayHit.transform.right))
//            hitDirection = HitDirection.Right;
//        if (isVectorClose(innerNormal, -1f * innerRayHit.transform.right))
//            hitDirection = HitDirection.Left;
//
//        return hitDirection;
//    }

    private HitDirection ReturnDirection(Vector3 contactPoint, Vector3 innerNormal, RaycastHit innerRayHit)
    {
        HitDirection hitDirection = HitDirection.None;
        innerNormal = -1f * innerNormal;
        innerNormal.Normalize();
        Debug.Log("innerNormal after transform: " + innerNormal);
        Debug.Log("up" + transform.up);
        Debug.Log("forward" + transform.forward);
        Debug.Log("right" + transform.right);

        //if (innerNormal == innerRayHit.transform.up)
        //    hitDirection = HitDirection.Top;
        //if (innerNormal == -1f * innerRayHit.transform.up)
        //    hitDirection = HitDirection.Bottom;
        //if (innerNormal == innerRayHit.transform.forward)
        //    hitDirection = HitDirection.Forward;
        //if (innerNormal == -1f * innerRayHit.transform.forward)
        //    hitDirection = HitDirection.Back;
        //if (innerNormal == innerRayHit.transform.right)
        //    hitDirection = HitDirection.Right;
        //if (innerNormal == -1f * innerRayHit.transform.right)
        //    hitDirection = HitDirection.Left;
        //if (isVectorClose(innerNormal, innerRayHit.transform.up))
        //    hitDirection = HitDirection.Top;
        //if (isVectorClose(innerNormal, -1f * innerRayHit.transform.up))
        //    hitDirection = HitDirection.Bottom;
        //if (isVectorClose(innerNormal, innerRayHit.transform.forward))
        //    hitDirection = HitDirection.Forward;
        //if (isVectorClose(innerNormal, -1f * innerRayHit.transform.forward))
        //    hitDirection = HitDirection.Back;
        //if (isVectorClose(innerNormal, innerRayHit.transform.right))
        //    hitDirection = HitDirection.Right;
        //if (isVectorClose(innerNormal, -1f * innerRayHit.transform.right))
        //    hitDirection = HitDirection.Left;
        if (isVectorClose(innerNormal, transform.up))
            hitDirection = HitDirection.Top;
        if (isVectorClose(innerNormal, -1f * transform.up))
            hitDirection = HitDirection.Bottom;
        if (isVectorClose(innerNormal, transform.forward))
            hitDirection = HitDirection.Forward;
        if (isVectorClose(innerNormal, -1f * transform.forward))
            hitDirection = HitDirection.Back;
        if (isVectorClose(innerNormal, transform.right))
            hitDirection = HitDirection.Right;
        if (isVectorClose(innerNormal, -1f * transform.right))
            hitDirection = HitDirection.Left;

        return hitDirection;
    }

    private void GetCutAxis(GameObject collidingObject, Vector3 contactPoint, out CubeAxis axis, out float magnitude)
    {
        Vector3 outerPlaneNormal;
        RaycastHit raycastHit;
        GetInnerCollisionNormal(contactPoint, out outerPlaneNormal, out raycastHit);
        HitDirection hitDir = ReturnDirection(contactPoint, outerPlaneNormal, raycastHit);
        axis = CubeAxis.None;
        magnitude = 0f;
        Debug.Log(hitDir);
        if (hitDir == HitDirection.None)
            return;

        outerPlaneNormal = -outerPlaneNormal;

        Quaternion rot = collidingObject.transform.rotation;
        Vector3 dir = rot * Vector3.up; // TODO: figure out if this is up or forward
        Vector3 proj = Vector3.ProjectOnPlane(dir, outerPlaneNormal);

        Quaternion invRot = Quaternion.Inverse(transform.rotation);
        proj = invRot * proj;
        proj.Normalize();

        if (hitDir == HitDirection.Top || hitDir == HitDirection.Bottom)
        {
            // y plane; ignore y component
            if (proj.x < proj.z)
            {
                axis = CubeAxis.X;
                magnitude = proj.x;
            }
            axis = CubeAxis.Z;
            magnitude = proj.z;
        }
        else if (hitDir == HitDirection.Forward || hitDir == HitDirection.Back)
        {
            // z plane; ignore z component
            if (proj.x < proj.y)
            {
                axis = CubeAxis.X;
                magnitude = proj.x;
            }
            axis = CubeAxis.Y;
            magnitude = proj.y;
        }
        else // hitDir == HitDirection.Left || HitDirection.Right
        {
            // x plane; ignore x component
            if (proj.z < proj.y)
            {
                axis = CubeAxis.Z;
                magnitude = proj.x;
            }
            axis = CubeAxis.Y;
            magnitude = proj.y;
        }
    }

    private void GetCutAxis(GameObject collidingObject, Vector3 contactPoint, HitDirection hitDir, out CubeAxis axis, out float magnitude)
    {
        axis = CubeAxis.None;
        magnitude = 0f;
        if (hitDir == HitDirection.None)
            return;

        Vector3 outerPlaneNormal;
        RaycastHit raycastHit;
        GetInnerCollisionNormal(contactPoint, out outerPlaneNormal, out raycastHit);
        outerPlaneNormal = -outerPlaneNormal;

        Quaternion rot = collidingObject.transform.rotation;
        Vector3 dir = rot * Vector3.up; // TODO: figure out if this is up or forward
        Vector3 proj = Vector3.ProjectOnPlane(dir, outerPlaneNormal);

        Quaternion invRot = Quaternion.Inverse(transform.rotation);
        proj = invRot * proj;
        proj.Normalize();

        if (hitDir == HitDirection.Top || hitDir == HitDirection.Bottom)
        {
            // y plane; ignore y component
            if (proj.x < proj.z)
            {
                axis = CubeAxis.X;
                magnitude = proj.x;
            }
            axis = CubeAxis.Z;
            magnitude = proj.z;
        }
        else if (hitDir == HitDirection.Forward || hitDir == HitDirection.Back)
        {
            // z plane; ignore z component
            if (proj.x < proj.y)
            {
                axis = CubeAxis.X;
                magnitude = proj.x;
            }
            axis = CubeAxis.Y;
            magnitude = proj.y;
        }
        else // hitDir == HitDirection.Left || HitDirection.Right
        {
            // x plane; ignore x component
            if (proj.z < proj.y)
            {
                axis = CubeAxis.Z;
                magnitude = proj.x;
            }
            axis = CubeAxis.Y;
            magnitude = proj.y;
        }
    }
}
