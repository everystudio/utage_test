// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEditor;
using UnityEngine;
using NUnit.Framework;


namespace Utage
{
	class UtageEditorTest
	{
		[Test]
		public void EditorTest() {
			//Arrange
			var gameObject = new GameObject();

			//Act
			//Try to rename the GameObject
			var newGameObjectName = "My game object";
			gameObject.name = newGameObjectName;

			//Assert
			//The object has a new name
			Assert.AreEqual(newGameObjectName, gameObject.name);
		}
	}
}