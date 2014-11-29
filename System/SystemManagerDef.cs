using System;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// メンバーマネージャ
	/// </summary>
	public partial class SystemManager
	{
		/************************************************************************/
		/* 内部定義                                                             */
		/************************************************************************/

		/// <summary>
		/// システム情報
		/// </summary>
		[Serializable]
		public struct SSystemInfo
		{
			/// <summary>
			/// オートセーブフラグ
			/// </summary>
			public bool mAutoSaveFlg;

			/// <summary>
			/// Tweeterアクセス用のアクセストークン
			/// </summary>
			public STwitterAccessToken mAccessToken;

			/// <summary>
			/// デフォルトのツイート情報
			/// </summary>
			public string mDefaultTweetStyle;
		};

		[Serializable]
		public struct STwitterAccessToken
		{
			/// <summary>
			/// トークン
			/// </summary>
			public string mToken;

			/// <summary>
			/// トークンシークレット
			/// </summary>
			public string mTokenSecret;

			/// <summary>
			/// スクリーン名
			/// </summary>
			public string mScreenName;

			/// <summary>
			/// ユーザーID
			/// </summary>
			public int mUserId;
		}
	}
}
