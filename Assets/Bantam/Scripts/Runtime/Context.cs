using System.Collections.Generic;

namespace Bantam.Unity
{
	public abstract class Context
	{
		private readonly List<Factory> factories = new List<Factory>();

		public abstract void Init();

		public virtual T GetInstance<T>(string id="") where T : class
		{
			var type = typeof(T);
			var numFactories = factories.Count;
			for (var i = 0; i < numFactories; i++)
			{
				var instance = factories[i].Build(type, id);
				if (null != instance)
					return instance as T;
			}
			return null;
		}

		public virtual void Inject<T>(out T injectee, string id="") where T : class
		{
			injectee = GetInstance<T>(id);
		}

		public virtual void BindSingleton<T>(string id="") where T : class, new()
		{
			factories.Add(new SingletonFactory<T>(id));
		}

		public virtual void BindTransient<T>(string id="") where T : class, new()
		{
			factories.Add(new TransientFactory<T>(id));
		}

		public virtual void BindInstance<T>(T instance, string id="") where T : class
		{
			factories.Add(new InstanceFactory<T>(instance, id));
		}

		public virtual void BindCustom(Factory factory)
		{
			factories.Add(factory);
		}
	}
}

