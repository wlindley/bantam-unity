using System;

namespace Bantam.Unity
{
	public interface Factory
	{
		object Build(Type type, string id);
	}

	internal abstract class BaseFactory : Factory
	{
		protected string id;

		internal BaseFactory(string id)
		{
			this.id = id;
		}

		public abstract object Build(Type Type, string id);
	}

	internal class SingletonFactory<T> : BaseFactory where T : class, new()
	{
		private T instance;

		internal SingletonFactory(string id) : base(id) {}

		public override object Build(Type type, string id)
		{
			if (typeof(T) != type || this.id != id)
				return null;
			if (null == instance)
				instance = new T();
			return instance;
		}
	}

	internal class TransientFactory<T> : BaseFactory where T : class, new()
	{
		internal TransientFactory(string id) : base(id) {}

		public override object Build(Type type, string id)
		{
			if (typeof(T) != type || this.id != id)
				return null;
			return new T();
		}
	}

	internal class InstanceFactory<T> : BaseFactory where T : class
	{
		private T instance;

		internal InstanceFactory(T instance, string id) : base(id)
		{
			this.instance = instance;
		}

		public override object Build(Type type, string id)
		{
			if (typeof(T) != type || this.id != id)
				return null;
			return instance;
		}
	}
}
