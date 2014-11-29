using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Net;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// 現在開催している大会の管理オブジェクト
	/// </summary>
	public partial class BattleOperatorManager : Singleton<BattleOperatorManager>
	{
		/************************************************************************/
		/* 基本処理			                                                    */
		/************************************************************************/

		/// <summary>
		/// 大会を実行する
		/// </summary>
		public void BeginBattle(BattleManager.EBattleKind iBattleKind)
		{
            if (mBeginBattleFlg)
            {
                SystemUtility.DisplayErrorDialog("大会管理の二重起動が発生しました");
                return;
            }

            mCurrentBattleInfo = new BattleManager.SBattleInfo();
            mCurrentBattleInfo.mBattleKind = iBattleKind;
            mCurrentBattleInfo.mDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            mCurrentBattleInfo.mJoinList = new List<BattleManager.SBattleJoinInfo>();
            mCurrentBattleInfo.mTeamList = new List<BattleManager.SBattleTeamInfo>();
            mLockJoinMemberFlg = false;
			mBeginBattleFlg = true;
			mLockTeamFlg = false;
		}

		/// <summary>
		/// 大会の結果を書き出しする
		/// </summary>
		public void CommitBattle()
		{
			if (!_CheckError()) { return; }
			BattleManager.GetInstance().AddBattle(mCurrentBattleInfo);

			//終了
			CancelBattle();
		}

        /// <summary>
        /// 大会の途中キャンセル
        /// </summary>
        public void CancelBattle()
        {
            mBeginBattleFlg = false;
        }

		/// <summary>
		/// メンバーのスクリーンショットを生成
		/// </summary>
		/// <param name="iFilePath">ファイルパス</param>
		public void CreateMemberScreenShot(string iFilePath)
		{
            BeginScreenShot();
            SystemUtility.OutputCaptureControl(iFilePath, SystemManager.GetInstance().GetScreenShotGrid());
            SystemManager.GetInstance().GetScreenShotGrid().Children.Clear();
		}

        /// <summary>
        /// メンバーのスクリーンショットを生成
        /// </summary>
        /// <param name="iFilePath">ファイルパス</param>
        /// <param name="iControl">追加コントロール</param>
        public void CreateMemberScreenShot(string iFilePath, UserControl iControl)
        {
            BeginScreenShot();
            
            //追加コントロールを結合する
            var grid = SystemManager.GetInstance().GetScreenShotGrid();
            var stock_Margin = iControl.Margin;
            var current_Size = new Size(grid.Width, grid.Height);
            if (grid.Width < grid.Height)
            {
                //サイズ調整
                grid.Width += iControl.Width;
                if (grid.Height < iControl.Height)
                {
                    grid.Height = iControl.Height;
                }

                //結合
                var margin = new Thickness(current_Size.Width, 0, 0, 0);
                iControl.Margin = margin;
            }
            else
            {
                //サイズ調整
                grid.Height += iControl.Height;
                if (grid.Width < iControl.Width)
                {
                    grid.Width = iControl.Width;
                }

                //結合
                var margin = new Thickness(0, current_Size.Height, 0, 0);
                iControl.Margin = margin;
            }
            
            grid.Children.Add(iControl);
            SystemUtility.OutputCaptureControl(iFilePath, grid);
            grid.Children.Remove(iControl);
            grid.Children.Clear();
            iControl.Margin = stock_Margin;
        }

        /// <summary>
        /// スクリーンショットを撮る準備(チーム画像をスクリーンショットグリッドに入れておく)
        /// </summary>
        public void BeginScreenShot()
        {
            //必要なコントロールを作成
            var control_List = new List<TeamDisplay>();
            var manager = BattleOperatorManager.GetInstance();
            var team_Val = manager.GetTeamVal();
            foreach (var iTeamIndex in System.Linq.Enumerable.Range(0, team_Val))
            {
                var team_Info = manager.GetTeamInfo(iTeamIndex);
                var list = new List<BattleManager.SBattleJoinInfo>();
                foreach (var iJoinMemberID in team_Info.mJoinIDList)
                {
                    var info = new BattleManager.SBattleJoinInfo();
                    manager.GetJoinInfo(ref info, iJoinMemberID);
                    list.Add(info);
                }

                var control = new TeamDisplay();
                control.TeamNameTextBlock.Text = team_Info.mTeamName;
                control.SetJoinMemberList(list);
                control_List.Add(control);
            }

            //正方形にできるだけなるように敷き詰める数を取得
            var val = 1;
            while (true)
            {
                var check = Math.Pow(2, (double)val);
                if (team_Val < check)
                {
                    break;
                }
                ++val;
            }

            //グリッドに挿入
            var margin = new Thickness();
            var grid = SystemManager.GetInstance().GetScreenShotGrid();
            var sum_Height = 0.0;
            while (control_List.Count > 0)
            {
                //横方向への挿入
                margin.Left = 0;
                var max_Height = 0.0;
                var sum_Width = 0.0;
                foreach (var iIndex in System.Linq.Enumerable.Range(0, val))
                {
                    var control = control_List[0];
                    control.Margin = margin;
                    grid.Children.Add(control);

                    //横へずらす
                    margin.Left += control.Width;
                    sum_Width += control.Width;

                    //縦の最大サイズ取得
                    if (max_Height < control.Height)
                    {
                        max_Height = control.Height;
                    }

                    //先頭を削除して無くなったら終わり
                    control_List.RemoveAt(0);
                    if (control_List.Count == 0)
                    {
                        break;
                    }
                }

                //横方向へサイズ変更チェック
                if (grid.Width < sum_Width)
                {
                    grid.Width = sum_Width;
                }

                //縦移動
                margin.Top += max_Height;
                sum_Height += max_Height;
            }

            //縦方向へサイズ変更チェック
            if (grid.Height < sum_Height)
            {
                grid.Height = sum_Height;
            }
        }

		/// <summary>
		/// デフォルトで表示されるツイート情報を取得
		/// </summary>
		/// <param name="iImageFilePath">イメージファイルパス</param>
		/// <returns>ツイート</returns>
		public string GetDefaultTweet(string iImageFilePath)
		{
			//デフォルト設定から全て置換掛けていくよ
			string tweet = SystemManager.GetInstance().GetSystemInfo().mDefaultTweetStyle;

			//大会名
			tweet = tweet.Replace("{TournamentName}", mCurrentBattleInfo.mName);

			//開始日時(全部)
			tweet = tweet.Replace("{TournamentData}", mCurrentBattleInfo.mDate);

			//開始日時(日にちだけ)
			var sep_list = mCurrentBattleInfo.mDate.Split(' ');
			tweet = tweet.Replace("{TournamentDay}", sep_list[0]);

			//開始日時(時間だけ)
			tweet = tweet.Replace("{TournamentTime}", sep_list[1]);

			//画像
			tweet = tweet.Replace("{ImageUrl}", iImageFilePath);

			return tweet;
		}

		/************************************************************************/
        /* アクセサ                                                             */
        /************************************************************************/

        /// <summary>
        /// 大会名の設定
        /// </summary>
        /// <param name="iBattleName">大会名</param>
        /// <returns>設定出来なかった</returns>
        public bool SetBattleName(string iBattleName)
        {
            if (!_CheckError()) { return false; }
            mCurrentBattleInfo.mName = iBattleName;
            return true;
        }

        /// <summary>
        /// 大会名の取得
        /// </summary>
        /// <returns>大会名</returns>
        public string GetBattleName()
        {
            if (!_CheckError()) { return ""; }
            return mCurrentBattleInfo.mName;
        }

        /// <summary>
        /// 個別大会オブジェクトの設定
        /// </summary>
        /// <param name="iObject">大会オブジェクト</param>
        public void SetBattleObject(object iObject)
        {
            if (!_CheckError()) { return ; }
            mCurrentBattleInfo.mBattleObject = iObject;
        }

        /// <summary>
        /// 個別大会オブジェクトの取得
        /// </summary>
        /// <returns>大会オブジェクト</returns>
        public object GetBattleObject()
        {
            if (!_CheckError()) { return null; }
            return mCurrentBattleInfo.mBattleObject;
        }

        /// <summary>
        /// チーム戦かどうかの取得
        /// </summary>
        /// <returns>チーム戦フラグ</returns>
        public bool IsTeamBattleFlg()
        {
            return (mCurrentBattleInfo.mTeamList.Count != 0);
        }

        /************************************************************************/
        /* 参加メンバー系アクセサ                                               */
        /************************************************************************/

        /// <summary>
        /// 参加するメンバーを追加(ほぼデバッグ用になる？)
        /// </summary>
        /// <param name="iMemberID">メンバーID</param>
        public void AddMember(int iMemberID)
        {
            var info = MemberManager.GetInstance().GetMemberInfo(iMemberID);
            AddMember(iMemberID, info.mDefaultCharacterID);
        }

        /// <summary>
        /// 参加するメンバーを追加
        /// </summary>
        /// <param name="iMemberID">メンバーID</param>
        /// <param name="iCharacterID">使用キャラクター番号</param>
        public void AddMember(int iMemberID, int iCharacterID)
        {
            if (_CheckLockJoinMember()) { return; }

            //既に追加済みの場合はエラーとする
            foreach (var iInfo in mCurrentBattleInfo.mJoinList)
            {
                if (iInfo.mMemberID == iMemberID)
                {
                    throw new System.Exception();
                }
            }

            //追加
            var member_Info = new BattleManager.SBattleJoinInfo();
            member_Info.mMemberID = iMemberID;
            member_Info.mUserCharacterID = iCharacterID;
            member_Info.mJoinID = mCurrentBattleInfo.mJoinList.Count;
            mCurrentBattleInfo.mJoinList.Add(member_Info);
        }

        /// <summary>
        /// 参加メンバー人数を取得
        /// </summary>
        /// <returns>メンバー人数</returns>
        public int GetJoinMemberVal()
        {
            if (!_CheckError()) { return 0; }

            return mCurrentBattleInfo.mJoinList.Count;
        }

        /// <summary>
        /// 参加者IDから参加情報を取得
        /// </summary>
        /// <param name="oJoinInfo">参加情報出力先</param>
        /// <param name="iJoinID">参加者ID</param>
        /// <returns>取得フラグ</returns>
        public bool GetJoinInfo(ref BattleManager.SBattleJoinInfo oJoinInfo, int iJoinID)
        {
            if (!_CheckError()) { return false; }

            oJoinInfo = mCurrentBattleInfo.mJoinList[iJoinID];
            return true;
        }

        /// <summary>
        /// メンバーIDから参加情報を取得
        /// </summary>
        /// <param name="oJoinInfo">参加情報出力先</param>
        /// <param name="iMemberID">メンバーID</param>
        /// <returns>取得フラグ</returns>
        public bool GetJoinInfoFromMemberID(BattleManager.SBattleJoinInfo oJoinInfo, int iMemberID)
        {
            if (!_CheckError()) { return false; }

            foreach (var iInfo in mCurrentBattleInfo.mJoinList)
            {
                if (iInfo.mMemberID == iMemberID)
                {
                    oJoinInfo = iInfo;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 参加するメンバーリストを取得
        /// </summary>
        /// <returns>メンバーリスト</returns>
        public List<BattleManager.SBattleJoinInfo> GetJoinMemberList()
        {
            if (!_CheckError()) { return null; }
            return new List<BattleManager.SBattleJoinInfo>(mCurrentBattleInfo.mJoinList.ToArray());
        }

        /// <summary>
        /// 参加メンバーの全削除
        /// </summary>
        public void ClearJoinMember()
        {
            if (!_CheckError()) { return; }
            if (_CheckLockJoinMember()) { return; }
            
            mCurrentBattleInfo.mJoinList.Clear();
        }

        /// <summary>
        /// コミット状態の設定
        /// </summary>
        /// <param name="iCommitFlg">コミットフラグ</param>
        public void SetLockJoinMember(bool iCommitFlg)
        {
            //チーム状態がコミットであればエラー
            if (mLockTeamFlg)
            {
                SystemUtility.DisplayErrorDialog("チームがコミット状態でメンバーのコミット状態を変更出来ません");
                return;
            }

            //一致の時無視
            if (mLockJoinMemberFlg == iCommitFlg)
            {
                return;
            }

            mLockJoinMemberFlg = iCommitFlg;
        }

        /************************************************************************/
		/* チーム系アクセサ	                                                    */
		/************************************************************************/

		/// <summary>
		/// チームの追加
		/// </summary>
		/// <param name="iTeamName">チーム名</param>
		public void AddTeam(BattleManager.SBattleTeamInfo iTeamInfo)
		{
			if (_CheckLockTeam()) { return; }
            
			//同名のチームが追加された場合はエラーとする
			foreach (var iInfo in mCurrentBattleInfo.mTeamList)
			{
				if (iInfo.mTeamName == iTeamInfo.mTeamName)
				{
                    SystemUtility.DisplayErrorDialog("大会に同盟のチームを設定できません");
					throw new System.Exception();
				}
			}

			//追加
            iTeamInfo.mTeamID = mCurrentBattleInfo.mTeamList.Count;
            mCurrentBattleInfo.mTeamList.Add(iTeamInfo);
		}

        /// <summary>
        /// チームの全削除
        /// </summary>
        public void ClearTeam()
        {
            if (_CheckLockTeam()) { return; }

            mCurrentBattleInfo.mTeamList.Clear();
        }

		/// <summary>
		/// チーム数の取得
		/// </summary>
		/// <returns>チーム数</returns>
		public int GetTeamVal()
		{
			return mCurrentBattleInfo.mTeamList.Count;
		}

        /// <summary>
        /// 指定チームの情報を取得
        /// </summary>
        /// <param name="iTeamIndex">チーム番号</param>
        /// <returns>チーム情報</returns>
        public BattleManager.SBattleTeamInfo GetTeamInfo(int iTeamIndex)
        {
            return mCurrentBattleInfo.mTeamList[iTeamIndex];
        }

		/// <summary>
		/// 現在入っているチームを確定してチーム番号等を割り振る
		/// </summary>
		public bool SetLockTeam(bool iCommitFlg)
		{
			//一致の時無視
			if (mLockTeamFlg == iCommitFlg)
			{
				return mLockTeamFlg;
			}

			//メンバー状態がコミットでなければエラー
			if (!mLockJoinMemberFlg)
			{
				SystemUtility.DisplayErrorDialog("参加メンバーが未コミット状態でチームのコミット状態を変更出来ません");
				mLockTeamFlg = false;
				return false;
			}
            
			mLockJoinMemberFlg = true;
			mLockTeamFlg = iCommitFlg;
			return mLockTeamFlg;
		}

        /************************************************************************/
        /* 内部処理                                                             */
        /************************************************************************/

        //============================================================================
        //! エラーチェック
        private bool _CheckError()
        {
            if (!mBeginBattleFlg)
            {
                SystemUtility.DisplayErrorDialog("大会管理が起動していない状態で\nプロパティの設定及び取得を行えません");
                return false;
            }

            return true;
        }

        //============================================================================
        //! メンバーのコミット状態チェック
        private bool _CheckLockJoinMember()
        {
            //コミット状態の場合は、この関数を呼ぶとエラーとして認識
            if (mLockJoinMemberFlg)
            {
                SystemUtility.DisplayErrorDialog("参加メンバー情報は既にはロックされています\nメンバー情報の変更は行えません");
            }

            return mLockJoinMemberFlg;
        }

        //============================================================================
        //! チームのコミット状態チェック
        private bool _CheckLockTeam()
        {
            //メンバーがコミット状態でない場合はエラー
            if (!mLockJoinMemberFlg)
            {
                SystemUtility.DisplayErrorDialog("参加メンバー情報は既にはロックされていなければ\nチームの情報の変更は出来ません");
                return true;
            }

            //コミット状態の場合は、この関数を呼ぶとエラーとして認識
            if (mLockTeamFlg)
            {
                SystemUtility.DisplayErrorDialog("メンバーは既にコミットされています");
            }
            return mLockTeamFlg;
        }

        /************************************************************************/
		/* 変数宣言				                                                */
		/************************************************************************/

        /// <summary>
        /// 大会が開催中か
        /// </summary>
        private bool mBeginBattleFlg = false;

		/// <summary>
		/// 現在開催中の大会情報
		/// </summary>
        private BattleManager.SBattleInfo mCurrentBattleInfo;

		/// <summary>
		/// メンバーコミット状態
		/// </summary>
		private bool mLockJoinMemberFlg;

		/// <summary>
		/// チームコミット状態
		/// </summary>
		private bool mLockTeamFlg;
	}
}
