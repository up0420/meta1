using UnityEngine;
using OscJack;
using TMPro;

public class SimpleOSCReceive : MonoBehaviour
{
    public int port = 7000;

    public string positionAddress = "/object/position";
    public string rotationAddress = "/object/rotation";

    public TMP_Text displayText;
    public GameObject Box;
    public GameObject TargetOffsetObject;

    private OscServer server;

    private Vector3 receivedPosition;
    private Vector3 receivedRotation;

    private Vector3 finalPosition;
    private Quaternion finalRotation;

    void Start()
    {
        server = new OscServer(port);
        server.MessageDispatcher.AddCallback(positionAddress, OnReceivePosition);
        server.MessageDispatcher.AddCallback(rotationAddress, OnReceiveRotation);
    }

    void OnReceivePosition(string address, OscDataHandle data)
    {
        if (data.GetElementCount() >= 3)
        {
            receivedPosition.x = data.GetElementAsFloat(0);
            receivedPosition.y = data.GetElementAsFloat(1);
            receivedPosition.z = data.GetElementAsFloat(2);

            Debug.Log($"[Position] Received: ({receivedPosition.x}, {receivedPosition.y}, {receivedPosition.z})");
        }
    }

    void OnReceiveRotation(string address, OscDataHandle data)
    {
        if (data.GetElementCount() >= 3)
        {
            receivedRotation.x = data.GetElementAsFloat(0);
            receivedRotation.y = data.GetElementAsFloat(1);
            receivedRotation.z = data.GetElementAsFloat(2);

            Debug.Log($"[Rotation] Received: ({receivedRotation.x}, {receivedRotation.y}, {receivedRotation.z})");
        }
    }

    void Update()
    {
        Vector3 offsetPos = TargetOffsetObject != null ? TargetOffsetObject.transform.position : Vector3.zero;
        Vector3 offsetRot = TargetOffsetObject != null ? TargetOffsetObject.transform.eulerAngles : Vector3.zero;

        finalPosition = receivedPosition + offsetPos;
        finalRotation = Quaternion.Euler(receivedRotation + offsetRot);

        if (Box != null)
        {
            Box.transform.position = finalPosition;
            Box.transform.rotation = finalRotation;
        }

        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (displayText != null)
        {
            displayText.text =
                $"Received Pos: {receivedPosition}\n" +
                $"Received Rot: {receivedRotation}\n" +
                $"Final Pos: {finalPosition}\n" +
                $"Final Rot: {finalRotation.eulerAngles}";
        }
    }

    void OnDestroy()
    {
        server?.Dispose();
    }
}
