using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private enum EnemyState
    {
        Patroling,
        Following,
        Attacking
    }

    [SerializeField] private Player player;
    [SerializeField] private Transform playerPos;

    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolWaitTime = 2f;
    [SerializeField] private float stopAtDistance = 0.5f;

    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float viewAngle = 180f;

    [SerializeField] private float losePlayerTimer = 2f;

    [SerializeField] private float speed = 2.5f;
    [SerializeField] private float sprintCoefficient = 1.5f;

    [SerializeField] private float attackRage = 1.2f;

    [SerializeField] private float attackAnimationTime = 1f;

    [SerializeField] private float damage = 2f;

    [SerializeField] private float health = 100f;

    [SerializeField] private float pushBackForce = 5f;
    [SerializeField] private float pushBackDecayRate = 15f;

    private Vector3 pushBackVelocity;

    private NavMeshAgent agent;

    private int currentPatrolIndex;
    private Coroutine waitCoroutine;

    private bool isDestroyed = false;

    private EnemyState currentState = EnemyState.Patroling;

    private float timeSinceLostPlayer;

    public bool IsWaiting {  get; private set; }
    public bool IsWalking { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsAttacking { get; private set; }

    private bool playerDead = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;

        player.OnPlayerDead += Player_OnPlayerDead;
    }

    private void Player_OnPlayerDead(object sender, System.EventArgs e)
    {
        if (currentState != EnemyState.Patroling)
        {
            StopAllCoroutines();

            IsAttacking = false;
            IsWaiting = false;
            waitCoroutine = null;

            if (agent != null && agent.isActiveAndEnabled)
            {
                agent.isStopped = false;
                agent.ResetPath();
            }

            currentState = EnemyState.Patroling;

            GoToClosestPatrolPoint();
        }
    }

    private void Start()
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            GoToNextPatrolPoint();
        }

        Debug.Log($"HP врага: {health}");
    }

    private void Update()
    {
        if (isDestroyed || agent == null) return;

        if(player == null)
        {
            currentState = EnemyState.Patroling;

            if (!playerDead)
            {
                playerDead = true;
                GoToClosestPatrolPoint();
            }

            Patrol();
            return;
        }

        if (pushBackVelocity.sqrMagnitude > 0.01f)
        {
            Vector3 pushDir = new Vector3(pushBackVelocity.x, 0f, pushBackVelocity.z).normalized;
            float pushDist = pushBackVelocity.magnitude * Time.deltaTime;

            if (!Physics.Raycast(transform.position, pushDir, pushDist, ~LayerMask.GetMask("Enemy")))
            {
                transform.position += pushDir * pushDist;
            }

            pushBackVelocity = Vector3.Lerp(pushBackVelocity, Vector3.zero, pushBackDecayRate * Time.deltaTime);
            return;
        }

        if (player.GetHealth() <= 0f && currentState != EnemyState.Patroling)
        {
            currentState = EnemyState.Patroling;
            GoToClosestPatrolPoint();

            agent.isStopped = false;
            IsAttacking = false;

            StopAllCoroutines();
            return;
        }

        if (IsSprinting)
            agent.speed = speed * sprintCoefficient;
        else agent.speed = speed;

        var distToPlayer = Vector3.Distance(playerPos.position, transform.position);

        switch (currentState)
        {
            case EnemyState.Patroling:
                IsSprinting = false;

                Patrol();
                if(distToPlayer <= detectionRange && CanSeePlayer())
                {
                    currentState = EnemyState.Following;
                    FollowPlayer();
                }
                break;

            case EnemyState.Following:
                IsSprinting = true;

                FollowPlayer();

                if(distToPlayer <= attackRage)
                {
                    currentState = EnemyState.Attacking;
                }

                if (!CanSeePlayer())
                {
                    timeSinceLostPlayer += Time.deltaTime;
                    if(timeSinceLostPlayer >= losePlayerTimer)
                    {
                        currentState = EnemyState.Patroling;
                        GoToClosestPatrolPoint();
                    }
                }
                else
                {
                    timeSinceLostPlayer = 0f;
                }
                break;

            case EnemyState.Attacking:
                Attack();

                if (!IsAttacking && distToPlayer <= attackRage)
                {
                    IsAttacking = true;
                    StartCoroutine(StartAttackAnimation());
                    StartAttackAnimation();
                }

                if (!IsAttacking && distToPlayer > attackRage)
                {
                    currentState = EnemyState.Following;
                    agent.isStopped = false;
                    IsAttacking = false;
                    StopAllCoroutines();
                }
                break;
        }

        Patrol();
        UpdateAnimations();
    }

    private void OnAttackAnimationEnd()
    {
        IsAttacking = false;
    }

    private void Attack()
    {
        agent.isStopped = true;

        var dir = (playerPos.position - transform.position).normalized;
        dir.y = 0f;

        if(dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    private IEnumerator StartAttackAnimation()
    {
        yield return new WaitForSeconds(attackAnimationTime);

        if(player != null && player.GetHealth() > 0f)
        {
            Vector3 pushBackDir = (playerPos.position - transform.position).normalized;
            pushBackDir.y = 0f;

            player.TakeDamageFromEnemy(damage, pushBackDir);

            Debug.Log(player.GetHealth());
        }

        OnAttackAnimationEnd();
    }

    private void GoToClosestPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0 || agent == null) return;

        int closestIndex = 0;
        float closestDistance = float.MaxValue;

        for(int i = 0; i < patrolPoints.Length; i++)
        {
            var dist = Vector3.Distance(transform.position, patrolPoints[i].position);

            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestIndex = i;
            }
        }

        currentPatrolIndex = closestIndex;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    private void FollowPlayer()
    {
        agent.SetDestination(playerPos.position);
    }

    private bool CanSeePlayer()
    {
        return IsFacingPlayer() && HasCLearPathToPlayer();
    }

    private bool IsFacingPlayer()
    {
        var dirToPlyaer = (playerPos.position - transform.position).normalized;
        var angle = Vector3.Angle(transform.forward, dirToPlyaer);

        return angle <= viewAngle / 2;
    }

    private bool HasCLearPathToPlayer()
    {
        var dirPlayer = playerPos.position - transform.position;

        if(Physics.Raycast(transform.position, dirPlayer.normalized, out RaycastHit hit, dirPlayer.magnitude))
        {
            return hit.transform == playerPos;
        }

        return true;
    }

    private void Patrol()
    {
        if (IsWaiting)
        {
            var distToPlayer = Vector3.Distance(playerPos.position, transform.position);

            if (CanSeePlayer() && distToPlayer <= detectionRange)
            {
                currentState = EnemyState.Following;
                FollowPlayer();
            }

            else return;
        }

        if (patrolPoints == null || patrolPoints.Length == 0) return;

        if (agent != null && agent.isActiveAndEnabled && !agent.pathPending && agent.remainingDistance <= stopAtDistance)
        {
            if (waitCoroutine == null)
                waitCoroutine = StartCoroutine(WaitPatrolPoint());
        }
    }

    private IEnumerator WaitPatrolPoint()
    {
        IsWaiting = true;

        if (agent != null)
            agent.isStopped = true;

        yield return new WaitForSeconds(patrolWaitTime);

        if (isDestroyed) yield break;

        if (agent != null)
            agent.isStopped = false;

        GoToNextPatrolPoint();
        IsWaiting = false;

        waitCoroutine = null;
    }

    private void GoToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0 || agent == null) return;

        if (patrolPoints[currentPatrolIndex] == null)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            return;
        }

        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    private void UpdateAnimations()
    {
        if (agent != null)
        {
            IsWalking = agent.velocity.sqrMagnitude > 0.01f;
        }
    }

    public void TakeDamage(float damage, Vector3 hitDirection)
    {
        if (isDestroyed) return;

        health -= damage;

        Debug.Log($"Враг получил урон {damage}. HP: {health}");

        if (hitDirection != Vector3.zero)
        {
            pushBackVelocity = hitDirection.normalized * pushBackForce;
        }

        if (player != null && player.GetHealth() > 0)
        {
            Vector3 dirToPlayer = (playerPos.position - transform.position).normalized;
            dirToPlayer.y = 0;

            if (dirToPlayer != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(dirToPlayer);
            }

            if (currentState != EnemyState.Following && currentState != EnemyState.Attacking)
            {
                currentState = EnemyState.Following;
                FollowPlayer();

                if (waitCoroutine != null)
                {
                    StopCoroutine(waitCoroutine);

                    waitCoroutine = null;
                    IsWaiting = false;

                    if (agent != null)
                        agent.isStopped = false;
                }
            }
        }

        if (health <= 0)
        {
            Die();
        }
    }

    private void OnDisable()
    {
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }
    }

    private void OnDestroy()
    {
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }

        isDestroyed = true;
    }

    public void Die()
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }
}
