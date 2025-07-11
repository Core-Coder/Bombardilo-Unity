using UnityEngine;
using UnityEngine.Playables;

public class CameraHandOff : MonoBehaviour
{
    public Camera timelineCam;
    public Camera playerCam;
    public CameraMovement camMovement;
    public float xRotation;
    public float YRotation;

    public void DoHandOff()
    {
        var pos = timelineCam.transform.position;
        var rot = timelineCam.transform.rotation;

        playerCam.transform.position = pos;
        playerCam.transform.rotation = rot;

        Vector3 e = playerCam.transform.localEulerAngles;
        float pitch = e.x > 180f ? e.x - 360f : e.x;
        camMovement.xRotation = pitch;
        camMovement.YRotation = e.y;

        playerCam.enabled   = true;
        timelineCam.enabled = false;
    }
}
