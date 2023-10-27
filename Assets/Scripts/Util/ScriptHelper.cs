using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ScriptHelper
{
    public static int GetIndex(this char[] charArr, char value)
    {
        int idx = -1;

        for(int i = 0; i < charArr.Length; ++i)
        {
            if(charArr[i] == value)
            {
                idx = i;
            }
        }

        return idx;
    }

    public static int GetIndex(this string[] charArr, string value)
    {
        int idx = -1;

        for (int i = 0; i < charArr.Length; ++i)
        {
            if (charArr[i] == value)
            {
                idx = i;
            }
        }

        return idx;
    }

    public static byte[] GetBytes(this string str, Encoding encoding)
    {
        return encoding.GetBytes(str);
    }
}
