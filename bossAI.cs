using System.Collections;
using Pathfinding;
using UnityEngine;

public class Boss : MonoBehaviour {

    [System.Serializable]
    public class BossStats {
        private int maxHealth;

        private int currentHealth;
        public int CurrentHealth {
            get { return currentHealth; }
            set { currentHealth = Mathf.Clamp (value, 0, maxHealth); }
        }

        public void Initialize () {
            int difficulty = PlayerPrefs.GetInt ("difficulty");
            switch (difficulty) {
                case 1:
                    maxHealth = 60;
                    break;
                case 2:
                    maxHealth = 100;
                    break;
                case 3:
                    maxHealth = 50;
                    break;
            }
            currentHealth = maxHealth;
        }
    }

    #region variables
    public BossStats bossStats = new BossStats ();

    [Header ("Boss Components")]
    public Sprite sprite;

    //  Point to Instantiate missiles
    public Transform firePoint1;
    public Transform firePoint2;

    //  This object's rigidbody
    public Rigidbody2D rigidBody2D;

    // This object's Circle Collider 2D
    public PolygonCollider2D polygonCollider2D;

    // This object's Animator
    public Animator animator;

    //  Caching
    public Seeker seeker;

    [Header ("Audio Components")]
    public AudioSource audioSource;
    public AudioClip deathSound;
    public AudioClip shootSound;

    //  What to chase
    private Transform target;

    // Spawn points
    private Transform[] spawnPoints = new Transform[16];

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

    //  Prevents the death logic from TakeDamage() from repeating 
    private bool justDied = false;

    private int difficulty;
    #endregion

    #region initialization
    // Start is called before the first frame update
    void OnEnable () {
        bossStats.Initialize ();
        ResetValues ();

        //  Moves the object back to a spawn point... if not used
        //  it will be spawned at its last death location
        Teleport ();

        if (target == null) {
            if (!searchingForPlayer) {
                searchingForPlayer = true;
                StartCoroutine (searchForPlayer ());
            }
            return;
        }

        //  Start a new path to the target position, return the result to the OnPathComplete method
        seeker.StartPath (transform.position, target.position, OnPathComplete);
        StartCoroutine (UpdatePath ());
    }

    private void ResetValues () {
        GameObject sp = GameObject.Find ("Spawn Points");
        for (int i = 0; i < 16; i++) {
            if (!spawnPoints[i]) {
                spawnPoints[i] = sp.transform.GetChild (i);
            }
        }

        difficulty = PlayerPrefs.GetInt ("difficulty");
        switch (difficulty) {
            case 1:
                speed = 150;
                break;
            case 2:
                speed = 200;
                break;
            case 3:
                speed = 100;
                break;
        }

        updateRate = 2f;
        path = null;
        speed = 200f;
        _fMode = ForceMode2D.Force;
        pathIsEnded = false;
        nextWayPointDistance = 1;
        currentWayPoint = 0;
        searchingForPlayer = false;
        justTouched = false;
        justFired = false;
        allowFire = true;
        justDied = false;
    }
    #endregion

    #region target search
    IEnumerator searchForPlayer () {
        GameObject searchResult = GameObject.FindGameObjectWithTag ("Player");
        if (searchResult == null) {
            yield return new WaitForSeconds (0.5f);
            StartCoroutine (searchForPlayer ());
        } else {
            target = searchResult.transform;
            searchingForPlayer = false;
            StartCoroutine (UpdatePath ());
            yield return false;
        }
    }
    #endregion

    #region pathing
    IEnumerator UpdatePath () {
        if (target == null) {
            if (!searchingForPlayer) {
                searchingForPlayer = true;
                StartCoroutine (searchForPlayer ());
            }
            yield return false;
        } else {
            //  Start a new path to the target position, return the result to the On Path Complete Method
            seeker.StartPath (transform.position, target.position, OnPathComplete);

            yield return new WaitForSeconds (1f / updateRate);
            StartCoroutine (UpdatePath ());
        }
    }

    public void OnPathComplete (Path p) {
        if (!p.error) {
            path = p;
            currentWayPoint = 0;
        }
    }
    #endregion

    #region teleporting
    private void Teleport () {
        StopCoroutine ("EnableDynamic"); // Restarts the function; prevents swinging
        rigidBody2D.bodyType = RigidbodyType2D.Static;
        int randInt = UnityEngine.Random.Range (0, 15);
        transform.position = spawnPoints[randInt].position;
        StartCoroutine ("EnableDynamic");
    }

    private IEnumerator EnableDynamic () {
        yield return new WaitForSeconds (0.5f);
        rigidBody2D.bodyType = RigidbodyType2D.Dynamic;
    }
    #endregion

    #region update
    // Use FixedUpdate for physics stuff
    void FixedUpdate () {
        //  Debugging purposes...
        /*
        if (Input.GetKey ("space")) {
            TakeDamage (100, false);
        }
        */

        //  Shoot Missiles
        if (!justFired && allowFire) {
            justFired = true;
            StartCoroutine (FireMissile ());
        }

        if (target == null) {
            if (!searchingForPlayer) {
                searchingForPlayer = true;
                StartCoroutine (searchForPlayer ());
            }
            return;
        }

        //  Stops shooting and just focuses on moving towards player if too far
        if ((target.transform.position - transform.position).sqrMagnitude >= 15 * 15) {
            allowFire = false;
        } else {
            allowFire = true;
        }

        if (path == null) {
            return;
        }

        if (currentWayPoint >= path.vectorPath.Count) {
            if (pathIsEnded) {
                return;
            }

            pathIsEnded = true;
            return;
        }

        pathIsEnded = false;

        //  If player is way too far, teleport to a spawn point
        if ((target.transform.position - transform.position).sqrMagnitude >= 30 * 30) {
            Teleport ();
        }

        //  Direction to the next waypoint
        Vector3 dir = path.vectorPath[currentWayPoint] - transform.position;
        dir *= speed * Time.fixedDeltaTime;

        //  Move the AI
        rigidBody2D.AddForce (dir, _fMode);

        //  Always rotate to face the player 
        float angle = Mathf.Atan2 (transform.position.y - target.transform.position.y, transform.position.x - target.transform.position.x) * Mathf.Rad2Deg - 90;
        transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.Euler (0, 0, angle), 3 * Time.deltaTime);

        if ((path.vectorPath[currentWayPoint] - transform.position).sqrMagnitude < nextWayPointDistance * nextWayPointDistance) {
            currentWayPoint++;
            return;
        }
    }

    private IEnumerator FireMissile () {
        audioSource.PlayOneShot (shootSound);
        GameObject bullet1 = ObjectPooler.SharedInstance.GetPooledObject ("bossMissile(Clone)");
        if (bullet1 != null) {
            bullet1.transform.position = firePoint1.position;
            bullet1.transform.rotation = firePoint1.rotation;
            bullet1.SetActive (true);
        }
        GameObject bullet2 = ObjectPooler.SharedInstance.GetPooledObject ("bossMissile(Clone)");
        if (bullet2 != null) {
            bullet2.transform.position = firePoint2.position;
            bullet2.transform.rotation = firePoint2.rotation;
            bullet2.SetActive (true);
        }
        yield return new WaitForSeconds (0.75f);
        justFired = false;
    }
    #endregion

    #region damage player
    //  Collision damage uses OnCollisionStay2D and RepeatDamage
    private void OnCollisionStay2D (Collision2D collision) {
        if (collision.transform.tag == "Player") {
            //  Prevents repeated rapid application of damages 
            //  (basically will one shot the player if justTouched is not in the logic)
            if (!justTouched) {
                justTouched = true;

                //  Show the impact
                GameObject impact = ObjectPooler.SharedInstance.GetPooledObject ("greenImpact(Clone)");
                if (impact != null) {
                    impact.transform.position = collision.contacts[0].point;
                    impact.transform.rotation = Quaternion.identity;
                    impact.SetActive (true);
                }

                //  Repeat the damage if there is still collision
                StartCoroutine (RepeatDamage (collision));
            }
        }
    }

    //  Prevents OnCollisionStay2D from continuosly damaging player... hence the delay of 1 second
    IEnumerator RepeatDamage (Collision2D collision) {
        switch (difficulty) {
            case 1:
                collision.transform.GetComponent<Player> ().TakeDamage (10 + WaveSpawner.RoundsBeat);
                break;
            case 2:
                collision.transform.GetComponent<Player> ().TakeDamage (15 + WaveSpawner.RoundsBeat);
                break;
            case 3:
                collision.transform.GetComponent<Player> ().TakeDamage (20 + WaveSpawner.RoundsBeat);
                break;
        }
        yield return new WaitForSeconds (1f);

        //  justTouched returns to false so the logic can repeat itself
        justTouched = false;
    }
    #endregion

    #region take damage
    public void TakeDamage (int damage, bool virus) {
        if (bossStats.CurrentHealth - damage <= 0 && !justDied) {
            justDied = true;
            if (GameMaster.Instance) {
                switch (difficulty) {
                    case 1:
                        GameMaster.Instance.UpdateScore (225);
                        break;
                    case 2:
                        GameMaster.Instance.UpdateScore (425);
                        break;
                    case 3:
                        GameMaster.Instance.UpdateScore (875);
                        break;
                }
            }
            audioSource.PlayOneShot (deathSound);

            //  Stop the Boss and play its death animation
            animator.enabled = true;
            rigidBody2D.bodyType = RigidbodyType2D.Static;
            polygonCollider2D.enabled = false;

            // Store last position for power up spawn
            Vector3 tempPos = this.transform.position;

            // Spawn Power Up with 100% Chance
            GameObject powerUp = ObjectPooler.SharedInstance.GetPooledObject ("Power Up(Clone)");
            if (powerUp != null) {
                powerUp.transform.position = transform.position;
                powerUp.transform.rotation = Quaternion.identity;
                powerUp.SetActive (true);
            }

            //  Spawns explosive virus if hit by explosive virus
            if (virus) {
                GameObject explosion = ObjectPooler.SharedInstance.GetPooledObject ("explosiveVirus(Clone)");
                if (explosion != null) {
                    explosion.transform.position = transform.position;
                    explosion.transform.rotation = transform.rotation;
                    explosion.SetActive (true);
                }
            }

            StartCoroutine (ReturnToPool ());
        } else {
            bossStats.CurrentHealth -= damage;
        }
    }

    private IEnumerator ReturnToPool () {
        yield return new WaitForSeconds (animator.GetCurrentAnimatorStateInfo (0).length);

        //  Resets the object to initial settings
        animator.enabled = false;
        GetComponent<SpriteRenderer> ().sprite = sprite;
        polygonCollider2D.enabled = true;

        //  Disables object...
        this.gameObject.SetActive (false);
    }
    #endregion
}
