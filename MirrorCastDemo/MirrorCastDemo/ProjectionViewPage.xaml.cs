using SecondaryViewsHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.UI.Core;
using Windows.UI.ViewManagement;

namespace MirrorCastDemo
{
    internal class ProjectionViewPageInitializationData
    {
        public CoreDispatcher MainDispatcher;
        public ViewLifetimeControl ProjectionViewPageControl;
        public int MainViewId;
    }

    /// <summary>
    /// Interaction logic for ProjectionViewPage.xaml
    /// </summary>
    public partial class ProjectionViewPage : Window
    {
        ViewLifetimeControl thisViewControl;
        CoreDispatcher mainDispatcher;
        int mainViewId;

        public ProjectionViewPage()
        {
            InitializeComponent();
            this.Loaded += ProjectionViewPage_Loaded;
            
        }

        private void ProjectionViewPage_Loaded(object sender, RoutedEventArgs e)
        {
            var initData = (ProjectionViewPageInitializationData)e.Source;  // TODO: work?

            // The ViewLifetimeControl is a convenient wrapper that ensures the
            // view is closed only when the user is done with it
            thisViewControl = initData.ProjectionViewPageControl;
            mainDispatcher = initData.MainDispatcher;
            mainViewId = initData.MainViewId;

            // Listen for when it's time to close this view
            thisViewControl.Released += thisViewControl_Released;
        }

        private async void thisViewControl_Released(object sender, EventArgs e)
        {
            // There are two major cases where this event will get invoked:
            // 1. The view goes unused for some time, and the system cleans it up
            // 2. The app calls "StopProjectingAsync"
            thisViewControl.Released -= thisViewControl_Released;
            await mainDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MainPage.Current.ProjectionViewPageControl = null;
            });
            this.Close();
        }

        private async void SwapViews_Click(object sender, RoutedEventArgs e)
        {
            // The view might arrive on the wrong display. The user can
            // easily swap the display on which the view appears
            thisViewControl.StartViewInUse();
            await ProjectionManager.SwapDisplaysForViewsAsync(
                ApplicationView.GetForCurrentView().Id,
                mainViewId
            );
            thisViewControl.StopViewInUse();
        }

        private async void StopProjecting_Click(object sender, RoutedEventArgs e)
        {
            // There may be cases to end the projection from the projected view
            // (e.g. the presentation hosted in that view concludes)
            thisViewControl.StartViewInUse();
            await ProjectionManager.StopProjectingAsync(
                ApplicationView.GetForCurrentView().Id,
                mainViewId);
            thisViewControl.StopViewInUse();
        }
    }
}
