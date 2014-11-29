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
	public partial class TournamentControl : UserControl
	{
		/// <summary>
		/// コンストラクタ
		/// </summary>
		public TournamentControl()
		{
			InitializeComponent();
			BackgroundImage.Source = PresetManager.GetInstance().GetBackGroundImage();
			SystemManager.GetInstance().SetCloseCheckCallback(_CheckClose);
			SystemManager.GetInstance().SetInputKeyDelegate(_InputKey);
            TeamDetailsControl.Visibility = Visibility.Hidden;
            MainControl.SetTeamDisplay(TeamDetailsControl);
		}

		/// <summary>
		/// 初期化
		/// </summary>
		/// <param name="iTopPonitChecker">トップポイントチェッカーを設定</param>
		public void Initialize(VersusPointChecker iTopPonitChecker)
		{
			MainControl.SetTopPointChecker(iTopPonitChecker);
		}

		/************************************************************************/
		/* アクセサ                                                             */
		/************************************************************************/

		/// <summary>
		/// バトルコントロールの取得
		/// </summary>
		/// <returns></returns>
		public TournamentBattleControl GetBattleControl()
		{
			return MainControl;
		}
		
		/************************************************************************/
		/* コールバック処理                                                     */
		/************************************************************************/

		//============================================================================
		//! スクロール初期化の為の物
		private void _ScrollChanged(object iSender, ScrollChangedEventArgs iArgs)
		{
			ContentScrollViewer.ScrollChanged -= _ScrollChanged;


			/*
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
			}*/

			//縮小倍率の算出
			var unfll_Width = ContentScrollViewer.ExtentHeight - Width;
			var unfll_Height = ContentScrollViewer.ExtentHeight - Height;

			//横幅の方が足りない部分が大きい
			if (unfll_Width > unfll_Height)
			{
				mMinScale = Width / ContentScrollViewer.ExtentWidth;
			}
			//縦幅の方が足りない部分が大きい
			else
			{
				mMinScale = Height / ContentScrollViewer.ExtentHeight;
			}
		}

		//============================================================================
		//! ウィンドウを閉じていいかどうかの確認
		private bool _CheckClose()
		{
			//ダイアログが出ている間だけ非表示
			if (MainControl.IsOpenOptionWindow())
			{
				MainControl.SetVisibleOptionWindow(false);
			}

            System.Media.SystemSounds.Exclamation.Play();
			var result = MessageBox.Show("大会中ですが終了しても宜しいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (result == MessageBoxResult.Yes)
			{
				MainControl.CloseOptionWindow();
				return true;
			}

			//キャンセルされた場合は再表示
			if (MainControl.IsOpenOptionWindow())
			{
				MainControl.SetVisibleOptionWindow(true);
			}
			return false;
		}

		//============================================================================
		//! キーが押された時に呼ばれる
		private void _InputKey(KeyEventArgs iArgs)
		{
			//Ctrl押されてる時
			if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None)
			{
				//全体表示
				if (iArgs.Key == Key.A)
				{
					_ContentScaling(mMinScale);
				}
                //オリジナル
                if (iArgs.Key == Key.D)
                {
                    _ContentScaling(1);
                }
			}
			//その他
			else
			{
				//オプションウィンドウの切り替え
				if (iArgs.Key == Key.F1)
				{
					if (MainControl.IsOpenOptionWindow())
					{
						MainControl.CloseOptionWindow();
					}
					else
					{
						MainControl.OpenOptionWindow();
					}
				}
			}
		}

		//============================================================================
		//! コンテンツがクリックされた
		private void _ClickScrollContent(object iSender, MouseButtonEventArgs iArgs)
		{
			//現在ラベルが選択されてる時は無視
			if (MainControl.IsSelectLeaf())
			{
				return;
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
		//! マウスホイールが回転
		private void _RollMouseWheel(object iSender, MouseWheelEventArgs iArgs)
		{
			_ContentScaling(mControlScale + ((iArgs.Delta < -1) ? -mWheelScaleSpeed : mWheelScaleSpeed));
			iArgs.Handled = true;
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
		/* 内部処理                                                             */
		/************************************************************************/

		//============================================================================
		//! 画面の拡縮を変更する
		private void _ContentScaling(double iScale)
		{
			if (iScale < mMinScale) { iScale = mMinScale; }
			else if (iScale > mMaxScale) { iScale = mMaxScale; }
			mControlScale = iScale;

			var group = new TransformGroup();
			group.Children.Add(new ScaleTransform(mControlScale, mControlScale));
			ScaleGrid.LayoutTransform = group;
		}

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
		/// ドラッグフラグ
		/// </summary>
		private bool mDragFlg;

		/// <summary>
		/// 一つ前のカーソル位置保存場所
		/// </summary>
		private Point mCursorPosition = new Point();
	}
}
