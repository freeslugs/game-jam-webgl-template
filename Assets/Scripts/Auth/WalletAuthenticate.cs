using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.SceneManagement;
using Near;
using UnityEngine.UI;

[Serializable]
public class RandomNumberResponse
{
    public RandomNumberResponseData data;
}

[Serializable]
public class RandomNumberResponseData
{
    public bool success;
    public string requestId;
    public string transactionHash;
    public string url;
    public string[] randomNumber;
}

public class WalletAuthenticate : MonoBehaviour
{
    //UI objects
    [SerializeField] private TextMeshProUGUI txtHeading;
    [SerializeField] private TextMeshProUGUI btnLoginText;
    [SerializeField] private TMP_Dropdown ddNetwork;
    [SerializeField] private TMP_InputField inputContract;
    [SerializeField] private TMP_InputField inputMethod;
    [SerializeField] private TMP_InputField inputArgs;
    [SerializeField] private Toggle toggleChange;
    [SerializeField] private TextMeshProUGUI txtContract;

    private UnityWebRequest request;

    /// <summary>
    /// Once authenticated with the Near wallet, the user is redirected back here.
    /// Near passes 2 perameters in the URL needed for the session (account_id and allKeys)
    /// </summary>

    #region Scene Methods

    private void Start()
    {
        //Set the network drop down
        CurrentNetwork();
        LoginStatus();
    }

    private void OnEnable()
    {
        ddNetwork.onValueChanged.AddListener(delegate { UpdateNetwork(); });
    }

    private void OnDisable()
    {
        ddNetwork.onValueChanged.RemoveListener(delegate { UpdateNetwork(); });
    }

    //Update dropdown selection at start
    private void CurrentNetwork()
    {
        if (PlayerPrefs.GetString("networkId") == "")
        {
            PlayerPrefs.SetString("networkId", ddNetwork.options[ddNetwork.value].text);
        }
        else
        {
            switch (PlayerPrefs.GetString("networkId"))
            {
                case "mainnet":
                    ddNetwork.SetValueWithoutNotify(1);
                    break;
                case "testnet":
                    ddNetwork.SetValueWithoutNotify(0);
                    break;
                case "betanet":
                    ddNetwork.SetValueWithoutNotify(2);
                    break;
            }
        }
    }

    //Update the network from any network dropdown change
    private void UpdateNetwork()
    {
        PlayerPrefs.SetString("networkId", ddNetwork.options[ddNetwork.value].text);
    }

    //Log messages to the heading label
    public void ChangeText(string message)
    {
        if (message == "")
        {
            message = "No Account";
        }
        txtHeading.text = message;
    }

    //Change the login button text and stored isLogged variable with each login/logout action
    public void ChangeLoginStatus(string status)
    {
        if (status == "true")
        {
            Near_API.isLoggedIn = true;
            btnLoginText.text = "Logout";
        }
        else
        {
            Near_API.isLoggedIn = false;
            btnLoginText.text = "Login";
        }
        ChangeText("Login Status: " + status);
    }

    //Update the stored accountId vriable
    public void UpdateAccountId(string accountId)
    {
        if (accountId == "")
        {
            accountId = "Zero";
        }
        txtHeading.text = accountId;
        Near_API.accountId = accountId;
    }

    //Load the RPC example scene
    public void RPCScene()
    {
        SceneManager.LoadScene("RPC");
    }

    //Display contract returned data
    public void DisplayContract(string json)
    {
        txtContract.text = json;
    }

    #endregion

    #region API Calls

    //Login to Near Wallet
    public void Login()
    {
        if (!Near_API.isLoggedIn)
        {
            Near_API.Login("", PlayerPrefs.GetString("networkId"));
        }
        else
        {
            Near_API.Logout(PlayerPrefs.GetString("networkId"));
        }
        LoginStatus();
    }

    public void CallVRF()
    {
        StartCoroutine(RequestRandomNumber());
    }


    private IEnumerator RequestRandomNumber()
    {
        ChangeText("Requesting random number from VRF.");

        string url = "https://0xcord.com/api/vrfv2/requestRandomNumber?network=fuji&numWords=1";
        string authToken = "xxxx";

        UnityWebRequest request = UnityWebRequest.Post(url, "");
        
        request.SetRequestHeader("Authorization", authToken);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            RandomNumberResponse response = JsonUtility.FromJson<RandomNumberResponse>(json);

            if (response != null && response.data != null && response.data.randomNumber != null && response.data.randomNumber.Length > 0)
            {
                string randomNumber = response.data.randomNumber[0];
                ChangeText("Random number: " + randomNumber);

                Debug.Log("requestId: " + response.data.requestId);
                Debug.Log("transactionHash: " + response.data.transactionHash);
                Debug.Log("url: " + response.data.url);
                Debug.Log("randomNumber: " + randomNumber);
            }
            else
            {
                Debug.Log("Failed to parse response");
            }
        }
        else
        {
            Debug.Log("Error: " + request.error);
        }        
    }

    //Ask Near for the login status
    public void LoginStatus()
    {
        Near_API.LoginStatus(PlayerPrefs.GetString("networkId"));
    }

    //Get the account ID
    public void AccountId()
    {
        Near_API.AccountId(PlayerPrefs.GetString("networkId"));
    }

    //Get the total account balance
    public void AccountBalance()
    {
        Near_API.AccountBalance(PlayerPrefs.GetString("networkId"), Near_API.accountId);
    }

    //Call contract
    public void CallContract()
    {
        Near_API.CallContract(inputContract.text, inputMethod.text, inputArgs.text, Near_API.accountId, PlayerPrefs.GetString("networkId"), toggleChange.isOn);
    }

    #endregion

}