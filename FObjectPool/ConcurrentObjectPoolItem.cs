using System;
using System.Threading;
using System.Threading.Tasks;

namespace FObjectPool
{
	public class ConcurrentObjectPoolItem<TItem> : IDisposable
	{
		public ConcurrentObjectPool<TItem> Pool { get; private set; }
		public TItem item { get; private set; }

		public TItem Item
		{
			get
			{
				if (!disposed)
					return item;
				else
					throw new ObjectDisposedException(nameof(Item), "Object is reverted back to pool");
			}
			private set => item = value;
		}

		private bool disposed = false;

		public ConcurrentObjectPoolItem(ConcurrentObjectPool<TItem> pool, TItem item)
		{
			Pool = pool;
			Item = item;
		}

		public void BackToPool()
		{
			Dispose();
		}

		public async Task<bool> BackToPool(int millisecondsTimeout = -1, CancellationToken token = default)
		{
			var isAdded = await Pool.AddObjectAsync(Item, millisecondsTimeout, token);
			if (isAdded)
			{
				disposed = true;
				item = default;
			}
			return isAdded;
		}

		public void Dispose()
		{
			Pool.AddObjectAsync(Item);
			disposed = true;
			item = default;
		}
	}
}