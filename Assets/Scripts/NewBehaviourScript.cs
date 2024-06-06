using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using SimpleJSON;

public class NewBehaviourScript : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void GameReady(string msg);

    public TMP_Text BetAmount;
    public TMP_Text TotalAmount;
    public TMP_Text alertText;
    public TMP_Text EarnAmount;
    public Button disable_BET;
    public Button disable_increase;
    public Button disable_decrease;
    public static Globalinitial _global;
    public static ReceiveJsonObject apiform;

    // public static Globalinitial _global;
    // public static ReceiveJsonObject apiform;
    private int flag = 0;
    public TMP_Text mark1;
    public TMP_Text mark2;
    public TMP_Text Compare_Text;
    // public GameObject Alert;
    public GameObject[] player1_poker = new GameObject[3];
    public GameObject[] player2_poker = new GameObject[3];
    private float betAmount;
    public float totalAmount;
    public Texture[] Hard = new Texture[13];
    public Texture[] Daya = new Texture[13];
    public Texture[] BlackHard = new Texture[13];
    public Texture[] BlackDaya = new Texture[13];
    public Texture[] BackMark = new Texture[1];

    // Start is called before the first frame update
    BetPlayer _player;
    void Start()
    {
        betAmount = 10;
        BetAmount.text = betAmount.ToString("F2") + " $";
        _player = new BetPlayer();
        totalAmount = 100f;
        TotalAmount.text = totalAmount.ToString("F2");
#if UNITY_WEBGL == true && UNITY_EDITOR == false
        GameReady("Ready");
#endif

    }

    // Update is called once per frame
    void Update()
    {

    }
    // public void RequestToken(string data)
    // {
    //     JSONNode usersInfo = JSON.Parse(data);
    //     _player.token = usersInfo["token"];
    //     float i_balance = float.Parse(usersInfo["amount"]);
    //     totalAmount = i_balance;
    //     TotalAmount.text = totalAmount.ToString("F2") + "$";
    // }

    public void double_increase()
    {
        StartCoroutine(_double_increase());
    }
    IEnumerator _double_increase()
    {
        if (flag == 0)
        {
            disable_increase.interactable = false;
            if (totalAmount >= 2 * betAmount)
            {
                betAmount = 2 * betAmount;
            }
            else if (totalAmount == 0)
            {
                // StartCoroutine(Alert());
                // alertText.text = "Not Enough Balance!";
                yield return new WaitForSeconds(1.5f);
                disable_increase.interactable = true;
            }
            else
            {
                betAmount = totalAmount;
                // StartCoroutine(Alert());
                // alertText.text = "Maximum bet limit is " + totalAmount.ToString("F2") + " USD!";
                yield return new WaitForSeconds(1.5f);
                disable_increase.interactable = true;

            }
            BetAmount.text = betAmount.ToString("F2") + " $";
            disable_increase.interactable = true;
        }

    }
    public void double_decrease()
    {
        StartCoroutine(_double_decrease());
    }
    IEnumerator _double_decrease()
    {
        if (flag == 0)
        {
            disable_decrease.interactable = false;
            if (totalAmount >= betAmount / 2)
            {
                if (betAmount / 2 >= 10)
                {
                    betAmount = betAmount / 2;
                }
                else
                {
                    betAmount = 10;
                    // StartCoroutine(Alert());
                    // alertText.text = "Minimum bet limit is 10.00 USD!";
                    yield return new WaitForSeconds(1.5f);
                    disable_decrease.interactable = true;

                }
            }
            else if (totalAmount == 0)
            {
                betAmount = 10;
                // StartCoroutine(Alert());
                // alertText.text = "Minimum bet limit is 10.00 USD!";
                yield return new WaitForSeconds(1.5f);
                disable_decrease.interactable = true;
            }
            else
            {
                betAmount = totalAmount;
            }
            BetAmount.text = betAmount.ToString("F2") + " $";
            disable_decrease.interactable = true;
        }
    }
    IEnumerator StartGame()
    {
        disable_BET.interactable = false;
        flag = 1;
        if (betAmount <= 0)
        {
            betAmount = 10;
            // StartCoroutine(Alert());
            // alertText.text = "Not Enough Balance!";
            yield return new WaitForSeconds(1.5f);
            flag = 0;
            disable_BET.interactable = true;
        }
        else
        {
            mark1.text = "0";
            mark2.text = "0";
            Compare_Text.text = "=";
            for (int i = 0; i < 3; i++)
            {
                player1_poker[i].GetComponent<RawImage>().texture = BackMark[0];
                player2_poker[i].GetComponent<RawImage>().texture = BackMark[0];
            }
            if (totalAmount >= betAmount)
            {
                totalAmount -= betAmount;
                StartCoroutine(Server());
            }
            else
            {
                // StartCoroutine(Alert());
                // alertText.text = "Not Enough Balance!";
                yield return new WaitForSeconds(1.5f);
                flag = 0;
                disable_BET.interactable = true;
            }
        }
    }
    public void PlayGame()
    {
        StartCoroutine(StartGame());
    }

    private IEnumerator Server()
    {
        WWWForm form = new WWWForm();
        form.AddField("token", "_player.token");
        form.AddField("betAmount", betAmount.ToString("F2"));
        _global = new Globalinitial();
        UnityWebRequest www = UnityWebRequest.Post(_global.BaseUrl + "api/BET", form);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            // StartCoroutine(Alert());
            // alertText.text = "Cannot Find SERVER!";
            yield return new WaitForSeconds(1.5f);
            flag = 0;
            disable_BET.interactable = true;
        }
        else
        {
            yield return new WaitForSeconds(1);
            string strdata = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
            apiform = JsonUtility.FromJson<ReceiveJsonObject>(strdata);
            if (apiform.Message == "SUCCESS!")
            {
                isDirectionAnimation.isDirection = false;
                int length1 = apiform.pocker_1.Length;
                int length2 = apiform.pocker_2.Length;
                for (int i = 0; i < length1; i++)
                {
                    if (i % 2 != 0)
                    {
                        if (apiform.pocker_1[i] == 0)
                        {
                            player1_poker[(i - (i + 1) / 2)].GetComponent<RawImage>().texture = Hard[apiform.pocker_1[i - 1]];
                            continue;
                        }
                        else if (apiform.pocker_1[i] == 1)
                        {
                            player1_poker[(i - (i + 1) / 2)].GetComponent<RawImage>().texture = BlackDaya[apiform.pocker_1[i - 1]];
                            continue;
                        }
                        else if (apiform.pocker_1[i] == 2)
                        {
                            player1_poker[(i - (i + 1) / 2)].GetComponent<RawImage>().texture = BlackHard[apiform.pocker_1[i - 1]];
                            continue;
                        }
                        else if (apiform.pocker_1[i] == 3)
                        {
                            player1_poker[(i - (i + 1) / 2)].GetComponent<RawImage>().texture = Daya[apiform.pocker_1[i - 1]];
                            continue;
                        }
                    }
                }
                for (int i = 0; i < length2; i++)
                {
                    if (i % 2 != 0)
                    {
                        if (apiform.pocker_2[i] == 0)
                        {
                            player2_poker[(i - (i + 1) / 2)].GetComponent<RawImage>().texture = Hard[apiform.pocker_2[i - 1]];
                            continue;
                        }
                        else if (apiform.pocker_2[i] == 1)
                        {
                            player2_poker[(i - (i + 1) / 2)].GetComponent<RawImage>().texture = BlackDaya[apiform.pocker_2[i - 1]];
                            continue;
                        }
                        else if (apiform.pocker_2[i] == 2)
                        {
                            player2_poker[(i - (i + 1) / 2)].GetComponent<RawImage>().texture = BlackHard[apiform.pocker_2[i - 1]];
                            continue;
                        }
                        else if (apiform.pocker_2[i] == 3)
                        {
                            player2_poker[(i - (i + 1) / 2)].GetComponent<RawImage>().texture = Daya[apiform.pocker_2[i - 1]];
                            continue;
                        }
                    }
                }
                mark1.text = apiform.pocker_1_sum.ToString();
                mark2.text = apiform.pocker_2_sum.ToString();
                if (apiform.reward == 1)
                {
                    isDirectionAnimation.isDirection = false;
                    alertText.text = "DRAWE";
                    EarnAmount.text = "1.00X";
                    Compare_Text.text = "=";
                }
                else if (apiform.reward == 2)
                {
                    isDirectionAnimation.isDirection = false;
                    alertText.text = "WIN!";
                    EarnAmount.text = "2.00X";
                    Compare_Text.text = ">";
                }
                else
                {
                    isDirectionAnimation.isDirection = false;
                    alertText.text = "LOSE!";
                    EarnAmount.text = "0.00X";
                    Compare_Text.text = "<";
                }
                totalAmount += apiform.earnAmount;
                TotalAmount.text = totalAmount.ToString("F2") + " $";
                yield return new WaitForSeconds(1);
                isDirectionAnimation.isDirection = true;
                disable_BET.interactable = true;
                flag = 0;
            }
            else if (apiform.Message == "BET ERROR!")
            {
                // StartCoroutine(Alert());
                // alertText.text = "BET ERROR!";
                yield return new WaitForSeconds(1.5f);
                flag = 0;
                disable_BET.interactable = true;
            }
            else if (apiform.Message == "SERVER ERROR!")
            {
                // StartCoroutine(Alert());
                // alertText.text = "SERVER ERROR!";
                yield return new WaitForSeconds(1.5f);
                flag = 0;
                disable_BET.interactable = true;
            }
        }
    }


}

public class BetPlayer
{
    public string username;
    public string token;
}
