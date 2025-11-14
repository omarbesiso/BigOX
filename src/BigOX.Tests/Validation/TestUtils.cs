namespace BigOX.Tests.Validation;

internal static class TestUtils
{
    public static T Expect<T>(
        Action action,
        Action<T>? validator = null) where T : Exception
    {
        try
        {
            action();
        }
        catch (T ex)
        {
            validator?.Invoke(ex);
            return ex;
        }

        Assert.Fail($"Expected exception of type {typeof(T).Name}");
        throw new InvalidOperationException();
    }
}