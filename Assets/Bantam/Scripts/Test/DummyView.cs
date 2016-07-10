using Bantam;
using System.Collections.Generic;

namespace Bantam.Unity.Test
{
	public class DummyModel : Model
	{
		public void Reset ()
		{
		}
	}

	public class DummyView : View<DummyModel>
	{
		public static List<DummyView> instances = new List<DummyView>();

		public void Awake()
		{
			instances.Add(this);
		}
	}
}

