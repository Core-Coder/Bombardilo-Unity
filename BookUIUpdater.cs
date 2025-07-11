using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BookUIUpdater : MonoBehaviour
{
    public Image bookIconImage;
    public TextMeshProUGUI bookCountText;
    private int booksCollected = 0;

    void Start()
    {
        if (bookIconImage != null)
        {
            bookIconImage.color = new Color(bookIconImage.color.r, bookIconImage.color.g, bookIconImage.color.b, 1f);
        }

        if (bookCountText != null)
        {
            bookCountText.text = booksCollected.ToString();
        }
    }

    public void BookCollected()
    {
        booksCollected++; 

        if (bookCountText != null)
        {
            bookCountText.text = booksCollected.ToString();
        }

        Debug.Log("Buku terkumpul: " + booksCollected);
    }

    public int GetBooksCollectedCount()
    {
        return booksCollected;
    }
}