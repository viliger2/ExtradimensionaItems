using UnityEngine;

namespace ExtradimensionalItems.Modules.Effects
{

    public class GroundFires : MonoBehaviour
    {
        const float timeBetweenSpawns = 0.15f;
        private float timer;

        public GameObject fire;

        void Update()
        {
            if (timer <= 0)
            {
                Instantiate(fire, transform.position, Quaternion.identity);
                timer = timeBetweenSpawns + UnityEngine.Random.Range(0f, 0.1f);
            }
            else
            {
                timer -= Time.deltaTime;
            }
        }
    }

}
