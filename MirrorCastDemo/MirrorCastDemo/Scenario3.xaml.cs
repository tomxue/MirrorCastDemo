﻿using SecondaryViewsHelpers;
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
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.UI.Core;
using Windows.UI.ViewManagement;

namespace MirrorCastDemo
{
    /// <summary>
    /// Interaction logic for Scenario3.xaml
    /// </summary>
    public partial class Scenario3 : Page
    {
        MainWindow rootPage = MainWindow.Current;
        int thisViewId;

        public Scenario3()
        {
            InitializeComponent();
            thisViewId = GetViewId(this);
        }

        private int GetViewId(Visual visual)
        {
            var source = PresentationSource.FromVisual(visual);
            if (source != null)
            {
                var hwndSource = source as System.Windows.Interop.HwndSource;
                if (hwndSource != null)
                {
                    return hwndSource.Handle.ToInt32();
                }
            }
            return -1; // Return -1 if the view ID cannot be obtained
        }

        private async void LoadAndDisplayScreens_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Use the device selector query of the ProjectionManager to list wired/wireless displays
                String projectorSelectorQuery = ProjectionManager.GetDeviceSelector();

                this.findAndProject_button.IsEnabled = false;

                // Clear the list box
                this.displayList_listview.Items.Clear();
                this.displayList_listview.Visibility = Visibility.Visible;

                rootPage.NotifyUser("Searching for devices...", NotifyType.StatusMessage);

                // Calling the device API to find devices based on the device query
                var outputDevices = await DeviceInformation.FindAllAsync(projectorSelectorQuery);

                // List found devices in the UI
                for (int i = 0; i < outputDevices.Count; i++)
                {
                    var device = outputDevices[i];
                    this.displayList_listview.Items.Add(device);
                }

                this.findAndProject_button.IsEnabled = true;
                rootPage.NotifyUser("Found devices are now listed.", NotifyType.StatusMessage);
            }
            catch (InvalidOperationException)
            {
                rootPage.NotifyUser("An error occured when querying and listing devices.", NotifyType.ErrorMessage);
            }
        }

        private async void StartProjecting(DeviceInformation selectedDisplay)
        {
            // If projection is already in progress, then it could be shown on the monitor again
            // Otherwise, we need to create a new view to show the presentation
            if (rootPage.ProjectionViewPageControl == null)
            {
                //Application.Current.MainWindow.Activate();
                // First, create a new, blank view
                //var thisDispatcher = Application.Current.MainWindow.Dispatcher;
                var thisDispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                await CoreApplication.CreateNewView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    // ViewLifetimeControl is a wrapper to make sure the view is closed only
                    // when the app is done with it
                    rootPage.ProjectionViewPageControl = ViewLifetimeControl.CreateForCurrentView();

                    // Assemble some data necessary for the new page
                    var initData = new ProjectionViewPageInitializationData();
                    initData.MainDispatcher = thisDispatcher;

                    initData.ProjectionViewPageControl = rootPage.ProjectionViewPageControl;
                    initData.MainViewId = thisViewId;

                    // Display the page in the view. Note that the view will not become visible
                    // until "StartProjectingAsync" is called
                    var rootFrame = new Frame();
                    rootFrame.Navigate(typeof(ProjectionViewPage), initData);
                    this.Content = rootFrame;

                    // The call to Window.Current.Activate is required starting in Windos 10.
                    // Without it, the view will never appear.
                    //this.Activate();
                    //Window.Current.Activate();
                });
            }

            try
            {
                // Start/StopViewInUse are used to signal that the app is interacting with the
                // view, so it shouldn't be closed yet, even if the user loses access to it
                rootPage.ProjectionViewPageControl.StartViewInUse();

                // Show the view on a second display that was selected by the user
                rootPage.NotifyUser("Starting projection on " + selectedDisplay.Name, NotifyType.StatusMessage);
                await ProjectionManager.StartProjectingAsync(rootPage.ProjectionViewPageControl.Id, thisViewId, selectedDisplay);

                rootPage.NotifyUser("Projection started with success on " + selectedDisplay.Name, NotifyType.StatusMessage);
                rootPage.ProjectionViewPageControl.StopViewInUse();
            }
            catch (InvalidOperationException)
            {
                rootPage.NotifyUser("Start projection failed", NotifyType.ErrorMessage);
            }
        }

        private void displayList_listview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the DeviceInformation object of selected ListView item
            ListView devicesList = (ListView)sender;
            if (devicesList.SelectedItem != null)
            {
                DeviceInformation selectedDevice = (DeviceInformation)devicesList.SelectedItem;

                // Start projecting to the selected display
                StartProjecting(selectedDevice);
            }
            this.displayList_listview.Visibility = Visibility.Collapsed;
        }
    }
}
