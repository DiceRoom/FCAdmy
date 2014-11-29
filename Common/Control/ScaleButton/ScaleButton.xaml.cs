using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// ScaleButton.xaml の相互作用ロジック
	/// </summary>
	public partial class ScaleButton : UserControl
	{
		/************************************************************************/
		/* 基本処理                                                             */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public ScaleButton()
		{
			InitializeComponent();
		}

		/************************************************************************/
		/* プロパティ設定                                                       */
		/************************************************************************/

		/// <summary>
		/// ボタンイメージプロパティ
		/// </summary>
		public static readonly DependencyProperty ButtonImageProperty =
			DependencyProperty.Register("ButtonImageSource",			// プロパティ名
										typeof(ImageSource),			// プロパティの型
										typeof(ScaleButton),　			// コントロールの型
										new FrameworkPropertyMetadata(  // メタデータ                    
											null,
											new PropertyChangedCallback(_ChangeButtonImage)));
		
		/// <summary>
		/// 依存プロパティのラッパー
		/// </summary>
		public ImageSource ButtonImageSource
		{
			get { return (ImageSource)GetValue(ButtonImageProperty); }
			set { SetValue(ButtonImageProperty, value); }
		}

		/// <summary>
		/// ボタンイメージプロパティ
		/// </summary>
		public static readonly DependencyProperty ScaleProperty =
			DependencyProperty.Register("AnimScale",					// プロパティ名
										typeof(double),					// プロパティの型
										typeof(ScaleButton),　			// コントロールの型
										new FrameworkPropertyMetadata(  // メタデータ                    
											1.0,
											new PropertyChangedCallback(_ChangeScale)));

		/// <summary>
		/// 依存プロパティのラッパー
		/// </summary>
		public double AnimScale
		{
			get { return (double)GetValue(ScaleProperty); }
			set { SetValue(ScaleProperty, value); }
		}

		/************************************************************************/
		/* コールバック処理                                                     */
		/************************************************************************/

		//============================================================================
		//! 依存プロパティが変更されたときの処理
		private static void _ChangeButtonImage(DependencyObject iSender, DependencyPropertyChangedEventArgs iArgs)
		{
			var control = iSender as ScaleButton;
			if (control != null)
			{
				var image_Source = (ImageSource)iArgs.NewValue;

				//設定
				control.mBaseWidth = image_Source.Width;
				control.mBaseHeight = image_Source.Height;

				//イメージサイズとイメージ本体変更
				control.ButtonImage.Width = control.mBaseWidth;
				control.ButtonImage.Height = control.mBaseHeight;
				control.ButtonImage.Source = image_Source;

				//コントロールサイズ変更
				control.Width = control.mBaseWidth * control.mScale * 1.2;
				control.Height = control.mBaseHeight * control.mScale * 1.2;
			}
		}

		//============================================================================
		//! 依存プロパティが変更されたときの処理
		private static void _ChangeScale(DependencyObject iSender, DependencyPropertyChangedEventArgs iArgs)
		{
			var control = iSender as ScaleButton;
			if (control != null)
			{
				//コントロールサイズ変更
				control.mScale = (double)iArgs.NewValue;
				control.Width = control.mBaseWidth * control.mScale * 1.2;
				control.Height = control.mBaseHeight * control.mScale * 1.2;
			}
		}

		//============================================================================
		//! カーソルがボタン内に入った
		private void _MouseEnter(object iSender, MouseEventArgs iArgs)
		{
			/*
			//拡大アニメーション
			mStoryboard = new Storyboard();
			var anim_List = _CreateAnimation(1, mScale , 60);
			mStoryboard.Children.Add(anim_List[0]);
			mStoryboard.Children.Add(anim_List[1]);
			ButtonImage.BeginStoryboard(mStoryboard, HandoffBehavior.SnapshotAndReplace);
			*/

			ButtonImage.Width = mBaseWidth * mScale;
			ButtonImage.Height = mBaseHeight * mScale;

		}

		//============================================================================
		//! カーソルがボタン内から出た
		private void _MouseLeave(object iSender, MouseEventArgs iArgs)
		{
			/*if(mStoryboard != null)
			{
				mStoryboard.Stop();
				mStoryboard = null;
			}

			//縮小アニメーション
			
			var storyboard = new Storyboard();
			var anim_List = _CreateAnimation(mScale, 1, 30);
			storyboard.Children.Add(anim_List[0]);
			storyboard.Children.Add(anim_List[1]);
			ButtonImage.BeginStoryboard(storyboard, HandoffBehavior.SnapshotAndReplace);*/
			ButtonImage.Width = mBaseWidth;
			ButtonImage.Height = mBaseHeight;
		}

		/************************************************************************/
		/* 内部定義		                                                        */
		/************************************************************************/

		//============================================================================
		//! 拡縮アニメーション作成
		private DoubleAnimation[] _CreateAnimation(double iFromScale, double iToScale, int iTimeSpan)
		{
			var animation_List = new DoubleAnimation[2];
			animation_List[0] = new DoubleAnimation
			{
				From = mBaseWidth * iFromScale,
				To = mBaseWidth * iToScale,
				Duration = TimeSpan.FromMilliseconds(iTimeSpan)
			};
			Storyboard.SetTargetProperty(animation_List[0], new PropertyPath("Width"));

			animation_List[1] = new DoubleAnimation
			{
				From = mBaseHeight * iFromScale,
				To = mBaseHeight * iToScale,
				Duration = TimeSpan.FromMilliseconds(iTimeSpan)
			};
			Storyboard.SetTargetProperty(animation_List[1], new PropertyPath("Height"));

			return animation_List;
		}

		/************************************************************************/
		/* 変数定義                                                             */
		/************************************************************************/

		/// <summary>
		/// 通常横幅
		/// </summary>
		private double mBaseWidth;

		/// <summary>
		/// 通常縦幅
		/// </summary>
		private double mBaseHeight;

		/// <summary>
		/// 拡大倍率
		/// </summary>
		private double mScale = 1.4;
	}
}
