using System;
using System.Net;
using System.Net.Sockets;

using System.Text;
using System.Text.RegularExpressions;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using WebSocketSharp;

public enum PayloadDataType
{
    Unkown = -1,
    Continuation = 0,
    Text = 1,
    Binary = 2,
    ConnectionClose = 8,
    Ping = 9,
    Pong = 10
}

public class WebSocketController
{
    public WebSocketState State { get; private set; } = WebSocketState.Closed;

    private readonly TcpClient targetClient;
    private readonly NetworkStream messageStream;
    private readonly byte[] dataBuffer = new byte[1024];

    private Dictionary<string, Action<List<string>>> EventDict
    {
        get
        {
            return NetworkEventManager.Instance.EventDict;
        }
    }

    public WebSocketController(TcpClient tcpClient)
    {
        State = WebSocketState.Connecting;

        targetClient = tcpClient;
        messageStream = targetClient.GetStream();
        messageStream.BeginRead(dataBuffer, 0, dataBuffer.Length, OnReadData, null);
    }

    private void OnReadData(IAsyncResult ar)
    {
        int size = messageStream.EndRead(ar);

        byte[] httpRequestRaw = new byte[7];    //HTTP request method는 7자리를 넘지 않는다.
        //GET만 확인하면 되므로 new byte[3]해도 상관없음
        Array.Copy(dataBuffer, httpRequestRaw, httpRequestRaw.Length);
        //Debug.Log(httpRequestRaw);
        string httpRequest = Encoding.UTF8.GetString(httpRequestRaw);
        //string str = "";
        //for (int i = 0; i < httpRequestRaw.Length; ++i)
        //{
        //    str += httpRequestRaw[i] + " ";
        //}

        //Debug.Log(str);

        //Debug.Log(Convert.ToBase64String(httpRequestRaw));

        //GET 요청인지 여부 확인
        if (Regex.IsMatch(httpRequest, "^GET", RegexOptions.IgnoreCase))
        {
            HandshakeToClient(size);        // 연결 요청에 대한 응답
            State = WebSocketState.Open;	// 응답이 성공하여 연결 중으로 상태 전환

            //Debug.Log("Message Received");
        }

        messageStream.BeginRead(dataBuffer, 0, dataBuffer.Length, OnReadData, null);

        ProcessClientRequest(size);

    }

    private void HandshakeToClient(int dataSize)
    {
        string raw = Encoding.UTF8.GetString(dataBuffer);

        string swk = Regex.Match(raw, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
        string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
        string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

        byte[] response = Encoding.UTF8.GetBytes(
            "HTTP/1.1 101 Switching Protocols\r\n" +
            "Connection: Upgrade\r\n" +
            "Upgrade: websocket\r\n" +
            "Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");

        messageStream.Write(response, 0, response.Length);
    }

    public string GetUniqueID(bool isServer)
    {
        string id = "";

        Func<string> getUniqueNum = () =>
        {
            return Math.Floor((1 + UnityEngine.Random.Range(0f, 1f)) * 0x10000).ToString();
        };

        id = (isServer ? "Server" : "Client") + getUniqueNum() + getUniqueNum() + "-" + getUniqueNum() + getUniqueNum() + "-" + getUniqueNum() + getUniqueNum();

        return id;
    }

    private bool ProcessClientRequest(int dataSize)
    {
        bool fin = (dataBuffer[0] & 0b10000000) != 0;
        bool mask = (dataBuffer[1] & 0b10000000) != 0;
        PayloadDataType opcode = (PayloadDataType)(dataBuffer[0] & 0b00001111);

        int msglen = dataBuffer[1] - 128;
        int offset = 2;
        if (msglen == 126)
        {
            msglen = BitConverter.ToInt16(new byte[] { dataBuffer[3], dataBuffer[2] });
            offset = 4;
        }
        else if (msglen == 127)
        {
            Console.WriteLine("Error: over int16 size");
            Debug.Log("Error: over int16 size");
            return true;
        }

        if (mask)
        {
            byte[] decoded = new byte[msglen];
            byte[] masks = new byte[4] { dataBuffer[offset], dataBuffer[offset + 1], dataBuffer[offset + 2], dataBuffer[offset + 3] };
            offset += 4;

            for (int i = 0; i < msglen; i++)
            {
                decoded[i] = (byte)(dataBuffer[offset + i] ^ masks[i % 4]);
            }

            switch (opcode)
            {
                case PayloadDataType.Text:
                    {
                        SendData(("event,log,normal,Success!").GetBytes(Encoding.UTF8), PayloadDataType.Text);
                    }
                    break;
                case PayloadDataType.Binary:
                    //Binary는 아무 동작 없음
                    break;
                case PayloadDataType.ConnectionClose:
                    //받은 요청이 서버에서 보낸 요청에 대한 응답이 아닌 경우에만 실행
                    if (State != WebSocketState.Closing)
                    {
                        SendCloseRequest(1000, "Graceful Close");
                    }
                    State = WebSocketState.Closed;

                    Dispose();		// 소켓 닫음
                    return false;	// 더 이상 메시지를 수신하지 않음
                default:
                    Console.WriteLine("Unknown Data Type");
                    Debug.Log("Unknown Data Type");
                    break;
            }

            Console.WriteLine(Encoding.UTF8.GetString(decoded));
            //Debug.Log(Encoding.UTF8.GetString(decoded));

            OnMessage(Encoding.UTF8.GetString(decoded));
        }
        else
        {
            Console.WriteLine("Error: Mask bit not valid");
            Debug.Log("Error: Mask bit not valid");
        }

        return true;
    }

    private void OnMessage(string message)
    {
        string[] strArr = message.Split(',');

        string dataType = strArr[0];
        string data = strArr[1];

        List<string> others = new();

        for (int i = 2; i < strArr.Length; ++i)
        {
            others.Add(strArr[i]);
        }

        //Debug.Log("MessageReceived! dataType: " + dataType + " " + data);

        switch (dataType)
        {
            case "event":
                {
                    //Debug.Log("Event!");
                    //EventDict[data]?.Invoke(others);
                    NetworkEventManager.Instance.InvokeEvent(data, others);
                }
                break;
        }
    }

    public void SendData(byte[] data, PayloadDataType opcode)
    {
        byte[] sendData;
        BitArray firstByte = new BitArray(new bool[]
        {
            opcode == PayloadDataType.Text || opcode == PayloadDataType.Ping,
            opcode == PayloadDataType.Binary || opcode == PayloadDataType.Pong,
            false,
            opcode == PayloadDataType.ConnectionClose || opcode == PayloadDataType.Ping || opcode == PayloadDataType.Pong,
            false, //RSV3
            false, //RSV2
            false, //RSV1
            true,  //Fin
        });

        if (data.Length < 126)
        {
            sendData = new byte[data.Length + 2];
            firstByte.CopyTo(sendData, 0);
            sendData[1] = (byte)data.Length;
            data.CopyTo(sendData, 2);
        }
        else
        {
            sendData = new byte[data.Length + 4];
            firstByte.CopyTo(sendData, 0);
            sendData[1] = 126;
            byte[] lengthData = BitConverter.GetBytes((ushort)data.Length);
            Array.Copy(lengthData, 0, sendData, 2, 2);
            data.CopyTo(sendData, 4);
        }

        messageStream.Write(sendData, 0, sendData.Length);
    }

    public void SendCloseRequest(ushort code, string reason)
    {
        byte[] closeReq = new byte[2 + reason.Length];
        BitConverter.GetBytes(code).CopyTo(closeReq, 0);
        byte temp = closeReq[0];
        closeReq[0] = closeReq[1];
        closeReq[1] = temp;

        Encoding.UTF8.GetBytes(reason).CopyTo(closeReq, 2);
        SendData(closeReq, PayloadDataType.ConnectionClose);
    }

    public void Dispose()
    {
        targetClient.Close();
        targetClient.Dispose();
    }
}
