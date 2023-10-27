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
        // ���� �Լ����� �޼����� Ÿ�ٰ� �޼��� ���� ���� �����ϸ�, �ش� �Լ����� �޼����� ���� �� �ִ� �������� Ȯ�� �� �޼����� ������.

        if (Server.Instance.ConnectionManager.WebSocketController.State == WebSocketState.Open)
        {
            Server.Instance.ConnectionManager.WebSocketController.SendData(msg.GetBytes(Encoding.UTF8), PayloadDataType.Text);
        }
        else
        {
            string debugTxt = "�޼��� ���ۿ� �����߽��ϴ�. ����: ";

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
                        debugTxt += "'������ �� �� ���� ����'";
                    }
                    break;
            }

            Debug.LogWarning(debugTxt);
        }
    }
}
