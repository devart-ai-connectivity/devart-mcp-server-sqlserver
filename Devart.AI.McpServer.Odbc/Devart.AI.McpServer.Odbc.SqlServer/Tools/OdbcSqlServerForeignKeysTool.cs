// --------------------------------------------------------------------------
// <copyright file="OdbcSqlServerForeignKeysTool.cs" company="Devart">
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
using Devart.AI.McpServer.Tools;
using Devart.AI.McpServer.Interfaces;

namespace Devart.AI.McpServer.Odbc.SqlServer.Tools
{
  internal sealed class OdbcSqlServerForeignKeysTool(McpConfiguration serverConfiguration) : ForeignKeysTool(serverConfiguration)
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
  IF OBJECT_ID('tempdb..#spfkeystmp') IS NULL
    CREATE TABLE #spfkeystmp(
      [ID] int IDENTITY,
      [PKTABLE_QUALIFIER] sysname,
      [PKTABLE_OWNER] sysname,
      [PKTABLE_NAME] sysname,
      [PKCOLUMN_NAME] sysname,
      [FKTABLE_QUALIFIER] sysname,
      [FKTABLE_OWNER] sysname,
      [FKTABLE_NAME] sysname,
      [FKCOLUMN_NAME] sysname,
      [KEY_SEQ] smallint,
      [UPDATE_RULE] smallint,
      [DELETE_RULE] smallint,
      [FK_NAME] sysname,
      [PK_NAME] sysname,
      [DEFERRABILITY] smallint
    )
    INSERT INTO #spfkeystmp(
      [PKTABLE_QUALIFIER],
      [PKTABLE_OWNER],
      [PKTABLE_NAME],
      [PKCOLUMN_NAME],
      [FKTABLE_QUALIFIER],
      [FKTABLE_OWNER],
      [FKTABLE_NAME],
      [FKCOLUMN_NAME],
      [KEY_SEQ],
      [UPDATE_RULE],
      [DELETE_RULE],
      [FK_NAME],
      [PK_NAME],
      [DEFERRABILITY]
    )
    EXEC sp_fkeys @fktable_qualifier = ?, @fktable_owner = ?, @fktable_name = ?
    SELECT
      [FK_NAME] AS 'FK_NAME',
      [FKCOLUMN_NAME] AS 'FKCOLUMN_NAME',
      [PKTABLE_QUALIFIER] AS 'PKTABLE_SCHEM',
      [PKTABLE_NAME] AS 'PKTABLE_NAME',
      [PKCOLUMN_NAME] AS 'PKCOLUMN_NAME',
      CASE [UPDATE_RULE]
        WHEN 0 THEN 'CASCADE'
        WHEN 1 THEN 'NO ACTION'
        WHEN 2 THEN 'SET NULL'
        WHEN 3 THEN 'SET DEFAULT'
      END AS 'UPDATE_RULE',
      CASE [DELETE_RULE]
        WHEN 0 THEN 'CASCADE'
        WHEN 1 THEN 'NO ACTION'
        WHEN 2 THEN 'SET NULL'
        WHEN 3 THEN 'SET DEFAULT'
      END AS 'DELETE_RULE'
    FROM #spfkeystmp
    ORDER BY ID
    DROP TABLE #spfkeystmp;
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

      return await reader.ToDataTableAsync(OdbcConstants.ForeignKeysCollectionName, cancellationToken).ConfigureAwait(false);
    }
  }
}