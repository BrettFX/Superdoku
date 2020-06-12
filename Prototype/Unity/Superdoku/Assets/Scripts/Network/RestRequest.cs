using System.IO;
using System.Net;
using UnityEngine;

public class RestRequest : MonoBehaviour
{
    public void Ping()
    {
        // string.Format("http://api.openweathermap.org/data/2.5/weather?id={0}&APPID={1}", CityId, API_KEY)
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:8080/ytc-api/converter/ping");
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());
        string jsonResponse = reader.ReadToEnd();

        Debug.Log(jsonResponse);
        //WeatherInfo info = JsonUtility.FromJson<WeatherInfo>(jsonResponse);
        //return info;
    }
}
