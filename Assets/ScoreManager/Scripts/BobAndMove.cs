using UnityEngine;

public class BobbingAndMoving : MonoBehaviour
{
	[SerializeField] private float bobSpeed = 1.0f;   // Speed of the bobbing motion
	[SerializeField] private float bobHeight = 0.5f;  // Height of the bobbing motion
	[SerializeField] private float moveSpeed = 1.0f;  // Speed of side-to-side movement
	[SerializeField] private float moveRange = 1.0f;  // Range of side-to-side movement
	[SerializeField] private float moveOffset = 0.0f; // Offset for side-to-side movement
	private Vector3 startPosition;

	void Start()
	{
		startPosition = transform.position;
	}

	void Update()
	{
		// Bobbing motion
		float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
		transform.position = new Vector3(transform.position.x, newY, transform.position.z);

		// Side-to-side movement with offset
		float newX = startPosition.x + Mathf.Sin(Time.time * moveSpeed + moveOffset) * moveRange;
		transform.position = new Vector3(newX, transform.position.y, transform.position.z);
	}
}
