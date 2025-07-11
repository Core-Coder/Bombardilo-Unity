using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class ChangeSceneAfterTimeline : MonoBehaviour
{
    public PlayableDirector director;
    public string nextSceneName = "Scene 2 ( inside House )";

    void Start()
    {
        if (director != null)
            director.stopped += OnTimelineFinished;
    }

    void OnTimelineFinished(PlayableDirector pd)
    {
        if (pd == director)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
