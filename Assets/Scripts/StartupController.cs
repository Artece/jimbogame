using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StartupController : MonoBehaviour
{
    public float logoDur = 5f;

    private Queue<FileInfo> videoList;

    private Canvas canvas;

    // video player panel, menu background panel, logo panel
    private GameObject vppanel, mbgpanel, logopanel;



   private IEnumerator LogoAnimation(float dur)
    {
        yield return new WaitForSeconds(dur/4);

        CanvasGroup lpc = logopanel.GetComponent<CanvasGroup>();
        float t = 0f;

        while (t < dur)
        {
            t += Time.deltaTime;
            lpc.alpha = Mathf.Sin(3.14f * t / dur);
            yield return null;
        }
        
        logopanel.SetActive(false);

        mbgpanel.SetActive(true);
        CanvasGroup mpc = mbgpanel.GetComponent<CanvasGroup>();
        
        t = 0f;
        dur /= 2;
        while (t < dur)
        {
            t += Time.deltaTime;
            mpc.alpha = Mathf.Sin(1.57f * t / dur);
            yield return null;
        }
    }

    private void ParseVids()
    {
        DirectoryInfo dir = new DirectoryInfo("./Assets/VideoPlayer");
        var fia = dir.GetFiles("*.m4v");
        videoList = new Queue<FileInfo>();
        foreach (FileInfo fi in fia)
        {
            videoList.Enqueue(fi);
        }
    }

    public void NextVid()
    {
        VideoPlayer vp = vppanel.GetComponent<VideoPlayer>();
        var fi = videoList.Dequeue();
        videoList.Enqueue(fi);
        vp.url = fi.FullName;
    }

    public void Quit()
    {
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Start is called before the first frame update
    void Start()
    {
        //canvas = GetComponent<Canvas>();
        vppanel = transform.Find("video player panel").gameObject;
        mbgpanel = transform.Find("menu bg panel").gameObject;
        logopanel = transform.Find("logo panel").gameObject;
        StartCoroutine(LogoAnimation(logoDur));
        ParseVids();
        NextVid();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
