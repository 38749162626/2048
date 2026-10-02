using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource win;
    public AudioSource merge;
    public AudioSource move;
    
    private void Awake()
    {
        instance = this;
    }
    
    public void PlayWinMusic()
    {
        win.Play();
    }

    public void PlayMergeMusic()
    {
        merge.Stop();
        move.Stop();
        merge.pitch = Random.Range(0.9f, 1.1f);
        merge.Play();
    }

    public void PlayMoveMusic()
    {
        move.Stop();
        move.pitch = Random.Range(0.9f, 1.1f);
        move.Play();
    }
}