using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Security.Cryptography.X509Certificates;
using System;

public class PlayerInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public float interactDistance = 3f;
    public LayerMask interactableLayer;
    public LayerMask hideLayer;
    public LayerMask bookLayer;
    public LayerMask bookPlacementLayer;
    public LayerMask keyLayer;
    public TMP_Text interactionText;
    public TMP_Text hideText;
    public TMP_Text bookText;
    public GameObject bookUIContainer;
    public GameObject keyUIContainer;
    public GameObject letterPanel;
    public TMP_Text letterText;
    public TMP_Text backHintText;
    public GameObject bookshelfToHide;
    public GameObject bookshelfToHide1;
    public BookUIUpdater bookUIUpdater;
    public TMP_Text keyCountText;
    
    // Tambahkan referensi ke CameraMovement
    public CameraMovement cameraMovementScript;
    public AudioClip pickupSound;
    public AudioSource playerAudioSource;
    public GameObject BookLink;

    private int booksCollected = 0;
    private int totalBooks = 3;
    private bool canPlaceBooks = false;
    private bool hasPlacedBooks = false;
    private bool isHiding = false;
    private Vector3 previousPosition;
    public int keysCollected = 0;

    private bool isNearBookShelf = false;
    private GameObject currentInteractable;
    private bool isReadingLetter = false;

    public bool IsPlayerHiding()
    {
        return isHiding;
    }

    void Start()
    {
        backHintText.gameObject.SetActive(false);
        if (keyUIContainer != null)
        {
            keyUIContainer.SetActive(false);
        }
        if (keyCountText != null)
        {
            keyCountText.text = keysCollected.ToString();
        }
        if (bookUIContainer != null)
        {
            bookUIContainer.SetActive(false);
        }

        if (cameraMovementScript == null)
        {
            Debug.LogError("CameraMovementScript tidak terhubung di PlayerInteraction! Pastikan diseret di Inspector.");
            cameraMovementScript = playerCamera.GetComponent<CameraMovement>();
            if (cameraMovementScript == null)
            {
                Debug.LogError("Tidak dapat menemukan CameraMovementScript di PlayerInteraction atau di Camera.");
            }
        }
    }

    void Update()
    {
        if (isReadingLetter)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("im here close letter");
                CloseLetter();
            }
            return;
        }

        if (isHiding)
        {
            if (hideText != null)
            {
                hideText.text = "Press F to unhide";
                hideText.gameObject.SetActive(true);
            }

            if (interactionText != null)
                interactionText.gameObject.SetActive(false);

            if (bookText != null)
                bookText.gameObject.SetActive(false);

            if (Input.GetKeyDown(KeyCode.F))
            {
                Hide();
            }

            return;
        }

        // Proceed with raycasting only if not hiding
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            GameObject hitObject = hit.collider.gameObject;
            int hitLayer = hitObject.layer;

            if (((1 << hitLayer) & interactableLayer) != 0)
            {
                currentInteractable = hitObject;
                interactionText.gameObject.SetActive(true);
                hideText.gameObject.SetActive(false);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Debug.Log("im here collect object");
                    CollectObject();
                }
            }
            else if (((1 << hitLayer) & hideLayer) != 0)
            {
                currentInteractable = hitObject;
                hideText.text = "Press F to hide";
                hideText.gameObject.SetActive(true);
                interactionText.gameObject.SetActive(false);

                if (Input.GetKeyDown(KeyCode.F))
                {
                    Hide();
                }
            }
            else if (((1 << hitLayer) & bookLayer) != 0)
            {
                currentInteractable = hitObject;
                bookText.text = "Press E to collect";
                bookText.gameObject.SetActive(true);
                interactionText.gameObject.SetActive(false);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Debug.Log("im here collect book");
                    collectBook();
                }
            }
            else if (((1 << hitLayer) & bookPlacementLayer) != 0 && canPlaceBooks && !hasPlacedBooks)
            {
                currentInteractable = hitObject;
                interactionText.text = "Press E to place books";
                interactionText.gameObject.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Debug.Log("im here place book");
                    PlaceBooks();
                }
            }
            else if (((1 << hitLayer) & keyLayer) != 0)
            {
                currentInteractable = hitObject;
                interactionText.text = "Press E to collect";
                interactionText.gameObject.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Debug.Log("im here collect key");
                    collectKey();
                }
            }
            else
            {
                ClearUI();
            }
        }
        else
        {
            ClearUI();
        }
    }

    void ClearUI()
    {
        currentInteractable = null;

        if (interactionText != null)
            interactionText.gameObject.SetActive(false);

        if (!isHiding && hideText != null)
            hideText.gameObject.SetActive(false);

        if (bookText != null)
            bookText.gameObject.SetActive(false);
    }

    void PlaceBooks()
    {
        hasPlacedBooks = true;
        interactionText.gameObject.SetActive(false);

        if (bookshelfToHide != null)
            bookshelfToHide.SetActive(false);
        if (bookshelfToHide1 != null)
            bookshelfToHide1.SetActive(false);

        if (bookUIContainer != null)
        {
            bookUIContainer.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Book UI Container tidak terhubung! UI buku tidak akan disembunyikan.");
        }

        if (keyUIContainer != null)
        {
            keyUIContainer.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Key UI Container tidak terhubung! UI kunci tidak akan ditampilkan.");
        }

        BookLink.SetActive(true);

        Debug.Log("Books placed. Secret room revealed! Now find the key!");
    }

    void collectBook()
    {
        Debug.Log("Book collected: " + currentInteractable.name);
        bookText.gameObject.SetActive(false);
        Destroy(currentInteractable);

        if (pickupSound != null)
        {
            playerAudioSource.PlayOneShot(pickupSound);
        }

        booksCollected++;

        if (bookUIContainer != null)
        {
            bookUIContainer.SetActive(true);
        }

        if (bookUIUpdater != null)
        {
            bookUIUpdater.BookCollected();
        }
        else
        {
            Debug.LogWarning("BookUIUpdater tidak terhubung! Pastikan UIManager diseret ke slot di Inspector.");
        }

        if (booksCollected >= totalBooks)
        {
            canPlaceBooks = true;
            Debug.Log("All books collected! Find a place to put them.");
        }
    }

    void collectKey()
    {
        Debug.Log("Collected Key: " + currentInteractable.name);
        interactionText.gameObject.SetActive(false);
        Destroy(currentInteractable);

        keysCollected++;
        if (keyCountText != null)
        {
            keyCountText.text = keysCollected.ToString();
        }
        else
        {
            Debug.LogWarning("Key Count Text tidak terhubung! UI jumlah kunci tidak akan terupdate.");
        }
    }

    void Hide()
    {
        HideSpot spot = currentInteractable.GetComponent<HideSpot>();
        CharacterController controller = GetComponentInParent<CharacterController>();
        PlayerMovement movement = GetComponentInParent<PlayerMovement>();
        Transform rootTransform = controller != null ? controller.transform : transform;

        if (spot != null && spot.hidePoint != null)
        {
            if (!isHiding)
            {
                // MASUK MODE HIDE
                previousPosition = rootTransform.position;

                if (controller != null) controller.enabled = false;
                rootTransform.position = spot.hidePoint.position;
                if (controller != null) controller.enabled = true;

                if (movement != null) movement.isMovementLocked = true;

                // Kunci pergerakan kamera vertikal
                if (cameraMovementScript != null) 
                {
                    cameraMovementScript.isVerticalLookLocked = true;
                }

                isHiding = true;
                hideText.text = "Press F to unhide";
            }
            else
            {
                // KELUAR MODE HIDE
                if (controller != null) controller.enabled = false;
                rootTransform.position = previousPosition;
                if (controller != null) controller.enabled = true;

                if (movement != null) movement.isMovementLocked = false;

                // Buka kunci pergerakan kamera vertikal
                if (cameraMovementScript != null) 
                {
                    cameraMovementScript.isVerticalLookLocked = false;
                }

                isHiding = false;
                hideText.text = "Press F to hide";
            }
        }
        else
        {
            Debug.LogWarning("HideSpot atau hidePoint tidak terhubung di: " + currentInteractable.name + ". Tidak dapat hide.");
            isHiding = false; 
            if (cameraMovementScript != null) 
            {
                cameraMovementScript.isVerticalLookLocked = false;
            }
        }
    }

    void CollectObject()
    {
        Debug.Log("Collected: " + currentInteractable.name);
        interactionText.gameObject.SetActive(false);

        ShowLetter("INI SEMUA GARA - GARA KAMU DIN...., ANDAI SAJA KAMU TIDAK ADA. LEBIH BAIK KAMU MA**");
    }

    void ShowLetter(string message)
    {
        letterPanel.SetActive(true);
        letterText.text = message;
        backHintText.gameObject.SetActive(true);
        isReadingLetter = true;
    }

    void CloseLetter()
    {
        letterPanel.SetActive(false);
        letterText.text = "";
        backHintText.gameObject.SetActive(false);
        isReadingLetter = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BookShelfTrigger"))
        {
            isNearBookShelf = true;
            if (bookUIContainer != null)
            {
                bookUIContainer.SetActive(true);
            }
            Debug.Log("Pemain mendekati rak buku.");
        }
    }
}