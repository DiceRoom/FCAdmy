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
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using Microsoft.Win32;
using WindowsForm = System.Windows.Forms;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// MemberControl.xaml の相互作用ロジック
	/// </summary>
	public partial class TeamSelectScene : UserControl,SceneInterface
	{
        /************************************************************************/
        /* 公開処理                                                             */
        /************************************************************************/

        /// <summary>
        /// メンバー確定時に呼ばれる
        /// </summary>
        public delegate void DecideTeamDelegate();

		/************************************************************************/
		/* 公開処理                                                             */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public TeamSelectScene()
		{
			InitializeComponent();
            DragMemberControl.Visibility = Visibility.Hidden;

			//更新
			_Refresh();
		}

		/// <summary>
		/// 画面を覆うフィルターの見える横幅のサイズ
		/// </summary>
		/// <returns>フィルター幅</returns>
		public double GetDisplayFilterWidth() { return 20; }

		/// <summary>
		/// 別のシーンから戻って来た時に呼び出される処理
		/// </summary>
		public void SceneBack() 
		{
			BattleOperatorManager.GetInstance().SetLockTeam(false);
			BattleOperatorManager.GetInstance().ClearTeam();
			_Refresh();
		}

		/// <summary>
		/// チームの追加
		/// </summary>
		/// <param name="iTeamName"></param>
		public void AddTeam(string iTeamName)
		{
			var info = new STeamInfo();
			info.mTeamID = mTeamList.Count;
			info.mTeamName = iTeamName;
			info.mJoinIDList = new List<int>();
			info.mCloseFlg = false;
			mTeamList.Add(info);
		}

        /************************************************************************/
        /* アクセサ                                                             */
        /************************************************************************/

        /// <summary>
        /// チーム確定時に呼ばれるデリゲートの設定
        /// </summary>
        /// <param name="iCallback">コールバック</param>
        public void SetDecideTeamDelegate(DecideTeamDelegate iCallback) { mDecideTeamDelegate = iCallback; }

		/// <summary>
		/// チームの手動による作成削除が行えるかのフラグ設定
		/// </summary>
		/// <param name="iTeamOperateFlg">操作可能フラグ</param>
		public void SetTeamOperateFlg(bool iTeamOperateFlg)
		{
			mTeamOperateFlg = iTeamOperateFlg;
			_RefreshTeamList();
		}

		/************************************************************************/
		/* コールバック処理                                                     */
		/************************************************************************/

		//============================================================================
		//! 戻るボタンが押された時の処理
		private void _ClickReturn(object iSender, MouseButtonEventArgs iArgs)
		{
			SceneManager.GetInstance().ReturnBackScene();
		}

		//============================================================================
		//! その他ボタンが押された時の処理
		private void _ClickButton(object iSender, RoutedEventArgs iArgs)
		{
			//チェック
			var button = iSender as Button;
			if (button == null)
			{
				return;
			}

            //チームに0人のところがあればエラー
            foreach (var iTeam in mTeamList)
            {
                if (iTeam.mJoinIDList.Count == 0)
                {
                    SystemUtility.DisplayErrorDialog("所属が0人のチームがあります\n使用しない場合は削除を行ってください");
                    return;
                }
            }

            //チーム設定をして次へ
            var manager = BattleOperatorManager.GetInstance();
            foreach (var iTeam in mTeamList)
            {
                var info = new BattleManager.SBattleTeamInfo();
                info.mJoinIDList = new List<int>(iTeam.mJoinIDList.ToArray());
                info.mTeamName = iTeam.mTeamName;
                manager.AddTeam(info);
            }
            manager.SetLockTeam(true);
            mDecideTeamDelegate();
        }

        //============================================================================
		//! メンバーがクリックされた
        private void _ClickMember(object iSender, MouseButtonEventArgs iArgs)
        {
			var control = iSender as NoTeamMemberControl;
            if (control == null)
            {
                return;
            }

            //クリック開始のマウス位置を取得
            mDragOffset = iArgs.GetPosition(control);

            //表示
            mDragFlg = true;
            var info = (BattleManager.SBattleJoinInfo)control.Tag;
            DragMemberControl.Width = control.Width;
            DragMemberControl.CaptureMouse();
            DragMemberControl.Background = control.Background;
            DragMemberControl.Visibility = Visibility.Visible;
            DragMemberControl.SetJoinMemberInfo(info);
            DragMemberControl.TabIndex = info.mJoinID;       //< ここに参加者IDを入れておく
            _RefreshDragMemberPosition(iArgs.GetPosition(null));

            //クリックされたアイテムを非表示に
            control.Visibility = Visibility.Hidden;
        }

        //============================================================================
        //! マウスが画面外へ出た
        private void _MouseMoveOut(object iSender, MouseEventArgs iArgs)
        {
            _ClearDrag();
        }

        //============================================================================
        //! マウスの左ボタンが離された時に呼ばれる
        private void _MouseRelease(object iSender, MouseButtonEventArgs iArgs)
        {
            if (mDragFlg)
            {
                //HIT領域がある場合
                if (mRollInTeamIndex != -1)
                {
                    mTeamList[mRollInTeamIndex].mJoinIDList.Add(DragMemberControl.TabIndex);
                }

                _ClearDrag();
            }
        }

        //============================================================================
        //! マウスが移動された
        private void _MouseMove(object iSender, MouseEventArgs iArgs)
        {
            if (mDragFlg)
            {
                var mouse_Pos = iArgs.GetPosition(null);
                _RefreshDragMemberPosition(mouse_Pos);

                var collide_Index = -1;
                foreach (var iControl in TeamStackPanel.Children)
                {
                    var control = iControl as TeamControl;
                    if (control != null)
                    {
                        //範囲外か
                        var client_Pos = iArgs.GetPosition(control);
                        if (client_Pos.X < 0 || client_Pos.Y < 0 || client_Pos.X > control.Width || client_Pos.Y > control.Height)
                        {
                            continue;
                        }

                        collide_Index = control.TabIndex;
                        break;
                    }
                }

                //カーソル位置が違う
                if (mRollInTeamIndex != collide_Index)
                {
                    //前のものがある場合消去
                    if (mRollInTeamIndex != -1)
                    {
                        var control = TeamStackPanel.Children[mRollInTeamIndex] as TeamControl;
                        if (control != null)
                        {
                            control.SetFilterVisible(false);
                        }
                    }
                    
                    //現在
                    if (collide_Index != -1)
                    {
                        var control = TeamStackPanel.Children[collide_Index] as TeamControl;
                        if (control != null)
                        {
                            control.SetFilterVisible(true);
                        }
                    }

                    mRollInTeamIndex = collide_Index;
                }
            }
        }

        //============================================================================
        //! チーム追加ボタンが押された時の処理
        private void _ClickAddTeam(object iSender, RoutedEventArgs iArgs)
        {
            //無かった場合はゲームプリセットを選択させる
            var window = new TeamAddWindow();

			//チーム名の自動設定
			int useable_Team_Index = 1;
			string team_Name = "";
			while (true)
			{
				team_Name = string.Format("チーム{0}", useable_Team_Index);
				bool end_Flg = true;
				foreach (var iTeam in mTeamList)
				{
					if (iTeam.mTeamName == team_Name)
					{
						end_Flg = false;
						break;
					}
				}

				//終了チェック
				if (end_Flg)
				{
					break;
				}
				++useable_Team_Index;
			}
			window.TeamNameTextBox.Text = team_Name;

            //追加ボタンが押されたときの重複チェック
            window.AddButton.Click += (iClickSender, iClickArgs) =>
            {
                //既に名前がある場合は無効
				string name = window.TeamNameTextBox.Text;
                foreach (var iTeam in mTeamList)
                {
                    if (name.Length > 15)
                    {
                        iClickArgs.Handled = true;
                        SystemUtility.DisplayErrorDialog("チーム名は15文字以内で入力してください");
                        return;
                    }
                    else if (iTeam.mTeamName == name)
                    {
                        iClickArgs.Handled = true;
                        string txt = string.Format("既に「{0}」というチームは存在します\n別の名前にして下さい", name);
                        SystemUtility.DisplayErrorDialog(txt);
                        break;
                    }
                }
            };
            window.SettingCallback();

            //追加が押されて全ての条件を満たした時に追加
            window.AddButton.Click += (iClickSender, iClickArgs) =>
            {
                //追加
                AddTeam(window.TeamNameTextBox.Text);

                //メンバーリストの更新
                _RefreshTeamList();
				_RefreshButton();
                TeamScrollViewer.ScrollToEnd();
            };

            window.ShowDialog();
        }

        //============================================================================
        //! チームの編集ボタンが押された時の処理
        private void _ClickEditTeam(object iSender, RoutedEventArgs iArgs)
        {
            //チェック
            var button = iSender as Button;
            if (button == null)
            {
                return;
            }
			
            //ウィンドウ表示準備
            var window = new TeamAddWindow();
            window.Title = "チーム編集";
			var index = button.TabIndex;
            string current_Name = mTeamList[index].mTeamName;
			window.TeamNameTextBox.Text = current_Name;
            window.AddButton.Content = "決定";

            //追加ボタンが押されたときの重複チェック
            window.AddButton.Click += (iClickSender, iClickArgs) =>
            {
                //既に名前がある場合は無効
				string name = window.TeamNameTextBox.Text;
                foreach (var iInfo in MemberManager.GetInstance().GetMemberList())
                {
                    if (iInfo.mName == name)
                    {
                        //現在の自分の物の場合は無視
                        if (current_Name != name)
                        {
                            iClickArgs.Handled = true;
                            string txt = string.Format("既に「{0}」というチームは存在します\n別の名前にして下さい", name);
                            SystemUtility.DisplayErrorDialog(txt);
                            break;
                        }
                    }
                }
            };
            window.SettingCallback();
            //決定が押されて全ての条件を満たした時に追加
            window.AddButton.Click += (iClickSender, iClickArgs) =>
            {
				if (current_Name != window.TeamNameTextBox.Text)
                {
                    try
                    {
						var info = mTeamList[index];
						info.mTeamName = window.TeamNameTextBox.Text;
						mTeamList[index] = info;
						_Refresh();
                    }
                    catch (System.Exception)
                    {
                    }
                }
            };

            window.ShowDialog();
        }

        //============================================================================
        //! チーム削除ボタンが押された時の処理
        private void _ClickRemoveTeam(object iSender, RoutedEventArgs iArgs)
        {
            var button = iSender as Button;
            if (button == null)
            {
                return;
            }

            //確認
            System.Media.SystemSounds.Exclamation.Play();
			var result = MessageBox.Show(string.Format("チーム[{0}]を削除して宜しいですか？", mTeamList[button.TabIndex].mTeamName), "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
				mTeamList.RemoveAt(button.TabIndex);

                //IDつけなおし
                foreach (var iIndex in System.Linq.Enumerable.Range(0, mTeamList.Count))
                {
                    var info = mTeamList[iIndex];
                    info.mTeamID = iIndex;
                    mTeamList[iIndex] = info;
                }

                _Refresh();
            }
        }

		//============================================================================
        //! チーム情報開閉ボタンが押された時の処理
		private void _ClickTeamInfoOpenClose(object iSender, RoutedEventArgs iArgs)
		{
			var button = iSender as Button;
			if (button == null)
			{
				return;
			}
			var grid = button.Parent as Grid;
			if (grid == null)
			{
				return;
			}
			var team_Control = grid.Parent as TeamControl;
			if (team_Control == null)
			{
				return;
			}

			var info = mTeamList[team_Control.TabIndex];
			info.mCloseFlg = !info.mCloseFlg;
			mTeamList[team_Control.TabIndex] = info;
			team_Control.SetOpenClose(info.mCloseFlg);
			team_Control.RefreshControlSize();
		}

        //============================================================================
        //! チームに所属しているメンバーの解除ボタンが押された時の処理
        private void _ClickRemoveMember(object iSender, RoutedEventArgs iArgs)
        {
            var button = iSender as Button;
            if (button == null)
            {
                return;
            }

            //削除
            mTeamList[(int)button.Tag].mJoinIDList.RemoveAt(button.TabIndex);
            _Refresh();
        }

		//============================================================================
		//! 現在存在するチームに自動振り分けするボタンが押された
		private void _ClickDivideMember(object iSender, RoutedEventArgs iArgs)
		{
			//コントロールをランダムに並び替えて取得
			var control_List = new List<NoTeamMemberControl>();
			foreach (NoTeamMemberControl iControl in MemberStackPanel.Children)
			{
				control_List.Add(iControl);
			}
			var rnd_Control_List = control_List.ToArray().OrderBy(i => Guid.NewGuid()).ToArray();

			//並び替えたコントロールごとに実行
			foreach (var iControl in rnd_Control_List)
			{
				//一番人数が多いチームの人数を保持
				int member_Max_Val = 0;
				foreach (var iTeam in mTeamList)
				{
					if (member_Max_Val < iTeam.mJoinIDList.Count)
					{
						member_Max_Val = iTeam.mJoinIDList.Count;
					}
				}

				//ランダムで選択可能なチーム番号リストを作成
				var in_Team_List = new List<int>();
				foreach (var iTeam in mTeamList)
				{
					if (member_Max_Val > iTeam.mJoinIDList.Count)
					{
						in_Team_List.Add(iTeam.mTeamID);
					}
				}

				//1チームもない場合は全て同じ
				if(in_Team_List.Count == 0)
				{
					foreach (var iIndex in System.Linq.Enumerable.Range(0, mTeamList.Count))
					{
						in_Team_List.Add(iIndex);
					}
				}

				//リスト内から抽選
				var rand = new byte[4];
				var rng = new RNGCryptoServiceProvider();
				rng.GetBytes(rand);
				var select_Index = Math.Abs(System.BitConverter.ToInt32(rand, 0)) % in_Team_List.Count;
				var select_Team_Index = in_Team_List[select_Index];

				//参加メンバーを上記で抽選したチームに居れる
				mTeamList[select_Team_Index].mJoinIDList.Add(((BattleManager.SBattleJoinInfo)iControl.Tag).mJoinID);
			}

			//更新
			_Refresh();
		}

		//============================================================================
		//! 全てのメンバーを指定した人数が埋まる分だけチームを生成して振り分ける
		private void _ClickRandom(object iSender, RoutedEventArgs iArgs)
		{
			var window = new TeamMemberValSettingWindow();

			//追加ボタンが押されたときの重複チェック
			window.DecideButton.Click += (iClickSender, iClickArgs) =>
			{
				//OKボタンの時は内部チェック
				if (((Button)iClickSender).TabIndex == 0)
				{
					//テキストボックスチェック
					if (window.TeamValueTextBox.Text.Length == 0)
					{
						SystemUtility.DisplayErrorDialog("人数を入力してください");
						return;
					}

					//数値チェック
					int val = 0;
					try
					{
						val = int.Parse(window.TeamValueTextBox.Text);
						
						//負の数チェック
						if (val < 2)
						{
							SystemUtility.DisplayErrorDialog("１人以下は設定出来ません");
							return;
						}

						//ぴったりでない場合の確認
						var member_Val = BattleOperatorManager.GetInstance().GetJoinMemberVal();
						var team_Val = member_Val / int.Parse(window.TeamValueTextBox.Text);
						if ((member_Val % val) != 0)
						{
							if (MessageBox.Show("端数が出るチームが出ますが宜しいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
							{
								return;
							}
							++team_Val;
						}

						//追加して更新
						window.Close();
						mTeamList.Clear();
						foreach (var iIndex in System.Linq.Enumerable.Range(0, team_Val))
						{
							AddTeam(string.Format("チーム{0}", iIndex + 1));
						}
						_ClickDivideMember(iSender, iArgs);
						TeamScrollViewer.ScrollToEnd();
					}
					catch
					{
						SystemUtility.DisplayErrorDialog("数値のみ入力できます");
					}
				}
			};
			window.ShowDialog();
		}

		//============================================================================
		//! 全チーム削除ボタンが押された時の処理
		private void _ClickRemoveAllTeam(object iSender, RoutedEventArgs iArgs)
		{
			//確認
			System.Media.SystemSounds.Exclamation.Play();
			var result = MessageBox.Show("全てのチームを削除して宜しいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (result == MessageBoxResult.Yes)
			{
				mTeamList.Clear();
				_Refresh();
			}
		}

		//============================================================================
		//! メンバー保存系ボタンが押された時に呼ばれる
		private void _ClickSaveLoadButton(object iSender, RoutedEventArgs iArgs)
		{
			try
			{
				var button = iSender as Button;
				if (button != null)
				{
					//保存
					if (button.TabIndex == 0)
					{
						//作成チームが無ければエラー
						if (mTeamList.Count == 0)
						{
							System.Media.SystemSounds.Hand.Play();
							MessageBox.Show("作成メンバーが一つもありません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
							return;
						}

						var dialog = new SaveFileDialog();
						dialog.Filter = "チーム構成ファイル(*.tsb)|*.tsb|すべてのファイル(*.*)|*.*";
						dialog.Title = "チーム構成ファイルを選択してください";
						dialog.RestoreDirectory = true;
						dialog.InitialDirectory = SystemUtility.GetSystemPath();
						if (dialog.ShowDialog() == true)
						{
							_WriteTeamData(dialog.FileName);
						}
					}
					//読み込み
					else
					{
						//参加メンバーが既にいれば確認
						if (mTeamList.Count != 0)
						{
							System.Media.SystemSounds.Asterisk.Play();
							var result = MessageBox.Show("既にチームが登録されていますが\n上書きしても宜しいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
							if (result != MessageBoxResult.Yes)
							{
								return;
							}
						}

						var dialog = new OpenFileDialog();
						dialog.Filter = "チーム構成ファイル(*.tsb)|*.tsb|すべてのファイル(*.*)|*.*";
						dialog.Title = "チーム構成ファイルを選択してください";
						dialog.RestoreDirectory = true;
						dialog.InitialDirectory = SystemUtility.GetSystemPath();
						if (dialog.ShowDialog() == true)
						{
							_ReadTeamData(dialog.FileName);
						}
					}
				}
			}
			catch (System.Exception)
			{

			}
		}

		/************************************************************************/
		/* 内部処理                                                             */
		/************************************************************************/

		//============================================================================
		//! 画面の更新
        private void _Refresh()
		{
			_RefreshJoinMemberList();
			_RefreshTeamList();
			_RefreshButton();
		}

		//============================================================================
		//! 選択していないメンバーリストの更新
        private void _RefreshJoinMemberList()
		{
			MemberStackPanel.Children.Clear();
			var set_Width = MemberGrid.Width - 44;
			int color_Index = 0;
			var color_List = new Brush[2] { new SolidColorBrush(SystemUtility.StringToColor("#55000000")), new SolidColorBrush(SystemUtility.StringToColor("#55555555")) };

			//作成
			foreach (var iMember in BattleOperatorManager.GetInstance().GetJoinMemberList())
			{
                if (!_IsInTeamJoinMember(iMember.mJoinID))
				{
					var control = new NoTeamMemberControl();
					control.Width = set_Width;
					control.Background = color_List[color_Index % 2];
					control.SetJoinMemberInfo(iMember);
                    control.OnSelectionFilter();
                    control.PreviewMouseLeftButtonDown += _ClickMember;
                    control.Tag = iMember;
                    MemberStackPanel.Children.Add(control);
					++color_Index;
				}
			}			
		}

		//============================================================================
		//! チームリストの更新
        private void _RefreshTeamList()
		{
			TeamStackPanel.Children.Clear();
			var set_Width = TeamGrid.Width - 44;
			var color_Index = 0;
			var color_List = new Brush[2] { new SolidColorBrush(SystemUtility.StringToColor("#55000000")), new SolidColorBrush(SystemUtility.StringToColor("#55555555")) };
			foreach (var iTeam in mTeamList)
			{
				var control = new TeamControl();
				control.Background = color_List[color_Index % 2];
				control.SetTeamInfo(iTeam.mTeamID, iTeam.mTeamName);
				control.RemoveButton.IsEnabled = mTeamOperateFlg;
                control.RemoveButton.Click += _ClickRemoveTeam;
				control.RemoveButton.TabIndex = iTeam.mTeamID;
				control.EditButton.IsEnabled = mTeamOperateFlg;
				control.EditButton.Click += _ClickEditTeam;
				control.EditButton.TabIndex = iTeam.mTeamID;
				control.OpenCloseButton.Click += _ClickTeamInfoOpenClose;
                control.TabIndex = iTeam.mTeamID;
                control.SetOpenClose(iTeam.mCloseFlg);

                //チームに登録されているメンバーを設定
                int join_Index = 0;
                foreach (var iJoinMemberID in iTeam.mJoinIDList)
                {
                    //戻ってきたメンバーごとのコントロールの削除ボタンの設定
                    var in_Control = control.AddJoinMember(iJoinMemberID);
                    in_Control.RemoveButton.Tag = iTeam.mTeamID;
                    in_Control.RemoveButton.TabIndex = join_Index;
                    in_Control.RemoveButton.Click += _ClickRemoveMember;
                    ++join_Index;
                }
                control.RefreshControlSize();

                //チームスタックに挿入
                TeamStackPanel.Children.Add(control);
				++color_Index;
			}

            //追加ボタンを追加
			if (mTeamOperateFlg)
			{
				var control = new TeamAddControl();
				control.AddButton.Click += _ClickAddTeam;
				control.Width = set_Width;
				TeamStackPanel.Children.Add(control);
			}
		}

		//============================================================================
		//! ボタンの表示更新
		private void _RefreshButton()
		{
			DivideTeamButton.IsEnabled = (MemberStackPanel.Children.Count > 0 && mTeamList.Count > 1);
			RandomTeamButton.IsEnabled = (mTeamList.Count == 0);
			TeamAllRemoveButton.IsEnabled = (RandomTeamButton.IsEnabled == false);
			FinishButton.IsEnabled = (MemberStackPanel.Children.Count == 0);
		}

        //============================================================================
        //! 指定した参加メンバーIDはチームに配属済みか
        private bool _IsInTeamJoinMember(int iJoinMemberID)
        {
            foreach (var iTeam in mTeamList)
            {
                foreach (var iID in iTeam.mJoinIDList)
                {
                    if (iJoinMemberID == iID)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        //============================================================================
        //! ドラッグされているコントロールの位置更新
        private void _RefreshDragMemberPosition(Point iCursorPosition)
        {
            var margin = DragMemberControl.Margin;
            margin.Left = iCursorPosition.X - mDragOffset.X;
            margin.Top = iCursorPosition.Y - mDragOffset.Y;
            DragMemberControl.Margin = margin;
        }

        //============================================================================
        //! ドラッグ状態の解除
        private void _ClearDrag()
        {
            if (mDragFlg)
            {
                DragMemberControl.Visibility = Visibility.Hidden;
                DragMemberControl.ReleaseMouseCapture();
                mDragFlg = false;
                mRollInTeamIndex = -1;
                _Refresh();
            }
        }

		//============================================================================
		//! メンバー状態を保存
		private void _WriteTeamData(string iFilePath)
		{
			var root = new DL.CXmlNode("TeamList");

			foreach (var iTeamInfo in mTeamList)
			{
				var team = new DL.CXmlNode("Team");
				team.AddChildNode(new DL.CXmlNode("TeamID", iTeamInfo.mTeamID.ToString()));
				team.AddChildNode(new DL.CXmlNode("TeamName", iTeamInfo.mTeamName));

				var member = new DL.CXmlNode("MemberList");
				foreach (var iMemberID in iTeamInfo.mJoinIDList)
				{
					member.AddChildNode(new DL.CXmlNode("ID", iMemberID.ToString()));
				}
				team.AddChildNode(member);

				team.AddChildNode(new DL.CXmlNode("CloseFlg", iTeamInfo.mCloseFlg ? "1" : "0"));
				root.AddChildNode(team);
			}

			//保存
			var save_node = new DL.CXmlNode("__SYS_ROOT__");
			save_node.AddChildNode(root);
			save_node.WriteXmlFile(iFilePath);
		}

		//============================================================================
		//! メンバー状態を読み込み
		private void _ReadTeamData(string iFilePath)
		{
			try
			{
				var analayzer = new DL.CXmlAnalyzer();
				analayzer.AnalyzeXmlFile(iFilePath);
				var node = analayzer.GetRootNode().GetChildNodeFromPath("TeamList")[0];

				mTeamList.Clear();
				foreach (var iNode in node.GetChildNode())
				{
					var info = new STeamInfo();
					info.mTeamID = int.Parse(iNode.GetChildNode("TeamID")[0].GetNodeInfo().mValue);
					info.mTeamName = iNode.GetChildNode("TeamName")[0].GetNodeInfo().mValue;
					info.mJoinIDList = new List<int>();
					foreach (var iMemberNode in iNode.GetChildNode("MemberList")[0].GetChildNode())
					{
						info.mJoinIDList.Add(int.Parse(iMemberNode.GetNodeInfo().mValue));
					}
					info.mCloseFlg = (iNode.GetChildNode("CloseFlg")[0].GetNodeInfo().mValue == "1");

					mTeamList.Add(info);
				}
				_Refresh();
			}
			catch (System.Exception)
			{
				mTeamList.Clear();
			}
		}

		/************************************************************************/
		/* 内部定義                                                             */
		/************************************************************************/

		/// <summary>
		/// チーム情報
		/// </summary>
		private struct STeamInfo
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

            /// <summary>
            /// メンバー情報が閉まっているか
            /// </summary>
            public bool mCloseFlg;
		};

		/************************************************************************/
		/* 変数定義                                                             */
		/************************************************************************/

		/// <summary>
		/// 参加者情報リスト
		/// </summary>
        private List<STeamInfo> mTeamList = new List<STeamInfo>();

        /// <summary>
        /// 参加メンバー確定時に呼ばれるデリゲート
        /// </summary>
        private DecideTeamDelegate mDecideTeamDelegate;

        /// <summary>
        /// 現在がドラッグ中か
        /// </summary>
        private bool mDragFlg = false;

        /// <summary>
        /// ドラッグアイテムの表示位置オフセット
        /// </summary>
        private Point mDragOffset;

        /// <summary>
        /// カーソルがチームのどこに入っているか
        /// </summary>
        private int mRollInTeamIndex = -1;

		/// <summary>
		/// チームの操作が行えるかのフラグ
		/// </summary>
		private bool mTeamOperateFlg = true;
	}
}
