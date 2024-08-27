using System.ComponentModel;

namespace SneikbotDiscord.ViewModels
{
    public class TextModel
    {
        private string _text;
        
        public void AppendString(string text)
        {
            Text = Text + text;
        }
        public string Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged("Text");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
