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

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// MemberControl.xaml の相互作用ロジック
	/// </summary>
	public partial class MemberScene : UserControl,SceneInterface
	{
		/************************************************************************/
		/* 公開処理                                                             */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public MemberScene()
		{
			InitializeComponent();
            BattleOperatorManager.GetInstance().SetLockJoinMember(false);

            var get_Obj = DataManager.GetInstance().GetData(SystemUtility.GetSceneName(this));
            if (get_Obj != null)
            {
                var info = (SSerialData)get_Obj;
                SortMemberComboBox.SelectedIndex = info.mSortKind;
                if(info.mDisplayKind == 0)
                {
                    NormalDisplayRadioButton.IsChecked = true;
                }
                else
                {
                    MinDisplayRadioButton.IsChecked = true;
                }
            }

            //メンバーリスト更新
            _RefreshMemberList();
    	}

		/// <summary>
		/// デストラクタ
		/// </summary>
		~MemberScene()
		{
		}

		/// <summary>
		/// 画面を覆うフィルターの見える横幅のサイズ
		/// </summary>
		/// <returns>フィルター幅</returns>
		public double GetDisplayFilterWidth() { return 70; }

		/// <summary>
		/// 別のシーンから戻って来た時に呼び出される処理
		/// </summary>
		public void SceneBack() { }

		/************************************************************************/
		/* コールバック処理                                                     */
		/************************************************************************/

		//============================================================================
		//! ソート形式が変更された時に呼ばれる
		private void _ChangeSort(object iSender, SelectionChangedEventArgs iArgs)
		{
            _RefreshMemberList();
			if (MemberScrollViewer != null)
			{
				MemberScrollViewer.ScrollToTop();
			}
		}

        //============================================================================
        //! 表示形式が変更された
        private void _ChangeDisplayMode(object iSender, RoutedEventArgs iArgs)
        {
            _RefreshMemberList();
        }

		//============================================================================
		//! 戻るボタンが押された時の処理
		private void _ClickReturn(object iSender, MouseButtonEventArgs iArgs)
		{
            //情報保存
            var info = new SSerialData();
            info.mSortKind = SortMemberComboBox.SelectedIndex;
            info.mDisplayKind = (NormalDisplayRadioButton.IsChecked == true) ? 0 : 1;
            DataManager.GetInstance().SetData(SystemUtility.GetSceneName(this), info);

			//プロジェクトの保存チェック
			SaveManager.GetInstance().AutoSaveProject();
			SceneManager.GetInstance().ReturnBackScene();
		}

		//============================================================================
		//! メンバーの追加ボタンが押された時の処理
		private void _ClickAddMember(object iSender, RoutedEventArgs iArgs)
		{
			//無かった場合はゲームプリセットを選択させる
			var window = new MemberAddWindow();

			//追加ボタンが押されたときの重複チェック
			window.AddButton.Click += (iClickSender, iClickArgs) =>
			{
				//既に名前がある場合は無効
				string name = window.MemberNameTextBox.Text;
				foreach (var iInfo in MemberManager.GetInstance().GetMemberList())
				{
					if (name.Length > 14)
					{
						iClickArgs.Handled = true;
						SystemUtility.DisplayErrorDialog("メンバー名は14文字以内で入力してください");
						return;
					}
					else if (iInfo.mName == name)
					{
						iClickArgs.Handled = true;
						string txt = string.Format("既に「{0}」という名前は存在します\n別の名前にして下さい", name);
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
				var info = new MemberManager.SMemberInfo();
				info.mName = window.MemberNameTextBox.Text;
				info.mDefaultCharacterID = window.GetSelectCharacterIndex();
				MemberManager.GetInstance().AddMember(info);

				//メンバーリストの更新
				_RefreshMemberList();
				MemberScrollViewer.ScrollToEnd();
			};

			window.ShowDialog();
		}

		//============================================================================
		//! メンバーの編集ボタンが押された時の処理
		private void _ClickEditMember(object iSender, RoutedEventArgs iArgs)
		{
			//チェック
			var button = iSender as Button;
			if (button == null)
			{
				return;
			}

			//押されたボタンのメンバー情報取得
			int id = button.TabIndex;
			var member_Info = MemberManager.GetInstance().GetMemberInfo(id);

			//選択されているセルが無かった場合は無視
			var window = new MemberAddWindow();
            window.Title = "メンバー編集";
			int index = 1;
			foreach (var iInfo in PresetManager.GetInstance().GetCharacterList())
			{
				if (iInfo.mIndex == member_Info.mDefaultCharacterID)
				{
					window.CharacterComboBox.SelectedIndex = index;
					break;
				}
				++index;
			}
	
			string current_Name = member_Info.mName;
			window.MemberNameTextBox.Text = current_Name;
			window.AddButton.Content = "決定";

			//追加ボタンが押されたときの重複チェック
			window.AddButton.Click += (iClickSender, iClickArgs) =>
			{
				//既に名前がある場合は無効
				string name = window.MemberNameTextBox.Text;
				foreach (var iInfo in MemberManager.GetInstance().GetMemberList())
				{
					if (iInfo.mName == name)
					{
						//現在の自分の物の場合は無視
						if (current_Name != name)
						{
							iClickArgs.Handled = true;
							string txt = string.Format("既に「{0}」という名前は存在します\n別の名前にして下さい", name);
							SystemUtility.DisplayErrorDialog(txt);
							break;
						}
					}
				}
			};
			window.SettingCallback();
			//追加が押されて全ての条件を満たした時に追加
			window.AddButton.Click += (iClickSender, iClickArgs) =>
			{
				if (current_Name != window.MemberNameTextBox.Text ||
					member_Info.mDefaultCharacterID != window.GetSelectCharacterIndex())
				{
					try
					{
						member_Info.mName = window.MemberNameTextBox.Text;
						member_Info.mDefaultCharacterID = window.GetSelectCharacterIndex();
						MemberManager.GetInstance().SetMemberInfo(id, member_Info);
						_RefreshMemberList();
					}
					catch (System.Exception)
					{
					}
				}
			};

			window.ShowDialog();
		}

		/************************************************************************/
		/* 内部処理                                                             */
		/************************************************************************/

		//============================================================================
		//! メンバーリストの更新
		private void _RefreshMemberList()
		{
            //まだ無視
            if (NormalDisplayRadioButton == null)
            {
                return;
            }

			//メンバーリスト作成
			var member_List = MemberManager.GetInstance().GetMemberList((MemberManager.ESortKind)SortMemberComboBox.SelectedIndex);

			//現在のメンバー全てを設定
			MemberStackPanel.Children.Clear();
			var set_Width = MemberScrollViewer.Width;
			int index = 0;
			var color_List = new Brush[2] { new SolidColorBrush(SystemUtility.StringToColor("#55000000")), new SolidColorBrush(SystemUtility.StringToColor("#55555555")) };

            //通常表示
            if (NormalDisplayRadioButton.IsChecked == true)
            {
                foreach (var iMember in member_List)
                {
                    var control = new MemberListControl();
                    control.Width = set_Width;
                    control.Background = color_List[index % 2];
                    control.EditButton.TabIndex = iMember.mID;
                    control.EditButton.Click += _ClickEditMember;
                    control.SetMemberInfo(iMember);
                    MemberStackPanel.Children.Add(control);

                    ++index;
                }
            }
            else
            {
                foreach (var iMember in member_List)
                {
                    var control = new MemberMinListControl();
                    control.Width = set_Width;
                    control.Background = color_List[index % 2];
                    control.EditButton.TabIndex = iMember.mID;
                    control.EditButton.Click += _ClickEditMember;
                    control.SetMemberInfo(iMember);
                    MemberStackPanel.Children.Add(control);

                    ++index;
                }
            }

			//追加コントロール
			var add_Contrl = new MemberAddControl();
			add_Contrl.AddButton.Click += _ClickAddMember;
			MemberStackPanel.Children.Add(add_Contrl);
		}

        /************************************************************************/
        /* 内部定義                                                             */
        /************************************************************************/

        /// <summary>
        /// 書き出し情報
        /// </summary>
        [Serializable]
        private struct SSerialData
        {
            /// <summary>
            /// 表示順番種別
            /// </summary>
            public int mSortKind;

            /// <summary>
            /// 表示サイズ
            /// </summary>
            public int mDisplayKind;
        };
	}
}
