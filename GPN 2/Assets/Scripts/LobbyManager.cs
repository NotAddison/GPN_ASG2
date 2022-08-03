using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject createInput;
    [SerializeField] GameObject joinInput;
    [SerializeField] private byte maxPlayers = 4;

    void Start(){
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    public void CreateRoom()
    {
        string code = createInput.GetComponent<TMP_InputField>().text;
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = maxPlayers;
        PhotonNetwork.CreateRoom(code, roomOptions);
        Debug.LogError($"[Photon] Creating Room: {code} | Max Players: {roomOptions.MaxPlayers} | Server: {PhotonNetwork.CloudRegion}");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[Photon]: Room creation failed: {message}");
    }

    public void JoinRoom()
    {
        string code = joinInput.GetComponent<TMP_InputField>().text;
        // Debug.LogError($"[Photon] Joining Room: {code} | Server: {PhotonNetwork.CloudRegion}");
        Debug.LogError($"[Photon] AutomaticallySyncScene Status: {PhotonNetwork.AutomaticallySyncScene}");
        PhotonNetwork.JoinRoom(code);
    }

    public override void OnJoinedRoom()
    {
        Debug.LogError($"[Photon] Joined Room: {PhotonNetwork.CurrentRoom.Name} | Server: {PhotonNetwork.CloudRegion}");
        Debug.LogError($"[Photon] Players in Room: {PhotonNetwork.CurrentRoom.PlayerCount}");

        // PhotonNetwork.LoadLevel("Test"); << To FIX {}

        // Only for Local Player
        // Debug.LogError($"Player {PhotonNetwork.LocalPlayer.ActorNumber} joined room {PhotonNetwork.CurrentRoom.Name}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[Photon]: Room join failed: {message}");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogError("[Photon]: Player entered room: " + newPlayer.ActorNumber);
    }

    public void StartGame(){
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount > 0){ PhotonNetwork.LoadLevel("Test");} 
            else { Debug.LogError("[Photon]: Not enough players!"); }
        }
        else
        {
            Debug.LogError("[Photon]: Only the Host Client can start the game");
        }
    }
}
