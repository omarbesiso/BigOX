using System.Transactions;
using BigOX.Factories;

namespace BigOX.Tests.Factories;

[TestClass]
public sealed class TransactionFactoryTests
{
    [TestMethod]
    public void CreateTransaction_Defaults_CreateScopeWithDefaults()
    {
        using var scope = TransactionFactory.CreateTransaction();
        Assert.IsNotNull(scope);

        // We cannot directly read IsolationLevel/Timeout from scope, but we can perform a simple transactional action
        // and ensure no exceptions. Completing should succeed.
        scope.Complete();
    }

    [TestMethod]
    public void CreateTransaction_CustomIsolationAndOptions_CreatesScope()
    {
        using var scope = TransactionFactory.CreateTransaction(
            IsolationLevel.Serializable,
            TransactionScopeOption.RequiresNew,
            TransactionScopeAsyncFlowOption.Enabled,
            TimeSpan.FromSeconds(30));

        Assert.IsNotNull(scope);
        scope.Complete();
    }

    [TestMethod]
    public void CreateTransaction_InvalidTimeout_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            TransactionFactory.CreateTransaction(timeOut: TimeSpan.Zero));

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            TransactionFactory.CreateTransaction(timeOut: TimeSpan.FromMilliseconds(-1)));
    }
}