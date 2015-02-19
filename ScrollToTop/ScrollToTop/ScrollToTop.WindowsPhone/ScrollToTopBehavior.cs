﻿using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace ScrollToTop
{
    public class ScrollToTopBehavior : DependencyObject, IBehavior
    {
        public Button ScrollToTopButton
        {
            get { return (Button)GetValue(ScrollToTopButtonProperty); }
            set { SetValue(ScrollToTopButtonProperty, value); }
        }

        public static readonly DependencyProperty ScrollToTopButtonProperty =
            DependencyProperty.Register("ScrollToTopButton", typeof(Button), typeof(ScrollToTopBehavior), new PropertyMetadata(null));

        private DependencyObject _associatedObject;
        private ScrollViewer scrollviewer;
        private ScrollBar verticalScrollBar;
        private List<double> offsets;
        private int count;
        private bool isHidden;
        private Button scrollToTopButton;

        DependencyObject IBehavior.AssociatedObject
        {
            get
            {
                return _associatedObject;
            }
        }

        public void Attach(DependencyObject associatedObject)
        {
            offsets = new List<double>();
            isHidden = true;

            if (!DesignMode.DesignModeEnabled)
            {
                _associatedObject = associatedObject;

                scrollviewer = _associatedObject as ScrollViewer;

                if (scrollviewer != null)
                {
                    scrollviewer.Loaded += Scrollviewer_Loaded;
                    scrollviewer.Unloaded += Scrollviewer_Unloaded;
                }
            }
        }

        private void Scrollviewer_Unloaded(object sender, RoutedEventArgs e)
        {
            if (scrollToTopButton != null)
            {
                scrollToTopButton.Tapped -= ScrollToTopButton_Tapped;
                scrollToTopButton = null;
            }

            if (verticalScrollBar != null)
            {
                verticalScrollBar.ValueChanged -= VerticalScrollBar_ValueChanged;
                verticalScrollBar = null;
            }
        }

        public void Detach()
        {
            if (scrollviewer != null)
            {
                scrollviewer.Loaded -= Scrollviewer_Loaded;
                scrollviewer.Unloaded -= Scrollviewer_Unloaded;
            }
        }

        private void Scrollviewer_Loaded(object sender, RoutedEventArgs e)
        {
            FindParts(scrollviewer);
        }

        private void FindParts(DependencyObject dp)
        {
            if (verticalScrollBar == null && scrollToTopButton == null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dp); i++)
                {
                    var control = VisualTreeHelper.GetChild(dp, i);

                    var frameworkElement = control as FrameworkElement;
                    if (frameworkElement != null)
                    {
                        if (frameworkElement.Name == "VerticalScrollBar")
                        {
                            verticalScrollBar = frameworkElement as ScrollBar;
                            verticalScrollBar.ValueChanged += VerticalScrollBar_ValueChanged;
                        }
                        else if (frameworkElement.Name == "GoToTopButton")
                        {
                            scrollToTopButton = frameworkElement as Button;
                            scrollToTopButton.Tapped += ScrollToTopButton_Tapped;
                        }
                        else
                        {
                            FindParts(control);
                        }
                    }
                    else
                    {
                        FindParts(control);
                    }
                }
            }
            else
            {
                return;
            }
        }

        private void ScrollToTopButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            scrollviewer.ChangeView(null, 0, null);
        }

        private void VerticalScrollBar_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var offset = e.NewValue as double?;

            if (offset.HasValue)
            {
                Debug.WriteLine(offset.Value);
                offsets.Add(offset.Value);
                count = count + 1;

                DetermineVisualstateChange();
            }
        }

        private void DetermineVisualstateChange()
        {
            if (count > 4)
            {
                if (offsets[count - 1] > 250.0 && offsets[count - 2] < offsets[count - 3]
                    && offsets[count - 1] < offsets[count - 2])
                {
                    Debug.WriteLine("############## SHOW BUTTON ############## ");
                    ShowGoToTopButton();
                }
                else if ((offsets[count - 1] > 250.0 && offsets[count - 3] < offsets[count - 2]
                    && offsets[count - 2] < offsets[count - 1]) || offsets[count - 1] < 250.0)
                {
                    Debug.WriteLine("############## HIDE BUTTON ############## ");
                    HideGoTopTopButton();
                }
            }
        }

        private void HideGoTopTopButton()
        {
            if (!isHidden)
            {
                isHidden = true;
                VisualStateManager.GoToState(scrollviewer, "GoToTopHidden", true);
            }
        }

        private void ShowGoToTopButton()
        {
            if (isHidden)
            {
                isHidden = false;
                VisualStateManager.GoToState(scrollviewer, "GoToTopVisible", true);
            }
        }
    }
}
