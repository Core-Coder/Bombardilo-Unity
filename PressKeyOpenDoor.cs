using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class PressKeyOpenDoor : MonoBehaviour
{
    public GameObject Instruction;
    public GameObject AnimeObject;
    public NavMeshLink navMeshLink;

    public bool IsOpen { get; private set; } = false;
    private bool playerIsPresent = false;

    private NavMeshObstacle navMeshObstacle;
    private AudioSource doorOpenSound;
    private AudioSource doorCloseSound;
    private TextMeshProUGUI instructionText;
    private Transform playerTransform;

    void Start()
    {
        instructionText = Instruction.GetComponent<TextMeshProUGUI>();
        navMeshObstacle = AnimeObject.GetComponentInChildren<NavMeshObstacle>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) playerTransform = playerObject.transform;

        if (AnimeObject != null)
        {
            AudioSource[] sources = AnimeObject.GetComponentsInChildren<AudioSource>();
            foreach (AudioSource source in sources)
            {
                if (source.gameObject.name.Contains("Open")) doorOpenSound = source;
                else if (source.gameObject.name.Contains("Close")) doorCloseSound = source;
            }
        }

        if(Instruction != null) Instruction.SetActive(false);
    }

    public void OpenDoorForAI()
    {
        if (!IsOpen)
        {
            PerformOpenActions();
        }
    }

    public void CloseDoor()
    {
        if (IsOpen)
        {
            PerformCloseActions();
        }
    }

    private void PerformOpenActions()
    {
        IsOpen = true;
        if(AnimeObject != null) AnimeObject.GetComponent<Animator>().Play("DoorOpen");
        if(doorOpenSound != null) doorOpenSound.Play();
        if(navMeshLink != null) navMeshLink.enabled = true;
    }

    private void PerformCloseActions()
    {
        IsOpen = false;
        if(AnimeObject != null) AnimeObject.GetComponent<Animator>().Play("DoorClose");
        if(doorCloseSound != null) doorCloseSound.Play();
        if(navMeshLink != null) navMeshLink.enabled = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsPresent = true;
            if(Instruction != null) Instruction.SetActive(true);
            UpdateInstructionText();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsPresent = false;
            if(Instruction != null) Instruction.SetActive(false);
        }
    }

    void Update()
    {
        if (playerIsPresent && Input.GetKeyDown(KeyCode.E))
        {
            if (IsOpen)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                if (distance <= 5f)
                {
                    PerformCloseActions();
                    UpdateInstructionText();
                }
            }
            else
            {
                PerformOpenActions();
                UpdateInstructionText();
            }
        }
    }

    void UpdateInstructionText()
    {
        if (instructionText != null)
        {
            instructionText.text = IsOpen ? "Press E to Close Door" : "Press E to Open Door";
        }
    }
}