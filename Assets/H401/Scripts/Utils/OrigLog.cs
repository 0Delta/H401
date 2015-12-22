using System.Collections;

abstract public class OrigLog
{
    static public string ToString(BitArray array)
    {
        string[] str = new string[1];
        for(int i = 0 ; i < array.Count ; i ++)
            str[0] += array[i] ? "1" : "0";
            
        return str[0];
    }
}
