using UnityEngine;

public class CustomImageSaver : MonoBehaviour
{
    public void OpenGallery()
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery(OnImageSelected, "Select Your Image");
        if (permission == NativeGallery.Permission.Denied)
        {
            Debug.LogError("Permission is Denied");
        }
    }

    public void OpenCamera()
    {
        NativeCamera.TakePicture(OnImageSelected, 2048, false, NativeCamera.PreferredCamera.Rear);
    }

    private void OnImageSelected(string path)
    {
        Texture2D result = NativeGallery.LoadImageAtPath(path, 1024, true, false);
        if (result != null)
        {
            ImageCropper.Settings cropSettings = new ImageCropper.Settings
            {   
                selectionMinAspectRatio = 1,
                selectionMaxAspectRatio = 1,
                markTextureNonReadable = false
            };
            ImageCropper.Instance.Show(result,OnImageCropped,cropSettings);
        }
    }

    private void OnImageCropped(bool result, Texture originalImage, Texture2D croppedImage)
    {
        if (!result) return;
        byte[] textureBytesData = croppedImage.EncodeToPNG();

        int count = StorageManager.GetFilesInDirectory(StringID.CustomTextureFolder).Length + 1;

        string savePath = $"{StringID.CustomTextureFolder}/{StringID.TextureID + count.ToString().PadLeft(4, '0')}.png";
        StorageManager.Write(savePath, textureBytesData);
        UIManager.Instance.AddCustomTexturePath(savePath,croppedImage);
    }
}
