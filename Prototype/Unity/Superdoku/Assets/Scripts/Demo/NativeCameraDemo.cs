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
                    // Create a Texture2D from the captured image
                    //Texture2D texture = NativeCamera.LoadImageAtPath(path, maxSize);
                    // Need to load PNG with game manager implementation to prevent Unity texture not readable exception
                    // (e.g., texture memory can't be accessed from script if using NativeCamera.LoadImageAtPath)
                    Texture2D texture = GameManager.LoadImage(path);
                    if (texture == null)
                    {
                        Debug.Log("Couldn't load texture from " + path);
                        return;
                    }

                    // Need to rotate the picture
                    Texture2D rotatedTexture = GameManager.RotateTexture(texture, true);

                    // Encode captured image texture to png to get the byte array data
                    byte[] data = rotatedTexture.EncodeToPNG();

                    // Invoke RestRequest PUT request to recognize snapped image of Sudoku puzzle
                    // TODO perform same preprocessing on jpg images with GameManager to ensure proper image orientation
                    RequestContent content = new RequestContent(data, "jpg");
                    RestRequest.Instance.SendRequest(string.Format(RestRequest.BASE_URL, "recognize"), "PUT", content);

                    // Assign texture to a temporary quad and destroy it after 5 seconds
                    //GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    //quad.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2.5f;
                    //quad.transform.forward = Camera.main.transform.forward;
                    //quad.transform.localScale = new Vector3(1f, texture.height / (float)texture.width, 1f);

                    //Material material = quad.GetComponent<Renderer>().material;
                    //if (!material.shader.isSupported) // happens when Standard shader is not included in the build
                    //    material.shader = Shader.Find("Legacy Shaders/Diffuse");

                    //material.mainTexture = texture;

                    //Destroy(quad, 5f);

                    // If a procedural texture is not destroyed manually, 
                    // it will only be freed after a scene change
                    //Destroy(texture, 5f);
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
