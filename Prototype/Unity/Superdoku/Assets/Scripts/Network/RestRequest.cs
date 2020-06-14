using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Superdoku
{
    public class RestRequest : MonoBehaviour
    {
        private const string BASE_URL = "http://localhost:5000/superdoku-api/{0}";

        public string SendRequest(string url, string method)
        {
            Debug.Log("URL: " + url);
            Debug.Log("Method: " + method);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            return reader.ReadToEnd();
        }

        public void Ping()
        {
            // Create ping request to test server state
            string pingUrl = string.Format(BASE_URL, "ping");
            string response = SendRequest(pingUrl, "GET");

            Debug.Log(response);
        }

        public void Test()
        {
            // Test getting sample sudoku response from server
            string response = SendRequest(string.Format(BASE_URL, "test"), "GET");
            Debug.Log("Original Repsonse: " + response);

            // Set puzzle grid and navigate to home screen
            PlayerPrefs.SetString("ScannedPuzzle", response);
            SceneManager.LoadScene(GameManager.HOME_SCENE);
        }
    }
}
