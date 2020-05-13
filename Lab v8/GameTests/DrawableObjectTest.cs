using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using Template;
using Template.Game.gameObjects.newObjects;

namespace GameTests
{
    [TestClass]
    public class DrawableObjectTest
    {
        [TestMethod]
        public void SetPositionTest()
        {
            List<MeshObject> meshObjects = new List<MeshObject>
            {
                new MeshObject(Vector4.Zero),
                new MeshObject(Vector4.Zero),
                new MeshObject(Vector4.Zero)
            };

            DrawableObject testObj = new DrawableObject(Vector4.One);
            testObj.AddMeshObjects(meshObjects);

            foreach(var obj in testObj.MeshObjects)
            {
                Assert.AreEqual(testObj.Position, obj.Position);
            }
        }

        [TestMethod]
        public void ChangePositionTest()
        {
            List<MeshObject> meshObjects = new List<MeshObject>
            {
                new MeshObject(Vector4.Zero),
                new MeshObject(Vector4.Zero),
                new MeshObject(Vector4.Zero)
            };

            DrawableObject testObj = new DrawableObject(Vector4.Zero);
            testObj.AddMeshObjects(meshObjects);

            testObj.Position = Vector4.One;

            foreach (var obj in testObj.MeshObjects)
            {
                Assert.AreEqual(testObj.Position, obj.Position);
            }
        }
    }
}
