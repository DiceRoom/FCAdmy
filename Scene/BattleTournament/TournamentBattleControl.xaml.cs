using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// TitleContorl.xaml の相互作用ロジック
	/// </summary>
	public partial class TournamentBattleControl : UserControl
	{
		/// <summary>
		/// コンストラクタ
		/// </summary>
		public TournamentBattleControl()
		{
			InitializeComponent();
			WinnerLeafLabel.Opacity = 0.3;
			WinnerLeafLabel.TopTextBlock.Text = "";
		}

		/// <summary>
		/// オプションウィンドウの表示
		/// </summary>
		public void OpenOptionWindow()
		{
			mOptionWindow = new TournamentBattleOptionWindow();
			mOptionWindow.Closing += (iSender, iArgs) =>
			{
				mOptionWindow = null;
			};
            mOptionWindow.CancelTournamentItem.Click += _ClickCancelTournament;
			mOptionWindow.SetCreateScreenShotCallback(_CreateScreenShot);
            mOptionWindow.Show();
            _RefreshOptionWindow();
		}

		/// <summary>
		/// オプションウィンドウが表示中か
		/// </summary>
		/// <returns></returns>
		public bool IsOpenOptionWindow()
		{
			return (mOptionWindow != null);
		}

		/// <summary>
		/// ツールウィンドウの一時的な表示切替
		/// </summary>
		/// <param name="iVisibleFlg">表示フラグ</param>
		public void SetVisibleOptionWindow(bool iVisibleFlg)
		{
			if (mOptionWindow != null)
			{
				mOptionWindow.Visibility = iVisibleFlg ? Visibility.Visible : Visibility.Hidden;
			}
		}

		/// <summary>
		/// オプションウィンドウの非表示
		/// </summary>
		public void CloseOptionWindow()
		{
			if (mOptionWindow != null)
			{
				mOptionWindow.Close();
				mOptionWindow = null;
			}
		}

		/// <summary>
		/// リフレッシュ
		/// </summary>
		public void Refresh()
		{
			//ラインの着色設定
			foreach (var iLeaf in mLeafInfoList)
			{
				//まだ対戦していない場合
				var root_Val = iLeaf.mPointRoot.Count;
				if (root_Val < 2)
				{
                    iLeaf.mPointRoot[0].SetLineColor(new SolidColorBrush(Colors.White), true);
                    iLeaf.mLabelControl.Opacity = 1;
					continue;
				}

				//勝ってる所に対してラインのZ位置及びカラーを設定
				var lose_Flg = (iLeaf.mPointRoot[root_Val - 1] == null);
				var set_Val = iLeaf.mPointRoot.Count - 1;
				if (lose_Flg) { --set_Val; }
				foreach (var iIndex in System.Linq.Enumerable.Range(0, set_Val))
				{
					iLeaf.mPointRoot[iIndex].SetLineColor(new SolidColorBrush(Colors.Red), true);
					iLeaf.mPointRoot[iIndex].SetZIndex(1000);
				}

				//負けてる場合の変更
				if (lose_Flg)
				{
					iLeaf.mPointRoot[set_Val].SetLineColor(new SolidColorBrush(Colors.White), false);
					iLeaf.mPointRoot[set_Val].SetZIndex(0);
					iLeaf.mLabelControl.Opacity = 0.3;
				}
				else
				{
					iLeaf.mLabelControl.Opacity = 1;
				}
			}
	
			//勝者ラベル設定
			if (mResultList.Count == mBattleVal)
			{
				WinnerLeafLabel.Opacity = 1;
				var last_Result = mResultList[mResultList.Count - 1];
				var leaf_Index = last_Result.mLeafIndexList[last_Result.mWinLeafIndex];
				WinnerLeafLabel.SetInfo(mLeafInfoList[leaf_Index].mLabelControl.GetSetInfo());
			}
			else
			{
				WinnerLeafLabel.RemoveInfo();
				WinnerLeafLabel.Opacity = 0.3;
				WinnerLeafLabel.TopTextBlock.Text = "";
			}
		}

		/************************************************************************/
		/* アクセサ                                                             */
		/************************************************************************/

		/// <summary>
		/// トップポイントチェッカーを設定
		/// </summary>
		/// <param name="iTopPonitChecker"></param>
		public void SetTopPointChecker(VersusPointChecker iTopPonitChecker)
		{
			mTopPointChecker = iTopPonitChecker;

			//作成
			if (false)
			{
				//両側
			}
			else
			{
				//片側
				_CreateSingleLeaf();
				_CreateSinglePointPosition();
			}

			//ポイントチェッカーのライン作成
			iTopPonitChecker.CreateLine(mLineSize, MainGrid);
		}

		/// <summary>
		/// リーフに対して番号を付与(チーム戦の場合はチーム番号、チーム戦でない場合はメンバー番号となる)
		/// </summary>
		/// <param name="iLeafIndex">リーフ番号</param>
		/// <param name="iInfoIndex">情報番号</param>
		public void SetLeafInfoIndex(int iLeafIndex, int iInfoIndex)
		{
			var control = mLeafInfoList[iLeafIndex].mLabelControl;
			control.SetInfo(iInfoIndex);
		}

		/// <summary>
		/// リーフが選択状態であるか
		/// </summary>
		/// <returns>選択状態フラグ</returns>
		public bool IsSelectLeaf()
		{
			foreach (var iLeaf in mLeafInfoList)
			{
				if (iLeaf.mLabelControl.SelectFilter.Visibility == Visibility.Visible)
				{
					return true;
				}
			}

			return false;
		}

        /// <summary>
        /// チーム詳細表示用コントロールの設定
        /// </summary>
        /// <param name="iControl">コントロール</param>
        public void SetTeamDisplay(TeamDisplay iControl)
        {
            mTeamDisplayControl = iControl;
        }

		/************************************************************************/
		/* コールバック処理                                                     */
		/************************************************************************/

		//============================================================================
		//! カーソルがチームラベルに入った時に呼ばれる処理
		private void _MouseEnterLeafLabel(object iSender, MouseEventArgs iArgs)
		{
			//チェック
			var control = iSender as TournamentLabelControl;
			if (control == null || !control.IsSettingInfo())
			{
				return;
			}
			_CheckSelectLabel(control.TabIndex);
		}

		//============================================================================
		//! カーソルがリーフラベルから出た時に呼ばれる処理
		private void _MouseLeaveLeafLabel(object iSender, MouseEventArgs iArgs)
		{
			//チェック
			var control = iSender as TournamentLabelControl;
			if (control == null || !control.IsSettingInfo())
			{
				return;
			}

			_ClearLeafFilter();
		}

		//============================================================================
		//! リーフラベルがクリックされた時に呼ばれる処理
		private void _MouseClickLeafLabel(object iSender, MouseEventArgs iArgs)
		{
			//チェック
			var control = iSender as TournamentLabelControl;
			if (control == null)
			{
				return;
			}

			//有効でない場合はコールバック
			if (!control.IsSettingInfo())
			{
			}
			//有効の場合は対戦チェック
			else
			{
				_ClearLeafFilter();
				_CheckBattleLeaf(control.TabIndex);
			}
		}

        //============================================================================
		//! リーフラベルが右クリックされた時に呼ばれる処理
        private void _MouseRightClickLeafLabel(object iSender, MouseEventArgs iArgs)
        {
            //チェック
            var control = iSender as TournamentLabelControl;
            if (control == null)
            {
                return;
            }

            //チーム詳細表示
            var manager = BattleOperatorManager.GetInstance();
            var team_Index = mLeafInfoList[control.TabIndex].mLabelControl.GetSetInfo();
            var team_Info = manager.GetTeamInfo(team_Index);
            var list = new List<BattleManager.SBattleJoinInfo>();
            foreach (var iJoinMemberID in team_Info.mJoinIDList)
            {
                var info = new BattleManager.SBattleJoinInfo();
                manager.GetJoinInfo(ref info, iJoinMemberID);
                list.Add(info);
            }
            mTeamDisplayControl.TeamNameTextBlock.Text = team_Info.mTeamName;
            mTeamDisplayControl.SetJoinMemberList(list);
            mTeamDisplayControl.Margin = new Thickness();
            mTeamDisplayControl.Visibility = Visibility.Visible;
        }

        //============================================================================
		//! リーフラベルが右クリックが離された時に呼ばれる処理
        private void _MouseRightReleaseLeafLabel(object iSender, MouseEventArgs iArgs)
        {
            mTeamDisplayControl.Visibility = Visibility.Hidden;
        }

		//============================================================================
		//! カーソルがオプションウィンドウの対戦待ちコントロールに入った時に呼ばれる処理
		private void _MouseEnterWaitLeaf(object iSender, MouseEventArgs iArgs)
		{
			//チェック
			var control = iSender as TournamentWaitControl;
			if (control == null)
			{
				return;
			}
			_CheckSelectLabel(control.TabIndex);
		}

		//============================================================================
		//! カーソルがオプションウィンドウの対戦待ちコントロールから出た時に呼ばれる処理
		private void _MouseLeaveWaitLeaf(object iSender, MouseEventArgs iArgs)
		{
			//チェック
			var control = iSender as TournamentWaitControl;
			if (control == null)
			{
				return;
			}

			_ClearLeafFilter();
		}

		//============================================================================
		//! オプションウィンドウの対戦待ちコントロールがクリックされた時に呼ばれる処理
		private void _MouseClickWaitLeaf(object iSender, MouseEventArgs iArgs)
		{
			//チェック
			var control = iSender as TournamentWaitControl;
			if (control == null)
			{
				return;
			}

			_ClearLeafFilter();
			_CheckBattleLeaf(control.TabIndex);
		}

		//============================================================================
		//! 対戦結果の解除ボタンが押された
		private void _ClickRemoveResult(object iSender, RoutedEventArgs iArgs)
		{
			var button = iSender as Button;
			if(button == null)
			{
				return;
			}
			_RemoveBattleResult(button.TabIndex);
		}

		//============================================================================
		//! 優勝者用ラベルがチームラベルに入った時に呼ばれる処理
		private void _MouseEnterWinnerLabel(object iSender, MouseEventArgs iArgs)
		{
			//チェック
			var control = iSender as TournamentLabelControl;
			if (control == null)
			{
				return;
			}

			//優勝者が確定してる時のみ
			if (WinnerLeafLabel.IsSettingInfo())
			{
				WinnerLeafLabel.SetVisibleFilter(true);
			}
		}

		//============================================================================
		//! 優勝者用ラベルがリーフラベルから出た時に呼ばれる処理
		private void _MouseLeaveWinnerLabel(object iSender, MouseEventArgs iArgs)
		{
			//チェック
			var control = iSender as TournamentLabelControl;
			if (control == null)
			{
				return;
			}

			//優勝者が確定してる時のみ
			if (WinnerLeafLabel.IsSettingInfo())
			{
				WinnerLeafLabel.SetVisibleFilter(false);
			}
		}

		//============================================================================
		//! 優勝者用ラベルがクリックされた時に呼ばれる処理
		private void _MouseClickWinnerLabel(object iSender, MouseButtonEventArgs iArgs)
		{
			//チェック
			var control = iSender as TournamentLabelControl;
			if (control == null)
			{
				return;
			}

			//優勝者が確定してる時のみ
			if (WinnerLeafLabel.IsSettingInfo())
			{
                System.Media.SystemSounds.Asterisk.Play();
				var result = MessageBox.Show("トーナメント結果を確定しても宜しいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (result == MessageBoxResult.Yes)
				{
					//実際はコールバックかな？取り敢えず今はこれでOK
					WinnerLeafLabel.SetVisibleFilter(false);

                    //クローズ
                    CloseOptionWindow();

					//取り敢えず今はここで書き出し及びループを行う
					var btl_Obj = BattleOperatorManager.GetInstance().GetBattleObject() as SimpleTournamentObject;
					foreach (var iLeaf in mLeafInfoList)
					{
						btl_Obj.AddLeafInfoIndex(iLeaf.mLabelControl.GetSetInfo());
					}
					foreach (var iResult in mResultList)
					{
						var info = new SimpleTournamentObject.SResult();
						info.mReafLabelIndexList = iResult.mLeafIndexList;
						info.mWinTeamIndex = iResult.mWinLeafIndex;
						info.mIgnoreTournament = false;
						btl_Obj.AddResult(info);
					}
					BattleOperatorManager.GetInstance().CommitBattle();
					SaveManager.GetInstance().AutoSaveProject();
					SceneManager.GetInstance().ReturnBackScene(SceneManager.EScene.SCENE_MAIN);
				}
			}
		}

        //============================================================================
		//! スクリーンショットの生成時に呼ばれる処理
        private string _CreateScreenShot(TournamentBattleOptionWindow.EScreenShotKind iKind)
        {
            switch (iKind)
            {
            case TournamentBattleOptionWindow.EScreenShotKind.SCREEN_SHOT_MEMBER:
                return _CreateJoinMemberScreenShot();
            case TournamentBattleOptionWindow.EScreenShotKind.SCREEN_SHOT_TOURNAMENT:
                return _CreateTournamentScreenShot();
            case TournamentBattleOptionWindow.EScreenShotKind.SCREEN_SHOT_ALL:
                return _CreateConnectScreenShot();
            }

            return "";
        }

		//============================================================================
		//! トーナメントキャンセルボタンが押された
		void _ClickCancelTournament(object iSender, RoutedEventArgs iArgs)
		{
			var result = MessageBox.Show("大会を中止して一つ前の画面に戻りますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (result == MessageBoxResult.Yes)
			{
				CloseOptionWindow();
				SceneManager.GetInstance().ReturnBackScene();
			}
		}

		/************************************************************************/
		/* 内部処理                                                             */
		/************************************************************************/

		//============================================================================
		//! リーフラベルの全てのフィルターを外す
		private void _ClearLeafFilter()
		{
			foreach (var iLeaf in mLeafInfoList)
			{
				iLeaf.mLabelControl.SetVisibleFilter(false);
			}
			if (mOptionWindow != null)
			{
				mOptionWindow.ClearSelectFilter();
			}
            mTeamDisplayControl.Visibility = Visibility.Hidden;
		}

		//============================================================================
		//! 指定したラベル番号に対戦相手がいる場合、そのラベルと対戦相手のラベルを光らせる
		private bool _CheckSelectLabel(int iLeafIndex)
		{
			//一旦選択解除
			_ClearLeafFilter();

			var target_Index = _GetBattleTargetIndex(iLeafIndex);
			if (target_Index != -1)
			{
				mLeafInfoList[iLeafIndex].mLabelControl.SetVisibleFilter(true);
				mLeafInfoList[target_Index].mLabelControl.SetVisibleFilter(true);

				//オプション画面にも反映
				if (mOptionWindow != null)
				{
					foreach (TournamentWaitControl iControl in mOptionWindow.VersusDisplayStackPanel.Children)
					{
						var check_Index = iControl.TabIndex;
						if (check_Index == iLeafIndex || check_Index == target_Index)
						{
							iControl.FilterRectangle.Visibility = Visibility.Visible;
							break;
						}
					}
				}

				return true;
			}

			return false;
		}

		//============================================================================
		//! 指定したラベル番号の対戦相手を取得(居ない場合は-1が返る)
		private int _GetBattleTargetIndex(int iLeafIndex)
		{
			//指定リーフの次のポイントが一致したリーフと対戦となる
			var my_Next_Point = _GetNextPointChecker(iLeafIndex);
			if (my_Next_Point == null)
			{
				return -1;
			}

			foreach (var iIndex in System.Linq.Enumerable.Range(0, mLeafInfoList.Count))
			{
				//自分は無視
				if (iLeafIndex == iIndex)
				{
					continue;
				}

				//一致したら決定
				if (my_Next_Point == _GetNextPointChecker(iIndex))
				{
					return iIndex;
				}
			}
			return -1;
		}

		//============================================================================
		//! 指定したリーフの次の対戦ポイントを取得する
		private VersusPointChecker _GetNextPointChecker(int iLeafIndex)
		{
			//敗北していた場合は無い
			var leaf = mLeafInfoList[iLeafIndex];
			var point_Checker = leaf.mPointRoot[leaf.mPointRoot.Count - 1];
			if (point_Checker == null)
			{
				return null;
			}

			//敗北していない場合は次を渡す
			point_Checker = point_Checker.GetDestConnectPoint();
			if (point_Checker == null || point_Checker.GetDepth() == -1)
			{
				return null;
			}
			return point_Checker;
		}

		//============================================================================
		//! 指定したラベル番号のチームに対戦相手が居た場合、対戦情報を設定する
		private void _CheckBattleLeaf(int iLeafIndex)
		{
			var target_Index = _GetBattleTargetIndex(iLeafIndex);
			if (target_Index != -1)
			{
				var up_Leaf_Index = (iLeafIndex < target_Index) ? iLeafIndex : target_Index;
				var down_Leaf_Index = (iLeafIndex < target_Index) ? target_Index : iLeafIndex;
				var up_Leaf = mLeafInfoList[up_Leaf_Index];
				var down_Leaf = mLeafInfoList[down_Leaf_Index];

				//ウィンドウ表示
                System.Media.SystemSounds.Asterisk.Play();
				var window = new TournamentResultWindow(up_Leaf.mLabelControl.GetTopName(), down_Leaf.mLabelControl.GetTopName());
				window.DecideButton.Click += (iSender, iArgs) =>
				{
					//ルートを繋げる
					var win_Team_Index = (window.UpLeafRadioButton.IsChecked == true) ? up_Leaf_Index : down_Leaf_Index;
					var lose_Team_Index = (window.UpLeafRadioButton.IsChecked == true) ? down_Leaf_Index : up_Leaf_Index;
					var last_Index = mLeafInfoList[win_Team_Index].mPointRoot.Count - 1;
					var next_Point = mLeafInfoList[win_Team_Index].mPointRoot[last_Index].GetDestConnectPoint();
					mLeafInfoList[win_Team_Index].mPointRoot.Add(next_Point);
					mLeafInfoList[lose_Team_Index].mPointRoot.Add(null);

					//リザルト情報を保存
					var info = new SBattleResult();
					info.mLeafIndexList = new int[2] { up_Leaf_Index, down_Leaf_Index };
					info.mWinLeafIndex = (window.UpLeafRadioButton.IsChecked == true) ? 0 : 1;
					var last_Game_Flg = (mResultList.Count == mBattleVal - 1);
					if (last_Game_Flg)
					{
						info.mRoundIndex = -1;
					}
					else
					{
						var up_Round = up_Leaf.mPointRoot.Count;
						var down_Round = down_Leaf.mPointRoot.Count;
						info.mRoundIndex = (up_Round > down_Round) ? up_Round : down_Round;
						--info.mRoundIndex;
					}
					mResultList.Add(info);

					//更新
					Refresh();

					//オプションウィンドウがあれば更新
					if (mOptionWindow != null)
					{
						_RefreshOptionWindow();
					}
				};

				//初期チェック
				if (iLeafIndex == down_Leaf_Index)
				{
					window.DownLeafRadioButton.IsChecked = true;
				}
				window.ShowDialogEx();
			}
		}

		//============================================================================
		//! 指定したリーフ同士の対戦を取り消せるか
		private bool IsRemoveBattleResult(int iLeafIndex1, int iLeafIndex2)
		{
			//片方が少なくとも敗北状態で無ければならない
			var last_Point1 = mLeafInfoList[iLeafIndex1].mPointRoot[mLeafInfoList[iLeafIndex1].mPointRoot.Count - 1];
			var last_Point2 = mLeafInfoList[iLeafIndex2].mPointRoot[mLeafInfoList[iLeafIndex2].mPointRoot.Count - 1];
			int win_Index;
			int lose_Index;
			if (last_Point1 == null)
			{
				if (last_Point2 == null)
				{
					return false;
				}

				win_Index = iLeafIndex2;
				lose_Index = iLeafIndex1;
			}
			else if (last_Point2 == null)
			{
				win_Index = iLeafIndex1;
				lose_Index = iLeafIndex2;
			}
			else
			{
				return false;
			}

			//勝者の現在のポイントが敗者の一つ前につながっているポイントの次のポイントの時OK
			var win_Val = mLeafInfoList[win_Index].mPointRoot.Count;
			var lose_Val = mLeafInfoList[lose_Index].mPointRoot.Count;
			if (win_Val < 2 || lose_Val < 2)
			{
				return false;
			}

			return (mLeafInfoList[win_Index].mPointRoot[win_Val - 1] == mLeafInfoList[lose_Index].mPointRoot[lose_Val - 2].GetDestConnectPoint());
		}

		//============================================================================
		//! 指定した番号のリザルトを削除する
		private void _RemoveBattleResult(int iResultIndex)
		{
			//一応念のために再チェック
			var info = mResultList[iResultIndex];
			if (!IsRemoveBattleResult(info.mLeafIndexList[0], info.mLeafIndexList[1]))
			{
				return;
			}

			//削除
			var val = mLeafInfoList[info.mLeafIndexList[0]].mPointRoot.Count;
			mLeafInfoList[info.mLeafIndexList[0]].mPointRoot.RemoveAt(val - 1);
			val = mLeafInfoList[info.mLeafIndexList[1]].mPointRoot.Count;
			mLeafInfoList[info.mLeafIndexList[1]].mPointRoot.RemoveAt(val - 1);
			mResultList.RemoveAt(iResultIndex);
			Refresh();

			//更新
			if (mOptionWindow != null)
			{
				_RefreshOptionWindow();
			}
		}

		//============================================================================
		//! ツールウィンドウの更新
		private void _RefreshOptionWindow()
		{
			//まずは対戦可能な表示から追加
			var check_List = new List<int>();
			mOptionWindow.VersusDisplayStackPanel.Children.Clear();
			int color_Index = 0;
			var color_List = new Brush[2] { new SolidColorBrush(SystemUtility.StringToColor("#AA000000")), new SolidColorBrush(SystemUtility.StringToColor("#AA555555")) };
			foreach (var iIndex in System.Linq.Enumerable.Range(0, mLeafInfoList.Count))
			{
				//既に追加済みの場合は無視
				if (check_List.IndexOf(iIndex) != -1)
				{
					continue;
				}
				check_List.Add(iIndex);

				//チーム未設定の場合は無視
				var leaf = mLeafInfoList[iIndex];
				if (!leaf.mLabelControl.IsSettingInfo())
				{
					continue;
				}

				//対戦相手の取得
				var target_Index = _GetBattleTargetIndex(iIndex);
				if (target_Index != -1)
				{
					//追加
					var control = new TournamentWaitControl();
					control.WaitNoTextBlock.Text = string.Format("対戦待機No.{0}", mOptionWindow.VersusDisplayStackPanel.Children.Count);
					control.SetUpWaitInfo(leaf.mLabelControl.GetTopName(), leaf.mLabelControl.GetDisplayImageSource());
					var my_Round = leaf.mPointRoot.Count;
					leaf = mLeafInfoList[target_Index];
					control.SetDownWaitInfo(leaf.mLabelControl.GetTopName(), leaf.mLabelControl.GetDisplayImageSource());
					var target_Round = leaf.mPointRoot.Count;
					
					//次がルートの場合は終わり
					var next = leaf.mPointRoot[leaf.mPointRoot.Count - 1].GetDestConnectPoint();
					if (next != null && next.GetDepth() == 0)
					{
						control.SetRoundDepth(-1);
					}
					else
					{
						control.SetRoundDepth((my_Round > target_Round) ? my_Round : target_Round);
					}
					control.MainGrid.Background = color_List[color_Index % 2];
					control.TabIndex = iIndex;
					control.MouseEnter += _MouseEnterWaitLeaf;
					control.MouseLeave += _MouseLeaveWaitLeaf;
					control.PreviewMouseLeftButtonDown += _MouseClickWaitLeaf;
					mOptionWindow.VersusDisplayStackPanel.Children.Add(control);
					check_List.Add(target_Index);

					++color_Index;
				}
			}

			//リザルト追加
			mOptionWindow.ResultDisplayStackPanel.Children.Clear();
			foreach (var iIndex in System.Linq.Enumerable.Range(0, mResultList.Count))
			{
				//追加
				var info = mResultList[iIndex];
				var control = new TournamentResultControl();
				control.WaitNoTextBlock.Text = string.Format("対戦結果No.{0}", mOptionWindow.ResultDisplayStackPanel.Children.Count);
				var up_Leaf = mLeafInfoList[info.mLeafIndexList[0]];
				var down_Leaf = mLeafInfoList[info.mLeafIndexList[1]];
				control.SetUpWaitInfo(up_Leaf.mLabelControl.GetTopName(), up_Leaf.mLabelControl.GetDisplayImageSource());
				control.SetDownWaitInfo(down_Leaf.mLabelControl.GetTopName(), down_Leaf.mLabelControl.GetDisplayImageSource());
				control.SetRoundDepth(info.mRoundIndex);
				control.SetWinIndex(info.mWinLeafIndex);
				control.Background = color_List[0];

				//結果解除が出来るかどうかで変更
				if (IsRemoveBattleResult(info.mLeafIndexList[0], info.mLeafIndexList[1]))
				{
					control.RemoveResultButton.TabIndex = iIndex;
					control.RemoveResultButton.Click += _ClickRemoveResult;
				}
				else
				{
					control.RemoveResultButton.IsEnabled = false;
				}

				mOptionWindow.ResultDisplayStackPanel.Children.Add(control);
				++color_Index;
			}
		}

		/************************************************************************/
		/* 片側だけで場合に使用する内部処理                                     */
		/************************************************************************/

		//============================================================================
		//! リーフ情報作成
		private void _CreateSingleLeaf()
		{
			//全てのリーフがチームとする(全て同じ深度でなければエラーとする)
			var root_Point = mTopPointChecker.GetSrcConnectPoint()[0];
			var all_Point_List = root_Point.GetLeafPointList();
			var depth = -1;
			foreach (var iPoint in all_Point_List)
			{
				if (depth != iPoint.GetDepth())
				{
					if (depth == -1)
					{
						depth = iPoint.GetDepth();
					}
					else
					{
						SystemUtility.DisplayErrorDialog("リーフ部分に深度が違う物が混じっています");
						break;
					}
				}
			}

			//作成
			foreach (var iPoint in all_Point_List)
			{
				var team_Info = new SPointLeafInfo();
				team_Info.mLabelControl = new TournamentLabelControl();
				team_Info.mLabelControl.TabIndex = mLeafInfoList.Count;
				team_Info.mLabelControl.MouseEnter += _MouseEnterLeafLabel;
				team_Info.mLabelControl.MouseLeave += _MouseLeaveLeafLabel;
				team_Info.mLabelControl.PreviewMouseLeftButtonDown += _MouseClickLeafLabel;
				MainGrid.Children.Add(team_Info.mLabelControl);
				team_Info.mVersusPoint = iPoint;
				team_Info.mPointRoot = new List<VersusPointChecker>();
				team_Info.mPointRoot.Add(team_Info.mVersusPoint);
				mLeafInfoList.Add(team_Info);
			}

            //チーム戦の場合の設定
            if (BattleOperatorManager.GetInstance().IsTeamBattleFlg())
            {
                foreach (var iLeaf in mLeafInfoList)
                {
                    iLeaf.mLabelControl.PreviewMouseRightButtonDown += _MouseRightClickLeafLabel;
                    iLeaf.mLabelControl.PreviewMouseRightButtonUp += _MouseRightReleaseLeafLabel;
                }
            }
		}

		//============================================================================
		//! ポイントチェッカーの位置を設定
		private void _CreateSinglePointPosition()
		{
			//このコントロールを必要なサイズに変更
			_RefreshSingleControlSize();

			//ポイントチェッカーのリーフ位置の設定
			_SettingSinglePointCheckerLeafPoisition();

			//ポイントチェッカーの位置設定
			_SettingSinglePointCheckerPoisition();

			//トップの位置設定
			var root = mTopPointChecker.GetSrcConnectPoint()[0];
			var top_Pos = root.GetPosition();
			top_Pos.X += mDepthWidth;
			mTopPointChecker.SetPosition(top_Pos);

			//決勝ラベルの位置変更
			var margin = WinnerLeafLabel.Margin;
			margin.Left = top_Pos.X;
			margin.Top = top_Pos.Y - (mRegistLabelSize.Height / 2);
			WinnerLeafLabel.Margin = margin;

			//対戦回数取得
			mBattleVal = root.GetInnerPointVal();
		}

		//============================================================================
		//! このコントロールを必要なサイズに変更
		private void _RefreshSingleControlSize()
		{
			var use_Size = new Point(0, 0);
			var set_Size = new Point(0, 0);
			var manager = BattleOperatorManager.GetInstance();
			var join_Val = manager.IsTeamBattleFlg() ? manager.GetTeamVal() : manager.GetJoinMemberVal();
			var depth = mTopPointChecker.GetMaxDepth();

			//横幅
			use_Size.X = Width;
			use_Size.X -= (mDisplayMargin.Left + mDisplayMargin.Right);
			use_Size.X -= (mRegistLabelSize.Width * 2);
			set_Size.X = use_Size.X / (depth + 1);

			//縦幅
			use_Size.Y = Height;
			use_Size.Y -= (mDisplayMargin.Top + mDisplayMargin.Bottom);
			use_Size.Y -= (mRegistLabelSize.Height * join_Val);
			set_Size.Y = use_Size.Y / (join_Val - 1);

			//調整
			if (set_Size.X < mDepthMinWidth)
			{
				set_Size.X = mDepthMinWidth;
			}
			if (set_Size.Y < mLabelMinOffsetHeight)
			{
				set_Size.Y = mLabelMinOffsetHeight;
			}

			//再設定
			var unfull = use_Size.X - (set_Size.X * (depth + 1));
			if (unfull < 0)
			{
				Width -= unfull;
			}
			unfull = use_Size.Y - (set_Size.Y * (join_Val - 1));
			if (unfull < 0)
			{
				Height -= unfull;
			}

			//設定
			mDepthWidth = set_Size.X;
			mLabelOffsetHeight = set_Size.Y;
		}

		//============================================================================
		//! ポイントチェッカーのリーフの位置の設定
		private void _SettingSinglePointCheckerLeafPoisition()
		{
			//ベースポイントチェッカーの位置設定(ついでに一緒に位置設定の初回分のリストを作成)
			var set_Pos = new Point(mRegistLabelSize.Width + mDisplayMargin.Left, (mRegistLabelSize.Height / 2) + mDisplayMargin.Top);
			foreach (var iLeaf in mLeafInfoList)
			{
				//位置設定
				iLeaf.mVersusPoint.SetPosition(set_Pos);

				//ラベル位置も設定
				var margin = iLeaf.mLabelControl.Margin;
				margin.Left = mDisplayMargin.Left;
				margin.Top = set_Pos.Y - (mRegistLabelSize.Height / 2);
				iLeaf.mLabelControl.Margin = margin;

				//更新
				set_Pos.Y += (mRegistLabelSize.Height + mLabelOffsetHeight);
			}
		}

		//============================================================================
		//! ポイントチェッカーの位置の設定
		private void _SettingSinglePointCheckerPoisition()
		{
			//リーフを追加
			var all_Check_Point_List = new List<VersusPointChecker>();
			var check_Point_List = new List<VersusPointChecker>();
			foreach (var iLeaf in mLeafInfoList)
			{
				all_Check_Point_List.Add(iLeaf.mVersusPoint);
				check_Point_List.Add(iLeaf.mVersusPoint);
			}

			//ループしてチェック
			while (check_Point_List.Count > 0)
			{
				var next_Point_List = new List<VersusPointChecker>();
				foreach (var iPoint in check_Point_List)
				{
					//ルートの場合は無視
					if(iPoint.GetDepth() == 0)
					{
						continue;
					}

					//次のポイントを追加
					var dest = iPoint.GetDestConnectPoint();
					if (next_Point_List.IndexOf(dest) == -1)
					{
						//まだ未挿入の場合でこのポイントの接続元両方がチェック済みの時のみ追加される
						var src_List = dest.GetSrcConnectPoint();
						if (all_Check_Point_List.IndexOf(src_List[0]) != -1 &&
						    all_Check_Point_List.IndexOf(src_List[1]) != -1)
						{
							next_Point_List.Add(dest);
						}
					}
				}

				//次の物に対して位置設定を行う
				foreach (var iPoint in next_Point_List)
				{
					//Y位置は接続元の中間地点とする
					var src_List = iPoint.GetSrcConnectPoint();
					var position_List = new Point[2] { src_List[0].GetPosition(), src_List[1].GetPosition() };
					var set_Point = new Point(0, (position_List[0].Y + position_List[1].Y) / 2);
					
					//X位置は大きい方から横への移動値を加算した物とする
					set_Point.X = (position_List[0].X > position_List[1].X) ? position_List[0].X : position_List[1].X;
					set_Point.X += mDepthWidth;
					
					//設定
					iPoint.SetPosition(set_Point);
				}

				//次の物をチェックリストへ
				check_Point_List.Clear();
				foreach (var iPoint in next_Point_List)
				{
					all_Check_Point_List.Add(iPoint);
					check_Point_List.Add(iPoint);
				}
			}
		}

        //============================================================================
        //! 参加メンバー一覧画像出力
        private string _CreateJoinMemberScreenShot()
        {
            string file_Path = SystemUtility.GetRootPath() + @"ScreenShot\" + BattleOperatorManager.GetInstance().GetBattleName() + "(参加者).png";
            try
            {
                BattleOperatorManager.GetInstance().CreateMemberScreenShot(file_Path);
            }
            catch (System.Exception iException)
            {
                System.Media.SystemSounds.Hand.Play();
                MessageBox.Show(string.Format("スクリーンショットの出力に失敗しました\n\n{0}", iException.Message), "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                file_Path = "";
            }
            return file_Path;
        }

        //============================================================================
        //! トーナメント画像出力
        private string _CreateTournamentScreenShot()
        {
            _ClearLeafFilter();

            string file_Path = SystemUtility.GetRootPath() + @"ScreenShot\" + BattleOperatorManager.GetInstance().GetBattleName() + "(トーナメント).png";
            var stock_Brush = Background;
            try
            {
                Background = new SolidColorBrush(Colors.Black);
                UpdateLayout();
                SystemUtility.OutputCaptureControl(file_Path, this);
                Background = stock_Brush;
            }
            catch (System.Exception iException)
            {
                Background = stock_Brush;
                System.Media.SystemSounds.Hand.Play();
                MessageBox.Show(string.Format("スクリーンショットの出力に失敗しました\n\n{0}", iException.Message), "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                file_Path = "";
            }

            return file_Path;
        }

        //============================================================================
        //! チーム+トーナメント画像出力
        private string _CreateConnectScreenShot()
        {
            _ClearLeafFilter();
            string file_Path = SystemUtility.GetRootPath() + @"ScreenShot\" + BattleOperatorManager.GetInstance().GetBattleName() + ".png";

            //一旦親から切り離す
            var stock_Brush = Background;
            var parent = (Panel)Parent;


            parent.Children.Remove(this);

            try
            {
                Background = new SolidColorBrush(Colors.Black);
                BattleOperatorManager.GetInstance().CreateMemberScreenShot(file_Path, this);
                parent.Children.Add(this);
                Background = stock_Brush;
            }
            catch (System.Exception iException)
            {
                if (Parent == null)
                {
                    parent.Children.Add(this);
                }

                Background = stock_Brush;
                System.Media.SystemSounds.Hand.Play();
                MessageBox.Show(string.Format("スクリーンショットの出力に失敗しました\n\n{0}", iException.Message), "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                file_Path = "";
            }

            return file_Path;
        }

		/************************************************************************/
		/* 内部定義                                                             */
		/************************************************************************/

		/// <summary>
		/// 終点情報情報
		/// </summary>
		private struct SPointLeafInfo
		{
			/// <summary>
			/// チームラベル
			/// </summary>
			public TournamentLabelControl mLabelControl;

			/// <summary>
			/// 直結しているポイントチェッカー
			/// </summary>
			public VersusPointChecker mVersusPoint;

			/// <summary>
			/// 通っているポイントチェッカーのルート(勝利する度に増え、負けると最後にnullが入る)
			/// </summary>
			public List<VersusPointChecker> mPointRoot;
		};

		/// <summary>
		/// 対戦結果情報
		/// </summary>
		private struct SBattleResult
		{
			/// <summary>
			/// リーフ番号リスト
			/// </summary>
			public int[] mLeafIndexList;

			/// <summary>
			/// 上のチームが勝っていれば0、下の場合は1とする
			/// </summary>
			public int mWinLeafIndex;

			/// <summary>
			/// 何回戦か
			/// </summary>
			public int mRoundIndex;
		};

		/************************************************************************/
		/* 変数定義                                                             */
		/************************************************************************/
		
		/// <summary>
		/// トップチェッカー
		/// </summary>
		private VersusPointChecker mTopPointChecker;

		/// <summary>
		/// 実表示のマージン
		/// </summary>
		private Thickness mDisplayMargin = new Thickness(5, 5, 5, 5);

		/// <summary>
		/// 登録ラベルのサイズ
		/// </summary>
		private Size mRegistLabelSize = new Size(200, 50);

		/// <summary>
		/// 一階層分の最小の横幅
		/// </summary>
		private double mDepthMinWidth = 96;

		/// <summary>
		/// 一階層分の横幅
		/// </summary>
		private double mDepthWidth;

		/// <summary>
		/// 登録ラベルごとの縦幅の最小の合間距離
		/// </summary>
		private double mLabelMinOffsetHeight = 23;

		/// <summary>
		/// 登録ラベルごとの縦幅の合間距離
		/// </summary>
		private double mLabelOffsetHeight;

		/// <summary>
		/// 線の太さ
		/// </summary>
		private double mLineSize = 8;

		/// <summary>
		/// チーム情報リスト
		/// </summary>
		private List<SPointLeafInfo> mLeafInfoList = new List<SPointLeafInfo>();

		/// <summary>
		/// オプションウィンドウ
		/// </summary>
		private TournamentBattleOptionWindow mOptionWindow = null;

		/// <summary>
		/// 勝敗リスト
		/// </summary>
		private List<SBattleResult> mResultList = new List<SBattleResult>();

		/// <summary>
		/// 発生する対戦回数
		/// </summary>
		private int mBattleVal;

        /// <summary>
        /// チーム詳細表示用コントロール
        /// </summary>
        private TeamDisplay mTeamDisplayControl;
	}
}
