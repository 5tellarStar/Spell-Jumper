using UnityEngine;

public class projectile : MonoBehaviour
{
    public float speed = 1;

    private float timeToLive = 10;
    private float timeLived = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.right * Time.deltaTime * speed;
        timeLived += Time.deltaTime;
        if(timeLived > timeToLive )
        {
            Destroy( this.gameObject );
        }
    }
}
