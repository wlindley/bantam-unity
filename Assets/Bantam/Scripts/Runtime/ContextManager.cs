using UnityEngine;

namespace Bantam.Unity
{
	public class ContextManager<T> : MonoBehaviour, ContextOwner where T : Context, new()
	{
		private T context = new T();

		public void Awake()
		{
			context.Init();
		}

		public Context Context
		{
			get { return context; }
		}
	}
}
