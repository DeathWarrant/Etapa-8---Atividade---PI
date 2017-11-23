using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class APIClient : MonoBehaviour
{
    public string baseUrl = null;

    [HideInInspector] public Player[] players = null;


    private string tempName = null;
    private int tempPoints = 0;
    void Start()
    {

    }

    public void GetLeaderboard()
    {
        StartCoroutine("GetPlayersAPISync");
    }

    public void PostOnLeaderBoard(string p_name, int p_points)
    {
        tempName = p_name;
        tempPoints = p_points;
        StartCoroutine("PostItemApiSync");
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

        /*for (int i = 0; i < players.Length; i++)
        {
            PrintPlayer(players[i]);
        }*/
    }

    IEnumerator PostItemApiSync()
    {
        WWWForm form = new WWWForm();

        form.AddField("Name", tempName);
        form.AddField("Points", tempPoints);

        using (UnityWebRequest request = UnityWebRequest.Post(baseUrl + "/Players/Create", form))
        {
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);
            }
            else
            {
                Debug.Log("Envio do item efetudado com sucesso");
            }
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
