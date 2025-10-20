using UnityEngine;
using OscJack;
using System;

public class SimpleOSCSend : MonoBehaviour
{
    public string ipAddress = "192.168.0.14";
    public int port = 7000;

    [Header("OSC Addresses")]
    public string positionAddress = "/object/position";
    public string rotationAddress = "/object/rotation";

    [Header("Source")]
    public GameObject targetObject;

    [Header("REsiliecence")]
    [Tooltip("전송 예외 발생 시 클라이언트를 즉시 재시도 하지 않고, 지정 초 후 재초기화합니다.")]
    public float retryInitInterval = 2.0f;
    [Tooltip("같은 오류 로그가 너무 많이 찍히는걸 방지하기 위한 최소 간격(초)")]
    public float logThrottleInterval = 1.5f;
    [Tooltip("성공적으로 전송했을 때 로그를 남깁니다.(성능 영향을 줄 수 있음).")]
    public bool verboseSendlog = false;

    private OscClient client;
    private bool isClientInitialized = false;

    //재시도/로그 스케쥴링
    private float nextInitTime = 0f;
    private float nextLogTime = 0f;

    void OnEnable()
    {
        TryInitClient();
    }
    void ONdisable()
    {
        DisposeClient();
    }
    void OnDestroy()
    {
        DisposeClient();
    }

    void Update()
    {
        if (!isClientInitialized && Time.time >= nextInitTime)
        {
            TryInitClient();
        }
        if (!isClientInitialized || targetObject == null) return;

        try
        {
            var t = targetObject.transform;
            Vector3 pos = t.position;
            Vector3 rot = t.eulerAngles;

            client.Send(positionAddress, pos.x, pos.y, pos.z);
            client.Send(rotationAddress, rot.x, rot.y, rot.z);

            if (verboseSendlog) Debug.Log("[OSC] Sent position/rotation.");
        }

        catch (Exception e)
        {
            //에러를 '흡수'하고, 재시도 스케줄만 설정
            ThroattledLogError($"[OSC] Send failed: {e.Message}. Will retry init in {retryInitInterval:0.##}s.");
            //안전하게 재초기화 준비
            DisposeClient();
            nextInitTime = Time.time + retryInitInterval;
        }

    }

    private void TryInitClient()
    {
        DisposeClient();

        try
        {
            client = new OscClient(ipAddress, port);
            isClientInitialized = true;
            Debug.Log($"[OSC] Client initialized -> {ipAddress}:{port}");
        }
        catch (Exception e)
        {
            isClientInitialized = false;
            ThroattledLogError($"[OSC] Client init failed -> {ipAddress}:{port} : {e.Message}");
            nextInitTime = Time.time + retryInitInterval; //다음 재시도 예약
        }
    }
    private void DisposeClient()
    {
        if (client != null)
        {
            try { client.Dispose(); } catch {/*ignore*/}
            client = null;
        }

        isClientInitialized = false;
    }
    private void ThroattledLogError(string msg)
    {
        if (Time.time >= nextLogTime)
        {
            Debug.LogError(msg);
            nextLogTime = Time.time + logThrottleInterval;
        }
    }
    
}
