using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardCreator : MonoBehaviour
{
    //[SerializeField] private Camera camera;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI text;

    [SerializeField] private int maxSprites = 144;
    [SerializeField] private Gradient gradient;

    [SerializeField] private Vector2Int imageSize = new Vector2Int(170, 250);
    [SerializeField] private string directory = "Sprites/Cards";

    private IEnumerator Start()
    {
        Texture2D screenshotTex = new Texture2D(imageSize.x, imageSize.y, TextureFormat.RGBA32, false);
        Vector2 position = new Vector2((renderTexture.width - imageSize.x) / 2.0f, (renderTexture.height - imageSize.y) / 2.0f);
        Rect screenshotRect = new Rect(position, imageSize);

        string path = $"{Application.dataPath}/{directory}";
        if (Path.DirectorySeparatorChar != '/')
        {
            path = path.Replace('/', Path.DirectorySeparatorChar);
        }


        for (int i=0; i<maxSprites; i++)
        {
            background.color = gradient.Evaluate((float)i / (float)maxSprites);
            text.text = (i+1).ToString();

            yield return new WaitForEndOfFrame();

            RenderTexture.active = renderTexture;
            screenshotTex.ReadPixels(screenshotRect, 0, 0, false);
            screenshotTex.Apply();

            string filename = Path.Combine(path, $"card-{i:000}.png");
            File.WriteAllBytes(filename, ImageConversion.EncodeToPNG(screenshotTex));

            Debug.Log($"Saved {filename}");
        }

        // Clean up
        RenderTexture.active = null;
        Object.Destroy(screenshotTex);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
