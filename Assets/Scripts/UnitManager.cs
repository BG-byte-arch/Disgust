using System.Collections;
using System.Drawing;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public GameObject objectToClone;
    public float moveSpeed = 1f;
    public EconomyManager economyManager;
    public LayerMask enemyLayer;

    public void SpawnBlue()
    {   
        if (economyManager.food >= 10){
            GameObject clonedObject = Instantiate(objectToClone, objectToClone.transform.position, objectToClone.transform.rotation);
            clonedObject.AddComponent<Unit>().speed = moveSpeed;
            clonedObject.GetComponent<Unit>().enemyLayer = enemyLayer;
            clonedObject.AddComponent<CircleCollider2D>().radius = 1.2f;
            clonedObject.GetComponent<CircleCollider2D>().isTrigger = true;
            clonedObject.SetActive(true);
            economyManager.food -= 10;
        }
    }
}

public class Unit : MonoBehaviour
{
    public float speed = 5f;
    private float oldSpeed;
    public int hitpoint = 10;
    public LayerMask enemyLayer;

    private void Start(){
        oldSpeed = speed;
    }

    private void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other){
        speed = 0f;
        StartCoroutine(Attack());
    }

    private IEnumerator Attack(){
        while (true){
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.left, 5f, enemyLayer);
            Debug.DrawRay(transform.position, hit.point);
            if (hit.collider != null) {
                Debug.DrawRay(transform.position, hit.point);
                Debug.Log("Stop");
                hit.collider.gameObject.GetComponent<Enemy>().hitpoint -= 1;
                
                if (hit.collider.gameObject.GetComponent<Enemy>().hitpoint <= 0){
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
