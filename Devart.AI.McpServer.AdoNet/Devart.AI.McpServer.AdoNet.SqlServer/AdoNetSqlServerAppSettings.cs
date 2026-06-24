// --------------------------------------------------------------------------
// <copyright file="AdoNetSqlServerAppSettings.cs" company="Devart">
//
// Copyright (c) Devart. ALL RIGHTS RESERVED
// Use of the source code is permitted under the license.
// </copyright>
// --------------------------------------------------------------------------

namespace Devart.AI.McpServer.AdoNet.SqlServer
{
  internal sealed class AdoNetSqlServerAppSettings : McpAppSettings
  {
    public override string ServerName => $"Devart {Properties.ProductInfo.ProductFullName}";

    public override string SourceName => "SQL Server";

    public override bool OnPremise => true;
  }
}
