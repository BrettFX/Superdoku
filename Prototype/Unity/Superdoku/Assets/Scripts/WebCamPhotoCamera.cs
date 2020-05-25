﻿using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static System.Environment;

namespace Superdoku
{
    public class WebCamPhotoCamera : MonoBehaviour
    {
        public static SpecialFolder BASE_OUT_DIR = SpecialFolder.LocalApplicationData;
        public string OUTPUT_DIR = GetFolderPath(BASE_OUT_DIR) + "/Superdoku";
        WebCamTexture webCamTexture;

        void Start()
        {
            webCamTexture = new WebCamTexture();
            webCamTexture.Play();
        }

        private void Update()
        {
            GetComponent<RawImage>().texture = webCamTexture;
        }

        public void OnSnap()
        {
            // Call coroutine to take a photo
            StartCoroutine("TakePhoto");
        }

        public void OnBack()
        {
            // Important so that the webcam doesn't keep running until Unity is restarted
            webCamTexture.Stop();

            // Change to home scene
            SceneManager.LoadScene(GameManager.HOME_SCENE);
        }

        /**
         * Capture the current texture frame and convert it to a png file to be
         * written to the file system.
         * 
         * @see https://stackoverflow.com/questions/24496438/can-i-take-a-photo-in-unity-using-the-devices-camera
         * @see http://docs.unity3d.com/ScriptReference/WaitForEndOfFrame.html
         */
        public IEnumerator TakePhoto()  // Start this Coroutine on some button click
        {
            // Required
            yield return new WaitForEndOfFrame();

            Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height);
            photo.SetPixels(webCamTexture.GetPixels());
            photo.Apply();

            // Encode to a PNG
            byte[] bytes = photo.EncodeToPNG();

            // Write out the PNG.
            long time = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
            string testFile = "/test_" + time + ".png";
            if (GameManager.WriteFile(OUTPUT_DIR, testFile, bytes, FileMode.Create))
            {
                // Save output file to player prefs so it can be referenced in image processor scene
                PlayerPrefs.SetString(GameManager.IMAGE_PATH_KEY, OUTPUT_DIR + testFile);

                if (GameManager.DEBUG_MODE)
                {
                    Debug.Log("Successfully wrote to " + OUTPUT_DIR + testFile);
                }
            }
        }
    }
}
