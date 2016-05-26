﻿/*
 * [The "BSD license"]
 * Copyright (c) 2016 Peter Hoch
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

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
using System.Windows.Shapes;
using DecisionTableCreator.TestCases;
using DecisionTableCreator.Utils;

namespace DecisionTableCreator
{
    /// <summary>
    /// Interaction logic for EditCondition.xaml
    /// </summary>
    public partial class EditCondition : Window
    {
        public EditCondition(IConditionAction condObject)
        {
            InitializeComponent();
            DataContext = condObject;
        }

        IConditionAction DataContainer { get { return (IConditionAction) DataContext; } }

        private void AppendEnumValue_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DataContainer.EnumValues.Add(new EnumValue("new name", "new value"));
        }

        private void AppendEnumValue_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void DeleteEnumValue_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DataGrid dataGrid = e.Source as DataGrid;
            DependencyObject dep = e.OriginalSource as DependencyObject;
            if (dataGrid != null && dep != null)
            {
                var dataGridRow = WpfTools.SearchForParent(dep, typeof(DataGridRow), false);
                if (dataGridRow != null)
                {
                    int index = dataGrid.ItemContainerGenerator.IndexFromContainer(dataGridRow);
                    if (index >= 0 && index < dataGrid.Items.Count)
                    {
                        DataContainer.EnumValues.RemoveAt(index);
                    }
                }
            }
        }

        private void DeleteEnumValue_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            DataGrid dataGrid = e.Source as DataGrid;
            DependencyObject dep = e.OriginalSource as DependencyObject;
            if (dataGrid != null && dep != null)
            {
                if (dataGrid.Items.Count > 1)
                {
                    var dataGridCell = WpfTools.SearchForParent(dep, typeof(DataGridCell), false);
                    if (dataGridCell != null)
                    {
                        e.CanExecute = true;
                    }
                }
            }
        }

        private void OnOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void OnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
