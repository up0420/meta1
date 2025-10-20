using UnityEngine;
using System.Collections;

public class FloatingBoxObject : MonoBehaviour
{
    [SerializeField] float floatStrength = 0.5f;
    [SerializeField] float floatSpeed = 1f;
    [SerializeField] float resetDelay = 10f;
    [SerializeField] float resetDuration = 1f; // 보간 시간(초)

    private Vector3 startPos;
    private bool isFloating = true;
    private Coroutine resetCoroutine;

    void Start()
    {
        startPos = transform.position;

        floatStrength *= Random.Range(0.8f, 1.2f);
        floatSpeed *= Random.Range(0.8f, 1.2f);
        startPos.y += Random.Range(-0.5f, 0.5f);
    }

    void Update()
    {
        if (isFloating)
        {
            transform.position = new Vector3(
                startPos.x,
                startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatStrength,
                startPos.z // ← z 추가
            );
        }
    }

    // 함수명 정확히 일치해야 콜백이 호출됩니다.
    void OnCollisionEnter(Collision collision)
    {
        isFloating = false;

        if (resetCoroutine != null)
            StopCoroutine(resetCoroutine);

        resetCoroutine = StartCoroutine(ResetPosition());
    }

    IEnumerator ResetPosition()
    {
        yield return new WaitForSeconds(resetDelay);

        float elapsed = 0f;
        Vector3 from = transform.position; // ← 현재 위치 올바르게 가져오기

        while (elapsed < resetDuration)
        {
            float t = elapsed / resetDuration;
            transform.position = Vector3.Lerp(from, startPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = startPos;
        isFloating = true;
        resetCoroutine = null;
    }
}
