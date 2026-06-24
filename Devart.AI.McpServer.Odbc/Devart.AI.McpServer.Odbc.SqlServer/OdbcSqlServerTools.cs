// --------------------------------------------------------------------------
// <copyright file="OdbcSqlServerTools.cs" company="Devart">
//
// Copyright (c) Devart. ALL RIGHTS RESERVED
// Use of the source code is permitted under the license.
// </copyright>
// --------------------------------------------------------------------------

using System.Collections.Generic;
using ModelContextProtocol.Server;
using Devart.AI.McpServer.Odbc.SqlServer.Tools;

namespace Devart.AI.McpServer.Odbc.SqlServer
{
  internal static class OdbcSqlServerTools
  {
    public static List<McpServerTool> CreateTools(McpConfiguration configuration)
      => OdbcTools.CreateBuilder(configuration)
        .Add(new OdbcSqlServerPrimaryKeysTool(configuration))
        .Add(new OdbcSqlServerForeignKeysTool(configuration))
        .Build();
  }
}
