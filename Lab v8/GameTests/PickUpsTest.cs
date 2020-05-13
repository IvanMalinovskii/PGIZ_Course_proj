using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using Template.Game.gameObjects.newObjects;
using Template.Game.GameObjects.Objects;
using Template.Game.GameObjects.Objects.PickUps;

namespace GameTests
{
    [TestClass]
    public class PickUpsTest
    {
        Archer archer = new Archer(Vector4.Zero);

        [TestMethod]
        public void MoreArmorTest()
        {
            PickUp armor = new MoreArmorItem(Vector4.Zero);
            float initialHealth = archer.Health;
            armor.ChangeStates(archer);

            Assert.AreEqual(initialHealth + armor.Up, archer.Health);
        }

        [TestMethod]
        public void MoreArrowTest()
        {
            PickUp arrow = new MoreArrowsItem(Vector4.Zero);
            float initialArrow = archer.ArrowAmount;
            arrow.ChangeStates(archer);

            Assert.AreEqual(initialArrow + arrow.Up, archer.ArrowAmount);
        }

        [TestMethod]
        public void MovementAbilityTest()
        {
            PickUp movement = new MovementAbilityItem(Vector4.Zero);
            movement.ChangeStates(archer);

            Assert.AreEqual(movement.Up, archer.TurnCount);
        }
    }
}
