﻿using BrainSimulator.Modules;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BrainSimulator
{
    public partial class ModuleView : DependencyObject
    {
        public static readonly DependencyProperty AreaNumberProperty =
    DependencyProperty.Register("AreaNumber", typeof(int), typeof(MenuItem));

        public static void CreateContextMenu(int i, ModuleView nr, FrameworkElement r, ContextMenu cm = null) //for a selection
        {
            cmCancelled = false;
            if (cm == null)
                cm = new ContextMenu();
            cm.SetValue(AreaNumberProperty, i);
            cm.PreviewKeyDown += Cm_PreviewKeyDown;

            StackPanel sp;
            MenuItem mi = new MenuItem();
            mi = new MenuItem();
            mi.Header = "Delete";
            mi.Click += Mi_Click;
            cm.Items.Add(mi);

            mi = new MenuItem();
            mi.Header = "Initialize";
            mi.Click += Mi_Click;
            cm.Items.Add(mi);

            mi = new MenuItem();
            mi.Header = "View Source";
            mi.Click += Mi_Click;
            cm.Items.Add(mi);

            mi = new MenuItem();
            mi.Header = "Info...";
            mi.Click += Mi_Click;
            cm.Items.Add(mi);

            mi = new MenuItem();
            mi.Header = new CheckBox { Name = "Enabled", Content = "Enabled", IsChecked = nr.TheModule.isEnabled, };
            mi.StaysOpenOnClick = true;
            cm.Items.Add(mi);

            sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 3, 3, 3) };
            sp.Children.Add(new Label { Content = "Width: ", VerticalAlignment = VerticalAlignment.Center, Padding = new Thickness(0) });
            TextBox tb0 = new TextBox { Text = nr.Width.ToString(), Width = 60, Name = "AreaWidth", VerticalAlignment = VerticalAlignment.Center };
            tb0.TextChanged += TextChanged;
            sp.Children.Add(tb0);
            sp.Children.Add(new Label { Content = "Height: " });
            TextBox tb1 = new TextBox { Text = nr.Height.ToString(), Width = 60, Name = "AreaHeight", VerticalAlignment = VerticalAlignment.Center };
            tb1.TextChanged += TextChanged;
            sp.Children.Add(tb1);
            cm.Items.Add(new MenuItem { Header = sp, StaysOpenOnClick = true });

            sp = new StackPanel { Orientation = Orientation.Horizontal };
            sp.Children.Add(new Label { Content = "Type: ", Padding = new Thickness(0) });
            string[] parts = nr.TheModule.GetType().ToString().Split('.');
            string moduleType = parts[2];
            sp.Children.Add(new Label { Content = moduleType, Padding = new Thickness(0) });
            cm.Items.Add(new MenuItem { Header = sp, StaysOpenOnClick = true });

            sp = new StackPanel { Orientation = Orientation.Horizontal };
            sp.Children.Add(new Label { Content = "Name: ", Padding = new Thickness(0) });
            sp.Children.Add(new TextBox { Text = nr.Label, Width = 140, Name = "AreaName", Padding = new Thickness(0) });
            cm.Items.Add(new MenuItem { Header = sp, StaysOpenOnClick = true });

            //color picker
            Color c = Utils.IntToColor(nr.Color);
            ComboBox cb = new ComboBox();
            cb.Width = 200;
            cb.Name = "AreaColor";
            PropertyInfo[] x1 = typeof(Colors).GetProperties();
            int sel = -1;
            for (int i1 = 0; i1 < x1.Length; i1++)
            {
                Color cc = (Color)ColorConverter.ConvertFromString(x1[i1].Name);
                if (cc == c)

                {
                    sel = i1;
                    break;
                }
            }
            if (nr.Color == 0) sel = 3;
            foreach (PropertyInfo s in x1)
            {
                ComboBoxItem cbi = new ComboBoxItem()
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(s.Name)),
                    Content = s.Name
                };
                Rectangle r1 = new Rectangle()
                {
                    Width = 20,
                    Height = 20,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(s.Name)),
                    Margin = new Thickness(0, 0, 140, 0),
                };
                Grid g = new Grid();
                g.Children.Add(r1);
                g.Children.Add(new Label() { Content = s.Name, Margin = new Thickness(25, 0, 0, 0) });
                cbi.Content = g;
                cbi.Width = 200;
                cb.Items.Add(cbi);
            }
            cb.SelectedIndex = sel;
            cm.Items.Add(new MenuItem { Header = cb, StaysOpenOnClick = true });

            if (MainWindow.theNeuronArray.Modules[i].TheModule != null)
            {
                var t = MainWindow.theNeuronArray.Modules[i].TheModule.GetType();
                Type t1 = Type.GetType(t.ToString() + "Dlg");
                while (t1 == null && t.BaseType.Name != "ModuleBase")
                {
                    t = t.BaseType;
                    t1 = Type.GetType(t.ToString() + "Dlg");
                }
                if (t1 != null)
                {
                    cm.Items.Add(new MenuItem { Header = "Show Dialog" });
                    ((MenuItem)cm.Items[cm.Items.Count - 1]).Click += Mi_Click;
                }
            }

            if (nr.TheModule.CustomContextMenuItems() is MenuItem miCustom)
            {
                cm.Items.Add(miCustom);
            }

            sp = new StackPanel { Orientation = Orientation.Horizontal };
            Button b0 = new Button { Content = "OK", Width = 100, Height = 25, Margin = new Thickness(10) };
            b0.Click += B0_Click;
            sp.Children.Add(b0);
            b0 = new Button { Content = "Cancel", Width = 100, Height = 25, Margin = new Thickness(10) };
            b0.Click += B0_Click;
            sp.Children.Add(b0);

            cm.Items.Add(new MenuItem { Header = sp, StaysOpenOnClick = true });

            cm.Closed += Cm_Closed;
        }

        private static void Cm_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            ContextMenu cm = sender as ContextMenu;
            if (e.Key == Key.Enter)
            {
                Cm_Closed(sender, e);
            }
        }

        private static void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
                if (tb.Parent is StackPanel sp)
                    if (sp.Parent is MenuItem mi)
                        if (mi.Parent is ContextMenu cm)
                        {
                            if (tb.Name == "AreaWidth")
                            {
                                int i = (int)cm.GetValue(AreaNumberProperty);
                                ModuleView theModuleView = MainWindow.theNeuronArray.modules[i];
                                MainWindow.theNeuronArray.GetNeuronLocation(MainWindow.theNeuronArray.modules[i].firstNeuron, out int col, out int row);
                                if (!float.TryParse(tb.Text, out float width))
                                    tb.Background = new SolidColorBrush(Colors.Pink);
                                else
                                {
                                    if (width < theModuleView.TheModule.MinWidth)
                                        tb.Background = new SolidColorBrush(Colors.Pink);
                                    else
                                    {
                                        if (width + col > MainWindow.theNeuronArray.Cols)
                                            tb.Background = new SolidColorBrush(Colors.Pink);
                                        else
                                            tb.Background = new SolidColorBrush(Colors.LightGreen);

                                    }
                                }
                            }
                            if (tb.Name == "AreaHeight")
                            {
                                int i = (int)cm.GetValue(AreaNumberProperty);
                                ModuleView theModuleView = MainWindow.theNeuronArray.modules[i];
                                MainWindow.theNeuronArray.GetNeuronLocation(MainWindow.theNeuronArray.modules[i].firstNeuron, out int col, out int row);
                                if (!float.TryParse(tb.Text, out float height))
                                    tb.Background = new SolidColorBrush(Colors.Pink);
                                else
                                {
                                    if (height < theModuleView.TheModule.MinHeight)
                                        tb.Background = new SolidColorBrush(Colors.Pink);
                                    else
                                    {
                                        if (height + row > MainWindow.theNeuronArray.rows)
                                            tb.Background = new SolidColorBrush(Colors.Pink);
                                        else
                                            tb.Background = new SolidColorBrush(Colors.LightGreen);

                                    }
                                }
                            }
                        }
        }

        static bool cmCancelled = false;
        private static void B0_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                if (b.Parent is StackPanel sp)
                {
                    if (sp.Parent is MenuItem mi)
                    {
                        if (mi.Parent is ContextMenu cm)
                        {
                            if ((string)b.Content == "Cancel")
                                cmCancelled = true;
                            Cm_Closed(cm, e);
                        }
                    }
                }
            }
        }


        static bool deleted = false;
        private static void Cm_Closed(object sender, RoutedEventArgs e)
        {
            if ((Keyboard.GetKeyStates(Key.Escape) & KeyStates.Down) > 0)
            {
                MainWindow.Update();
                return;
            }
            if (deleted)
            {
                deleted = false;
            }
            else if (sender is ContextMenu cm)
            {
                if (!cm.IsOpen) return;
                cm.IsOpen = false;
                if (cmCancelled) return;

                int i = (int)cm.GetValue(AreaNumberProperty);
                string label = "";
                string theModuleTypeStr = "";
                Color color = Colors.Wheat;
                int width = 1, height = 1;

                Control cc = Utils.FindByName(cm, "AreaName");
                if (cc is TextBox tb)
                    label = tb.Text;
                cc = Utils.FindByName(cm, "Enabled");
                bool isEnabled = true;
                if (cc is CheckBox cb2)
                    isEnabled = (bool)cb2.IsChecked;

                cc = Utils.FindByName(cm, "AreaWidth");
                if (cc is TextBox tb1)
                    int.TryParse(tb1.Text, out width);
                cc = Utils.FindByName(cm, "AreaHeight");
                if (cc is TextBox tb2)
                    int.TryParse(tb2.Text, out height);
                cc = Utils.FindByName(cm, "AreaType");
                if (cc is ComboBox cb && cb.SelectedValue != null)
                {
                    theModuleTypeStr = "Module" + (string)cb.SelectedValue;
                    if (theModuleTypeStr == "") return;//something went wrong
                    label = (string)cb.SelectedValue;
                }

                cc = Utils.FindByName(cm, "AreaColor");
                if (cc is ComboBox cb1)
                    color = ((SolidColorBrush)((ComboBoxItem)cb1.SelectedValue).Background).Color;
                if (label == "" && theModuleTypeStr == "") return;

                ModuleView theModuleView = MainWindow.theNeuronArray.modules[i];
                MainWindow.theNeuronArray.SetUndoPoint();
                MainWindow.theNeuronArray.AddModuleUndo(i, theModuleView);
                //update the existing module
                theModuleView.Label = label;
                theModuleView.ModuleTypeStr = theModuleTypeStr;
                theModuleView.Color = Utils.ColorToInt(color);
                theModuleView.TheModule.isEnabled = isEnabled;

                //did we change the module type?
                Type t1x = Type.GetType("BrainSimulator.Modules." + theModuleTypeStr);
                if (t1x != null && (MainWindow.theNeuronArray.modules[i].TheModule == null || MainWindow.theNeuronArray.modules[i].TheModule.GetType() != t1x))
                {
                    MainWindow.theNeuronArray.modules[i].TheModule = (ModuleBase)Activator.CreateInstance(t1x);
                    MainWindow.theNeuronArray.modules[i].label = theModuleTypeStr;
                }

                MainWindow.theNeuronArray.GetNeuronLocation(MainWindow.theNeuronArray.modules[i].firstNeuron, out int col, out int row);
                if (width < theModuleView.TheModule.MinWidth) width = theModuleView.TheModule.MinWidth;
                if (height < theModuleView.TheModule.MinHeight) height = theModuleView.TheModule.MinHeight;

                bool dimsChanged = false;

                if (width + col > MainWindow.theNeuronArray.Cols)
                {
                    width = MainWindow.theNeuronArray.Cols - col;
                    dimsChanged = true;
                }
                if (height + row > MainWindow.theNeuronArray.rows)
                {
                    height = MainWindow.theNeuronArray.rows - row;
                    dimsChanged = true;
                }
                if (dimsChanged)
                    MessageBox.Show("Dimensions reduced to stay within neuron array bondary.", "Warning", MessageBoxButton.OK);
                theModuleView.Width = width;
                theModuleView.Height = height;
            }
            MainWindow.Update();
        }

        public static void CreateModule(string label, string commandLine, Color color, int firstNeuron, int width, int height)
        {
            ModuleView mv = new ModuleView(firstNeuron, width, height, label, commandLine, Utils.ColorToInt(color));

            if (mv.Width < mv.theModule.MinWidth) mv.Width = mv.theModule.MinWidth;
            if (mv.Height < mv.theModule.MinHeight) mv.Height = mv.theModule.MinHeight;
            MainWindow.theNeuronArray.modules.Add(mv);
            string[] Params = commandLine.Split(' ');
            Type t1x = Type.GetType("BrainSimulator.Modules." + Params[0]);
            if (t1x != null && (mv.TheModule == null || mv.TheModule.GetType() != t1x))
            {
                mv.TheModule = (ModuleBase)Activator.CreateInstance(t1x);
                //  MainWindow.theNeuronArray.areas[i].TheModule.Initialize();
            }
        }

        private static void Mi_Click(object sender, RoutedEventArgs e)
        {
            //Handle delete  & initialize commands
            if (sender is MenuItem mi)
            {
                if (mi.Header is StackPanel sp && sp.Children[0] is Label l && l.Content.ToString().StartsWith("Random"))
                {
                    if (sp.Children[1] is TextBox tb0)
                    {
                        if (int.TryParse(tb0.Text, out int count))
                        {
                            MainWindow.arrayView.CreateRandomSynapses(count);
                            MainWindow.theNeuronArray.ShowSynapses = true;
                            MainWindow.thisWindow.SetShowSynapsesCheckBox(true);
                            MainWindow.Update();
                        }
                    }
                    return;
                }
                if ((string)mi.Header == "Cut")
                {
                    MainWindow.arrayView.CutNeurons();
                    MainWindow.Update();
                }
                if ((string)mi.Header == "Copy")
                {
                    MainWindow.arrayView.CopyNeurons();
                }
                if ((string)mi.Header == "Clear Selection")
                {
                    MainWindow.arrayView.ClearSelection();
                    MainWindow.Update();
                }
                if ((string)mi.Header == "Mutual Suppression")
                {
                    MainWindow.arrayView.MutualSuppression();
                    MainWindow.theNeuronArray.ShowSynapses = true;
                    MainWindow.thisWindow.SetShowSynapsesCheckBox(true);
                    MainWindow.Update();
                }
                if ((string)mi.Header == "Delete")
                {
                    int i = (int)mi.Parent.GetValue(AreaNumberProperty);
                    if (i < 0)
                    {
                        MainWindow.arrayView.DeleteSelection();
                    }
                    else
                    {
                        MainWindow.theNeuronArray.SetUndoPoint();
                        MainWindow.theNeuronArray.AddModuleUndo(-1, MainWindow.theNeuronArray.modules[i]);
                        DeleteModule(i);
                        deleted = true;
                    }
                }
                if ((string)mi.Header == "Initialize")
                {
                    int i = (int)mi.Parent.GetValue(AreaNumberProperty);
                    if (i < 0)
                    {
                    }
                    else
                    {
                        {
                            try
                            {
                                MainWindow.theNeuronArray.Modules[i].TheModule.Initialize();
                            }
                            catch (Exception e1)
                            {
                                MessageBox.Show("Initialize failed on module " + MainWindow.theNeuronArray.Modules[i].Label + ".   Message: " + e1.Message);
                            }
                        }

                    }
                }
                if ((string)mi.Header == "View Source")
                {
                    int i = (int)mi.Parent.GetValue(AreaNumberProperty);
                    ModuleView m = MainWindow.theNeuronArray.Modules[i];
                    ModuleBase m1 = m.TheModule;
                    string theModuleType = m1.GetType().Name.ToString();
                    string cwd = System.IO.Directory.GetCurrentDirectory();
                    cwd = cwd.ToLower().Replace("bin\\debug\\net6.0-windows", "");
                    string fileName = cwd + @"modules\" + theModuleType + ".cs";
                    if (File.Exists(fileName))
                        OpenSource(fileName);
                    else
                    {
                        fileName = cwd + @"BrainSim2modules\" + theModuleType + ".cs";
                        OpenSource(fileName);
                    }
                }
                if ((string)mi.Header == "Show Dialog")
                {
                    int i = (int)mi.Parent.GetValue(AreaNumberProperty);
                    if (i < 0)
                    {
                    }
                    else
                    {
                        MainWindow.theNeuronArray.Modules[i].TheModule.ShowDialog();
                    }
                }
                if ((string)mi.Header == "Info...")
                {
                    int i = (int)mi.Parent.GetValue(AreaNumberProperty);
                    if (i < 0)
                    {
                    }
                    else
                    {
                        ModuleView m = MainWindow.theNeuronArray.Modules[i];
                        ModuleBase m1 = m.TheModule;
                        string theModuleType = m1.GetType().Name.ToString();
                        ModuleDescriptionDlg md = new ModuleDescriptionDlg(theModuleType);
                        md.ShowDialog();
                    }
                }
                if ((string)mi.Header == "Reset Hebbian Weights")
                {
                    MainWindow.theNeuronArray.SetUndoPoint();
                    foreach (SelectionRectangle sr in MainWindow.arrayView.theSelection.selectedRectangles)
                    {
                        foreach (int Id in sr.NeuronInRectangle())
                        {
                            Neuron n = MainWindow.theNeuronArray.GetNeuron(Id);
                            foreach (Synapse s in n.Synapses)
                            {
                                if (s.model != Synapse.modelType.Fixed)
                                {
                                    //TODO: Add some UI for this:
                                    //s.model = Synapse.modelType.Hebbian2;
                                    n.AddSynapseWithUndo(s.targetNeuron, 0, s.model);
                                    s.Weight = 0;
                                }
                            }
                        }
                    }
                    MainWindow.Update();
                }
            }
        }
        public static void OpenSource(string fileName)
        {
            Process process = new Process();
            string taskFile = @"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe";
            if (!File.Exists(taskFile))
                taskFile = @"C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\devenv.exe";
            if (!File.Exists(taskFile))
                taskFile = @"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe";
            if (!File.Exists(taskFile))
                return;

            ProcessStartInfo startInfo = new ProcessStartInfo(taskFile, "/edit " + fileName);
            process.StartInfo = startInfo;
            process.Start();
        }
        public static void DeleteModule(int i)
        {
            ModuleView mv = MainWindow.theNeuronArray.Modules[i];
            mv.theModule.CloseDlg();
            foreach (Neuron n in mv.Neurons)
            {
                n.Reset();
                n.DeleteAllSynapes();
            }
            MainWindow.theNeuronArray.Modules.RemoveAt(i);
        }
    }
}
