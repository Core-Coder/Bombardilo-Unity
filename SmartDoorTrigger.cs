using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class EnemyDoorInteractor : MonoBehaviour
{
    [Header("Component Links")]
    public PressKeyOpenDoor doorScript;

    [Header("Settings")]
    public float closeDelay = 1.0f;

    private int agentsInRange = 0;
    private Coroutine closeCoroutine = null;

    void Start()
    {
        if (doorScript == null) doorScript = GetComponent<PressKeyOpenDoor>();
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyAI enemy = other.GetComponentInParent<EnemyAI>();
        if (enemy != null && doorScript != null && !doorScript.IsOpen)
        {
            if (closeCoroutine != null)
            {
                StopCoroutine(closeCoroutine);
                closeCoroutine = null;
            }
            agentsInRange++;
            enemy.RequestDoorOpen(doorScript);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        EnemyAI enemy = other.GetComponentInParent<EnemyAI>();
        if (enemy != null && doorScript != null && doorScript.IsOpen)
        {
            agentsInRange--;
            if(agentsInRange <= 0)
            {
                closeCoroutine = StartCoroutine(CloseDoorAfterDelay());
            }
        }
    }

    private IEnumerator CloseDoorAfterDelay()
    {
        yield return new WaitForSeconds(closeDelay);
        if (agentsInRange <= 0)
        {
            doorScript.CloseDoor();
        }
        closeCoroutine = null;
    }
}