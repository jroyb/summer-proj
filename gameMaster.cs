using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameMaster : MonoBehaviour
{
    public static gameMaster gm;
    public static GameObject cam;

    public Transform powerUp;

    public Transform ship1r;
    public Transform ship1rRespawn;

    private float shakeAmount = 0.035f;

    // Start is called before the first frame update
    private void Start()
    {
        if (gm == null || cam == null)
        {
            gm = GameObject.FindGameObjectWithTag("gm").GetComponent<gameMaster>();
            cam = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    public void Shake()
    {
        InvokeRepeating("doShake", 0, 0.01f);
        Invoke("stopShake", 0.25f);
    }

    private void doShake()
    {
        if (shakeAmount > 0)
        {
            Vector3 camPositionShake = cam.transform.position;

            float offsetX = Random.value * shakeAmount * 2 - shakeAmount;
            float offsetY = Random.value * shakeAmount * 2 - shakeAmount;

            camPositionShake.x += offsetX;
            camPositionShake.y += offsetY;

            cam.transform.position = camPositionShake;
        }
    }

    private void stopShake()
    {
        CancelInvoke("doShake");
        cam.transform.localPosition = Vector3.zero;
    }

    public IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(5f);
        Vector3 spawnPos = new Vector3(cam.transform.position.x, cam.transform.position.y, cam.transform.position.z + 1);
        Instantiate(ship1rRespawn, spawnPos, cam.transform.rotation);
    }

    public void SpawnPlayer()
    {
        Vector3 spawnPos = new Vector3(cam.transform.position.x, cam.transform.position.y, cam.transform.position.z + 1);

        Instantiate(ship1r, spawnPos, cam.transform.rotation);
    }

    public void Kill(GameObject theObject)
    {
        //  Kills Rook
        if (theObject.GetComponent<rookAI>() != null)
        {
            theObject.GetComponent<Animator>().enabled = true;
            theObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            theObject.GetComponent<CircleCollider2D>().enabled = false;

            //  Store last position for power up spawn
            Vector3 tempPos = theObject.gameObject.transform.position;

            Destroy(theObject.gameObject, theObject.gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length - 0.02f);

            //  Spawn Power Up with 30% chance
            if (Random.value > 0.7)
            {
                Instantiate(powerUp, tempPos, Quaternion.identity);
            }
        }
        //  Kills Boss
        else if (theObject.GetComponent<bossAI>() != null)
        {
            theObject.GetComponent<Animator>().enabled = true;
            theObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            theObject.GetComponent<PolygonCollider2D>().enabled = false;

            //  Store last position for power up spawn
            Vector3 tempPos = theObject.gameObject.transform.position;

            Destroy(theObject.gameObject, theObject.gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length - 0.02f);

            //  Spawn Power Up with 100% Chance
            Instantiate(powerUp, tempPos, Quaternion.identity);
        }
        //  Kills Player
        else
        {
            theObject.transform.Find("sprite").GetComponent<Animator>().enabled = true;
            theObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            theObject.transform.Find("sprite").GetComponent<PolygonCollider2D>().enabled = false;
            Destroy(theObject, theObject.transform.Find("sprite").GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length - 0.02f);
            StartCoroutine(RespawnPlayer());
        }
    }

    public void RaycastDamage(RaycastHit2D hitInfo, int damage, Vector3 dir)
    {
        //  Kick back whatever the raycast hits
        if (hitInfo.transform.GetComponent<Rigidbody2D>() != null)
        {
            hitInfo.transform.GetComponent<Rigidbody2D>().AddForce(dir * 100);
        }

        if (hitInfo.transform.tag == "Enemy")
        {
            if (hitInfo.transform.GetComponent<rookAI>() != null)
            {
                hitInfo.transform.GetComponent<rookAI>().takeDamage(damage);
            }
            else if (hitInfo.transform.GetComponent<bossAI>() != null)
            {
                hitInfo.transform.GetComponent<bossAI>().takeDamage(damage);
            }
            else
            {
                Debug.LogError("Enemy is missing 'rookAI' or 'bossAI'!");
                return;
            }
        }
        else if (hitInfo.transform.tag == "Player")
        {
            if (hitInfo.transform.GetComponent<player>() != null)
            {
                hitInfo.transform.GetComponent<player>().takeDamage(5);
            }
            else
            {
                Debug.LogError("Player is missing 'player'!");
                return;
            }
        }
        else
        {
            if (hitInfo.transform.gameObject.GetComponent<bossMissile>() != null)
            {
                hitInfo.transform.gameObject.GetComponent<bossMissile>().onHitByRaycast();
            }
            else if (hitInfo.transform.tag == "ForgottenSins")
            {
                hitInfo.transform.GetComponent<forgottenSins>().Detonate();
            }
        }
    }

    public void MissileCollisionDamage(Collision2D collision, int damage, bool isEnemyMissile)
    {
        if (collision.transform.tag == "Enemy")
        {
            //  Enemy missile hitting Enemy
            if (isEnemyMissile)
            {
                if (collision.transform.GetComponent<rookAI>() != null)
                {
                    collision.transform.GetComponent<rookAI>().takeDamage(5);
                }
                else if (collision.transform.GetComponent<bossAI>() != null)
                {
                    collision.transform.GetComponent<bossAI>().takeDamage(5);
                }
                else
                {
                    Debug.LogError("Enemy is missing 'rookAI' or 'bossAI'!");
                    return;
                }
            }
            //  Player missile hitting Enemy
            else
            {
                if (collision.transform.GetComponent<rookAI>() != null)
                {
                    collision.transform.GetComponent<rookAI>().takeDamage(damage);
                }
                else if (collision.transform.GetComponent<bossAI>() != null)
                {
                    collision.transform.GetComponent<bossAI>().takeDamage(damage);
                }
                else
                {
                    Debug.LogError("Enemy is missing 'rookAI' or 'bossAI'!");
                    return;
                }
            }
        }
        else if (collision.transform.tag == "Player")
        {
            //  Enemy missile hitting Player
            if (isEnemyMissile)
            {
                if (collision.transform.GetComponent<player>() != null)
                {
                    collision.transform.GetComponent<player>().takeDamage(damage);
                }
                else
                {
                    Debug.LogError("Player is missing 'player'!");
                    return;
                }
            }
            //  Player missile hitting Player
            else
            {
                if (collision.transform.GetComponent<player>() != null)
                {
                    collision.transform.GetComponent<player>().takeDamage(5);
                }
                else
                {
                    Debug.LogError("Player is missing 'player'!");
                    return;
                }
            }
        }
        else if (collision.transform.tag == "ForgottenSins")
        {
            collision.transform.GetComponent<forgottenSins>().Detonate();
        }
    }

    public void AOECollisionDamage(Collider2D collision, int damage, Vector2 pos)
    {
        if (collision.transform.tag == "Enemy")
        {
            if (collision.GetComponent<rookAI>() != null)
            {
                Vector2 dir = (Vector2)collision.transform.position - pos;
                collision.GetComponent<Rigidbody2D>().AddForce(dir.normalized * 1000);
                collision.GetComponent<rookAI>().takeDamage(50);
            }
            else if (collision.GetComponent<bossAI>() != null)
            {
                Vector2 dir = (Vector2)collision.transform.position - pos;
                collision.GetComponent<Rigidbody2D>().AddForce(dir.normalized * 1000);
                collision.GetComponent<bossAI>().takeDamage(50);

            }
            else
            {
                Debug.LogError("Enemy is missing 'rookAI' or 'bossAI'!");
                return;
            }
        }
        else if (collision.transform.parent != null)
        {
            if (collision.transform.parent.gameObject.tag == "Player")
            {
                GameObject playerObject = collision.transform.parent.gameObject;

                if (playerObject.GetComponent<player>() != null && playerObject.GetComponent<Rigidbody2D>() != null)
                {
                    Vector2 dir = (Vector2)collision.transform.position - pos;
                    playerObject.GetComponent<Rigidbody2D>().AddForce(dir.normalized * 500);
                    playerObject.GetComponent<player>().takeDamage(5);
                }
                else
                {
                    Debug.LogError("Player is missing 'player' and 'Rigidbody2D!");
                    return;
                }
            }
        }
        else if (collision.transform.tag == "ForgottenSins")
        {
            collision.GetComponent<forgottenSins>().Detonate();
        }
    }
}