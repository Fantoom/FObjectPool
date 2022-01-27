using System;
using System.Collections.Generic;
using System.Text;

namespace FObjectPool
{
	public struct ObjectPoolItem<TItem>
	{
		public ObjectPool<TItem> Pool { get; private set; }
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

		public ObjectPoolItem(ObjectPool<TItem> pool, TItem item)
		{
			Pool = pool;
			this.item = item;
		}

		public void ReturnToPool()
		{
			Pool.AddObject(Item);
			disposed = true;
			item = default!;
		}
	}
}
