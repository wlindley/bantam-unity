namespace Bantam.Unity
{
	public class ViewCreatedEvent : Event
	{
		public View view;

		public void Reset()
		{
			view = null;
		}
	}
}
