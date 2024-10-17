using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class UnitManager : MonoBehaviour
{
    public EconomyManager economyManager;
    public LayerMask enemyLayer;
    public GameObject playerUnitTemplate;
    public GameObject playerUnitProjectileTemplate;
    public UnitClass[] unitClass;

    public void SpawnPlayerUnit(int unitIndex){
        if (economyManager.food >= unitClass[unitIndex].foodCost){
            GameObject clonedUnit = Instantiate(playerUnitTemplate, transform.position, transform.rotation);
            clonedUnit.name = unitClass[unitIndex].name;
            clonedUnit.AddComponent<Unit>().speed = unitClass[unitIndex].speed;
            clonedUnit.GetComponent<SpriteRenderer>().sprite = unitClass[unitIndex].spriteTexture;
            clonedUnit.GetComponent<Unit>().attackDamage = unitClass[unitIndex].attackDamage;
            clonedUnit.GetComponent<Unit>().hitpoint = unitClass[unitIndex].hitpoint;
            clonedUnit.GetComponent<Unit>().attackDelay = unitClass[unitIndex].attackDelay;
            clonedUnit.GetComponent<Unit>().enemyLayer = enemyLayer;
            clonedUnit.AddComponent<CircleCollider2D>().radius = unitClass[unitIndex].hitboxSize;
            clonedUnit.GetComponent<CircleCollider2D>().isTrigger = true;
            if (unitClass[unitIndex].ranged == true){
                clonedUnit.GetComponent<Unit>().ranged = true;
                clonedUnit.GetComponent<Unit>().rangedAttackDelay = unitClass[unitIndex].rangedAttackDelay;
                clonedUnit.GetComponent<Unit>().rangedProjectileSpeed = unitClass[unitIndex].rangedProjectileSpeed;
                clonedUnit.GetComponent<Unit>().rangedAttackRange = unitClass[unitIndex].rangedAttackRange;
                clonedUnit.GetComponent<Unit>().rangedProjectileSpriteTexture = unitClass[unitIndex].rangedProjectileSpriteTexture;
                clonedUnit.GetComponent<Unit>().rangedProjectileTemplate = playerUnitProjectileTemplate;
                clonedUnit.GetComponent<Unit>().rangedProjectileDamage = unitClass[unitIndex].rangedProjectileDamage;
            }
            clonedUnit.SetActive(true);
            economyManager.food -= unitClass[unitIndex].foodCost;
        }
    }
}

public class Unit : MonoBehaviour
{
    public float speed;
    private float oldSpeed;
    public int hitpoint;
    public int attackDamage;
    public float attackDelay;
    public LayerMask enemyLayer;
    public bool ranged;
    public float rangedAttackRange;
    public Sprite rangedProjectileSpriteTexture;
    public GameObject rangedProjectileTemplate;
    public float rangedProjectileSpeed;
    public int rangedProjectileDamage;
    public float rangedAttackDelay;
    private RaycastHit2D shootingRange;
    private bool isShooting;

    private void Start(){
        oldSpeed = speed;
    }

    private void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);
        if (hitpoint <= 0){
            Destroy(gameObject);
        }
        shootingRange = Physics2D.Raycast(transform.position, Vector2.left, rangedAttackRange, enemyLayer);
        if (ranged == true && shootingRange == true && isShooting == false){
            speed = 0;
            StartCoroutine(Attack(true));
            isShooting = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other){
        speed = 0f;
        StartCoroutine(Attack(false));
    }

    private IEnumerator Attack(bool ranged){
        while (true){ 
            if (ranged == false){
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.left, 1.2f, enemyLayer);
                if (hit.collider != null) {
                    hit.collider.gameObject.GetComponent<Enemy>().hitpoint -= attackDamage;
                    yield return new WaitForSeconds(attackDelay);
                } else {
                    speed = oldSpeed;
                    yield break;
                }
            }else{
                RaycastHit2D checkRangeAgain = Physics2D.Raycast(transform.position, Vector2.left, rangedAttackRange, enemyLayer);
                if (checkRangeAgain == true){
                    if (checkRangeAgain.distance >= 1.2f){
                        GameObject projectile = Instantiate(rangedProjectileTemplate, transform.position, Quaternion.identity);
                        projectile.AddComponent<PlayerUnitProjectile>().rangedProjectileSpeed = rangedProjectileSpeed;
                        projectile.GetComponent<PlayerUnitProjectile>().rangedProjectileDamage = rangedProjectileDamage;
                        projectile.AddComponent<BoxCollider2D>().isTrigger = true;
                        yield return new WaitForSeconds(rangedAttackDelay);
                    }else{     
                        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.left, 1.2f, enemyLayer);
                        if (hit.collider != null) {
                            hit.collider.gameObject.GetComponent<Enemy>().hitpoint -= attackDamage;
                            yield return new WaitForSeconds(attackDelay);
                        } else {
                            speed = oldSpeed;
                            yield break;
                        }
                    }     
                }else{
                    speed = oldSpeed;
                    isShooting = false;
                    yield break;
                }
            }
        }
    }
}

public class PlayerUnitProjectile : MonoBehaviour
{
    public float rangedProjectileSpeed;
    public int rangedProjectileDamage;

    private void Update()
    {
        transform.Translate(Vector2.left * rangedProjectileSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other){
        if (other.gameObject != null){
            other.GetComponent<Enemy>().hitpoint -= rangedProjectileDamage;
            Destroy(gameObject);
        }
    }

}