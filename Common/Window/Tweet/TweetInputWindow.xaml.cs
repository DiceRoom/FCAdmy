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
using System.Windows.Shapes;

namespace FightingCommunityAdministrator
{
    /// <summary>
    /// DefaultTweetInputWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class TweetInputWindow : Window
    {
        /************************************************************************/
        /* 基本処理                                                             */
        /************************************************************************/

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TweetInputWindow()
        {
            InitializeComponent();

			//初期化
            TweetTextBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            TweetTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }

        /************************************************************************/
        /* コールバック処理                                                     */
        /************************************************************************/
		
		//============================================================================
		//! テキストが変更された時に呼ばれる
		private void _ChangeText(object iSender, TextChangedEventArgs iArgs)
		{
			var str = TweetTextBox.Text.Trim();
			int result = 130 - str.Length;

			//URL系統のチェック
			int pos = 0;
			while (true)
			{
				var begin_pos = str.IndexOf("http://", pos);
				if (begin_pos == -1)
				{
					begin_pos = str.IndexOf("https://", pos);
				}

				//終了
				if (begin_pos == -1)
				{
					break;
				}

				//URLの文字数取得
				var end_pos_list = new int[] { str.IndexOf(" ", begin_pos), str.IndexOf("　", begin_pos), str.IndexOf("\n", begin_pos), str.IndexOf("\t", begin_pos) };
				var end_pos_index = -1;
				foreach (var iIndex in System.Linq.Enumerable.Range(0, end_pos_list.Length))
				{
					//入れ替え
					if (end_pos_list[iIndex] != -1 && (end_pos_index == -1 || end_pos_index > end_pos_list[iIndex]))
					{
						end_pos_index = iIndex;
					}
				}

				//文字数
				var not_found_end = (end_pos_index == -1);
				var url = not_found_end ? str.Substring(pos) : str.Substring(pos, end_pos_list[end_pos_index] - begin_pos - 1);
				if (url.Length > 23)
				{
					result += (url.Length - 23);
				}

				//終了
				if (not_found_end)
				{
					break;
				}
				pos = end_pos_list[end_pos_index];
			}

			//予想ツイート文字数設定
			CharaLengthTextBlock.Text = string.Format("残入力可能文字数：{0}", result);
			var color = (result < 0) ? "#FFFF0000" : "#FF0000FF";
			CharaLengthTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(SystemUtility.StringToColor(color));
			TweetButton.IsEnabled = (result >= 0);
		}

		//============================================================================
        //! OKボタンが押された
		private void _ClickOKButton(object iSender, RoutedEventArgs iArgs)
		{
			//チェック
			var tweet = TweetTextBox.Text.Trim();
			if (tweet.Length == 0)
			{
				System.Media.SystemSounds.Hand.Play();
				MessageBox.Show("ツイートが入力されて居ません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			tweet += "\n\n#ＦＣあどみぃ！";

			//ツイート
			try
			{
				var manager = SystemManager.GetInstance();
				var service = new TweetSharp.TwitterService(manager.GetTwitterAPICKey(), manager.GetTwitterAPICSecret());
				var info = manager.GetSystemInfo();
				service.AuthenticateWith(info.mAccessToken.mToken, info.mAccessToken.mTokenSecret);
				var responce = service.SendTweet(new TweetSharp.SendTweetOptions { Status = tweet });

				System.Media.SystemSounds.Asterisk.Play();
				MessageBox.Show("ツイートに成功しました！", "確認", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (System.Exception iException)
			{
				SystemUtility.DisplayErrorDialog("ツイートに失敗しました\n\n" + iException.Message);
			}

			Close();
		}
    }
}
