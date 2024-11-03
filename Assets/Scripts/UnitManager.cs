using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Random=UnityEngine.Random;
using UnityEditor.Experimental.GraphView;
using Unity.VisualScripting;

public class UnitManager : MonoBehaviour
{
    [Header("Player Settings")]
    public UnitClass[] playerUnitClass;
    public EconomyManager economyManager;
    public LayerMask enemyLayer;
    public GameObject playerUnitTemplate;
    public GameObject playerUnitProjectileTemplate;
    [Header("Enemy Settings")]
    public UnitClass[] enemyUnitClass;
    public bool EnableEnemy;
    public LayerMask playerLayer;
    public GameObject enemyUnitTemplate;
    public GameObject enemyUnitProjectileTemplate;
    public float cloneInterval = 5f; 
    public float AcloneInterval = 5f;

    public void Start(){
        if (EnableEnemy == true){
            StartCoroutine(SpawnEnemyUnit(0));
            StartCoroutine(SpawnEnemyUnit(1));
        }
    }

    IEnumerator SpawnEnemyUnit(int enemyUnitIndex)
    {
        while (true)
        {
            SpawnUnit(enemyUnitClass, enemyUnitIndex, playerLayer, true, enemyUnitTemplate);
            if (enemyUnitIndex == 0){
                yield return new WaitForSeconds(cloneInterval);
            }else{
                yield return new WaitForSeconds(AcloneInterval);
            }
        }
    }

    public void SpawnPlayerUnit(int playerUnitIndex){
        if (economyManager.food >= playerUnitClass[playerUnitIndex].foodCost){
            SpawnUnit(playerUnitClass, playerUnitIndex, enemyLayer, false, playerUnitTemplate);
        }   
    }

    public void SpawnUnit(UnitClass[] unitClass, int unitIndex, LayerMask unitLayer, bool isEnemyUnit, GameObject unitTemplate){
        GameObject clonedUnit = Instantiate(unitTemplate, new Vector2(unitTemplate.transform.position.x, unitTemplate.transform.position.y + Random.Range(unitClass[unitIndex].minSpawnYoffset, unitClass[unitIndex].maxSpawnYoffset)), unitTemplate.transform.rotation);
        clonedUnit.name = unitClass[unitIndex].name;
        clonedUnit.GetComponent<SpriteRenderer>().sortingOrder = (int) Mathf.Abs(clonedUnit.transform.position.y * 10) + unitClass[unitIndex].additionalOrderInLayer;
        clonedUnit.AddComponent<Unit>().isEnemyUnit = isEnemyUnit;
        clonedUnit.GetComponent<Unit>().speed = unitClass[unitIndex].speed;
        clonedUnit.GetComponent<Unit>().attackDamage = unitClass[unitIndex].attackDamage;
        clonedUnit.GetComponent<Unit>().hitpoint = unitClass[unitIndex].hitpoint;
        clonedUnit.GetComponent<Unit>().attackDelay = unitClass[unitIndex].attackDelay;
        clonedUnit.GetComponent<Unit>().fightingLayer = unitLayer;
        clonedUnit.AddComponent<CircleCollider2D>().radius = unitClass[unitIndex].hitboxSize;
        clonedUnit.GetComponent<CircleCollider2D>().isTrigger = true;
        clonedUnit.AddComponent<Animator>().runtimeAnimatorController = unitClass[unitIndex].animatorController;
        if (unitClass[unitIndex].ranged == true){
            clonedUnit.GetComponent<Unit>().ranged = true;
            clonedUnit.GetComponent<Unit>().rangedAttackDelay = unitClass[unitIndex].rangedAttackDelay;
            clonedUnit.GetComponent<Unit>().rangedProjectileSpeed = unitClass[unitIndex].rangedProjectileSpeed;
            clonedUnit.GetComponent<Unit>().rangedAttackRange = unitClass[unitIndex].rangedAttackRange;
            clonedUnit.GetComponent<Unit>().rangedProjectileSpriteTexture = unitClass[unitIndex].rangedProjectileSpriteTexture;
            clonedUnit.GetComponent<Unit>().rangedProjectileTemplate = playerUnitProjectileTemplate;
            clonedUnit.GetComponent<Unit>().rangedProjectileDamage = unitClass[unitIndex].rangedProjectileDamage;
            clonedUnit.GetComponent<Unit>().rangedProjectileDamage = unitClass[unitIndex].rangedProjectileDamage;
        }
        clonedUnit.SetActive(true);
        if (isEnemyUnit == false){
            economyManager.food -= unitClass[unitIndex].foodCost;
        }
    }
}

public class Unit : MonoBehaviour
{
    public bool isEnemyUnit;
    public float speed;
    private float oldSpeed;
    public int hitpoint;
    public int attackDamage;
    public float attackDelay;
    public LayerMask fightingLayer;
    public bool ranged;
    public float rangedAttackRange;
    public Sprite rangedProjectileSpriteTexture;
    public GameObject rangedProjectileTemplate;
    public float rangedProjectileSpeed;
    public int rangedProjectileDamage;
    public float rangedAttackDelay;
    private RaycastHit2D shootingRange;
    private bool isShooting;
    public Animator animator;
    public GameObject floatingDamagePrefab;
    private Vector3 direction;

    private void Start(){
        oldSpeed = speed;
        animator = gameObject.GetComponent<Animator>();
        animator.SetFloat("moving_speed", speed);
        if (isEnemyUnit == true){
            direction = Vector3.right;
        }else{
            direction = Vector3.left;
        }
    }
    
    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
        if (hitpoint <= 0){
            Destroy(gameObject);
        }
        if (ranged == true){
            shootingRange = Physics2D.Raycast(transform.position, direction, rangedAttackRange, fightingLayer);
            if (shootingRange == true && isShooting == false){
                isShooting = true;
                Debug.Log("shooting");
                animator.SetBool("shooting", true);
                StopAllCoroutines();
                StartCoroutine(Shoot());
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other){
        SetSpeed(0);
        animator.SetFloat("moving_speed", speed);
        StopAllCoroutines();
        StartCoroutine(Attack());
    }

    private IEnumerator Attack(){
        while (true){
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1.2f, fightingLayer);
            if (hit.collider != null) {
                SetIsShooting(false);
                hit.collider.gameObject.GetComponent<Unit>().hitpoint -= attackDamage;
                Debug.Log("Attack");
            }else{
                isShooting = false;
                SetSpeed(oldSpeed);
            }
            yield return new WaitForSeconds(attackDelay);
        }
    }

    private IEnumerator Shoot(){
        while (true){ 
            RaycastHit2D checkRangeAgain = Physics2D.Raycast(transform.position, direction, rangedAttackRange, fightingLayer);
            yield return new WaitForSeconds(rangedAttackDelay);
            if (checkRangeAgain.collider != null){
                if (checkRangeAgain.distance >= 0.8f){
                    SetSpeed(0);
                    GameObject projectile = Instantiate(rangedProjectileTemplate, transform.position, Quaternion.identity);
                    projectile.AddComponent<PlayerUnitProjectile>().rangedProjectileSpeed = rangedProjectileSpeed;
                    projectile.GetComponent<PlayerUnitProjectile>().rangedProjectileDamage = rangedProjectileDamage;
                    projectile.GetComponent<PlayerUnitProjectile>().direction = direction;
                    projectile.AddComponent<BoxCollider2D>().isTrigger = true;
                    projectile.AddComponent<BoxCollider2D>().size = new Vector2 (1, 5);
                }else{
                    SetIsShooting(false);
                    SetSpeed(0);
                }
            }
            else{
                isShooting = false;
                SetIsShooting(false);
                SetSpeed(oldSpeed);
            }
        }
    }

    public void ShowDamage(int damage, Vector3 position)
    {
        GameObject damageTextObj = Instantiate(floatingDamagePrefab, position, Quaternion.identity);
        DamageIndicator damageText = damageTextObj.GetComponent<DamageIndicator>();
        damageText.SetDamageText(damage);
    }

    public void SetSpeed(float speedA){
        speed = speedA;
        animator.SetFloat("moving_speed", speed);
    }
    
    public void SetIsShooting(bool isShootingA){
        if (ranged == true){
            Debug.Log("false");
            animator.SetBool("shooting", isShootingA);
        }
    }
}

public class PlayerUnitProjectile : MonoBehaviour
{
    public float rangedProjectileSpeed;
    public int rangedProjectileDamage;
    public float yOffset = 0.4f;
    public Vector2 direction;

    private void Start()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y + yOffset, transform.position.z);
    }

    private void Update()
    {
        transform.Translate(direction * rangedProjectileSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other){
        if (other.gameObject.CompareTag("projectilesDestroyer") == false){
            other.GetComponent<Unit>().hitpoint -= rangedProjectileDamage;
            Destroy(gameObject);
        }else{
            Destroy(gameObject);
        }
    }
}