using System.Collections;
using System.Collections.Generic;
using System.Linq; // string select
using UnityEngine;
using UnityEngine.UI; 

public class ReceiveSerialData : MonoBehaviour
{

    private NativeUart nu;
    private object lockBuffer = new object();
    private string buffer = "";
    private string serialDeviceList;
    private string serialStatus = "";
    public Text text;
    public Light light;
    private bool JSON_Data = true;

    // Start is called before the first frame update
    void Start()
    {
        nu = NativeUart.Instance;
        nu.OnUartState += SerialState;
        nu.OnUartDeviceList += SerialDeviceList;
        nu.OnUartMessageRead += SerialMessageReceived;
        nu.OnUartMessageReadLine += SerialMessageReceivedLine;
        nu.Init();

        nu.Connection(115200);
    }

    // Update is called once per frame
    void Update()
    {
        lock(lockBuffer)
        {
            int length = buffer.Length;
            if (length != 0)
            {
                ReceivedData receivedData;
                if (JSON_Data) {
                    receivedData = JsonUtility.FromJson<ReceivedData>(buffer);
                } else {
                    receivedData = new ReceivedData();

                    byte[] byteData = buffer.Select(x => (byte)x).ToArray();
                    int current_index = 0;
                    receivedData.temperature = PacketParser.ConvertToFloat(byteData, ref current_index);
                    receivedData.humidity = PacketParser.ConvertToFloat(byteData, ref current_index);
                    receivedData.brightness = PacketParser.ConvertToFloat(byteData, ref current_index);
                }

                text.text = receivedData.GetText();
                light.intensity = receivedData.brightness / 2.5f;
            }
            // text.text = serialStatus + ", " + serialDeviceList;
        }
    }

    public void SerialState(string msg){
        serialStatus = msg;
    }

    public void SerialDeviceList(string msg){
        serialDeviceList = msg;
    }

    public void SerialMessageReceived(string msg){
        lock(lockBuffer)
        {
            buffer = msg;
        }
    }

    public void SerialMessageReceivedLine(string msg){
        // lock(lockBuffer)
        // {
        //     buffer = msg;
        // }
    }
}
