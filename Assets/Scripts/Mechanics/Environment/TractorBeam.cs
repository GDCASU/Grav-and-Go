using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 * 
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/// <summary>
/// 
/// </summary>
public class TractorBeam : MonoBehaviour
{
    // Use this bool to gate all your Debug.Log Statements please
    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog;

	[Header("Settings")]
	public Direction PullDirection;
	public float PullSpeed;
	public float ObjectDeceleration;

	private void Start()
	{
		Material mat = GetComponent<SpriteRenderer>().material;
		string scrollDirectionVectorPropName = mat.shader.GetPropertyName(1);
		mat.SetVector(scrollDirectionVectorPropName, GetPullVector());
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if(collision.gameObject.tag == "Player")
		{
			collision.GetComponent<PlayerMovementController>()._tractorBeam = null;
		}
		if(collision.gameObject.tag == "Physics Object")
		{
			collision.GetComponent<Rigidbody2D>().gravityScale = 1f;
		}
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if(collision.gameObject.tag == "Player")
		{
			collision.GetComponent<PlayerMovementController>()._tractorBeam = this;
		}
		if(collision.gameObject.tag == "Physics Object")
		{
			Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
			Vector2 targetVelocity = PullSpeed * GetPullVector();
			Vector2 difference = targetVelocity - rb.linearVelocity;
			if(difference.magnitude > ObjectDeceleration * Time.fixedDeltaTime)
			{
				rb.linearVelocity += ObjectDeceleration * Time.fixedDeltaTime * difference.normalized;
			}
			else
			{
				rb.linearVelocity = targetVelocity;
			}
			rb.gravityScale = 0f;
		}
	}

	public Vector2 GetPullVector() => GetPullVector(PullDirection);

	public Vector2 GetPullVector(Direction pullDirection)
	{
		switch(pullDirection)
		{
			case Direction.Up:
				return Vector2.up;
			case Direction.Down:
				return Vector2.down;
			case Direction.Left:
				return Vector2.left;
			case Direction.Right:
				return Vector2.right;
		}
		return Vector2.zero;
	}
}

public enum Direction
{
	Up,
	Down,
	Left,
	Right
}