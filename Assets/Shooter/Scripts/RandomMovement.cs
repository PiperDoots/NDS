using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMovement : MonoBehaviour
{
	[SerializeField] private List<Transform> targetTransforms;
	[SerializeField] private float initialMoveSpeed = 5f;
	[SerializeField] private float delayBetweenMoves = 2f;

	private Transform currentTarget;
	private bool isMoving = false;
	private bool isMovingRight = true;
	private float currentSpeed; // Added variable to track current speed

	private void Start()
	{
		// Initialize the first target
		PickRandomTarget();
	}

	private void Update()
	{
		if (!isMoving)
		{
			StartCoroutine(MoveToTarget(currentTarget));
		}
	}

	private void PickRandomTarget()
	{
		if (targetTransforms.Count > 0)
		{
			int randomIndex = Random.Range(0, targetTransforms.Count);
			currentTarget = targetTransforms[randomIndex];

			// Determine the direction based on the current and new target positions
			isMovingRight = transform.position.x < currentTarget.position.x;
			currentSpeed = initialMoveSpeed; // Reset speed to initial value
		}
	}

	private IEnumerator MoveToTarget(Transform target)
	{
		isMoving = true;

		Vector3 targetPosition = target.position;
		Vector3 startPosition = transform.position;
		float journeyLength = Vector3.Distance(startPosition, targetPosition);

		float startTime = Time.time;

		while (transform.position != targetPosition)
		{
			float journeyDuration = Time.time - startTime;
			float fractionOfJourney = journeyDuration / journeyLength;

			// Use a quadratic easing function to adjust speed
			currentSpeed = Mathf.Lerp(initialMoveSpeed, 0, fractionOfJourney * fractionOfJourney);

			// Calculate the direction vector
			Vector3 direction = (isMovingRight) ? Vector3.right : Vector3.left;

			transform.position = Vector3.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);

			// Flip the sprite if moving left
			if (!isMovingRight)
			{
				// Assuming your object has a SpriteRenderer component
				GetComponent<SpriteRenderer>().flipX = false;
			}
			else
			{
				// Reset sprite flip if moving right
				GetComponent<SpriteRenderer>().flipX = true;
			}

			yield return null;
		}

		// Wait for a delay before picking the next target
		yield return new WaitForSeconds(delayBetweenMoves);

		// Pick a new random target
		PickRandomTarget();

		isMoving = false;
	}
}
