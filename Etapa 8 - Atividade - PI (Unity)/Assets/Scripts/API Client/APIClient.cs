using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class APIClient : MonoBehaviour
{
    [HideInInspector] public Player[] players = null;

    public string baseUrl = null;

    void Start()
    {
        StartCoroutine("GetPlayersAPISync");
    }

    private IEnumerator GetPlayersAPISync()
    {
        UnityWebRequest request = UnityWebRequest.Get(baseUrl + "/Players");

        yield return request.SendWebRequest();

        if(request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }

        string response = request.downloadHandler.text;
        Debug.Log(response);

        players = JSonHelper.getJsonArray<Player>(response);

        foreach (Player p in players)
        {
            PrintPlayer(p);
        }
    }

    private void PrintPlayer(Player p)
    {
        Debug.Log(" ========== Player Data ========== ");
        Debug.Log("ID: " + p.PlayerID);
        Debug.Log("Name: " + p.Name);
        Debug.Log("Points: " + p.Points);
    }
}
