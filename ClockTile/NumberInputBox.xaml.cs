using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClockTile
{
	/// <summary>
	/// NumberInputBox.xaml 的交互逻辑
	/// </summary>
	public partial class NumberInputBox: UserControl
	{
		#region 依赖属性

		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register ("Value", typeof (double), typeof (NumberInputBox),
				new FrameworkPropertyMetadata (0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
					OnValueChanged, CoerceValue));

		public static readonly DependencyProperty MinimumProperty =
			DependencyProperty.Register ("Minimum", typeof (double), typeof (NumberInputBox),
				new PropertyMetadata (0.0));

		public static readonly DependencyProperty MaximumProperty =
			DependencyProperty.Register ("Maximum", typeof (double), typeof (NumberInputBox),
				new PropertyMetadata (100.0));

		public static readonly DependencyProperty IncrementProperty =
			DependencyProperty.Register ("Increment", typeof (double), typeof (NumberInputBox),
				new PropertyMetadata (1.0));

		public static readonly DependencyProperty DecimalPlacesProperty =
			DependencyProperty.Register ("DecimalPlaces", typeof (int), typeof (NumberInputBox),
				new PropertyMetadata (0));

		#endregion

		#region 路由事件

		public static readonly RoutedEvent ValueChangedEvent =
			EventManager.RegisterRoutedEvent ("ValueChanged", RoutingStrategy.Bubble,
				typeof (RoutedEventHandler), typeof (NumberInputBox));

		public event RoutedEventHandler ValueChanged
		{
			add { AddHandler (ValueChangedEvent, value); }
			remove { RemoveHandler (ValueChangedEvent, value); }
		}

		#endregion

		#region 属性

		public double Value
		{
			get { return (double)GetValue (ValueProperty); }
			set { SetValue (ValueProperty, value); }
		}

		public double Minimum
		{
			get { return (double)GetValue (MinimumProperty); }
			set { SetValue (MinimumProperty, value); }
		}

		public double Maximum
		{
			get { return (double)GetValue (MaximumProperty); }
			set { SetValue (MaximumProperty, value); }
		}

		public double Increment
		{
			get { return (double)GetValue (IncrementProperty); }
			set { SetValue (IncrementProperty, value); }
		}

		public int DecimalPlaces
		{
			get { return (int)GetValue (DecimalPlacesProperty); }
			set { SetValue (DecimalPlacesProperty, value); }
		}

		#endregion

		private bool _isUpdatingText = false;

		public NumberInputBox ()
		{
			InitializeComponent ();
		}

		#region 事件处理

		private void InputTextBox_PreviewTextInput (object sender, TextCompositionEventArgs e)
		{
			// 仅允许数字、负号和小数点
			foreach (char c in e.Text)
			{
				if (!Char.IsDigit (c) && c != '-' && c != '.')
				{
					e.Handled = true;
					return;
				}
			}

			// 防止多个负号
			if (e.Text == "-" && InputTextBox.Text.Contains ("-"))
			{
				e.Handled = true;
				return;
			}

			// 防止负号不在开头
			if (e.Text == "-" && InputTextBox.SelectionStart != 0)
			{
				e.Handled = true;
				return;
			}

			// 防止多个小数点
			if (e.Text == "." && InputTextBox.Text.Contains ("."))
			{
				e.Handled = true;
				return;
			}
		}

		private void InputTextBox_TextChanged (object sender, TextChangedEventArgs e)
		{
			if (_isUpdatingText)
				return;
			double value;
			if (Double.TryParse (InputTextBox.Text, out value))
			{
				// 更新依赖属性的值（会自动通过 CoerceValue 进行限制）
				SetValue (ValueProperty, value);
			}
		}

		private void InputTextBox_LostFocus (object sender, RoutedEventArgs e)
		{
			double value;
			// 失去焦点时验证和修正值
			if (String.IsNullOrEmpty (InputTextBox.Text))
			{
				SetValue (ValueProperty, Minimum);
			}
			else if (!Double.TryParse (InputTextBox.Text, out value))
			{
				// 恢复到上一个有效值
				UpdateTextFromValue ();
			}
			else
			{
				// 确保值在有效范围内
				SetValue (ValueProperty, value);
			}
		}

		private void IncreaseButton_Click (object sender, RoutedEventArgs e)
		{
			Value = Math.Min (Value + Increment, Maximum);
		}

		private void DecreaseButton_Click (object sender, RoutedEventArgs e)
		{
			Value = Math.Max (Value - Increment, Minimum);
		}

		#endregion

		#region 私有方法

		private static void OnValueChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			NumberInputBox control = (NumberInputBox)d;
			control.UpdateTextFromValue ();

			// 触发 ValueChanged 路由事件
			RoutedEventArgs args = new RoutedEventArgs (ValueChangedEvent);
			control.RaiseEvent (args);
		}

		private static object CoerceValue (DependencyObject d, object baseValue)
		{
			NumberInputBox control = (NumberInputBox)d;
			double value = (double)baseValue;

			// 限制值在 Min 和 Max 之间
			if (value < control.Minimum)
				return control.Minimum;
			if (value > control.Maximum)
				return control.Maximum;

			return value;
		}

		private void UpdateTextFromValue ()
		{
			_isUpdatingText = true;
			try
			{
				string format = DecimalPlaces > 0 ? "F" + DecimalPlaces : "F0";
				InputTextBox.Text = Value.ToString (format);
			}
			finally
			{
				_isUpdatingText = false;
			}
		}

		#endregion
	}
}