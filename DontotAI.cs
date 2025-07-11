using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Audio;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    [Header("Pathfinding & Movement")]
    public Transform[] wanderPoints;
    public float wanderSpeed = 2f;
    public float chaseSpeed = 5f;
    public float jumpscareDistance = 1.5f;

    [Header("Wander Behavior")]
    public float minWanderWaitTime = 1f;
    public float maxWanderWaitTime = 4f;
    public int numberOfPointsToConsider = 5;
    [Tooltip("How long the enemy waits at a door after opening it.")]
    public float doorOpenWaitTime = 1.0f;

    [Header("Vision & Detection")]
    public float visionRange = 15f;
    [Range(0, 360)]
    public float visionAngle = 90f;
    public float proximityDetectionRadius = 3f;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;
    public float eyeHeight = 1.5f;

    [Header("Spawning")]
    public float minSpawnDistanceFromPlayer = 10f;

    [Header("Audio")]
    public AudioSource musicAudioSource;
    public AudioSource footstepAudioSource;
    public AudioClip chaseMusic;
    public AudioClip[] walkFootstepSounds;
    public AudioClip[] runFootstepSounds;
    public AudioClip jumpscareSound;
    public float walkStepInterval = 0.7f;
    public float runStepInterval = 0.35f;
    [Range(0, 1)]
    public float footstepVolume = 0.5f;
    public float occludedVolumeMultiplier = 0.4f;
    public AudioMixerSnapshot gameplaySnapshot;
    public AudioMixerSnapshot chaseEndReverbSnapshot;
    public float musicFadeOutTime = 3f;

    public Camera mainPlayerCamera;
    public Camera jumpscareCamera;
    public GameObject gameOverPanel;
    public GameObject otherPanel;

    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private Transform player;
    private PlayerInteraction playerInteraction;
    private int currentWanderPointIndex = -1;
    private bool isWaiting = false;
    private float footstepTimer = 0;

    private Vector3 lastKnownPlayerPosition;
    private bool isSearching = false;

    private int wanderPointsVisited = 0;
    private int wanderPointsToVisitBeforeIdle;

    public enum AIState { Idle, Wander, Chase, Search, Jumpscare }
    private AIState currentState;
    private AudioLowPassFilter footstepFilter;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerInteraction = player.GetComponentInChildren<PlayerInteraction>();

        if (footstepAudioSource != null)
        {
            footstepFilter = footstepAudioSource.GetComponent<AudioLowPassFilter>();
            if (footstepFilter != null)
            {
                footstepFilter.enabled = false;
            }
        }

        if (musicAudioSource == null || footstepAudioSource == null)
        {
            AudioSource[] sources = GetComponents<AudioSource>();
            if (sources.Length >= 2)
            {
                musicAudioSource = sources[0];
                footstepAudioSource = sources[1];
            }
        }

        SpawnAtRandomPoint();
        SwitchState(AIState.Idle);
        wanderPointsToVisitBeforeIdle = Random.Range(1, 6);
        StartCoroutine(WanderWait());
    }

    void Update()
    {
        if (navMeshAgent.isOnOffMeshLink)
        {
            StartCoroutine(TraverseLink());
            return;
        }

        if (currentState == AIState.Wander && !isWaiting && !isSearching && !navMeshAgent.pathPending)
        {
            if (navMeshAgent.remainingDistance < 0.5f)
            {
                wanderPointsVisited++;

                if (wanderPointsVisited >= wanderPointsToVisitBeforeIdle)
                {
                    StartCoroutine(WanderWait());
                }
                else
                {
                    GoToNextWanderPoint();
                }
            }
        }

        if (!isWaiting && !isSearching && CanSeePlayer())
        {
            SwitchState(AIState.Chase);
        }

        if (currentState == AIState.Chase)
        {
            Chase();
        }
        else if (currentState == AIState.Search)
        {
            Search();
        }

        animator.SetFloat("Speed", navMeshAgent.velocity.magnitude);
        HandleFootsteps();
        UpdateAudioOcclusion();
    }

    void UpdateAudioOcclusion()
    {
        if (footstepFilter == null || player == null) return;

        bool isObstructed = Physics.Linecast(transform.position + Vector3.up * eyeHeight, player.position, obstacleLayer);

        footstepFilter.enabled = isObstructed;

        if (isObstructed)
        {
            footstepAudioSource.volume = footstepVolume * occludedVolumeMultiplier;
        }
        else
        {
            footstepAudioSource.volume = footstepVolume;
        }
    }

    public void RequestDoorOpen(PressKeyOpenDoor door)
    {
        if (isWaiting) return;
        StartCoroutine(WaitAtDoor(door));
    }

    private IEnumerator WaitAtDoor(PressKeyOpenDoor door)
    {
        isWaiting = true;
        navMeshAgent.isStopped = true;
        door.OpenDoorForAI();
        yield return new WaitForSeconds(doorOpenWaitTime);
        navMeshAgent.isStopped = false;
        isWaiting = false;
    }

    private IEnumerator TraverseLink()
    {
        navMeshAgent.isStopped = true;
        OffMeshLinkData data = navMeshAgent.currentOffMeshLinkData;
        Vector3 startPos = transform.position;
        Vector3 endPos = data.endPos;
        float duration = Vector3.Distance(startPos, endPos) / navMeshAgent.speed;
        float time = 0f;

        while (time < duration)
        {
            Vector3 newPos = Vector3.Lerp(startPos, endPos, time / duration);
            newPos.y = transform.position.y;
            transform.position = newPos;
            time += Time.deltaTime;
            yield return null;
        }
        Vector3 finalPos = endPos;
        finalPos.y = transform.position.y;
        transform.position = finalPos;

        navMeshAgent.CompleteOffMeshLink();
        navMeshAgent.isStopped = false;
    }

    private void Chase()
    {
        Vector3 enemyPos2D = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 playerPos2D = new Vector3(player.position.x, 0, player.position.z);
        float horizontalDistance = Vector3.Distance(enemyPos2D, playerPos2D);

        if (horizontalDistance < jumpscareDistance)
        {
            SwitchState(AIState.Jumpscare);
            return;
        }

        if (CanSeePlayer())
        {
            lastKnownPlayerPosition = player.position;
            navMeshAgent.destination = player.position;
        }
        else
        {
            SwitchState(AIState.Search);
        }
    }

    private void Search()
    {
        if (CanSeePlayer())
        {
            SwitchState(AIState.Chase);
            return;
        }

        if (!isSearching && !navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
        {
            StartCoroutine(InvestigateArea());
        }
    }

    private IEnumerator InvestigateArea()
    {
        isSearching = true;
        isWaiting = true;

        navMeshAgent.isStopped = true;
        yield return new WaitForSeconds(3.0f);

        if (!CanSeePlayer())
        {
            SwitchState(AIState.Wander);
        }

        isWaiting = false;
        isSearching = false;
        navMeshAgent.isStopped = false;
    }


    private void HandleFootsteps()
    {
        if (currentState == AIState.Idle || navMeshAgent.velocity.magnitude < 0.1f) return;

        footstepTimer -= Time.deltaTime;
        if (footstepTimer <= 0)
        {
            if (currentState == AIState.Wander && walkFootstepSounds.Length > 0)
            {
                footstepAudioSource.pitch = 0.7f;
                footstepAudioSource.PlayOneShot(walkFootstepSounds[Random.Range(0, walkFootstepSounds.Length)], footstepVolume);
                footstepTimer = walkStepInterval;
            }
            else if (currentState == AIState.Chase && runFootstepSounds.Length > 0)
            {
                footstepAudioSource.pitch = 0.7f;
                footstepAudioSource.PlayOneShot(runFootstepSounds[Random.Range(0, runFootstepSounds.Length)], footstepVolume);
                footstepTimer = runStepInterval;
            }
        }
    }

    private void SwitchState(AIState newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        switch (currentState)
        {
            case AIState.Idle:
                navMeshAgent.isStopped = true;
                navMeshAgent.velocity = Vector3.zero;
                if (chaseMusic != null) chaseEndReverbSnapshot.TransitionTo(musicFadeOutTime);
                break;
            case AIState.Wander:
                navMeshAgent.isStopped = false;
                if (chaseMusic != null) chaseEndReverbSnapshot.TransitionTo(musicFadeOutTime);
                navMeshAgent.speed = wanderSpeed;
                GoToNextWanderPoint();
                break;
            case AIState.Chase:
                isWaiting = false;
                isSearching = false;
                navMeshAgent.isStopped = false;
                navMeshAgent.speed = chaseSpeed;
                if (chaseMusic != null) gameplaySnapshot.TransitionTo(0.1f);
                if (musicAudioSource.clip != chaseMusic || !musicAudioSource.isPlaying)
                {
                    musicAudioSource.clip = chaseMusic;
                    musicAudioSource.loop = true;
                    musicAudioSource.Play();
                }
                break;
            case AIState.Search:
                if (chaseMusic != null) chaseEndReverbSnapshot.TransitionTo(musicFadeOutTime);
                navMeshAgent.speed = wanderSpeed;
                navMeshAgent.destination = lastKnownPlayerPosition;
                break;
            case AIState.Jumpscare:
                TriggerJumpscare();
                break;
        }
    }

    private void GoToNextWanderPoint()
    {
        if (wanderPoints.Length == 0) return;
        var sortedPoints = wanderPoints.Where(point => point != (currentWanderPointIndex != -1 ? wanderPoints[currentWanderPointIndex] : null)).OrderBy(point => Vector3.Distance(transform.position, point.position)).ToList();
        int range = Mathf.Min(numberOfPointsToConsider, sortedPoints.Count);
        var closestPoints = sortedPoints.Take(range).ToList();
        if (closestPoints.Count > 0)
        {
            Transform nextPoint = closestPoints[Random.Range(0, closestPoints.Count)];
            currentWanderPointIndex = System.Array.IndexOf(wanderPoints, nextPoint);
            if (navMeshAgent.isOnNavMesh) navMeshAgent.destination = nextPoint.position;
        }
    }

    private IEnumerator WanderWait()
    {
        isWaiting = true;
        SwitchState(AIState.Idle);
        float waitTime = Random.Range(minWanderWaitTime, maxWanderWaitTime);
        yield return new WaitForSeconds(waitTime);
        isWaiting = false;
        wanderPointsVisited = 0;
        wanderPointsToVisitBeforeIdle = Random.Range(1, 6);
        SwitchState(AIState.Wander);
    }

    private IEnumerator CheckDestinationReached()
    {
        while (enabled)
        {
            yield return new WaitForSeconds(0.5f);

            if (!isWaiting && !isSearching && currentState == AIState.Wander && navMeshAgent.isOnNavMesh && !navMeshAgent.pathPending)
            {
                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                {
                    wanderPointsVisited++;

                    if (wanderPointsVisited >= wanderPointsToVisitBeforeIdle)
                    {
                        StartCoroutine(WanderWait());
                    }
                    else
                    {
                        GoToNextWanderPoint();
                    }
                }
            }
        }
    }

    private bool CanSeePlayer()
    {
        if (playerInteraction == null || playerInteraction.IsPlayerHiding()) return false;
        Vector3 enemyEyePosition = transform.position + Vector3.up * eyeHeight;
        Vector3 directionToPlayer = player.position - enemyEyePosition;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer < proximityDetectionRadius)
        {
            if (!Physics.Linecast(enemyEyePosition, player.position, obstacleLayer)) return true;
        }
        if (distanceToPlayer < visionRange)
        {
            if (Vector3.Angle(transform.forward, directionToPlayer) < visionAngle / 2)
            {
                if (!Physics.Linecast(enemyEyePosition, player.position, obstacleLayer)) return true;
            }
        }
        return false;
    }

    private void SpawnAtRandomPoint()
    {
        if (wanderPoints.Length == 0) return;
        List<Transform> validSpawnPoints = new List<Transform>();
        foreach (Transform point in wanderPoints)
        {
            string pointName = point.gameObject.name;
            bool isExcluded = false;
            int startIndex = pointName.IndexOf('(');
            int endIndex = pointName.IndexOf(')');
            if (startIndex != -1 && endIndex != -1)
            {
                string numberStr = pointName.Substring(startIndex + 1, endIndex - startIndex - 1);
                if (int.TryParse(numberStr, out int pointNumber))
                {
                    if (pointNumber >= 52 && pointNumber <= 64) isExcluded = true;
                }
            }
            if (isExcluded) continue;
            if (Vector3.Distance(point.position, player.position) > minSpawnDistanceFromPlayer)
            {
                validSpawnPoints.Add(point);
            }
        }
        if (validSpawnPoints.Count > 0)
        {
            Transform spawnPoint = validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];
            navMeshAgent.Warp(spawnPoint.position);
        }
        else
        {
            Debug.LogWarning("No valid spawn points found. Spawning at a random point as a fallback.");
            List<Transform> fallbackPoints = new List<Transform>();
            foreach (Transform point in wanderPoints)
            {
                if (!point.gameObject.name.Contains(" (5") && !point.gameObject.name.Contains(" (6")) fallbackPoints.Add(point);
            }
            if (fallbackPoints.Count > 0) navMeshAgent.Warp(fallbackPoints[Random.Range(0, fallbackPoints.Count)].position);
            else navMeshAgent.Warp(wanderPoints[Random.Range(0, wanderPoints.Length)].position);
        }
    }

    private void RestartLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    private void TriggerJumpscare()
    {
        isWaiting = true;
        isSearching = true;
        enabled = false;
        if (navMeshAgent.isOnNavMesh) navMeshAgent.enabled = false;
        otherPanel.SetActive(false);

        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null) playerMovement.isMovementLocked = true;

        if (mainPlayerCamera != null)
        {
            transform.position = mainPlayerCamera.transform.position + mainPlayerCamera.transform.forward * jumpscareDistance;
            Vector3 lookAtPosition = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.LookAt(lookAtPosition);
        }

        StartCoroutine(JumpscareCameraSequence());
        animator.SetTrigger("Jumpscare");
        if (jumpscareSound != null) footstepAudioSource.PlayOneShot(jumpscareSound);
    }

    private IEnumerator JumpscareCameraSequence()
    {
        if (mainPlayerCamera != null) mainPlayerCamera.gameObject.SetActive(false);
        if (jumpscareCamera != null) jumpscareCamera.gameObject.SetActive(true);

        Animator cameraAnimator = jumpscareCamera.GetComponent<Animator>();
        if (cameraAnimator != null)
        {
            cameraAnimator.SetTrigger("Shake");
        }

        yield return new WaitForSeconds(2.0f);

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, proximityDetectionRadius);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, jumpscareDistance);

        Gizmos.color = Color.red;
        Vector3 forward = transform.forward;
        Vector3 leftRayDirection = Quaternion.Euler(0, -visionAngle / 2, 0) * forward;
        Gizmos.DrawRay(transform.position, leftRayDirection * visionRange);
        Gizmos.DrawRay(transform.position, (Quaternion.Euler(0, visionAngle, 0) * leftRayDirection) * visionRange);

        #if UNITY_EDITOR
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawWireArc(transform.position, Vector3.up, leftRayDirection, visionAngle, visionRange);
        #endif
    }
}