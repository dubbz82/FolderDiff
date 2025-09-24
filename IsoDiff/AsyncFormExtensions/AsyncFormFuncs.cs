using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderDiff.AsyncForms
{
    public static class AsyncFormFuncs
    {
        public static void WriteTextSafe(this TextBox txt, string text)
        {
            if (txt.InvokeRequired)
            {
                Action safeWrite = delegate { WriteTextSafe(txt, $"{text}"); };
                txt.Invoke(safeWrite);
            }
            else
                txt.Text = text;
        }

        public static void ShowHideCheckboxAsync(this CheckBox checkBox, bool isVisible)
        {
            if (checkBox.InvokeRequired)
            {
                Action showHide = delegate { ShowHideCheckboxAsync(checkBox, isVisible); };
                checkBox.Invoke(showHide);
            }
            else
                checkBox.Visible = isVisible;
        }

        public static void ShowHideButtonAsync(this Button button, bool isVisible)
        {
            if (button.InvokeRequired)
            {
                Action showHide = delegate { ShowHideButtonAsync(button, isVisible); };
                button.Invoke(showHide);
            }
            else
                button.Visible = isVisible;
        }

        public static void ShowHideComboBoxAsync(this ComboBox cbo, bool isVisible)
        {
            if (cbo.InvokeRequired)
            {
                Action showHide = delegate { ShowHideComboBoxAsync(cbo, isVisible); };
                cbo.Invoke(showHide);
            }
            else
                cbo.Visible = isVisible;
        }

        

    }
}
