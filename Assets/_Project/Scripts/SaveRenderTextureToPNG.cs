using System.IO;
using UnityEngine;

public class SaveRenderTextureToPNG : MonoBehaviour
{
    public RenderTexture renderTexture;

    public void SaveTextureToPNG(string filePath)
    {
        // Ensure the render texture is the active render target
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;

        // Create a new Texture2D with the same dimensions as the render texture
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

        // Read the pixels from the RenderTexture and apply them to the Texture2D
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        // Reset the active render texture
        RenderTexture.active = currentRT;

        // Encode the texture to PNG format
        byte[] pngData = texture.EncodeToPNG();

        // Save the PNG to the specified file path
        if (pngData != null)
        {
            File.WriteAllBytes(filePath, pngData);
            Debug.Log("Saved RenderTexture to " + filePath);
        }
        else
        {
            Debug.LogError("Failed to encode RenderTexture to PNG.");
        }

        // Clean up the Texture2D
        Destroy(texture);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
            SaveTextureToPNG(Application.dataPath + "/SavedRenderTexture.png");
    }
}
