using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MenangTextController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI textComponent;
    public CanvasGroup panelGroup;

    [Header("Typing Settings")]
    public string[] lines;
    public float textSpeed = 0.05f;
    public float pauseAfterLine = 2f;

    [Header("Fade & Scene")]
    public float endDelay      = 2f;
    public float fadeDuration = 1f;

    private int index = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private Coroutine advanceCoroutine;

    void Start()
    {
        ShowCurrentLine();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                textComponent.text = lines[index];
                isTyping = false;

                if (advanceCoroutine != null)
                    StopCoroutine(advanceCoroutine);
                advanceCoroutine = StartCoroutine(AutoAdvance());
            }
            else
            {
                if (advanceCoroutine != null)
                    StopCoroutine(advanceCoroutine);

                NextLineOrEnd();
            }
        }
    }

    private void ShowCurrentLine()
    {
        textComponent.text = string.Empty;
        typingCoroutine = StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine()
    {
        isTyping = true;
        foreach (char c in lines[index])
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
        isTyping = false;

        advanceCoroutine = StartCoroutine(AutoAdvance());
    }

    private IEnumerator AutoAdvance()
    {
        yield return new WaitForSeconds(pauseAfterLine);
        NextLineOrEnd();
    }

    private void NextLineOrEnd()
    {
        index++;
        if (index < lines.Length)
        {
            if (typingCoroutine  != null) StopCoroutine(typingCoroutine);
            if (advanceCoroutine != null) StopCoroutine(advanceCoroutine);

            ShowCurrentLine();
        }
        else
        {
            StartCoroutine(EndIntro());
        }
    }

    private IEnumerator EndIntro()
    {
        yield return new WaitForSeconds(endDelay);

        float t = 0f;
        while (t < fadeDuration)
        {
            panelGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }
        panelGroup.alpha = 0f;

        SceneManager.LoadScene("Main Menu");
    }
}
