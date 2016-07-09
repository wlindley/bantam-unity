using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;

namespace Bantam.Unity
{
	public class ViewSupervisor
	{
		private EventBus eventBus;
		private List<ViewBinding> viewBindings;
		private Dictionary<Type, List<View>> views;
		private Dictionary<Model, List<View>> modelViewMap;

		public ViewSupervisor(EventBus eventBus)
		{
			this.eventBus = eventBus;
			viewBindings = new List<ViewBinding>();
			views = new Dictionary<Type, List<View>>();
			modelViewMap = new Dictionary<Model, List<View>>();
			eventBus.AddListener<ModelCreatedEvent>(HandleModelCreated);
			eventBus.AddListener<ModelDestroyedEvent>(HandleModelDestroyed);
		}

		public ViewBindingContinuation<T> For<T>() where T : class, Model, new()
		{
			return new ViewBindingContinuation<T>(this);
		}

		public ReadOnlyCollection<View> GetViews<U>() where U : View
		{
			return views[typeof(U)].AsReadOnly();
		}

		internal void RegisterBinding(ViewBinding binding)
		{
			viewBindings.Add(binding);
		}

		internal void RegisterView<U>(Model model, View view) where U : View
		{
			var type = typeof(U);
			if (!views.ContainsKey(type))
				views[type] = new List<View>();
			if (!modelViewMap.ContainsKey(model))
				modelViewMap[model] = new List<View>();
			views[type].Add(view);
			modelViewMap[model].Add(view);
			eventBus.Dispatch<ViewCreatedEvent>(evt => {
				evt.view = view;
			});
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
				pair.Value.Remove(view);

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
			activeBinding.SetTargetGameObject(gameObj);
		}

		public void OnChildOf(GameObject gameObj)
		{
			activeBinding.SetParentGameObject(gameObj);
		}
	}

	internal interface ViewBinding
	{
		bool MatchesModelType(Type modelType);
		void CreateViewForModel(Model model);
		void SetTargetGameObject(GameObject gameObj);
		void SetParentGameObject(GameObject gameObj);
	}

	internal class ViewBinding<T, U> : ViewBinding where T : class, Model, new() where U : View<T>
	{
		private ViewSupervisor viewSupervisor;
		private GameObject targetGameObject;
		private GameObject parentGameObject;

		public ViewBinding(ViewSupervisor viewSupervisor)
		{
			this.viewSupervisor = viewSupervisor;
		}

		public bool MatchesModelType(Type modelType)
		{
			return typeof(T) == modelType;
		}

		public void CreateViewForModel(Model model)
		{
			var gameObj = GetGameObject();
			var view = gameObj.AddComponent<U>();
			view.Model = model as T;
			viewSupervisor.RegisterView<U>(model, view);
		}

		public void SetTargetGameObject(GameObject gameObj)
		{
			targetGameObject = gameObj;
		}

		public void SetParentGameObject(GameObject gameObj)
		{
			parentGameObject = gameObj;
		}

		private GameObject GetGameObject()
		{
			if (null != targetGameObject)
				return targetGameObject;
			var gameObj = new GameObject();
			if (null != parentGameObject)
			{
				gameObj.transform.parent = parentGameObject.transform;
				gameObj.transform.localPosition = Vector3.zero;
				gameObj.transform.localRotation = Quaternion.identity;
				gameObj.transform.localScale = Vector3.one;
			}
			return gameObj;
		}
	}
}
