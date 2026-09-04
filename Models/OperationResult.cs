namespace MemberManagementSystem.Models;

/// <summary>
/// Basic result used for actions that do not return a data object.
/// </summary>
public class OperationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Standard result used for service operations and validation responses.
/// </summary>
public class OperationResult<T> : OperationResult
{
    public T? Data { get; set; }
}
