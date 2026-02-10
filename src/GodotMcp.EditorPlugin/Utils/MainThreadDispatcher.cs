using Godot;
using System;
using System.Threading.Tasks;

namespace GodotMcp.EditorPlugin.Utils;

public static class MainThreadDispatcher
{
    private static Node _node;

    public static void Initialize(Node node)
    {
        _node = node;
    }

    public static Task<T> ExecuteOnMainThread<T>(Func<T> action)
    {
        var tcs = new TaskCompletionSource<T>();

        if (_node == null)
        {
            tcs.SetException(new InvalidOperationException("MainThreadDispatcher not initialized."));
            return tcs.Task;
        }

        // Use Callable.From to create a callable and defer its execution
        // Note: Callable.From is available in Godot 4.x
        Callable.From(() =>
        {
            try
            {
                var result = action();
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }).CallDeferred();

        return tcs.Task;
    }

    public static Task ExecuteOnMainThread(Action action)
    {
        var tcs = new TaskCompletionSource();

        if (_node == null)
        {
            tcs.SetException(new InvalidOperationException("MainThreadDispatcher not initialized."));
            return tcs.Task;
        }

        Callable.From(() =>
        {
            try
            {
                action();
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }).CallDeferred();

        return tcs.Task;
    }
}
