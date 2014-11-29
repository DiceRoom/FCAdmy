using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// TitleContorl.xaml の相互作用ロジック
	/// </summary>
	public partial class SettingsScene : UserControl, SceneInterface
	{
		/// <summary>
		/// コンストラクタ
		/// </summary>
        public SettingsScene()
		{
			InitializeComponent();

			//更新
			_Refresh();
		}

        /************************************************************************/
        /* 継承処理                                                             */
        /************************************************************************/

		/// <summary>
		/// 画面を覆うフィルターの見える横幅のサイズ
		/// </summary>
		/// <returns>フィルター幅</returns>
		public double GetDisplayFilterWidth() { return 50; }

		/// <summary>
		/// 他のシーンから戻ってきた際の処理
		/// </summary>
		public void SceneBack() { }

		/************************************************************************/
		/* コールバック処理                                                     */
		/************************************************************************/

		//============================================================================
		//! 戻るボタンが押された
		private void _ClickReturnButton(object iSender, RoutedEventArgs iArgs)
		{
            var button = iSender as Button;
            if (button == null)
            {
                return;
            }

            _UpdateInfo();
            SaveManager.GetInstance().AutoSaveProject();
            SceneManager.GetInstance().ReturnBackScene();
		}

		//============================================================================
		//! ツイッター連携ボタンが押された
		private void _ClickRegistTwitterAccountButton(object iSender, RoutedEventArgs iArgs)
		{
			//ウィンドウを表示する
			var window = new TwitterSettingWindow();
			window.Closing += (iClosingSender, iClosingArgs) =>
			{
				//成功した時のみ
				if (window.IsAccess())
				{
					var manager = SystemManager.GetInstance();
					var info = manager.GetSystemInfo();
					info.mAccessToken = window.GetAccessToken();
					manager.SetSystemInfo(info);
					
					_Refresh();
					SaveManager.GetInstance().SaveProject();
				}
			};
			window.ShowDialog();
		}

		//============================================================================
		//! デフォルトツイート設定ボタンが押された
		private void _ClickDefaultTweetButton(object iSender, RoutedEventArgs iArgs)
		{
			//ウィンドウを表示する
			var manager = SystemManager.GetInstance();
			var info = manager.GetSystemInfo();
			var window = new DefaultTweetInputWindow();
			if (info.mDefaultTweetStyle == null || info.mDefaultTweetStyle.Trim().Length == 0)
			{
				window.SetDefaultText();
			}
			else
			{
				window.SetText(info.mDefaultTweetStyle);
			}
			window.Closing += (iClosingSender, iClosingArgs) =>
			{
				//成功した時のみ
				if (window.IsSuccess())
				{
					info.mDefaultTweetStyle = window.GetDefaultTweet();
					manager.SetSystemInfo(info);
					SaveManager.GetInstance().SaveProject();
				}
			};
			window.ShowDialog();
		}

		//============================================================================
		//! ツイッター連携解除ボタンが押された
		private void _ClickRemoveTwitterAccountButton(object iSender, RoutedEventArgs iArgs)
		{
			var manager = SystemManager.GetInstance();
			var info = manager.GetSystemInfo();
			info.mAccessToken = new SystemManager.STwitterAccessToken();
			manager.SetSystemInfo(info);
			_Refresh();
		}

        /************************************************************************/
        /* 内部処理                                                             */
        /************************************************************************/

        //============================================================================
        //! 入力されている項目をマネージャに保存
        private void _UpdateInfo()
        {
			var info = SystemManager.GetInstance().GetSystemInfo();
            info.mAutoSaveFlg = (AutoSaveCheckBox.IsChecked == true);
            SystemManager.GetInstance().SetSystemInfo(info);
        }

		//============================================================================
		//! 画面更新
		private void _Refresh()
		{
			var info = SystemManager.GetInstance().GetSystemInfo();
			if (info.mAccessToken.mToken == null)
			{
				AccountRegistButton.IsEnabled = true;
				AccountRemoveButton.IsEnabled = false;
				TweetDefaultSettingButton.IsEnabled = false;
				IDTextBlock.Text = "";
				NameTextBlock.Text = "";
			}
			else
			{
				AccountRegistButton.IsEnabled = false;
				AccountRemoveButton.IsEnabled = true;
				TweetDefaultSettingButton.IsEnabled = true;
				IDTextBlock.Text = "TwitterID : " + info.mAccessToken.mUserId;
				NameTextBlock.Text = "TwitterName : " + info.mAccessToken.mScreenName;
			}
			AutoSaveCheckBox.IsChecked = info.mAutoSaveFlg;
		}
	}
}
