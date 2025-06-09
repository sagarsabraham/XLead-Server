namespace XLead_Server.DTOs
{
    public class TableSchemaDto
    {
        public string TableName { get; set; }
        public string SchemaName { get; set; }
        public List<ColumnSchemaDto> Columns { get; set; } = new List<ColumnSchemaDto>();
        public List<RelationshipSchemaDto> Relationships { get; set; } = new List<RelationshipSchemaDto>();
    }

    public class ColumnSchemaDto
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
        public bool IsNullable { get; set; }
    }

    public class RelationshipSchemaDto
    {
        public string ForeignKeyName { get; set; }
        public string DependantTableName { get; set; }
        public List<string> DependantColumnNames { get; set; } = new List<string>();
        public string PrincipalTableName { get; set; }
        public List<string> PrincipalColumnNames { get; set; } = new List<string>();
    }
}