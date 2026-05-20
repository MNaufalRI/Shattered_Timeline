using System.IO;
using UnityEngine;

public class RenderTextureToPNG : MonoBehaviour
{
    public RenderTexture renderTexture;
    public string namaFile = "BaseAxe_Sprite";

    [ContextMenu("Ambil Foto PNG")] // Ini akan memunculkan tombol rahasia di Inspector
    public void SavePNG()
    {
        if (renderTexture == null) return;

        RenderTexture.active = renderTexture;
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        byte[] bytes = texture.EncodeToPNG();
        string path = Application.dataPath + "/" + namaFile + ".png";
        File.WriteAllBytes(path, bytes);

        Debug.Log("<color=lime>[STUDIO]</color> Gambar PNG berhasil dibuat di: " + path);
    }
}