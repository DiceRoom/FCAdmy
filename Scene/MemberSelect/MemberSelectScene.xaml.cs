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
using Microsoft.Win32;
using WindowsForm = System.Windows.Forms;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// MemberControl.xaml の相互作用ロジック
	/// </summary>
	public partial class MemberSelectScene : UserControl,SceneInterface
	{
        /************************************************************************/
        /* 公開処理                                                             */
        /************************************************************************/

        /// <summary>
        /// メンバー確定時に呼ばれる
        /// </summary>
        public delegate void DecideMemberDelegate();

		/************************************************************************/
		/* 公開処理                                                             */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public MemberSelectScene()
		{
			InitializeComponent();

			//初期設定
			SystemManager.GetInstance().SetInputKeyDelegate(_InputKey);
			SystemManager.GetInstance().SetCloseCheckCallback(_CheckClose);

			//更新
			_RefreshMemberList();
		}

		/// <summary>
		/// デストラクタ
		/// </summary>
		~MemberSelectScene()
		{
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
			BattleOperatorManager.GetInstance().SetLockJoinMember(false);
			BattleOperatorManager.GetInstance().ClearJoinMember();
			_RefreshMemberList();
		}

		/// <summary>
		/// オプションウィンドウの表示
		/// </summary>
		public void OpenSearchWindow()
		{
			mSearchWindow = new MemberSearchWindow();
			mSearchWindow.SetChangeSearchCallback(_ChangeSearch);
			mSearchWindow.Closing += (iSender, iArgs) =>
			{
				mSearchWindow = null;
				_RefreshNonSelectMemberList();
			};
			mSearchWindow.Show();
		}

		/// <summary>
		/// オプションウィンドウが表示中か
		/// </summary>
		/// <returns></returns>
		public bool IsOpenSearchWindow()
		{
			return (mSearchWindow != null);
		}

		/// <summary>
		/// オプションウィンドウの非表示
		/// </summary>
		public void CloseSearchWindow()
		{
			if (mSearchWindow != null)
			{
				mSearchWindow.Close();
				mSearchWindow = null;
			}
		}

        /************************************************************************/
        /* アクセサ                                                             */
        /************************************************************************/

        /// <summary>
        /// 参加メンバー確定時に呼ばれるデリゲートの設定
        /// </summary>
        /// <param name="iCallback">コールバック</param>
        public void SetDecideMemberDelegate(DecideMemberDelegate iCallback) { mDecideMemberDelegate = iCallback; }

		/************************************************************************/
		/* コールバック処理                                                     */
		/************************************************************************/

		//============================================================================
		//! キーが押された時に呼ばれる
		private void _InputKey(KeyEventArgs iArgs)
		{
			//Ctrl+F又はF3が押された時に検索ウィンドウの開閉状態を入れ替える
			if (((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None && iArgs.Key == Key.F) ||
				iArgs.Key == Key.F3)
			{
				if (IsOpenSearchWindow())
				{
					CloseSearchWindow();
				}
				else
				{
					OpenSearchWindow();
				}
			}
		}

		//============================================================================
		//! ウィンドウを閉じていいかどうかの確認
		private bool _CheckClose()
		{
			if (IsOpenSearchWindow())
			{
				CloseSearchWindow();
			}
			return true;
		}

		//============================================================================
		//! 戻るボタンが押された時の処理
		private void _ClickReturn(object iSender, RoutedEventArgs iArgs)
		{
			if (IsOpenSearchWindow())
			{
				CloseSearchWindow();
			}
			SceneManager.GetInstance().ReturnBackScene();
		}

		//============================================================================
		//! 参加ボタンが押された時の処理
		private void _ClickJoin(object iSender, RoutedEventArgs iArgs)
		{
			//チェック
			var button = iSender as Button;
			if (button == null)
			{
				return;
			}

			//参加者の追加
			var member_Info = (MemberManager.SMemberInfo)button.Tag;
			var info = new SJoinMemberInfo();
			info.mJoinID = mJoinMemberList.Count;
			info.mMemberID = member_Info.mID;
			info.mUseCharacterID = member_Info.mDefaultCharacterID;
			mJoinMemberList.Add(info);
			_RefreshMemberList();

			SelectMemberScrollViewer.ScrollToEnd();
		}

		//============================================================================
		//! 取消ボタンが押された時の処理
		private void _ClickJoinRemove(object iSender, RoutedEventArgs iArgs)
		{
			//チェック
			var button = iSender as Button;
			if (button == null)
			{
				return;
			}

			//参加者の追加
			var info = (SJoinMemberInfo)button.Tag;
			if (info.mJoinID < mJoinMemberList.Count)
			{
				mJoinMemberList.RemoveAt(info.mJoinID);

				//参加者IDのつけ直し
				foreach (var iIndex in System.Linq.Enumerable.Range(0, mJoinMemberList.Count))
				{
					var set_Info = mJoinMemberList[iIndex];
					set_Info.mJoinID = iIndex;
					button.Tag = set_Info;
					mJoinMemberList[iIndex] = set_Info;
				}

				_RefreshMemberList();
			}
		}

		//============================================================================
		//! ソート形式が変更された時に呼ばれる
		private void _ChangeSort(object iSender, SelectionChangedEventArgs iArgs)
		{
			_RefreshNonSelectMemberList();
			if (NonSelectMemberScrollViewer != null)
			{
				NonSelectMemberScrollViewer.ScrollToTop();
			}			
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

			//メンバーの確定
			if (button.TabIndex != 0)
			{
                if (mDecideMemberDelegate != null)
                {
                    //メンバー確定
                    var manager = BattleOperatorManager.GetInstance();
                    foreach (var iJoinMember in mJoinMemberList)
                    {
                        manager.AddMember(iJoinMember.mMemberID, iJoinMember.mUseCharacterID);
                    }
                    manager.SetLockJoinMember(true);
                    mDecideMemberDelegate();

					if (IsOpenSearchWindow())
					{
						CloseSearchWindow();
					}
                }
			}
			//メンバー管理画面へ
			else
			{
				SceneManager.GetInstance().AdvanceScene(SceneManager.EScene.SCENE_MEMBER);
			}
		}

		//============================================================================
		//! 参加メンバーが押された時の処理
		private void _ClickJoinMember(object iSender, MouseButtonEventArgs iArgs)
		{
			var rect = iSender as Rectangle;
			if (rect == null)
			{
				return;
			}
            var grid = rect.Parent as Grid;
            if (grid == null)
            {
                return;
            }
            var control = grid.Parent as SelectMemberControl;
            if (control == null)
            {
                return;
            }

			//選択
			var wnd = new CharacterSelectWindow();
			wnd.Closing += (iCloseSender, iCloseArgs) =>
			{
				var character_ID = wnd.GetSelectCharacterID();
				if (character_ID != -1)
				{
					control.SetCharacter(character_ID);

					//情報切り替え
					var index = control.TabIndex;
					var info = mJoinMemberList[index];
					info.mUseCharacterID = character_ID;
					mJoinMemberList[index] = info;
				}
			};
			wnd.ShowDialog();
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
						//参加メンバーが居なければエラー
						if (mJoinMemberList.Count == 0)
						{
							System.Media.SystemSounds.Hand.Play();
							MessageBox.Show("参加メンバーが一人もいません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
							return;
						}

						var dialog = new SaveFileDialog();
						dialog.Filter = "メンバー構成ファイル(*.msb)|*.msb|すべてのファイル(*.*)|*.*";
						dialog.Title = "メンバー構成ファイルを選択してください";
						dialog.RestoreDirectory = true;
						dialog.InitialDirectory = SystemUtility.GetSystemPath();
						if (dialog.ShowDialog() == true)
						{
							_WriteMemberData(dialog.FileName);
						}
					}
					//読み込み
					else
					{
						//参加メンバーが既にいれば確認
						if (mJoinMemberList.Count != 0)
						{
							System.Media.SystemSounds.Asterisk.Play();
							var result = MessageBox.Show("既に参加者が登録されていますが\n上書きしても宜しいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
							if (result != MessageBoxResult.Yes)
							{
								return;
							}
						}

						var dialog = new OpenFileDialog();
						dialog.Filter = "メンバー構成ファイル(*.msb)|*.msb|すべてのファイル(*.*)|*.*";
						dialog.Title = "メンバー構成ファイルを選択してください";
						dialog.RestoreDirectory = true;
						dialog.InitialDirectory = SystemUtility.GetSystemPath();
						if (dialog.ShowDialog() == true)
						{
							_ReadMemberData(dialog.FileName);
						}
					}
				}
			}
			catch (System.Exception)
			{
				
			}
		}

		//============================================================================
		//! 検索状態が変更されたら呼ばれる
		private void _ChangeSearch(string iSearchMemberName, int iSearchCharacterID)
		{
			_RefreshNonSelectMemberList();
		}

		/************************************************************************/
		/* 内部処理                                                             */
		/************************************************************************/

		//============================================================================
		//! メンバーリストの更新
		private void _RefreshMemberList()
		{
			_RefreshNonSelectMemberList();
			_RefreshSelectMemberList();
			JoinMemberNumTextBlock.Text = string.Format("現在の参加人数は {0}人 です" , mJoinMemberList.Count);
			FinishButton.IsEnabled = (mJoinMemberList.Count > 2);
		}

		//============================================================================
		//! 選択していないメンバーリストの更新
		private void _RefreshNonSelectMemberList()
		{
			NonSelectMemberStackPanel.Children.Clear();
			var set_Width = NonSelectMemberScrollViewer.Width;
			int color_Index = 0;
			var color_List = new Brush[2] { new SolidColorBrush(SystemUtility.StringToColor("#55000000")), new SolidColorBrush(SystemUtility.StringToColor("#55555555")) };

			//メンバーリスト作成
			var member_List = MemberManager.GetInstance().GetMemberList((MemberManager.ESortKind)SortMemberComboBox.SelectedIndex);

			//検索情報
			string search_member_name = "";
			int search_character_id = -1;
			if (IsOpenSearchWindow())
			{
				search_member_name = mSearchWindow.GetSearchMemberName();
				search_character_id = mSearchWindow.GetSearchCharacterID();
			}

			//作成
			foreach (var iMember in member_List)
			{
				if (!_IsJoinMember(iMember.mID))
				{
					//サーチチェック
					if (search_member_name.Length > 0 && iMember.mName.IndexOf(search_member_name) == -1)
					{
						continue;
					}
					if (search_character_id != -1 && search_character_id != iMember.mDefaultCharacterID)
					{
						continue;
					}

					var control = new NonSelectMemberControl();
					control.Width = set_Width;
					control.EntryButton.Tag = iMember;
					control.EntryButton.Click += _ClickJoin;
					control.Background = color_List[color_Index % 2];
					control.SetMemberInfo(iMember);
					NonSelectMemberStackPanel.Children.Add(control);
					++color_Index;
				}
			}
		}

		//============================================================================
		//! 選択済みのメンバーリストの更新
		private void _RefreshSelectMemberList()
		{
			SelectMemberStackPanel.Children.Clear();
			var set_Width = SelectMemberScrollViewer.Width;
			var member_Manager = MemberManager.GetInstance();
			var color_Index = 0;
			var color_List = new Brush[2] { new SolidColorBrush(SystemUtility.StringToColor("#55000000")), new SolidColorBrush(SystemUtility.StringToColor("#55555555")) };
			foreach (var iJoinMember in mJoinMemberList)
			{
				var control = new SelectMemberControl();
				control.Width = set_Width;
				control.RemoveEntryButton.Tag = iJoinMember;
				control.RemoveEntryButton.Click += _ClickJoinRemove;
				control.CollideRectangle.PreviewMouseLeftButtonDown += _ClickJoinMember;
				control.Background = color_List[color_Index % 2];
				control.SetMemberInfo(iJoinMember.mJoinID, member_Manager.GetMemberInfo(iJoinMember.mMemberID).mName, iJoinMember.mUseCharacterID);
				control.TabIndex = SelectMemberStackPanel.Children.Count;
				SelectMemberStackPanel.Children.Add(control);
				++color_Index;
			}
		}

		//============================================================================
		//! 指定したメンバーIDのメンバーが参加中か
		private bool _IsJoinMember(int iMemberID)
		{
			foreach (var iJoinMember in mJoinMemberList)
			{
				if (iJoinMember.mMemberID == iMemberID)
				{
					return true;
				}
			}

			return false;
		}

		//============================================================================
		//! メンバー状態を保存
		private void _WriteMemberData(string iFilePath)
		{
			var root = new DL.CXmlNode("MemberList");

			foreach (var iJoinInfo in mJoinMemberList)
			{
				var member = new DL.CXmlNode("Member");
				member.AddChildNode(new DL.CXmlNode("MemberID", iJoinInfo.mMemberID.ToString()));
				member.AddChildNode(new DL.CXmlNode("JoinID", iJoinInfo.mJoinID.ToString()));
				member.AddChildNode(new DL.CXmlNode("CharacterID", iJoinInfo.mUseCharacterID.ToString()));
				root.AddChildNode(member);
			}

			//保存
			var save_node = new DL.CXmlNode("__SYS_ROOT__");
			save_node.AddChildNode(root);
			save_node.WriteXmlFile(iFilePath);
		}

		//============================================================================
		//! メンバー状態を読み込み
		private void _ReadMemberData(string iFilePath)
		{
			try
			{
				var analayzer = new DL.CXmlAnalyzer();
				analayzer.AnalyzeXmlFile(iFilePath);
				var node = analayzer.GetRootNode().GetChildNodeFromPath("MemberList")[0];

				mJoinMemberList.Clear();
				foreach (var iNode in node.GetChildNode())
				{
					var info = new SJoinMemberInfo();
					info.mMemberID = int.Parse(iNode.GetChildNode("MemberID")[0].GetNodeInfo().mValue);
					info.mJoinID = int.Parse(iNode.GetChildNode("JoinID")[0].GetNodeInfo().mValue);
					info.mUseCharacterID = int.Parse(iNode.GetChildNode("CharacterID")[0].GetNodeInfo().mValue);
					mJoinMemberList.Add(info);
				}
				_RefreshMemberList();
			}
			catch (System.Exception)
			{
				mJoinMemberList.Clear();
			}
		}

		/************************************************************************/
		/* 内部定義                                                             */
		/************************************************************************/

		/// <summary>
		/// 大会参加メンバー情報
		/// </summary>
		private struct SJoinMemberInfo
		{
			/// <summary>
			/// 参加者番号
			/// </summary>
			public int mJoinID;

			/// <summary>
			/// メンバーID
			/// </summary>
			public int mMemberID;

			/// <summary>
			/// 使用キャラクター番号
			/// </summary>
			public int mUseCharacterID;
		};

		/************************************************************************/
		/* 変数定義                                                             */
		/************************************************************************/

		/// <summary>
		/// 参加者情報リスト
		/// </summary>
		private List<SJoinMemberInfo> mJoinMemberList = new List<SJoinMemberInfo>();

        /// <summary>
        /// 参加メンバー確定時に呼ばれるデリゲート
        /// </summary>
        private DecideMemberDelegate mDecideMemberDelegate;

		/// <summary>
		/// 検索ウィンドウ
		/// </summary>
		private MemberSearchWindow mSearchWindow = null;
	}
}
