using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// TournamentBaseControl.xaml の相互作用ロジック
	/// </summary>
	public partial class TournamentBaseControl : UserControl
	{
		/************************************************************************/
		/* 公開処理                                                             */
		/************************************************************************/

		// トーナメントの通常終了時に呼ばれるコールバック
		public delegate void EndTournamentGameDelegate();

		/// <summary>
		/// 対戦結果情報
		/// </summary>
		public struct SBattleResult
		{
			/// <summary>
			/// チームラベル番号リスト
			/// </summary>
			public int[] mTeamLabelIndexList;

			/// <summary>
			/// チーム番号リスト
			/// </summary>
			public int[] mTeamIndexList;

			/// <summary>
			/// 上のチームが勝っていれば0、下の場合は1とする
			/// </summary>
			public int mWinTeamIndex;

			/// <summary>
			/// トーナメントに表示されない部分の対戦か(3位決定戦等)
			/// </summary>
			public bool mIgnoreTournament;
		};

		/************************************************************************/
		/* 基本処理	                                                            */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public TournamentBaseControl()
		{
			InitializeComponent();
			BattleOptionItem.Click += _ClickBattleOptionItem;
			TeamListItem.Click += _ClickTeamOutputItem;
			TournamentItem.Click += _ClickTournamentOutputItem;
			ScrollbarItem.Click += _ClickScrollbarItem;
		}

		/// <summary>
		/// 初期化
		/// </summary>
		public void Initialize()
		{
			//左対応
			var btl_Operator = BattleManager.GetInstance().GetBattleOperator();
			var rule = btl_Operator.GetRuleObject() as CTournamentGameRuleBase;
			if (rule.GetTournamentDirection() == CTournamentGameRuleBase.ETournamentDirection.TOURNAMENT_DIR_LEFT)
			{
				_CreateDirLeftTournament();
			}
			//右対応
			else if (rule.GetTournamentDirection() == CTournamentGameRuleBase.ETournamentDirection.TOURNAMENT_DIR_RIGHT)
			{
				_CreateDirRightTournament();
			}
			//下対応
			else if (rule.GetTournamentDirection() == CTournamentGameRuleBase.ETournamentDirection.TOURNAMENT_DIR_DOWN)
			{
				_CreateDirDownTournament();
			}
			//左右対応
			else if (rule.GetTournamentDirection() == CTournamentGameRuleBase.ETournamentDirection.TOURNAMENT_DIR_LEFT_RIGHT)
			{
				_CreateDirLeftRithtTournament();
			}

			//初期化
			mControlScale = 1;
			mEndWaitFlg = false;
			SceneManager.GetInstance().AddKeyDownEvent(_KeyDown);
			MaskFilter.Visibility = Visibility.Hidden;
			DisplayItem.Visibility = Visibility.Visible;
			WinnerTeamLabel.TeamName.SetText("");
			mDragFlg = false;
			WinnerTeamLabel.BackGroundRectangle.Fill = SystemUtility.CreateLinearGradientBrash(Color.FromArgb(100, 200, 200, 200), Color.FromArgb(100, 255, 255, 255));
			TeamListItem.IsEnabled = btl_Operator.IsTeamBattleFlg();
		}

		/// <summary>
		/// 終了処理
		/// </summary>
		public void Terminate()
		{
			//トーナメントグループの削除
			foreach (var iControl in mTournamentGroupControlList)
			{
				ContentGrid.Children.Remove(iControl);
			}
			mTournamentGroupControlList.Clear();

			//決着トーナメントラインを削除
			foreach (var iLine in mLastLineList)
			{
				ContentGrid.Children.Remove(iLine);
			}
			mLastLineList.Clear();

			//状態保存
			var serial_Data = new SerializeData();
			serial_Data.mDiplayToolWindowFlg = (BattleOptionItem.IsChecked == true);
			serial_Data.mDisplayScrollbarFlg = (ScrollbarItem.IsChecked == true);
			DataManager.GetInstance().SetData(mContentKey, serial_Data);

			//ツールウィンドウを閉じる
			CloseToolWindow();
		}

		

		

		/************************************************************************/
		/* アクセサ                                                             */
		/************************************************************************/
		/// <summary>
		/// 結果リストの取得
		/// </summary>
		/// <returns></returns>
		public List<SBattleResult> GetReultList()
		{
			return mResultList;
		}

		/************************************************************************/
		/* コールバック処理                                                     */
		/************************************************************************/

		//============================================================================
		//! 表示が開始された時に呼ばれる処理
		private void _IsVisibleChanged(object iSender, DependencyPropertyChangedEventArgs iArgs)
		{
			//見えない時は終了
			if (IsVisible != true)
			{
				return;
			}

			//読み込み
			var get_object = DataManager.GetInstance().GetData(mContentKey);
			if (get_object != null)
			{
				var data = (SerializeData)get_object;

				//ツールウィンドウの表示
				if (data.mDiplayToolWindowFlg)
				{
					OpenToolWindow();
				}
				BattleOptionItem.IsChecked = data.mDiplayToolWindowFlg;

				//スクロールバー表示
				ScrollbarItem.IsChecked = data.mDisplayScrollbarFlg;
				var visibility = ScrollbarItem.IsChecked ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden;
				ContentScrollViewer.HorizontalScrollBarVisibility = visibility;
				ContentScrollViewer.VerticalScrollBarVisibility = visibility;
			}
			else
			{
				OpenToolWindow();
			}
		}

		//============================================================================
		//! キー入力が行われた
		private void _KeyDown(object iSender, KeyEventArgs iArgs)
		{
			//終了
			if (mEndWaitFlg)
			{
				if (iArgs.Key == Key.Return)
				{
					if (mEndTournamentGameDelegate != null)
					{
						mEndTournamentGameDelegate();
					}
					return;
				}
			}

			//リザルト情報がありCTRL + Zの時
			else if (mResultList.Count > 0 &&
					((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None) &&
					iArgs.Key == Key.Z)
			{
				//警告
				System.Media.SystemSounds.Exclamation.Play();
				var end_Result = MessageBox.Show("最後に勝利した対戦を取り消しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
				if (end_Result == MessageBoxResult.Yes)
				{
					_RemoveBattleResult(mResultList.Count - 1);
				}
			}
		}

		//============================================================================
		//! スクロール初期化の為の物
		private void _ScrollChanged(object iSender, ScrollChangedEventArgs iArgs)
		{
			ContentScrollViewer.ScrollChanged -= _ScrollChanged;
			var btl_Operator = BattleManager.GetInstance().GetBattleOperator();
			var rule = btl_Operator.GetRuleObject() as CTournamentGameRuleBase;

			//スクロール位置を一番右へ設定
			if (rule.GetTournamentDirection() == CTournamentGameRuleBase.ETournamentDirection.TOURNAMENT_DIR_RIGHT)
			{
				ContentScrollViewer.ScrollToRightEnd();
			}
			else if (rule.GetTournamentDirection() == CTournamentGameRuleBase.ETournamentDirection.TOURNAMENT_DIR_DOWN)
			{
				ContentScrollViewer.ScrollToBottom();
			}

			//縮小倍率の算出
			if (ContentScrollViewer.ExtentWidth > ContentScrollViewer.ExtentHeight)
			{
				mMinScale = Width / ContentScrollViewer.ExtentWidth;
			}
			else
			{
				mMinScale = Height / ContentScrollViewer.ExtentHeight;
			}

			if (mMinScale > 1)
			{
				mMinScale = 1;
			}
		}



		//============================================================================
		//! マウスホイールが回転
		private void _RollMouseWheel(object iSender, MouseWheelEventArgs iArgs)
		{
			//拡縮
			mControlScale += ((iArgs.Delta < -1) ? -mWheelScaleSpeed : mWheelScaleSpeed);
			if (mControlScale < mMinScale) { mControlScale = mMinScale; }
			else if (mControlScale > mMaxScale) { mControlScale = mMaxScale; }

			//設定
			var group = new TransformGroup();
			group.Children.Add(new ScaleTransform(mControlScale, mControlScale));
			ScaleGrid.LayoutTransform = group;

			iArgs.Handled = true;
		}

		//============================================================================
		//! ツールウィンドウの項目の範囲内にマウスが入った
		private void _MouseEnterBattleInfo(object iSender, MouseEventArgs iArgs)
		{
			var control = iSender as TournamentVersusControl;
			if (control != null)
			{
				_CheckSelectLabel(control.GetTeamLabelIndexList()[0]);
			}
		}

		//============================================================================
		//! ツールウィンドウの項目の範囲内にからマウスが出た
		private void _MouseLeaveBattleInfo(object iSender, MouseEventArgs iArgs)
		{
			//全部のチームを未選択にする
			foreach (var iGroupControl in mTournamentGroupControlList)
			{
				iGroupControl.ClearSelectTeam();
			}
		}

		//============================================================================
		//! ツールウィンドウの項目がクリックされた
		private void _MouseClickBattleInfo(object iSender, MouseEventArgs iArgs)
		{
			var control = iSender as TournamentVersusControl;
			if (control != null)
			{
				var label_Index_List = control.GetTeamLabelIndexList();
				_CheckBattleTeam(label_Index_List[0]);
			}
		}

		//============================================================================
		//! ツールウィンドウのリザルト項目の範囲内にマウスが入った
		private void _MouseEnterResultInfo(object iSender, MouseEventArgs iArgs)
		{
			var control = iSender as TournamentVersusControl;
			if (control != null)
			{
				var label_Indexl_List = control.GetTeamLabelIndexList();

				//二つのラベルを光らせて終了
				var group_Control = mTournamentGroupControlList[0];
				group_Control.SetVisibleTeamSelectFilter(label_Indexl_List[0], true);
				group_Control.SetVisibleTeamSelectFilter(label_Indexl_List[1], true);
			}
		}

		//============================================================================
		//! ツールウィンドウの項目の範囲内にからマウスが出た
		private void _MouseLeaveResultInfo(object iSender, MouseEventArgs iArgs)
		{
			//全部のチームを未選択にする
			foreach (var iGroupControl in mTournamentGroupControlList)
			{
				iGroupControl.ClearSelectTeam();
			}
		}

		//============================================================================
		//! ツールウィンドウのリザルト項目がクリックされた
		private void _MouseClickResultInfo(object iSender, MouseEventArgs iArgs)
		{
			var control = iSender as TournamentVersusControl;
			if (control != null)
			{
				var index_List = control.GetTeamLabelIndexList();
				if (_IsRemoveBattleResult(index_List[0], index_List[1]))
				{
					//リザルト番号検出して削除
					int result_Index = -1;
					foreach (var iIndex in System.Linq.Enumerable.Range(0, mResultList.Count))
					{
						var result_Index_List = mResultList[iIndex].mTeamLabelIndexList;
						if ((index_List[0] == result_Index_List[0] && index_List[1] == result_Index_List[1]) ||
						   (index_List[0] == result_Index_List[1] && index_List[1] == result_Index_List[0]))
						{
							result_Index = iIndex;
							break;
						}
					}

					//検出されていれば削除
					if (result_Index != -1)
					{
						//警告
						System.Media.SystemSounds.Exclamation.Play();
						var end_Result = MessageBox.Show("指定した対戦結果を取り消しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
						if (end_Result == MessageBoxResult.Yes)
						{
							_RemoveBattleResult(result_Index);
						}
					}
				}
				else
				{
					System.Media.SystemSounds.Asterisk.Play();
					MessageBox.Show("この対戦はキャンセル出来ません", "エラー", MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
		}

		//============================================================================
		//! チームメンバー一覧画像出力
		private void _ClickTeamOutputItem(object iSender, RoutedEventArgs iArgs)
		{
			string file_Path = SystemUtility.GetRootPath() + @"ScreenShot\" + BattleManager.GetInstance().GetBattleOperator().GetBattleName() + "(チーム).png";
			if (TeamSelectControl.OutputTeamImageFile(file_Path))
			{
				System.Media.SystemSounds.Asterisk.Play();
				MessageBox.Show("以下にスクリーンショットを出力しました\n\n" + file_Path, "確認", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			else
			{
				System.Media.SystemSounds.Hand.Play();
				MessageBox.Show("スクリーンショットの出力に失敗しました", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		//============================================================================
		//! トーナメント画像出力
		private void _ClickTournamentOutputItem(object iSender, RoutedEventArgs iArgs)
		{
			var stack_Group = ScaleGrid.LayoutTransform;
			var visible = ContentScrollViewer.VerticalScrollBarVisibility;

			try
			{
				string file_Path = SystemUtility.GetRootPath() + @"ScreenShot\" + BattleManager.GetInstance().GetBattleOperator().GetBattleName() + "(トーナメント).png";
				foreach (var iControl in mTournamentGroupControlList)
				{
					iControl.ClearSelectTeam();
				}

				var group = new TransformGroup();
				group.Children.Add(new ScaleTransform(1, 1));
				ScaleGrid.LayoutTransform = group;
				ContentScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
				ContentScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
				ContentGrid.UpdateLayout();

				SystemUtility.OutputCaptureControl(file_Path, ScaleGrid);

				ScaleGrid.LayoutTransform = stack_Group;
				ContentScrollViewer.VerticalScrollBarVisibility = visible;
				ContentScrollViewer.HorizontalScrollBarVisibility = visible;
				ScaleGrid.UpdateLayout();
				System.Media.SystemSounds.Asterisk.Play();
				MessageBox.Show("以下にスクリーンショットを出力しました\n\n" + file_Path, "確認", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (System.Exception)
			{
				ScaleGrid.LayoutTransform = stack_Group;
				ContentScrollViewer.VerticalScrollBarVisibility = visible;
				ContentScrollViewer.HorizontalScrollBarVisibility = visible;
				ScaleGrid.UpdateLayout();
				System.Media.SystemSounds.Hand.Play();
				MessageBox.Show("スクリーンショットの出力に失敗しました", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		//============================================================================
		//! オプション画面表示アイテムがクリックされた
		private void _ClickBattleOptionItem(object iSender, RoutedEventArgs iArgs)
		{
			if (mOptionWindow != null)
			{
				CloseToolWindow();
			}
			else
			{
				OpenToolWindow();
			}
		}

		//============================================================================
		//! スクロールバー表示切り替えアイテムがクリックされた
		private void _ClickScrollbarItem(object iSender, RoutedEventArgs iArgs)
		{
			ScrollbarItem.IsChecked = !ScrollbarItem.IsChecked;

			//切り替え
			var visibility = ScrollbarItem.IsChecked ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden;
			ContentScrollViewer.HorizontalScrollBarVisibility = visibility;
			ContentScrollViewer.VerticalScrollBarVisibility = visibility;
		}

		//============================================================================
		//! コンテンツがクリックされた
		private void _ClickScrollContent(object iSender, MouseButtonEventArgs iArgs)
		{
			//現在ラベルが選択されてる時は無視
			foreach (var iControl in mTournamentGroupControlList)
			{
				if (iControl.IsSelectLabel())
				{
					return;
				}
			}

			//ドラッグ開始
			mCursorPosition = iArgs.GetPosition(null);
			mDragFlg = true;
		}

		//============================================================================
		//! メイン画面からカーソルが外に出る
		private void _MouseLeaveControl(object iSender, MouseEventArgs iArgs)
		{
			mDragFlg = false;
		}

		//============================================================================
		//! コンテンツのドラッグ状態を終了
		private void _ReleaseMouseLeftButton(object iSender, MouseButtonEventArgs iArgs)
		{
			mDragFlg = false;
		}

		//============================================================================
		//! マウスの移動が行われた
		private void _MouseMove(object iSender, MouseEventArgs iArgs)
		{
			if (mDragFlg)
			{
				//スクロール位置の設定
				var position = iArgs.GetPosition(null);
				var set_Scroll_Pos = mCursorPosition - position;
				set_Scroll_Pos.X = ContentScrollViewer.ContentHorizontalOffset + set_Scroll_Pos.X;
				if (set_Scroll_Pos.X < 0) { set_Scroll_Pos.X = 0; }
				else if (set_Scroll_Pos.X > ContentScrollViewer.ScrollableWidth) { set_Scroll_Pos.X = ContentScrollViewer.ScrollableWidth; }
				set_Scroll_Pos.Y = ContentScrollViewer.ContentVerticalOffset + set_Scroll_Pos.Y;
				if (set_Scroll_Pos.Y < 0) { set_Scroll_Pos.Y = 0; }
				else if (set_Scroll_Pos.Y > ContentScrollViewer.ScrollableHeight) { set_Scroll_Pos.Y = ContentScrollViewer.ScrollableHeight; }

				ContentScrollViewer.ScrollToHorizontalOffset(set_Scroll_Pos.X);
				ContentScrollViewer.ScrollToVerticalOffset(set_Scroll_Pos.Y);

				//情報保存
				mCursorPosition = position;
			}
		}

		/************************************************************************/
		/* 内部定義	                                                            */
		/************************************************************************/
		

        

		//============================================================================
		//! 指定したチーム同士の対戦結果を消去できるか
		private bool _IsRemoveBattleResult(int iTeamLabel1Index, int iTeamLabel2Index)
		{
			//複数ブロックに分かれている場合
			if (mTournamentGroupControlList.Count > 1)
			{
				return false;
			}
			else
			{
				//削除できるか
				return (mTournamentGroupControlList[0].IsRemoveBattleResult(iTeamLabel1Index, iTeamLabel2Index));
			}
		}

		//============================================================================
		//! 指定した番号のリザルトを削除する
		private bool _RemoveBattleResult(int iResultIndex)
		{
			if (mTournamentGroupControlList.Count > 1)
			{
			}
			else
			{
				var info = mResultList[iResultIndex];
				var win_Label_Index = info.mTeamLabelIndexList[info.mWinTeamIndex];
				var control = mTournamentGroupControlList[0];
				if (control.ClearBattleResult(win_Label_Index))
				{
					mResultList.RemoveAt(iResultIndex);

					if (mOptionWindow != null)
					{
						_RefreshToolWindow();
					}
					return true;
				}
			}
			return false;
		}

		//============================================================================
		//! 全ての対戦が終了したかチェック
		private void _CheckFinishTournament()
		{
			if(mTournamentGroupControlList.Count == 1 && mResultList.Count == mTournamentGroupControlList[0].GetBattleValue())
			{
				//終了
				_FinishTournamentGame();

				//ラインに線を引く
				foreach (var iLine in mLastLineList)
				{
					iLine.Stroke = new SolidColorBrush(Colors.Red);
				}

				var stack = BattleOptionItem.IsChecked;
				CloseToolWindow();
				BattleOptionItem.IsChecked = stack;
			}
		}

		//============================================================================
		//! 勝利チームの確定
		private void _FinishTournamentGame()
		{
			//優勝者ラベル設定
			var last_Result = mResultList[mResultList.Count - 1];
			WinnerTeamLabel.SetTeamIndex(last_Result.mTeamIndexList[last_Result.mWinTeamIndex]);

			//スクロールバーの非表示
			ContentScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
			ContentScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;

			//表示アイテム使用不可能
			DisplayItem.Visibility = Visibility.Hidden;

			//マスクフィルタ設定
			MaskFilter.Visibility = Visibility.Visible;
			MaskFilter.Width = ActualWidth;
			MaskFilter.Height = ActualHeight - TournamentMenu.Height;
			MaskFilter.Margin = new Thickness(0, TournamentMenu.Height,0,0);

			//フェードアニメーション
			var storyboard = new Storyboard();
			storyboard.Completed += (iSender, iArgs) =>
			{
				mEndWaitFlg = true;
			};
			bool back_flg = SceneManager.GetInstance().IsChangeSceneBackFlg();
			var animation = new DoubleAnimation
			{
				From = 0,
				To = 0.5,
				Duration = TimeSpan.FromMilliseconds(300)
			};
			Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));
			storyboard.Children.Add(animation);
			MaskFilter.BeginStoryboard(storyboard);
		}

		/************************************************************************/
		/* 内部定義                                                             */
		/************************************************************************/
		/************************************************************************/
		/* 変数定義                                                             */
		/************************************************************************/

		/// <summary>
		/// 現在のコントロールの拡大度
		/// </summary>
		private double mControlScale = 1;

		/// <summary>
		/// コントロールの最大拡大度
		/// </summary>
		private double mMaxScale = 2.0f;

		/// <summary>
		/// コントロールの最小拡大度
		/// </summary>
		private double mMinScale = 0.5f;

		/// <summary>
		/// マウスホイールに寄る回転の拡縮の速度
		/// </summary>
		private float mWheelScaleSpeed = 0.1f;

		/// <summary>
		/// 終了待ちフラグ
		/// </summary>
		private bool mEndWaitFlg;

		/// <summary>
		/// ドラッグフラグ
		/// </summary>
		private bool mDragFlg;

		/// <summary>
		/// 一つ前のカーソル位置保存場所
		/// </summary>
		private Point mCursorPosition = new Point();

		/// <summary>
		/// 勝敗リスト
		/// </summary>
		private List<SBattleResult> mResultList = new List<SBattleResult>();

		


		/// <summary>
		/// 最終決着ラインリスト
		/// </summary>
		private List<Line> mLastLineList = new List<Line>();

		/// <summary>
		/// トーナメントの通常終了時に呼ばれるコールバック
		/// </summary>
		private EndTournamentGameDelegate mEndTournamentGameDelegate;

		/// <summary>
		/// コンテンツキー
		/// </summary>
		private string mContentKey = "TournamentBaseControl";
	}
}
