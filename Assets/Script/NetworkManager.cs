using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public InputField NicknameInput;
    public GameObject DisconnectPanel;
    public GameObject RespawnPanel;

    private bool test;

    private void Awake()
    {
        Screen.SetResolution(960, 540, false); // screen set 
        PhotonNetwork.SendRate = 60; // 클라이언트의 전송률 
        PhotonNetwork.SerializationRate = 30; // 메서드 호출 빈도수 
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(0.2f);
        foreach (GameObject Go in GameObject.FindGameObjectsWithTag("Bullet"))
        {
            Go.GetComponent<PhotonView>().RPC("DestroyRPC", RpcTarget.All);
        }
    }

    public void Connect() => PhotonNetwork.ConnectUsingSettings();



    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = NicknameInput.text;
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 6 }, null);
    }



    public override void OnJoinedRoom()
    {
        DisconnectPanel.SetActive(false);
        StartCoroutine("DestroyBullet");
        Spawn();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        DisconnectPanel.SetActive(true);
        RespawnPanel.SetActive(false);
    }

    public void Spawn()
    {
        PhotonNetwork.Instantiate("Player",new Vector3(Random.Range(-2.5f , -4f) ,Random.Range(0.5f,6f), 0), Quaternion.identity);
        RespawnPanel.SetActive(false);
    }
}
