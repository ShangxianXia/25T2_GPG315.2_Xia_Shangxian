using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class FakingEnemyVisualisationsTest : MonoBehaviour
{
    public static FakingEnemyVisualisationsTest fakingVisualisationsTestInstance;
    
    [Header("Other References")]
    public NavMeshAgent agentEnemy;
    public Transform target;
    public LayerMask targetLayer;
    public GameObject enemyAttackBoxCollider;
    
    [Header("Enemy Range / Checks if attacking")]
    public float enemySightRange;
    public float enemyAttackRange;
    public bool isAttacking;
    
    [Header("Checks for if target is in range")]
    public bool targetInSightRange;
    public bool targetInAttackRange;

    [Header("Enemy Patrolling Settings")]
    public Vector3 patrolPoint;
    public bool foundPatrolPoint;
    public float patrollingDistanceArea;

    [Header("Gizmo Settings")]
    public bool turnOnAllGizmos;
    public bool allGizmosEnabled;
    
    public bool turnOnEnemyAttackRangeGizmo;
    public bool attackRangeGizmoEnabled;
    
    public bool turnOnEnemySightRangeGizmo;
    public bool sightRangeGizmoEnabled;
    
    public bool turnOnEnemyAttackBoxColliderGizmo;
    public bool enemyAttackBoxColliderGizmoEnabled;
    
    public bool turnOnPatrolPointGizmo;
    public bool patrolPointGizmoEnabled;

    private void Awake()
    {
        fakingVisualisationsTestInstance = this;
        target = GameObject.FindGameObjectWithTag("Target").transform;
        agentEnemy = GetComponent<NavMeshAgent>();
        enemyAttackBoxCollider = transform.GetChild(0).gameObject;
    }

    private void Update()
    {
        targetInSightRange = Physics.CheckSphere(transform.position, enemySightRange, targetLayer);
        targetInAttackRange = Physics.CheckSphere(transform.position, enemyAttackRange, targetLayer);

        if (!targetInSightRange && !targetInAttackRange)
        {
            Patrolling();
        }

        if (targetInSightRange && !targetInAttackRange)
        {
            Chasing();
        }

        if (targetInSightRange && targetInAttackRange)
        {
            Attacking();
        }
    }

    private void Patrolling()
    {
        agentEnemy.speed = 1f;

        if (!foundPatrolPoint)
        {
            FindAPatrolPoint();
        }
        else if (foundPatrolPoint)
        {
            agentEnemy.SetDestination(patrolPoint);
        }
        
        Vector3 theDistanceToPatrolPoint = transform.position - patrolPoint;

        if (theDistanceToPatrolPoint.magnitude < 1f)
        {
            foundPatrolPoint = false;
        }
    }

    private void FindAPatrolPoint()
    {
        float randomisingXAxisPatrolArea = Random.Range(-patrollingDistanceArea, patrollingDistanceArea);
        float randomisingZAxisPatrolArea = Random.Range(-patrollingDistanceArea, patrollingDistanceArea);
        
        patrolPoint = new Vector3(transform.position.x + randomisingXAxisPatrolArea, transform.position.y, transform.position.z + randomisingZAxisPatrolArea);

        if (Physics.Raycast(patrolPoint, -transform.up, 2f))
        {
            foundPatrolPoint = true;
        }
    }

    private void Attacking()
    {
        foundPatrolPoint = false;
        agentEnemy.SetDestination(transform.position);
        TurnTowardsTarget();
        StartCoroutine(ShowingEnemyAttackColliderBox());
    }

    private void Chasing()
    {
        agentEnemy.speed = 2f;
        agentEnemy.SetDestination(target.position);
        
        TurnTowardsTarget();
    }

    private IEnumerator ShowingEnemyAttackColliderBox()
    {
        isAttacking = true;
        if (isAttacking)
        {
            enemyAttackBoxCollider.SetActive(true);
        }
        yield return new WaitForSeconds(2f);
        isAttacking = false;
        enemyAttackBoxCollider.SetActive(false);
    }

    private void TurnTowardsTarget()
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0; // This keeps the rotation horizontal
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    // helps visualise the enemy variables
    private void OnDrawGizmos()
    {
        if (turnOnAllGizmos)
        {
            // enemy attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemyAttackRange);
            
            // enemy sight range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemySightRange);
            
            // patrol areas
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, patrolPoint);
            Gizmos.DrawSphere(patrolPoint, 0.1f);
            
            GameObject attackCollider = transform.GetChild(0).gameObject;
            if (attackCollider && attackCollider.GetComponent<Collider>())
            {
                Gizmos.color = new Color(0, 0, 0, 0.3f);
                Gizmos.matrix = attackCollider.transform.localToWorldMatrix;
            
                if (attackCollider.TryGetComponent<BoxCollider>(out var enemyAttackBoxColliderGizmo))
                {
                    Gizmos.DrawCube(enemyAttackBoxColliderGizmo.center, enemyAttackBoxColliderGizmo.size);
                }
            }

            allGizmosEnabled = true;
        }
        else
        {
            allGizmosEnabled = false;
        }

        if (turnOnPatrolPointGizmo)
        {
            // patrol areas
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, patrolPoint);
            Gizmos.DrawSphere(patrolPoint, 0.1f);
            patrolPointGizmoEnabled = true;
        }
        else
        {
            patrolPointGizmoEnabled = false;
        }

        if (turnOnEnemyAttackRangeGizmo)
        {
            // enemy attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemyAttackRange);
            attackRangeGizmoEnabled = true;
        }
        else
        {
            attackRangeGizmoEnabled = false;
        }

        if (turnOnEnemySightRangeGizmo)
        {
            // enemy sight range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemySightRange);
            sightRangeGizmoEnabled = true;
        }
        else
        {
            sightRangeGizmoEnabled = false;
        }

        if (turnOnEnemyAttackBoxColliderGizmo)
        {
            if (!turnOnAllGizmos)
            {
                GameObject attackCollider = transform.GetChild(0).gameObject;
                if (attackCollider && attackCollider.GetComponent<Collider>())
                {
                    Gizmos.color = new Color(0, 0, 0, 0.3f);
                    Gizmos.matrix = attackCollider.transform.localToWorldMatrix;
            
                    if (attackCollider.TryGetComponent<BoxCollider>(out var enemyAttackBoxColliderGizmo))
                    {
                        Gizmos.DrawCube(enemyAttackBoxColliderGizmo.center, enemyAttackBoxColliderGizmo.size);
                    }
                }
                enemyAttackBoxColliderGizmoEnabled = true;
            }
        }
        else
        {
            enemyAttackBoxColliderGizmoEnabled = false;
        }
    }
}
