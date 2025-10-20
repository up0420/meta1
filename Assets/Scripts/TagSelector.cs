using UnityEngine;

public class TagSelector : MonoBehaviour
{
    [Tooltip("손 오브젝트에 붙일 태그 이름 (예: Black, White)")]
    public string tagToAssign;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Untagged"))
        {
            other.tag = tagToAssign;
            OSCManager.Instance.SendDeactivate(tagToAssign);
            gameObject.SetActive(false);
        }
    }
}
