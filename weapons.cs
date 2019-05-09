using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class weapons : MonoBehaviour
{
    [System.Serializable]
    public class M3
    {
        public int damage;

        private GameObject m3ImpactEffect;
        private Transform m3FirePoint;
        private LineRenderer m3LineRenderer;

        public void m3Init(GameObject impact, Transform firePoint, LineRenderer lineRenderer)
        {
            m3ImpactEffect = impact;
            m3FirePoint = firePoint;
            m3LineRenderer = lineRenderer;
            damage = 10;
        }

        public void setColor()
        {
            m3LineRenderer.startColor = Color.red;
            m3LineRenderer.endColor = Color.red;
        }

        public IEnumerator M3Shoot()
        {
            RaycastHit2D hitInfo = Physics2D.Raycast(m3FirePoint.position, m3FirePoint.up);

            if (hitInfo)
            {
                gameMaster.gm.RaycastDamage(hitInfo, damage, m3FirePoint.up);
                Instantiate(m3ImpactEffect, hitInfo.point, Quaternion.identity);

                m3LineRenderer.SetPosition(0, m3FirePoint.position);
                m3LineRenderer.SetPosition(1, hitInfo.point);
            }
            else
            {
                m3LineRenderer.SetPosition(0, m3FirePoint.position);
                m3LineRenderer.SetPosition(1, m3FirePoint.position + m3FirePoint.up * 100);
            }

            m3LineRenderer.enabled = true;

            yield return new WaitForSeconds(0.02f);

            m3LineRenderer.enabled = false;
        }
    }

    [System.Serializable]
    public class StingingTruths
    {
        public int damage;

        private GameObject stImpactEffect;
        private Transform[] stFirePoint = new Transform[2];
        private LineRenderer[] stLineRenderer = new LineRenderer[2];

        public void stInit(GameObject impact, Transform firePoint1, Transform firePoint2, LineRenderer lineRenderer1, LineRenderer lineRenderer2)
        {
            stImpactEffect = impact;
            stFirePoint[0] = firePoint1;
            stFirePoint[1] = firePoint2;
            stLineRenderer[0] = lineRenderer1;
            stLineRenderer[1] = lineRenderer2;
            damage = 4;
        }

        public void setColor()
        {
            stLineRenderer[0].startColor = Color.yellow;
            stLineRenderer[0].endColor = Color.yellow;
            stLineRenderer[1].startColor = Color.yellow;
            stLineRenderer[1].endColor = Color.yellow;
        }

        public IEnumerator STShoot()
        {
            RaycastHit2D hitInfo1 = Physics2D.Raycast(stFirePoint[0].position, stFirePoint[0].up);
            RaycastHit2D hitInfo2 = Physics2D.Raycast(stFirePoint[1].position, stFirePoint[1].up);

            shootLeft(hitInfo1);
            shootRight(hitInfo2);

            stLineRenderer[1].enabled = true;
            stLineRenderer[0].enabled = true;

            yield return new WaitForSeconds(0.02f);

            stLineRenderer[0].enabled = false;
            stLineRenderer[1].enabled = false;
        }

        private void shootLeft(RaycastHit2D hitInfo)
        {
            if (hitInfo)
            {
                gameMaster.gm.RaycastDamage(hitInfo, damage, stFirePoint[0].up);
                Instantiate(stImpactEffect, hitInfo.point, Quaternion.identity);

                stLineRenderer[0].SetPosition(0, stFirePoint[0].position);
                stLineRenderer[0].SetPosition(1, hitInfo.point);
            }
            else
            {
                stLineRenderer[0].SetPosition(0, stFirePoint[0].position);
                stLineRenderer[0].SetPosition(1, stFirePoint[0].position + stFirePoint[0].up * 100);
            }
        }

        private void shootRight(RaycastHit2D hitInfo)
        {
            if (hitInfo)
            {
                gameMaster.gm.RaycastDamage(hitInfo, damage, stFirePoint[0].up);
                Instantiate(stImpactEffect, hitInfo.point, Quaternion.identity);

                stLineRenderer[1].SetPosition(0, stFirePoint[1].position);
                stLineRenderer[1].SetPosition(1, hitInfo.point);
            }
            else
            {
                stLineRenderer[1].SetPosition(0, stFirePoint[1].position);
                stLineRenderer[1].SetPosition(1, stFirePoint[1].position + stFirePoint[1].up * 100);
            }
        }
    }

    [System.Serializable]
    public class RequiemForADream
    {
        private GameObject rfadImpactEffect;
        private GameObject rfadBulletPrefab;
        private Transform[] rfadFirePoint = new Transform[5];

        public void rfadInit(GameObject impact, GameObject bulletPrefab, Transform[] firePoint)
        {
            rfadImpactEffect = impact;
            rfadBulletPrefab = bulletPrefab;

            for (int i = 0; i < 5; i++)
            {
                rfadFirePoint[i] = firePoint[i];
            }
        }

        public void RFADShoot()
        {
            for (int i = 0; i < 5; i++)
            {
                Instantiate(rfadBulletPrefab, rfadFirePoint[i].position, rfadFirePoint[i].rotation);
            }
        }
    }

    [System.Serializable]
    public class LastBreath
    {
        private GameObject lbPrefab;
        private Transform lbFirePoint;

        public void lbInit(GameObject prefab, Transform firePoint)
        {
            lbPrefab = prefab;
            lbFirePoint = firePoint;
        }

        public void LBShoot()
        {
            Instantiate(lbPrefab, lbFirePoint.position, lbFirePoint.rotation);
        }
    }

    [System.Serializable]
    public class ForgottenSins
    {
        private GameObject fsPrefab;
        private Transform fsDropPoint;

        public void fsInit(GameObject prefab, Transform dropPoint)
        {
            fsPrefab = prefab;
            fsDropPoint = dropPoint;
        }
        public void FSPlace()
        {
            Instantiate(fsPrefab, fsDropPoint.position, fsDropPoint.rotation);
        }
    }

    //  Weapon 1
    public M3 m3 = new M3();

    //  Weapon 2
    public StingingTruths stingingTruths = new StingingTruths();

    //  Weapon 3
    public RequiemForADream requiemForADream = new RequiemForADream();

    //  Weapon 4
    public LastBreath lastBreath = new LastBreath();

    //  Weapon 5
    public ForgottenSins forgottenSins = new ForgottenSins();

    //  Impact Effect
    public GameObject impactEffect;

    //  Requiem For A Dream bullet prefab
    public GameObject rfadbulletPrefab;

    //  Last Breath prefab
    public GameObject lbPrefab;

    //  Forgotten Sins prefab
    public GameObject fsPrefab;

    //  FirePoint and Line Renderer Objects for M3 and Stinging Truths
    [SerializeField]
    private Transform[] firePoint = new Transform[5];
    [SerializeField]
    private LineRenderer[] lineRenderer = new LineRenderer[5];

    //  Shoot Button Handling Stuff
    private buttonHandler shootButtonHandler;
    private bool shootPressed;
    private bool justShot = false;

    //  Switch Button Handling Stuff
    private buttonHandler switchButtonHandler;
    private bool switchPressed;
    private bool switchJustPressed = false;

    //  Weapon Handling Stuff
    private bool[] weaponUnlocked = { true, true, true, true, true, false, false};
    private int weapon = 0;

    private void Start()
    {
        shootButtonHandler = GameObject.Find("ui").transform.Find("shoot").GetComponent<buttonHandler>();
        switchButtonHandler = GameObject.Find("ui").transform.Find("switch").GetComponent<buttonHandler>();

        //  Error Checking; this component should already be filled in the inspector
        if (firePoint[0] == null || firePoint[1] == null || firePoint[2] == null || firePoint[3] == null || firePoint[4] == null)
        {
            if (transform.Find("sprite") != null)
            {
                Transform tempSprite = transform.Find("sprite");

                if (tempSprite.transform.Find("firePoint1") != null && tempSprite.transform.Find("firePoint2") != null && tempSprite.transform.Find("firePoint3") != null && tempSprite.transform.Find("firePoint4") != null && tempSprite.transform.Find("firePoint5") != null)
                {
                    firePoint[0] = transform.Find("sprite").transform.Find("firePoint1");
                    firePoint[1] = transform.Find("sprite").transform.Find("firePoint2");
                    firePoint[2] = transform.Find("sprite").transform.Find("firePoint3");
                    firePoint[3] = transform.Find("sprite").transform.Find("firePoint4");
                    firePoint[4] = transform.Find("sprite").transform.Find("firePoint5");
                }
                else
                {
                    Debug.LogError("This object is missing one or more 'firePoint'(s).");
                    return;
                }
            }
            else
            {
                Debug.LogError("This object has no child 'sprite'.");
                return;
            }
        }

        //  Error Checking; this component should already be filled in the inspector
        if (lineRenderer[0] == null || lineRenderer[1] == null || lineRenderer[2] == null || lineRenderer[3] == null)
        {
            if (transform.Find("sprite") != null)
            {
                Transform tempSprite = transform.Find("sprite");

                if (tempSprite.transform.Find("line1") != null && tempSprite.transform.Find("line2") != null && tempSprite.transform.Find("line3") != null && tempSprite.transform.Find("line4") != null && tempSprite.transform.Find("line5") != null)
                {
                    Transform[] tempLineRend = new Transform[5];

                    tempLineRend[0] = tempSprite.transform.Find("line1");
                    tempLineRend[1] = tempSprite.transform.Find("line2");
                    tempLineRend[2] = tempSprite.transform.Find("line3");
                    tempLineRend[3] = tempSprite.transform.Find("line4");
                    tempLineRend[4] = tempSprite.transform.Find("line5");

                    if (tempLineRend[0].transform.GetComponent<LineRenderer>() != null && tempLineRend[1].transform.GetComponent<LineRenderer>() != null && tempLineRend[2].transform.GetComponent<LineRenderer>() != null && tempLineRend[3].transform.GetComponent<LineRenderer>() != null && tempLineRend[4].transform.GetComponent<LineRenderer>() != null)
                    {
                        lineRenderer[0] = tempLineRend[0].transform.GetComponent<LineRenderer>();
                        lineRenderer[1] = tempLineRend[1].transform.GetComponent<LineRenderer>();
                        lineRenderer[2] = tempLineRend[2].transform.GetComponent<LineRenderer>();
                        lineRenderer[3] = tempLineRend[3].transform.GetComponent<LineRenderer>();
                        lineRenderer[4] = tempLineRend[4].transform.GetComponent<LineRenderer>();

                    }
                    else
                    {
                        Debug.LogError("One or more 'line' (s) are missing 'LineRenderer'");
                        return;
                    }
                }
                else
                {
                    Debug.LogError("This object is missing one or more 'line'(s).");
                    return;
                }
            }
            else
            {
                Debug.LogError("This object has no child 'sprite'.");
                return;
            }
        }

        //  Checks whether Impact Effect prefab is fulfilled in inspector
        if (impactEffect != null)
        {
            m3.m3Init(impactEffect, firePoint[2], lineRenderer[2]);
            stingingTruths.stInit(impactEffect, firePoint[1], firePoint[3], lineRenderer[1], lineRenderer[3]);
        }
        else
        {
            Debug.LogError("impactEffect Prefab is missing!");
            return;
        }

        //  Checks whether Requiem For A Dream prefab is fulfilled in inspector
        if (rfadbulletPrefab != null)
        {
            requiemForADream.rfadInit(impactEffect, rfadbulletPrefab, firePoint);
        }
        else
        {
            Debug.LogError("Requiem For A Dream Bullet Prefab is missing!");
            return;
        }

        //  Checks whether Last Breath prefab is fulfilled in inspector
        if (lbPrefab != null)
        {
            lastBreath.lbInit(lbPrefab, firePoint[2]);
        }
        else
        {
            Debug.LogError("Last Breath Prefab is missing!");
            return;
        }

        //  Checks whether Forgotten Sins prefab is fulfilled in inspector
        if (fsPrefab != null)
        {
            forgottenSins.fsInit(fsPrefab, this.transform);
        }
        else
        {
            Debug.LogError("Forgotten Sins Prefab is missing!");
            return;
        }
    }

    private void Update()
    {
        shootPressed = shootButtonHandler.returnBool();
        switchPressed = switchButtonHandler.returnBool();

        //  Shoot Weapon
        if (shootPressed == true && !justShot)
        {
            justShot = true;
            StartCoroutine(shootWeapon(weapon));
        }

        //  Switch Weapons
        if (switchPressed && !switchJustPressed)
        {
            switchJustPressed = true;
            switchWeapons();
        }
        else if (!switchPressed)
        {
            switchJustPressed = false;
        }
    }

    private void switchWeapons()
    {
        if (weapon != 7)
        {
            if (weaponUnlocked[weapon + 1] == false)
            {
                weapon = 0;
            }
            else
            {
                weapon++;
            }
        }
        else
        {
            weapon = 0;
        }
    }

    private IEnumerator shootWeapon(int weaponChoice)
    {
        if (weaponChoice == 0 && weaponUnlocked[weaponChoice] == true)
        {
            m3.setColor();
            StartCoroutine(m3.M3Shoot());
            yield return new WaitForSeconds(0.25f);
            justShot = false;
        }
        else if (weaponChoice == 1 && weaponUnlocked[weaponChoice] == true)
        {
            stingingTruths.setColor();
            StartCoroutine(stingingTruths.STShoot());
            yield return new WaitForSeconds(0.07f);
            justShot = false;
        }
        else if (weaponChoice == 2 && weaponUnlocked[weaponChoice] == true)
        {
            requiemForADream.RFADShoot();
            yield return new WaitForSeconds(0.5f);
            justShot = false;
        }
        else if (weaponChoice == 3 && weaponUnlocked[weaponChoice] == true)
        {
            lastBreath.LBShoot();
            yield return new WaitForSeconds(1f);
            justShot = false;
        }
        else if (weaponChoice == 4 && weaponUnlocked[weaponChoice] == true)
        {
            forgottenSins.FSPlace();
            yield return new WaitForSeconds(1f);
            justShot = false;
        }
        justShot = false;
    }
}
