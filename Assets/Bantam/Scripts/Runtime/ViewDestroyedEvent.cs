using System;

namespace Bantam.Unity
{
	public class ViewDestroyedEvent : Event
	{
		public View view;

		public void Reset()
		{
			view = null;
		}
	}
}
	