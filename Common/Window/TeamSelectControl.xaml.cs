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

namespace FightingCommunityAdministrator
{
    /// <summary>
    /// TeamSelectControl.xaml の相互作用ロジック
    /// </summary>
    public partial class TeamSelectControl : UserControl, SceneInterface
    {
        /************************************************************************/
        /* 基本処理                                                             */
        /************************************************************************/

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TeamSelectControl()
        {
            InitializeComponent();

            //システム用のイメージを生成
            _CreateSystemImage();

            //ドラッグ用のコントロールを生成
            mDragDisplayMember = new MemberDetailsControl();
            mDragDisplayMember.HorizontalAlignment = HorizontalAlignment.Left;
            mDragDisplayMember.VerticalAlignment = VerticalAlignment.Top;
            mDragDisplayMember.Opacity = 0.75;
            MainGrid.Children.Add(mDragDisplayMember);
        }

        /************************************************************************/
        /* 継承処理                                                             */
        /************************************************************************/

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="iBackFlg">シーンから戻ってきたかのフラグ</param>
        public void Initialize(bool iBackFlg) 
        {
            mDragDisplayMember.Visibility = Visibility.Hidden;
            mDragMember = null;
            mCurrentCursorPosition = -1;
            mOperator = BattleManager.GetInstance().GetBattleOperator();

            //コントロールの作成
            foreach (var iInfo in mOperator.GetMemberList())
            {
                //コントロール作成と追加
                var member = new MemberDetailsControl(iInfo);
                member.TabIndex = mJoinMemberControlList.Count;
                member.PreviewMouseLeftButtonDown += _ClickLeftCharacterControl;
                member.PreviewMouseRightButtonDown += _ClickRightCharacterControl;
                member.SetBackGrondColor(Color.FromArgb(50, 0, 255, 0), Color.FromArgb(0, 0, 0, 0));
                mJoinMemberControlList.Add(member);
            }

            //状態更新
            _RefreshDisplay();
        }

        /// <summary>
        /// 終端処理
        /// </summary>
        public void Terminate() { }

		/************************************************************************/
		/* アクセサ                                                             */
		/************************************************************************/

		/// <summary>
		/// 次のシーンを設定
		/// </summary>
		/// <param name="iScene">シーン番号</param>
		public void SetmNextScene(SceneManager.EScene iScene)
		{
			mNextScene = iScene;
		}

        /************************************************************************/
        /* コールバック処理                                                     */
        /************************************************************************/

        //============================================================================
        //! メンバーが左クリックされた
        private void _ClickLeftCharacterControl(object iSender, MouseButtonEventArgs iArgs)
        {
            var control = iSender as MemberDetailsControl;
            if (control != null)
            {
                mDragMember = control;

                var info = mOperator.GetMember(control.TabIndex);
                mDragDisplayMember.Visibility = Visibility.Visible;
                mDragDisplayMember.SetName(mDragMember.GetName());
                mDragDisplayMember.SetCharacterIcon(info.mUserCharacterID);
                _SetDragDisplayPosition(iArgs.GetPosition(null));
            }
        }

        //============================================================================
        //! メンバーが右クリックされた
        private void _ClickRightCharacterControl(object iSender, MouseButtonEventArgs iArgs)
        {
            var control = iSender as MemberDetailsControl;
            if (control != null)
            {
                _SetMemberTeam(control.TabIndex, -1);
                _RefreshDisplay();
            }
        }

        //============================================================================
        //! マウスの移動が行われた
        private void _MouseMove(object iSender, MouseEventArgs iArgs)
        {
            if (mDragMember != null)
            {
                _SetDragDisplayPosition(iArgs.GetPosition(null));
            }
        }

        //============================================================================
        //! メイン画面からカーソルが外に出る
        private void _MouseLeaveControl(object iSender, MouseEventArgs iArgs)
        {
            _ClearDrag();
        }

        //============================================================================
        //! 画面中でマウスの左ボタンが離された
        private void _ReleaseMouseLeftButton(object iSender, MouseButtonEventArgs iArgs)
        {
            if (mDragMember != null)
            {
                int member_Index = mDragMember.TabIndex;
                int current_Team_Index = mOperator.GetMemberJoinTeamIndex(member_Index);

                try
                {
                    //パネルに所属させる
                    if (mCurrentCursorPosition != -1)
                    {
                        if (mCurrentCursorPosition == 0)
                        {
                            mOperator.RemoveTeamMember(member_Index);
                        }
                        else
                        {
                            mOperator.AddTeamMember(mCurrentCursorPosition - 1, member_Index);
                        }
                    }

                    //ドラッグ終了
                    _ClearDrag();
                    _RefreshDisplay();

                    //ここで全てのメンバーの所属が決まったら次に進むかを聞いてＯＫなら進む
                    foreach (var iInfo in mOperator.GetMemberList())
                    {
                        if (mOperator.GetMemberJoinTeamIndex(iInfo.mMemberID) == -1) { return; }
                    }

                    //１チームしかない場合はエラー
                    if (TeamWrapPanel.Children.Count < 2)
                    {
                        SystemUtility.DisplayErrorDialog("チームが１つ以下です、チームを２チーム以上に分割して下さい");
                        throw new System.Exception();
                    }

					//各チームにメンバーが一人は所属していること
					foreach (TeamDetailsControl iControl in TeamWrapPanel.Children)
					{
						if(iControl.MemberWrapPanel.Children.Count == 0)
						{
	                        SystemUtility.DisplayErrorDialog("一人のメンバーも所属していないチームが存在しています");
		                    throw new System.Exception();
						}
                    }

                    //確認
                    System.Media.SystemSounds.Question.Play();
                    var result = MessageBox.Show("このチーム編成で先に進みますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                    if (result == MessageBoxResult.Yes)
                    {
                        //予約先へ進む
						if (mOperator.SetCommitTeam(true))
						{
							SceneManager.GetInstance().AdvanceScene(mNextScene);

							var stack = TeamWrapPanel.Background;
							TeamWrapPanel.Background = new SolidColorBrush(SystemUtility.StringToColor("#C8000000"));
							TeamWrapPanel.UpdateLayout();
							mCurrentTeamBitmap = SystemUtility.GetCaptureControlBitmap(TeamWrapPanel);
							TeamWrapPanel.Background = stack;
						}
                    }
                    else
                    {
						throw new System.Exception();
                    }
                }
                catch (System.Exception)
                {
                    mOperator.RemoveTeamMember(member_Index);
                    _RefreshDisplay();
                }
            }
        }

        //============================================================================
		//! メンバースタックパネルにカーソルが入った
        private void _MouseEnterMemberStackPanel(object iSender, MouseEventArgs iArgs)
        {
			var viewer = iSender as ContentControl;
			if (viewer != null)
			{
				mCurrentCursorPosition = viewer.TabIndex;
			}
        }

        //============================================================================
        //! 表示中のフィルターからカーソルが出た
        private void _MouseLeaveFilter(object iSender, MouseEventArgs iArgs)
        {
            var rectangle = iSender as Rectangle;
            if (rectangle != null)
            {
                mCurrentCursorPosition = -1;
            }
        }

		//============================================================================
        //! 新規チームが作成された
		private void _ClickNewTeamButton(object iSender, RoutedEventArgs iArgs)
		{
			int index = mOperator.GetTeamVal();
			while (true)
			{
				try
				{
					mOperator.AddTeam("チーム" + index.ToString());
					break;
				}
				catch (System.Exception)
				{
				}

				++index;
			}

			_RefreshDisplay();
		}

		//============================================================================
		//! 戻るボタンのクリック
		private void _ClickReturnBackButton(object iSender, RoutedEventArgs iArgs)
		{
			//参加者のチーム分けを全て戻す
			BattleManager.GetInstance().GetBattleOperator().ClearTeam();
			SceneManager.GetInstance().ReturnBackScene();
		}

		//============================================================================
		//! チームの削除ボタンがクリックされた
		private void _ClickTeamRemoveButton(object iSender, RoutedEventArgs iArgs)
		{
			var button = iSender as Button;
			if (button != null)
			{
				mOperator.RemoveTeam(button.TabIndex);
				_RefreshDisplay();
			}
		}

		//============================================================================
		//! チーム名変更時に呼ばれる
		private void _ChangeTeamName(object iSender, RoutedEventArgs iArgs)
		{
			var textbox = iSender as TextBox;
			if (textbox != null)
			{
				//変更が無ければ無視
				var index = textbox.TabIndex;
				var info = mOperator.GetTeamInfo(index);
				if (info.mTeamName == textbox.Text)
				{
					return;
				}

				//変更があった場合は全てのチームと比較して同一の名前があった時にエラーを出して戻す
				foreach (var iIndex in System.Linq.Enumerable.Range(0, mOperator.GetTeamVal()))
				{
					//自分自身以外
					if (iIndex != index && mOperator.GetTeamInfo(iIndex).mTeamName == textbox.Text)
					{
						SystemUtility.DisplayErrorDialog("同じチーム名が存在する為、変更に失敗しました");
						textbox.Text = mOperator.GetTeamInfo(index).mTeamName;
						return;
					}
				}

				//成功という事で変更をかける
				info.mTeamName = textbox.Text;
				mOperator.SetTeamInfo(index, info);
			}
		}

		/************************************************************************/
		/* 静的処理                                                             */
		/************************************************************************/

		/// <summary>
		/// 
		/// </summary>
		/// <param name="iFilePath"></param>
		/// <returns></returns>
		static public bool OutputTeamImageFile(string iFilePath)
		{
			try
			{
				SystemUtility.OutputImageFile(iFilePath, mCurrentTeamBitmap);
				return true;
			}
			catch (System.Exception)
			{
				return false;
			}
		}

        /************************************************************************/
        /* 内部定義                                                             */
        /************************************************************************/

        //============================================================================
        //! システムイメージの作成
        private void _CreateSystemImage()
        {
            var manager = ResourceManager.GetInstance();
            var resource_Path = SystemUtility.GetResourcePath();
			MemberHeaderImage.Source = manager.CreateImage(resource_Path + @"\System\MemberHeader.adg");
        }

        //============================================================================
        //! ドラッグ表示物の位置を設定
        private void _SetDragDisplayPosition(Point iPosition)
        {
            //位置設定
            var margin = new Thickness();
            margin.Left = iPosition.X - (mDragDisplayMember.Width / 2);
            margin.Top = iPosition.Y - mDragDisplayMember.Height - 5;
            margin.Right = margin.Bottom = 0;
            mDragDisplayMember.Margin = margin;
        }

        //============================================================================
        //! ドラッグ状態の初期化
        private void _ClearDrag()
        {
            mDragMember = null;
            mDragDisplayMember.Visibility = Visibility.Hidden;
        }

        //============================================================================
        //! メンバー状態の更新
        private void _RefreshDisplay()
        {
            MemberWrapPanel.Children.Clear();
            foreach (TeamDetailsControl iControl in TeamWrapPanel.Children)
            {
                iControl.MemberWrapPanel.Children.Clear();
            }
            TeamWrapPanel.Children.Clear();

            //チームコントロール作成
            foreach (var iIndex in System.Linq.Enumerable.Range(0, mOperator.GetTeamVal()))
            {
                var control = new TeamDetailsControl();
                var team_Info = mOperator.GetTeamInfo(iIndex);
                control.TeamNameTextBox.Text = team_Info.mTeamName;
				control.TeamNameTextBox.TabIndex = iIndex;
				control.TeamNameTextBox.LostFocus += _ChangeTeamName;
				control.DeleteButton.TabIndex = iIndex;
				control.DeleteButton.Click += _ClickTeamRemoveButton;
                control.MouseEnter += _MouseEnterMemberStackPanel;
                control.TabIndex = iIndex + 1;

                //このチームに参加しているメンバーを追加
                foreach (var iMemberID in team_Info.mMemberIDList)
                {
                    control.MemberWrapPanel.Children.Add(mJoinMemberControlList[mOperator.GetMember(iMemberID).mJoinID]);
                    //member_List.Remove(iMemberID);
                }

                TeamWrapPanel.Children.Add(control);
            }

            //メンバーの所属
            var index = 0;
            foreach (var iInfo in BattleManager.GetInstance().GetBattleOperator().GetMemberList())
            {
                var control = mJoinMemberControlList[index];
                var team_Index = mOperator.GetMemberJoinTeamIndex(iInfo.mMemberID);
                if(team_Index == -1)
                {
                    MemberWrapPanel.Children.Add(control);
                }
                else
                {
                }
                ++index;
            }
        }

        //============================================================================
        //! 指定番号のメンバーの所属をリセットする
        private void _SetMemberTeam(int iMemberIndex, int iTeamIndex)
        {
            
        }

        /************************************************************************/
        /* 変数定義                                                             */
        /************************************************************************/

        /// <summary>
        /// 参加者のコントロールリスト
        /// </summary>
        private List<MemberDetailsControl> mJoinMemberControlList = new List<MemberDetailsControl>();

        /// <summary>
        /// ドラッグしているメンバーのオブジェクトを取得
        /// </summary>
        private MemberDetailsControl mDragMember;

        /// <summary>
        /// ドラッグ中に半透明でカーソルに追従するメンバー情報
        /// </summary>
        private MemberDetailsControl mDragDisplayMember;

        /// <summary>
        /// 大会操作機構
        /// </summary>
        private BattleOperator mOperator;

        /// <summary>
        /// カーソルの位置(0:メンバーパネル 1～:チーム番号+1)
        /// </summary>
        private int mCurrentCursorPosition;

		/// <summary>
		/// 現在のチーム編成の保存画像
		/// </summary>
		static private RenderTargetBitmap mCurrentTeamBitmap;

		/// <summary>
		/// 次に進むシーン番号
		/// </summary>
		private SceneManager.EScene mNextScene;
    }
}
