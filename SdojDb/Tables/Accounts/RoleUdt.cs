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
public struct RoleUdt: INullable
{
    public override string ToString()
    {
        return ((Roles)_Role).ToString();
    }
    
    public bool IsNull
    {
        get
        {
            return _IsNull;
        }
    }
    
    public static RoleUdt Null
    {
        get
        {
            var h = new RoleUdt();
            h._IsNull = true;
            return h;
        }
    }
    
    public static RoleUdt Parse(SqlString s)
    {
        if (s.IsNull)
            return Null;

        var u = new RoleUdt();
        u._Role = (byte)Enum.Parse(typeof(Roles), s.Value, true);
        u._IsNull = false;

        return u;
    }
    
    public byte _Role;
 
    private bool _IsNull;
}

public enum Roles : byte
{
    User = 1, 
    Admin = 2, 

}