using System;
using System.Collections.Generic;
using System.Text;

namespace FObjectPool
{
    interface IObjectPoolItem<TItem> : IDisposable
    {
        TItem Item { get; }
        void ReturnToPool();
    }
}
