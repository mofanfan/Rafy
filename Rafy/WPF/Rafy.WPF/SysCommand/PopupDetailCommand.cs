﻿/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120415
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120415
 * 
*******************************************************/

using System.Windows;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.MetaModel.Attributes;
using Rafy.WPF;
using Rafy.WPF.Behaviors;
using Rafy.WPF.Controls;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Rafy.WPF.Command
{
    public abstract class PopupDetailCommand : ListViewCommand
    {
        protected WindowButton PopupEditingDialog(WPFEntityViewMeta evm, Entity tmpEntity, Action<ViewDialog> windowSetter)
        {
            //弹出窗体显示详细面板
            var detailView = AutoUI.ViewFactory.CreateDetailView(evm);
            detailView.Data = tmpEntity;

            //使用一个 StackPanel，保证详细面板不会自动变大
            var form = detailView.Control;
            form.VerticalAlignment = VerticalAlignment.Top;
            var ctrl = new StackPanel { Children = { form } };

            var result = App.Windows.ShowDialog(ctrl, w =>
            {
                w.Buttons = ViewDialogButtons.YesNo;
                w.SizeToContent = SizeToContent.Height;
                w.MinHeight = 200;
                w.MinWidth = 400;
                w.Width = 400 * detailView.CalculateColumnsCount();
                w.ValidateOperations += (o, e) =>
                {
                    var broken = tmpEntity.Validate();
                    if (broken.Count > 0)
                    {
                        App.MessageBox.Show(broken.ToString(), "属性错误".Translate());
                        e.Cancel = true;
                    }
                };

                windowSetter(w);

                w.Loaded += (o, e) =>
                {
                    var txt = w.GetVisualChild<TextBox>();
                    if (txt != null) { Keyboard.Focus(txt); }
                };

                //窗口在数据改变后再关闭窗口，需要提示用户是否保存。
                bool changed = false;
                tmpEntity.PropertyChanged += (o, e) => changed = true;
                w.Closing += (o, e) =>
                {
                    if (changed && w.DialogResult != true)
                    {
                        var res = App.MessageBox.Show("直接退出将不会保存数据，是否退出？".Translate(), MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        e.Cancel = res == MessageBoxResult.No;
                    }
                };

                this.OnWindowShowing(w);
            });

            return result;
        }

        public event EventHandler<WindowShowingEventArgs> WindowShowing;

        protected virtual void OnWindowShowing(Window win)
        {
            var handler = this.WindowShowing;
            if (handler != null) handler(this, new WindowShowingEventArgs(win));
        }

        public class WindowShowingEventArgs : EventArgs
        {
            public WindowShowingEventArgs(Window window)
            {
                this.Window = window;
            }

            public Window Window { get; private set; }
        }
    }
}
