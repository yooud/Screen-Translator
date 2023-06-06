﻿namespace Screen_Translator.Helpers;

public static class StringHelper
{
    public static string Capitalize(string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;
        return char.ToUpper(str[0]) + str[1..];
    }
}