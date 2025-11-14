using System.Runtime.CompilerServices;
using System.Transactions;
using BigOX.Internals;

namespace BigOX.Factories;

/// <summary>
///     Factory for creating <see cref="TransactionScope" /> objects.
/// </summary>
public static class TransactionFactory
{
    /// <summary>
    ///     Creates a new <see cref="TransactionScope" /> with the specified settings.
    /// </summary>
    /// <param name="isolationLevel">
    ///     The desired <see cref="IsolationLevel" /> for the transaction scope. Defaults to
    ///     <see cref="IsolationLevel.ReadCommitted" />.
    /// </param>
    /// <param name="transactionScopeOption">
    ///     The desired <see cref="TransactionScopeOption" /> for the transaction scope.
    ///     Defaults to <see cref="TransactionScopeOption.Required" />.
    /// </param>
    /// <param name="transactionScopeAsyncFlowOption">
    ///     The desired <see cref="TransactionScopeAsyncFlowOption" /> for the
    ///     transaction scope. Defaults to <see cref="TransactionScopeAsyncFlowOption.Enabled" />.
    /// </param>
    /// <param name="timeOut">
    ///     The desired timeout for the transaction scope. If not provided,
    ///     <see cref="TransactionManager.MaximumTimeout" /> is used.
    /// </param>
    /// <returns>A new <see cref="TransactionScope" /> instance with the specified settings.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="timeOut" /> is less than or equal to zero.</exception>
    /// <example>
    ///     <code><![CDATA[
    /// using (var scope = CreateTransaction(IsolationLevel.Serializable, TransactionScopeOption.RequiresNew))
    /// {
    ///     // Perform transactional work here.
    ///     scope.Complete();
    /// }
    /// ]]></code>
    /// </example>
    /// <remarks>
    ///     The <see cref="CreateTransaction" /> method is a helper method that simplifies the creation of a
    ///     <see cref="TransactionScope" /> with custom settings. It allows you to specify the <see cref="IsolationLevel" />,
    ///     <see cref="TransactionScopeOption" />, <see cref="TransactionScopeAsyncFlowOption" />, and an optional timeout.
    ///     By default, it creates a transaction scope with <see cref="IsolationLevel.ReadCommitted" />,
    ///     <see cref="TransactionScopeOption.Required" />, and <see cref="TransactionScopeAsyncFlowOption.Enabled" />. If no
    ///     timeout is provided, it uses the <see cref="TransactionManager.MaximumTimeout" /> value.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TransactionScope CreateTransaction(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        TransactionScopeOption transactionScopeOption = TransactionScopeOption.Required,
        TransactionScopeAsyncFlowOption transactionScopeAsyncFlowOption = TransactionScopeAsyncFlowOption.Enabled,
        TimeSpan? timeOut = null)
    {
        if (timeOut.HasValue && timeOut.Value <= TimeSpan.Zero)
        {
            ThrowHelper.ThrowArgumentOutOfRange(nameof(timeOut), timeOut.Value, "Timeout must be greater than zero.");
        }

        var transactionOptions = new TransactionOptions
        {
            IsolationLevel = isolationLevel,
            Timeout = timeOut ?? TransactionManager.MaximumTimeout
        };

        return new TransactionScope(transactionScopeOption, transactionOptions, transactionScopeAsyncFlowOption);
    }
}