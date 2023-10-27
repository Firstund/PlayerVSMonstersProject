using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Server : MonoSingleton<Server>
{
    private ConnectionManager connectionManager;
    public ConnectionManager ConnectionManager
    {
        get
        {
            if (connectionManager == null)
            {
                connectionManager = new ConnectionManager("127.0.0.1", 8081);
            }
            return connectionManager;
        }
    }
    //WebSocketController webSocketController = new WebSocketController();

    private Action onWebSocketConnected = () =>
    {
        Debug.Log("WebSocketConnected!");
    };
    public Action OnWebSocketConnected
    {
        get
        {
            return onWebSocketConnected;
        }

        set
        {
            onWebSocketConnected = value;
        }
    }

    private Action onWebSocketClosed = () =>
    {
        Debug.Log("WebSocketClosed!");
    };
    public Action OnWebSocketClosed
    {
        get
        {
            return onWebSocketClosed;
        }

        set
        {
            onWebSocketClosed = value;
        }
    }

    private bool prevWebSocketConnectedValue = false;
    void Start()
    {
        if (connectionManager == null)
        {
            connectionManager = new ConnectionManager("127.0.0.1", 8081);
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (connectionManager.WebSocketController != null)
            {
                Debug.Log(connectionManager.WebSocketController.State);
            }
            else
            {
                Debug.Log(WebSocketSharp.WebSocketState.Closed);
            }
        }

        CheckWebSocketConnected();
    }

    private void CheckWebSocketConnected()
    {
        if (prevWebSocketConnectedValue != connectionManager.WebSocketControllerConnected)
        {
            if (!prevWebSocketConnectedValue && connectionManager.WebSocketControllerConnected)
            {
                onWebSocketConnected.Invoke();
            }
            else if (prevWebSocketConnectedValue && !connectionManager.WebSocketControllerConnected)
            {
                onWebSocketClosed.Invoke();
            }
        }

        prevWebSocketConnectedValue = connectionManager.WebSocketControllerConnected;
    }

    private void OnApplicationQuit()
    {
        connectionManager.Dispose();
    }
}
