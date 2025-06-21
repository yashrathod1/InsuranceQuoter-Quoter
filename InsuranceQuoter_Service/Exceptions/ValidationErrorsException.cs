namespace InsuranceQuoter_Service.Exceptions;

public class ValidationErrorsException : Exception
{
    public List<string> Errors { get; }

    public ValidationErrorsException(List<string> errors)
        : base("Validation failed.")
    {
        Errors = errors;
    }
}

public class ErrorResponse
{
    public List<string> Errors { get; set; } = new();
}
