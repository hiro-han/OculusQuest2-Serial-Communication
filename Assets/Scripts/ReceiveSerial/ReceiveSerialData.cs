using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // string select
using System.Threading;
using UnityEngine;
using UnityEngine.UI; 

public class ReceiveSerialData : MonoBehaviour
{

    public Text text;
    public Text err_text;
    public Light light;
    private bool JSON_Data = true;
    private string errMsg_ = "";
    ReceivedData receivedData_ = null;
    AndroidJavaClass androidJavaClass_ = null;
    private object lockObject_ = new object();
    private Thread _receiveThread;

    // Start is called before the first frame update
    void Start()
    {
        text.alignment = TextAnchor.UpperLeft;
        err_text.enabled = false;

        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
        AndroidJavaObject intent = activity.Call<AndroidJavaObject>("getIntent");
        androidJavaClass_ = new AndroidJavaClass("com.hoho.android.usbserial.wrapper.UsbSerialWrapper");

        try {
            androidJavaClass_.CallStatic("Initialize", context, activity, intent);
            bool ret = androidJavaClass_.CallStatic<bool>("OpenDevice", 115200);
            if (!ret) {
                err_text.text = androidJavaClass_.CallStatic<string>("ErrorMsg");
            }
            err_text.text = androidJavaClass_.CallStatic<string>("ErrorMsg");
        } catch (Exception e) {
            err_text.text = "call error :" + e.Message;
        }

        _receiveThread = new Thread(_ReceiveData);
        _receiveThread.Start();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        lock(lockObject_) {
            if (receivedData_ != null) {
                text.text = JsonUtility.ToJson(receivedData_, true);
                light.intensity = receivedData_.brightness / 2.0f;
            }
            err_text.text = errMsg_;
        }
    }

    private void _ReceiveData()
    {
        AndroidJNI.AttachCurrentThread();
        bool connected = false;
        string data = "";
        string msg = "";

        while (true) 
        {
            msg = "";
            try {
                if (androidJavaClass_ == null) {
                    msg = "androidJavaClass_ is null.";
                    lock(lockObject_) {
                        errMsg_ = msg;
                    }
                    break;
                }

                connected = androidJavaClass_.CallStatic<bool>("Connected");
                if (connected) {
                    data = androidJavaClass_.CallStatic<string>("Read");

                    if (data.Length != 0) {
                        ReceivedData receivedData ;
                        receivedData = new ReceivedData();
                        if (JSON_Data) {
                            receivedData = JsonUtility.FromJson<ReceivedData>(data);
                        } else {
                            receivedData = new ReceivedData();

                            byte[] byteData = data.Select(x => (byte)x).ToArray();
                            int current_index = 0;
                            receivedData.temperature = PacketParser.ConvertToFloat(byteData, ref current_index);
                            receivedData.humidity = PacketParser.ConvertToFloat(byteData, ref current_index);
                            receivedData.brightness = PacketParser.ConvertToFloat(byteData, ref current_index);
                        }
                        lock(lockObject_) {
                            receivedData_ = receivedData;
                        }
                    }
                } else {
                    msg = "Not connected";
                    if (!androidJavaClass_.CallStatic<bool>("Connect")) {
                        msg = androidJavaClass_.CallStatic<string>("ErrorMsg");
                    }
                }
            } catch (Exception e) {
                msg = "Exception :" + e.Message;
            }
            lock(lockObject_) {
                errMsg_ = msg;
            }
            Thread.Sleep(100);
        }
    }
}
