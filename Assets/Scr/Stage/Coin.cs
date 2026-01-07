using Scr.Utility;
using UnityEngine;

namespace Scr.Stage {
    public class Coin : MonoBehaviour{
        
        public void OnTriggerEnter(Collider other) {
            var player = other.gameObject.GetComponentFromWhole<Player.Player>();
            if (player is not null) {
                player.GetCoin();
                Destroy(gameObject);
            }
        }
    }
}