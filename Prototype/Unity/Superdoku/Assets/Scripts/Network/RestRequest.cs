using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

public class RestRequest : MonoBehaviour
{
    private const string BASE_URL = "http://localhost:5000/superdoku-api/{0}";

    [System.Serializable]
    public class Cell
    {
        public int data;
    }

    [System.Serializable]
    public class SudokuPuzzle
    {
        public List<int> puzzle;
    }

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
        // Parse response to integer array
        // int[] testSudoku = Array.ConvertAll(response.Split(','), int.Parse);
        SudokuPuzzle testSudoku = JsonUtility.FromJson<SudokuPuzzle>(response);

        // Get 2d version of test sudoku
        int bounds = 9;
        int puzzleCount = testSudoku.puzzle.Count;

        Debug.Log("Rows: " + bounds + ", Columns: " + bounds);

        int[,] grid = new int[bounds, bounds];
        int i = 0;
        for (int x = 0; x < 9; x++){
            for (int y = 0; y < 9; y++){
                if (i < puzzleCount)
                {
                    grid[x, y] = testSudoku.puzzle[i];
                }               
                i++;
            }
        }

        // The character to determine formatting
        string display = "";

        for (int row = 0; row < bounds; row++)
        {
            display += "\n";

            for (int col = 0; col < bounds; col++)
            {
                // Print out a divider every third line
                display += ((col % 3 == 0) ? "\t" : " ") + grid[row, col];
            }
            display += ((row + 1) % 3 == 0) ? "\n" : "";
        }

        Debug.Log("\nSudoku Puzzle:");
        Debug.Log(display);        
    }
}
