using System.Collections.Generic;
using UnityEngine;
using OscJack;

public class OSCManager : MonoBehaviour
{
    public static OSCManager Instance;
    private Queue<System.Action> mainThreadActions = new Queue<System.Action>();
    private readonly object queueLock = new object();

    [Header("OSC Settings")]
    public string remoteIp = "192.168.0.15"; // 상대방 IP
    public int sendPort = 7000;
    public int receivePort = 7000;
    public GameObject[] selectorObjects = new GameObject[2]; // 0: Black, 1: White

    private OscClient client;
    private OscServer server;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        client = new OscClient(remoteIp, sendPort);
        server = new OscServer(receivePort);

        server.MessageDispatcher.AddCallback("/omok/place", OnReceivePlace);
        server.MessageDispatcher.AddCallback("/selector/deactivate", OnReceiveDeactivate);
    }

    void Update()
    {
        lock (queueLock)
        {
            while (mainThreadActions.Count > 0)
            {
                var action = mainThreadActions.Dequeue();
                action?.Invoke();
            }
        }
    }

    public void SendPlace(int x, int y, int player)
    {
        client.Send("/omok/place", x, y, player);
    }

    public void SendDeactivate(string objectName)
    {
        client.Send("/selector/deactivate", objectName);
    }

    private void OnReceivePlace(string address, OscDataHandle data)
    {
        int x = data.GetElementAsInt(0);
        int y = data.GetElementAsInt(1);
        int player = data.GetElementAsInt(2);

        Debug.Log($"[OSC] Received /omok/place → x:{x}, y:{y}, player:{player}");

        lock (queueLock)
        {
            mainThreadActions.Enqueue(() =>
            {
                GameManager.Instance.PlaceStoneFromRemote(x, y, player);
            });
        }
    }

    private void OnReceiveDeactivate(string address, OscDataHandle data)
    {
        string tagName = data.GetElementAsString(0);
        Debug.Log($"[OSC] Received /selector/deactivate → tag: {tagName}");

        lock (queueLock)
        {
            mainThreadActions.Enqueue(() =>
            {
                if (tagName == "Black" && selectorObjects.Length > 0 && selectorObjects[0] != null)
                {
                    selectorObjects[0].SetActive(false);
                    Debug.Log("[OSC] Black selector deactivated");
                }
                else if (tagName == "White" && selectorObjects.Length > 1 && selectorObjects[1] != null)
                {
                    selectorObjects[1].SetActive(false);
                    Debug.Log("[OSC] White selector deactivated");
                }
                else
                {
                    Debug.LogWarning($"[OSC] Tag {tagName} not matched or object missing.");
                }
            });
        }
    }

    void OnDestroy()
    {
        client?.Dispose();
        server?.Dispose();
    }
}
