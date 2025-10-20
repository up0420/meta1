using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static int currentPlayer = 1; // 1: Black, 2: White
    public GameObject[,] grid = new GameObject[5, 5];
    public int[,] boardState = new int[5, 5]; // 0: 없음, 1: 흑, 2: 백
    public GameObject blackStonePrefab;
    public GameObject whiteStonePrefab;

    private void Awake()
    {
        Instance = this;
    }

    public void TryPlaceStone(int x, int y, Vector3 position, int tagPlayer)
    {
        if (tagPlayer != currentPlayer) return;
        if (boardState[x, y] != 0) return;

        GameObject prefab = (currentPlayer == 1) ? blackStonePrefab : whiteStonePrefab;
        Instantiate(prefab, position + Vector3.up * 0.02f, Quaternion.identity);
        boardState[x, y] = currentPlayer;

        grid[x, y].GetComponent<GridPoint>().MarkOccupied();

        // 착수 정보 OSC로 전송 (상대방에게 보내기)
        OSCManager.Instance.SendPlace(x, y, currentPlayer);

        if (CheckWin(x, y, currentPlayer))
        {
            Debug.Log((currentPlayer == 1 ? "Black" : "White") + " wins!");
            return; // 승리 후 턴 넘기지 않음
        }

        currentPlayer = (currentPlayer == 1) ? 2 : 1;
    }

    private bool CheckWin(int x, int y, int player)
    {
        // 4방향 검사
        Vector2Int[] dirs = {
            Vector2Int.right,
            Vector2Int.up,
            new Vector2Int(1, 1),
            new Vector2Int(1, -1)
        };

        foreach (var dir in dirs)
        {
            int count = 1;
            count += CountDir(x, y, dir, player);
            count += CountDir(x, y, -dir, player);
            if (count >= 5) return true;
        }
        return false;
    }

    private int CountDir(int x, int y, Vector2Int dir, int player)
    {
        int count = 0;
        int nx = x + dir.x;
        int ny = y + dir.y;
        while (nx >= 0 && nx < 5 && ny >= 0 && ny < 5 && boardState[nx, ny] == player)
        {
            count++;
            nx += dir.x;
            ny += dir.y;
        }
        return count;
    }

    public void PlaceStoneFromRemote(int x, int y, int player)
    {
        if (boardState[x, y] != 0) return;

        Vector3 position = grid[x, y].transform.position;
        PlaceStone(x, y, position, player);

        // 수신한 착수 정보로 currentPlayer를 반영
        currentPlayer = (player == 1) ? 2 : 1;
    }

    private void PlaceStone(int x, int y, Vector3 position, int player)
    {
        GameObject prefab = (player == 1) ? blackStonePrefab : whiteStonePrefab;
        Instantiate(prefab, position + Vector3.up * 0.02f, Quaternion.identity);

        boardState[x, y] = player;
        grid[x, y].GetComponent<GridPoint>().MarkOccupied();
    }
}
