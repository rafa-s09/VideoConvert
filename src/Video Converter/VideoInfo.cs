using System.ComponentModel;

namespace Video_Converter
{
    public class VideoInfo : INotifyPropertyChanged
    {
        public string? NomeArquivo { get; set; }
        public string? CaminhoArquivo { get; set; } //Mantem o caminho completo

        private int _progresso;
        public int Progresso
        {
            get { return _progresso; }
            set
            {
                _progresso = value;
                OnPropertyChanged("Progresso");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
