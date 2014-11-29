
namespace FightingCommunityAdministrator
{
	/// <summary>
	/// シングルトン
	/// </summary>
    public partial class Singleton<ClassType> where ClassType : class, new()
	{
		/************************************************************************/
		/* 静的アクセサ                                                         */
		/************************************************************************/

		/// <summary>
		/// インスタンスの取得
		/// </summary>
		/// <returns></returns>
        public static ClassType GetInstance()
		{
			if (sInstance == null)
			{
                sInstance = new ClassType();
			}

			return sInstance;
		}

		/************************************************************************/
		/* 静的変数定義                                                         */
		/************************************************************************/

		/// <summary>
		/// システムマネージャの本体
		/// </summary>
        static ClassType sInstance;
	}
}
