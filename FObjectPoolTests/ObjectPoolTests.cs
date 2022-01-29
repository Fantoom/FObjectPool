using NUnit.Framework;
using FObjectPool;
using System.Collections.Generic;

namespace FObjectPoolTests
{
    public class ObjectPoolTests
    {
        [Test]
        public void InitialObjectsTest()
        {
            var initialObjects = new[] { 1, 2, 3, 4 };
            var pool = new ObjectPool<int>(initialObjects);
            var list = new List<int>();
            while(pool.Count > 0)
            {
                list.Add(pool.GetObject().Item);
            }
            CollectionAssert.AreEqual(initialObjects, list);
        }

        [Test]
        public void ObjectCreationTest()
        { 
            var pool = new ObjectPool<int>(() => 1);
            Assert.AreEqual(pool.GetObject().Item, 1);
        }

        [Test]
        public void ReturnToPoolTest()
        {
            var initialObjects = new[] { 1, 2, 3, 4 };
            var testObjects = new[] { 2, 3, 4, 1 };

            var pool = new ObjectPool<int>(initialObjects);
            pool.GetObject().ReturnToPool();

            var list = new List<int>();
            while (pool.Count > 0)
            {
                list.Add(pool.GetObject().Item);
            }

            CollectionAssert.AreEqual(testObjects, list);
        }

        [Test]
        public void FillWithObjectsTest()
        {
            var initialObjects = new[] { 1, 2 };
            var fillObjects = new[] { 3, 4 };
            var testObjects = new[] { 1, 2, 3, 4, };

            var pool = new ObjectPool<int>(initialObjects);
            pool.FillPool(fillObjects);

            var list = new List<int>();
            while (pool.Count > 0)
            {
                list.Add(pool.GetObject().Item);
            }

            CollectionAssert.AreEqual(testObjects, list);
        }

        [Test]
        public void FillTest()
        {
            var testObjects = new[] { 1, 2, 3, 4 };
            var pool = new ObjectPool<int>();
            pool.FillPool(testObjects);

            var list = new List<int>();
            while (pool.Count > 0)
            {
                list.Add(pool.GetObject().Item);
            }

            CollectionAssert.AreEqual(testObjects, list);
        }

        [Test]
        public void FillWithCreationTest()
        {
            var testObjects = new[] { 1, 2, 3, 4};
            var x = 1;
            var pool = new ObjectPool<int>(() => x++);
            pool.FillPool(4);

            var list = new List<int>();
            while (pool.Count > 0)
            {
                list.Add(pool.GetObject().Item);
            }

            CollectionAssert.AreEqual(testObjects, list);
        }
    }
}