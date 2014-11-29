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
    public partial class DefaultTweetInputWindow : Window
    {
        /************************************************************************/
        /* 基本処理                                                             */
        /************************************************************************/

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DefaultTweetInputWindow()
        {
            InitializeComponent();

            TweetTextBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            TweetTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
			TweetTextBox.IsUndoEnabled = false;
			TournamentNameButton.IsEnabled = false;
			TournamentDataButton.IsEnabled = false;
			TournamentDayButton.IsEnabled = false;
			TournamentTimeButton.IsEnabled = false;
			ImageButton.IsEnabled = false;
        }

        /************************************************************************/
        /* アクセサ                                                             */
        /************************************************************************/

		/// <summary>
		/// デフォルトテキストの設定
		/// </summary>
		public void SetDefaultText()
		{
			string text = "";
			text += "大会名:{TournamentName}\n";
			text += "{TournamentData}開始\n\n";
			text += "↓↓↓↓↓対戦情報画像はこちら↓↓↓↓↓\n";
			text += "{ImageUrl}";
			SetText(text);
		}

        /// <summary>
        /// テキストの設定
        /// </summary>
        /// <param name="iText">テキスト</param>
        public void SetText(string iText)
        {
            //設定
            mCurrentText = iText;

			//既に無視状態の場合
			if (mIgnoreChangeFlg)
			{
				_RefreshTextBox();
			}
			else
			{
				mIgnoreChangeFlg = true;
				_RefreshTextBox();
				mIgnoreChangeFlg = false;
			}
        }

		/// <summary>
		/// ＯＫで終了したかのフラグ取得
		/// </summary>
		/// <returns>成功フラグ</returns>
		public bool IsSuccess()
		{
			return mSuccessFlg;
		}

		/// <summary>
		/// 設定されているデフォルトツイートの取得
		/// </summary>
		/// <returns>デフォルトツイート</returns>
		public string GetDefaultTweet()
		{
			return mCurrentText;
		}

        /************************************************************************/
        /* コールバック処理                                                     */
        /************************************************************************/

        //============================================================================
        //! テキスト挿入ボタンが押された
        private void _ClickInsertTextButton(object iSender, RoutedEventArgs iArgs)
        {
            var button = iSender as Button;
            if (button != null)
            {
				var insert_str = "";
                switch (button.TabIndex)
                {
				case 0:
					insert_str = "{TournamentName}";
					break;
				case 1:
					insert_str = "{TournamentData}";
					break;
				case 2:
					insert_str = "{TournamentDay}";
					break;
				case 3:
					insert_str = "{TournamentTime}";
					break;
				case 4:
					insert_str = "{ImageUrl}";
					break;
				case 5:
					SetDefaultText();
					return;
                }

				//挿入先の取得
				var insert_pos = TweetTextBox.SelectionStart;
				foreach (var iPair in mSystemWordMap)
				{
					//終了チェック
					if(iPair.Key >= TweetTextBox.SelectionStart)
					{
						break;
					}

					//加算
					insert_pos += (iPair.Value.mReplaceLength - iPair.Value.mSystemWordLength);
				}

				SetText(mCurrentText.Insert(insert_pos, insert_str));
            }
        }

        //============================================================================
        //! テキストボックスの選択が変更されたとき
        private void _ChangeSelection(object iSender, RoutedEventArgs iArgs)
        {
            bool enable_flg = (TweetTextBox.SelectionLength == 0) ? (_IsSystemKeywordPosition(TweetTextBox.SelectionStart - 1) == -1) : false;
			TournamentNameButton.IsEnabled = enable_flg;
            TournamentDataButton.IsEnabled = enable_flg;
            TournamentDayButton.IsEnabled = enable_flg;
            TournamentTimeButton.IsEnabled = enable_flg;
			ImageButton.IsEnabled = enable_flg;
		}

		//============================================================================
		//! テキストが変更された時に呼ばれる
		private void _ChangeText(object iSender, TextChangedEventArgs iArgs)
		{
			//無視チェック
			if (mIgnoreChangeFlg)
			{
				return;
			}

			foreach (var iChange in iArgs.Changes)
			{
				//追加された文字列を取得しておく
				var add_str = (iChange.AddedLength > 0) ? TweetTextBox.Text.Substring(iChange.Offset, iChange.AddedLength) : "";

				//オフセット位置の計算(開始位置がシステムワードの場所の場合は変更)
				var begin_pos = iChange.Offset;
				var end_pos = iChange.Offset + iChange.RemovedLength;
				var sys_pos = _IsDeleteSystemKeywordPosition(begin_pos);
				if (sys_pos != -1)
				{
					begin_pos = sys_pos;
				}
				
				//削除チェック
				var check_pos = begin_pos;
				var current_pos = check_pos;
				var set_text = mCurrentText;
				var delete_offset = 0;
				var delete_map = new Dictionary<int, int>();
				var init_key = -1;
				while (current_pos < end_pos)
				{
					//システムワードの場合は削除予約
					int pos = _IsDeleteSystemKeywordPosition(current_pos);
					if (pos != -1)
					{
						//ここまでの分の削除
						var del_length = current_pos - check_pos;
						if (del_length > 0)
						{
							var stock_pos = check_pos;
							foreach (var iPair in mSystemWordMap)
							{
								//終了チェック
								if (iPair.Key >= stock_pos)
								{
									break;
								}
								check_pos += (iPair.Value.mReplaceLength - iPair.Value.mSystemWordLength);
							}
							set_text = set_text.Remove(check_pos - delete_offset, del_length);
							delete_offset += del_length;
						}

						//予約
						if (init_key == -1)
						{
							init_key = pos;
						}
						delete_map.Add(pos, del_length);

						//スキップする
						++current_pos;
						while (current_pos < end_pos)
						{
							int check_sys_pos = _IsDeleteSystemKeywordPosition(current_pos);
							
							//終了
							if (pos != check_sys_pos)
							{
								break;
							}

							++current_pos;
						}

						check_pos = current_pos;
					}
					else
					{
						++current_pos;
					}
				}

				//残り削除
				if (check_pos != end_pos)
				{
					var del_length = end_pos - check_pos;
					var stock_pos = check_pos;
					foreach (var iPair in mSystemWordMap)
					{
						//終了チェック
						if (iPair.Key >= stock_pos)
						{
							break;
						}
						check_pos += (iPair.Value.mReplaceLength - iPair.Value.mSystemWordLength);
					}
					set_text = set_text.Remove(check_pos - delete_offset , del_length);
				}

				//システムワード削除
				if(delete_map.Count > 0)
				{
					delete_offset = 0;
					foreach (var iPair in mSystemWordMap)
					{
						if(iPair.Key >= init_key)
						{
							break;
						}
						delete_offset += (iPair.Value.mSystemWordLength - iPair.Value.mReplaceLength);
					}
					foreach (var iPair in delete_map)
					{
						int length = mSystemWordMap[iPair.Key].mReplaceLength;
						set_text = set_text.Remove(iPair.Key - iPair.Value - delete_offset, length);
						delete_offset += length;
						delete_offset += iPair.Value;
						delete_offset += (mSystemWordMap[iPair.Key].mSystemWordLength - mSystemWordMap[iPair.Key].mReplaceLength);
					}
				}

				//追加
				if(add_str.Length > 0)
				{
					var add_offset = 0;
					foreach (var iPair in mSystemWordMap)
					{
						if (iPair.Key >= begin_pos)
						{
							break;
						}
						add_offset += (iPair.Value.mSystemWordLength - iPair.Value.mReplaceLength);
					}
					set_text = set_text.Insert(begin_pos - add_offset, add_str);
				}

				//設定して終了
				SetText(set_text);
				TweetTextBox.SelectionStart = begin_pos + add_str.Length;
				break;
			}
		}

		//============================================================================
        //! プレビューボタンが押された
		private void _ClickTweetPreview(object iSender, RoutedEventArgs iArgs)
		{
			//文字列作成
			string tweet = mCurrentText;
			var data_str = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
			tweet = tweet.Replace("{TournamentName}", "大会名表示");
			tweet = tweet.Replace("{TournamentData}", data_str);
			var sep_list = data_str.Split(' ');
			tweet = tweet.Replace("{TournamentDay}", sep_list[0]);
			tweet = tweet.Replace("{TournamentTime}", sep_list[1]);
			tweet = tweet.Replace("{ImageUrl}", "http://画像のURL～");
			tweet += "\n\n#ＦＣあどみぃ！";

			System.Media.SystemSounds.Asterisk.Play();
			MessageBox.Show(tweet, "ツイート確認", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		//============================================================================
        //! OKボタンが押された
		private void _ClickOKButton(object iSender, RoutedEventArgs iArgs)
		{
			//ありえないけどチェック
			var button = iSender as Button;
			if (button == null)
			{
				return;
			}

			//キャンセル
			if(button.TabIndex == 1)
			{
				Close();
				return;
			}

			//チェック
			if (mCurrentText.Trim().Length == 0)
			{
				System.Media.SystemSounds.Hand.Play();
				MessageBox.Show("デフォルトツイートが入力されて居ません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			//終了
			mSuccessFlg = true;
			Close();
		}

        /************************************************************************/
        /* 内部処理                                                             */
        /************************************************************************/

        //============================================================================
        //! テキストボックス内部を更新
        private void _RefreshTextBox()
        {
            //システムワード情報リストの作成
            string text = mCurrentText;
            int text_length = 10;
            int name_index = 0;
			mSystemWordMap.Clear();
			TweetTextBox.Text = "";
			int offset = 0;
			var current_pos = 0;
            while (true)
            {
				//開始チェック
                int pos = text.IndexOf('{' ,  current_pos);
                if (pos == -1)
                {
                    break;
                }

                //終了チェック
                int end_pos = text.IndexOf('}', pos);
                if (end_pos == -1)
                {
                    break;
                }

                //現在の位置から開始位置まっでの分を挿入する
				TweetTextBox.Text += text.Substring(current_pos, pos - current_pos);
				text_length += (pos - current_pos);

                //キーワード部分を変換して代入
                var get_text = text.Substring(pos, end_pos - pos + 1);
				var length = get_text.Length;
                if(get_text == "{TournamentName}")
                {
                    get_text = "【大会名が挿入されます】";
                    ++name_index;
                }
                else if(get_text == "{TournamentData}")
                {
                    get_text = "【大会日時がYYYY/MM/DD HH:MM:SSで入ります】";
                    text_length += 19;
                }
                else if(get_text == "{TournamentDay}")
                {
                    get_text = "【大会日時がYYYY/MM/DDで入ります】";
                    text_length += 10;
                }
                else if(get_text == "{TournamentTime}")
                {
                    get_text = "【大会時間がHH:MM:SSで入ります】";
                    text_length += 8;
                }
				else if (get_text == "{ImageUrl}")
				{
					get_text = "【添付されるイメージファイルのURLが入ります】";
					text_length += 22;
				}
				else
				{
					//キーワードじゃない場合
					length = 0;
				}
				TweetTextBox.Text += get_text;
				
                //データ保存
				if (length != 0)
				{
					var info = new SSystemWordInfo();
					info.mSystemWordLength = get_text.Length;
					info.mReplaceLength = length;
					mSystemWordMap.Add(pos + offset, info);

					offset += (info.mSystemWordLength - info.mReplaceLength);
				}
				current_pos = end_pos + 1;
            }

            //残ってる分があれば入れる
            if (current_pos != text.Length)
            {
				var get_text = text.Substring(current_pos);
				TweetTextBox.Text += get_text;
				text_length += get_text.Length;
            }

            //予想ツイート文字数設定
            if(name_index == 0)
            {
                CharaLengthTextBlock.Text = string.Format("ツイート文字数は{0}文字です" , text_length);
            }
            else if (name_index == 1)
            {
                CharaLengthTextBlock.Text = string.Format("ツイート文字数は{0}文字＋大会名の長さです", text_length);
            }
            else
            {
                CharaLengthTextBlock.Text = string.Format("ツイート文字数は{0}文字＋大会名の長さ×{1}です" , text_length , name_index);
            }
        }

        //============================================================================
        //! 指定した位置がシステムの位置であるかどうか
        private int _IsSystemKeywordPosition(int iPosition)
        {
            foreach (var iPair in mSystemWordMap)
            {
                if (iPair.Key <= iPosition && (iPair.Key + iPair.Value.mSystemWordLength - 1) > iPosition)
                {
					return iPair.Key;
                }
            }

            return -1;
        }

		//============================================================================
		//! 指定した位置がシステム削除の位置であるかどうか
		private int _IsDeleteSystemKeywordPosition(int iPosition)
		{
			foreach (var iPair in mSystemWordMap)
			{
				if (iPair.Key <= iPosition && (iPair.Key + iPair.Value.mSystemWordLength) > iPosition)
				{
					return iPair.Key;
				}
			}

			return -1;
		}

		/************************************************************************/
		/* 内部定義                                                             */
		/************************************************************************/

		/// <summary>
		/// システムワード情報
		/// </summary>
		private struct SSystemWordInfo
		{
			/// <summary>
			/// 変換後の長さ
			/// </summary>
			public int mSystemWordLength;

			/// <summary>
			/// 元のキーワードの長さ
			/// </summary>
			public int mReplaceLength;
		};

        /************************************************************************/
        /* 変数定義                                                             */
        /************************************************************************/

        /// <summary>
        /// 現在設定されているテキスト
        /// </summary>
        private string mCurrentText = "";

		/// <summary>
		/// テキストの変更チェック無視
		/// </summary>
		private bool mIgnoreChangeFlg = false;

		/// <summary>
		/// 成功フラグ
		/// </summary>
		private bool mSuccessFlg = false;

        /// <summary>
        /// システムで使われている部分の情報マップ
        /// </summary>
		private Dictionary<int, SSystemWordInfo> mSystemWordMap = new Dictionary<int, SSystemWordInfo>();
    }
}
