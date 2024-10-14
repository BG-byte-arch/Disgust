using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject objectToClone;
    public float cloneInterval = 5f;
    public float moveSpeed = 1f;
    public LayerMask playerLayer;

    void Start()
    {
        StartCoroutine(SpawnClone());
    }

    IEnumerator SpawnClone()
    {
        while (true)
        {
            GameObject clonedObject = Instantiate(objectToClone, objectToClone.transform.position, objectToClone.transform.rotation);
            
            clonedObject.AddComponent<Enemy>().speed = moveSpeed;
            clonedObject.GetComponent<Enemy>().playerLayer = playerLayer;
            clonedObject.AddComponent<CircleCollider2D>().radius = 1.2f;
            clonedObject.GetComponent<CircleCollider2D>().isTrigger = true;
            clonedObject.SetActive(true);

            yield return new WaitForSeconds(cloneInterval);
        }
    }
}

// A separate class to handle the movement of each cloned object
public class Enemy : MonoBehaviour
{
    public float speed;
    private float oldSpeed;
    public int hitpoint = 10;
    public LayerMask playerLayer;

    private void Start(){
        oldSpeed = speed;
    }

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other){
        speed = 0f;
        StartCoroutine(Attack());
    }

    private IEnumerator Attack(){
        while (true){
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, 5f, playerLayer);
            Debug.DrawRay(transform.position, hit.point);
            if (hit.collider != null) {
                Debug.DrawRay(transform.position, hit.point);
                Debug.Log("Stop");
                hit.collider.gameObject.GetComponent<Unit>().hitpoint -= 1;
                
                if (hit.collider.gameObject.GetComponent<Unit>().hitpoint <= 0){
                    Destroy(hit.collider.gameObject);
                }
                yield return new WaitForSeconds(1f);
            } else {
                speed = oldSpeed;
                Debug.Log("Go");
                yield break;
            }
        }
    }
}
