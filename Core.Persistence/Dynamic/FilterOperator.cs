namespace Core.Persistence.Dynamic;

public enum FilterOperator
{
    Eq,        // Equal
    Neq,       // Not Equal
    Lt,        // Less Than
    Lte,       // Less Than or Equal
    Gt,        // Greater Than
    Gte,       // Greater Than or Equal
    Contains,  // Contains
    StartsWith,// Starts With
    EndsWith,  // Ends With
    IsNull,    // Is Null
    IsNotNull  // Is Not Null
}