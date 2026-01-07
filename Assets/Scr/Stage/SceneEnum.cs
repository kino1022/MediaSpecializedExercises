using System;

namespace Scr.Stage {
    [Serializable]
    public enum SceneEnum {
        
        TitleScene,
        Stage1_1,
        DeadScene,
        GameOverScene
    }

    public static class SceneEnumExtension {
        
        /// <summary>
        /// シーンの列挙型を元に対応するシーンの名前を取得する
        /// </summary>
        /// <param name="sceneEnum"></param>
        /// <returns></returns>
        public static string ToSceneName(this SceneEnum sceneEnum) {
            return sceneEnum switch {
                SceneEnum.TitleScene => "TitleScene",
                SceneEnum.Stage1_1 => "Stage1-1-1",
                SceneEnum.DeadScene => "DeadScene",
                SceneEnum.GameOverScene => "GameOverScene",
                _ => "UnknownScene"
            };
        }
        
        /// <summary>
        /// 列挙型に対応したシーンに遷移する
        /// </summary>
        /// <param name="sceneEnum"></param>
        public static void LoadScene(this SceneEnum sceneEnum) {
            UnityEngine
                .SceneManagement
                .SceneManager
                .LoadScene(sceneEnum.ToSceneName());
        }
    }
}