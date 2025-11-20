namespace Scr.Utility {
    public static partial class GameObjectExtension  {
        
        /// <summary>
        /// 指定した型のコンポーネントを子オブジェクトも含めて取得する
        /// </summary>
        /// <typeparam name="T">コンポーネントの型</typeparam>
        /// <param name="obj">対象のゲームオブジェクト</param>
        /// <returns>指定した型のコンポーネント。存在しない場合はnull。</returns>
        public static T GetComponentFromWhole<T>(this UnityEngine.GameObject obj)  {
            return obj.transform.root.GetComponentInChildren<T>();
        }
        
    }
}