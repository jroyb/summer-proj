using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour {

    #region variables
    private static GameMaster gameMaster;
    public static GameMaster Instance { get { return gameMaster; } }

    [Header ("Display Cluster Texts")]
    public Text scoreText;
    public Text multiplierText;
    public Text livesText;

    [Header ("Notification")]
    public Transform UITransform;
    public GameObject notificationText;

    //  This one is used by GameMaster
    private string[] notificationStrings = new string[25];

    //  This one is used by PowerUp
    private string notificationString;
    public string NotificationString { get { return notificationString; } set { notificationString = value; } }

    [Header ("Upgrade UI Texts")]
    public Text[] upgradeTexts = new Text[25];

    [Header ("Game Over Components")]
    public GameObject gameOverUI;
    public Text gameOverText;
    public Text gameOverScoreText;

    [Header ("Prefabs")]
    public GameObject p1Respawn;

    //  Multiplier times
    private float timeOne;
    private float timeTwo;
    private float timeThree;
    private float timeFour;
    private float timeFive;

    //  Upgrade variables
    private int unlockNumber = 0;
    private int nextUnlock;
    private int firstStage;
    private int caseOne;
    private int caseTwo;
    private int caseThree;
    private int caseFour;
    private int caseFive;
    private int add;
    private int max;

    #region gameplay
    private int difficulty;
    private float score = 0;
    private int remainingLives = 3;
    private int multiplier = 1;
    private bool reduceMultiplierSwitch = true;
    #endregion
    #endregion

    #region initialization
    //  Singleton initialization
    private void Awake () {
        if (!Instance) {
            gameMaster = this;
        } else if (this != Instance) {
            Destroy (this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start () {
        difficulty = PlayerPrefs.GetInt ("difficulty");
        SetNotificationString ();
        SetMultiplierTimes ();
        SetUpgradeVariables ();
    }

    private void SetNotificationString () {
        notificationStrings[0] = "NEW WEAPON UNLOCKED: STINGING TRUTHS";
        notificationStrings[1] = "G3+: DOUBLE DAMAGE";
        notificationStrings[2] = "STINGING TRUTHS+: DOUBLE AMMO";
        notificationStrings[3] = "NEW WEAPON UNLOCKED: REQUIEM FOR A DREAM";
        notificationStrings[4] = "G3+: RAPID FIRE";
        notificationStrings[5] = "REQUIEM FOR A DREAM+: DOUBLE AMMO";
        notificationStrings[6] = "STINGING TRUTHS+: DOUBLE DAMAGE";
        notificationStrings[7] = "HEAL+: INSTANT START";
        notificationStrings[8] = "REQUIEM FOR A DREAM+: DOUBLE DAMAGE";
        notificationStrings[9] = "STINGING TRUTHS+: RAPID FIRE";
        notificationStrings[10] = "NEW WEAPON UNLOCKED: LAST BREATH";
        notificationStrings[11] = "STINGING TRUTHS+: QUAD AMMO";
        notificationStrings[12] = "REQUIEM FOR A DREAM+: RAPID FIRE";
        notificationStrings[13] = "LAST BREATH+: DOUBLE AMMO";
        notificationStrings[14] = "REQUIEM FOR A DREAM: QUAD AMMO";
        notificationStrings[15] = "HEAL+: RAPID RATE";
        notificationStrings[16] = "LAST BREATH+: RAPID FIRE";
        notificationStrings[17] = "REQUIEM FOR A DREAM+: KNOCKBACK INCREASED";
        notificationStrings[18] = "LAST BREATH+: RANGE INCREASED";
        notificationStrings[19] = "NEW WEAPON UNLOCKED: FORGOTTEN SINS";
        notificationStrings[20] = "FORGOTTEN SINS+: DOUBLE AMMO";
        notificationStrings[21] = "EXPLOSION+: DOUBLE DAMAGE";
        notificationStrings[22] = "FORGOTTEN SINS+: RAPID FIRE";
        notificationStrings[23] = "G3+: DEATH'S TOUCH";
        notificationStrings[24] = "FORGOTTEN SINS+: EXPLOSIVE VIRUS";
    }

    private void SetMultiplierTimes () {
        switch (difficulty) {
            case 1:
                timeOne = 4f;
                timeTwo = 2f;
                timeThree = 1f;
                timeFour = 0.25f;
                timeFive = 0.1f;
                break;
            case 2:
                timeOne = 3f;
                timeTwo = 1.5f;
                timeThree = 0.75f;
                timeFour = 0.5f;
                timeFive = 0.1f;
                break;
            case 3:
                timeOne = 2f;
                timeTwo = 1f;
                timeThree = 0.5f;
                timeFour = 0.25f;
                timeFive = 0.1f;
                break;
        }
    }

    private void SetUpgradeVariables () {
        switch (difficulty) {
            case 1:
                caseOne = 2;
                caseTwo = 4;
                caseThree = 8;
                caseFour = 15;
                caseFive = 25;
                firstStage = 25;
                nextUnlock = 35;
                add = 10;
                max = 225;
                break;
            case 2:
                caseOne = 3;
                caseTwo = 5;
                caseThree = 10;
                caseFour = 20;
                caseFive = 35;
                firstStage = 35;
                nextUnlock = 50;
                add = 15;
                max = 335;
                break;
            case 3:
                caseOne = 5;
                caseTwo = 10;
                caseThree = 20;
                caseFour = 35;
                caseFive = 50;
                firstStage = 50;
                nextUnlock = 75;
                add = 25;
                max = 550;
                break;
        }
    }
    #endregion

    #region spawning
    public void SpawnPlayer () {
        //  After every spawn decrease remainingLives
        remainingLives -= 1;

        UpdateLivesText (); //  Updates Remaining Lives on the UI

        if (remainingLives <= 0) {
            EndGame ();
            return;
        }

        // Somehow does not work if SpawnSequence coroutine is called directly from player script
        StartCoroutine (SpawnSequence ());
    }

    private IEnumerator SpawnSequence () {
        //  Wait 5 seconds before spawning
        yield return new WaitForSeconds (5f);

        //  Create a random position to spawn from in reference to the current camera position
        Transform camTransform = CameraFunctions.CameraTransform;
        Vector3 spawnPos = new Vector3 (camTransform.position.x + UnityEngine.Random.Range (-20f, 20f), camTransform.position.y + UnityEngine.Random.Range (-20f, 20f), camTransform.position.z + 1); // Z is offset since camera transform has a Z of -1

        //  Spawn...
        Instantiate (p1Respawn, spawnPos, camTransform.rotation);
    }
    #endregion

    #region update
    // Update is called once per frame
    void Update () {
        if (reduceMultiplierSwitch && multiplier > 1) {
            reduceMultiplierSwitch = false; //  This prevents the Reduce Multiplier coroutine from being called constantly before completing
            StartCoroutine (ReduceMultiplier ());
        }
    }

    #region score
    public void UpdateScore (int points) {
        //  Add to score
        score += points * multiplier;

        //  Prevents multiplier from going past 9999
        if (multiplier < 9999) {
            multiplier += 1;
        }

        //  For the serious players who want to see their scores past 999999999999
        if (score > 999999999999) {
            //  A convertion from float to int is needed or the float will automatically be rounded due to precision limitation.
            //  Similar conversions will be seen in the rest of the code
            Debug.Log ("Score: " + Convert.ToInt64 (score).ToString ());
        }
        if (multiplier > 999) {
            Debug.Log ("Multiplier: " + multiplier.ToString ());
        }

        Upgrade (); //  Upgrade the weapons
        UpdateScoreText (); //  Update the Score on the UI
        UpdateMultiplierText (); //  Update the Multiplier on the UI
    }
    #endregion

    #region multiplier
    private IEnumerator ReduceMultiplier () {
        if (multiplier < 50) {
            yield return (new WaitForSeconds (timeOne));
        } else if (multiplier < 100) {
            yield return (new WaitForSeconds (timeTwo));
        } else if (multiplier < 200) {
            yield return (new WaitForSeconds (timeThree));
        } else if (multiplier < 950) {
            yield return (new WaitForSeconds (timeFour));
        } else {
            yield return (new WaitForSeconds (timeFive));
        }
        multiplier -= 1;
        UpdateMultiplierText ();
        reduceMultiplierSwitch = true;
    }
    #endregion

    #region texts
    private void UpdateScoreText () {
        scoreText.text = (score > 999999999999) ? "999999999999" : Convert.ToInt64 (score).ToString ("000000000000.##");
    }

    private void UpdateMultiplierText () {
        multiplierText.text = (multiplier > 999) ? "x999" : "x" + multiplier.ToString ("000.##");
    }

    private void UpdateLivesText () {
        livesText.text = remainingLives.ToString ();
    }

    //  This function is called after every UpgradeData.SetUpgradesUnlocked(x, true);
    //  UpgradeData.SetUpgradesUnlocked(x, true) is not called repeatedly even after
    //  losing multiplier and regaining multiplier so there is no need to check if it
    //  has already been notified
    public void NotifyUpgrade (int i) {
        notificationString = notificationStrings[i]; //  Set the appropriate string for the notifcation
        Instantiate (notificationText, UITransform); //  Instantiate the notification
        //  In the upgrades menu change the color if unlocked
        //  Red for WEAPON UNLOCKS
        //  Cyan for WEAPON UPGRADES
        upgradeTexts[i].color = (i == 0 || i == 3 || i == 10 || i == 19) ? Color.red : Color.cyan;
    }
    #endregion
    #endregion

    #region upgrade
    private void Upgrade () {
        /*  Example where:

            caseOne = 2;
            caseTwo = 4;
            caseThree = 8;
            caseFour = 15;
            caseFive = 25;
            firstStage = 25;
            nextUnlock = 35;
            add = 10;
            max = 225;

            This implies the Upgrade Sequence:
            2 > 4 > 8 > 15 > 25 > 35 > 45 > 55 > 65..
            After 25 (firstStage), +10 is added subsequently henceforth until 225.
            However, below 25 is a non linear addition.
        */

        if (multiplier <= firstStage) {
            if (multiplier == caseOne && !UpgradeData.GetUpgradesUnlocked (0)) {
                UpgradeData.SetUpgradesUnlocked (0, true);
            } else if (multiplier == caseTwo && !UpgradeData.GetUpgradesUnlocked (1)) {
                UpgradeData.SetUpgradesUnlocked (1, true);
            } else if (multiplier == caseThree && !UpgradeData.GetUpgradesUnlocked (2)) {
                UpgradeData.SetUpgradesUnlocked (2, true);
            } else if (multiplier == caseFour && !UpgradeData.GetUpgradesUnlocked (3)) {
                UpgradeData.SetUpgradesUnlocked (3, true);
            } else if (multiplier == caseFive && !UpgradeData.GetUpgradesUnlocked (4)) {
                UpgradeData.SetUpgradesUnlocked (4, true);
            }
        } else if (multiplier >= nextUnlock && nextUnlock <= max) {
            if (!UpgradeData.GetUpgradesUnlocked (5 + unlockNumber)) {
                UpgradeData.SetUpgradesUnlocked (5 + unlockNumber, true);

                unlockNumber++;
                nextUnlock += add;
            }
        }
    }
    #endregion

    #region endgame
    private void EndGame () {
        Time.timeScale = 0f; //  Stop the game after the game has ended...

        switch (difficulty) {
            case 1:
                if (score > PlayerPrefs.GetFloat ("easyHighScore", 0)) {
                    PlayerPrefs.SetFloat ("easyHighScore", score);
                    gameOverText.text = "HIGH SCORE:";
                }
                break;
            case 2:
                if (score > PlayerPrefs.GetFloat ("normalHighScore", 0)) {
                    PlayerPrefs.SetFloat ("normalHighScore", score);
                    PlayGamesManager.AddScoreToLeaderboard (GPGSIds.leaderboard_global_high_score_normal, Convert.ToInt64 (score));
                    gameOverText.text = "HIGH SCORE:";
                }
                break;
            case 3:
                if (score > PlayerPrefs.GetFloat ("hellHighScore", 0)) {
                    PlayerPrefs.SetFloat ("hellHighScore", score);
                    PlayGamesManager.AddScoreToLeaderboard (GPGSIds.leaderboard_global_high_score_hell, Convert.ToInt64 (score));
                    gameOverText.text = "HIGH SCORE:";
                }
                break;
        }
        gameOverScoreText.text = (score > 999999999999) ? "999999999999" : Convert.ToInt64 (score).ToString ("000000000000.##");
        gameOverUI.SetActive (true);
    }
    #endregion
}
