using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthMove : MonoBehaviour
{
	[SerializeField] private float initialMoveSpeed = 7f;
	[SerializeField] private float delayBetweenMoves = 1f;
	[SerializeField] private float radius = 3f;
	private Vector3 initialPosition;

	private Vector3 randomTarget;
	private bool isMoving = false;
	private float currentSpeed;

	private void Start()
	{
		initialPosition = transform.position;
		PickRandomTarget();
	}

	private void Update()
	{
		if (!isMoving)
		{
			StartCoroutine(MoveToTarget(randomTarget));
		}
	}

	private void PickRandomTarget()
	{
		randomTarget = initialPosition + Random.insideUnitSphere * radius;
		randomTarget.z = 0;
		currentSpeed = initialMoveSpeed;
	}

	private IEnumerator MoveToTarget(Vector3 target)
	{
		isMoving = true;

		Vector3 startPosition = transform.position;
		float journeyLength = Vector3.Distance(startPosition, target);

		float startTime = Time.time;

		while (transform.position != target)
		{
			float journeyDuration = Time.time - startTime;
			float fractionOfJourney = journeyDuration / journeyLength;

			// Use a quadratic easing function to adjust speed
			currentSpeed = Mathf.Lerp(initialMoveSpeed, 0, fractionOfJourney * fractionOfJourney);

			// Calculate the direction vector
		
			transform.position = Vector3.MoveTowards(transform.position, target, currentSpeed * Time.deltaTime);

			yield return null;
		}

		// Wait for a delay before picking the next random target
		yield return new WaitForSeconds(delayBetweenMoves);

		// Pick a new random target
		PickRandomTarget();

		isMoving = false;
	}
}
