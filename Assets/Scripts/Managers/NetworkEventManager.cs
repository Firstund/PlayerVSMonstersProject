using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

//using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
//using System.Text;

using UnityEngine;
using WebSocketSharp;



public class NetworkEventManager : MonoSingleton<NetworkEventManager>
{
    private Dictionary<string, Action<List<string>>> eventDict = new();
    public Dictionary<string, Action<List<string>>> EventDict
    {
        get
        {
            return eventDict;
        }
    }

    public void AddEvent(string eventName, Action<List<string>> action)
    {
        if (eventDict.ContainsKey(eventName))
        {
            eventDict[eventName] += action;
        }
        else
        {
            eventDict.Add(eventName, action);
            eventDict[eventName] = action;
        }
    }

    public void SubEvent(string eventName, Action<List<string>> action)
    {
        if (eventDict.ContainsKey(eventName))
        {
            if (eventDict.Count > 1)
            {
                eventDict[eventName] -= action;
            }
            else
            {
                eventDict.Remove(eventName);
            }
        }
    }

    private void Awake()
    {
        AddEvent("log", (x) =>
        {
            string logContents = "ServerLog: " + x[1];
            switch (x[0])
            {
                case "normal":
                    {
                        Debug.Log(logContents);
                    }
                    break;
                case "warn":
                    {
                        Debug.LogWarning(logContents);
                    }
                    break;
                case "error":
                    {
                        Debug.LogError(logContents);
                    }
                    break;
            }
        });
    }

    private void Update()
    {

    }

    public void InvokeEvent(string eventName, List<string> strList)
    {
        eventDict[eventName]?.Invoke(strList);
    }

    //////////////////////////////////////////////////////////////////////////////////

    public void sendMessageToTarget(string message)
    {
        string msg = "fromServer," + message;

        socketSend(msg);
    }

    private void socketSend(string msg)
    {
        // 위의 함수에서 메세지의 타겟과 메세지 내용 등을 정리하면, 해당 함수에서 메세지를 보낼 수 있는 상태인지 확인 후 메세지를 보낸다.

        if (Server.Instance.ConnectionManager.WebSocketController.State == WebSocketState.Open)
        {
            Server.Instance.ConnectionManager.WebSocketController.SendData(msg.GetBytes(Encoding.UTF8), PayloadDataType.Text);
        }
        else
        {
            string debugTxt = "메세지 전송에 실패했습니다. 사유: ";

            switch (Server.Instance.ConnectionManager.WebSocketController.State)
            {
                case WebSocketState.Connecting:
                    {
                        debugTxt += "Server State is 'CONNECTING'";
                    }
                    break;
                case WebSocketState.Closed:
                    {
                        debugTxt += "Server State is 'CLOSED'";
                    }
                    break;
                case WebSocketState.Closing:
                    {
                        debugTxt += "Server State is 'CLOSING'";
                    }
                    break;
                default:
                    {
                        debugTxt += "'원인을 알 수 없는 사유'";
                    }
                    break;
            }

            Debug.LogWarning(debugTxt);
        }
    }
}
