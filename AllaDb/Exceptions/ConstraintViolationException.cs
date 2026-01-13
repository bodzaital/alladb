namespace AllaDb.Exceptions;

public class ConstraintViolationException(string message) : Exception(message)
{
    public static string RequiredViolation(string key) => $"Required key {key} is missing.";
    public static string UniqueViolation(string key) => $"Value of {key} is not unique.";
    public static string OutOfRangeViolation(string key) => $"Value of {key} is out of range.";
}