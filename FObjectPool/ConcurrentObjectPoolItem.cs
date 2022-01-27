using System;
using System.Threading;
using System.Threading.Tasks;

namespace FObjectPool
{
	public struct ConcurrentObjectPoolItem<TItem>
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
			this.item = item;
		}

		public async Task<bool> ReturnToPool(CancellationToken token = default, int millisecondsTimeout = -1)
		{
			var isAdded = await Pool.AddObjectAsync(Item, millisecondsTimeout, token);
			if (isAdded)
			{
				disposed = true;
				item = default!;
			}
			return isAdded;
		}
	}
}