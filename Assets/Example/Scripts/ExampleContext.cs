using Bantam;
using Bantam.Unity;

public class ExampleContext : Context
{
	public override void Init()
	{
		InstantiateObjects();
		ConfigureCommands();
	}

	private void InstantiateObjects()
	{
		BindInstance<ObjectPool>(new ObjectPool());
		BindInstance<EventBus>(new EventBus(GetInstance<ObjectPool>()));
		BindInstance<CommandRelay>(new CommandRelay(GetInstance<EventBus>(), GetInstance<ObjectPool>()));
		BindInstance<ModelRegistry>(new ModelRegistry(GetInstance<ObjectPool>(), GetInstance<EventBus>()));
		BindInstance<ViewSupervisor>(new ViewSupervisor(GetInstance<EventBus>()));
	}

	private void ConfigureCommands()
	{
		
	}
}
