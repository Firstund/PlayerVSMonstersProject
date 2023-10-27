using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using UnityEngine;

public class ConnectionManager
{
    private readonly TcpListener tcpListener;

    private WebSocketController webSocketController = null;
    public WebSocketController WebSocketController
    {
        get
        {
            return webSocketController;
        }
    }

    private string serverID = "";
    public string ServerID
    {
        get
        {
            return serverID;
        }
    }

    public bool WebSocketControllerConnected
    {
        get
        {
            if (webSocketController != null)
            {
                return webSocketController.State == WebSocketSharp.WebSocketState.Open;
            }
            else
            {
                return false;
            }
        }
    }

    public ConnectionManager(string address, int port)
    {
        tcpListener = new TcpListener(IPAddress.Parse(address), port);
        tcpListener.Start();//

        tcpListener.BeginAcceptTcpClient(OnAcceptClient, null);

        Debug.Log("Server Started On " + address + " Port: " + port);
    }

    private void OnAcceptClient(IAsyncResult ar)
    {
        //if (WebSocketControllerConnected)
        //{
        //    Debug.Log("��Ʈ�ѷ� ���� �õ��� �־�����, �̹� ��Ʈ�ѷ��� ���ӵǾ��־ ���É�.");
        //    //return;
        //}

        TcpClient client = tcpListener.EndAcceptTcpClient(ar);
        webSocketController = new WebSocketController(client);

        tcpListener.BeginAcceptTcpClient(OnAcceptClient, null);

        serverID = webSocketController.GetUniqueID(true);

        //webSocketController.SendData(webSocketController.GetUniqueID(false).GetBytes(Encoding.UTF8), PayloadDataType.Text);
        //webSocketController.SendData(("event,log,normal,Success!").GetBytes(Encoding.UTF8), PayloadDataType.Text);
    }

    public void Dispose()
    {
        if (webSocketController != null)
        {
            webSocketController.Dispose();
        }
    }
}
