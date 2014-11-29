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
	/// SelectPresetWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class SelectPresetWindow : Window
	{
		/// <summary>
		/// コンストラクタ
		/// </summary>
		public SelectPresetWindow()
		{
			InitializeComponent();
			_Initialize();
		}

		/// <summary>
		/// プリセット名の取得
		/// </summary>
		/// <returns>プリセット名</returns>
		public string GetPresetName()
		{
			return (mSelectIndex == -1) ? null : mPresetNameList[mSelectIndex];
		}

		/************************************************************************/
		/* コールバック処理		                                                */
		/************************************************************************/

		//============================================================================
		//! ボタンが押された時の処理
		private void _ClickButton(object iSender, RoutedEventArgs iArgs)
		{
			mSelectIndex = PresetComboBox.SelectedIndex;
			Close();
		}

		/************************************************************************/
		/* 内部定義                                                             */
		/************************************************************************/

		//============================================================================
		//! 初期化
		private void _Initialize()
		{
			string preset_Path = SystemUtility.GetResourcePath() + @"Preset";
			string[] directory_List = System.IO.Directory.GetDirectories(preset_Path);
			foreach (string iPath in directory_List)
			{
				string preset_File_Path = iPath + @"\Preset.psx";

				try
				{
					if (System.IO.File.Exists(preset_File_Path))
					{
						//XMLを開いてゲーム名を取得
						var analayzer = new DL.CXmlAnalyzer();
						analayzer.AnalyzeXmlFile(preset_File_Path);
						var node = analayzer.GetRootNode().GetChildNodeFromPath("Preset/GameName")[0];
						string game_Name = node.GetNodeInfo().mValue;
						PresetComboBox.Items.Add(game_Name);

						//プリセット名を追加
						string preset_Name = preset_File_Path.Substring(0 , preset_File_Path.LastIndexOf(@"\"));
						preset_Name = preset_Name.Substring(preset_Name.LastIndexOf(@"\") + 1);
						mPresetNameList.Add(preset_Name);
					}
				}
				catch (System.Exception iException)
				{
					//無効なプリセットデータがあります
					string message = string.Format("不正なプリセットデータが存在します\n[{0}]\n\n -> {1}", preset_File_Path, iException.Message);
					SystemUtility.DisplayErrorDialog(message);
					System.Environment.Exit(1);
				}
			}

			//無し
			if (PresetComboBox.Items.Count == 0)
			{
				SystemUtility.DisplayErrorDialog("使用可能なプリセットデータが存在しません");
				System.Environment.Exit(1);
			}

			//選択
			PresetComboBox.SelectedIndex = 0;
			mSelectIndex = -1;
		}

		/************************************************************************/
		/* 変数宣言                                                             */
		/************************************************************************/

		/// <summary>
		/// 選択番号
		/// </summary>
		private int mSelectIndex;

		/// <summary>
		/// プリセット名リスト
		/// </summary>
		private List<string> mPresetNameList = new List<string>();
	}
}
