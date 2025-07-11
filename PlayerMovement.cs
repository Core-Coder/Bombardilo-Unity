using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public bool isMovementLocked = false;

    public float speed = 12f;
    public float gravity = -40;
    public float jumpHeight = 1f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private Vector3 velocity;
    private bool isGrounded;

    public AudioSource audioSource;
    public AudioClip[] woodFootstepSounds;
    public AudioClip[] dirtGravelFootstepSounds;

    private int currentSurfaceLayer = -1;

    void Update()
    {
        if (isMovementLocked)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            return;
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        bool isMoving = move.magnitude > 0.1f;

        if (isGrounded && isMoving)
        {
            RaycastHit hit;
            Vector3 rayStartPoint = groundCheck.position + Vector3.up * 0.1f;
            if (Physics.Raycast(rayStartPoint, Vector3.down, out hit, 1f, groundMask))
            {
                int newSurfaceLayer = hit.collider.gameObject.layer;

                if (newSurfaceLayer != currentSurfaceLayer || !audioSource.isPlaying)
                {
                    currentSurfaceLayer = newSurfaceLayer;
                    PlayFootstepSound(currentSurfaceLayer);
                }
            }
        }
        else
        {
            audioSource.Stop();
            currentSurfaceLayer = -1;
        }

        // if (Input.GetButtonDown("Jump") && isGrounded)
        // {
        //     velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        // }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void PlayFootstepSound(int surfaceLayer)
    {
        AudioClip[] soundGroup = GetSurfaceSound(surfaceLayer);

        if (soundGroup != null && soundGroup.Length > 0)
        {
            AudioClip clip = soundGroup[Random.Range(0, soundGroup.Length)];
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    private AudioClip[] GetSurfaceSound(int layer)
    {
        switch (layer)
        {
            case 11: // floor
                return woodFootstepSounds;
            case 8: // Ground
                return dirtGravelFootstepSounds;
            default:
                return woodFootstepSounds;
        }
    }
}