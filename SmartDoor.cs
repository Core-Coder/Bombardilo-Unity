using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.AI.Navigation;

[RequireComponent(typeof(NavMeshObstacle))]
public class SmartDoor : MonoBehaviour
{
    [Header("Door Settings")]
    public bool isOpen = false;
    public float openSpeed = 3f;
    public float rotationAmount = 90f;

    [Header("Component Links")]
    [Tooltip("The NavMeshLink object that belongs to this door.")]
    public NavMeshLink navMeshLink;

    private NavMeshObstacle obstacle;
    private Quaternion startRotation;
    private Quaternion endRotation;
    private Coroutine animationCoroutine;

    void Awake()
    {
        obstacle = GetComponent<NavMeshObstacle>();
        obstacle.carving = false;
        obstacle.enabled = false;
        startRotation = transform.rotation;

        if (navMeshLink != null) navMeshLink.enabled = false;
    }

    public void Open(Vector3 userPosition)
    {
        if (!isOpen)
        {
            if (animationCoroutine != null) StopCoroutine(animationCoroutine);
            float dot = Vector3.Dot(transform.forward, (userPosition - transform.position).normalized);
            endRotation = startRotation * Quaternion.Euler(0, rotationAmount * (dot < 0 ? 1 : -1), 0);
            animationCoroutine = StartCoroutine(AnimateDoor(true));
        }
    }

    public void Close()
    {
        if (isOpen)
        {
            if (animationCoroutine != null) StopCoroutine(animationCoroutine);
            animationCoroutine = StartCoroutine(AnimateDoor(false));
        }
    }

    IEnumerator AnimateDoor(bool opening)
    {
        isOpen = opening;
        Quaternion targetRotation = opening ? endRotation : startRotation;

        if (navMeshLink != null) navMeshLink.enabled = opening;
        if (!opening) obstacle.enabled = false;

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
            yield return null;
        }

        if (opening) obstacle.enabled = true;
    }
}