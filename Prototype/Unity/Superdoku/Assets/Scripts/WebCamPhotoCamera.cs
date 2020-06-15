﻿using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static System.Environment;

// Create task extension to use async Task in Coroutine
// @see http://www.stevevermeulen.com/index.php/2017/09/using-async-await-in-unity3d-2017/
public static class TaskExtensions
{
    public static IEnumerator AsIEnumerator(this Task task)
    {
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.IsFaulted)
        {
            throw task.Exception;
        }
    }
}

namespace Superdoku
{
    public class WebCamPhotoCamera : MonoBehaviour
    {
        public GameObject btnBack;
        public GameObject btnSnap;
        
        //public static SpecialFolder BASE_OUT_DIR = SpecialFolder.LocalApplicationData;
        //public string OUTPUT_DIR = BASE_OUT_DIR + "/Superdoku";
        private WebCamTexture webCamTexture;

        private bool m_snapped = false;

        void Start()
        {
            // Get the name of the front-facing camera
            //string frontCamName = null;
            //WebCamDevice[] webCamDevices = WebCamTexture.devices;
            //foreach (WebCamDevice camDevice in webCamDevices)
            //{
            //    if (camDevice.isFrontFacing)
            //    {
            //        frontCamName = camDevice.name;
            //        break;
            //    }
            //}

            //if (GameManager.DEBUG_MODE) { Debug.Log("Front Facing Camera: " + frontCamName); }

            //webCamTexture = new WebCamTexture(frontCamName);

            webCamTexture = new WebCamTexture();
            webCamTexture.Play();
        }

        
        private void Update()
        {
            // Don't proceed if the cam is in an image snapped state
            if (m_snapped)
            {
                return;
            }

            // Don't proceed if the webcam texture hasn't finished getting metadata
            if (webCamTexture.width < 100)
            {
                if (GameManager.DEBUG_MODE) { Debug.Log("Waiting for correct webcam texture info..."); }
                return;
            }

            // @see https://answers.unity.com/questions/773464/webcamtexture-correct-resolution-and-ratio.html
            RawImage rawImage = GetComponent<RawImage>();

            // change as user rotates iPhone or Android:

            int cwNeeded = webCamTexture.videoRotationAngle;
            // Unity helpfully returns the _clockwise_ twist needed
            // guess nobody at Unity noticed their product works in counterclockwise:
            int ccwNeeded = -cwNeeded;

            // IF the image needs to be mirrored, it seems that it
            // ALSO needs to be spun. Strange: but true.
            if (webCamTexture.videoVerticallyMirrored) ccwNeeded += 180;

            // you'll be using a UI RawImage, so simply spin the RectTransform
            RectTransform rawImageRT = rawImage.GetComponent<RectTransform>();
            rawImageRT.localEulerAngles = new Vector3(0f, 0f, ccwNeeded);

            float videoRatio = (float)webCamTexture.width / (float)webCamTexture.height;

            // you'll be using an AspectRatioFitter on the Image, so simply set it
            AspectRatioFitter rawImageARF = rawImage.GetComponent<AspectRatioFitter>();
            rawImageARF.aspectRatio = videoRatio;

            // alert, the ONLY way to mirror a RAW image, is, the uvRect.
            // changing the scale is completely broken.
            if (webCamTexture.videoVerticallyMirrored)
                rawImage.uvRect = new Rect(1, 0, -1, 1);  // means flip on vertical axis
            else
                rawImage.uvRect = new Rect(0, 0, 1, 1);  // means no flip

            // Finally, set the raw image texture to the modified webCamTexture to render each frame
            rawImage.texture = webCamTexture;
        }

        public void OnSnap()
        {
            m_snapped = true;
            ToggleAnimation(m_snapped);
            if (webCamTexture.isPlaying)
            {
                webCamTexture.Pause();
            }

            // Call coroutine to process image
            StartCoroutine(ProcessImageCoroutine());

            // Call coroutine to take a photo
            //StartCoroutine("TakePhoto");
        }

        private void ToggleAnimation(bool b)
        {
            Animator btnBackAnimator = btnBack.GetComponent<Animator>();
            btnBackAnimator.SetTrigger(b ? "ScanStart" : "ScanFinish");

            Animator btnSnapAnimator = btnSnap.GetComponent<Animator>();
            btnSnapAnimator.SetTrigger(b ? "ScanStart" : "ScanFinish");
        }

        public void OnBack()
        {
            // Important so that the webcam doesn't keep running until Unity is restarted
            webCamTexture.Stop();

            // Change to home scene
            SceneManager.LoadScene(GameManager.HOME_SCENE);
        }

        private async Task ProcessImageAsync(WebCamTexture wct)
        {
            //await Task.Delay(5000);
            await Task.Run(() =>
            {
               // TODO Invoke RestRequest PUT request to recognize snapped image of Sudoku puzzle


                //int count = 0;
                //while (count < 1000000000)
                //{
                //    count++;
                //}
            });
        }
        
        public IEnumerator ProcessImageCoroutine()
        {
            // Keep track of processing time
            Debug.Log("Began processing image at timestamp: " + Time.time);

            yield return ProcessImageAsync(webCamTexture).AsIEnumerator();

            //yield return new WaitForEndOfImageProcessing(webCamTexture);

            //yield return new WaitForEndOfFrame();

            // yield on a new YieldInstruction that waits for n seconds (Debugging).
            //yield return new WaitForSeconds(5);

            // Return back to original state to test animations
            m_snapped = false;
            ToggleAnimation(m_snapped);

            // Play the webcam again
            if (!webCamTexture.isPlaying)
            {
                webCamTexture.Play();
            }

            // After we have waited 5 seconds print the time again.
            Debug.Log("Finished processing at timestamp: " + Time.time);
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
            string outputDir = Application.persistentDataPath;
            string outFileName = "superdoku_snap.png";
            if (GameManager.WriteFile(outputDir, outFileName, bytes, FileMode.Create))
            {
                // Save output file to player prefs so it can be referenced in image processor scene
                PlayerPrefs.SetString(GameManager.IMAGE_PATH_KEY, outputDir + "/" + outFileName);

                if (GameManager.DEBUG_MODE)
                {
                    Debug.Log("Successfully wrote to " + outputDir + outFileName);
                }

                // Important so that the webcam doesn't keep running until Unity is restarted
                webCamTexture.Stop();

                // Navigate to the Image Processor scene
                SceneManager.LoadScene(GameManager.IMAGE_PROCESSOR_SCENE);
            }
        }
    }
}
