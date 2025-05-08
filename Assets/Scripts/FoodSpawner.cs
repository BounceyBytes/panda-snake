using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab; // Assign in the inspector
    public Transform borderTop; // Assign with a GameObject representing the top boundary
    public Transform borderBottom; // Assign with a GameObject representing the bottom boundary
    public Transform borderLeft; // Assign with a GameObject representing the left boundary
    public Transform borderRight; // Assign with a GameObject representing the right boundary

    private void Start()
    {
        SpawnFood();
    }

    public void SpawnFood()
    {
        // Calculate random position within the boundaries
        int x = (int)Random.Range(borderLeft.position.x, borderRight.position.x);
        int y = (int)Random.Range(borderBottom.position.y, borderTop.position.y);

        // Instantiate the food at (x, y)
        Instantiate(foodPrefab, new Vector2(x, y), Quaternion.identity);
    }
}
