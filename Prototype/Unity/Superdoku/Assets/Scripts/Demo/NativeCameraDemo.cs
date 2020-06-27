using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Superdoku
{
    public class NativeCameraDemo : MonoBehaviour
    {

        void Update()
        {
            /*
             The following code has two functions:

                if you click left half of the screen, the camera is opened and after a picture is taken, 
                it is displayed on a temporary quad that is placed in front of the camera.

                if you click right half of the screen, the camera is opened and after a video is recorded,
                it is played using the Handheld.PlayFullScreenMovie function.

             */
            //if (Input.GetMouseButtonDown(0))
            //{
            //    // Don't attempt to use the camera if it is already open
            //    if (NativeCamera.IsCameraBusy())
            //        return;

            //    if (Input.mousePosition.x < Screen.width / 2)
            //    {
            //        // Take a picture with the camera
            //        // If the captured image's width and/or height is greater than 512px, down-scale it
            //        TakePicture(512);
            //    }
            //    else
            //    {
            //        // Record a video with the camera
            //        RecordVideo();
            //    }
            //}
        }

        public void OnPicture()
        {
            // Don't attempt to use the camera if it is already open
            if (NativeCamera.IsCameraBusy())
                return;

            // Take a picture with the camera
            // Don't enforce any limit on the size of the image to ensure the best image quality
            TakePicture(int.MaxValue);
        }

        public void OnVideo()
        {
            // Don't attempt to use the camera if it is already open
            if (NativeCamera.IsCameraBusy())
                return;

            // Record a video with the camera
            RecordVideo();
        }

        public void OnBack()
        {
            // Change to home scene
            SceneManager.LoadScene(GameManager.HOME_SCENE);
        }

        public void OnLog()
        {
            Debug.Log("Testing log from Superdoku application");
        }

        private void TakePicture(int maxSize)
        {
            NativeCamera.Permission permission = NativeCamera.TakePicture((path) =>
            {
                Debug.Log("Image path: " + path);
                if (path != null)
                {
                    PlayerPrefs.SetString(GameManager.IMAGE_PATH_KEY, path);
                    SceneManager.LoadScene(GameManager.WEB_CAM_SCENE);
                }
            }, maxSize);

            Debug.Log("Permission result: " + permission);
        }

        private void RecordVideo()
        {
            NativeCamera.Permission permission = NativeCamera.RecordVideo((path) =>
            {
                Debug.Log("Video path: " + path);
                if (path != null)
                {
                    // Play the recorded video
                    Handheld.PlayFullScreenMovie("file://" + path);
                }
            });

            Debug.Log("Permission result: " + permission);
        }
    }
}
