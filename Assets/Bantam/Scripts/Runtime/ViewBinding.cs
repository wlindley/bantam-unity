using System;
using UnityEngine;

namespace Bantam.Unity
{
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
			ReparentIfNecessary(gameObj);
			return gameObj;
		}

		void ReparentIfNecessary(GameObject gameObj)
		{
			if (null != parentGameObject)
			{
				gameObj.transform.parent = parentGameObject.transform;
				gameObj.transform.localPosition = Vector3.zero;
				gameObj.transform.localRotation = Quaternion.identity;
				gameObj.transform.localScale = Vector3.one;
			}
		}
	}
}
	