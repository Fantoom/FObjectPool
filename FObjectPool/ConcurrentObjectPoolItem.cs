using System;
using System.Threading;
using System.Threading.Tasks;

namespace FObjectPool
{
	public class ConcurrentObjectPoolItem<TItem> : IObjectPoolItem<TItem>
	{
		public ConcurrentObjectPool<TItem> Pool { get; private set; }
		private TItem item;
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

		public void ReturnToPool()
		{
			Dispose();
		}

		public async Task<bool> ReturnToPool(CancellationToken token = default, int millisecondsTimeout = -1)
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