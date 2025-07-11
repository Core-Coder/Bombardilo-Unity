using System.Collections;
using UnityEngine;
using TMPro;

public class SubtitleTrigger : MonoBehaviour
{
    public string subtitleMessage;
    public float displayTime = 3f;
    public TMP_Text subtitleText;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(ShowSubtitle());
        }
    }

    private IEnumerator ShowSubtitle()
    {
        subtitleText.text = subtitleMessage;
        yield return new WaitForSeconds(displayTime);
        subtitleText.text = "";
        Destroy(gameObject);
    }
}
