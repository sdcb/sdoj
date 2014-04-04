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


[Serializable]
[Microsoft.SqlServer.Server.SqlUserDefinedType(Format.Native)]
public struct SolutionStatusUdt: INullable
{    
    public bool IsNull
    {
        get
        {
            // 在此处放置代码
            return _IsNull;
        }
    }
    
    public static SolutionStatusUdt Null
    {
        get
        {
            var h = new SolutionStatusUdt();
            h._IsNull = true;
            return h;
        }
    }
    
    public static SolutionStatusUdt Parse(SqlString s)
    {
        if (s.IsNull) return Null;
        var u = new SolutionStatusUdt();

        u._SolutionStatus = (byte)Enum.Parse(typeof(SolutionStatus), s.Value, true);
        u._IsNull = false;

        return u;
    }

    public override string ToString()
    {
        return ((SolutionStatus)_SolutionStatus).ToString();
    }
    
    // 这是占位符成员字段
    public byte _SolutionStatus;
 
    //  私有成员
    private bool _IsNull;
}

public enum SolutionStatus : byte
{
    Pending = 1, 
    Compiling = 2, 
    Judging = 3, 
    Accepted = 4,  
    WrongAnswer = 5, 
    TimeLimitExcced = 6, 
    MemoryLimitExcced = 7, 
    RuntimeException = 8, 
}