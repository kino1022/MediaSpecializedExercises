using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scr.Player {

    public interface IPlayableManager : IPlayableProvider {
        
        bool SetPlayable(bool playable);
    }

    public interface IPlayableProvider {
        
        bool Playable { get; }
        
    }
    
    [Serializable]
    public class PlayableManager : IPlayableManager {
        
        [SerializeField]
        [LabelText("操作可能フラグ")]
        private bool _playable = true;
        
        public bool Playable => _playable;
        
        public bool SetPlayable(bool playable) => _playable = playable;
        
    }
}