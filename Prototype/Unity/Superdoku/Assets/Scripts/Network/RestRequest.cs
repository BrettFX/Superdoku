using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Superdoku
{
    public class RequestContent
    {
        public RequestContent()
        {
            data = null;
            filetype = "";
        }

        public RequestContent(byte[] data, string filetype)
        {
            this.data = data;
            this.filetype = filetype;
        }

        public byte[] data;
        public string filetype;
    }

    public class RestRequest : MonoBehaviour
    {
        // Static Refs //
        private static RestRequest instance;
        public static RestRequest Instance
        {
            get
            {
                return instance;
            }
        }

        public GameObject testImage;
        public const string BASE_URL = "http://192.168.0.238:8080/SuperdokuAPI/{0}";
        public const int TIMEOUT = 60; // in seconds

        /**
         * Ensure this class remains a singleton instance
         * */
        void Awake()
        {
            // If the instance variable is already assigned...
            if (instance != null)
            {
                // If the instance is currently active...
                if (instance.gameObject.activeInHierarchy == true)
                {
                    // Warn the user that there are multiple Game Managers within the scene and destroy the old manager.
                    Debug.LogWarning("There are multiple instances of the RestRequest script. Removing the old instance from the scene.");
                    Destroy(instance.gameObject);
                }

                // Remove the old manager.
                instance = null;
            }

            // Assign the instance variable as the Game Manager script on this object.
            instance = GetComponent<RestRequest>();
        }

        public string SendRequest(string url, string method, RequestContent content)
        {
            Debug.Log("URL: " + url);
            Debug.Log("Method: " + method);

            // Get response info from respective rest request
            string responseStr = "";

            // Determine how to process request based on HTTP method type
            switch (method)
            {
                case "GET":
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    responseStr = reader.ReadToEnd();
                    break;
                case "PUT":
                    // Use UnityWebRequest to send post request with data in request body
                    // See: https://docs.unity3d.com/Manual/UnityWebRequest-UploadingRawData.html
                    StartCoroutine(Upload(url, content));
                    break;
            }

            return responseStr;
        }

        public void Ping()
        {
            // Create ping request to test server state
            string pingUrl = string.Format(BASE_URL, "ping");
            string response = SendRequest(pingUrl, "GET", null);

            Debug.Log(response);
        }

        public void Test()
        {
            // Test getting sample sudoku response from server
            string response = SendRequest(string.Format(BASE_URL, "test"), "GET", null);
            Debug.Log("Original Repsonse: " + response);

            // Set puzzle grid and navigate to home screen
            PlayerPrefs.SetString("ScannedPuzzle", response);
            SceneManager.LoadScene(GameManager.HOME_SCENE);
        }

        public void RecognizeTest()
        {
            RawImage raw = testImage.GetComponent<RawImage>();
            Texture2D img = (Texture2D)raw.texture;
            RequestContent content = new RequestContent(img.EncodeToPNG(), "sample.png");
            SendRequest(string.Format(BASE_URL, "recognize"), "PUT", content);
        }

        IEnumerator Upload(string url, RequestContent content)
        {
            
            UnityWebRequest request = UnityWebRequest.Put(url, content.data);
            request.SetRequestHeader("Content-Type", "application/octet-stream");
            request.SetRequestHeader("Content-Disposition", "attachment; filetype=\"" + content.filetype + "\"");
            request.timeout = TIMEOUT;
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);

                // Set error flag in player prefs so that an error message can be displayed to the user in the main scene
                PlayerPrefs.SetString("RestRequestError", request.error);
                SceneManager.LoadScene(GameManager.HOME_SCENE);
            }
            else
            {
                Debug.Log("Form upload complete!");

                // Get Response information
                StringBuilder sb = new StringBuilder();
                foreach (KeyValuePair<string, string> dict in request.GetResponseHeaders())
                {
                    sb.Append(dict.Key).Append(": \t[").Append(dict.Value).Append("]\n");
                }

                // Print Headers
                Debug.Log(sb.ToString());

                // Print Body
                Debug.Log(request.downloadHandler.text);

                // Set puzzle grid and navigate to home screen
                PlayerPrefs.SetString("ScannedPuzzle", request.downloadHandler.text);
                SceneManager.LoadScene(GameManager.HOME_SCENE);
            }
        }
    }
}
