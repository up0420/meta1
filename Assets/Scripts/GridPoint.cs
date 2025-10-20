using UnityEngine;

public class GridPoint : MonoBehaviour
{
    public int x;
    public int y;
    public bool isOccupied = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isOccupied) return;

        if (other.CompareTag("Black") || other.CompareTag("White"))
        {
            int player = (other.CompareTag("Black")) ? 1 : 2;
            GameManager.Instance.TryPlaceStone(x, y, transform.position, player);
            Debug.Log("Trigger");
        }
    }

    public void MarkOccupied()
    {
        isOccupied = true;
    }
}
