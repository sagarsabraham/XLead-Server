// XLead_Server/Services/DapperDataQueryService.cs
using Dapper;
// Remove: using Microsoft.Extensions.Configuration; // No longer needed here
using System;
using System.Collections.Generic;
using System.Data;
// using Microsoft.Data.SqlClient; // Dapper will work with the IDbConnection from EF Core
using System.Linq;
using System.Threading.Tasks;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;
using XLead_Server.Data; // Add this using for ApiDbContext
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // Add this for DatabaseFacade extensions like GetDbConnection

namespace XLead_Server.Services
{
    public class DapperDataQueryRepository : IDataQueryRepository
    {
        // Remove: private readonly string _connectionString;
        private readonly ApiDbContext _context; // Inject ApiDbContext
        private readonly ILogger<DapperDataQueryRepository> _logger;

        public DapperDataQueryRepository(
            ApiDbContext context, // Inject ApiDbContext
            ILogger<DapperDataQueryRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); // Store the context
            _logger = logger;
            // Connection string is no longer directly configured or fetched in this service.
            // It's managed by ApiDbContext's configuration.
        }

        public async Task<QueryResultDto> ExecuteSelectQueryAsync(string sqlQuery)
        {
            _logger.LogInformation("Executing SQL Query via Dapper using DbContext connection: {Query}", sqlQuery);
            IDbConnection dbConnection = null;
            try
            {
                // Get the database connection from the DbContext  
                dbConnection = _context.Database.GetDbConnection();

                // Explicitly open the connection if it's closed  
                if (dbConnection.State != ConnectionState.Open)
                {
                    dbConnection.Open(); // Use synchronous Open method instead of OpenAsync  
                }

                var result = (await dbConnection.QueryAsync(sqlQuery, commandTimeout: 30)).ToList();

                var dictionaries = result
                    .Select(row => ((IDictionary<string, object>)row)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value))
                    .ToList();

                _logger.LogInformation("Query executed successfully. Rows returned: {RowCount}", dictionaries.Count);
                return new QueryResultDto
                {
                    Success = true,
                    Data = dictionaries,
                    RecordsAffected = dictionaries.Count
                };
            }
            catch (System.Data.Common.DbException ex)
            {
                _logger.LogError(ex, "Database Exception during query execution. Query: {Query}", sqlQuery);
                return new QueryResultDto { Success = false, ErrorMessage = $"Database error: {ex.Message}" };
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "SQL query timed out. Query: {Query}", sqlQuery);
                return new QueryResultDto { Success = false, ErrorMessage = "The database query timed out." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception during query execution. Query: {Query}", sqlQuery);
                return new QueryResultDto { Success = false, ErrorMessage = $"An unexpected error occurred: {ex.Message}" };
            }
        }
    }
}