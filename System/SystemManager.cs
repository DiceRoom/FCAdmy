using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// システムマネージャ
	/// </summary>
	public partial class SystemManager : Singleton<SystemManager>
	{
		/************************************************************************/
		/* 公開定義                                                             */
		/************************************************************************/

		/// <summary>
		/// キー入力が行われた時に呼ばれるデリゲート
		/// </summary>
		public delegate void InputKeyDelegate(KeyEventArgs iArgs);

		/// <summary>
		/// ウィンドウが閉じられようとしている時に呼ばれるデリゲート
		/// </summary>
		public delegate bool CloseWindowDelegate();

		/************************************************************************/
		/* 公開処理                                                             */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public SystemManager()
		{
			mSystemInfo.mAutoSaveFlg = true;
			mSystemInfo.mAccessToken = new STwitterAccessToken();
		}

        /// <summary>
        /// システムデータの取得
        /// </summary>
        public void LoadSystemData()
        {
            mSystemInfo = (SSystemInfo)DataManager.GetInstance().GetData(SaveManager.GetInstance().SystemInfoKey);

            //バージョンが古いとここがNULLになるので対応(そのうち消す)
            if (mSystemInfo.mDefaultTweetStyle == null)
            {
                mSystemInfo.mDefaultTweetStyle = "大会名:{TournamentName}\n";
                mSystemInfo.mDefaultTweetStyle += "{TournamentData}開始\n\n";
                mSystemInfo.mDefaultTweetStyle += "↓↓↓↓↓対戦情報画像はこちら↓↓↓↓↓\n";
                mSystemInfo.mDefaultTweetStyle += "{ImageUrl}";
            }
        }
     
        /************************************************************************/
        /* アクセサ                                                             */
        /************************************************************************/

        /// <summary>
        /// ユーザー名の取得
        /// </summary>
        /// <param name="iUserName">ユーザー名</param>
        public void SetUserName(string iUserName)
        {
            mUserName = iUserName;
        }

        /// <summary>
        /// ユーザー名の取得
        /// </summary>
        /// <returns>ユーザー名</returns>
        public string GetUserName()
        {
            return mUserName;
        }

        /// <summary>
        /// システム情報の設定
        /// </summary>
        /// <param name="iSystemInfo">システム情報</param>
        public void SetSystemInfo(SSystemInfo iSystemInfo)
        {
            mSystemInfo = iSystemInfo;
        }

		/// <summary>
		/// システム情報の取得
		/// </summary>
		/// <returns></returns>
		public SSystemInfo GetSystemInfo()
		{
			return mSystemInfo;
		}

		/// <summary>
		/// キー入力が行われた時に呼ばれるデリゲート
		/// </summary>
		/// <param name="iCallback">コールバック</param>
		public void SetInputKeyDelegate(InputKeyDelegate iCallback)
		{
			mInputKeyDelegate = iCallback;
		}

		/// <summary>
		/// ウィンドウが閉じられようとしている時に呼ばれるデリゲートの設定
		/// </summary>
		/// <param name="iCallback">コールバック</param>
		public void SetCloseCheckCallback(CloseWindowDelegate iCallback)
		{
			mCloseWindowDelegate = iCallback;
		}

		/// <summary>
		/// ウィンドウを閉じていいかのチェック
		/// </summary>
		/// <returns>終了フラグ</returns>
		public bool IsWindowCheckClose()
		{
			if (mCloseWindowDelegate != null)
			{
				return mCloseWindowDelegate();
			}

			return true;
		}

		/// <summary>
		/// ウィンドウにキー入力があった時に呼ばれる
		/// </summary>
		/// <param name="iArgs"></param>
		public void KeyInput(KeyEventArgs iArgs)
		{
			if (mInputKeyDelegate != null)
			{
				mInputKeyDelegate(iArgs);
			}
		}

        /// <summary>
        /// スクリーンショット用のグリッド設定
        /// </summary>
        /// <param name="iGrid">グリッド</param>
        public void SetScreenShotGrid(Grid iGrid)
        {
            mScreenShotGrid = iGrid;
        }

        /// <summary>
        /// スクリーンショット用のグリッドの取得
        /// </summary>
        /// <returns>グリッド</returns>
        public Grid GetScreenShotGrid()
        {
            return mScreenShotGrid;
        }

		/// <summary>
		/// TwitterAPIのコンシューマキーの取得
		/// </summary>
		/// <returns>コンシューマキー</returns>
		public string GetTwitterAPICKey()
		{
			return mTwitterAPIConsumerKey;
		}

		/// <summary>
		/// TwitterAPIのコンシューマシークレットの取得
		/// </summary>
		/// <returns>コンシューマシークレット</returns>
		public string GetTwitterAPICSecret()
		{
			return mTwitterAPIConsumerSecret;
		}

		/// <summary>
		/// TwitpicのAPIキーの取得
		/// </summary>
		/// <returns>APIキー</returns>
		public string GetTwitPicAPIKey()
		{
			return mTwitPicAPIKey;
		}

        /************************************************************************/
        /* 変数定義                                                             */
        /************************************************************************/

        /// <summary>
        /// ツール使用者名
        /// </summary>
        private string mUserName;

		/// <summary>
		/// システム情報
		/// </summary>
		private SSystemInfo mSystemInfo;

		/// <summary>
		/// キー入力が行われた時に呼ばれるデリゲート
		/// </summary>
		private InputKeyDelegate mInputKeyDelegate;

		/// <summary>
		/// ウィンドウが閉じられようとしている時に呼ばれるデリゲート
		/// </summary>
		private CloseWindowDelegate mCloseWindowDelegate;

        /// <summary>
        /// スクリーンショット撮影用のグリッド
        /// </summary>
        private Grid mScreenShotGrid;

		/// <summary>
		/// TwitterAPIのコンシューマキー
		/// </summary>
		private string mTwitterAPIConsumerKey = "TTTFC7hrQryrIJzS39hzAw";

		/// <summary>
		/// TwitterAPIのコンシューマシークレット
		/// </summary>
		private string mTwitterAPIConsumerSecret = "4xPSN7rnaeOB8PQTh1zDYeL1vKJzUVdRfFe8XYzs";

		/// <summary>
		/// TwitpicのAPIキー
		/// </summary>
		private string mTwitPicAPIKey = "f2e4ff6c190f36f1d091399c235ae515";
	}
}
