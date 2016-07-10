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
		void SetPrefab(GameObject prefab);
	}

	internal class ViewBinding<T, U> : ViewBinding where T : class, Model, new() where U : View<T>
	{
		private ViewSupervisor viewSupervisor;
		private GameObject targetGameObject;
		private GameObject parentGameObject;
		private GameObject prefab;

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
			var view = GetView(gameObj);
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

		public void SetPrefab(GameObject prefab)
		{
			this.prefab = prefab;
		}

		private GameObject GetGameObject()
		{
			if (null != targetGameObject)
				return targetGameObject;

			GameObject gameObj;
			if (null == prefab)
				gameObj = new GameObject(typeof(U).ToString());
			else
				gameObj = GameObject.Instantiate(prefab);

			ReparentIfNecessary(gameObj);
			return gameObj;
		}

		private void ReparentIfNecessary(GameObject gameObj)
		{
			if (null != parentGameObject)
			{
				gameObj.transform.parent = parentGameObject.transform;
				gameObj.transform.localPosition = Vector3.zero;
				gameObj.transform.localRotation = Quaternion.identity;
				gameObj.transform.localScale = Vector3.one;
			}
		}

		private U GetView(GameObject gameObj)
		{
			var view = gameObj.GetComponent<U>();
			if (null != prefab && null != view)
				return view;
			return gameObj.AddComponent<U>();
		}
	}
}
	