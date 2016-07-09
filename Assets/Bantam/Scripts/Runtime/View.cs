using UnityEngine;

namespace Bantam.Unity
{
	public abstract class View : MonoBehaviour {}

	public abstract class View<T> : View where T : class, Model
	{
		public T Model { get; set; }
	}
}
