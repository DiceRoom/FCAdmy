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

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// BattleWaitControl.xaml の相互作用ロジック
	/// </summary>
	public partial class TournamentResultControl : UserControl
	{
		/************************************************************************/
		/* 公開処理		                                                        */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public TournamentResultControl()
		{
			InitializeComponent();
		}

		/************************************************************************/
		/* アクセサ                                                             */
		/************************************************************************/

		/// <summary>
		/// 上側(左側)のリーフ情報設定
		/// </summary>
		public void SetUpWaitInfo(string iName, ImageSource iCharacterImageSource)
		{
			UpNameTextBlock.Text = iName;
			UpLeafImage.Source = iCharacterImageSource;
		}

		/// <summary>
		/// 下側(右側)のリーフ情報設定
		/// </summary>
		public void SetDownWaitInfo(string iName, ImageSource iCharacterImageSource)
		{
			DownNameTextBlock.Text = iName;
			DownLeafImage.Source = iCharacterImageSource;
		}

		/// <summary>
		/// 何回戦かを設定
		/// </summary>
		/// <param name="iRound">対戦数</param>
		public void SetRoundDepth(int iRound)
		{
			RoundTextBlock.Text = (iRound == -1) ? "決勝戦" : string.Format("第{0}回戦\n", iRound);
		}

		/// <summary>
		/// 勝利チームの設定
		/// </summary>
		/// <param name="iIndex">勝利番号</param>
		public void SetWinIndex(int iIndex)
		{
			var opcity = 0.3;
			if (iIndex == 0)
			{
				DownNameTextBlock.Opacity = opcity;
				DownLeafImage.Opacity = opcity;
			}
			else
			{
				UpNameTextBlock.Opacity = opcity;
				UpLeafImage.Opacity = opcity;

				//WIN,LOSE文字の入れ替え
				var brush = WinTextBlock.Foreground;
				WinTextBlock.Foreground = LoseTextBlock.Foreground;
				LoseTextBlock.Foreground = brush;
				WinTextBlock.Text = "LOSE";
				LoseTextBlock.Text = "WIN";

				//LOSEの位置を少しずらす
				var margin = LoseTextBlock.Margin;
				margin.Right += 2;
				LoseTextBlock.Margin = margin;
			}
		}
	}
}
