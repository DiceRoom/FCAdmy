using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// シングルトン型の大会マネージャ
	/// </summary>
	public partial class BattleManager
	{
		/************************************************************************/
		/* 共通公開定義                                                         */
		/************************************************************************/

		/// <summary>
		/// 大会種別
		/// </summary>
		[Serializable]
		public enum EBattleKind
		{
			BATTLE_NONE,				//< 大会未開催
			BATTLE_SIMPLE_TOURNAMENT,	//< シンプルトーナメント
			BATTLE_ROUND_ROBIN,			//< リーグ戦
			BATTLE_INTRASQUAD,			//< 紅白戦
		};

		/// <summary>
		/// 大会に出場したメンバーの結果
		/// </summary>
		public struct SMemberBattleResult
		{
			/// <summary>
			/// 大会番号
			/// </summary>
			public int mIndex;

			/// <summary>
			/// 使用キャラクターID
			/// </summary>
			public int mUseCharacterID;

			/// <summary>
			/// 大会種別による結果
			/// </summary>
			public object mResult;
		};

		/// <summary>
		/// 参加メンバー情報
		/// </summary>
		[Serializable]
		public struct SBattleJoinInfo
		{
            /// <summary>
            /// 参加ID(リスト内のインデックスと一致)
            /// </summary>
            public int mJoinID;

			/// <summary>
			/// メンバーID
			/// </summary>
			public int mMemberID;

			/// <summary>
			/// 使用キャラクターID
			/// </summary>
			public int mUserCharacterID;
		};

		/// <summary>
		/// 参加チーム情報
		/// </summary>
		[Serializable]
		public struct SBattleTeamInfo
		{
			/// <summary>
			/// チームID
			/// </summary>
			public int mTeamID;

			/// <summary>
			/// チーム名
			/// </summary>
			public string mTeamName;

			/// <summary>
			/// 参加IDリスト
			/// </summary>
			public List<int> mJoinIDList;
		};

		/// <summary>
		/// 大会情報
		/// </summary>
		[Serializable]
		public struct SBattleInfo
		{
			/// <summary>
			/// 大会種別
			/// </summary>
			public EBattleKind mBattleKind;

			/// <summary>
			/// 大会名
			/// </summary>
			public string mName;

			/// <summary>
			/// 開催日時
			/// </summary>
			public string mDate;

			/// <summary>
			/// 参加情報リスト
			/// </summary>
			public List<SBattleJoinInfo> mJoinList;

			/// <summary>
			/// 参加チームリスト
			/// </summary>
			public List<SBattleTeamInfo> mTeamList;

			/// <summary>
			/// 各種大会書き出し用オブジェクト
			/// </summary>
            public object mBattleObject;
		};
	}
}
