using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace Bantam.Unity.Test
{
	public class ViewSupervisorTest
	{
		private ViewSupervisor testObj;
		private ModelRegistry modelRegistry;
		private EventBus eventBus;
		private GameObject gameObj;

		[SetUp]
		public void SetUp()
		{
			var pool = new ObjectPool();
			eventBus = new EventBus(pool);
			modelRegistry = new ModelRegistry(pool, eventBus);
			testObj = new ViewSupervisor(eventBus);
		}

		[TearDown]
		public void TearDown()
		{
			var views = testObj.GetViews<DummyView>();
			if (null != views)
				foreach (var view in views)
					GameObject.DestroyImmediate(view.gameObject);

			if (null != gameObj)
			{
				GameObject.DestroyImmediate(gameObj);
				gameObj = null;
			}
		}

		[Test]
		public void BindingViewCausesItToBeInstantiatedWhenModelIsCreated()
		{
			testObj.For<DummyModel>().Create<DummyView>();
			modelRegistry.Create<DummyModel>();
			Assert.AreEqual(1, testObj.GetViews<DummyView>().Count());
		}

		[Test]
		public void BindingViewToNewGameObjectCausesNewGameObjectToBeInstantiated()
		{
			testObj.For<DummyModel>().Create<DummyView>().OnNewGameObject();
			modelRegistry.Create<DummyModel>();
			Assert.AreEqual(1, testObj.GetViews<DummyView>().Count());
			Assert.NotNull(testObj.GetViews<DummyView>().First().gameObject);
		}

		[Test]
		public void InstantiatedViewHasModelInjected()
		{
			DummyModel model = null;
			testObj.For<DummyModel>().Create<DummyView>().OnNewGameObject();
			modelRegistry.Create<DummyModel>(mdl => model = mdl);
			var view = testObj.GetViews<DummyView>().First() as DummyView;
			Assert.NotNull(view.Model);
			Assert.AreEqual(model, view.Model);
		}

		[Test]
		public void BindingViewToExistingGameObjectCausesComponentToBeAddedToThatObject()
		{
			gameObj = new GameObject();
			testObj.For<DummyModel>().Create<DummyView>().OnExistingGameObject(gameObj);
			modelRegistry.Create<DummyModel>();
			Assert.AreEqual(1, testObj.GetViews<DummyView>().Count());
			Assert.AreEqual(gameObj, testObj.GetViews<DummyView>().First().gameObject);
		}

		[Test]
		public void BindingViewToChildOfGameObjectCausesNewGameObjectToBeCreatedAsChild()
		{
			gameObj = new GameObject();
			testObj.For<DummyModel>().Create<DummyView>().OnChildOf(gameObj);
			modelRegistry.Create<DummyModel>();
			Assert.AreEqual(1, testObj.GetViews<DummyView>().Count());
			Assert.NotNull(testObj.GetViews<DummyView>().First().gameObject);
			Assert.AreEqual(gameObj.transform, testObj.GetViews<DummyView>().First().transform.parent);
		}

		[Test]
		public void BindingViewToChildOfGameObjectCausesChildTransformToBeResetAfterParenting()
		{
			gameObj = new GameObject();
			gameObj.transform.position = new Vector3(1, 1, 1);
			gameObj.transform.rotation = new Quaternion(2, 5, 8, 1);
			testObj.For<DummyModel>().Create<DummyView>().OnChildOf(gameObj);
			modelRegistry.Create<DummyModel>();
			var childObj = testObj.GetViews<DummyView>().First();
			Assert.AreEqual(Vector3.zero, childObj.transform.localPosition);
			Assert.AreEqual(Quaternion.identity, childObj.transform.localRotation);
			Assert.AreEqual(Vector3.one, childObj.transform.localScale);
		}

		[Test]
		public void BindingViewToPrefabInstantiatesInstanceOfThatPrefabAndAddsViewIfNotPresent()
		{
			var prefab = Resources.Load<GameObject>("EmptyPrefab");
			testObj.For<DummyModel>().Create<DummyView>().UsingPrefab(prefab);
			modelRegistry.Create<DummyModel>();
			Assert.AreEqual(1, testObj.GetViews<DummyView>().Count());
			Assert.NotNull(testObj.GetViews<DummyView>().First());
			Assert.NotNull(testObj.GetViews<DummyView>().First().Model);
		}

		[Test]
		public void BindingViewToPrefabInstantiatesInstanceOfThatPrefabAndUsesExistingViewIfPresent()
		{
			var prefab = Resources.Load<GameObject>("DummyViewPrefab");
			testObj.For<DummyModel>().Create<DummyView>().UsingPrefab(prefab);
			modelRegistry.Create<DummyModel>();
			Assert.AreEqual(1, testObj.GetViews<DummyView>().Count());
			Assert.NotNull(testObj.GetViews<DummyView>().First());
			Assert.NotNull(testObj.GetViews<DummyView>().First().Model);
			Assert.AreEqual(1, testObj.GetViews<DummyView>().First().GetComponents<DummyView>().Count());
		}

		[Test]
		public void BindingViewToPrefabWithParentInstantiatesPrefabAndReparentsIt()
		{
			gameObj = new GameObject();
			var prefab = Resources.Load<GameObject>("EmptyPrefab");
			testObj.For<DummyModel>().Create<DummyView>().UsingPrefab(prefab).OnChildOf(gameObj);
			modelRegistry.Create<DummyModel>();
			Assert.AreEqual(gameObj.transform, testObj.GetViews<DummyView>().First().transform.parent);
		}

		[Test]
		public void BindingViewToPrefabAndExistingGameObjectThrowsException()
		{
			gameObj = new GameObject();
			var prefab = Resources.Load<GameObject>("EmptyPrefab");
			Assert.Throws<InvalidOperationException>(() => {
				testObj.For<DummyModel>().Create<DummyView>().UsingPrefab(prefab).OnExistingGameObject(gameObj);
			});
		}

		[Test]
		public void BoundViewIsDestroyedWhenModelIsDestroyed()
		{
			testObj.For<DummyModel>().Create<DummyView>();
			DummyModel model = null;
			modelRegistry.Create<DummyModel>(mdl => model = mdl);
			gameObj = testObj.GetViewForModel<DummyView>(model).gameObject;
			modelRegistry.Destroy<DummyModel>(model);
			Assert.AreEqual(0, testObj.GetViews<DummyView>().Count());
		}

		[Test]
		public void DispatchesEventWhenViewIsCreated()
		{
			var wasCalled = false;
			testObj.For<DummyModel>().Create<DummyView>();
			eventBus.AddListener<ViewCreatedEvent>(evt => {
				var viewObject = testObj.GetViews<DummyView>().First();
				Assert.AreEqual(viewObject, evt.view);
				wasCalled = true;
			});
			modelRegistry.Create<DummyModel>();
			Assert.IsTrue(wasCalled);
		}

		[Test]
		public void DispatchesEventWhenViewIsDestroyed()
		{
			var wasCalled = false;
			testObj.For<DummyModel>().Create<DummyView>();
			DummyModel expectedModel = null;
			modelRegistry.Create<DummyModel>(mdl => expectedModel = mdl);
			var expectedView = testObj.GetViews<DummyView>().First();
			gameObj = expectedView.gameObject;

			eventBus.AddListener<ViewDestroyedEvent>(evt => {
				Assert.AreEqual(expectedView, evt.view);
				wasCalled = true;
			});
			modelRegistry.Destroy<DummyModel>(expectedModel);

			Assert.IsTrue(wasCalled);
		}

		[Test]
		public void GetViewForModelReturnsViewCreatedForTheGivenModel()
		{
			DummyModel model = null;
			testObj.For<DummyModel>().Create<DummyView>().OnNewGameObject();

			modelRegistry.Create<DummyModel>();
			modelRegistry.Create<DummyModel>(mdl => model = mdl);
			modelRegistry.Create<DummyModel>();

			var view = testObj.GetViewForModel<DummyView>(model);
			Assert.IsNotNull(view);
			Assert.AreEqual(model, view.Model);
		}

		[Test]
		public void GetViewForModelReturnsNullWhenNoViewExistsForModel()
		{
			DummyModel model = null;
			modelRegistry.Create<DummyModel>(mdl => model = mdl);
			testObj.For<DummyModel>().Create<DummyView>().OnNewGameObject();

			modelRegistry.Create<DummyModel>();
			modelRegistry.Create<DummyModel>();

			var view = testObj.GetViewForModel<DummyView>(model);
			Assert.IsNull(view);
		}
	}
}
