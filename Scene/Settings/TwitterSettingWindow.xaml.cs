using System;
using System.Collections.Generic;
using System.Windows;
using TweetSharp;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// TwitterSettingWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class TwitterSettingWindow : Window
	{
		/************************************************************************/
		/* 基本処理                                                             */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public TwitterSettingWindow()
		{
			InitializeComponent();

			//認証画面を出す
			var manager = SystemManager.GetInstance();
			mService = new TwitterService(manager.GetTwitterAPICKey(), manager.GetTwitterAPICSecret());
			mRequestToken = mService.GetRequestToken();
			string authorize_Url = "https://api.twitter.com/oauth/authorize?" + "oauth_token=" + mRequestToken.Token + "&oauth_token_secret=" + mRequestToken.TokenSecret;
			//Uri uri = mService.GetAuthenticationUrl(mRequestToken);
			TwitterWebBrowser.Source = new Uri(authorize_Url);
		}

		/************************************************************************/
		/* アクセサ	                                                            */
		/************************************************************************/

		/// <summary>
		/// アクセストークンの生成に成功したか
		/// </summary>
		/// <returns>生成成功フラグ</returns>
		public bool IsAccess()
		{
			return (mAccessToken != null);
		}

		/// <summary>
		/// アクセストークンの取得
		/// </summary>
		/// <returns>アクセストークン</returns>
		public SystemManager.STwitterAccessToken GetAccessToken()
		{
			var token = new SystemManager.STwitterAccessToken();
			token.mToken = mAccessToken.Token;
			token.mTokenSecret = mAccessToken.TokenSecret;
			token.mUserId = mAccessToken.UserId;
			token.mScreenName = mAccessToken.ScreenName;
			return token;
		}

		/************************************************************************/
		/* コールバック処理                                                     */
		/************************************************************************/

		//============================================================================
		//! PIN表示画面が来たらPIN取得して閉じる
		private void _DocumentLoadCompleted(object iSender, System.Windows.Navigation.NavigationEventArgs iArgs)
		{
			//コードからPINデータを検索して、なければ終わり
			var document = (mshtml.HTMLDocument)TwitterWebBrowser.Document;
			var code = document.body.innerHTML;
			var begin = code.IndexOf("<code>");
			if (begin == -1)
			{
                begin = code.IndexOf("<CODE>");
                if (begin == -1)
                {
                    return;
                }
			}
			begin += 6;
			var end = code.IndexOf("</code>", begin);
			if(end == -1)
			{
                end = code.IndexOf("</CODE>", begin);
                if (end == -1)
                {
                    return;
                }
			}

			//存在する
			var verifier = code.Substring(begin, end - begin);
			mAccessToken = mService.GetAccessToken(mRequestToken, verifier);
			mService.AuthenticateWith(mAccessToken.Token, mAccessToken.TokenSecret);

			//成功時終了
			if (IsAccess())
			{
				Close();
			}
		}

		/************************************************************************/
		/* 変数宣言                                                             */
		/************************************************************************/

		/// <summary>
		/// サービス
		/// </summary>
		private TwitterService mService;

		/// <summary>
		/// リクエストトークン
		/// </summary>
		private OAuthRequestToken mRequestToken;
		
		/// <summary>
		/// アクセストークン
		/// </summary>
		OAuthAccessToken mAccessToken = null;
	}
}
