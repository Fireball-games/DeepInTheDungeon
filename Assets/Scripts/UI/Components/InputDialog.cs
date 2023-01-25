using System.Threading.Tasks;
using Scripts.System.MonoBases;

namespace Scripts.UI.Components
{
    public class InputDialog : DialogBase
    {
        private InputField _input;
        
        protected void Awake()
        {
            _input = body.transform.Find("Content/InputField").GetComponent<InputField>();
        }
        
        /// <summary>
        /// Invokes Input text dialog. Upon clicking confirm button, returns entered text or null if cancelled.
        /// </summary>
        /// <param name="inputTitle">Title of the dialog window.</param>
        /// <param name="placeholder"></param>
        /// <param name="defaultValue"></param>
        /// <param name="inputLabelText"></param>
        /// <returns></returns>
        public async Task<string> Show(string inputTitle, string placeholder, string defaultValue, string inputLabelText = null)
        {
            title.SetTitle(inputTitle);

            _input.SetLabelText(inputLabelText);
            
            _input.SetPlaceholderText(placeholder);
            
            _input.SetInputText(string.IsNullOrEmpty(defaultValue) ? string.Empty : defaultValue);

            return await base.Show(inputTitle) == EConfirmResult.Ok ? _input.Text : null;
        }
    }
}
