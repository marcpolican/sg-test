using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
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

    public Sprite sprite;
}

[System.Serializable]
public class DialogueModel
{
    public List<DialogueInfo> dialogue;
    public List<AvatarInfo> avatars;
}

public class UIMagicWords : UIBase
{
    [SerializeField] private string urlData = "https://private-624120-softgamesassignment.apiary-mock.com/v3/magicwords";
    [SerializeField] private TextMeshProUGUI textStatus;
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject dialogueLeft;
    [SerializeField] private GameObject dialogueRight;

    private DialogueModel model;

    private void Start()
    {
        Initialize();
    }

    private async Awaitable Initialize()
    {
        SetStatusText("Downloading data");
        contentParent.DestroyChildren();

        await LoadDataFromUrl();

        if (model == null || model.dialogue == null || model.dialogue.Count <= 0)
        {
            SetStatusText("No Data");
            return;
        }

        ProcessDialogueText();

        SetStatusText("Fetching Avatars");
        List<Awaitable> tasks = new();
        foreach (var avatar in model.avatars)
        {
            tasks.Add(LoadAvatarSprite(avatar));
        }

        bool isDone = false;
        float timeout = 15.0f;
        while (timeout > 0.0f && !isDone)
        {
            isDone = true;
            foreach (var task in tasks)
            {
                if (!task.IsCompleted)
                {
                    isDone = false;
                    break;
                }
            }
            timeout -= Time.deltaTime;
            await Awaitable.NextFrameAsync();
        }

        SetStatusText("");
        FillDialogueList();
    }

    private async Awaitable LoadDataFromUrl()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(urlData))
        {
            // Request and wait for the desired page.
            await webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                model = JsonUtility.FromJson<DialogueModel>(webRequest.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error: " + webRequest.error);
            }
        }
    }

    private async Awaitable LoadAvatarSprite(AvatarInfo avatarInfo)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(avatarInfo.url))
        {
            webRequest.timeout = 10; // seconds
            await webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // Create sprite from the texture
                var texture = DownloadHandlerTexture.GetContent(webRequest);
                avatarInfo.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                Debug.Log($"Created sprite from: {avatarInfo.url}");
            }
            else
            {
                Debug.LogWarning($"Failed to download texture from: {avatarInfo.url}");
            }
        }
    }

    private void FillDialogueList()
    {
        foreach (var dialogue in model.dialogue)
        {
            var avatarInfo = GetValidAvatarInfo(dialogue.name);
            if (avatarInfo == null) continue;

            GameObject go;
            if (string.Compare(avatarInfo.position, "left") == 0)
            {
                go = Instantiate(dialogueLeft);
            }
            else
            {
                go = Instantiate(dialogueRight);
            }
            
            go.transform.SetParent(contentParent);
            go.transform.SetAsLastSibling();
            go.transform.ResetTransformation();

            var dialoguePanel = go.GetComponent<UIDialoguePanel>();
            if (dialoguePanel == null) continue;

            dialoguePanel.SetData(dialogue, avatarInfo);
        }
    }

    private AvatarInfo GetValidAvatarInfo(string name)
    {
        return model.avatars.Find((a) => a.sprite != null && string.Compare(a.name, name, true) == 0);
    }

    private void SetStatusText(string status)
    {
        textStatus.text = status;
    }

    private void ProcessDialogueText()
    {
        Regex regex = new Regex(@"{(\w+)}", RegexOptions.Compiled);
        foreach (var dialogue in model.dialogue)
        {
            dialogue.text = regex.Replace(dialogue.text, match =>
            {
                string key = match.Groups[1].Value;
                return $"<sprite name=\"{key}\">";
            });
        }
    }
}
