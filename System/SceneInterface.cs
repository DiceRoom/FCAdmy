
namespace FightingCommunityAdministrator
{
	/// <summary>
	/// シーンインターフェース用
	/// </summary>
	public interface SceneInterface
    {
        /************************************************************************/
        /* 継承処理                                                             */
        /************************************************************************/

		/// <summary>
		/// 画面を覆うフィルターの見える横幅のサイズ
		/// </summary>
		/// <returns>フィルター幅</returns>
		double GetDisplayFilterWidth();

        /// <summary>
        /// 別のシーンから戻って来た時に呼び出される処理
        /// </summary>
        void SceneBack();
    }
}
