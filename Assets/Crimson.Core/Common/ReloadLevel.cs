using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Crimson.Core.Common
{
	public class ReloadLevel : MonoBehaviour
	{
		public void Execute()
		{
			var scene = SceneManager.GetActiveScene();
			SceneManager.LoadScene(scene.buildIndex);
		}
	}
}