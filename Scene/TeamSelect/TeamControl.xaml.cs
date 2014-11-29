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
	/// MemberListControl.xaml の相互作用ロジック
	/// </summary>
	public partial class TeamControl : UserControl
	{
		/************************************************************************/
		/* 基本処理                                                             */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
        public TeamControl()
		{
			InitializeComponent();
            FilterRectangle.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// コントロールサイズの更新
        /// </summary>
        public void RefreshControlSize()
        {
            if (mCloseFlg)
            {
                OpenCloseButton.Content = "▼";
                Height = 54;
            }
            else
            {
                OpenCloseButton.Content = "▲";

                var stack_Panel_Height = mMemberList.Count * 30;
                MemberStackPanel.Height = stack_Panel_Height;
                Height = 54 + stack_Panel_Height;
            }
        }

		/************************************************************************/
		/* アクセサ                                                             */
		/************************************************************************/

        /// <summary>
        /// チーム情報の設定
        /// </summary>
        /// <param name="iTeamIndex"></param>
        /// <param name="iTeamName"></param>
        public void SetTeamInfo(int iTeamIndex, string iTeamName)
        {
            TeamNoTextBlock.Text = String.Format("No.{0:D2}", iTeamIndex + 1);
            TeamNameTextBlock.Text = iTeamName;
        }

        /// <summary>
        /// 参加メンバーIDの追加
        /// </summary>
        /// <param name="iJoinIndex">参加メンバーID</param>
        public InTeamMemberControl AddJoinMember(int iJoinIndex)
        {
            //コントロールの設定
            var color_List = new Brush[2] { new SolidColorBrush(SystemUtility.StringToColor("#55000000")), new SolidColorBrush(SystemUtility.StringToColor("#55555555")) };
            var control = new InTeamMemberControl();
            control.Width = Width;
            control.Background = color_List[mMemberList.Count % 2];
            control.TabIndex = mMemberList.Count;
            var get_Info = new BattleManager.SBattleJoinInfo();
            BattleOperatorManager.GetInstance().GetJoinInfo(ref get_Info, iJoinIndex);
            control.SetJoinMemberInfo(mMemberList.Count, get_Info);
            MemberStackPanel.Children.Add(control);
            mMemberList.Add(iJoinIndex);
            MemberValTextBlock.Text = string.Format("このチームには{0}人所属しています", mMemberList.Count);
            return control;
        }

        /// <summary>
        /// メンバー表示の開閉設定
        /// </summary>
        /// <param name="iCloseFlg">閉じるフラグ</param>
        public void SetOpenClose(bool iCloseFlg)
        {
            mCloseFlg = iCloseFlg;
            OpenCloseButton.Content = mCloseFlg ? "▼" : "▲";
        }

        /// <summary>
        /// フィルターの表示切替
        /// </summary>
        /// <param name="iVisibleFlg"></param>
        public void SetFilterVisible(bool iVisibleFlg)
        {
            FilterRectangle.Visibility = iVisibleFlg ? Visibility.Visible : Visibility.Hidden;
        }

        /************************************************************************/
        /* コールバック処理                                                     */
        /************************************************************************/

        //============================================================================
        //! カーソルが項目に乗った時の処理
        private void _MouseEnter(object iSender, MouseEventArgs iArgs)
        {
            FilterRectangle.Visibility = Visibility.Visible;
        }

        //============================================================================
        //! カーソルが項目から出た時の処理
        private void _MouseLeave(object iSender, MouseEventArgs iArgs)
        {
            FilterRectangle.Visibility = Visibility.Hidden;
        }

        /************************************************************************/
        /* 変数定義                                                             */
        /************************************************************************/

        /// <summary>
        /// メンバーリスト
        /// </summary>
        private List<int> mMemberList = new List<int>();

        /// <summary>
        /// メンバー表示がクローズか
        /// </summary>
        private bool mCloseFlg = false;
	}
}
