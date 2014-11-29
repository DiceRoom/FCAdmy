using System;
using System.Collections.Generic;
using System.Linq;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// シンプルトーナメント用ゲーム結果
	/// </summary>
	[Serializable]
	public class SimpleTournamentObject
	{
		/************************************************************************/
		/* 公開処理                                                             */
		/************************************************************************/

		/// <summary>
		/// 対戦結果情報
		/// </summary>
		[Serializable]
		public struct SResult
		{
			/// <summary>
			/// リーフ番号リスト
			/// </summary>
			public int[] mReafLabelIndexList;

			/// <summary>
			/// 上のチームが勝っていれば0、下の場合は1とする
			/// </summary>
			public int mWinTeamIndex;

			/// <summary>
			/// トーナメントに表示されない部分の対戦か(3位決定戦等)
			/// </summary>
			public bool mIgnoreTournament;
		};

		/// <summary>
		/// 個人用の結果情報
		/// </summary>
		public struct SMemberResult
		{
			/// <summary>
			/// 順位
			/// </summary>
			public int mRank;

			/// <summary>
			/// 何回戦からのスタートか(0か1しかないけど)
			/// </summary>
			public int mBeginVersus;

			/// <summary>
			/// 何回戦まで行ったか
			/// </summary>
			public int mVersusVal;
		};

		/// <summary>
		/// シンプルトーナメントの大会結果
		/// </summary>
		public struct SSimpleTournamentResult
		{
			/// <summary>
			/// 優勝ＩＤ（シングルの場合はメンバーＩＤ、チームの場合はチームＩＤ）
			/// </summary>
			public int mWinnerID;

			/// <summary>
			/// チーム戦か
			/// </summary>
			public bool mTeamBattleFlg;

			/// <summary>
			/// 順位マップ（キーにメンバーIDが入ってる)
			/// </summary>
			public Dictionary<int, int> mRankMap;
		};

		/************************************************************************/
		/* 基本処理                                                             */
		/************************************************************************/

		/// <summary>
		/// リーフ情報番号の追加
		/// </summary>
		/// <param name="iIndex">番号</param>
		public void AddLeafInfoIndex(int iIndex)
		{
			mLeafInfoIndexList.Add(iIndex);
		}

		/// <summary>
		/// 結果の追加
		/// </summary>
		/// <param name="iResult">追加</param>
		public void AddResult(SResult iResult)
		{
			mResultList.Add(iResult);
		}

		/************************************************************************/
		/* 個人用の結果取得用                                                   */
		/************************************************************************/

		/// <summary>
		/// 指定した大会の結果情報を取得
		/// </summary>
		/// <param name="iBattleInfo">大会情報</param>
		/// <returns></returns>
		public SSimpleTournamentResult GetSimpleTournamentResult(BattleManager.SBattleInfo iBattleInfo)
		{
			var info = new SSimpleTournamentResult();
			info.mTeamBattleFlg = (iBattleInfo.mTeamList.Count != 0);
			info.mRankMap = new Dictionary<int, int>();
			foreach (var iJoinInfo in iBattleInfo.mJoinList)
			{
				var rank = GetSimpleTournamentResult(iBattleInfo, iJoinInfo.mMemberID).mRank;
				if (rank == 1)
				{
					info.mWinnerID = (info.mTeamBattleFlg) ? _GetTeamID(iBattleInfo, iJoinInfo.mJoinID) : iJoinInfo.mMemberID;
				}
				info.mRankMap.Add(iJoinInfo.mMemberID, rank);
			}
			return info;
		}

		/// <summary>
		/// 指定した大会で指定したメンバーの情報を取得
		/// </summary>
		/// <param name="iBattleInfo">大会情報</param>
		/// <param name="iMemberID">メンバーID</param>
		/// <returns>情報</returns>
		public SMemberResult GetSimpleTournamentResult(BattleManager.SBattleInfo iBattleInfo, int iMemberID)
		{
			var info = new SMemberResult();

			//リーフ番号取得
			int leaf_index = _GetLeafIndex(iBattleInfo, iMemberID);
			if (leaf_index == -1)
			{
				return info;
			}

			//何回戦かの初期化
			int depth = 0;
			var join_val = ((iBattleInfo.mTeamList.Count == 0) ? iBattleInfo.mJoinList.Count : iBattleInfo.mTeamList.Count);
			while (true)
			{
				int val = (int)Math.Pow(2, (int)depth);
				if (val >= join_val)
				{
					break;
				}
				++depth;
			}
			
			var check_val = ((int)Math.Pow(2, (int)depth) - join_val);
			bool just_tournament_flg = (check_val == 0);

			//偶数の場合
			if ((check_val % 2) == 0)
			{
				check_val /= 2;
				info.mVersusVal = (check_val == 0 || leaf_index < check_val || leaf_index >= (join_val - check_val)) ? 1 : 0;
			}
			//奇数の場合
			else
			{
				check_val /= 2;
				info.mVersusVal = (leaf_index < check_val || leaf_index >= (join_val - (check_val + 1))) ? 1 : 0;
			}
			info.mBeginVersus = info.mVersusVal;

			//勝利数から結果を算出
			var obj = (SimpleTournamentObject)iBattleInfo.mBattleObject;
			var win_val = _GetSimpleTournamentWinVal(leaf_index);
			if (win_val == 0)
			{
				//初戦敗退
				info.mRank = join_val;
			}
			else
			{
				//回戦数を取得
				var versus_val = info.mVersusVal;
				info.mVersusVal += win_val;

				//順位の計算
				if (just_tournament_flg)
				{
					//割り切れるトーナメントの場合は単純に
					int pow = (int)depth - info.mVersusVal + 1;
					info.mRank = (int)Math.Pow(2, pow);
				}
				else
				{
					//自分が0回戦スタートで1勝の場合順位の計算方法が変わる
					if (versus_val == 0 && win_val == 1)
					{
						//初戦敗退者の数を計算する
						int first_versus_lost_val = 0;
						foreach (var iIndex in System.Linq.Enumerable.Range(0, join_val))
						{
							foreach (var iResult in obj.GetResultList())
							{
								//無視
								if (iResult.mIgnoreTournament)
								{
									continue;
								}

								//上方向にある
								var check_index = -1;
								if (iResult.mReafLabelIndexList[0] == iIndex)
								{
									check_index = 1;
								}
								//下方向にある
								else if (iResult.mReafLabelIndexList[1] == iIndex)
								{
									check_index = 0;
								}

								//存在する
								if (check_index != -1)
								{
									if (iResult.mWinTeamIndex == check_index)
									{
										++first_versus_lost_val;
									}
									break;

								}
							}
						}

						info.mRank = join_val - first_versus_lost_val;
					}
					//優勝
					else if (info.mVersusVal == depth)
					{
						info.mRank = 1;
					}
					else
					{
						int pow = (int)depth - info.mVersusVal;
						info.mRank = (int)Math.Pow(2, pow);
					}
				}

				
			}

			return info;
		}

		/************************************************************************/
		/* アクセサ                                                             */
		/************************************************************************/

		/// <summary>
		/// リーフに配属されているメンバー（チーム）番号リストの取得
		/// </summary>
		/// <returns></returns>
		public List<int> GetLeafInfoIndexList() { return mLeafInfoIndexList; }

		/// <summary>
		/// 結果リストの取得
		/// </summary>
		/// <returns>結果リスト</returns>
		public List<SResult> GetResultList() { return mResultList; }

		/************************************************************************/
		/* 内部処理                                                             */
		/************************************************************************/

		//============================================================================
		//! 指定したメンバーIDからリーフ番号を取得
		private int _GetLeafIndex(BattleManager.SBattleInfo iBattleInfo, int iMemberID)
		{
			//参加番号を取得
			int join_ID = _GetJoinID(iBattleInfo, iMemberID);
			if (join_ID == -1)
			{
				return -1;
			}

			//チーム戦の場合はチーム番号
			int check_ID = join_ID;
			if (iBattleInfo.mTeamList.Count != 0)
			{
				check_ID = _GetTeamID(iBattleInfo, join_ID);
				if (check_ID == -1)
				{
					return -1;
				}
			}

			//リーフ番号を取得する
			var obj = (SimpleTournamentObject)iBattleInfo.mBattleObject;
			var leaf_list = obj.GetLeafInfoIndexList();
			foreach (var iIndex in System.Linq.Enumerable.Range(0, leaf_list.Count))
			{
				if (leaf_list[iIndex] == check_ID)
				{
					return iIndex;
				}
			}

			//ありえないけど一応-1返す
			return -1;
		}

		//============================================================================
		//! 指定したメンバーIDの参加IDを取得する
		private int _GetJoinID(BattleManager.SBattleInfo iBattleInfo, int iMemberID)
		{
			foreach (var iJoinInfo in iBattleInfo.mJoinList)
			{
				if (iJoinInfo.mMemberID == iMemberID)
				{
					return iJoinInfo.mJoinID;
				}
			}

			//未参加
			return -1;
		}

		//============================================================================
		//! 指定した参加IDからチームIDを取得する
		private int _GetTeamID(BattleManager.SBattleInfo iBattleInfo, int iCheckJoinID)
		{
			foreach (var iTeamInfo in iBattleInfo.mTeamList)
			{
				foreach (var iJoinID in iTeamInfo.mJoinIDList)
				{
					if (iCheckJoinID == iJoinID)
					{
						return iTeamInfo.mTeamID;
					}
				}
			}

			//無し
			return -1;
		}

		//============================================================================
		//! 指定したリーフの勝利回数を取得
		private int _GetSimpleTournamentWinVal(int iLeafIndex)
		{
			int win_val = 0;
			foreach (var iResult in GetResultList())
			{
				//無視
				if (iResult.mIgnoreTournament)
				{
					continue;
				}

				//上方向にある
				bool? win_flg = null;
				if (iResult.mReafLabelIndexList[0] == iLeafIndex)
				{
					win_flg = (iResult.mWinTeamIndex == 0);
				}
				//下方向にある
				else if (iResult.mReafLabelIndexList[1] == iLeafIndex)
				{
					win_flg = (iResult.mWinTeamIndex == 1);
				}

				//勝利の場合はカウンティング、敗北の場合は終了
				if (win_flg != null)
				{
					if (win_flg == true)
					{
						++win_val;
					}
					else
					{
						break;
					}
				}
			}
			return win_val;
		}

		/************************************************************************/
		/* 変数定義                                                             */
		/************************************************************************/

		/// <summary>
		/// リーフにどのメンバー(チーム)が配属されていたかのリスト
		/// </summary>
		private List<int> mLeafInfoIndexList = new List<int>();

		/// <summary>
		/// 結果リスト
		/// </summary>
		private List<SResult> mResultList = new List<SResult>();
	};
}
