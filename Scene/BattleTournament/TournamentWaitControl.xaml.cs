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
	public partial class TournamentWaitControl : UserControl
	{
		/************************************************************************/
		/* 公開処理		                                                        */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public TournamentWaitControl()
		{
			InitializeComponent();
			FilterRectangle.Visibility = Visibility.Hidden;
			RoundTextBlock.Text = "第〇回戦";
			VSTextBlock.Text = "VS";
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
			RoundTextBlock.Text = (iRound == -1) ? "決勝戦" : string.Format("第{0}回戦", iRound);
		}
	}
}
