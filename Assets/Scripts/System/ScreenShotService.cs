using System;
using System.IO;
using System.Threading.Tasks;
using Scripts.Helpers;
using Scripts.System.MonoBases;
using UnityEngine;

namespace Scripts.System
{
    public class ScreenShotService : SingletonNotPersisting<ScreenShotService>
    {
        [SerializeField] private int captureWidth = 480;
        [SerializeField] private int captureHeight = 270;
        

        public async Task<string> CaptureScreenshotFile(string positionFileName)
        {
            try
            {
                byte[] bytes = await GetCurrentScreenshotBytes();
                return WriteScreenshotBytesToPng(positionFileName, bytes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        public static string WriteScreenshotBytesToPng(string fileName, byte[] bytes)
        {
            string screenshotFileName = $"{fileName}.png";
            
            File.WriteAllBytes(screenshotFileName, bytes);

            return screenshotFileName;
        }

        public async Task<byte[]> GetCurrentScreenshotBytes()
        {
            Camera targetCamera = CameraManager.Instance.mainCamera;
            RenderTexture renderTexture = new(captureWidth, captureHeight, 24);
            Texture2D screenShot = new(captureWidth, captureHeight, TextureFormat.RGB565, false);
         
            // get main camera and manually render scene into it
            targetCamera.targetTexture = renderTexture;
            
            await AsyncHelpers.WaitForEndOfFrameAsync();
            
            targetCamera.Render();
            RenderTexture.active = renderTexture;
            screenShot.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);

            targetCamera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(renderTexture);

            
            return screenShot.EncodeToPNG();
        }

        public Texture2D LoadScreenshotFromFile(string filePath) 
        {
            if (!File.Exists(filePath)) return null;
 
            byte[] fileData = File.ReadAllBytes(filePath);

            return GetScreenshotTextureFromBytes(fileData);
        }

        public Texture2D GetScreenshotTextureFromBytes(byte[] bytes)
        {
            Texture2D texture = new(captureWidth, captureHeight);
            texture.LoadImage(bytes); //..this will auto-resize the texture dimensions.

            return texture;
        }
        
        public Sprite GetScreenshotSpriteFromBytes(byte[] bytes)
        {
            Texture2D texture = GetScreenshotTextureFromBytes(bytes);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }
    }
}