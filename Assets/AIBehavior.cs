using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIBehavior : MonoBehaviour {

	private Animator anim;
	private Vector2 currDest;
	private bool hasDest;
	private NavMeshAgent nmagent;
	private float health = 100f;
	public float Health
	{
		get
		{
			return health;
		}
	}
	public GameObject healthBar;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator>();
		hasDest = false;
		nmagent = GetComponent<NavMeshAgent>();
	}

	public void TakeDamage()
	{
		health -= Mathf.Min(health, 10f);
		healthBar.transform.localScale = new Vector3(health / 100f, 1f, 1f);
		//Debug.LogError("AI TOOK DAMAGE");
		if (health <= 0f)
		{
			health = 0f;
			nmagent.speed = 0f;
			anim.SetBool("isDead", true);
			GetComponent<Rigidbody>().velocity = Vector3.zero;
		}
	}

	public void Reset()
	{
        healthBar.transform.localScale = new Vector3(1f, 1f, 1f);
        health = 100f;
		nmagent.speed = 7f;
		anim.SetBool("isDead", false);
	}
	
	// Update is called once per frame
	void Update () {
		if (!hasDest)
		{
			currDest = new Vector2(Random.Range(-41.9f, 39.9f), Random.Range(-7f, 17.2f));
			hasDest = true;
		}
		else
		{
			Vector3 direction = new Vector3(currDest.x, 0f, currDest.y) - transform.position;
			direction.y = 0f;
			direction.Normalize();
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.1f);

			anim.SetBool("isIdle", false);
			anim.SetBool("isRunning", true);
			nmagent.SetDestination(new Vector3(currDest.x, transform.position.y, currDest.y));
			if (Vector3.Distance(transform.position, new Vector3(currDest.x, 0f, currDest.y)) < 2f)
			{
				hasDest = false;
				anim.SetBool("isIdle", true);
				anim.SetBool("isRunning", false);
			}
		}

	}
}
