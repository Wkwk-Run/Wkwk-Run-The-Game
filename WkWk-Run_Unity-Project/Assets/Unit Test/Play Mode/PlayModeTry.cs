using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class PlayModeTry
    {
        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator SpawnTestWithEnumeratorPasses()
        {
            var gameObject = new GameObject();
            gameObject.AddComponent<AudioManager>();
            
            yield return new WaitForSeconds(3);

            Assert.IsNotNull(GameObject.FindObjectOfType<AudioManager>());
        }
    }
}
