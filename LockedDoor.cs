using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LockedDoor : MonoBehaviour
{
    [Header("Required Components")]
    [Tooltip("The text UI that shows 'Pintu ini terkunci'")]
    public TMP_Text lockedMessageText;
    public GameObject playerObject;

    [Header("Sounds")]
    public AudioSource lockedSound;
    public AudioSource unlockedSound;

    [Header("Win Condition")]
    [Tooltip("The name of the scene to load when the door opens")]
    public string winSceneName = "Menang";
    public Image fadeToBlackImage;

    private bool playerIsPresent = false;
    private PlayerInteraction playerInteraction;

    void Start()
    {
        if (playerObject != null)
        {
            playerInteraction = playerObject.GetComponentInChildren<PlayerInteraction>();
        }
        if (lockedMessageText != null)
        {
            lockedMessageText.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
      if (other.CompareTag("Player"))
      {
        playerIsPresent = true;
        if (playerInteraction != null && playerInteraction.keysCollected <= 0)
        {
          if (lockedMessageText != null)
          {
            lockedMessageText.text = "Pintu ini terkunci";
            lockedMessageText.gameObject.SetActive(true);
          }
        }
        else
        {
          if (lockedMessageText != null)
          {
            lockedMessageText.text = "Press E to Open Door";
            lockedMessageText.gameObject.SetActive(true);
          }
        }
      }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsPresent = false;
            if (lockedMessageText != null)
            {
                lockedMessageText.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (playerIsPresent && Input.GetKeyDown(KeyCode.E))
        {
            if (playerInteraction != null && playerInteraction.keysCollected > 0)
            {
              StartCoroutine(UnlockAndWin());
            }
            else
            {
              if (lockedSound != null) lockedSound.Play();
            }
        }
    }

    private IEnumerator UnlockAndWin()
    {
        GetComponent<Collider>().enabled = false;

        if (unlockedSound != null) unlockedSound.Play();

        float fadeDuration = 1.5f;
        float timer = 0f;

        if (fadeToBlackImage != null)
        {
            fadeToBlackImage.gameObject.SetActive(true);
        }

        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            if (fadeToBlackImage != null)
            {
                fadeToBlackImage.color = new Color(0, 0, 0, alpha);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        SceneManager.LoadScene(winSceneName);
    }
}