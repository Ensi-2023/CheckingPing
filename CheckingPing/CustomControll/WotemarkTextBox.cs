using System.Windows;
using System.Windows.Controls;

namespace CheckingPing.CustomControll
{
    [TemplatePart(Name = "PART_Wotemark", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_ButtomClear", Type = typeof(Button))]

    public class WotemarkTextBox : TextBox
    {


        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty;


        public string Wotemark
        {
            get { return (string)GetValue(WotemarkProperty); }
            set { SetValue(WotemarkProperty, value); }
        }

        public static readonly DependencyProperty WotemarkProperty;

        static WotemarkTextBox()
        {
            CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(WotemarkTextBox), new PropertyMetadata(new CornerRadius(0)));
            WotemarkProperty = DependencyProperty.Register("Wotemark", typeof(string), typeof(WotemarkTextBox), new PropertyMetadata(string.Empty));
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WotemarkTextBox), new FrameworkPropertyMetadata(typeof(WotemarkTextBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var button = this.GetTemplateChild("PART_ButtomClear") as Button;
            if (button != null)
            {
                button.Click += Button_Click;
            }
        } 

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Text = string.Empty;
        }
    }
}
