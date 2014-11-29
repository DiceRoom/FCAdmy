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
	/// CharacterSelectWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class CharacterSelectWindow : Window
	{
		/************************************************************************/
		/* 基本処理                                                             */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public CharacterSelectWindow()
		{
			InitializeComponent();
			_CreateCharacterList();

			//ラムダ式で解決
			SourceInitialized += (iSender, iArgs) =>
			{
				//ウィンドウ位置の読み込み
				var get_obj = DataManager.GetInstance().GetData("CharacterSelectWindow");
				if (get_obj != null)
				{
					var size = (Size)get_obj;
					Width = size.Width;
					Height = size.Height;
				}
			};

			//閉じる直前
			Closing += (iSender, iArgs) =>
			{
				DataManager.GetInstance().SetData("CharacterSelectWindow", new Size(Width, Height));
			};
		}

		/************************************************************************/
		/* アクセサ                                                             */
		/************************************************************************/

		/// <summary>
		/// 選択されたキャラクター番号の取得
		/// </summary>
		/// <returns></returns>
		public int GetSelectCharacterID()
		{
			return mSelectCharacterID;
		}

		/************************************************************************/
		/* コールバック処理                                                     */
		/************************************************************************/

		//============================================================================
		//! キャラクターが選択された時に呼ばれる処理
		private void _ClickCharacter(object iSender, MouseButtonEventArgs iArgs)
		{
			var control = iSender as CharacterSelectListControl;
			if (control == null)
			{
				return;
			}

			mSelectCharacterID = control.TabIndex;
			Close();
		}

		/************************************************************************/
		/* 内部処理                                                             */
		/************************************************************************/

		//============================================================================
		//! キャラクターリストの作成
		public void _CreateCharacterList()
		{
			var set_Width = SelectCharacterScrollViewer.Width;
			int color_Index = 0;
			var color_List = new Brush[2] { new SolidColorBrush(SystemUtility.StringToColor("#55000000")), new SolidColorBrush(SystemUtility.StringToColor("#55555555")) };

			//作成
			foreach (var iCharacter in PresetManager.GetInstance().GetCharacterList())
			{
				var control = new CharacterSelectListControl();
				control.Width = set_Width;
				control.Background = color_List[color_Index % 2];
				control.SetCharacterInfo(iCharacter);
				control.TabIndex = iCharacter.mIndex;
				control.PreviewMouseLeftButtonDown += _ClickCharacter;
				SelectCharacterStackPanel.Children.Add(control);
				++color_Index;
			}
		}

		/************************************************************************/
		/* 変数定義                                                             */
		/************************************************************************/

		/// <summary>
		/// 選択されたキャラクター番号
		/// </summary>
		public int mSelectCharacterID = -1;
	}
}
