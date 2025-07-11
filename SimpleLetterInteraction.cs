using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SimpleLetterInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public float interactDistance = 3f;
    public LayerMask interactableLayer;
    public TMP_Text interactionText;
    public GameObject letterPanel;
    public TMP_Text letterText;
    public TMP_Text backHintText;

    private GameObject currentInteractable;
    private bool isReadingLetter = false;

    void Start()
    {
        if (backHintText != null)
            backHintText.gameObject.SetActive(false);

        if (interactionText != null)
            interactionText.gameObject.SetActive(false);

        if (letterPanel != null)
            letterPanel.SetActive(false);
    }

    void Update()
    {
        if (isReadingLetter)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                CloseLetter();
            }
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactableLayer))
        {
            currentInteractable = hit.collider.gameObject;
            if (interactionText != null)
            {
                interactionText.text = "Press E to Read";
                interactionText.gameObject.SetActive(true);
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                CollectObject();
            }
        }
        else
        {
            currentInteractable = null;
            if (interactionText != null)
                interactionText.gameObject.SetActive(false);
        }
    }

    void CollectObject()
    {
        Debug.Log("Collected Letter: " + currentInteractable.name);
        if (interactionText != null)
            interactionText.gameObject.SetActive(false);

        ShowLetter("INI SEMUA GARA - GARA KAMU DIN...., ANDAI SAJA KAMU TIDAK ADA. LEBIH BAIK KAMU MA**");

        Destroy(currentInteractable);
    }

    void ShowLetter(string message)
    {
        if (letterPanel != null)
            letterPanel.SetActive(true);
        if (letterText != null)
            letterText.text = message;
        if (backHintText != null)
            backHintText.gameObject.SetActive(true);

        isReadingLetter = true;
    }

    void CloseLetter()
    {
        if (letterPanel != null)
            letterPanel.SetActive(false);
        if (letterText != null)
            letterText.text = "";
        if (backHintText != null)
            backHintText.gameObject.SetActive(false);

        isReadingLetter = false;

        SceneManager.LoadScene("Cutscene Dipukul Dontol");
    }
}
