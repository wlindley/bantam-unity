using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Bantam.Unity.Test
{
	public class ViewSupervisorTest
	{
		private ViewSupervisor testObj;
		private ModelRegistry modelRegistry;
		private EventBus eventBus;

		[SetUp]
		public void SetUp()
		{
			var pool = new ObjectPool();
			eventBus = new EventBus(pool);
			modelRegistry = new ModelRegistry(pool, eventBus);
			testObj = new ViewSupervisor(eventBus);
		}

		[Test]
		public void BindingViewCausesItToBeInstantiatedWhenModelIsCreated()
		{
			testObj.For<DummyModel>().Create<DummyView>();
			modelRegistry.Create<DummyModel>();
			Assert.AreEqual (1, testObj.GetViews<DummyView>().Count);
		}

		[Test]
		public void BindingViewToNewGameObjectCausesNewGameObjectToBeInstantiated()
		{
			testObj.For<DummyModel>().Create<DummyView>().OnNewGameObject();
			modelRegistry.Create<DummyModel>();
			Assert.AreEqual (1, testObj.GetViews<DummyView>().Count);
			Assert.NotNull(testObj.GetViews<DummyView>()[0].gameObject);
		}

		[Test]
		public void InstantiatedViewHasModelInjected()
		{
			DummyModel model = null;
			testObj.For<DummyModel>().Create<DummyView>().OnNewGameObject();
			modelRegistry.Create<DummyModel>(mdl => model = mdl);
			var view = testObj.GetViews<DummyView>()[0] as DummyView;
			Assert.NotNull(view.Model);
			Assert.AreEqual(model, view.Model);
		}

		[Test]
		public void BindingViewToExistingGameObjectCausesComponentToBeAddedToThatObject()
		{
			var gameObj = new GameObject();
			testObj.For<DummyModel>().Create<DummyView>().OnExistingGameObject(gameObj);
			modelRegistry.Create<DummyModel>();
			Assert.AreEqual (1, testObj.GetViews<DummyView>().Count);
			Assert.AreEqual(gameObj, testObj.GetViews<DummyView>()[0].gameObject);
		}

		[Test]
		public void BindingViewToChildOfGameObjectCausesNewGameObjectToBeCreatedAsChild()
		{
			var gameObj = new GameObject();
			testObj.For<DummyModel>().Create<DummyView>().OnChildOf(gameObj);
			modelRegistry.Create<DummyModel>();
			Assert.AreEqual (1, testObj.GetViews<DummyView>().Count);
			Assert.NotNull(testObj.GetViews<DummyView>()[0].gameObject);
			Assert.AreEqual(gameObj.transform, testObj.GetViews<DummyView>()[0].transform.parent);
		}

		[Test]
		public void BindingViewToChildOfGameObjectCausesChildTransformToBeResetAfterParenting()
		{
			var gameObj = new GameObject();
			gameObj.transform.position = new Vector3(1, 1, 1);
			gameObj.transform.rotation = new Quaternion(2, 5, 8, 1);
			testObj.For<DummyModel>().Create<DummyView>().OnChildOf(gameObj);
			modelRegistry.Create<DummyModel>();
			var childObj = testObj.GetViews<DummyView>()[0];
			Assert.AreEqual(Vector3.zero, childObj.transform.localPosition);
			Assert.AreEqual(Quaternion.identity, childObj.transform.localRotation);
			Assert.AreEqual(Vector3.one, childObj.transform.localScale);
		}

		[Test]
		public void BoundViewIsDestroyedWhenModelIsDestroyed()
		{
			testObj.For<DummyModel>().Create<DummyView>();
			DummyModel model = null;
			modelRegistry.Create<DummyModel>(mdl => model = mdl);
			modelRegistry.Destroy<DummyModel>(model);
			Assert.AreEqual(0, testObj.GetViews<DummyView>().Count);
		}

		[Test]
		public void DispatchesEventWhenViewIsCreated()
		{
			var wasCalled = false;
			testObj.For<DummyModel>().Create<DummyView>();
			eventBus.AddListener<ViewCreatedEvent>(evt => {
				var viewObject = testObj.GetViews<DummyView>()[0];
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
			var expectedView = testObj.GetViews<DummyView>()[0];

			eventBus.AddListener<ViewDestroyedEvent>(evt => {
				Assert.AreEqual(expectedView, evt.view);
				wasCalled = true;
			});
			modelRegistry.Destroy<DummyModel>(expectedModel);

			Assert.IsTrue(wasCalled);
		}
	}

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
