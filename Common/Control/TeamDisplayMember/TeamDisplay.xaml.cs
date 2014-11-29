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
    /// TeamDisplay.xaml の相互作用ロジック
    /// </summary>
    public partial class TeamDisplay : UserControl
    {
        /************************************************************************/
        /* 基本処理                                                             */
        /************************************************************************/

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TeamDisplay()
        {
            InitializeComponent();
        }

        /************************************************************************/
        /* アクセサ                                                             */
        /************************************************************************/

        /// <summary>
        /// 参加メンバーを表としてコントロールを生成する
        /// </summary>
        /// <param name="iMemberList"></param>
        public void SetJoinMemberList(List<BattleManager.SBattleJoinInfo> iMemberList)
        {
            //まずはコントロールを作成
            var control_List = new List<TeamDisplayMember>();
            int color_Index = 0;
            var color_List = new Brush[2] { new SolidColorBrush(SystemUtility.StringToColor("#55000000")), new SolidColorBrush(SystemUtility.StringToColor("#55555555")) };
            var margin = new Thickness();
            MemberGrid.Children.Clear();
            foreach (var iMember in iMemberList)
            {
                var control = new TeamDisplayMember();
                control.Margin = margin;
                control.Background = color_List[color_Index % 2];
                control.SetJoinMemberInfo(control_List.Count, iMember);
                control_List.Add(control);
                MemberGrid.Children.Add(control);

                margin.Top += control.Height;
                ++color_Index;
            }

            //コントロール分だけ自分のコントロールのサイズを変更
            MemberGrid.Width = control_List[0].Width;
            Width = MemberGrid.Width + 44;
            MemberGrid.Height = (control_List[0].Height * control_List.Count);
            Height = MemberGrid.Height + IndexGrid.Height + 44;
        }
    }
}
