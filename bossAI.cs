using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

//  This makes sure that this script does 
//  not get added onto a Game Object without the following components
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Seeker))]

public class bossAI : MonoBehaviour
{
    [System.Serializable]
    public class BossStats
    {
        private int maxHealth = 100;

        private int privCurrentHealth;

        public int currentHealth
        {
            get { return privCurrentHealth; }
            set { privCurrentHealth = Mathf.Clamp(value, 0, maxHealth); }
        }

        public void init()
        {
            currentHealth = maxHealth;
        }
    }

    public BossStats bossStats = new BossStats();

    //  Point to Instantiate missiles
    [SerializeField]
    private Transform firePoint1;
    [SerializeField]
    private Transform firePoint2;

    //  Caching
    [SerializeField]
    private Seeker seeker;
    [SerializeField]
    private Rigidbody2D rigidBody2D;

    //  Impact effect of boss
    public GameObject bossImpact;

    //  Boss Missile
    public GameObject bossMissile;

    //  What to chase
    private Transform target;

    //  How many times each second we will up date our path
    private float updateRate = 2f;

    //  Calculated path
    public Path path;

    //  AI's speed per second
    private float speed = 200f;
    private ForceMode2D _fMode = ForceMode2D.Force;

    //  Have we completed the path?
    private bool pathIsEnded = false;

    //  Max distance from the AI to a waypoint for it to continue to the next waypoint
    private float nextWayPointDistance = 1;

    //  The waypoint we are currently moving towards
    private int currentWayPoint = 0;

    //  Are we searching for a player?
    private bool searchingForPlayer = false;

    //  Check if the boss has just touched a player
    private bool justTouched = false;

    //  Checks if the boss just fired its missiles
    private bool justFired = false;

    //  Stop shooting if player is far
    private bool allowFire = true;

    // Start is called before the first frame update
    void Start()
    {
        bossStats.init();

        //  Error Checking; this component should already be filled in the inspector
        if (firePoint1 == null || firePoint2 == null)
        {
            if (transform.Find("firePoint1") != null || transform.Find("firePoint2") != null)
            {
                firePoint1 = transform.Find("firePoint1");
                firePoint2 = transform.Find("firePoint2");
            }
            else
            {
                Debug.LogError("firePoint1 or firePoint2 children are missing!");
                return;
            }
        }

        //  Error Checking; this component should already be filled in the inspector
        if (seeker == null)
        {
            if (GetComponent<Seeker>() != null)
            {
                seeker = GetComponent<Seeker>();
            }
            else
            {
                Debug.LogError("Seeker component is missing from this object!");
                return;
            }
        }

        //  Error Checking; this component should already be filled in the inspector
        if (rigidBody2D == null)
        {
            if (GetComponent<Rigidbody2D>() != null)
            {
                rigidBody2D = GetComponent<Rigidbody2D>();
            }
            else
            {
                Debug.LogError("Rigidbody2D component is missing from this object!");
                return;
            }
        }

        if (target == null)
        {
            if (!searchingForPlayer)
            {
                searchingForPlayer = true;
                StartCoroutine(searchForPlayer());
            }
            return;
        }

        //  Start a new path to the target position, return the result to the OnPathComplete method
        seeker.StartPath(transform.position, target.position, OnPathComplete);
        StartCoroutine(UpdatePath());
    }

    IEnumerator searchForPlayer()
    {
        GameObject searchResult = GameObject.FindGameObjectWithTag("Player");
        if (searchResult == null)
        {
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(searchForPlayer());
        }
        else
        {
            target = searchResult.transform;
            searchingForPlayer = false;
            StartCoroutine(UpdatePath());
            yield return false;
        }
    }

    IEnumerator UpdatePath()
    {
        if (target == null)
        {
            if (!searchingForPlayer)
            {
                searchingForPlayer = true;
                StartCoroutine(searchForPlayer());
            }
            yield return false;
        }
        else
        {
            //  Start a new path to the target position, return the result to the On Path Complete Method
            seeker.StartPath(transform.position, target.position, OnPathComplete);

            yield return new WaitForSeconds(1f / updateRate);
            StartCoroutine(UpdatePath());
        }
    }

    public void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWayPoint = 0;
        }
    }

    // Use FixedUpdate for physics stuff
    void FixedUpdate()
    {
        //  Shoot Prefab
        if (!justFired && allowFire)
        {
            justFired = true;
            StartCoroutine(fireMissile());
        }

        if (target == null)
        {
            if (!searchingForPlayer)
            {
                searchingForPlayer = true;
                StartCoroutine(searchForPlayer());
            }
            return;
        }

        if (Vector3.Distance(target.transform.position, transform.position) >= 15)
        {
            allowFire = false;
        }
        else
        {
            allowFire = true;
        }

        //  Always rotate face the player 
        float angle = Mathf.Atan2(transform.position.y - target.transform.position.y, transform.position.x - target.transform.position.x) * Mathf.Rad2Deg - 90;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, angle), 3 * Time.deltaTime);

        if (path == null)
        {
            return;
        }

        if (currentWayPoint >= path.vectorPath.Count)
        {
            if (pathIsEnded)
            {
                return;
            }

            pathIsEnded = true;
            return;
        }

        pathIsEnded = false;

        //  Direction to the next waypoint
        Vector3 dir = path.vectorPath[currentWayPoint] - transform.position;
        dir *= speed * Time.fixedDeltaTime;

        //  Move the AI
        rigidBody2D.AddForce(dir, _fMode);

        float dist = Vector3.Distance(transform.position, path.vectorPath[currentWayPoint]);

        if (dist < nextWayPointDistance)
        {
            currentWayPoint++;
            return;
        }
    }

    IEnumerator fireMissile()
    {
        Instantiate(bossMissile, firePoint1.position, firePoint1.rotation);
        Instantiate(bossMissile, firePoint2.position, firePoint2.rotation);
        yield return new WaitForSeconds(0.75f);
        justFired = false;
    }

    //  Collision damage uses OnCollisionStay2D and repeatDamage
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            if (!justTouched)
            {
                justTouched = true;
                Instantiate(bossImpact, collision.contacts[0].point, Quaternion.identity);
                StartCoroutine(repeatDamage(collision));
            }
        }
    }

    //  Prevents OnCollisionStay2D from continuosly damaging player... hence the delay of 1 second
    IEnumerator repeatDamage(Collision2D collision)
    {
        if (collision.transform.GetComponent<player>() != null)
        {
            collision.transform.GetComponent<player>().takeDamage(20);
            yield return new WaitForSeconds(1f);
            justTouched = false;
        }
        else
        {
            Debug.LogError("Player is missing 'player'!");
            yield return false;
        }
    }

    public void takeDamage(int damage)
    {
        if (bossStats.currentHealth - damage <= 0)
        {
            gameMaster.gm.Kill(this.gameObject);
        }
        else
        {
            bossStats.currentHealth -= damage;
        }
    }
}
