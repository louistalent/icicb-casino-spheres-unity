using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using System;
// using System.Timers;
// using Random = UnityEngine.Random;
using TMPro;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
// using Newtonsoft.Json.Linq;
// using System.Linq;
// using UnityEngine.SceneManagement;
using SimpleJSON;


public class Main : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void GameReady(string msg);
    public TMP_InputField BetAmount;
    public TMP_Text TotalAmount;
    public TMP_Text result_time;
    public TMP_Text result_money;
    public Button disable_Play;
    public Button disable_increase;
    public Button disable_decrease;
    public static ReceiveJsonObject apiform;
    private int flag = 0;
    private float betAmount;
    public float totalAmount;
    public GameObject[] User_Ball = new GameObject[5];
    public GameObject[] Robot_Ball = new GameObject[5];
    public Texture[] Textures = new Texture[7];
    public RuntimeAnimatorController[] Front_Ball = new RuntimeAnimatorController[2];
    public RuntimeAnimatorController[] Stop_Front_Ball = new RuntimeAnimatorController[2];
    BetPlayer _player;
    private string BaseUrl = "http://83.136.219.243";
    void Start()
    {
        betAmount = 10.0f;
        BetAmount.text = betAmount.ToString("F2");
        _player = new BetPlayer();
#if UNITY_WEBGL == true && UNITY_EDITOR == false
        GameReady("Ready");
#endif
    }
    void Update()
    {

    }
    public void RequestToken(string data)
    {
        JSONNode usersInfo = JSON.Parse(data);
        _player.token = usersInfo["token"];
        totalAmount = float.Parse(usersInfo["amount"]);
        TotalAmount.text = totalAmount.ToString("F2");
    }
    public void double_increase()
    {
        if (flag == 0)
        {
            disable_increase.interactable = false;
            betAmount = float.Parse(BetAmount.text);
            if (totalAmount >= 2 * betAmount)
            {
                betAmount = 2 * betAmount;
            }
            else if (totalAmount < 10f)
            {
                result_time.text = "";
                result_money.text = "";
                result_time.text = "WARNING";
                result_money.text = "NOT ENOUGH BALANCE!";
                disable_increase.interactable = true;
            }
            else
            {
                betAmount = totalAmount;
                result_time.text = "";
                result_money.text = "";
                result_time.text = "MAXIMUM BET LIMIT";
                result_money.text = totalAmount.ToString("F2");
                disable_increase.interactable = true;

            }
            BetAmount.text = betAmount.ToString("F2");
            disable_increase.interactable = true;
        }

    }
    public void double_decrease()
    {
        if (flag == 0)
        {
            disable_decrease.interactable = false;
            betAmount = float.Parse(BetAmount.text);
            if (totalAmount >= betAmount / 2)
            {
                if (betAmount / 2 >= 10f)
                {
                    betAmount = betAmount / 2;
                }
                else
                {
                    betAmount = 10f;
                    result_time.text = "";
                    result_money.text = "";
                    result_time.text = "MINIMUM BET LIMIT";
                    result_money.text = "10.00";
                    disable_decrease.interactable = true;

                }
            }
            else if (totalAmount < 10f)
            {
                betAmount = 10f;
                result_time.text = "";
                result_money.text = "";
                result_time.text = "MINIMUM BET LIMIT";
                result_money.text = "10.00";
                disable_decrease.interactable = true;
            }
            else
            {
                betAmount = totalAmount;
            }
            BetAmount.text = betAmount.ToString("F2");
            disable_decrease.interactable = true;
        }
    }
    public void PlayGame()
    {
        disable_Play.interactable = false;
        result_time.text = "";
        result_money.text = "";
        flag = 1;
        betAmount = float.Parse(BetAmount.text);
        if (betAmount < 10)
        {
            betAmount = 10;
            result_time.text = "";
            result_money.text = "";
            result_time.text = "MINIMUM BET LIMIT";
            result_money.text = "10.00";
            flag = 0;
            disable_Play.interactable = true;
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                Robot_Ball[i].SetActive(false);
                User_Ball[i].SetActive(false);
            }
            if (totalAmount >= betAmount)
            {
                StartCoroutine(Server());
            }
            else
            {
                result_time.text = "";
                result_money.text = "";
                result_time.text = "WARNING";
                result_money.text = "NOT ENOUGH BALANCE!";
                flag = 0;
                disable_Play.interactable = true;
            }
        }
    }
    private IEnumerator Server()
    {
        betAmount = float.Parse(BetAmount.text);
        WWWForm form = new WWWForm();
        form.AddField("token", _player.token);
        form.AddField("betAmount", betAmount.ToString("F2"));

        UnityWebRequest www = UnityWebRequest.Post(BaseUrl + "/api/PlayGame", form);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            result_time.text = "";
            result_money.text = "";
            result_time.text = "ERROR";
            result_money.text = "CANNOT FIND SERVER!";
            flag = 0;
            disable_Play.interactable = true;
        }
        else
        {
            string strdata = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
            apiform = JsonUtility.FromJson<ReceiveJsonObject>(strdata);
            if (apiform.Message == "SUCCESS!")
            {
                for (int i = 0; i < 5; i++)
                {
                    Robot_Ball[i].GetComponent<Animator>().runtimeAnimatorController = Stop_Front_Ball[0];
                    User_Ball[i].GetComponent<Animator>().runtimeAnimatorController = Stop_Front_Ball[1];
                    yield return new WaitForSeconds(0.5f);
                    if (i == 0)
                    {
                        Robot_Ball[i].SetActive(true);
                        Robot_Ball[i].GetComponent<RawImage>().texture = Textures[apiform.Robot_color[i]];
                        Robot_Ball[i].GetComponent<RawImage>().color = Color.white;
                    }
                    else
                    {
                        for (int j = 0; j < i; j++)
                        {
                            if (apiform.Robot_color[j] == apiform.Robot_color[i])
                            {
                                Robot_Ball[i].SetActive(true);
                                Robot_Ball[i].GetComponent<RawImage>().texture = Textures[apiform.Robot_color[i]];
                                Robot_Ball[i].GetComponent<RawImage>().color = Color.white;
                                Robot_Ball[i].GetComponent<Animator>().runtimeAnimatorController = Front_Ball[0];
                                Robot_Ball[j].GetComponent<Animator>().runtimeAnimatorController = Front_Ball[0];
                            }
                            else if (apiform.Robot_color[j] != apiform.Robot_color[i])
                            {
                                Robot_Ball[i].SetActive(true);
                                Robot_Ball[i].GetComponent<RawImage>().texture = Textures[apiform.Robot_color[i]];
                                Robot_Ball[i].GetComponent<RawImage>().color = Color.white;
                            }
                        }
                    }
                    if (i == 0)
                    {

                        User_Ball[i].SetActive(true);
                        User_Ball[i].GetComponent<RawImage>().texture = Textures[apiform.User_color[i]];
                        User_Ball[i].GetComponent<RawImage>().color = Color.white;
                    }
                    else
                    {
                        for (int j = 0; j < i; j++)
                        {
                            if (apiform.User_color[j] == apiform.User_color[i])
                            {
                                User_Ball[i].SetActive(true);
                                User_Ball[i].GetComponent<RawImage>().texture = Textures[apiform.User_color[i]];
                                User_Ball[i].GetComponent<RawImage>().color = Color.white;
                                User_Ball[i].GetComponent<Animator>().runtimeAnimatorController = Front_Ball[1];
                                User_Ball[j].GetComponent<Animator>().runtimeAnimatorController = Front_Ball[1];
                            }
                            else if (apiform.User_color[j] != apiform.User_color[i])
                            {
                                User_Ball[i].SetActive(true);
                                User_Ball[i].GetComponent<RawImage>().texture = Textures[apiform.User_color[i]];
                                User_Ball[i].GetComponent<RawImage>().color = Color.white;
                            }
                        }
                    }
                }
                yield return new WaitForSeconds(0.5f);
                if (apiform.reward == 1)
                {
                    result_time.text = "1.00 X";
                    float rewardMoney = betAmount * 1;
                    result_money.text = rewardMoney.ToString("F2");
                }
                else if (apiform.reward == 2)
                {
                    result_time.text = "2.00 X";
                    float rewardMoney = betAmount * 2;
                    result_money.text = rewardMoney.ToString("F2");
                }
                else if (apiform.reward == 3)
                {
                    result_time.text = "3.00 X";
                    float rewardMoney = betAmount * 3;
                    result_money.text = rewardMoney.ToString("F2");
                }
                else if (apiform.reward == 4)
                {
                    result_time.text = "4.00 X";
                    float rewardMoney = betAmount * 4;
                    result_money.text = rewardMoney.ToString("F2");
                }
                else if (apiform.reward == 6)
                {
                    result_time.text = "6.00 X";
                    float rewardMoney = betAmount * 6;
                    result_money.text = rewardMoney.ToString("F2");
                }
                else if (apiform.reward == 5)
                {
                    result_time.text = "5.00 X";
                    float rewardMoney = betAmount * 5;
                    result_money.text = rewardMoney.ToString("F2");
                }
                else if (apiform.reward == 0)
                {
                    result_time.text = "0.00 X";
                    float rewardMoney = betAmount * 0;
                    result_money.text = rewardMoney.ToString("F2");
                }
                totalAmount += apiform.earnAmount;
                TotalAmount.text = totalAmount.ToString("F2");
                disable_Play.interactable = true;
                flag = 0;
            }
            else if (apiform.Message == "BET ERROR!")
            {
                result_time.text = "";
                result_money.text = "";
                result_time.text = "ERROR";
                result_money.text = "BET ERROR!";
                flag = 0;
                disable_Play.interactable = true;
            }
            else if (apiform.Message == "SERVER ERROR!")
            {
                result_time.text = "";
                result_money.text = "";
                result_time.text = "ERROR";
                result_money.text = "SERVER ERROR!";
                flag = 0;
                disable_Play.interactable = true;
            }
        }
    }
}
public class BetPlayer
{
    public string token;
}
