using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    NavMeshAgent navMeshAgent;
    Animator animator;
    CharacterStats characterStats;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
    }

    // Start is called before the first frame update
    void Start()
    {
        MouseManager.instance.onMousePrimaryButtonClick += TryMoveToDestination;
        MouseManager.instance.onMousePrimaryButtonClick += TryAttackTargetEnemy;
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("Speed", navMeshAgent.velocity.sqrMagnitude);
    }

    void TryMoveToDestination(RaycastHit mouseClickInfo)
    {
        if ((bool)mouseClickInfo.collider?.CompareTag("Ground"))
        {
            StopAllCoroutines();
            navMeshAgent.isStopped = false;
            navMeshAgent.destination = mouseClickInfo.point;
        }
    }

    GameObject targetEnemy;
    float lastAttackTime;
    bool isTargetEnemyWithinAttackArea;
    void TryAttackTargetEnemy(RaycastHit mouseClickInfo)
    {
        if ((bool)mouseClickInfo.collider?.CompareTag("Enemy"))
        {
            targetEnemy = mouseClickInfo.collider.gameObject;
            StartCoroutine(Coroutine_MoveToTargetEnemy());
            StartCoroutine(Coroutine_AttackTargetEnemy());
        }
    }

    IEnumerator Coroutine_MoveToTargetEnemy()
    {
        isTargetEnemyWithinAttackArea = false;
        transform.LookAt(targetEnemy.transform);

        while (Vector3.Distance(transform.position, targetEnemy.transform.position) > characterStats.rawData.attackRange)
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.destination = targetEnemy.transform.position;
            yield return null;
        }

        isTargetEnemyWithinAttackArea = true;
    }

    IEnumerator Coroutine_AttackTargetEnemy()
    {
        while (!isTargetEnemyWithinAttackArea) yield return null;

        navMeshAgent.isStopped = true;
        if (Time.time > lastAttackTime + characterStats.rawData.attackCD)
        {
            animator.SetTrigger("Attack");
            lastAttackTime = Time.time;
        }
    }
}
