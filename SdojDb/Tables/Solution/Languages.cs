//------------------------------------------------------------------------------
// <copyright file="CSSqlUserDefinedType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Runtime.InteropServices;


[Serializable]
[Microsoft.SqlServer.Server.SqlUserDefinedType(Format.Native)]
public struct LanguageUdt: INullable
{
    public override string ToString()
    {
        return ((Languages)(_Languages)).ToString();
    }
    
    public bool IsNull
    {
        get
        {
            // 在此处放置代码
            return _IsNull;
        }
    }
    
    public static LanguageUdt Null
    {
        get
        {
            var h = new LanguageUdt();
            h._IsNull = true;
            return h;
        }
    }
    
    public static LanguageUdt Parse(SqlString s)
    {
        if (s.IsNull)
            return Null;

        var u = new LanguageUdt();
        u._Languages = (byte)Enum.Parse(typeof(Languages), s.Value, true);
        u._IsNull = false;

        return u;
    }
    
    public byte _Languages;
 
    private bool _IsNull;
}

public enum Languages : byte
{
    CSharp = 1, 
    VisualBasic = 2, 
    Cpp = 3, 
    Java = 4, 
}