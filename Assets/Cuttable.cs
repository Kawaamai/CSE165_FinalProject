using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cuttable : MonoBehaviour {

	public GameObject CuttableObjectPrefab;

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
            //Debug.Log(contact.point - transform.position);
            Debug.Log(ReturnDirection(contact.point));
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
                innerNormal = innerRayHit.transform.TransformDirection(innerNormal);
            }
        }
    }

    private HitDirection ReturnDirection(Vector3 contactPoint)
    {
        HitDirection hitDirection = HitDirection.None;
        Vector3 innerNormal;
        RaycastHit innerRayHit;
        GetInnerCollisionNormal(contactPoint, out innerNormal, out innerRayHit);

        if (innerNormal == innerRayHit.transform.up)
            hitDirection = HitDirection.Top;
        if (innerNormal == -innerRayHit.transform.up)
            hitDirection = HitDirection.Bottom;
        if (innerNormal == innerRayHit.transform.forward)
            hitDirection = HitDirection.Forward;
        if (innerNormal == -innerRayHit.transform.forward)
            hitDirection = HitDirection.Back;
        if (innerNormal == innerRayHit.transform.right)
            hitDirection = HitDirection.Right;
        if (innerNormal == -innerRayHit.transform.right)
            hitDirection = HitDirection.Left;

        return hitDirection;
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
