using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { Guard, Patrol, Chase, Dead };

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    public EnemyState originState;
    public float patrolAreaRaidus;
    public float maxSightAreaRadius;
    public float normalSpeed;
    public float chaseSpeed;
    public float wayPointStopIntervalSecs;

    EnemyState currentState;

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
        currentState = originState;
        navMeshAgent.speed = normalSpeed;
        patrolCenterPoint = transform.position;
        currentWayPoint = patrolCenterPoint;
        remainWayPointStopIntervalTime = wayPointStopIntervalSecs;
    }

    // Update is called once per frame
    void Update()
    {
        if (HuntPlayerInSightArea())
        {
            currentState = EnemyState.Chase;
        }
        SwitchBasicState();
        SwitchAnimatorState();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxSightAreaRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, patrolAreaRaidus);
    }

    float remainWayPointStopIntervalTime;
    float lastAttackTime;
    void SwitchBasicState()
    {
        switch (currentState)
        {
        case EnemyState.Guard:
            isBattle = false;
            isChase = false;
            isWalk = Vector3.Distance(transform.position, patrolCenterPoint)
                > navMeshAgent.stoppingDistance;
            // Enemy may walk back to the guard point.
            navMeshAgent.isStopped = false;
            navMeshAgent.speed = normalSpeed;
            navMeshAgent.destination = patrolCenterPoint;
            break;
        case EnemyState.Patrol:
            isBattle = false;
            navMeshAgent.isStopped = false;
            navMeshAgent.speed = normalSpeed;
            if (Vector3.Distance(transform.position, currentWayPoint) < navMeshAgent.stoppingDistance)
            {
                isWalk = false;
                remainWayPointStopIntervalTime -= Time.deltaTime;
                if (remainWayPointStopIntervalTime < 0)
                {
                    ResetWayPoint();
                    remainWayPointStopIntervalTime = wayPointStopIntervalSecs;
                }
            }
            else
            {
                isWalk = true;
                navMeshAgent.destination = currentWayPoint;
            }
            break;
        case EnemyState.Chase:
            isWalk = false;
            isBattle = true;
            navMeshAgent.isStopped = false;
            navMeshAgent.speed = chaseSpeed;
            if (HuntPlayerInSightArea())
            {

                if (IsTargetPlayerInAttackRange() || IsTargetPlayerInSkillRange())
                {
                    isChase = false;
                    navMeshAgent.isStopped = true;
                    if (Time.time > lastAttackTime + characterStats.rawData.attackCD)
                    {
                        Random.InitState(System.DateTime.Now.Second);
                        characterStats.isNextAttackCritical = Random.value < characterStats.rawData.criticalChance;
                        AttackTargetPlayer();
                        lastAttackTime = Time.time;
                    }
                }
                else
                {
                    isChase = true;
                    navMeshAgent.isStopped = false;
                    navMeshAgent.destination = targetPlayer.transform.position;
                }
            }
            else
            {
                isChase = false;
                currentState = originState;
                // Stop immediately when out of fight.
                navMeshAgent.destination = transform.position;
            }
            break;
        case EnemyState.Dead:
            break;
        default:
            break;
        }
    }

    void AttackTargetPlayer()
    {
        transform.LookAt(targetPlayer.transform);
        animator.SetTrigger("Attack");
    }

    bool IsTargetPlayerInAttackRange()
    {
        return IsTargetPlayerInRange(characterStats.rawData.attackRange);
    }

    bool IsTargetPlayerInSkillRange()
    {
        return IsTargetPlayerInRange(characterStats.rawData.skillRange);
    }

    bool IsTargetPlayerInRange(float range)
    {
        if (targetPlayer != null)
        {
            return Vector3.Distance(transform.position, targetPlayer.transform.position) < range;
        }
        return false;
    }

    GameObject targetPlayer;
    bool HuntPlayerInSightArea()
    {
        var colliders = Physics.OverlapSphere(transform.position, maxSightAreaRadius);
        foreach (var target in colliders)
        {
            if (target.CompareTag("Player"))
            {
                targetPlayer = target.gameObject;
                return true;
            }
        }
        targetPlayer = null;
        return false;
    }

    bool isWalk, isBattle, isChase;
    void SwitchAnimatorState()
    {
        animator.SetBool("Walk", isWalk);
        animator.SetBool("Battle", isBattle);
        animator.SetBool("Chase", isChase);
        animator.SetBool("Critical", characterStats.isNextAttackCritical);
    }

    Vector3 patrolCenterPoint;
    Vector3 currentWayPoint;
    void ResetWayPoint()
    {
        float randomX = Random.Range(-patrolAreaRaidus, patrolAreaRaidus);
        float randomZ = Random.Range(-patrolAreaRaidus, patrolAreaRaidus);
        Vector3 randomPoint = patrolCenterPoint + new Vector3(randomX, 0.0f, randomZ);
        NavMeshHit navMeshHit;
        currentWayPoint = NavMesh.SamplePosition(randomPoint, out navMeshHit, 1.0f, 1)
            ? navMeshHit.position : currentWayPoint;
    }
}
