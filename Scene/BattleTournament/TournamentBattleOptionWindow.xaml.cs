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
	/// TournamentBattleOptionWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class TournamentBattleOptionWindow : Window
	{
		/************************************************************************/
		/* 公開定義                                                             */
		/************************************************************************/

        /// <summary>
        /// スクリーンショットの種別
        /// </summary>
        public enum EScreenShotKind
        {
            /// <summary>
            /// メンバー表
            /// </summary>
            SCREEN_SHOT_MEMBER,

            /// <summary>
            /// トーナメント
            /// </summary>
            SCREEN_SHOT_TOURNAMENT,

            /// <summary>
            /// メンバー表+トーナメント
            /// </summary>
            SCREEN_SHOT_ALL,
        };
		
		/// <summary>
		/// スクリーンショットを実生成するためのコールバック
		/// </summary>
		public delegate string CreateScreenShotDelegate(EScreenShotKind iScreenShotKind);
		
		/************************************************************************/
		/* 基本処理                                                             */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public TournamentBattleOptionWindow()
		{
			InitializeComponent();

			//ウィンドウサイズを読み込む
			var get_Object = DataManager.GetInstance().GetData(mContentKey);
            bool auto_Check = (get_Object == null);
			if (!auto_Check)
			{
                var data = (SerializeData)get_Object;
				Left = data.mWindowPosition.X;
				Top = data.mWindowPosition.Y;

                var w = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
                var h = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
                if (Left > w || Top > h)
                {
                    auto_Check = true;
                }
                else
                {
                    Width = data.mWindowSize.Width;
                    Height = data.mWindowSize.Height;
                }
			}

            //自動
            if(auto_Check)
			{
				var main_Window = Application.Current.MainWindow;
				Left = main_Window.Left + main_Window.Width + 10;
				Top = main_Window.Top;

                var w = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
                var h = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
                if (Left > w || Top > h)
                {
                    Left = Top = 0;
                }
			}

			//F1キーでクローズする
			KeyDown += (iSender, iArgs) =>
			{
				if (iArgs.Key == Key.F1)
				{
					Close();
				}
			};

			//チーム戦でなければメンバー表は作れない
			TeamListItem.IsEnabled = BattleOperatorManager.GetInstance().IsTeamBattleFlg();
			ConnectImageItem.IsEnabled = TeamListItem.IsEnabled;
            UploadTeamListItem.IsEnabled = TeamListItem.IsEnabled;
            UploadConnectImageItem.IsEnabled = TeamListItem.IsEnabled;
		}

		/// <summary>
		/// 対戦待ちのフィルターの全非表示化
		/// </summary>
		public void ClearSelectFilter()
		{
			foreach (TournamentWaitControl iContron in VersusDisplayStackPanel.Children)
			{
				iContron.FilterRectangle.Visibility = Visibility.Hidden;
			}
		}

		/// <summary>
		/// スクリーンショットを実生成するためのコールバックの設定
		/// </summary>
		/// <param name="iCallback">コールバック</param>
		public void SetCreateScreenShotCallback(CreateScreenShotDelegate iCallback) { mCreateScreenShotDelegate = iCallback; }

		/************************************************************************/
		/* コールバック定義                                                     */
		/************************************************************************/

		//============================================================================
		//! 閉じるときの処理
		private void _Closing(object iSender, System.ComponentModel.CancelEventArgs iArgs)
		{
			//ウィンドウサイズを保存する
			var serial_Data = new SerializeData();
			serial_Data.mWindowPosition = new Point(Left, Top);
			serial_Data.mWindowSize = new Size(Width, Height);
			DataManager.GetInstance().SetData(mContentKey, serial_Data);
		}

		//============================================================================
		//! スクリーンショットフォルダを開く
		private void _OpenScreenShotFolder(object iSender, RoutedEventArgs iArgs)
		{
			var directory_Path = SystemUtility.GetRootPath() + @"ScreenShot\";
			System.Diagnostics.Process.Start(directory_Path);
		}

		//============================================================================
		//! 指定した画像を表示する
		private void _OpenUploadPhotoPage(object iSender, RoutedEventArgs iArgs)
		{
			var item = iSender as MenuItem;
			if (item != null)
			{
				var index = item.TabIndex;
				System.Diagnostics.Process.Start(mUploadURLList[index]);
			}
		}

        //============================================================================
		//! スクリーンショットの生成を行う
        private void _ClickCreateScreenShot(object iSender, RoutedEventArgs iArgs)
        {
            var item = iSender as MenuItem;
            if (item != null)
            {
                var path = mCreateScreenShotDelegate((EScreenShotKind)item.TabIndex);
                if (path.Length > 0)
                {
                    System.Media.SystemSounds.Asterisk.Play();
                    MessageBox.Show("以下にスクリーンショットを出力しました\n\n" + path, "確認", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        //============================================================================
        //! 画像のアップロードを行う
        private void _ClickUploadScreenShot(object iSender, RoutedEventArgs iArgs)
        {
            //まずは画像の生成
            var item = iSender as MenuItem;
            var path = "";
            var file_name = "";
            if (item != null)
            {
                var kind = (EScreenShotKind)item.TabIndex;
                path = mCreateScreenShotDelegate(kind);
                switch (kind)
                {
                case EScreenShotKind.SCREEN_SHOT_MEMBER:
                    file_name = "メンバー表";
                    break;
                case EScreenShotKind.SCREEN_SHOT_TOURNAMENT:
                    file_name = "トーナメント表";
                    break;
                case EScreenShotKind.SCREEN_SHOT_ALL:
                    file_name = "メンバー表+トーナメント表";
                    break;
                }
            }

            //生成に失敗
            if (path.Length == 0)
            {
                return;
            }

            try
            {
                System.Media.SystemSounds.Asterisk.Play();
                MessageBox.Show("画像のアップロード中画面が止まりますが\nそのままでお待ちください", "確認", MessageBoxButton.OK, MessageBoxImage.Information);

                //アップロードする
                string url = DL.UploaderUtility.Upload(path);
                
                //エラー
                if (url == null || url.Length == 0)
                {
                    throw new System.Exception("ファイルのアップロードに失敗しました");
                }

                //アップロード結果からメニューにアイテムを追加
                var index = mUploadURLList.Count;
                var diplay = string.Format("{0}({1})", file_name, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                mUploadURLList.Add(url);

                //初追加の場合はセパレータも追加
                if (index == 0)
                {
                    UploadItem.Items.Add(new Separator());
                }

                //アイテム作成
                var new_item = new MenuItem();
                new_item.Header = diplay;
                new_item.TabIndex = index;
                UploadItem.Items.Add(new_item);

                //ツイッター認証してるかどうかでサブアイテム作るか決める
                var system_info = SystemManager.GetInstance().GetSystemInfo();
                if (system_info.mAccessToken.mScreenName == null || system_info.mAccessToken.mScreenName.Length == 0 ||
                    system_info.mAccessToken.mToken == null || system_info.mAccessToken.mToken.Length == 0 ||
                    system_info.mAccessToken.mTokenSecret == null || system_info.mAccessToken.mTokenSecret.Length == 0)
                {
                    //連携なし
                    new_item.Click += _OpenUploadPhotoPage;
                }
                else
                {
                    //連携有
                    var sub_item = new MenuItem();
                    sub_item.Header = "表示";
                    sub_item.TabIndex = index;
                    sub_item.Click += _OpenUploadPhotoPage;
                    new_item.Items.Add(sub_item);
                    sub_item = new MenuItem();
                    sub_item.Header = "ツイート";
                    sub_item.TabIndex = index;
                    sub_item.Click += _ClickTweetButton;
                    new_item.Items.Add(sub_item);
                }

                System.Media.SystemSounds.Asterisk.Play();
                MessageBox.Show("画像のアップロードに成功しました\n\n" + url, "確認", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception iException)
            {
            	System.Media.SystemSounds.Hand.Play();
                MessageBox.Show("画像のアップロードに失敗しました\n\n" + iException.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

		//============================================================================
		//! 指定した画像付きのツイートを行う
		private void _ClickTweetButton(object iSender, RoutedEventArgs iArgs)
		{
			var item = iSender as MenuItem;
			if (item != null)
			{
                var path = mCreateScreenShotDelegate((EScreenShotKind)item.TabIndex);
                if (path.Length > 0)
                {
                    //ツイート
			        string tweet = BattleOperatorManager.GetInstance().GetDefaultTweet(mUploadURLList[item.TabIndex]);
			
			        //ツイート編集ウィンドウを表示
			        var window = new TweetInputWindow();
			        window.TweetTextBox.Text = tweet;
    			    window.ShowDialog();
                }
			}
		}

		/************************************************************************/
		/* 内部定義                                                             */
		/************************************************************************/

		/// <summary>
		/// 個別書き出し用データ
		/// </summary>
		[Serializable]
		private class SerializeData
		{
			/// <summary>
			/// ウィンドウ位置
			/// </summary>
			public Point mWindowPosition;

			/// <summary>
			/// ウィンドウサイズ
			/// </summary>
			public Size mWindowSize;
		};

		/************************************************************************/
		/* 変数定義                                                             */
		/************************************************************************/

		/// <summary>
		/// コンテンツキー
		/// </summary>
		private string mContentKey = "TournamentBattleToolWindow";

		/// <summary>
		/// 現在アップロードされている画像のURLリスト
		/// </summary>
		private List<string> mUploadURLList = new List<string>();

		/// <summary>
		/// スクリーンショットを実生成するためのコールバックの設定
		/// </summary>
		private CreateScreenShotDelegate mCreateScreenShotDelegate;
	}
}
