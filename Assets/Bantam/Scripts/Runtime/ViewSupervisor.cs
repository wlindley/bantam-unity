using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bantam.Unity
{
	public class ViewSupervisor
	{
		private EventBus eventBus;
		private List<ViewBinding> viewBindings;
		private Dictionary<Type, ViewListWrapper> views;
		private Dictionary<Model, List<View>> modelViewMap;

		public ViewSupervisor(EventBus eventBus)
		{
			this.eventBus = eventBus;
			viewBindings = new List<ViewBinding>();
			views = new Dictionary<Type, ViewListWrapper>();
			modelViewMap = new Dictionary<Model, List<View>>();

			eventBus.AddListener<ModelCreatedEvent>(HandleModelCreated);
			eventBus.AddListener<ModelDestroyedEvent>(HandleModelDestroyed);
		}

		public ViewBindingContinuation<T> For<T>() where T : class, Model, new()
		{
			return new ViewBindingContinuation<T>(this);
		}

		public IEnumerable<U> GetViews<U>() where U : View
		{
			var type = typeof(U);
			if (!views.ContainsKey (type))
				return null;
			return views[type].GetViews<U>();
		}

		public U GetViewForModel<U>(Model model) where U : View
		{
			List<View> modelViews;
			if (!modelViewMap.TryGetValue(model, out modelViews))
				return null;

			foreach (var view in modelViews)
			{
				var specificView = view as U;
				if (null != specificView)
					return specificView;
			}
			return null;
		}

		internal void RegisterBinding(ViewBinding binding)
		{
			viewBindings.Add(binding);
		}

		internal void RegisterView<U>(Model model, View view) where U : View
		{
			EnsureViewModelExists<U>(model);

			views[typeof(U)].AddView<U>(view as U);
			modelViewMap[model].Add(view);

			eventBus.Dispatch<ViewCreatedEvent>(evt => {
				evt.view = view;
			});
		}

		private void EnsureViewModelExists<U>(Model model) where U : View
		{
			var type = typeof(U);
			if (!views.ContainsKey(type))
				views[type] = new ViewListWrapper<U>();
			if (!modelViewMap.ContainsKey(model))
				modelViewMap[model] = new List<View>();
		}

		private void HandleModelCreated(ModelCreatedEvent evt)
		{
			foreach (var binding in viewBindings)
				if (binding.MatchesModelType(evt.type))
					binding.CreateViewForModel(evt.model);
		}

		private void HandleModelDestroyed(ModelDestroyedEvent evt)
		{
			foreach (var pair in modelViewMap)
			{
				if (evt.model == pair.Key)
				{
					foreach (var view in pair.Value)
						DestroyView(view);
					pair.Value.Clear();
				}
			}
		}

		private void DestroyView(View view)
		{
			foreach (var pair in views)
				pair.Value.RemoveView(view);

			eventBus.Dispatch<ViewDestroyedEvent>(evt => {
				evt.view = view;
			});

			#if UNITY_EDITOR
				GameObject.DestroyImmediate(view);
			#else
				GameObject.Destroy(view);
			#endif
		}
	}

	public class ViewBindingContinuation<T> where T : class, Model, new()
	{
		private ViewSupervisor viewSupervisor;
		private ViewBinding activeBinding;
		private bool isPrefabSet;

		internal ViewBindingContinuation(ViewSupervisor viewSupervisor)
		{
			this.viewSupervisor = viewSupervisor;
		}

		public ViewBindingContinuation<T> Create<U>() where U : View<T>
		{
			activeBinding = new ViewBinding<T, U>(viewSupervisor);
			viewSupervisor.RegisterBinding(activeBinding);
			return this;
		}

		public void OnNewGameObject()
		{
			
		}

		public void OnExistingGameObject(GameObject gameObj)
		{
			if (isPrefabSet)
				throw new InvalidOperationException("Cannot use View prefab on existing GameObject.");
			activeBinding.SetTargetGameObject(gameObj);
		}

		public void OnChildOf(GameObject gameObj)
		{
			activeBinding.SetParentGameObject(gameObj);
		}

		public ViewBindingContinuation<T> UsingPrefab(GameObject prefab)
		{
			isPrefabSet = true;
			activeBinding.SetPrefab(prefab);
			return this;
		}
	}

	internal interface ViewListWrapper
	{
		void AddView<U>(U view) where U : View;
		void RemoveView(View view);
		IEnumerable<U> GetViews<U>() where U : View;
	}

	internal class ViewListWrapper<T> : ViewListWrapper where T : View
	{
		private List<T> views = new List<T>();

		public void AddView<U>(U view) where U : View
		{
			views.Add(view as T);
		}

		public void RemoveView(View view)
		{
			views.Remove(view as T);
		}

		public IEnumerable<U> GetViews<U>() where U : View
		{
			return views as IEnumerable<U>;
		}
	}
}
