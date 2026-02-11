using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

[System.Serializable]
public class DialogueInfo
{
    public string name;
    public string text;
}

[System.Serializable]
public class AvatarInfo
{
    public string name;
    public string url;
    public string position;

}

[System.Serializable]
public class DialogueModel
{
    public DialogueInfo[] dialogue;
    public AvatarInfo[] avatars;
}

public class UIMagicWords : UIBase
{
    [SerializeField] private string url;

    private DialogueModel model;

    private void Start()
    {
        StartCoroutine(LoadDataFromUrl());
    }

    private IEnumerator LoadDataFromUrl()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log("Received: " + webRequest.downloadHandler.text);

                    model = JsonUtility.FromJson<DialogueModel>(webRequest.downloadHandler.text);
                    break;
            }
        }

        if (model == null)
            yield break;

        foreach (var d in model.dialogue)
        {
            Debug.Log($"{d.name} - {d.text}");
        }

        foreach (var a in model.avatars)
        {
            Debug.Log($"{a.name} - {a.position} - {a.url}");
        }
    }
}
