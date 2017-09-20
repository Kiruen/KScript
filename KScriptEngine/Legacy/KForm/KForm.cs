using KScript.Callable;
using KScript.KAttribute;
using KScript.KSystem.BuiltIn;
using System.Windows.Forms;

namespace KScript.KSystem.Legacy
{
    [MemberMap("Form", MapModifier.Instance, MapType.CommonClass)]
    public class KForm : KBuiltIn
    {
        static KForm()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
        }

        private Form form = null;

        [MemberMap("title", MapModifier.Instance, MapType.Data)]
        public KString Title
        {
            set { form.Text = value; }
            get { return form.Text; }
        }

        [MemberMap("x", MapModifier.Instance, MapType.Data)]
        public double X
        {
            set { form.Left = (int)value; }
            get { return form.Left; }
        }

        [MemberMap("y", MapModifier.Instance, MapType.Data)]
        public double Y
        {
            set { form.Top = (int)value; }
            get { return form.Top; }
        }

        //事件处理
        [MemberMap("onLoad", MapModifier.Instance, MapType.Data)]
        public Function OnLoad
        {
            set
            {
                form.Load += (obj, arg) => value.Invoke(null);
            }
        }

        [MemberMap("onClose", MapModifier.Instance, MapType.Data)]
        public Function OnClosing
        {
            set
            {
                form.FormClosing += (obj, arg) => value.Invoke(null);
            }
        }

        [MemberMap("_cons", MapModifier.Static, MapType.Constructor)]
        public KForm(KString title)
        {
            form = new Form();
            Title = title;
        }

        [MemberMap("show", MapModifier.Instance, MapType.Method)]
        public void Show()
        {
            Application.Run(form);
        }
    }
}
