// XLead_Server/Services/SqlValidationService.cs
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XLead_Server.Interfaces;

namespace XLead_Server.Services
{
    public class SqlValidationService : ISqlValidationService
    {
        // More restrictive keywords. Add any others you deem unsafe.
        private static readonly HashSet<string> ForbiddenKeywords = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            "INSERT", "UPDATE", "DELETE", "MERGE", "TRUNCATE",
            "CREATE", "ALTER", "DROP", "RENAME",
            "EXEC", "EXECUTE", "SP_", "XP_",
            "GRANT", "REVOKE", "DENY",
            "SHUTDOWN", "BACKUP", "RESTORE"
            // "DECLARE", "SET" // Could be too restrictive, allow if needed for complex SELECTs
        };

        public SqlValidationResult ValidateQuery(string sqlQuery, out string validationMessage)
        {
            validationMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(sqlQuery))
            {
                validationMessage = "SQL query cannot be empty.";
                return SqlValidationResult.EmptyOrNull;
            }

            // 1. Basic Keyword Check (quick filter)
            var tokens = sqlQuery.Split(new[] { ' ', '\r', '\n', '\t', ';', '(', ')', ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                if (ForbiddenKeywords.Contains(token))
                {
                    validationMessage = $"Query contains forbidden keyword: '{token}'. Only SELECT statements are allowed.";
                    return SqlValidationResult.UnsafeKeywords;
                }
            }

            // 2. TSqlParser for deeper validation (for SQL Server)
            // Ensure the parser is set to the correct SQL Server compatibility level if needed.
            var parser = new TSql150Parser(true); // true for initialQuotedIdentifiers
            IList<ParseError> errors;
            TSqlFragment fragment;

            using (var reader = new StringReader(sqlQuery))
            {
                fragment = parser.Parse(reader, out errors);
            }

            if (errors != null && errors.Any())
            {
                validationMessage = $"SQL syntax error: {string.Join("; ", errors.Select(e => e.Message))}";
                return SqlValidationResult.SyntaxError;
            }

            // 3. Visitor pattern to ensure it's a SELECT and doesn't contain disallowed statements
            var visitor = new SafeQueryAstVisitor();
            fragment.Accept(visitor);

            if (!visitor.IsSafe)
            {
                validationMessage = visitor.UnsafeReason ?? "The query structure is not allowed (e.g., DML, DDL, or disallowed elements found).";
                return SqlValidationResult.UnsafeKeywords;
            }

            if (!visitor.HasSelectStatement)
            {
                validationMessage = "The query must be a SELECT statement.";
                return SqlValidationResult.UnsafeKeywords; // Treat as unsafe if not a SELECT
            }

            validationMessage = "Query appears safe.";
            return SqlValidationResult.Safe;
        }
    }

    // AST Visitor for ScriptDom
    internal class SafeQueryAstVisitor : TSqlFragmentVisitor
    {
        public bool IsSafe { get; private set; } = true;
        public bool HasSelectStatement { get; private set; } = false;
        public string UnsafeReason { get; private set; }

        // Allow SELECT statements
        public override void Visit(SelectStatement node)
        {
            HasSelectStatement = true;
            base.Visit(node); // Continue visiting children
        }

        // Disallow common DML
        public override void Visit(InsertStatement node) { SetUnsafe("INSERT statement found."); base.Visit(node); }
        public override void Visit(UpdateStatement node) { SetUnsafe("UPDATE statement found."); base.Visit(node); }
        public override void Visit(DeleteStatement node) { SetUnsafe("DELETE statement found."); base.Visit(node); }
        public override void Visit(MergeStatement node) { SetUnsafe("MERGE statement found."); base.Visit(node); }
        public override void Visit(TruncateTableStatement node) { SetUnsafe("TRUNCATE TABLE statement found."); base.Visit(node); }

        // Disallow common DDL
        public override void Visit(CreateProcedureStatement node) { SetUnsafe("CREATE PROCEDURE statement found."); base.Visit(node); }
        public override void Visit(AlterProcedureStatement node) { SetUnsafe("ALTER PROCEDURE statement found."); base.Visit(node); }
        public override void Visit(DropProcedureStatement node) { SetUnsafe("DROP PROCEDURE statement found."); base.Visit(node); }
        public override void Visit(CreateTableStatement node) { SetUnsafe("CREATE TABLE statement found."); base.Visit(node); }
        public override void Visit(AlterTableStatement node) { SetUnsafe("ALTER TABLE statement found."); base.Visit(node); }
        public override void Visit(DropTableStatement node) { SetUnsafe("DROP TABLE statement found."); base.Visit(node); }
        // Add more DDL types as needed (CreateView, CreateIndex, etc.)

        // Disallow execution statements
        public override void Visit(ExecuteStatement node) { SetUnsafe("EXECUTE statement found."); base.Visit(node); }
        public override void Visit(ExecuteAsStatement node) { SetUnsafe("EXECUTE AS statement found."); base.Visit(node); }

        // Potentially disallow certain system functions or objects if needed, though tricky
        // public override void Visit(FunctionCall node)
        // {
        //     if (node.FunctionName.Value.StartsWith("xp_", System.StringComparison.OrdinalIgnoreCase) ||
        //         node.FunctionName.Value.StartsWith("sp_", System.StringComparison.OrdinalIgnoreCase))
        //     {
        //         SetUnsafe($"System function/procedure call '{node.FunctionName.Value}' found.");
        //     }
        //     base.Visit(node);
        // }

        private void SetUnsafe(string reason)
        {
            if (IsSafe) // Only set reason for the first unsafe element found
            {
                IsSafe = false;
                UnsafeReason = reason;
            }
        }
    }
}