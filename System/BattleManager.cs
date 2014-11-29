using System;
using System.Collections.Generic;
using System.Linq;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// シングルトン型の大会マネージャ
	/// </summary>
	public partial class BattleManager : Singleton<BattleManager>
	{
		/************************************************************************/
		/* 公開処理                                                             */
		/************************************************************************/

		/// <summary>
		/// 大会データの読み込み
		/// </summary>
		public void LoadBattleData()
		{
			var get_object = DataManager.GetInstance().GetData(SaveManager.GetInstance().BattleInfoKey);
			if (get_object != null)
			{
				mBattleList = new List<SBattleInfo>((SBattleInfo[])get_object);
			}
		}

		/************************************************************************/
		/* アクセサ                                                             */
		/************************************************************************/

		/// <summary>
		/// 大会履歴を追加
		/// </summary>
		/// <param name="iBattleInfo">大会情報</param>
		public void AddBattle(SBattleInfo iBattleInfo)
		{
			mBattleList.Add(iBattleInfo);
		}

		/// <summary>
		/// 大会履歴リストを取得
		/// </summary>
		/// <returns>大会履歴リスト</returns>
		public List<SBattleInfo> GetBattleList()
		{
			return mBattleList;
		}

		/// <summary>
		/// 指定番号の大会履歴を取得
		/// </summary>
		/// <param name="iIndex"></param>
		/// <returns></returns>
		public SBattleInfo GetBattle(int iIndex)
		{
			return mBattleList[iIndex];
		}

		/// <summary>
		/// 今までに開催した大会の回数を取得
		/// </summary>
		/// <returns>回数</returns>
		public int GetBattleVal()
		{
			return mBattleList.Count;
		}

		/// <summary>
		/// 指定したメンバーの大会結果リストを取得する
		/// </summary>
		/// <param name="iMemberID">メンバーID</param>
		/// <returns>大会結果リスト</returns>
		public List<SMemberBattleResult> GetMemberBattleResultList(int iMemberID, bool iIgnoreDetails)
		{
			var ret_list = new List<SMemberBattleResult>();
			var index = 0;
			foreach (var iBattle in mBattleList)
			{
				//まずはこの大会に参加しているかチェック
				var join_index = -1;
				foreach (var iJoinIndex in System.Linq.Enumerable.Range(0, iBattle.mJoinList.Count))
				{
					if (iBattle.mJoinList[iJoinIndex].mMemberID == iMemberID)
					{
						join_index = iJoinIndex;
						break;
					}
				}

				//参加している場合はデータ追加
				if (join_index != -1)
				{
					var info = _GetBattleResult(iBattle, iMemberID);
					info.mIndex = index;
					info.mUseCharacterID = iBattle.mJoinList[join_index].mUserCharacterID;
					ret_list.Add(info);
				}

				++index;
			}
			return ret_list;
		}

		/************************************************************************/
		/* 内部処理                                                             */
		/************************************************************************/

		//============================================================================
		//! 指定した大会で指定したメンバーの情報を取得
		private SMemberBattleResult _GetBattleResult(SBattleInfo iBattleInfo, int iMemberID)
		{
			SMemberBattleResult info = new SMemberBattleResult();

			//大会の形式によって違う
			switch (iBattleInfo.mBattleKind)
			{
			case EBattleKind.BATTLE_SIMPLE_TOURNAMENT:
				info.mResult = ((SimpleTournamentObject)iBattleInfo.mBattleObject).GetSimpleTournamentResult(iBattleInfo, iMemberID);
				break;
			}

			return info;
		}

		/************************************************************************/
		/* 変数宣言				                                                */
		/************************************************************************/

		/// <summary>
		/// 大会履歴
		/// </summary>
		private List<SBattleInfo> mBattleList = new List<SBattleInfo>();
	}
}
