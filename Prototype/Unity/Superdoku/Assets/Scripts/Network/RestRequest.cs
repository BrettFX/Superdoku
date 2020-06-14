using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Superdoku
{
    public class RestRequest : MonoBehaviour
    {
        private const string BASE_URL = "http://localhost:5000/superdoku-api/{0}";

        public string SendRequest(string url, string method, Object data)
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
                case "POST":
                    // TODO Use UnityWebRequest to send post request with data in request body
                    // See: https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.Post.html
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

        public void RecognizeTest(Texture2D image)
        {
            string response = SendRequest(string.Format(BASE_URL, "recognize"), "POST", image);
            Debug.Log("Recognize Test Response: " + response);
        }
    }
}
