using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Pivot;
using System.Windows.Browser;
using System.Text.RegularExpressions;

namespace BL_PPivotPiewer
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            string collection = App.Current.Host.InitParams["collection"].ToString();
            MainPivotViewer.LoadCollection(collection + "?giud=" + getGuid(), "$view$=2&$facet0$=Species");
        }

        private string getGuid()
        {
            var random = new Random(System.DateTime.Now.Millisecond);
            int randomNumber = random.Next(1, 5000000);
            return randomNumber.ToString();
        }
        private void PivotViewerControl_ItemDoubleClicked(object sender, System.Windows.Pivot.ItemEventArgs e)
        {

            PivotItem piv_item = MainPivotViewer.GetItem(e.ItemId);
            if (!string.IsNullOrWhiteSpace(piv_item.Href))
            {
                MainPivotViewer.CurrentItemId = e.ItemId;

                OpenLink(piv_item.Href);
            }
            else
            {
                MessageBox.Show("No Web Page...");
            }

        }
        private void OpenLink(string linkUri)
        {
            HtmlPage.Window.Navigate(new Uri(linkUri, UriKind.Absolute), "_blank");
        }

        private void PivotViewerControl_LinkClicked(object sender, System.Windows.Pivot.LinkEventArgs e)
        {

            string url = e.Link.ToString();
            if (!string.IsNullOrWhiteSpace(url))
            {
                OpenLink(url);
            }
            else
            {
                MessageBox.Show("No Web Page...");
            }

        }

        private bool validFacet(string facetName)
        {
            if (facetName == "RefCount" || facetName == "References" || facetName == "SubmissionDate" ||
                facetName == "Description")
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        private void ShowItemTable(IList<string> items)
        {
            string tblTxt = "";
            string facet = "";
            int c = 0;
            foreach (string item in items)
            {
                PivotItem pivotItem = MainPivotViewer.GetItem(item);
                if (c == 0)
                {
                    foreach (KeyValuePair<string, IList<string>> kvp in pivotItem.Facets)
                    {
                        if (validFacet(kvp.Key))
                        {
                            tblTxt += kvp.Key + "\t";
                        }
                    }
                    tblTxt += "\n";
                }

                foreach (KeyValuePair<string, IList<string>> kvp in pivotItem.Facets)
                {

                    if (validFacet(kvp.Key))
                    {
                        string val = String.Join(", ", ((List<string>)kvp.Value).ToArray());
                        string[] vals = Regex.Split(val, "[|]{2}");
                        if (vals.Length > 1)
                        {
                            facet = vals[1];
                        }
                        else
                        {
                            facet = vals[0];
                        }
                        tblTxt += facet + "\t";
                    }
                }
                tblTxt += "\n";
                c += 1;
            }

            ChildWindow childWin = new ChildWindow();
            childWin.Width = 800;
            childWin.Height = 400;
            childWin.Title = "BL!P Output in Tablular Format";
            TextBox tb = new TextBox();
            tb.Text = tblTxt;
            childWin.Content = tb;
            tb.IsReadOnly = true;
            tb.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            tb.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            childWin.Show();
        }

        private void ShowSequences(IList<string> items)
        {
            string tblTxt = "";
            string facet = "";
            int c = 0;
            foreach (string item in items)
            {
                PivotItem pivotItem = MainPivotViewer.GetItem(item);
                string queryName = "";
                string querySequence = "";
                foreach (KeyValuePair<string, IList<string>> kvp in pivotItem.Facets)
                {

                    if (kvp.Key == "QueryName")
                    {
                        string val = String.Join(", ", ((List<string>)kvp.Value).ToArray());
                        string[] vals = Regex.Split(val, "[|]{2}");
                        if (vals.Length > 1)
                        {
                            facet = vals[1];
                        }
                        else
                        {
                            facet = vals[0];
                        }
                        queryName = facet;
                    }
                    if (kvp.Key == "QuerySequence")
                    {
                        string val = String.Join(", ", ((List<string>)kvp.Value).ToArray());
                        string[] vals = Regex.Split(val, "[|]{2}");
                        if (vals.Length > 1)
                        {
                            facet = vals[1];
                        }
                        else
                        {
                            facet = vals[0];
                        }
                        querySequence = facet;
                    }
                }
                tblTxt += ">" + queryName + "\n" + querySequence + "\n";
                c += 1;
            }
            ChildWindow childWin = new ChildWindow();
            childWin.Width = 800;
            childWin.Height = 400;
            childWin.Title = "BL!P Query Sequences in FASTA Format";
            TextBox tb = new TextBox();
            tb.Text = tblTxt;
            childWin.Content = tb;
            tb.IsReadOnly = true;
            tb.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            tb.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            childWin.Show();
        }
        private void OnItemsInViewClicked(object sender, RoutedEventArgs e)
        {
            var itemsInView = MainPivotViewer.InScopeItemIds;
            ShowItemTable(itemsInView.ToList());
        }

        private void OnItemsInViewClickedSequences(object sender, RoutedEventArgs e)
        {
            var itemsInView = MainPivotViewer.InScopeItemIds;
            ShowSequences(itemsInView.ToList());
        }

        private void OnAllInViewClicked(object sender, RoutedEventArgs e)
        {
            IList<string> items = new List<string>();
            for (int i = 0; i < MainPivotViewer.CollectionItemCount; i++){
                items.Add(i.ToString());
            }
            //MessageBox.Show(items.Count.ToString());
            ShowItemTable(items);
        }
    }
}
