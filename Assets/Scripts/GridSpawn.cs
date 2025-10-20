using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    public GameObject gridPointPrefab;
    public float spacing = 0.1f; // 간격 설정

    void Start()
    {
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                Vector3 spawnPos = new Vector3(x * spacing, 0, y * spacing);
                GameObject point = Instantiate(gridPointPrefab, spawnPos, Quaternion.identity);
                point.GetComponent<GridPoint>().x = x;
                point.GetComponent<GridPoint>().y = y;
                GameManager.Instance.grid[x, y] = point;
            }
        }
    }
}
