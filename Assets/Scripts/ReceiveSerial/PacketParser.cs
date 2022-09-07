using System;

public class PacketParser {
    public static int ConvertToInt(byte [] src, ref int current_index) {
        int ret = BitConverter.ToInt32(src, current_index);
        current_index += 4;
        return ret;
    }

    public static float ConvertToFloat(byte [] src, ref int current_index) {
        float ret = BitConverter.ToSingle(src, current_index);
        current_index += 4;
        return ret;
    }
}