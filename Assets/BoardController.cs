#pragma warning disable IDE0051
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using TMPro;

public class BoardController : MonoBehaviour
{
    [SerializeField] ParticleSystem forward;
    [SerializeField] ParticleSystem back;
    [SerializeField] float torqueAmount = 5;
    [SerializeField] float torque;
    [SerializeField] ParticleSystem forwardSpinEffect;
    [SerializeField] ParticleSystem backwardSpinEffect;
    public AudioSource swingAudio;
    public AudioSource slidingAudio;
    public ParticleSystem bigScoreEffect;
    float tempTorque;
    float maxTorqueAmount = 20f;
    float hAxis;
    float count;
    float maxCount = 3;
    float countRatio;

    public float heightDist;
    Rigidbody2D rb2d;
    public bool isJump = false;
    public LayerMask layerMask;
    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        torque = torqueAmount;
        StartCoroutine(HeightCheck());
        StartCoroutine(SpiningCheck());
    }

    // Update is called once per frame
    void Update()
    {
        #region 테스트용 드로우레이
#if false //테스트용

        Debug.DrawRay(transform.position, transform.up * 10, Color.red);
        Debug.DrawRay(transform.position, Vector3.up * 10, Color.blue);
        Debug.DrawRay(transform.position, /*transform.up + */new Vector3(Mathf.Cos((90 + 90) * Mathf.Deg2Rad), Mathf.Sin((90 + 90) * Mathf.Deg2Rad), 0f) * 20, Color.white);
        Debug.DrawRay(transform.position, /*transform.up + */new Vector3(Mathf.Cos((90 - 90) * Mathf.Deg2Rad), Mathf.Sin((90 - 90) * Mathf.Deg2Rad), 0f) * 20, Color.white);

#endif
        #endregion

        playerDot = Vector3.Dot(Vector3.up, transform.up);
        Vector3 cross = Vector3.Cross(transform.up, Vector3.up);

        //Debug.DrawRay(transform.position, transform.rotation * new Vector3(Mathf.Cos((90 + radarAngle) * Mathf.Deg2Rad), 0f, Mathf.Sin((90 + radarAngle) * Mathf.Deg2Rad)) * 20f, color, 0.03f);
        hAxis = -Input.GetAxisRaw("Horizontal");

        #region Rotate & Jump
        if (hAxis != 0)
        {
            if (count < maxCount)
            {
                countRatio = count / maxCount;
                count += Time.deltaTime;
                tempTorque = Mathf.Lerp(torqueAmount, maxTorqueAmount, countRatio);
            }

            if (isJump)
            {

                rb2d.AddTorque(hAxis * torque * 140 * Time.deltaTime);
            }
        }

        if (!isJump && Input.GetKeyDown(KeyCode.Space))
        {
            rb2d.AddForce(new Vector2(0, 200));
        }
        #endregion

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        slidingAudio.Play();
        count = 0;
        torque = torqueAmount;
        tempTorque = torque;
        isJump = false;
        if (GameManager.GetInstance().isFinish)
        {
            GameManager.GetInstance().Restart();
        }

        #region 셋스코어, 변수초기화
        if (forwardSpin > 0 || backwardSpin > 0)
        {
            GameManager.GetInstance().SetScore(forwardSpin, backwardSpin, changeSpin);
            forwardSpin = 0;
            backwardSpin = 0;
            changeSpin = 0;
            previousSpinState = EPreviousSpinState.NOT;
        }
        #endregion

    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        slidingAudio.Pause();
        torque = tempTorque;
        isJump = true;
    }


    RaycastHit2D hit;
    IEnumerator HeightCheck()
    {
        while (true)
        {
            hit = Physics2D.Raycast(transform.position, -Vector2.up, 500, layerMask);
            // Debug.DrawRay(transform.position, -Vector2.up * 300);
            heightDist = hit.distance;
            yield return new WaitWhile(() => !isJump);
        }
    }
    Vector3 playerCross = Vector3.zero;
    bool isSpinForward = true;
    bool isPreviousSpinForward = true;
    enum EPreviousSpinState
    {
        NOT,
        FORWARD,
        BACKWARD
    }
    EPreviousSpinState previousSpinState = EPreviousSpinState.NOT;
    bool isSpin = false;
    float playerDot = 0f;
    float forwardSpin = 0;
    float backwardSpin = 0;
    float changeSpin = 0;
    int spinCheckAngle = 90;
    IEnumerator SpiningCheck()
    {
        yield return new WaitWhile(() => playerDot > Mathf.Cos(spinCheckAngle * Mathf.Deg2Rad));

        while (true)
        {

            playerCross = Vector3.Cross(transform.up, Vector3.up);

            if (playerCross.z > 0)
            {
                isSpinForward = true;
            }
            else
            {
                isSpinForward = false;
            }
            while (isJump)
            {

                if (playerDot > Mathf.Cos(spinCheckAngle * Mathf.Deg2Rad))
                {
                    playerCross = Vector3.Cross(transform.up, Vector3.up);

                    if (playerCross.z > 0 && isSpinForward || playerCross.z < 0 && !isSpinForward)
                    {
                        break;
                    }
                    else if (playerCross.z < 0 && isSpinForward)
                    {
                        if (previousSpinState == EPreviousSpinState.BACKWARD)
                        {
                            changeSpin++;
                        }
                        forwardSpin++;
                        previousSpinState = EPreviousSpinState.FORWARD;
                        forwardSpinEffect.Play();
                        swingAudio.Play();
                        break;
                    }
                    else if (playerCross.z > 0 && !isSpinForward)
                    {
                        if (previousSpinState == EPreviousSpinState.FORWARD)
                        {
                            changeSpin++;
                        }
                        backwardSpin++;
                        previousSpinState = EPreviousSpinState.BACKWARD;
                        forwardSpinEffect.Play();
                        swingAudio.Play();
                        break;
                    }
                }
                yield return null;
            }

            Debug.Log($"F: {forwardSpin}, B: {backwardSpin}, C: {changeSpin}");
            yield return new WaitWhile(() => playerDot > Mathf.Cos(spinCheckAngle * Mathf.Deg2Rad));
        }

    }

}

public class GameManager : MonoBehaviour
{
    float score = 0f;
    public TMP_Text scoreText;
    public BoardController boardController;
    [InitializeOnEnterPlayMode]
    static void OnEnterPlaymodeInEditor(EnterPlayModeOptions options)
    {
        Debug.Log("Entering PlayMode");

        if (options.HasFlag(EnterPlayModeOptions.DisableDomainReload))
        {
            instance = null;
        }
    }

    public static GameManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<GameManager>();

            if (instance == null)
            {
                GameObject container = new GameObject("Game Manager");

                instance = container.AddComponent<GameManager>();
            }
        }
        return instance;
    }

    static GameManager instance;
    public bool isFinish = false;

    private void Awake()
    {
        GetInstance();
    }

    void Start()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    public void Restart()
    {
        Invoke("ReloadScene", 1.2f);
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(0);
    }

    public void SetScore(float forwardSpinCount, float backSpinCount, float changeCount)
    {
        float tempScore = (forwardSpinCount + backSpinCount) * 100f * (changeCount + 1);

        if (tempScore > 500)
        {
            boardController.bigScoreEffect.Play();
        }
        score += tempScore;

        scoreText.text = $"Score: {score}";
    }


}
