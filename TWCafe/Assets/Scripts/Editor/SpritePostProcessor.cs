using UnityEditor;
using UnityEngine;

public class SpritePostProcessor : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        var lowerCaseAssetPath = assetPath.ToLower();
        lowerCaseAssetPath = lowerCaseAssetPath.Replace("\\", "/");

        var inSpriteDirectory = lowerCaseAssetPath.Contains("assets/textures/");
        if(!inSpriteDirectory)
            return;
        var textureImporter = assetImporter as TextureImporter;
        if (textureImporter != null)
        {
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            textureImporter.filterMode = FilterMode.Point;
        }
    }
}
