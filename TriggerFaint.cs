using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class TriggerFaint : MonoBehaviour
{
    public PlayableDirector cutsceneTimeline;
    private bool triggered = false;

    public string nextSceneName = "Cutscene Dipukul Dontol";

    public void TriggerCutscene()
    {
        if (triggered) return;
        triggered = true;

        cutsceneTimeline.stopped += OnCutsceneFinished;
        cutsceneTimeline.Play();
    }

    private void OnCutsceneFinished(PlayableDirector director)
    {
        cutsceneTimeline.stopped -= OnCutsceneFinished;

        SceneManager.LoadScene(nextSceneName);
    }
}
