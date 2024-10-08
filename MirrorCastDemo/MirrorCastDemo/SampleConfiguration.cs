using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MirrorCastDemo
{
    public partial class MainPage : Page
    {
        public const string FEATURE_NAME = "Projection";

        List<Scenario> scenarios = new List<Scenario>
        {
            new Scenario() { Title="Custom display selection and project", ClassType=typeof(Scenario3)}
        };

        // Keep track of the view that's being projected
        public SecondaryViewsHelpers.ViewLifetimeControl ProjectionViewPageControl;
    }

    public class Scenario
    {
        public string Title { get; set; }
        public Type ClassType { get; set; }
    }
}
