using BigOX.Types;

namespace BigOX.Tests.Types;

[TestClass]
public class DisposableObjectTests
{
    [TestMethod]
    public void Dispose_CallsManagedAndUnmanaged_Once()
    {
        var sut = new AsyncTestDisposable();

        sut.Dispose();

        Assert.AreEqual(1, sut.ManagedDisposedCount, "Managed dispose should run once");
        Assert.AreEqual(1, sut.UnmanagedDisposedCount, "Unmanaged dispose should run once");
        Assert.AreEqual(0, sut.AsyncCoreCount, "Async core should not run for sync Dispose");

        // Subsequent calls are no-ops
        sut.Dispose();
        Assert.AreEqual(1, sut.ManagedDisposedCount);
        Assert.AreEqual(1, sut.UnmanagedDisposedCount);

        // Public members must throw after disposal
        Assert.ThrowsExactly<ObjectDisposedException>(() => sut.Touch());
    }

    [TestMethod]
    public async Task DisposeAsync_CallsAsyncCore_ManagedAndUnmanaged_Once()
    {
        var sut = new AsyncTestDisposable { AsyncDelayMs = 10 };

        await sut.DisposeAsync();

        Assert.AreEqual(1, sut.AsyncCoreCount, "Async core should run once");
        Assert.AreEqual(1, sut.ManagedDisposedCount, "Managed dispose should also run via async path");
        Assert.AreEqual(1, sut.UnmanagedDisposedCount, "Unmanaged dispose should run once");

        // Subsequent async dispose is a no-op
        await sut.DisposeAsync();
        Assert.AreEqual(1, sut.AsyncCoreCount);
        Assert.AreEqual(1, sut.ManagedDisposedCount);
        Assert.AreEqual(1, sut.UnmanagedDisposedCount);
    }

    [TestMethod]
    public async Task DisposeAsync_MultipleConcurrentCalls_RunOnlyOnce()
    {
        var sut = new AsyncTestDisposable { AsyncDelayMs = 50 };

        var t1 = sut.DisposeAsync().AsTask();
        var t2 = sut.DisposeAsync().AsTask();
        var t3 = sut.DisposeAsync().AsTask();
        await Task.WhenAll(t1, t2, t3);

        Assert.AreEqual(1, sut.AsyncCoreCount, "Async core should only execute once");
        Assert.AreEqual(1, sut.ManagedDisposedCount, "Managed should execute once");
        Assert.AreEqual(1, sut.UnmanagedDisposedCount, "Unmanaged should execute once");
    }

    [TestMethod]
    public async Task DefaultAsyncCore_InvokesManaged_OnDisposeAsync()
    {
        var sut = new SyncOnlyDisposable();

        await sut.DisposeAsync();

        Assert.AreEqual(1, sut.ManagedDisposedCount, "Base DisposeAsyncCore should call DisposeManagedResources");
        Assert.AreEqual(1, sut.UnmanagedDisposedCount, "Unmanaged should run once");
    }

    [TestMethod]
    public void FinalizerPath_DisposeFalse_OnlyUnmanaged()
    {
        var sut = new SyncOnlyDisposable();

        sut.DisposeFromFinalizerPath();

        Assert.AreEqual(0, sut.ManagedDisposedCount, "Managed should not run on finalizer path");
        Assert.AreEqual(1, sut.UnmanagedDisposedCount, "Unmanaged should run once");

        // Subsequent normal dispose is a no-op
        sut.Dispose();
        Assert.AreEqual(0, sut.ManagedDisposedCount);
        Assert.AreEqual(1, sut.UnmanagedDisposedCount);
    }

    [TestMethod]
    public void ThrowIfDisposed_DuringDisposing_ThrowsOnConcurrentPublicCalls()
    {
        using var start = new ManualResetEventSlim(false);
        using var release = new ManualResetEventSlim(false);

        var sut = new SyncOnlyDisposable
        {
            ManagedStarted = start,
            ManagedContinue = release
        };

        var disposingTask = Task.Run(() => sut.Dispose());

        // Wait until managed dispose has started (state should be Disposing)
        Assert.IsTrue(start.Wait(1000), "Timed out waiting for managed dispose to start");

        // Public members should observe disposing and throw
        Assert.ThrowsExactly<ObjectDisposedException>(() => sut.Touch());

        // Allow disposal to complete
        release.Set();
        disposingTask.GetAwaiter().GetResult();

        // After completion still throws
        Assert.ThrowsExactly<ObjectDisposedException>(() => sut.Touch());
    }

    [TestMethod]
    public async Task DisposeAsync_WhenAsyncCoreThrows_StillCallsUnmanaged_AndMarksDisposed()
    {
        var sut = new ThrowingAsyncDisposable();

        try
        {
            await sut.DisposeAsync();
            Assert.Fail("Expected InvalidOperationException");
        }
        catch (InvalidOperationException)
        {
            // expected
        }

        // Despite the exception, unmanaged cleanup should have been called and object marked disposed
        Assert.AreEqual(1, sut.UnmanagedDisposedCount, "Unmanaged should run in finally even when async core throws");

        // Subsequent dispose calls should be no-ops and not throw
        await sut.DisposeAsync();
    }

    private sealed class SyncOnlyDisposable : DisposableObject
    {
        public ManualResetEventSlim? ManagedContinue;
        public int ManagedDisposedCount;

        public ManualResetEventSlim? ManagedStarted;
        public int UnmanagedDisposedCount;

        protected override void DisposeManagedResources()
        {
            Interlocked.Increment(ref ManagedDisposedCount);
            // Signal that managed dispose started, and optionally block until released
            ManagedStarted?.Set();
            ManagedContinue?.Wait();
        }

        protected override void DisposeUnmanagedResources()
        {
            Interlocked.Increment(ref UnmanagedDisposedCount);
        }

        public void Touch()
        {
            ThrowIfDisposed();
        }

        public void DisposeFromFinalizerPath()
        {
            Dispose(false);
        }
    }

    private sealed class AsyncTestDisposable : DisposableObject
    {
        public int AsyncCoreCount;
        public int AsyncDelayMs;
        public int ManagedDisposedCount;
        public int UnmanagedDisposedCount;

        protected override void DisposeManagedResources()
        {
            Interlocked.Increment(ref ManagedDisposedCount);
        }

        protected override async ValueTask DisposeAsyncCore()
        {
            Interlocked.Increment(ref AsyncCoreCount);
            if (AsyncDelayMs > 0)
            {
                await Task.Delay(AsyncDelayMs).ConfigureAwait(false);
            }

            // Also clean up sync-managed resources to keep parity
            await base.DisposeAsyncCore();
        }

        protected override void DisposeUnmanagedResources()
        {
            Interlocked.Increment(ref UnmanagedDisposedCount);
        }

        public void Touch()
        {
            ThrowIfDisposed();
        }
    }

    private sealed class ThrowingAsyncDisposable : DisposableObject
    {
        private int _asyncCoreCount;
        public int UnmanagedDisposedCount;

        protected override ValueTask DisposeAsyncCore()
        {
            Interlocked.Increment(ref _asyncCoreCount);
            throw new InvalidOperationException("boom");
        }

        protected override void DisposeUnmanagedResources()
        {
            Interlocked.Increment(ref UnmanagedDisposedCount);
        }

        // ReSharper disable once UnusedMember.Local
        public void Touch()
        {
            ThrowIfDisposed();
        }
    }
}