using UnityEngine;
using TMPro;

public class UIFPS : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textValue;
    [SerializeField] private float updateInterval = 0.5f;

    private int frameCounter = 0;
    private float timeElapsed = 0.0f;

    static private UIFPS instance = null;
    static public void Initialize()
    {
        if (instance != null) 
            return;

        var goPrefab = Resources.Load<GameObject>("UIFPS");
        if (goPrefab == null) 
            return;

        var go = Instantiate(goPrefab);
        instance = go.GetComponent<UIFPS>();
        DontDestroyOnLoad(go);
        go.SetActive(true);
    }

    private void Update()
    {
        frameCounter++;
        timeElapsed += Time.unscaledDeltaTime;

        if (timeElapsed < updateInterval) 
            return;

        int fps = (int)Mathf.Round((float)frameCounter / timeElapsed);
        textValue.text = fps.ToString();

        frameCounter = 0;
        timeElapsed = 0;
    }
}
