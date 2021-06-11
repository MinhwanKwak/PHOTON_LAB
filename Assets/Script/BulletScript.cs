using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class BulletScript : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    int dir;


    private void Start() => Destroy(gameObject, 3.5f);

    private void Update() => transform.Translate(Vector3.right * 7 * Time.deltaTime * dir);

    [PunRPC]
    void DirRPC(int dir) => this.dir = dir;

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Ground") PV.RPC("DestroyRPC", RpcTarget.AllBuffered);

        if(!PV.IsMine && collision.tag == "Player" && collision.GetComponent<PhotonView>().IsMine) //느린쪽이 맞춰서 히트판정 
        {
            collision.GetComponent<PlayerScript>().Hit();
            PV.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }


    
}
