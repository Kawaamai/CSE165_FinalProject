using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurretBehavior : MonoBehaviour {

	private bool isAiming = true;
	private LineRenderer laser;
	private float startFireTime;
	private float timeToWait;
	private float timeLastFired;
	private float aimSpread = 1f;
	private Vector3 beamEnd;
	public GameObject explosion;
	public static int remaining = 7;
	public Text counter;

	// Use this for initialization
	void Start () {
		laser = GetComponent<LineRenderer>();
		timeToWait = Random.Range(5f, 10f);
		timeLastFired = 0f;
	}

	private void Fire()
	{
		startFireTime = Time.time;
		laser.enabled = true;
		beamEnd = Quaternion.Euler(Random.Range(-1f * aimSpread, aimSpread), Random.Range(-1f * aimSpread, aimSpread), Random.Range(-1f * aimSpread, aimSpread)) *
					(transform.GetChild(1).position + transform.GetChild(1).forward * 100f);
	}

	public void Explode()
	{
		GameObject explode = Instantiate(explosion);
		explode.transform.position = transform.position;
		explode.transform.localScale = new Vector3(2f, 2f, 2f);
		remaining -= 1;
		transform.parent.gameObject.SetActive(false);
	}

	void OnCollisionEnter(Collision col)
	{
		if (col.gameObject.tag == "lightsaber" || (col.gameObject.tag == "crate" && GameManager.Inst.hmd.position.z > -40.76f))
		{
			Explode();
		}
	}

	// Update is called once per frame
	void Update () {
		if(GameManager.Inst.ai.GetComponent<AIBehavior>().Health <= 0f)
		{
			laser.enabled = false;
			return;
		}
		if (!laser.enabled)
		{
			Vector3 direction = GameManager.Inst.ai.position - transform.position;
			direction.Normalize();
			Quaternion rot = Quaternion.LookRotation(direction);
			Vector3 euler = rot.eulerAngles;
			euler.x = 0f;
			euler.z = 0f;
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(euler), 100f * Time.deltaTime);
			if(Time.time - timeLastFired > timeToWait)
			{
				Fire();
			}
		}
		else
		{
			if ((Time.time - startFireTime) > 1.0f)
			{
				laser.enabled = false;
				timeToWait = Random.Range(2f, 10f);
				timeLastFired = Time.time;
			}
			else
			{
				laser.SetPosition(0, transform.GetChild(1).position);
				laser.SetPosition(1, beamEnd);
				RaycastHit hit;
				Ray ray = new Ray(laser.GetPosition(0), (laser.GetPosition(1) - laser.GetPosition(0)));
				if(Physics.Raycast(ray, out hit))
				{
					if(hit.collider.gameObject.tag == "AI" && GameManager.Inst.hmd.position.z > -9.15f)
					{
						GameManager.Inst.ai.GetComponent<AIBehavior>().TakeDamage();
					}
				}
			}
		}
	}
}
