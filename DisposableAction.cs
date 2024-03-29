﻿using System;
using System.Threading;

namespace mrGitTags
{
    internal sealed class DisposableAction : IDisposable
    {

        public static readonly DisposableAction Empty = new(null);

        private Action? _disposeAction;

        public DisposableAction(Action? disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            if (_disposeAction is null)
            {
                return;
            }

            // Interlocked allows the continuation to be executed only once
            Action continuation = Interlocked.Exchange(ref _disposeAction, null);
            continuation?.Invoke();
        }
    }
}