using System;
using System.Collections.Generic;
using Hazelcast.Client.Request.Multimap;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    internal class ClientMultiMapProxy<K, V> : ClientProxy, IMultiMap<K, V>
    {
        private readonly string name;

        public ClientMultiMapProxy(string serviceName, string name) : base(serviceName, name)
        {
            this.name = name;
        }

        public virtual bool Put(K key, V value)
        {
            IData keyData = GetSerializationService().ToData(key);
            IData valueData = GetSerializationService().ToData(value);
            var request = new PutRequest(name, keyData, valueData, -1, ThreadUtil.GetThreadId());
            var result = Invoke<bool>(request, keyData);
            return result;
        }

        public virtual ICollection<V> Get(K key)
        {
            IData keyData = GetSerializationService().ToData(key);
            var request = new GetAllRequest(name, keyData, ThreadUtil.GetThreadId());
            var result = Invoke<PortableCollection>(request, keyData);
            return ToObjectCollection<V>(result, true);
        }

        public virtual bool Remove(object key, object value)
        {
            IData keyData = GetSerializationService().ToData(key);
            IData valueData = GetSerializationService().ToData(value);
            var request = new RemoveRequest(name, keyData, valueData, ThreadUtil.GetThreadId());
            var result = Invoke<bool>(request, keyData);
            return result;
        }

        public virtual ICollection<V> Remove(object key)
        {
            IData keyData = GetSerializationService().ToData(key);
            var request = new RemoveAllRequest(name, keyData, ThreadUtil.GetThreadId());
            var result = Invoke<PortableCollection>(request, keyData);
            return ToObjectCollection<V>(result, true);
        }

        public virtual ISet<K> KeySet()
        {
            var request = new KeySetRequest(name);
            var result = Invoke<PortableCollection>(request);
            return (ISet<K>) ToObjectCollection<K>(result, false);
        }

        public virtual ICollection<V> Values()
        {
            var request = new ValuesRequest(name);
            var result = Invoke<PortableCollection>(request);
            return ToObjectCollection<V>(result, true);
        }

        public virtual ISet<KeyValuePair<K, V>> EntrySet()
        {
            var request = new EntrySetRequest(name);
            var result = Invoke<PortableEntrySetResponse>(request);
            ICollection<KeyValuePair<IData, IData>> dataEntrySet = result.EntrySet;
            ISet<KeyValuePair<K, V>> entrySet = new HashSet<KeyValuePair<K, V>>();
            foreach (var entry in dataEntrySet)
            {
                var key = GetSerializationService().ToObject<K>(entry.Key);
                var val = GetSerializationService().ToObject<V>(entry.Value);
                entrySet.Add(new KeyValuePair<K, V>(key, val));
            }
            return entrySet;
        }

        public virtual bool ContainsKey(K key)
        {
            IData keyData = GetSerializationService().ToData(key);
            var request = new KeyBasedContainsRequest(name, keyData, null, ThreadUtil.GetThreadId());
            var result = Invoke<bool>(request, keyData);
            return result;
        }

        public virtual bool ContainsValue(object value)
        {
            IData valueData = GetSerializationService().ToData(value);
            var request = new ContainsRequest(name, valueData);
            var result = Invoke<bool>(request);
            return result;
        }

        public virtual bool ContainsEntry(K key, V value)
        {
            IData keyData = GetSerializationService().ToData(key);
            IData valueData = GetSerializationService().ToData(value);
            var request = new KeyBasedContainsRequest(name, keyData, valueData, ThreadUtil.GetThreadId());
            var result = Invoke<bool>(request, keyData);
            return result;
        }

        public virtual int Size()
        {
            var request = new SizeRequest(name);
            var result = Invoke<int>(request);
            return result;
        }

        public virtual void Clear()
        {
            var request = new ClearRequest(name);
            Invoke<object>(request);
        }

        public virtual int ValueCount(K key)
        {
            IData keyData = GetSerializationService().ToData(key);
            var request = new CountRequest(name, keyData, ThreadUtil.GetThreadId());
            var result = Invoke<int>(request, keyData);
            return result;
        }

        public virtual string AddEntryListener(IEntryListener<K, V> listener, bool includeValue)
        {
            var request = new AddEntryListenerRequest(name, null, includeValue);
            DistributedEventHandler handler = eventData => OnEntryEvent(eventData, includeValue, listener);

            return Listen(request, handler);
        }

        public virtual bool RemoveEntryListener(string registrationId)
        {
            var request = new RemoveEntryListenerRequest(name, registrationId);
            return StopListening(request, registrationId);
        }

        public virtual string AddEntryListener(IEntryListener<K, V> listener, K key, bool includeValue)
        {
            IData keyData = GetSerializationService().ToData(key);
            var request = new AddEntryListenerRequest(name, keyData, includeValue);
            DistributedEventHandler handler = eventData => OnEntryEvent(eventData, includeValue, listener);
            return Listen(request, keyData, handler);
        }

        public virtual void Lock(K key)
        {
            IData keyData = GetSerializationService().ToData(key);
            var request = new MultiMapLockRequest(keyData, ThreadUtil.GetThreadId(), name);
            Invoke<object>(request, keyData);
        }

        public virtual void Lock(K key, long leaseTime, TimeUnit timeUnit)
        {
            IData keyData = GetSerializationService().ToData(key);
            var request = new MultiMapLockRequest(keyData, ThreadUtil.GetThreadId(),
                GetTimeInMillis(leaseTime, timeUnit), -1, name);
            Invoke<object>(request, keyData);
        }

        public virtual bool IsLocked(K key)
        {
            IData keyData = GetSerializationService().ToData(key);
            var request = new MultiMapIsLockedRequest(keyData, name);
            var result = Invoke<bool>(request, keyData);
            return result;
        }

        public virtual bool TryLock(K key)
        {
            try
            {
                return TryLock(key, 0, TimeUnit.SECONDS);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <exception cref="System.Exception"></exception>
        public virtual bool TryLock(K key, long time, TimeUnit timeunit)
        {
            IData keyData = GetSerializationService().ToData(key);
            var request = new MultiMapLockRequest(keyData, ThreadUtil.GetThreadId(), long.MaxValue,
                GetTimeInMillis(time, timeunit), name);
            var result = Invoke<bool>(request, keyData);
            return result;
        }

        public virtual void Unlock(K key)
        {
            IData keyData = GetSerializationService().ToData(key);
            var request = new MultiMapUnlockRequest(keyData, ThreadUtil.GetThreadId(), name);
            Invoke<bool>(request, keyData);
        }

        public virtual void ForceUnlock(K key)
        {
            IData keyData = GetSerializationService().ToData(key);
            var request = new MultiMapUnlockRequest(keyData, ThreadUtil.GetThreadId(), true, name);
            Invoke<bool>(request, keyData);
        }

        protected override void OnDestroy()
        {
        }


        private ICollection<T> ToObjectCollection<T>(PortableCollection result, bool list)
        {
            ICollection<IData> coll = result.GetCollection();
            ICollection<T> collection;
            if (list)
            {
                collection = new List<T>(coll == null ? 0 : coll.Count);
            }
            else
            {
                collection = new HashSet<T>();
            }
            if (coll == null)
            {
                return collection;
            }
            foreach (IData data in coll)
            {
                var item = GetSerializationService().ToObject<T>(data);
                collection.Add(item);
            }
            return collection;
        }

        private ISerializationService GetSerializationService()
        {
            return GetContext().GetSerializationService();
        }

        protected internal virtual long GetTimeInMillis(long time, TimeUnit timeunit)
        {
            return timeunit != null ? timeunit.ToMillis(time) : time;
        }

        public void OnEntryEvent(IData eventData, bool includeValue, IEntryListener<K, V> listener)
        {
            var _event = ToObject<PortableEntryEvent>(eventData);
            V value = default(V);
            V oldValue = default(V);
            if (includeValue)
            {
                value = GetSerializationService().ToObject<V>(_event.GetValue());
                oldValue = GetSerializationService().ToObject<V>(_event.GetOldValue());
            }
            var key = GetSerializationService().ToObject<K>(_event.GetKey());
            var member = GetContext().GetClusterService().GetMember(_event.GetUuid());
            switch (_event.GetEventType())
            {
                case EntryEventType.Added:
                {
                    listener.EntryAdded(new EntryEvent<K, V>(name, member, _event.GetEventType(), key, oldValue, value));
                    break;
                }
                case EntryEventType.Removed:
                {
                    listener.EntryRemoved(new EntryEvent<K, V>(name, member, _event.GetEventType(), key, oldValue, value));
                    break;
                }
                case EntryEventType.ClearAll:
                {
                    listener.MapCleared(new MapEvent(name, member, _event.GetEventType(), _event.GetNumberOfAffectedEntries()));
                    break;
                }
            }
        }
    }
}