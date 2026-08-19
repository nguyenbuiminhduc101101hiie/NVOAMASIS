namespace NVOAMASIS.Accounting.B09.Domain;

public enum B09ValueType
{
    Text = 1,
    Amount = 2,
    Quantity = 3,
    Percent = 4
}

public enum B09SourceType
{
    Manual = 1,
    GeneralLedger = 2,
    CustomSql = 3,
    Formula = 4,
    Hybrid = 5
}

public enum B09MappingMode
{
    AccountBalance = 1,
    PeriodActivity = 2,
    CustomSql = 3
}

public enum B09ReportStatus
{
    Draft = 1,
    Generated = 2,
    Validated = 3,
    Reviewed = 4,
    Approved = 5,
    Locked = 6
}

public enum B09ValidationSeverity
{
    Info = 1,
    Warning = 2,
    Error = 3
}
