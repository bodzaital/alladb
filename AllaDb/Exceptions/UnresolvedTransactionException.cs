namespace AllaDb.Exceptions;

public class UnresolvedTransactionException(string message) : Exception
{
	public override string Message => message;
}