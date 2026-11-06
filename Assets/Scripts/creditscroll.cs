using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UIElements;

public class creditscroll : MonoBehaviour
{
    public GameObject crediits;
    public float speed = 5f; // adjust for speed
    private float positionY;

    
        void Update()
        {
            positionY += speed * Time.deltaTime;
            crediits.transform.position = new Vector3(transform.position.x, positionY, transform.position.z);
        }
    

}
