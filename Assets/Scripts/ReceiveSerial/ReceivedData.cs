using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ReceivedData
{
    public float temperature = 0.0f;
    public float humidity = 0.0f;
    public float brightness = 1.0f;

    public string GetText()
    {
        return "Temperature: " + temperature.ToString() + "\nHumidity: " + humidity.ToString() + "\nBrightness: " + brightness.ToString() ;
    }
}
