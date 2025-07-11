using UnityEngine;
using UnityEngine.Playables;

public class AutoCutscenePlay : MonoBehaviour
{
    public PlayableDirector cutsceneTimeline;

    void Start()
    {
        if (cutsceneTimeline != null)
        {
            cutsceneTimeline.Play();
        }
    }
}
