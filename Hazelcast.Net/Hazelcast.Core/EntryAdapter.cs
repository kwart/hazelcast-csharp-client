﻿using System;

namespace Hazelcast.Core
{
    public class EntryAdapter<K, V> : IEntryListener<K, V>
    {
        private readonly Action<EntryEvent<K, V>> fAdded;
        private readonly Action<EntryEvent<K, V>> fEvicted;
        private readonly Action<EntryEvent<K, V>> fRemoved;
        private readonly Action<EntryEvent<K, V>> fUpdated;
        private readonly Action<MapEvent> fEvictAll;
        private readonly Action<MapEvent> fClearAll;

        public EntryAdapter(Action<EntryEvent<K, V>> fAdded, Action<EntryEvent<K, V>> fRemoved,
            Action<EntryEvent<K, V>> fUpdated, Action<EntryEvent<K, V>> fEvicted)
        {
            this.fAdded = fAdded;
            this.fRemoved = fRemoved;
            this.fUpdated = fUpdated;
            this.fEvicted = fEvicted;
        }

        public EntryAdapter(Action<EntryEvent<K, V>> fAdded, Action<EntryEvent<K, V>> fRemoved,
            Action<EntryEvent<K, V>> fUpdated, Action<EntryEvent<K, V>> fEvicted, Action<MapEvent> fEvictAll, Action<MapEvent> fClearAll)
        {
            this.fAdded = fAdded;
            this.fRemoved = fRemoved;
            this.fUpdated = fUpdated;
            this.fEvicted = fEvicted;
            this.fEvictAll = fEvictAll;
            this.fClearAll = fClearAll;
        }


        public void EntryAdded(EntryEvent<K, V> @event)
        {
            fAdded(@event);
        }

        public void EntryRemoved(EntryEvent<K, V> @event)
        {
            fRemoved(@event);
        }

        public void EntryUpdated(EntryEvent<K, V> @event)
        {
            fUpdated(@event);
        }

        public void EntryEvicted(EntryEvent<K, V> @event)
        {
            fEvicted(@event);
        }

        public void MapEvicted(MapEvent @event)
        {
            fEvictAll(@event);
        }

        public void MapCleared(MapEvent @event)
        {
            fClearAll(@event);
        }
    }
}