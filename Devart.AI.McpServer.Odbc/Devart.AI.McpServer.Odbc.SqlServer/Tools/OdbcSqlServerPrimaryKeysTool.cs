// --------------------------------------------------------------------------
// <copyright file="OdbcSqlServerPrimaryKeysTool.cs" company="Devart">
//
// Copyright (c) Devart. ALL RIGHTS RESERVED
// Use of the source code is permitted under the license.
// </copyright>
// --------------------------------------------------------------------------

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Devart.AI.McpServer.Extensions;
using Devart.AI.McpServer.Interfaces;
using Devart.AI.McpServer.Tools;

namespace Devart.AI.McpServer.Odbc.SqlServer.Tools
{
  internal sealed class OdbcSqlServerPrimaryKeysTool(McpConfiguration serverConfiguration) : PrimaryKeysTool(serverConfiguration)
  {
    protected override async Task<DataTable> GetMetadataTable(
      DbConnection connection,
      string schema,
      string tableName,
      IServiceProvider services,
      CancellationToken cancellationToken)
    {
      const string sql =
"""
  IF OBJECT_ID('tempdb..#sppkeystmp') IS NULL
    CREATE TABLE #sppkeystmp(
      [ID] int IDENTITY,
      [TABLE_QUALIFIER] sysname,
      [TABLE_OWNER] sysname,
      [TABLE_NAME] sysname,
      [COLUMN_NAME] sysname,
      [KEY_SEQ] smallint,
      [PK_NAME] sysname
    )
    INSERT INTO #sppkeystmp(
      [TABLE_QUALIFIER],
      [TABLE_OWNER],
      [TABLE_NAME],
      [COLUMN_NAME],
      [KEY_SEQ],
      [PK_NAME]
    )
    EXEC sp_pkeys @table_qualifier = ?, @table_owner = ?, @table_name = ?
    SELECT
      [PK_NAME] AS 'PK_NAME',
      [COLUMN_NAME] AS 'COLUMN_NAME'
    FROM #sppkeystmp
    ORDER BY ID;
    DROP TABLE #sppkeystmp;
""";

      var database = services.GetRequiredService<IDatabase>();
      var commandHelper = services.GetRequiredService<ICommandHelper>();

      await using var reader = await database.ExecuteReaderAsync(
        connection,
        sql,
        cmd =>
        {
          commandHelper.AddParameter(cmd, connection.Database);
          commandHelper.AddParameter(cmd, "dbo");
          commandHelper.AddParameter(cmd, tableName);
        },
        cancellationToken
      ).ConfigureAwait(false);

      return await reader.ToDataTableAsync(OdbcConstants.PrimaryKeysCollectionName, cancellationToken).ConfigureAwait(false);
    }
  }
}
