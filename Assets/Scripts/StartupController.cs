using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Models;

public class StartupController : MonoBehaviour
{
    public float introDuration = 2f;

    public Sprite mainMenuBg1, mainMenuBg2, pressAnyKeyScreen;

    private Queue<FileInfo> videoList;

    private List<DirectoryInfo> scenarios;

    private Canvas canvas;

    // video player panel, menu background panel, studio logo panel, scenario select panel
    private GameObject vpp, mbp, slp, ssp;

   private IEnumerator MenuAnimation()
    {
        var mbi = mbp.GetComponent<Image>();
        var q = new Queue<Sprite>();
        q.Enqueue(mainMenuBg1);
        q.Enqueue(mainMenuBg2);
        while (mbp.activeSelf)
        {
            yield return new WaitForSeconds(5f);
            var bg = q.Dequeue();
            mbi.sprite = bg;
            q.Enqueue(bg);
        }
    }

   private IEnumerator IntroAnimation(float dur)
    {
        float hdur = dur * .5f, qdur = dur * .25f, hpi = 1.57f, pi = 3.14f;

        yield return new WaitForSeconds(qdur);

        CanvasGroup lpc = slp.GetComponent<CanvasGroup>();

        for (float t = 0f;  t < hdur; t += Time.deltaTime)
        {
            lpc.alpha = Mathf.Sin(pi * t / hdur);
            yield return null;
        }

        slp.GetComponent<Image>().sprite = pressAnyKeyScreen;

        for (float t = 0f; t < qdur; t += Time.deltaTime)
        {
            lpc.alpha = Mathf.Sin(hpi * t / qdur);
            yield return null;
        }

        yield return new WaitUntil(() => Input.anyKey);

        for (int i = 0; i < 3; i++)
        {
            slp.GetComponentInChildren<Text>().CrossFadeAlpha(0f, 0.05f, false);
            yield return new WaitForSeconds(0.05f);
            slp.GetComponentInChildren<Text>().CrossFadeAlpha(1f, 0.05f, false);
            yield return new WaitForSeconds(0.05f);
        }
        slp.GetComponentInChildren<Text>().CrossFadeAlpha(0f, 0.05f, false);
        yield return new WaitForSeconds(0.05f);

        for (float t = 0f; t < qdur; t += Time.deltaTime)
        {
            lpc.alpha = Mathf.Sin(hpi + (hpi * t / qdur));
            yield return null;
        }

        slp.SetActive(false);
        mbp.SetActive(true);

        CanvasGroup mpc = mbp.GetComponent<CanvasGroup>();

        for (float t = 0f; t < qdur; t += Time.deltaTime)
        {
            mpc.alpha = Mathf.Sin(hpi * t / qdur);
            yield return null;
        }

        StartCoroutine(MenuAnimation());
    }
    private void ParseScenarios()
    {
        DirectoryInfo dir = new DirectoryInfo("./Assets/Scenarios");
        scenarios = new List<DirectoryInfo>(dir.GetDirectories());
    }

    public void SwitchToScenarioSelectScreen()
    {
        mbp.SetActive(false);

        foreach (var s in scenarios)
        {
            
        }
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
        vpp = transform.Find("VPP").gameObject;
        mbp = transform.Find("MBP").gameObject;
        slp = transform.Find("SLP").gameObject;
        ssp = transform.Find("SSP").gameObject;
        StartCoroutine(IntroAnimation(introDuration));
        ParseScenarios();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
