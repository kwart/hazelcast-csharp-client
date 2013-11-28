using System;
using Hazelcast.Client.Request.Base;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;
using Hazelcast.Util;


namespace Hazelcast.Client.Request.Map
{
    public abstract class AbstractMapQueryRequest : IPortable, IRetryableRequest
	{
		private string name;

		private IterationType iterationType;

		public AbstractMapQueryRequest()
		{
		}

		public AbstractMapQueryRequest(string name, IterationType iterationType)
		{
			this.name = name;
			this.iterationType = iterationType;
		}

		public int GetFactoryId()
		{
			return MapPortableHook.FId;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void WritePortable(IPortableWriter writer)
		{
			writer.WriteUTF("n", name);
			writer.WriteUTF("t", iterationType.ToString());
			WritePortableInner(writer);
		}

		/// <exception cref="System.IO.IOException"></exception>
		protected internal abstract void WritePortableInner(IPortableWriter writer);

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void ReadPortable(IPortableReader reader)
		{
			name = reader.ReadUTF("n");
            Enum.TryParse(reader.ReadUTF("t"), out iterationType);
			ReadPortableInner(reader);
		}

		/// <exception cref="System.IO.IOException"></exception>
		protected internal abstract void ReadPortableInner(IPortableReader reader);

		public abstract int GetClassId();
	}
}