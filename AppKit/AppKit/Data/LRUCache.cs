namespace AdMaiora.AppKit.Data
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.CompilerServices;
    using System.Linq;

    class LRUCache<K,V>
    {
        #region Inner Classes

        internal class LRUCacheItem<K, V>
        {
            public LRUCacheItem(K k, V v)
            {
                this.Key = k;
                this.Value = v;
            }

            public K Key;
            public V Value;
        }

        #endregion

        #region Constants and Fields

        private int _capacity;
        private Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>> _cacheMap = new Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>>();
        private LinkedList<LRUCacheItem<K, V>> _lruList = new LinkedList<LRUCacheItem<K, V>>();
                   
        #endregion

        #region Constructors and Destructors

        public LRUCache(int capacity)
        {
            _capacity = capacity;
        }

        #endregion

        #region Properties

        public int Capacity
        {
            get
            {
                return _capacity;
            }
            set
            {
                _capacity = value;
            }
        }

        #endregion

        #region Public Methods

        public V Get(K key)
        {
            lock (_lruList)
            {
                LinkedListNode<LRUCacheItem<K, V>> node;
                if (_cacheMap.TryGetValue(key, out node))
                {
                    //System.Console.WriteLine("Cache HIT " + key);
                    V value = node.Value.Value;

                    _lruList.Remove(node);
                    _lruList.AddLast(node);
                    return value;
                }

                //System.Console.WriteLine("Cache MISS " + key);
                return default(V);
            }
        }
        
        public void Add(K key, V val, out V removed)
        {
            lock (_lruList)
            {
                removed = default(V);
                if (_cacheMap.Count >= _capacity)
                {
                    removed = RemoveFirst();
                }

                LRUCacheItem<K, V> cacheItem = new LRUCacheItem<K, V>(key, val);
                LinkedListNode<LRUCacheItem<K, V>> node = new LinkedListNode<LRUCacheItem<K, V>>(cacheItem);
                _lruList.AddLast(node);

                if (!_cacheMap.ContainsKey(key))
                    _cacheMap.Add(key, node);
                else
                    _cacheMap[key] = node;
            }
        }

        public V Remove(K key)
        {
            // Remove from LRUPriority
            LinkedListNode<LRUCacheItem<K, V>> node;
            if (_cacheMap.TryGetValue(key, out node))
            {
                // Remove from LRU
                _lruList.Remove(node);
                // Remove from cache
                _cacheMap.Remove(key);

                return node.Value.Value;
            }
            else
            {
                return default(V);
            }
        }

        #endregion

        #region Methods
            
        protected V RemoveFirst()
        {           
            // Remove from LRUPriority
            LinkedListNode<LRUCacheItem<K,V>> node = _lruList.First;
            V removed = _lruList.Count == _capacity ? _lruList.First.Value.Value : default(V);
            _lruList.RemoveFirst();
            // Remove from cache
            _cacheMap.Remove(node.Value.Key);
            return removed;
        }

        #endregion
    }        
}

