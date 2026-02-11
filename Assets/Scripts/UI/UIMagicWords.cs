using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

// Define the classes that will hold the json data
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
    [SerializeField] private GameObject dialogueLeftPrefab;
    [SerializeField] private GameObject dialogueRightPrefab;

    private List<Texture2D> loadedTextures = new();
    private DialogueModel model;
    private Regex emojiRegex;

    private void Start()
    {
        emojiRegex = new Regex(@"{(\w+)}", RegexOptions.Compiled);
        _ = Initialize();
    }

    private void OnDestroy()
    {
        foreach (var avatar in model.avatars)
        {
            if (avatar.sprite == null) continue;
            Destroy(avatar.sprite);
            avatar.sprite = null;
        }

        foreach (var texture in loadedTextures)
        {
            if (texture == null) continue;
            Destroy(texture);
        }
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
                loadedTextures.Add(texture);
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
            if (avatarInfo.position.Equals("left", StringComparison.OrdinalIgnoreCase))
            {
                go = Instantiate(dialogueLeftPrefab);
            }
            else
            {
                go = Instantiate(dialogueRightPrefab);
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
        return model.avatars.Find((a) => a.sprite != null && a.name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private void SetStatusText(string status)
    {
        textStatus.text = status;
    }

    private void ProcessDialogueText()
    {
        foreach (var dialogue in model.dialogue)
        {
            dialogue.text = emojiRegex.Replace(dialogue.text, match =>
            {
                string key = match.Groups[1].Value;
                return $"<sprite name=\"{key}\">";
            });
        }
    }
}
