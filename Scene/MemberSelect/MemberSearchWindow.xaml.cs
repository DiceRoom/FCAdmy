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
	/// MemberSearchWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MemberSearchWindow : Window
	{
		/************************************************************************/
		/* 公開定義		                                                        */
		/************************************************************************/

		/// <summary>
		/// 検索状態が変更された時に呼ばれるコールバック
		/// </summary>
		public delegate void ChangeSearchDelegate(string iSerachMemberName, int iSearchCharacterID);

		/************************************************************************/
		/* 基本処理	                                                            */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public MemberSearchWindow()
		{
			InitializeComponent();

			//初期設定
			MemberNameTextBox.Text = "";
			MemberNameTextBox.TextChanged += (iSender, iArgs) =>
			{
				_ChangeSearch();
			};

			//コンボボックス初期化
			CharacterIDComboBox.Items.Add("未選択");
			foreach (var iCharacterInfo in PresetManager.GetInstance().GetCharacterList())
			{
				mCharacterIDList.Add(iCharacterInfo.mIndex);
				CharacterIDComboBox.Items.Add(iCharacterInfo.mName);
			}
			CharacterIDComboBox.SelectedIndex = 0;
			CharacterIDComboBox.SelectionChanged += (iSender, iArgs) =>
			{
				_ChangeSearch();
			};
		}

		/************************************************************************/
		/* アクセサ	                                                            */
		/************************************************************************/

		/// <summary>
		/// 検索状態が変更された時に呼ばれるコールバックの設定
		/// </summary>
		/// <param name="iCallback">コールバック</param>
		public void SetChangeSearchCallback(ChangeSearchDelegate iCallback) { mChangeSearchDelegate = iCallback; }

		/// <summary>
		/// メンバー名検索文字列の取得
		/// </summary>
		/// <returns>メンバー名</returns>
		public string GetSearchMemberName() { return MemberNameTextBox.Text; }

		/// <summary>
		/// 検索デフォルト使用キャラクター番号の取得
		/// </summary>
		/// <returns>キャラクター番号</returns>
		public int GetSearchCharacterID()
		{
			return (CharacterIDComboBox.SelectedIndex == 0) ? -1 : mCharacterIDList[CharacterIDComboBox.SelectedIndex - 1];
		}

		/************************************************************************/
		/* コールバック                                                         */
		/************************************************************************/

		//============================================================================
		//! キーが押された時に呼ばれる
		private void _ClickKey(object iSender, KeyEventArgs iArgs)
		{
			//Ctrl+F又はF3が押された時に検索ウィンドウの開閉状態を入れ替える
			if (((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None && iArgs.Key == Key.F) ||
				iArgs.Key == Key.F3)
			{
				Close();
			}
		}

		/************************************************************************/
		/* 内部処理                                                             */
		/************************************************************************/

		//============================================================================
		//! 検索状態が変更された
		private void _ChangeSearch()
		{
			if (mChangeSearchDelegate != null)
			{
				mChangeSearchDelegate(GetSearchMemberName(), GetSearchCharacterID());
			}
		}

		/************************************************************************/
		/* 変数定義                                                             */
		/************************************************************************/

		/// <summary>
		/// 検索状態が変更された時に呼ばれるコールバック
		/// </summary>
		private ChangeSearchDelegate mChangeSearchDelegate;

		/// <summary>
		/// キャラクターIDリスト
		/// </summary>
		private List<int> mCharacterIDList = new List<int>();
	}
}
