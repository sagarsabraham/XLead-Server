namespace XLead_Server.Interfaces
{
    public enum SqlValidationResult
    {
        Safe,
        UnsafeKeywords,
        SyntaxError,
        EmptyOrNull
    }

    public interface ISqlValidationService
    {
        SqlValidationResult ValidateQuery(string sqlQuery, out string validationMessage);
    }
}