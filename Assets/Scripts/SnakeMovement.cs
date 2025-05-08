using System.Collections.Generic;
using UnityEngine;

public class SnakeMovement : MonoBehaviour
{
    public float speed = 5f;
    public float growthSpacing = 200f;

    public Transform segmentPrefab; // Assign in the inspector
    public FoodSpawner foodSpawner; // Assign in the inspector

    private Vector2 direction = Vector2.right;
    private List<Transform> segments = new List<Transform>();
    public int initialSize = 4;
    private List<Vector2> positions = new List<Vector2>(); // Stores the positions for segments to follow

    private void Start()
    {
        segments.Add(this.transform);
        for(int n=0; n<10; n++) {
            foodSpawner.SpawnFood();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) direction = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) direction = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) direction = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow)) direction = Vector2.right;
    }

    void FixedUpdate()
    {
        // Calculate how much to move this frame
        Vector2 movement = direction * speed;

        // Record old head position before overwriting (for advancing first body segment).
        Vector2 oldHeadPosition = new Vector2(this.transform.position.x, this.transform.position.y);

        // Move the head
        this.transform.position = new Vector2(
            Mathf.Round(this.transform.position.x + movement.x), 
            Mathf.Round(this.transform.position.y + movement.y));

        // Update the position of each segment to follow the path
        if (segments.Count > 1) {
            for (int i = segments.Count-1; i > 0; i--)
            {
                segments[i].position = FollowTheLeader(segments[i-1].position, segments[i].position);
            }
        }
    }

    private Vector2 FollowTheLeader(Vector2 leader, Vector2 follower) {
        Vector2 direction = (leader - follower).normalized;

        // Ensure we don't overshoot and maintain a minimum distance
        float step = Mathf.Min(speed, (leader - follower).magnitude - growthSpacing);

        return follower + direction * step;
    }


    public void Grow()
    {
        // Offset the new body segment to minimize overlap with other segments
        Vector2 growthPosition = new Vector2(
            (segments[segments.Count - 1].position.x + growthSpacing),
            (segments[segments.Count - 1].position.y + growthSpacing)
        );

        // Instantiate a new segment
        Transform segment = Instantiate(segmentPrefab, growthPosition, Quaternion.identity);
        segments.Add(segment);

        // Make the new segment a child of a specific GameObject to keep the Hierarchy clean
        segment.SetParent(transform.parent);

        GameManager.instance.IncreaseScore(1);
    }

    // Only for debugging use
    private void PrintSegments() {
        for (int i = 0; i < segments.Count; i++) {
            Debug.Log("Segment " + i + ": " + JsonUtility.ToJson(segments[i].position));
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Food"))
        {
            Grow();
            Destroy(other.gameObject);
            // Notify the FoodSpawner to spawn new food
            foodSpawner.SpawnFood();
        }
        else if (other.gameObject.CompareTag("Wall"))
        {
            FindObjectOfType<GameManager>().GameOver();
        }
    }
}
