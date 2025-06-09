using Dapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XLead_Server.Data;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;
using Microsoft.Extensions.Logging;
// using Microsoft.EntityFrameworkCore.DbLoggerCategory.Database; // This was commented out, which is fine.

namespace XLead_Server.Repositories
{
    public class DbSchemaRepository : IDbSchemaRepository
    {
        private readonly ApiDbContext _context;
        private readonly ILogger<DbSchemaRepository> _logger;

        // Define RawRelationshipInfo as a private nested class for clarity and proper scope.
        private class RawRelationshipInfo
        {
            public string ForeignKeyName { get; set; }
            public string DependantTableName { get; set; }
            public string DependantColumnName { get; set; }
            public string PrincipalTableName { get; set; }
            public string PrincipalColumnName { get; set; }
        }

        public DbSchemaRepository(ApiDbContext context, ILogger<DbSchemaRepository> logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        public async Task<IEnumerable<TableSchemaDto>> GetDetailedSchemaStructureAsync()
        {
            _logger?.LogInformation("Fetching detailed DB schema structure using Dapper via DbContext connection.");
            var tablesData = new List<TableSchemaDto>();
            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                var tableNames = (await connection.QueryAsync<string>(
                    "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME"
                )).ToList();

                foreach (var tableName in tableNames) // Loop for each table
                {
                    var tableDto = new TableSchemaDto { TableName = tableName, SchemaName = "dbo" };

                    // Fetch Columns for the current table
                    var columns = (await connection.QueryAsync<ColumnSchemaDto>(
                        @"SELECT
                            COLUMN_NAME AS ColumnName,
                            DATA_TYPE AS DataType,
                            (CASE IS_NULLABLE WHEN 'YES' THEN 1 ELSE 0 END) AS IsNullable,
                            (SELECT CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END
                             FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                             JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME AND tc.TABLE_SCHEMA = kcu.TABLE_SCHEMA AND tc.TABLE_NAME = kcu.TABLE_NAME
                             WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY' AND kcu.TABLE_NAME = @tableName AND kcu.COLUMN_NAME = c.COLUMN_NAME) AS IsPrimaryKey
                        FROM INFORMATION_SCHEMA.COLUMNS c
                        WHERE TABLE_NAME = @tableName
                        ORDER BY ORDINAL_POSITION", new { tableName }
                    )).ToList();
                    tableDto.Columns = columns;

                    // Fetch Relationships for the current table
                    // RawRelationshipInfo class is now defined as a nested class above
                    var rawRelationships = (await connection.QueryAsync<RawRelationshipInfo>(
                        @"SELECT
                            rc.CONSTRAINT_NAME AS ForeignKeyName,
                            kcu_dep.TABLE_NAME AS DependantTableName,
                            kcu_dep.COLUMN_NAME AS DependantColumnName,
                            kcu_princ.TABLE_NAME AS PrincipalTableName,
                            kcu_princ.COLUMN_NAME AS PrincipalColumnName
                        FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc
                        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu_dep
                            ON rc.CONSTRAINT_NAME = kcu_dep.CONSTRAINT_NAME AND rc.CONSTRAINT_SCHEMA = kcu_dep.CONSTRAINT_SCHEMA
                        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu_princ
                            ON rc.UNIQUE_CONSTRAINT_NAME = kcu_princ.CONSTRAINT_NAME AND rc.UNIQUE_CONSTRAINT_SCHEMA = kcu_princ.CONSTRAINT_SCHEMA
                            AND kcu_dep.ORDINAL_POSITION = kcu_princ.ORDINAL_POSITION
                        WHERE kcu_dep.TABLE_NAME = @tableName;", new { tableName }
                    )).ToList();

                    var groupedRelationships = rawRelationships
                        .GroupBy(r => new { r.ForeignKeyName, r.DependantTableName, r.PrincipalTableName })
                        .Select(g => new RelationshipSchemaDto
                        {
                            ForeignKeyName = g.Key.ForeignKeyName,
                            DependantTableName = g.Key.DependantTableName,
                            PrincipalTableName = g.Key.PrincipalTableName,
                            DependantColumnNames = g.Select(r => r.DependantColumnName).Distinct().OrderBy(cn => cn).ToList(),
                            PrincipalColumnNames = g.Select(r => r.PrincipalColumnName).Distinct().OrderBy(cn => cn).ToList()
                        }).ToList();

                    tableDto.Relationships = groupedRelationships;
                    tablesData.Add(tableDto); // Add the fully populated tableDto to the list INSIpackageDE the loop
                } // End of foreach (var tableName in tableNames)
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error fetching detailed DB schema structure.");
                throw; // Re-throw to allow higher-level error handling
            }
            // Do NOT manually close 'connection' here as its lifetime is managed by the scoped DbContext.

            return tablesData;
        } // End of GetDetailedSchemaStructureAsync

        public async Task<string> GetFormattedSchemaAsync()
        {
            var detailedSchema = await GetDetailedSchemaStructureAsync();
            var sb = new StringBuilder();
            _logger?.LogInformation("Formatting DB schema string.");

            foreach (var table in detailedSchema)
            {
                string fullTableName = string.IsNullOrEmpty(table.SchemaName) || table.SchemaName.Equals("dbo", StringComparison.OrdinalIgnoreCase)
                                     ? table.TableName
                                     : $"{table.SchemaName}.{table.TableName}";
                sb.AppendLine($"Table: {fullTableName}");

                if (table.Columns.Any())
                {
                    sb.AppendLine("  Columns:");
                    foreach (var column in table.Columns)
                    {
                        sb.Append($"    - {column.ColumnName} (Type: {column.DataType}");
                        if (column.IsPrimaryKey) sb.Append(", PK");
                        if (column.IsNullable) sb.Append(", Nullable");
                        sb.AppendLine(")");
                    }
                }

                if (table.Relationships.Any())
                {
                    sb.AppendLine("  Relationships (Foreign Keys defined in this table):");
                    foreach (var rel in table.Relationships)
                    {
                        var fkColumns = string.Join(", ", rel.DependantColumnNames);
                        var pkColumns = string.Join(", ", rel.PrincipalColumnNames);
                        // Assuming PrincipalTableName in RelationshipSchemaDto does not include schema.
                        // If PrincipalTable could be in a different schema, this might need adjustment.
                        // For now, using the plain PrincipalTableName. If tables can be in different schemas,
                        // you might need to query schema for principal table or ensure PrincipalTableName includes it.
                        string principalFullTableName = rel.PrincipalTableName;
                        sb.AppendLine($"    - Constraint: {rel.ForeignKeyName ?? "N/A"}, Columns: [{fkColumns}] point to {principalFullTableName}[{pkColumns}]");
                    }
                }
                sb.AppendLine(); // Extra line break between tables
            }
            return sb.ToString();
        }
    } // End of DbSchemaRepository class
} // End of namespace