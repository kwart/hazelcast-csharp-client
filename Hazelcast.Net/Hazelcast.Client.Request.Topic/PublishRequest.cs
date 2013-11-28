using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Topic
{
	
	public class PublishRequest : IPortable
	{
		internal string name;

		private Data message;

		public PublishRequest()
		{
		}

		public PublishRequest(string name, Data message)
		{
			this.name = name;
			this.message = message;
		}

		public virtual int GetFactoryId()
		{
			return TopicPortableHook.FId;
		}

		public virtual int GetClassId()
		{
			return TopicPortableHook.Publish;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void WritePortable(IPortableWriter writer)
		{
			writer.WriteUTF("n", name);
			IObjectDataOutput output = writer.GetRawDataOutput();
			message.WriteData(output);
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void ReadPortable(IPortableReader reader)
		{
			name = reader.ReadUTF("n");
			IObjectDataInput input = reader.GetRawDataInput();
			message = new Data();
			message.ReadData(input);
		}
	}
}