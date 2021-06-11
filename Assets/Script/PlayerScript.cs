using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Cinemachine;

public class PlayerScript : MonoBehaviourPunCallbacks , IPunObservable
{
    public Rigidbody2D rb;
    public Animator Anim;
    public SpriteRenderer SR;
    public PhotonView PV;
    public Text NameText;
    public Image HealthImage;

    public Transform FirePos;
    bool IsGround;
    Vector3 CurPos;

    float axis;
    private void Awake()
    {
        NameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NameText.color = PV.IsMine ? Color.green : Color.red;

        if (photonView.IsMine)
        {
            var CM = GameObject.Find("CMvcam").GetComponent<CinemachineVirtualCamera>();
            CM.Follow = transform;
            CM.LookAt = transform;
        }
    }

    //위치 체력 변수 동기화 
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(HealthImage.fillAmount);
        }
        else
        {
            CurPos = (Vector3)stream.ReceiveNext();
            HealthImage.fillAmount = (float)stream.ReceiveNext();
        }
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) && IsGround) PV.RPC("JumpRPC", RpcTarget.All);
        if (PV.IsMine)
        {
            axis = Input.GetAxisRaw("Horizontal");
            rb.velocity = new Vector2(4 * axis, rb.velocity.y);
        }
    }
    private void Update()
    {
        if(PV.IsMine)
        {
            rb.velocity = new Vector2(4 * axis, rb.velocity.y);

            if(axis != 0)
            {
                Anim.SetBool("Walk", true);
                PV.RPC("FlipXRPC", RpcTarget.AllBuffered, axis); // w재접속 동기화 같은 뷰에 있는사람에게 이 함수를 실행시켜준다는 의미 
            }
            else
            {
                Anim.SetBool("Walk", false);
            }

            IsGround = Physics2D.OverlapCircle((Vector2)transform.position +new Vector2(0 , -0.5f), 0.07f , 1 << LayerMask.NameToLayer("Ground"));

            Anim.SetBool("Jump", !IsGround);
            if(Input.GetKeyDown(KeyCode.UpArrow) && IsGround)PV.RPC("JumpRPC", RpcTarget.All);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                PhotonNetwork.Instantiate("Bullet", transform.position + new Vector3(SR.flipX ? -0.4f : 0.4f, -0.11f, 0), Quaternion.identity)
               .GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, SR.flipX ? -1 : 1);
                Anim.SetTrigger("Shot");
            }
;        }
        else if ((transform.position - CurPos).sqrMagnitude >= 100)
        {
            transform.position = CurPos;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, CurPos, Time.deltaTime * 10);
        }
    }

    [PunRPC]
    void FlipXRPC(float axis) => SR.flipX = axis == -1;


    [PunRPC]
    void JumpRPC()
    {
        rb.velocity = Vector2.zero;
        rb.AddForce(Vector2.up * 700);
    }
    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);

    public void Hit()
    {
        HealthImage.fillAmount -= 0.1f;
        if(HealthImage.fillAmount <= 0)
        {
            GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject.SetActive(true);
            PV.RPC("DestroyRPC", RpcTarget.AllBuffered); // AllBuffered 해야 제대로 사라져 복제 버그가 안생긴다. 
        }
    }
}
